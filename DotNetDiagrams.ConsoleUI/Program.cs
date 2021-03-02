namespace DotNetDiagrams.ConsoleUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Factories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.Common.Factories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.SequenceDiagrams.Factories.Generators;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Generators;
    using DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis;
    using DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis.MSBuild;
    using DotNetDiagrams.SequenceDiagrams.InterfacesFactories.Generators;

    internal sealed class Program
    {
        private const string expectedSolutionExtension = ".sln";

        private static void Main(string[] args)
        {
            if (args.Length != 1 || Path.GetExtension(args[0]).ToLowerInvariant() != expectedSolutionExtension)
            {
                Console.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} <solutionPath>");

                return;
            }

            IMSBuildWorkspaceFactory MSBuildWorkspaceFactory = new MSBuildWorkspaceFactory();

            IPlantUMLSequenceDiagramGeneratorFactory PlantUMLDiagramGeneratorFactory = new PlantUMLSequenceDiagramGeneratorFactory();

            ISolutionFactory solutionFactory = new SolutionFactory();

            Solution solution = solutionFactory.Create(
                MSBuildWorkspaceFactory.CreateAndRegisterDefaults(),
                args[0]);

            ISequenceDiagramGenerator sequencediagramGenerator = PlantUMLDiagramGeneratorFactory.Create();

            IDiagrams diagrams = sequencediagramGenerator.Process(solution);

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