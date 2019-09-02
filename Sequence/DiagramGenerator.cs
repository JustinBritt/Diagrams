// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetDiagrams
{
    internal class DiagramGenerator
    {
        public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace)
        {
            _solution = workspace.OpenSolutionAsync(solutionPath).Result;
        }

        private readonly Solution _solution;

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> _methodDeclarationSyntaxes = 
            new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder = 
            new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();

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
                    await ProcessClass(@class, compilation, treeCopy);
            }
        }


        /// <summary>
        ///     Gets a list of methods that call the method based on the method symbol
        ///     also builds a list of called methods by the calling method as the key and then the value is a dictionary
        ///     of UInt64,MethodDeclarationSyntax where the UInt64 is the start location of the span where the called method is
        ///     called
        ///     from inside the calling method - this will allow us to order our sequence diagrams, but this functionality should
        ///     be moved out into a separate method at some point
        ///     (in fact this whole method needs a ton of refactoring and is too complex)
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<List<MethodDeclarationSyntax>> GetCallingMethodsAsync(IMethodSymbol methodSymbol, MethodDeclarationSyntax method)
        {
            List<MethodDeclarationSyntax> result = new List<MethodDeclarationSyntax>();
            IList<SymbolCallerInfo> referencingSymbolsList = (await SymbolFinder.FindCallersAsync(methodSymbol, _solution)).ToList();

            if (!referencingSymbolsList.Any(s => s.Locations.Any()))
                return result;

            foreach (Location location in referencingSymbolsList.SelectMany(s => s.Locations))
            {
                int position = location.SourceSpan.Start;
                SyntaxNode root = await location.SourceTree.GetRootAsync();
                IEnumerable<MethodDeclarationSyntax> nodes = root.FindToken(position).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>();

                MethodDeclarationSyntax[] methodDeclarationSyntaxes = nodes as MethodDeclarationSyntax[] ?? nodes.ToArray();
                result.AddRange(methodDeclarationSyntaxes);

                // we need to know what order methods are called in
                foreach (MethodDeclarationSyntax methodCall in methodDeclarationSyntaxes)
                {
                    if (!_methodOrder.ContainsKey(methodCall))
                        _methodOrder[methodCall] = new Dictionary<int, MethodDeclarationSyntax>();

                    _methodOrder[methodCall].Add(location.SourceSpan.Start, method);
                }
            }

            return result;
        }

        #endregion

        #region [build & output js-sequence-diagrams formatted text]

        /// <summary>
        ///     generates diagram by order of methods getting called based on the first method found that does not have anything
        ///     calling it
        /// </summary>
        public Dictionary<string,IEnumerable<string>> GenerateDiagrams()
        {
            Dictionary<string,IEnumerable<string>> result = new Dictionary<string, IEnumerable<string>>();

            // methods that are not being called by anything
            foreach (MethodDeclarationSyntax root in _methodDeclarationSyntaxes.Keys.Where(key => !_methodDeclarationSyntaxes.Values.Any(value => value.Contains(key))))
                result[root.ToFullString()] = PrintMethodInfo(root);

            return result;
        }

        private IEnumerable<string> PrintMethodInfo(MethodDeclarationSyntax callingMethod)
        {
            if (!_methodDeclarationSyntaxes.ContainsKey(callingMethod))
            {
                return new string[0];
            }

            List<string> result = new List<string>();
            Dictionary<int, MethodDeclarationSyntax> calledMethods = _methodOrder[callingMethod];
            IOrderedEnumerable<KeyValuePair<int, MethodDeclarationSyntax>> orderedCalledMethods = calledMethods.OrderBy(kvp => kvp.Key);

            foreach (MethodDeclarationSyntax calledMethod in orderedCalledMethods.Select(kvp => kvp.Value))
            {
                if (SyntaxNodeHelper.TryGetParentSyntax(callingMethod, out ClassDeclarationSyntax callingClass) && 
                    SyntaxNodeHelper.TryGetParentSyntax(calledMethod, out ClassDeclarationSyntax calledClass))
                {
                    string resultLine = PrintOutgoingCallInfo(calledClass, callingClass, callingMethod, calledMethod);

                    if (callingMethod != calledMethod)
                        resultLine += PrintMethodInfo(calledMethod);

                    resultLine += PrintReturnCallInfo(calledClass, callingClass, callingMethod, calledMethod);
                    result.Add(resultLine);
                }
            }

            return result;
        }

        private static string PrintOutgoingCallInfo(ClassDeclarationSyntax classBeingCalled, 
                                                    ClassDeclarationSyntax callingClass, 
                                                    MethodDeclarationSyntax callingMethod, 
                                                    MethodDeclarationSyntax calledMethod, 
                                                    bool includeCalledMethodArguments = false)
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
                callInfo += calledMethodArguments;

            callInfo += calledMethodConstraints;

            return BuildOutgoingCallInfo(actor, actedUpon, callInfo);
        }

        private static string PrintReturnCallInfo(ClassDeclarationSyntax classBeingCalled, ClassDeclarationSyntax callingClass, MethodDeclarationSyntax callingMethod, MethodDeclarationSyntax calledMethod)
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

            returnCallInfo = returnMethodParameters.Where(parameter => parameter.Modifiers.Any(m => m.Text == "out"))
                                                   .Aggregate(returnCallInfo, (current, parameter) => current + ("," + parameter.ToFullString()));

            return BuildReturnCallInfo(actor, actedUpon, calledMethodInfo, callerName, returnCallInfo);
        }

        private static string BuildOutgoingCallInfo(string actor, string actedUpon, string callInfo)
        {
            const string calls = "->";
            const string descriptionSeparator = ": ";

            string callingInfo = actor + calls + actedUpon + descriptionSeparator + callInfo;

            return callingInfo.RemoveNewLines(true) + Environment.NewLine;
        }

        private static string BuildReturnCallInfo(string actor, string actedUpon, string calledMethodInfo, string callerName, string returnInfo)
        {
            const string returns = "-->";
            const string descriptionSeparator = ": ";

            string returningInfo = actedUpon + returns + actor + descriptionSeparator + calledMethodInfo + " returns " + returnInfo + " to " + callerName;
            returningInfo = returningInfo.RemoveNewLines(true);

            return returningInfo + Environment.NewLine;
        }

        public async Task ProcessSolution()
        {
            foreach (Project project in _solution.Projects)
                await ProcessProject(project);
        }

        private async Task ProcessProject(Project project)
        {
            Compilation compilation = await project.GetCompilationAsync();
            
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SyntaxNode root = await tree.GetRootAsync();
                IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                SyntaxTree treeCopy = tree;

                foreach (ClassDeclarationSyntax @class in classes)
                    await ProcessClass(@class, compilation, treeCopy);
            }
        }

        private async Task ProcessClass(ClassDeclarationSyntax @class, Compilation compilation, SyntaxTree syntaxTree)
        {
            IEnumerable<MethodDeclarationSyntax> methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methods)
                await ProcessMethod(method, compilation, syntaxTree);
        }

        private async Task ProcessMethod(MethodDeclarationSyntax method, Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);
            IMethodSymbol methodSymbol = model.GetDeclaredSymbol(method);
            List<MethodDeclarationSyntax> callingMethods = await GetCallingMethodsAsync(methodSymbol, method);

            Parallel.ForEach(callingMethods
                           , callingMethod =>
                             {
                                 if (SyntaxNodeHelper.TryGetParentSyntax(method, out ClassDeclarationSyntax _))
                                 {
                                     if (!_methodDeclarationSyntaxes.ContainsKey(callingMethod))
                                         _methodDeclarationSyntaxes[callingMethod] = new List<MethodDeclarationSyntax>();

                                     _methodDeclarationSyntaxes[callingMethod].Add(method);
                                 }
                             });
        }
#endregion
    }
}
