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

namespace DotNetDiagrams
{
    internal abstract class RoslynDiagramGenerator : IDiagramGenerator, IDisposable
    {
        protected readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder = new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();
        protected readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> methodDeclarations = new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();
        protected readonly Solution solution;

        protected readonly MSBuildWorkspace workspace;

        protected DiagramResult diagrams;

        protected List<(MethodDeclarationSyntax Node, string Assembly, string Class, string Method)> Methods = new List<(MethodDeclarationSyntax Node, string Assembly, string Class, string Method)>();

        static RoslynDiagramGenerator()
        {
            MSBuildLocator.RegisterDefaults();
        }

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

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                workspace?.Dispose();
            }
        }

        protected abstract Task<DiagramResult> GenerateDiagrams();

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

                MethodDeclarationSyntax[] declarations = root.FindToken(position).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().ToArray();
                references.AddRange(declarations);

                // we need to know what order methods are called in
                foreach (MethodDeclarationSyntax methodCall in declarations)
                {
                    if (!_methodOrder.ContainsKey(methodCall))
                        _methodOrder[methodCall] = new Dictionary<int, MethodDeclarationSyntax>();

                    if (!_methodOrder[methodCall].ContainsKey(location.SourceSpan.Start))
                        _methodOrder[methodCall].Add(location.SourceSpan.Start, method);
                }
            }

            return references;
        }

        protected virtual IEnumerable<string> InternalGetDiagram(string diagramName)
        {
            return diagrams.FirstOrDefault(kv => kv.Key.Parent is ClassDeclarationSyntax && diagramName == MethodDisplay(kv.Key)).Value;
        }

        protected virtual IEnumerable<string> InternalGetDiagramNames()
        {
            return diagrams.Keys
                           .Where(method => method.TryGetParentSyntax(out ClassDeclarationSyntax _))
                           .Select(MethodDisplay)
                           .ToList();
        }

        protected virtual string MethodDisplay(MethodDeclarationSyntax method)
        {
            (MethodDeclarationSyntax Node, string Assembly, string Class, string Method) descriptor = Methods.FirstOrDefault(m => m.Node == method);

            return descriptor.Node != null
                       ? $"{descriptor.Assembly}:{descriptor.Class}.{descriptor.Method}"
                       : method.Identifier.ValueText;
        }

        protected async Task ProcessClass(ClassDeclarationSyntax @class, Compilation compilation, SyntaxTree syntaxTree, string assemblyName, string className)
        {
            IEnumerable<MethodDeclarationSyntax> methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methods)
                await ProcessMethod(method
                                  , compilation
                                  , syntaxTree
                                  , assemblyName
                                  , className
                                  , method.Identifier.ToFullString().Trim('\r', '\n'));
        }

        protected async Task ProcessCompilation(Compilation compilation, string assemblyName)
        {
            IEnumerable<SyntaxTree> trees = compilation.SyntaxTrees;

            foreach (SyntaxTree tree in trees)
            {
                SyntaxNode root = await tree.GetRootAsync();
                IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                SyntaxTree treeCopy = tree;

                foreach (ClassDeclarationSyntax @class in classes)
                    await ProcessClass(@class, compilation, treeCopy, assemblyName, @class.Identifier.ToFullString().Trim('\r', '\n'));
            }
        }

        protected async Task ProcessMethod(MethodDeclarationSyntax method, Compilation compilation, SyntaxTree syntaxTree, string assemblyName, string className, string methodName)
        {
            if (!Methods.Contains((method, assemblyName, className, methodName)))
                Methods.Add((method, assemblyName, className, methodName));

            SemanticModel model = compilation.GetSemanticModel(syntaxTree);
            IMethodSymbol methodSymbol = model.GetDeclaredSymbol(method);
            IEnumerable<SymbolCallerInfo> callers = await SymbolFinder.FindCallersAsync(methodSymbol, solution);

            List<MethodDeclarationSyntax> callingMethods = await GetCallingMethodsAsync(methodSymbol, method);

            Parallel.ForEach(callingMethods
                           , callingMethod =>
                             {
                                 if (!methodDeclarations.ContainsKey(callingMethod))
                                     methodDeclarations[callingMethod] = new List<MethodDeclarationSyntax>();

                                 methodDeclarations[callingMethod].Add(method);
                             }); 
        }

        protected class DiagramResult : Dictionary<MethodDeclarationSyntax, IEnumerable<string>> { }
    }
}
