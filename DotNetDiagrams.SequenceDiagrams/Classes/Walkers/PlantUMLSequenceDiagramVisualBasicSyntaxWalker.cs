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
        private const string group_do = "group do";
        private const string group_doWhile = "group do/while";
        private const string group_for = "group for";
        private const string group_foreach = "group foreach";
        private const string group_while = "group while";

        private const string PlantUML_alt = "alt";
        private const string PlantUML_arrow = "->";
        private const string PlantUML_autoactivate = "autoactivate";
        private const string PlantUML_colon = ":";
        private const string PlantUML_dottedArrow = "-->";
        private const string PlantUML_else = "else";
        private const string PlantUML_end = "end";
        private const string PlantUML_footbox = "footbox";
        private const string PlantUML_hide = "hide";
        private const string PlantUML_off = "off";
        private const string PlantUML_on = "on";
        private const string PlantUML_opt = "opt";
        private const string PlantUML_show = "show";
        private const string PlantUML_title = "title";

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