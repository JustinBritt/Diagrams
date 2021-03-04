﻿namespace DotNetDiagrams.ClassDiagrams.Classes.Walkers
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
        private const string modifier_abstract = "{abstract}";
        private const string modifier_protectedInternal = "# <<internal>>";
        private const string modifier_static = "{static}";

        private const string stereotype_event = "<<event>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_new = "<<new>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_readonly = "<<readonly>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";
        private const string stereotype_virtual = "<<virtual>>";

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
        private const string PlantUML_title = "title";

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
            AddCommand(PlantUML_enduml);
        }

        private string DetermineTitle(
            TypeDeclarationSyntax typeDeclaration)
        {
            string namespaceName = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().SingleOrDefault().Name.ToString();

            List<TypeDeclarationSyntax> parentTypes = new List<TypeDeclarationSyntax>();

            List<TypeDeclarationSyntax> declaredTypes = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            foreach (TypeDeclarationSyntax declaredType in declaredTypes)
            {
                if (declaredType.DescendantNodesAndSelf().Contains(typeDeclaration))
                {
                    parentTypes.Add(declaredType);
                }
            }

            string typeName = String.Join(".", parentTypes.Select(w => w.Identifier.ValueText));

            return $"{namespaceName}.{typeName}";
        }

        private void StartDiagram(
            TypeDeclarationSyntax typeDeclaration)
        {
            currentTitle = this.DetermineTitle(typeDeclaration);

            if (!String.IsNullOrEmpty(currentTitle))
            {
                if (!Diagrams.ContainsTitle(currentTitle))
                {
                    Diagrams.AddTitle(currentTitle);
                }

                this.AddHeader(
                    currentTitle);
            }
        }

        private void AddHeader(
            string title)
        {
            currentHeader = new List<string>();

            currentHeader.Add(PlantUML_startuml);

            currentHeader.Add($"{PlantUML_title} {title}");

            currentHeader.ForEach(w => AddCommand(w));
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
                case BaseListSyntax baseList:
                    this.Visit(baseList);
                    break;
                case ClassDeclarationSyntax classDeclaration:
                    this.Visit(classDeclaration);
                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    this.Visit(constructorDeclaration);
                    break;
                case FieldDeclarationSyntax fieldDeclaration:
                    this.Visit(fieldDeclaration);
                    break;
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    this.Visit(interfaceDeclaration);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    this.Visit(methodDeclaration);
                    break;
                case ParameterSyntax parameter:
                    this.Visit(parameter);
                    break;
                case ParameterListSyntax parameterList:
                    this.Visit(parameterList);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    this.Visit(propertyDeclaration);
                    break;
                case SimpleBaseTypeSyntax simpleBaseType:
                    this.Visit(simpleBaseType);
                    break;
                case StructDeclarationSyntax structDeclaration:
                    this.Visit(structDeclaration);
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
            BaseListSyntax baseList)
        {
            base.Visit(
                baseList);
        }

        private void Visit(
            ClassDeclarationSyntax classDeclaration)
        {
            string className = classDeclaration.Identifier.ValueText;

            List<string> CSharpModifiers = classDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "abstract" => PlantUML_abstract,

                    "internal" => stereotype_internal,

                    "partial" => stereotype_partial,

                    "private" => PlantUML_private,

                    "protected" => PlantUML_protected,

                    "public" => PlantUML_public,

                    "sealed" => stereotype_sealed,

                    "static" => stereotype_static,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            string combinedModifiers = String.Join(" ", PlantUMLModifiers);

            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (classDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    classDeclaration);
            }

            this.AddCommand($"{PlantUML_class} {className} {combinedModifiers}");

            base.Visit(
                classDeclaration);

            if (classDeclaration == declaredTypes.Last())
            {
                this.EndDiagram();
            }
        }

        private void Visit(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(
                constructorDeclaration);
        }

        private void Visit(
            FieldDeclarationSyntax fieldDeclaration)
        {
            base.Visit(
                fieldDeclaration);
        }

        private void Visit(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            string interfaceName = interfaceDeclaration.Identifier.ValueText;

            List<string> CSharpModifiers = interfaceDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "internal" => stereotype_internal,

                    "private" => PlantUML_private,

                    "public" => PlantUML_public,

                    _ => String.Empty
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            string combinedModifiers = String.Join(" ", PlantUMLModifiers);

            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (interfaceDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    interfaceDeclaration);
            }

            this.AddCommand($"{combinedModifiers} {PlantUML_interface} {interfaceName}");

            base.Visit(
                interfaceDeclaration);

            if (interfaceDeclaration == declaredTypes.Last())
            {
                this.EndDiagram();
            }
        }

        private void Visit(
            MethodDeclarationSyntax methodDeclarationSyntax)
        {
            base.Visit(
                methodDeclarationSyntax);
        }

        private void Visit(
            ParameterSyntax parameterSyntax)
        {
            base.Visit(
                parameterSyntax);
        }

        private void Visit(
            ParameterListSyntax parameterList)
        {
            base.Visit(
                parameterList);
        }

        private void Visit(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            base.Visit(
                propertyDeclaration);
        }

        private void Visit(SimpleBaseTypeSyntax simpleBaseType)
        {
            base.Visit(
                simpleBaseType);
        }

        private void Visit(
            StructDeclarationSyntax structDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (structDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    structDeclaration);
            }

            base.Visit(
                structDeclaration);

            if (structDeclaration == declaredTypes.Last())
            {
                this.EndDiagram();
            }
        }
    }
}