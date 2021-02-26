namespace DotNetDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotNetDiagrams.Classes.Generators;

    internal class Program
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
         PlantUMLDiagramGenerator generator = new PlantUMLDiagramGenerator(args[0]);
         generator.Process();

         foreach (string title in generator.Diagrams.Value.Keys)
         {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(title);
            Console.WriteLine("-----------------------------");

            foreach (string code in generator.Diagrams.Value[title])
               Console.WriteLine(code);

            Console.WriteLine();
         }
      }
   }
}