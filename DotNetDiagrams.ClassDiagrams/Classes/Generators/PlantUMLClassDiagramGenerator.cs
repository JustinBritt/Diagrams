namespace DotNetDiagrams.ClassDiagrams.Classes.Generators
{
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;

    internal sealed class PlantUMLClassDiagramGenerator : IPlantUMLClassDiagramGenerator
    {
        public PlantUMLClassDiagramGenerator()
        {
        }

        // TODO: Finish
        public IDiagrams Process(
            Solution solution)
        {
            IDiagrams diagrams = new PlantUMLDiagrams();

            foreach (Project project in solution.Projects.Where(w => w.Language == "C#"))
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    //PlantUMLSequenceDiagramWalker walker = new PlantUMLSequenceDiagramWalker(
                    //    compilation,
                    //    syntaxTree,
                    //    solution,
                    //    project);

                    //walker.Visit(syntaxTree.GetRoot());

                    //diagrams.Value.AddRange(walker.Diagrams.Value);
                }
            }

            return diagrams;
        }
    }
}