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
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";

        private const string PlantUML_abstract = "abstract";
        private const string PlantUML_annotation = "annotation";
        private const string PlantUML_class = "class";
        private const string PlantUML_entity = "entity";
        private const string PlantUML_enum = "enum";
        private const string PlantUML_interface = "interface";
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_packageProtected = "~";
        private const string PlantUML_private = "-";
        private const string PlantUML_protected = "#";
        private const string PlantUML_public = "+";
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

        private List<string> PlantUMLCode => Diagrams.GetCodeAtTitleOrDefault(currentTitle);

        public IPlantUMLDiagrams Diagrams { get; }

        private void AddCommand(
            string command)
        {
            this.PlantUMLCode.Add(command);
        }

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
                case AccessorDeclarationSyntax accessorDeclaration:
                    this.Visit(accessorDeclaration);
                    break;
                case AccessorListSyntax accessorList:
                    this.Visit(accessorList);
                    break;
                case ClassDeclarationSyntax classDeclaration:
                    this.Visit(classDeclaration);
                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    this.Visit(constructorDeclaration);
                    break;
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    this.Visit(interfaceDeclaration);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        private void Visit(
           AccessorDeclarationSyntax accessorDeclaration)
        {
            base.Visit(
                accessorDeclaration);
        }

        private void Visit(
           AccessorListSyntax accessorList)
        {
            base.Visit(
                accessorList);
        }

        private void Visit(
            ClassDeclarationSyntax classDeclaration)
        {
            string className = classDeclaration.Identifier.ValueText;

            base.Visit(
                classDeclaration);
        }

        private void Visit(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(
                constructorDeclaration);
        }

        private void Visit(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            string interfaceName = interfaceDeclaration.Identifier.ValueText;

            base.Visit(
                interfaceDeclaration);
        }
    }
}