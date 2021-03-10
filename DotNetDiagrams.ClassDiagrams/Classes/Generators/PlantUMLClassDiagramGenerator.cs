﻿namespace DotNetDiagrams.ClassDiagrams.Classes.Generators
{
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Classes.Walkers;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;
    using System.Collections.Generic;

    internal sealed class PlantUMLClassDiagramGenerator : IPlantUMLClassDiagramGenerator
    {
        public PlantUMLClassDiagramGenerator()
        {
        }

        public IDiagrams Process(
            Solution solution)
        {
            IDiagrams diagrams = new PlantUMLDiagrams();

            Dictionary<Project, Compilation> compilations = new Dictionary<Project, Compilation>();

            foreach (Project project in solution.Projects.Where(w => w.Language is LanguageNames.CSharp))
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                compilations.Add(
                    project,
                    compilation);
            }

            foreach (Project project in solution.Projects.Where(w => w.Language is LanguageNames.CSharp))
            {
                Compilation compilation = compilations.Where(w => w.Key == project).SingleOrDefault();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLClassDiagramWalker walker = new PlantUMLClassDiagramWalker(
                        compilation,
                        compilations,
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