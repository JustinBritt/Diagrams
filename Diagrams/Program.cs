namespace DotNetDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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

         ExecuteStrategy2(args);

         Console.WriteLine("Hit any key to continue");
         
         Console.ReadKey();
      }

      private static void ExecuteStrategy2(string[] args)
      {
         IDiagramGenerator generator = new PlantUMLDiagramGenerator(args[0]);
         
         generator.Process();
            
         WriteDiagramsToConsole(generator.Diagrams);
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