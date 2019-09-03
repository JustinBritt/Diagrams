using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetDiagrams
{

   internal class Program
   {
      private static void Main(string[] args)
      {
         if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != ".sln")
         {
            Console.WriteLine("Usage: Diagrams <solutionPath>");
            return;
         }

         //ExecuteStrategy1(args);
         ExecuteStrategy2(args);

         Console.WriteLine("Hit any key to continue");
         Console.ReadKey();
      }

      private static void ExecuteStrategy2(string[] args)
      {
         PlantUmlDiagramGenerator generator = new PlantUmlDiagramGenerator(args[0]);
         generator.Process();

         foreach (string title in generator.Diagrams.Keys)
         {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(title);
            Console.WriteLine("-----------------------------");

            foreach (string code in PlantWalker.Diagrams[title])
               Console.WriteLine(code);

            Console.WriteLine();
         }
      }

      private static void ExecuteStrategy1(string[] args)
      {
         JsSequenceDiagramGenerator diagramGenerator = new JsSequenceDiagramGenerator(args[0]);
         IEnumerable<string> diagramNames = diagramGenerator.GetDiagramNames();

         foreach (string diagramName in diagramNames)
         {
            IEnumerable<string> diagramCode = diagramGenerator.GetDiagram(diagramName);

            Console.WriteLine(diagramName);

            foreach (string text in diagramCode)
               Console.WriteLine("   " + text);

            Console.WriteLine();
         }
      }
   }
}
