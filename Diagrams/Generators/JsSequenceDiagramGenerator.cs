// 

// 

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetDiagrams
{
    internal class JsSequenceDiagramGenerator : RoslynDiagramGenerator
    {

        public JsSequenceDiagramGenerator(string solutionPath) : base(solutionPath)
        {
        }

        protected override IEnumerable<string> InternalGetDiagramNames()
        {
            return diagrams.Keys
                           .Where(method => method.TryGetParentSyntax(out ClassDeclarationSyntax _))
                           .Select(MethodDisplay)
                           .ToList();
        }

        private string MethodDisplay(MethodDeclarationSyntax method)
        {
            return method.TryGetParentSyntax(out ClassDeclarationSyntax @class)
                       ? $"{@class.Identifier.ValueText}.{method.Identifier.ValueText}"
                       : method.Identifier.ValueText;
        }

        protected override IEnumerable<string> InternalGetDiagram(string diagramName)
        {
            return diagrams.FirstOrDefault(kv => kv.Key.Parent is ClassDeclarationSyntax && diagramName == MethodDisplay(kv.Key)).Value;
        }

        protected override async Task<DiagramResult> GenerateDiagrams()
        {
            foreach (Project project in solution.Projects)
                await ProcessCompilation(await project.GetCompilationAsync(), project.AssemblyName);

            DiagramResult result = new DiagramResult();

            foreach (MethodDeclarationSyntax root in methodDeclarationSyntaxes.Keys.Where(key => key != null && !methodDeclarationSyntaxes.Values.Any(value => value.Contains(key))))
                result[root] = GenerateMethod(root);

            return result;
        }

        /// <summary>
        ///     generates diagram by order of methods getting called based on the first method found that does not have anything
        ///     calling it
        /// </summary>
        private IEnumerable<string> GenerateMethod(MethodDeclarationSyntax caller)
        {
            if (!methodDeclarationSyntaxes.ContainsKey(caller))
                return new string[0];

            List<string> resultList = new List<string>();

            foreach (MethodDeclarationSyntax target in _methodOrder[caller].OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value))
            {
                bool callerIsLocal = caller.TryGetParentSyntax(out ClassDeclarationSyntax callingClass);
                bool targetIsLocal = target.TryGetParentSyntax(out ClassDeclarationSyntax targetClass);

                if (callerIsLocal && targetIsLocal)
                {
                    resultList.Add(GeneratingOutgoingCall(targetClass, callingClass, caller, target));

                    if (caller != target)
                        resultList.AddRange(GenerateMethod(target));

                    resultList.Add(GenerateReturnCall(targetClass, callingClass, caller, target));
                }
            }

            return resultList;
        }

        private static string GeneratingOutgoingCall(ClassDeclarationSyntax targetClass, 
                                                         ClassDeclarationSyntax callingClass, 
                                                         MethodDeclarationSyntax caller, 
                                                         MethodDeclarationSyntax target, 
                                                         bool includeCalledMethodArguments = false)
        {
            string callingMethodName = caller.Identifier.ToFullString();
            string targetMethodReturnType = target.ReturnType.ToFullString();
            string targetMethodName = target.Identifier.ToFullString();
            string targetMethodArguments = target.ParameterList.ToFullString();
            string targetMethodModifiers = target.Modifiers.ToString();
            string targetMethodConstraints = target.ConstraintClauses.ToFullString();
            string actedUpon = targetClass.Identifier.ValueText;
            string actor = callingClass.Identifier.ValueText;

            string targetMethodTypeParameters = target.TypeParameterList != null
                                                    ? target.TypeParameterList.ToFullString()
                                                    : string.Empty;

            string callingMethodTypeParameters = caller.TypeParameterList != null
                                                     ? caller.TypeParameterList.ToFullString()
                                                     : string.Empty;

            string callInfo = callingMethodName + callingMethodTypeParameters + " => " + targetMethodModifiers + " " + targetMethodReturnType + targetMethodName + targetMethodTypeParameters;

            if (includeCalledMethodArguments)
                callInfo += targetMethodArguments;

            callInfo += targetMethodConstraints;

            string result = actor + "->" + actedUpon + ": " + callInfo;

            return result.RemoveNewLines(true);
        }

        private static string GenerateReturnCall(ClassDeclarationSyntax classBeingCalled, 
                                                 ClassDeclarationSyntax callingClass, 
                                                 MethodDeclarationSyntax callingMethod, 
                                                 MethodDeclarationSyntax calledMethod)
        {
            string actedUpon = classBeingCalled.Identifier.ValueText;
            string actor = callingClass.Identifier.ValueText;
            string callerName = callingMethod.Identifier.ToFullString();

            string callerTypeParameters = callingMethod.TypeParameterList != null
                                              ? callingMethod.TypeParameterList.ToFullString()
                                              : string.Empty;

            string targetTypeParameters = calledMethod.TypeParameterList != null
                                              ? calledMethod.TypeParameterList.ToFullString()
                                              : string.Empty;

            string targetInfo = calledMethod.Identifier.ToFullString() + targetTypeParameters;

            callerName += callerTypeParameters;

            string returnCallInfo = calledMethod.ReturnType.ToString();

            returnCallInfo = calledMethod.ParameterList.Parameters.Where(parameter => parameter.Modifiers.Any(m => m.Text == "out")).Aggregate(returnCallInfo, (current, parameter) => current + "," + parameter.ToFullString());

            string result = actedUpon + "-->" + actor + ": " + targetInfo + " returns " + returnCallInfo + " to " + callerName;

            return result.RemoveNewLines(true);
        }
    }
}
