namespace DotNetDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    using DotNetDiagrams.Classes.Generators;
    using DotNetDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Interfaces.Generators;

    internal sealed class Program
    {
      private static void Main(string[] args)
      {
         if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != ".sln")
         {
            Console.WriteLine("Usage: Diagrams <solutionPath>");

            return;
         }

         Solution solution = OpenSolution(
             args[0],
             CreateMSBuildWorkspace());

         IDiagramGenerator diagramGenerator = new PlantUMLDiagramGenerator();

         IDiagrams diagrams = ProcessSolution(
             diagramGenerator,
             solution);

         WriteDiagramsToConsole(diagrams);

         Console.WriteLine("Hit any key to continue");
         
         Console.ReadKey();
      }

     private static MSBuildWorkspace CreateMSBuildWorkspace()
     {
        MSBuildLocator.RegisterDefaults();

        return MSBuildWorkspace.Create();
     }

      private static Solution OpenSolution(
          string solutionPath,
          MSBuildWorkspace workspace)
      {
            return workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
      }

      private static IDiagrams ProcessSolution(
          IDiagramGenerator diagramGenerator,
          Solution solution)
      {
            return diagramGenerator.Process(solution);
      }

        private static void WriteDiagramsToConsole(
            IDiagrams diagrams)
        {
            string seperator = "-----------------------------";

            foreach (string title in diagrams.Value.Select(w => w.Title))
            {
                Console.WriteLine(seperator);
                Console.WriteLine(title);
                Console.WriteLine(seperator);

                foreach (string code in diagrams.GetCodeAtTitleOrDefault(title))
                    Console.WriteLine(code);

                Console.WriteLine();
            }
        }
   }
}