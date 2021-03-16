namespace DotNetDiagrams.ClassDiagrams.Classes.Walkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.VisualBasic;
    using Microsoft.CodeAnalysis.VisualBasic.Syntax;

    using DotNetDiagrams.ClassDiagrams.Classes.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLClassDiagramVisualBasicSyntaxWalker : VisualBasicSyntaxWalker, IPlantUMLClassDiagramVisualBasicSyntaxWalker
    {
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_sealed = "<<sealed>>";

        private const string stringJoinSeparator_modifiers = " ";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private string currentTitle;

        public PlantUMLClassDiagramVisualBasicSyntaxWalker(
            Compilation compilation,
            SyntaxTree syntaxTree,
            Solution solution,
            Project project)
        {
            this.Diagrams = new PlantUMLClassDiagrams();

            this.compilation = compilation;

            this.syntaxTree = syntaxTree;

            this.solution = solution;

            this.project = project;
        }

        private IPlantUMLClassDiagram Diagram => Diagrams.GetClassDiagramAtTitleOrDefault(currentTitle);

        public IPlantUMLClassDiagrams Diagrams { get; }

        private string GetJoinedModifiers(
            ClassStatementSyntax classStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                classStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "NotInheritable" => stereotype_sealed,

                    "Partial" => stereotype_partial,

                    "Private" => stereotype_private,

                    "Protected" => stereotype_protected,

                    "Public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            InterfaceStatementSyntax interfaceStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                interfaceStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "Private" => stereotype_private,

                    "Protected" => stereotype_protected,

                    "Public" => stereotype_public,

                    _ => throw new Exception("")
                }));
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
                case ClassBlockSyntax classBlock:
                    this.Visit(classBlock);
                    break;
                case InterfaceBlockSyntax interfaceBlock:
                    this.Visit(interfaceBlock);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        // TODO: Finish
        private void Visit(
            ClassBlockSyntax classBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                classBlock.ClassStatement);

            var implements = classBlock.Implements;

            var inherits = classBlock.Inherits;
        }
        
        // TODO: Finish
        private void Visit(
            InterfaceBlockSyntax interfaceBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                interfaceBlock.InterfaceStatement);
        }
    }
}