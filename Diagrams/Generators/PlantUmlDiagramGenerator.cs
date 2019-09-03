using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetDiagrams
{
    internal class PlantUmlDiagramGenerator : RoslynDiagramGenerator
    {
        public PlantUmlDiagramGenerator(string solutionPath) : base(solutionPath)
        {
        }

        protected override async Task<DiagramResult> GenerateDiagrams()
        {
            foreach (Project project in solution.Projects)
                await ProcessCompilation(await project.GetCompilationAsync(), project.AssemblyName);

            DiagramResult result = new DiagramResult();

            foreach (MethodDeclarationSyntax root in methodDeclarations.Keys.Where(key => key != null && !methodDeclarations.Values.Any(value => value.Contains(key))))
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                List<string> commands = new List<string>();

                commands.Add("@startuml");
                commands.Add("autoactivate on");
                commands.Add("hide footbox");
                GenerateMethod(root, commands);
                commands.Add("@enduml");

                result[root] = commands;
            }

            return result;
        }

        private IEnumerable<string> GenerateMethod(MethodDeclarationSyntax caller, List<string> resultList = null)
        {
            if (!methodDeclarations.ContainsKey(caller))
                return new string[0];

            resultList = resultList ?? new List<string>();

            foreach (MethodDeclarationSyntax target in _methodOrder[caller].OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value))
            {
                bool callerIsLocal = caller.TryGetParentSyntax(out ClassDeclarationSyntax callingClass);
                bool targetIsLocal = target.TryGetParentSyntax(out ClassDeclarationSyntax targetClass);

                if (callerIsLocal && targetIsLocal)
                {
                    resultList.Add(GeneratingOutgoingCall(targetClass, callingClass, caller, target));

                    if (caller != target)
                        resultList.AddRange(GenerateMethod(target));

                    resultList.Add(GenerateReturnCall(target));
                }
            }

            return resultList;
        }

        private static string GeneratingOutgoingCall(ClassDeclarationSyntax targetClass, ClassDeclarationSyntax callingClass, MethodDeclarationSyntax caller, MethodDeclarationSyntax target, bool includeCalledMethodArguments = false)
        {
            string targetMethodName = target.Identifier.ToFullString();
            string targetMethodArguments = target.ParameterList.ToFullString();
            string targetMethodConstraints = target.ConstraintClauses.ToFullString();
            string targetClassName = targetClass.Identifier.ValueText;
            string callingClassName = callingClass.Identifier.ValueText;

            string targetMethodTypeParameters = target.TypeParameterList != null
                                                    ? target.TypeParameterList.ToFullString()
                                                    : string.Empty;

            string callInfo = targetMethodName + targetMethodTypeParameters;

            if (includeCalledMethodArguments)
                callInfo += targetMethodArguments;

            callInfo += targetMethodConstraints;

            return (callingClassName + "->" + targetClassName + ": " + callInfo).RemoveNewLines(true);
        }

        private static string GenerateReturnCall(MethodDeclarationSyntax target)
        {
            string targetTypeParameters = target.TypeParameterList != null
                                              ? target.TypeParameterList.ToFullString()
                                              : string.Empty;

            string targetInfo = target.Identifier.ToFullString() + targetTypeParameters;

            return ("return " + targetInfo).RemoveNewLines(true);
        }
    }
}
