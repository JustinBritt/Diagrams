﻿namespace DotNetDiagrams.Classes.Generators
{
    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Classes.Diagrams;
    using DotNetDiagrams.Classes.Walkers;
    using DotNetDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Interfaces.Generators;

    internal sealed class PlantUMLDiagramGenerator : IDiagramGenerator
    {
        public PlantUMLDiagramGenerator()
        {
        }

        public IDiagrams Process(
            Solution solution)
        {
            IDiagrams diagrams = new PlantUMLDiagrams();

            foreach (Project project in solution.Projects)
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLWalker walker = new PlantUMLWalker(
                        compilation,
                        syntaxTree,
                        solution,
                        project);

                    walker.Visit(syntaxTree.GetRoot());

                    diagrams.Value.AddRange(walker.Diagrams.Value);
                }
            }

            return diagrams;
        }
    }
}