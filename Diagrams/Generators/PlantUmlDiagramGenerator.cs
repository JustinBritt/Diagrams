using System.Collections.Generic;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetDiagrams
{
   internal class PlantUmlDiagramGenerator
   {
      private readonly Solution solution;
      private static readonly MSBuildWorkspace workspace;
      public Dictionary<string, List<string>> Diagrams { get; private set; }

      static PlantUmlDiagramGenerator()
      {
         MSBuildLocator.RegisterDefaults();
         workspace = MSBuildWorkspace.Create();
      }

      public PlantUmlDiagramGenerator(string solutionPath)
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
               PlantWalker walker = new PlantWalker(compilation, syntaxTree, solution, project);
               walker.Visit(syntaxTree.GetRoot());
            }
         }

         Diagrams = PlantWalker.Diagrams;
      }
   }
}
