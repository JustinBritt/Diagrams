namespace DotNetDiagrams.ClassDiagrams.Classes.Walkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Extensions;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLClassDiagramWalker : CSharpSyntaxWalker, IPlantUMLClassDiagramWalker
    {
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_startuml = "@startuml";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private List<string> currentHeader;
        private string currentTitle;

        public PlantUMLClassDiagramWalker(
            Compilation compilation,
            SyntaxTree syntaxTree,
            Solution solution,
            Project project)
        {
            this.Diagrams = new PlantUMLDiagrams();

            this.compilation = compilation;

            this.syntaxTree = syntaxTree;

            this.solution = solution;

            this.project = project;
        }

        public IPlantUMLDiagrams Diagrams { get; }

        private void EndDiagram()
        {
        }

        private void StartDiagram(
            TypeDeclarationSyntax typeDeclaration)
        {
        }

        /// <summary>
        /// This visits a node in the syntax tree.
        /// </summary>
        /// <param name="node">Node</param>
        public override void Visit(
            SyntaxNode node)
        {
            switch (node)
            {
                case ConstructorDeclarationSyntax constructorDeclaration:
                    this.Visit(constructorDeclaration);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        private void Visit(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(
                constructorDeclaration);
        }
    }
}