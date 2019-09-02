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

    internal class Program
    {
        private static void Main(string[] args)
        {
            string solutionPath = args[0];
            MSBuildLocator.RegisterDefaults();

            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                DiagramGenerator diagramGenerator = new DiagramGenerator(solutionPath, workspace);
                diagramGenerator.ProcessSolution().GetAwaiter().GetResult();
                diagramGenerator.GenerateDiagramFromRoot();
                Console.ReadKey();
            }
        }
    }

    internal class DiagramGenerator
    {
        private readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> _methodDeclarationSyntaxes
         = new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder
         = new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();

        private readonly Solution _solution;

        public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace)
        {
            _solution = workspace.OpenSolutionAsync(solutionPath).Result;
        }

        #region [process the tree]

        private async Task ProcessCompilation(Compilation compilation)
        {
            IEnumerable<SyntaxTree> trees = compilation.SyntaxTrees;

            foreach (SyntaxTree tree in trees)
            {
                SyntaxNode root = await tree.GetRootAsync();
                IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                SyntaxTree treeCopy = tree;

                foreach (ClassDeclarationSyntax @class in classes)
                {
                    await ProcessClass(@class, compilation, treeCopy);
                }
            }
        }

        private async Task ProcessClass(ClassDeclarationSyntax @class
                                      , Compilation compilation
                                      , SyntaxTree syntaxTree)
        {
            IEnumerable<MethodDeclarationSyntax> methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methods)
            {
                await ProcessMethod(method, compilation, syntaxTree);
            }
        }

        private async Task ProcessMethod(MethodDeclarationSyntax method
                                       , Compilation compilation
                                       , SyntaxTree syntaxTree)
        {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);

            IMethodSymbol methodSymbol = model.GetDeclaredSymbol(method);

            List<MethodDeclarationSyntax> callingMethods = await GetCallingMethodsAsync(methodSymbol, method);

            Parallel.ForEach(callingMethods
                           , callingMethod =>
                             {
                                 ClassDeclarationSyntax callingClass = null;

                                 if (SyntaxNodeHelper.TryGetParentSyntax(method, out callingClass))
                                 {
                                     List<MethodDeclarationSyntax> value;

                                     if (!_methodDeclarationSyntaxes.TryGetValue(callingMethod, out value))
                                     {
                                         if (!_methodDeclarationSyntaxes.TryAdd(callingMethod
                                                                           , new List<MethodDeclarationSyntax>
                                                                             {
                                                                             method
                                                                             }))
                                         {
                                             throw new Exception("Could not add item to _methodDeclarationSyntaxes!");
                                         }
                                     }
                                     else
                                     {
                                         value.Add(method);
                                     }
                                 }
                             });
        }

        /// <summary>
        ///    Gets a list of methods that call the method based on the method symbol
        ///    also builds a list of called methods by the calling method as the key and then the value is a dictionary
        ///    of UInt64,MethodDeclarationSyntax where the UInt64 is the start location of the span where the called method is
        ///    called
        ///    from inside the calling method - this will allow us to order our sequence diagrams, but this functionality should be
        ///    moved out into a separate method at some point
        ///    (in fact this whole method needs a ton of refactoring and is too complex)
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<List<MethodDeclarationSyntax>> GetCallingMethodsAsync(IMethodSymbol methodSymbol, MethodDeclarationSyntax method)
        {
            List<MethodDeclarationSyntax> references = new List<MethodDeclarationSyntax>();

            IEnumerable<SymbolCallerInfo> referencingSymbols = await SymbolFinder.FindCallersAsync(methodSymbol, _solution);
            IList<SymbolCallerInfo> referencingSymbolsList = referencingSymbols as IList<SymbolCallerInfo> ?? referencingSymbols.ToList();

            if (!referencingSymbolsList.Any(s => s.Locations.Any()))
            {
                return references;
            }

            foreach (SymbolCallerInfo referenceSymbol in referencingSymbolsList)
            {
                foreach (Location location in referenceSymbol.Locations)
                {
                    int position = location.SourceSpan.Start;
                    SyntaxNode root = await location.SourceTree.GetRootAsync();
                    IEnumerable<MethodDeclarationSyntax> nodes = root.FindToken(position).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>();

                    MethodDeclarationSyntax[] methodDeclarationSyntaxes = nodes as MethodDeclarationSyntax[] ?? nodes.ToArray();
                    references.AddRange(methodDeclarationSyntaxes);

                    // we need to know what order methods are called in
                    foreach (MethodDeclarationSyntax methodCall in methodDeclarationSyntaxes)
                    {
                        Dictionary<int, MethodDeclarationSyntax> value;

                        if (!_methodOrder.TryGetValue(methodCall, out value))
                        {
                            Dictionary<int, MethodDeclarationSyntax> dictionary = new Dictionary<int, MethodDeclarationSyntax>();
                            dictionary.Add(location.SourceSpan.Start, method);

                            if (!_methodOrder.TryAdd(methodCall, dictionary))
                            {
                                throw new Exception("Could not add item to _methodOrder!");
                            }
                        }
                        else
                        {
                            value.Add(location.SourceSpan.Start, method);
                        }
                    }
                }
            }

            return references;
        }

        #endregion

        #region [build & output js-sequence-diagrams formatted text]

        /// <summary>
        ///    generates diagram by order of methods getting called based on the first method found that does not have anything
        ///    calling it
        /// </summary>
        public void GenerateDiagramFromRoot()
        {
            MethodDeclarationSyntax root = null;

            foreach (MethodDeclarationSyntax key in _methodDeclarationSyntaxes.Keys)
            {
                if (!_methodDeclarationSyntaxes.Values.Any(value => value.Contains(key)))
                {
                    // then we have a method that's not being called by anything
                    root = key;

                    break;
                }
            }

            if (root != null)
            {
                PrintMethodInfo(root);
            }
        }

        public void PrintMethodInfo(MethodDeclarationSyntax callingMethod)
        {
            if (!_methodDeclarationSyntaxes.ContainsKey(callingMethod))
            {
                return;
            }

            Dictionary<int, MethodDeclarationSyntax> calledMethods = _methodOrder[callingMethod];
            IOrderedEnumerable<KeyValuePair<int, MethodDeclarationSyntax>> orderedCalledMethods = calledMethods.OrderBy(kvp => kvp.Key);

            foreach (KeyValuePair<int, MethodDeclarationSyntax> kvp in orderedCalledMethods)
            {
                MethodDeclarationSyntax calledMethod = kvp.Value;
                ClassDeclarationSyntax callingClass = null;
                ClassDeclarationSyntax calledClass = null;

                if (!SyntaxNodeHelper.TryGetParentSyntax(callingMethod, out callingClass)
                 || !SyntaxNodeHelper.TryGetParentSyntax(calledMethod, out calledClass))
                {
                    continue;
                }

                PrintOutgoingCallInfo(calledClass
                                    , callingClass
                                    , callingMethod
                                    , calledMethod);

                if (callingMethod != calledMethod)
                {
                    PrintMethodInfo(calledMethod);
                }

                PrintReturnCallInfo(calledClass
                                  , callingClass
                                  , callingMethod
                                  , calledMethod);
            }
        }

        private static void PrintOutgoingCallInfo(ClassDeclarationSyntax classBeingCalled
                                                , ClassDeclarationSyntax callingClass
                                                , MethodDeclarationSyntax callingMethod
                                                , MethodDeclarationSyntax calledMethod
                                                , bool includeCalledMethodArguments = false)
        {
            string callingMethodName = callingMethod.Identifier.ToFullString();
            string calledMethodReturnType = calledMethod.ReturnType.ToFullString();
            string calledMethodName = calledMethod.Identifier.ToFullString();
            string calledMethodArguments = calledMethod.ParameterList.ToFullString();
            string calledMethodModifiers = calledMethod.Modifiers.ToString();
            string calledMethodConstraints = calledMethod.ConstraintClauses.ToFullString();
            string actedUpon = classBeingCalled.Identifier.ValueText;
            string actor = callingClass.Identifier.ValueText;

            string calledMethodTypeParameters = calledMethod.TypeParameterList != null
                                                ? calledMethod.TypeParameterList.ToFullString()
                                                : string.Empty;

            string callingMethodTypeParameters = callingMethod.TypeParameterList != null
                                                 ? callingMethod.TypeParameterList.ToFullString()
                                                 : string.Empty;

            string callInfo = callingMethodName + callingMethodTypeParameters + " => " + calledMethodModifiers + " " + calledMethodReturnType + calledMethodName + calledMethodTypeParameters;

            if (includeCalledMethodArguments)
            {
                callInfo += calledMethodArguments;
            }

            callInfo += calledMethodConstraints;

            string info
            = BuildOutgoingCallInfo(actor
                                  , actedUpon
                                  , callInfo);

            Console.Write(info);
        }

        private static void PrintReturnCallInfo(ClassDeclarationSyntax classBeingCalled
                                              , ClassDeclarationSyntax callingClass
                                              , MethodDeclarationSyntax callingMethod
                                              , MethodDeclarationSyntax calledMethod)
        {
            string actedUpon = classBeingCalled.Identifier.ValueText;
            string actor = callingClass.Identifier.ValueText;
            string callerName = callingMethod.Identifier.ToFullString();

            string callingMethodTypeParameters = callingMethod.TypeParameterList != null
                                                 ? callingMethod.TypeParameterList.ToFullString()
                                                 : string.Empty;

            string calledMethodTypeParameters = calledMethod.TypeParameterList != null
                                                ? calledMethod.TypeParameterList.ToFullString()
                                                : string.Empty;

            string calledMethodInfo = calledMethod.Identifier.ToFullString() + calledMethodTypeParameters;

            callerName += callingMethodTypeParameters;

            string returnCallInfo = calledMethod.ReturnType.ToString();

            SeparatedSyntaxList<ParameterSyntax> returnMethodParameters = calledMethod.ParameterList.Parameters;

            foreach (ParameterSyntax parameter in returnMethodParameters)
            {
                if (parameter.Modifiers.Any(m => m.Text == "out"))
                {
                    returnCallInfo += "," + parameter.ToFullString();
                }
            }

            string info = BuildReturnCallInfo(actor
                                         , actedUpon
                                         , calledMethodInfo
                                         , callerName
                                         , returnCallInfo);

            Console.Write(info);
        }

        private static string BuildOutgoingCallInfo(string actor, string actedUpon, string callInfo)
        {
            const string calls = "->";
            const string descriptionSeparator = ": ";

            string callingInfo = actor + calls + actedUpon + descriptionSeparator + callInfo;

            callingInfo = callingInfo.RemoveNewLines(true);

            string result = callingInfo + Environment.NewLine;

            return result;
        }

        private static string BuildReturnCallInfo(string actor
                                                , string actedUpon
                                                , string calledMethodInfo
                                                , string callerName
                                                , string returnInfo)
        {
            const string returns = "-->";
            const string descriptionSeparator = ": ";

            string returningInfo = actedUpon + returns + actor + descriptionSeparator + calledMethodInfo + " returns " + returnInfo + " to " + callerName;
            returningInfo = returningInfo.RemoveNewLines(true);

            string result = returningInfo + Environment.NewLine;

            return result;
        }

        public async Task ProcessSolution()
        {
            foreach (Project project in _solution.Projects)
            {
                Compilation compilation = await project.GetCompilationAsync();
                await ProcessCompilation(compilation);
            }
        }

        #endregion
    }

}
