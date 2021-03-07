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
        private const string PlantUML_leftParenthesis = "(";
        private const string PlantUML_packageProtected = "~";
        private const string PlantUML_private = "-";
        private const string PlantUML_protected = "#";
        private const string PlantUML_public = "+";
        private const string PlantUML_rightBrace = "}";
        private const string PlantUML_rightParenthesis = ")";
        private const string PlantUML_title = "title";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

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

        public IPlantUMLClassDiagrams Diagrams { get; }

        private void AddCommand(
            string command)
        {
            this.Diagram.Body.Add(command);
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
            this.currentTitle = this.DetermineTitle(typeDeclaration);

            if (!String.IsNullOrEmpty(this.currentTitle))
            {
                if (!this.Diagrams.ContainsTitle(this.currentTitle))
                {
                    this.Diagrams.AddTitle(this.currentTitle);
                }

                this.AddHeader(
                    this.currentTitle);
            }
        }

        private void AddHeader(
            string title)
        {
            List<string> currentHeader = new List<string>();

            currentHeader.Add($"{PlantUML_title} {title}");

            this.Diagram.Header.AddRange(currentHeader);
        }

        private string BuildMethodDeclarationCommand(
            string explicitInterfaceSpecifierTypeName,
            string joinedConstraintClauses,
            string joinedModifiers,
            string joinedParameters,
            string joinedTypeParameters,
            string methodName,
            string returnTypeName)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);
            }

            sb.Append(" ");

            sb.Append(returnTypeName);

            sb.Append(" ");

            if (explicitInterfaceSpecifierTypeName.Length > 0)
            {
                sb.Append($"{explicitInterfaceSpecifierTypeName}.");
            }

            sb.Append(methodName);

            if (joinedTypeParameters.Length > 0)
            {
                sb.Append($"<{joinedTypeParameters}>");
            }

            sb.Append(PlantUML_leftParenthesis);

            sb.Append(joinedParameters);

            sb.Append(PlantUML_rightParenthesis);

            if (joinedConstraintClauses.Length > 0)
            {
                sb.Append(" where ");

                sb.Append(joinedConstraintClauses);
            }

            return sb.ToString();
        }

        // TODO: Finish
        private string BuildPropertyDeclarationCommand(
            string explicitInterfaceSpecifierTypeName,
            string joinedAccessors,
            string joinedModifiers,
            string propertyName,
            string propertyTypeName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(joinedModifiers);

            sb.Append(" ");

            sb.Append(propertyTypeName);

            sb.Append(" ");

            if (explicitInterfaceSpecifierTypeName.Length > 0)
            {
                sb.Append($"{explicitInterfaceSpecifierTypeName}.");
            }

            sb.Append(propertyName);

            sb.Append(" ");

            sb.Append(":");

            sb.Append(" ");

            sb.Append(joinedAccessors);

            return sb.ToString();
        }

        private List<string> GetAccessors(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            List<string> accessors = new List<string>();

            if (propertyDeclaration.AccessorList is not null)
            {
                foreach (AccessorDeclarationSyntax accessorDeclaration in propertyDeclaration.AccessorList.Accessors)
                {
                    accessors.Add($"<<{accessorDeclaration.Keyword.ValueText}>>");
                }
            }

            return accessors;
        }

        private string GetConstraint(
            SemanticModel semanticModel,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            string typeParameterConstraintName = String.Empty;

            if (typeParameterConstraint is TypeConstraintSyntax typeConstraint)
            {
                if (ModelExtensions.GetTypeInfo(semanticModel, typeConstraint.Type).Type is INamedTypeSymbol targetType)
                {
                    typeParameterConstraintName = targetType.ToString();
                }
            }
            else
            {
                typeParameterConstraintName = typeParameterConstraint.ToString();
            }

            return typeParameterConstraintName;
        }

        private List<string> GetConstraintClauses(
            MethodDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel)
        {
            List<string> constraintClauses = new List<string>();

            if (methodDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in methodDeclaration.ConstraintClauses.ToList())
                {
                    string constraintClauseName = constraintClause.Name.Identifier.ValueText;

                    string joinedConstraints = this.GetJoinedConstraints(
                        constraintClause,
                        semanticModel);

                    constraintClauses.Add($"{constraintClauseName} : {joinedConstraints}");
                }
            }

            return constraintClauses;
        }

        private List<string> GetConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            SemanticModel semanticModel)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                string typeParameterConstraintName = this.GetConstraint(
                    semanticModel,
                    typeParameterConstraint);

                constraints.Add(
                    typeParameterConstraintName);
            }

            return constraints;
        }

        private string GetJoinedAccessors(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            List<string> accessors = this.GetAccessors(
                propertyDeclaration);

            return String.Join(
                " ",
                accessors);
        }

        private string GetJoinedConstraintClauses(
            MethodDeclarationSyntax methodDeclaration,
            SemanticModel semanticModel)
        {
            List<string> constraintClauses = this.GetConstraintClauses(
                methodDeclaration,
                semanticModel);

            return String.Join(
                ", ",
                constraintClauses);
        }

        private string GetJoinedConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            SemanticModel semanticModel)
        {
            List<string> constraints = this.GetConstraints(
                constraintClause,
                semanticModel);

            return String.Join(
                ", ",
                constraints.Select(w => w.ToString()));
        }

        private string GetJoinedModifiers(
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                fieldDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedModifiers(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                methodDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedModifiers(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                propertyDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedParameters(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> parameters = this.GetParameters(
                methodDeclaration);

            return String.Join(
                ",",
                parameters);
        }

        private string GetJoinedTypeParameters(
             MethodDeclarationSyntax methodDeclaration)
        {
            List<string> typeParameters = this.GetTypeParameters(
                methodDeclaration);

            return String.Join(
                ", ",
                typeParameters);
        }

        private List<string> GetModifiers(
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> CSharpModifiers = fieldDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> CSharpModifiers = methodDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "abstract" => modifier_abstract,

                    "internal" => stereotype_internal,

                    "override" => stereotype_override,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "static" => modifier_static,

                    "unafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            List<string> CSharpModifiers = propertyDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "abstract" => modifier_abstract,

                    "internal" => stereotype_internal,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "static" => modifier_static,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetParameters(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> parameters = new List<string>();

            if (methodDeclaration.ParameterList.Parameters.Count > 0)
            {
                foreach (ParameterSyntax parameter in methodDeclaration.ParameterList.Parameters)
                {
                    string parameterName = parameter.Identifier.ValueText;

                    string parameterTypeName = this.GetTypeNameOrFallback(
                        parameter.Type.ToString(),
                        parameter.Type,
                        methodDeclaration.SyntaxTree);

                    parameters.Add($"{parameterTypeName} {parameterName}");
                }
            }

            return parameters;
        }

        private string GetTypeNameOrFallback(
            string fallback,
            SyntaxNode syntaxNode,
            SyntaxTree syntaxTree)
        {
            string name = String.Empty;

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree, true);

            if (ModelExtensions.GetTypeInfo(semanticModel, syntaxNode).Type is INamedTypeSymbol targetType)
            {
                name = targetType.ToString();
            }
            else
            {
                name = fallback;
            }

            return name;
        }

        private List<string> GetTypeParameters(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> typeParameters = new List<string>();

            if (methodDeclaration.TypeParameterList is not null)
            {
                foreach (TypeParameterSyntax typeParameter in methodDeclaration.TypeParameterList.Parameters.ToList())
                {
                    string typeParameterName = typeParameter.Identifier.ValueText;

                    typeParameters.Add(
                        typeParameterName);
                }
            }

            return typeParameters;
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
                case PropertyDeclarationSyntax propertyDeclaration:
                    this.Visit(propertyDeclaration);
                    break;
                case StructDeclarationSyntax structDeclaration:
                    this.Visit(structDeclaration);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        // TODO: Need to account for ConstraintClauses and TypeParameterList
        private void Visit(
            ClassDeclarationSyntax classDeclaration)
        {
            string className = classDeclaration.Identifier.ValueText;

            List<string> modifiers = new List<string>();

            foreach (string CSharpModifier in classDeclaration.Modifiers.Select(w => w.ValueText))
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

                modifiers.Add(PlantUMLModifier);
            }

            string joinedModifiers = String.Join(" ", modifiers);

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
                foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
                {
                    SemanticModel semanticModel;

                    semanticModel = compilation.GetSemanticModel(baseType.SyntaxTree, true);

                    string baseTypeName = String.Empty;

                    if (ModelExtensions.GetTypeInfo(semanticModel, baseType.Type).Type is INamedTypeSymbol targetType)
                    {
                        baseTypeName = targetType.ToString();
                    }
                    else
                    {
                        baseTypeName = baseType.Type.ToString();
                    }

                    baseTypeNames.Add(
                            baseTypeName);
                }
            }

            string joinedBaseTypeNames = String.Empty;

            if (baseTypeNames.Count > 0)
            {
                joinedBaseTypeNames = $"{PlantUML_implements} {String.Join(",", baseTypeNames)}";
            }

            // TypeParameterList
            if (classDeclaration.TypeParameterList is not null)
            {
                foreach (TypeParameterSyntax typeParameter in classDeclaration.TypeParameterList.Parameters.ToList())
                {
                    string typeParameterName = typeParameter.Identifier.ValueText;
                }
            }

            if (classDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in classDeclaration.ConstraintClauses.ToList())
                {
                    foreach (TypeParameterConstraintSyntax constraint in constraintClause.Constraints.ToList())
                    {
                        // TODO: Change
                        string val = constraint.ToFullString();
                    }
                }
            }

            if (classDeclaration.BaseList is null)
            {
                if (classDeclaration.Modifiers.Count > 0)
                {
                    this.AddCommand($"{PlantUML_class} {className} {joinedModifiers} {PlantUML_leftBrace}");
                }
                else
                {
                    this.AddCommand($"{PlantUML_class} {className} {PlantUML_leftBrace}");
                }
            }
            else
            {
                if (classDeclaration.Modifiers.Count > 0)
                {
                    this.AddCommand($"{PlantUML_class} {className} {joinedModifiers} {joinedBaseTypeNames} {PlantUML_leftBrace}");
                }
                else
                {
                    this.AddCommand($"{PlantUML_class} {className} {joinedBaseTypeNames} {PlantUML_leftBrace}");
                }
            }

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

        // TODO: Finish
        private void Visit(
            FieldDeclarationSyntax fieldDeclaration)
        {
            // Modifiers

            List<VariableDeclaratorSyntax> variables = fieldDeclaration.Declaration.Variables.ToList();

            TypeSyntax type = fieldDeclaration.Declaration.Type;

            SemanticModel semanticModel;

            semanticModel = compilation.GetSemanticModel(fieldDeclaration.Declaration.Type.SyntaxTree, true);

            string fieldTypeName = this.GetTypeNameOrFallback(
                fieldDeclaration.Declaration.Type.ToString(),
                fieldDeclaration.Declaration,
                fieldDeclaration.SyntaxTree);

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

        private void Visit(
            MethodDeclarationSyntax methodDeclaration)
        {
            string explicitInterfaceSpecifierTypeName = String.Empty;

            if (methodDeclaration.ExplicitInterfaceSpecifier is not null)
            {
                explicitInterfaceSpecifierTypeName = this.GetTypeNameOrFallback(
                   methodDeclaration.ExplicitInterfaceSpecifier.Name.ToString(),
                   methodDeclaration.ExplicitInterfaceSpecifier.Name,
                   methodDeclaration.SyntaxTree);
            }

            string command = this.BuildMethodDeclarationCommand(
                explicitInterfaceSpecifierTypeName: explicitInterfaceSpecifierTypeName,
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    methodDeclaration: methodDeclaration,
                    semanticModel: compilation.GetSemanticModel(
                        methodDeclaration.SyntaxTree,
                        true)),
                joinedModifiers: this.GetJoinedModifiers(
                    methodDeclaration),
                joinedParameters: this.GetJoinedParameters(
                    methodDeclaration),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    methodDeclaration),
                methodName: methodDeclaration.Identifier.ValueText,
                returnTypeName: this.GetTypeNameOrFallback(
                    fallback: methodDeclaration.ReturnType.ToString(),
                    syntaxNode: methodDeclaration.ReturnType,
                    syntaxTree: methodDeclaration.SyntaxTree));

            this.AddCommand(command);

            base.Visit(
                methodDeclaration);
        }

        // TODO: Account for ExpressionBody and Initializer
        private void Visit(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            string explicitInterfaceSpecifierTypeName = String.Empty;

            if (propertyDeclaration.ExplicitInterfaceSpecifier is not null)
            {
                explicitInterfaceSpecifierTypeName = this.GetTypeNameOrFallback(
                   propertyDeclaration.ExplicitInterfaceSpecifier.Name.ToString(),
                   propertyDeclaration.ExplicitInterfaceSpecifier.Name,
                   propertyDeclaration.SyntaxTree);
            }

            if (propertyDeclaration.ExpressionBody is not null)
            {
                // TODO: Add method
            }

            if(propertyDeclaration.Initializer is not null)
            {
                // TODO: Add method
                ExpressionSyntax initializer = propertyDeclaration.Initializer.Value;
            }

            string command = this.BuildPropertyDeclarationCommand(
                explicitInterfaceSpecifierTypeName: explicitInterfaceSpecifierTypeName,
                joinedAccessors: this.GetJoinedAccessors(
                    propertyDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    propertyDeclaration),
                propertyName: propertyDeclaration.Identifier.ValueText,
                propertyTypeName: this.GetTypeNameOrFallback(
                    propertyDeclaration.Type.ToString(),
                    propertyDeclaration.Type,
                    propertyDeclaration.SyntaxTree));

            this.AddCommand(command);           

            base.Visit(
                propertyDeclaration);
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