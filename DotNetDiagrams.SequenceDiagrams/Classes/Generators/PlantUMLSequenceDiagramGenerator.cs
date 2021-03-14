namespace DotNetDiagrams.SequenceDiagrams.Classes.Generators
{
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Classes.Configurations;
    using DotNetDiagrams.SequenceDiagrams.Classes.Walkers;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Configurations;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Generators;

    internal sealed class PlantUMLSequenceDiagramGenerator : IPlantUMLSequenceDiagramGenerator
    {
        public PlantUMLSequenceDiagramGenerator()
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
                    PlantUMLSequenceDiagramCSharpSyntaxWalker walker = new PlantUMLSequenceDiagramCSharpSyntaxWalker(
                        compilation,
                        syntaxTree,
                        solution,
                        project);

                    walker.Visit(syntaxTree.GetRoot());

                    diagrams.Value.AddRange(walker.Diagrams.Value);
                }
            }

            foreach(IPlantUMLSequenceDiagram diagram in diagrams.Value)
            {
                diagram.EndDiagram();
            }

            return diagrams;
        }
    }
}