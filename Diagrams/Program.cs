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

            Console.WriteLine("Hit any key to continue");
            Console.ReadKey();
        }
    }
}
