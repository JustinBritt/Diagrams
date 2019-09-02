// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetDiagrams {
    internal abstract class RoslynDiagramGenerator : IDiagramGenerator, IDisposable
    {
        static RoslynDiagramGenerator()
        {
            MSBuildLocator.RegisterDefaults();

        }

        protected class DiagramResult : Dictionary<MethodDeclarationSyntax, IEnumerable<string>> { }

        protected readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder = new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();
        protected readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> methodDeclarationSyntaxes = new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();
        protected readonly Solution solution;

        protected readonly MSBuildWorkspace workspace;

        protected DiagramResult diagrams;

        protected RoslynDiagramGenerator(string solutionPath)
        {
            workspace = MSBuildWorkspace.Create();
            solution = workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
        }

        public IEnumerable<string> GetDiagramNames()
        {
            if (diagrams == null)
                diagrams = GenerateDiagrams().GetAwaiter().GetResult();

            return InternalGetDiagramNames();
        }

        public IEnumerable<string> GetDiagram(string diagramName)
        {
            if (diagrams == null)
                diagrams = GenerateDiagrams().GetAwaiter().GetResult();

            return InternalGetDiagram(diagramName);
        }

        protected abstract IEnumerable<string> InternalGetDiagramNames();

        protected abstract IEnumerable<string> InternalGetDiagram(string diagramName);

        protected abstract Task<DiagramResult> GenerateDiagrams();

        protected async Task ProcessCompilation(Compilation compilation, string assemblyName)
        {
            IEnumerable<SyntaxTree> trees = compilation.SyntaxTrees;

            foreach (SyntaxTree tree in trees)
            {
                SyntaxNode root = await tree.GetRootAsync();
                IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                SyntaxTree treeCopy = tree;

                foreach (ClassDeclarationSyntax @class in classes)
                {
                    string className = @class.Identifier.ToFullString();
                    await ProcessClass(@class, compilation, treeCopy, assemblyName, className);
                }
            }
        }

        protected async Task ProcessClass(ClassDeclarationSyntax @class, Compilation compilation, SyntaxTree syntaxTree, string assemblyName, string className)
        {
            IEnumerable<MethodDeclarationSyntax> methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methods)
            {
                string methodName = method.Identifier.ToFullString();
                await ProcessMethod(method, compilation, syntaxTree, assemblyName, className, methodName);
            }
        }

        protected async Task ProcessMethod(MethodDeclarationSyntax method, Compilation compilation, SyntaxTree syntaxTree, string assemblyName, string className, string methodName)
        {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);

            IMethodSymbol methodSymbol = model.GetDeclaredSymbol(method);

            List<MethodDeclarationSyntax> callingMethods = await GetCallingMethodsAsync(methodSymbol, method);

            Parallel.ForEach(callingMethods
                           , callingMethod =>
                             {
                                 if (!methodDeclarationSyntaxes.ContainsKey(callingMethod))
                                     methodDeclarationSyntaxes[callingMethod] = new List<MethodDeclarationSyntax>();

                                 methodDeclarationSyntaxes[callingMethod].Add(method);
                             });
        }

        protected async Task<List<MethodDeclarationSyntax>> GetCallingMethodsAsync(IMethodSymbol methodSymbol, MethodDeclarationSyntax method)
        {
            List<MethodDeclarationSyntax> references = new List<MethodDeclarationSyntax>();

            List<SymbolCallerInfo> referencingSymbolsList = (await SymbolFinder.FindCallersAsync(methodSymbol, solution)).ToList();

            if (!referencingSymbolsList.Any(s => s.Locations.Any()))
                return references;

            foreach (Location location in referencingSymbolsList.SelectMany(l => l.Locations))
            {
                int position = location.SourceSpan.Start;
                SyntaxNode root = await location.SourceTree.GetRootAsync();

                MethodDeclarationSyntax[] methodDeclarations = root.FindToken(position).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().ToArray();
                references.AddRange(methodDeclarations);

                // we need to know what order methods are called in
                foreach (MethodDeclarationSyntax methodCall in methodDeclarations)
                {
                    if (!_methodOrder.ContainsKey(methodCall))
                        _methodOrder[methodCall] = new Dictionary<int, MethodDeclarationSyntax>();

                    if (!_methodOrder[methodCall].ContainsKey(location.SourceSpan.Start))
                        _methodOrder[methodCall].Add(location.SourceSpan.Start, method);
                }
            }

            return references;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                workspace?.Dispose();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
