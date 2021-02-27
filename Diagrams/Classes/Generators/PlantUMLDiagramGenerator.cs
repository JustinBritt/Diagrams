namespace DotNetDiagrams.Classes.Generators
{
    using System.Collections.Generic;

    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    using DotNetDiagrams.Classes.Diagrams;
    using DotNetDiagrams.Classes.Walkers;
    using DotNetDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Interfaces.Generators;
    using DotNetDiagrams.Interfaces.Walkers;
    // ReSharper disable UnusedMemberInSuper.Global

    internal sealed class PlantUMLDiagramGenerator : IDiagramGenerator
    {
        private readonly Solution solution;

        private static readonly MSBuildWorkspace workspace;

        public IDiagrams Diagrams { get; private set; }

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