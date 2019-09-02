using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.MSBuild;

namespace DotNetDiagrams
{
    /// <summary>
    ///     this is a proof of concept brute-force approach to get familiar with Roslyn
    ///     - the Wiki provides details on usage: https://github.com/SoundLogic/Diagrams/wiki
    ///     For interested parties:
    ///     Due to some of the limitations of Roslyn (the nature of analyzing IL - lambdas/iterator
    ///     methods/await | accessing code in external DLLs ), I did not continue
    ///     to pursue / formalize this project - it would likely be easier to use a static analysis
    ///     library like Mono.Cecil
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != ".sln")
            {
                Console.WriteLine("Usage: Sequence <solution path>");
                return;
            }

            string solutionPath = args[0];

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            DiagramGenerator diagramGenerator = new DiagramGenerator(solutionPath, workspace);
            diagramGenerator.ProcessSolution().GetAwaiter().GetResult();
            Dictionary<string, IEnumerable<string>> diagrams = diagramGenerator.GenerateDiagrams();

            foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in diagrams)
            {
                Console.WriteLine();
                Console.WriteLine(keyValuePair);

                foreach (string s in keyValuePair.Value)
                {
                    Console.WriteLine("   "+s);
                }
            }
            Console.WriteLine("Hit any key to close");
            Console.ReadKey();
        }
    }
}
