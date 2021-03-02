namespace DotNetDiagrams.SequenceDiagrams.Classes.Generators
{
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Classes.Configurations;
    using DotNetDiagrams.SequenceDiagrams.Classes.Walkers;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Configurations;
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

            foreach (Project project in solution.Projects.Where(w => w.Language == "C#"))
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    PlantUMLSequenceDiagramWalker walker = new PlantUMLSequenceDiagramWalker(
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