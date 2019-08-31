using System;

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
            string solutionPath = args[0];

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            DiagramGenerator diagramGenerator = new DiagramGenerator(solutionPath, workspace);
            diagramGenerator.ProcessSolution().Wait();
            diagramGenerator.GenerateDiagramFromRoot();
            Console.ReadKey();
        }
    }
}
