namespace DotNetDiagrams.ClassDiagrams.ConsoleUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Factories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.Common.Factories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.ClassDiagrams.Factories.Generators;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;
    using DotNetDiagrams.ClassDiagrams.InterfacesFactories.Generators;

    internal sealed class Program
    {
        private const string expectedSolutionExtension = ".sln";

        public static void Main(string[] args)
        {
            if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != expectedSolutionExtension)
            {
                Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} <solutionPath>");

                return;
            }

            IMSBuildWorkspaceFactory MSBuildWorkspaceFactory = new MSBuildWorkspaceFactory();

            IPlantUMLClassDiagramGeneratorFactory PlantUMLClassDiagramGeneratorFactory = new PlantUMLClassDiagramGeneratorFactory();

            ISolutionFactory solutionFactory = new SolutionFactory();

            Solution solution = solutionFactory.Create(
                MSBuildWorkspaceFactory.CreateAndRegisterDefaults(),
                args[0]);

            IClassDiagramGenerator classdiagramGenerator = PlantUMLClassDiagramGeneratorFactory.Create();

            IDiagrams diagrams = classdiagramGenerator.Process(solution);

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