using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

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

         ExecuteStrategy1(args);
         //ExecuteStrategy2(args);

            Console.WriteLine("Hit any key to continue");
            Console.ReadKey();
        }

        private static void ExecuteStrategy2(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = workspace.OpenSolutionAsync(args[0]).GetAwaiter().GetResult();

            foreach (Project project in solution.Projects)
            {
                Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    Walker walker = new Walker();
                    SyntaxNode root = syntaxTree.GetRoot();
                    walker.Visit(root);
                    Console.WriteLine("-----------------------------");
                }
            }
        }

        private static void ExecuteStrategy1(string[] args)
        {
            PlantUmlDiagramGenerator diagramGenerator = new PlantUmlDiagramGenerator(args[0]);
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
