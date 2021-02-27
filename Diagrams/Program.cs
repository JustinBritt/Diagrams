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

         Solution solution = OpenSolution(args[0]);

         ExecuteStrategy2(solution);

         Console.WriteLine("Hit any key to continue");
         
         Console.ReadKey();
      }

      private static Solution OpenSolution(
          string solutionPath)
      {
            MSBuildLocator.RegisterDefaults();

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            return workspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
      }

      private static void ExecuteStrategy2(
          Solution solution)
      {
         IDiagramGenerator generator = new PlantUMLDiagramGenerator();
         
         IDiagrams diagrams = generator.Process(solution);
            
         WriteDiagramsToConsole(diagrams);
      }

        private static void WriteDiagramsToConsole(
            IDiagrams diagrams)
        {
            foreach (string title in diagrams.Value.Select(w => w.Title))
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine(title);
                Console.WriteLine("-----------------------------");

                foreach (string code in diagrams.GetCodeAtTitleOrDefault(title))
                    Console.WriteLine(code);

                Console.WriteLine();
            }
        }
   }
}