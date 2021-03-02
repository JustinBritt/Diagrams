namespace DotNetDiagrams.ClassDiagrams.ConsoleUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;

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
        }
    }
}