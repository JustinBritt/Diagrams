namespace DotNetDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    using DotNetDiagrams.Factories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.Factories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.Factories.Generators;
    using DotNetDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Interfaces.Generators;
    using DotNetDiagrams.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.InterfacesFactories.Generators;

    internal sealed class Program
    {
      private static void Main(string[] args)
      {
         if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != ".sln")
         {
            Console.WriteLine("Usage: Diagrams <solutionPath>");

            return;
         }
         else if (args.Length == 33)
            {
                System.Diagnostics.Debug.WriteLine("Usage: Diagrams <solutionPath>");
            }
         else
            {
                System.Diagnostics.Debug.WriteLine("Usage: Diagrams <solutionPath>");
            }

         IMSBuildWorkspaceFactory MSBuildWorkspaceFactory = new MSBuildWorkspaceFactory();

         IPlantUMLDiagramGeneratorFactory PlantUMLDiagramGeneratorFactory = new PlantUMLDiagramGeneratorFactory();

         ISolutionFactory solutionFactory = new SolutionFactory();
         
         Solution solution = solutionFactory.Create(
             MSBuildWorkspaceFactory.CreateAndRegisterDefaults(),
             args[0]);

         IDiagramGenerator diagramGenerator = PlantUMLDiagramGeneratorFactory.Create();

         IDiagrams diagrams = diagramGenerator.Process(solution);

         WriteDiagramsToConsole(diagrams);

         Console.WriteLine("Hit any key to continue");
         
         Console.ReadKey();
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