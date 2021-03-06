﻿namespace DotNetDiagrams.ClassDiagrams.Classes.Generators
{
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Classes.Walkers;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLClassDiagramGenerator : IPlantUMLClassDiagramGenerator
    {
        public PlantUMLClassDiagramGenerator()
        {
        }

        public IDiagrams Process(
            Solution solution)
        {
            IDiagrams diagrams = new PlantUMLDiagrams();

            foreach (Project project in solution.Projects.Where(w => w.Language is LanguageNames.CSharp))
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLClassDiagramCSharpSyntaxWalker walker = new PlantUMLClassDiagramCSharpSyntaxWalker(
                        compilation,
                        syntaxTree,
                        solution,
                        project);

                    walker.Visit(syntaxTree.GetRoot());

                    diagrams.Value.AddRange(walker.Diagrams.Value);
                }
            }

            foreach (Project project in solution.Projects.Where(w => w.Language is LanguageNames.VisualBasic))
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLClassDiagramVisualBasicSyntaxWalker walker = new PlantUMLClassDiagramVisualBasicSyntaxWalker(
                        compilation,
                        syntaxTree,
                        solution,
                        project);

                    walker.Visit(syntaxTree.GetRoot());

                    diagrams.Value.AddRange(walker.Diagrams.Value);
                }
            }

            foreach (IPlantUMLClassDiagram diagram in diagrams.Value)
            {
                diagram.EndDiagram();
            }

            return diagrams;
        }
    }
}