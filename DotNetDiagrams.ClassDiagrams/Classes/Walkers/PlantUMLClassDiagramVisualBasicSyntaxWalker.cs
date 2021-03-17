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
        private const string stereotype_add = "<<add>>";
        private const string stereotype_const = "<<const>>";
        private const string stereotype_get = "<<get>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_raise = "<<raise>>";
        private const string stereotype_remove = "<<remove>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_set = "<<set>>";

        private const string stringJoinSeparator_accessors = " ";
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

        private string GetAccessor(
            AccessorStatementSyntax accessorStatement)
        {
            return String.Concat(
                this.GetJoinedModifiers(accessorStatement),
                accessorStatement.AccessorKeyword.ValueText switch
                {
                    "AddHandler" => stereotype_add,

                    "Get" => stereotype_get,

                    "RaiseEvent" => stereotype_raise,

                    "RemoveHandler" => stereotype_remove,

                    "Set" => stereotype_set,

                    _ => throw new Exception(nameof(accessorStatement.AccessorKeyword))
                });
        }

        private string GetJoinedAccessors(
            PropertyBlockSyntax propertyBlock)
        {
            return propertyBlock.Accessors.Count() > 0
                ? String.Join(
                    stringJoinSeparator_accessors,
                    propertyBlock.Accessors.Select(w => this.GetAccessor(w.AccessorStatement)))
                : String.Empty;
        }

        private string GetJoinedModifiers(
            AccessorStatementSyntax accessorStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                accessorStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "Private" => stereotype_private,

                    "Public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

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
            EnumStatementSyntax enumStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                enumStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "Private" => stereotype_private,

                    "Protected" => stereotype_protected,

                    "Public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

        // TODO: Should Dim be mapped to an empty string?
        private string GetJoinedModifiers(
            FieldDeclarationSyntax fieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                fieldDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Const" => stereotype_const,

                    "Dim" => String.Empty,

                    "Friend" => stereotype_internal,

                    "Private" => stereotype_private,

                    "Protected" => stereotype_protected,

                    "Public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            SubNewStatementSyntax subNewStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                subNewStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

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

                    "Partial" => stereotype_partial,

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
                case ConstructorBlockSyntax constructorBlock:
                    this.Visit(constructorBlock);
                    break;
                case EnumBlockSyntax enumBlock:
                    this.Visit(enumBlock);
                    break;
                case EnumMemberDeclarationSyntax enumMemberDeclaration:
                    this.Visit(enumMemberDeclaration);
                    break;
                case EventBlockSyntax eventBlock:
                    this.Visit(eventBlock);
                    break;
                case FieldDeclarationSyntax fieldDeclaration:
                    this.Visit(fieldDeclaration);
                    break;
                case InterfaceBlockSyntax interfaceBlock:
                    this.Visit(interfaceBlock);
                    break;
                case PropertyBlockSyntax propertyBlock:
                    this.Visit(propertyBlock);
                    break;
                case StructureBlockSyntax structureBlock:
                    this.Visit(structureBlock);
                    break;
                case SubNewStatementSyntax subNewStatement:
                    this.Visit(subNewStatement);
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

            base.Visit(
                classBlock);
        }

        // TODO: Finish
        private void Visit(
            ConstructorBlockSyntax constructorBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                constructorBlock.SubNewStatement);

            base.Visit(
                constructorBlock);
        }

        // TODO: Finish
        private void Visit(
            EnumBlockSyntax enumBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                enumBlock.EnumStatement);

            base.Visit(
                enumBlock);
        }

        // TODO: Finish
        private void Visit(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            base.Visit(
                enumMemberDeclaration);
        }

        // TODO: Finish
        private void Visit(
            EventBlockSyntax eventBlock)
        {
            base.Visit(
                eventBlock);
        }

        // TODO: Finish
        private void Visit(
            FieldDeclarationSyntax fieldDeclaration)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                fieldDeclaration);

            base.Visit(
                fieldDeclaration);
        }

        // TODO: Finish
        private void Visit(
            InterfaceBlockSyntax interfaceBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                interfaceBlock.InterfaceStatement);

            base.Visit(
                interfaceBlock);
        }

        // TODO: Finish
        private void Visit(
            PropertyBlockSyntax propertyBlock)
        {
            string joinedAccessors = this.GetJoinedAccessors(
                propertyBlock);

            base.Visit(
                propertyBlock);
        }

        // TODO: Finish 
        private void Visit(
            StructureBlockSyntax structureBlock)
        {
            base.Visit(
                structureBlock);
        }

        // TODO: Finish
        private void Visit(
            SubNewStatementSyntax subNewStatement)
        {
            base.Visit(
                subNewStatement);
        }
    }
}