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

         IDiagrams diagrams = ProcessSolution(solution);

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
          Solution solution)
      {
            IDiagramGenerator generator = new PlantUMLDiagramGenerator();

            return generator.Process(solution);
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