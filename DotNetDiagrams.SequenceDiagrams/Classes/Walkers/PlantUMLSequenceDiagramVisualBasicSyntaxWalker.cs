namespace DotNetDiagrams.SequenceDiagrams.Classes.Walkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.VisualBasic;
    using Microsoft.CodeAnalysis.VisualBasic.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;

    using DotNetDiagrams.Common.Extensions;
    using DotNetDiagrams.SequenceDiagrams.Classes.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLSequenceDiagramVisualBasicSyntaxWalker : VisualBasicSyntaxWalker, IPlantUMLSequenceDiagramVisualBasicSyntaxWalker
    {
        private const string group_while = "group while";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private string currentTitle;

        private bool ignore;

        public PlantUMLSequenceDiagramVisualBasicSyntaxWalker(
            Compilation compilation,
            SyntaxTree syntaxTree,
            Solution solution,
            Project project)
        {
            this.Diagrams = new PlantUMLSequenceDiagrams();

            this.compilation = compilation;

            this.syntaxTree = syntaxTree;

            this.solution = solution;

            this.project = project;
        }

        public IPlantUMLSequenceDiagram Diagram => Diagrams.GetSequenceDiagramAtTitleOrDefault(currentTitle);

        public IPlantUMLSequenceDiagrams Diagrams { get; }

        // TODO: Finish
        private void Visit(
            WhileBlockSyntax whileBlock)
        {
        }
    }
}