namespace Diagrams.Classes.Generators
{
    using System.Collections.Generic;

    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    using Diagrams.Classes.Walkers;
    using Diagrams.Interfaces.Generators;
    // ReSharper disable UnusedMemberInSuper.Global

    internal sealed class PlantUMLDiagramGenerator : IDiagramGenerator
    {
        private readonly Solution solution;
        private static readonly MSBuildWorkspace workspace;
        public Dictionary<string, List<string>> Diagrams { get; private set; }

        static PlantUMLDiagramGenerator()
        {
            MSBuildLocator.RegisterDefaults();
            workspace = MSBuildWorkspace.Create();
        }

        public PlantUMLDiagramGenerator(string solutionPath)
        {
            solution = workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
        }

        public void Process()
        {
            foreach (Project project in solution.Projects)
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLWalker walker = new PlantUMLWalker(compilation, syntaxTree, solution, project);
                    walker.Visit(syntaxTree.GetRoot());
                }
            }

            Diagrams = PlantUMLWalker.Diagrams;
        }
    }
}