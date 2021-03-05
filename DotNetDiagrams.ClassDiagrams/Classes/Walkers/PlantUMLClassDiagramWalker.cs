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
    using DotNetDiagrams.ClassDiagrams.Classes.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLClassDiagramWalker : CSharpSyntaxWalker, IPlantUMLClassDiagramWalker
    {
        private const string modifier_abstract = "{abstract}";
        private const string modifier_protectedInternal = "# <<internal>>";
        private const string modifier_static = "{static}";

        private const string stereotype_abstract = "<<abstract>>";
        private const string stereotype_event = "<<event>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_new = "<<new>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_readonly = "<<readonly>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";
        private const string stereotype_unsafe = "<<unsafe>>";
        private const string stereotype_virtual = "<<virtual>>";

        private const string PlantUML_abstract = "abstract";
        private const string PlantUML_annotation = "annotation";
        private const string PlantUML_class = "class";   
        private const string PlantUML_entity = "entity";
        private const string PlantUML_enum = "enum";
        private const string PlantUML_implements = "implements";
        private const string PlantUML_interface = "interface";
        private const string PlantUML_leftBrace = "{";
        private const string PlantUML_packageProtected = "~";
        private const string PlantUML_private = "-";
        private const string PlantUML_protected = "#";
        private const string PlantUML_public = "+";
        private const string PlantUML_rightBrace = "}";
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
            this.Diagrams = new PlantUMLClassDiagrams();

            this.compilation = compilation;

            this.syntaxTree = syntaxTree;

            this.solution = solution;

            this.project = project;
        }

        private IPlantUMLClassDiagram Diagram => Diagrams.GetClassDiagramAtTitleOrDefault(currentTitle);

        //private List<string> PlantUMLCode => Diagrams.GetCodeAtTitleOrDefault(currentTitle);

        public IPlantUMLClassDiagrams Diagrams { get; }

        private void AddCommand(
            string command)
        {
            Diagram.Body.Add(command);
        }

        // TOOD: namespaceName can be null in unit test projects
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

            currentHeader.Add($"{PlantUML_title} {title}");

            Diagram.Header.AddRange(currentHeader);
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
                    "abstract" => stereotype_abstract,

                    "internal" => stereotype_internal,

                    "partial" => stereotype_partial,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "sealed" => stereotype_sealed,

                    "static" => stereotype_static,
                    
                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            string joinedModifiers = String.Join(" ", PlantUMLModifiers);

            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (classDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    classDeclaration);
            }

            // Base types
            List<string> baseTypeNames = new List<string>();

            if (classDeclaration.BaseList is not null)
            {
                List<BaseTypeSyntax> baseTypes = classDeclaration.BaseList.Types.ToList();

                foreach (BaseTypeSyntax baseType in baseTypes)
                {
                    SemanticModel semanticModel;

                    semanticModel = compilation.GetSemanticModel(baseType.SyntaxTree, true);

                    if (ModelExtensions.GetTypeInfo(semanticModel, baseType.Type).Type is INamedTypeSymbol targetType)
                    {
                        baseTypeNames.Add(
                            targetType.ToString());
                    }
                }
            }

            string joinedBaseTypeNames = String.Empty;
            
            if(baseTypeNames.Count > 0)
            {
                joinedBaseTypeNames = $"{PlantUML_implements} {String.Join(",", baseTypeNames)}";
            }

            // Properties
            List<string> properties = new List<string>();

            if (classDeclaration.Members.OfType<PropertyDeclarationSyntax>() is not null)
            {
                foreach (PropertyDeclarationSyntax propertyDeclaration in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
                {
                    string propertyName = propertyDeclaration.Identifier.ValueText;

                    List<string> accessors = new List<string>();

                    if (propertyDeclaration.AccessorList is not null)
                    {
                        foreach (AccessorDeclarationSyntax accessorDeclaration in propertyDeclaration.AccessorList.Accessors)
                        {
                            accessors.Add($"<<{accessorDeclaration.Keyword.ValueText}>>");
                        }
                    }

                    properties.Add($"{propertyName} : {string.Join(" ", accessors)}");
                }
            }

            string joinedProperties = String.Empty;

            if (properties.Count > 0)
            {
                joinedProperties = $"{String.Join("\n", properties)}";
            }

            // Fields
            if (classDeclaration.Members.OfType<FieldDeclarationSyntax>() is not null)
            {
                foreach (FieldDeclarationSyntax fieldDeclaration in classDeclaration.Members.OfType<FieldDeclarationSyntax>())
                {

                }
            }

            // Methods
            if (classDeclaration.Members.OfType<MethodDeclarationSyntax>() is not null)
            {
                foreach (MethodDeclarationSyntax methodDeclaration in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
                {

                }
            }

            this.AddCommand($"{PlantUML_class} {className} {joinedModifiers} {joinedBaseTypeNames} {PlantUML_leftBrace}");
            this.AddCommand($"{joinedProperties}");

            base.Visit(
                classDeclaration);

            this.AddCommand($"{PlantUML_rightBrace}");
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

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            string joinedModifiers = String.Join(" ", PlantUMLModifiers);

            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (interfaceDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    interfaceDeclaration);
            }

            this.AddCommand($"{PlantUML_interface} {interfaceName} {joinedModifiers}");

            base.Visit(
                interfaceDeclaration);
        }

        // TODO: Finish
        // TODO: How does this handle nesting?
        private void Visit(
            MethodDeclarationSyntax methodDeclaration)
        {
            string methodName = methodDeclaration.Identifier.ValueText;

            // TODO: Need to get all parents to account for nesting
            string parentName = methodDeclaration.GetParent<TypeDeclarationSyntax>().Identifier.ValueText;

            AddCommand($"{parentName} : {methodName}");

            base.Visit(
                methodDeclaration);
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

        // TODO: Finish
        private void Visit(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            //string propertyName = propertyDeclaration.Identifier.ValueText;

            //List<string> accessors = new List<string>();

            //if (propertyDeclaration.AccessorList is not null)
            //{
            //    foreach (AccessorDeclarationSyntax accessorDeclaration in propertyDeclaration.AccessorList.Accessors)
            //    {
            //        accessors.Add($"<<{accessorDeclaration.Keyword.ValueText}>>");
            //    }
            //}

            //this.AddCommand($"{propertyName} : {string.Join(" ", accessors)}");

            base.Visit(
                propertyDeclaration);
        }

        private void Visit(
            SimpleBaseTypeSyntax simpleBaseType)
        {
            base.Visit(
                simpleBaseType);
        }

        private void Visit(
            StructDeclarationSyntax structDeclaration)
        {
            string structName = structDeclaration.Identifier.ValueText;

            List<string> CSharpModifiers = structDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "internal" => stereotype_internal,

                    "partial" => stereotype_partial,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            string joinedModifiers = String.Join(" ", PlantUMLModifiers);

            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (structDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    structDeclaration);
            }

            this.AddCommand($"{PlantUML_interface} {structName} {joinedModifiers}");

            base.Visit(
                structDeclaration);
        }
    }
}