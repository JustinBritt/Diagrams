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

    using DotNetDiagrams.ClassDiagrams.Classes.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLClassDiagramCSharpSyntaxWalker : CSharpSyntaxWalker, IPlantUMLClassDiagramCSharpSyntaxWalker
    {
        private const string modifier_abstract = "{abstract}";
        private const string modifier_protectedInternal = "# <<internal>>";
        private const string modifier_static = "{static}";

        private const string stereotype_abstract = "<<abstract>>";
        private const string stereotype_add = "<<add>>";
        private const string stereotype_arrowExpression = @"<<""=>"">>";
        private const string stereotype_async = "<<async>>";
        private const string stereotype_const = "<<const>>";
        private const string stereotype_equals = "<<=>>";
        private const string stereotype_event = "<<event>>";
        private const string stereotype_extern = "<<extern>>";
        private const string stereotype_fixed = "<<fixed>>";
        private const string stereotype_get = "<<get>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_new = "<<new>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_readonly = "<<readonly>>";
        private const string stereotype_remove = "<<remove>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_set = "<<set>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";
        private const string stereotype_unsafe = "<<unsafe>>";
        private const string stereotype_virtual = "<<virtual>>";
        private const string stereotype_volatile = "<<volatile>>";

        private const string stringJoinSeparator_accessors = " ";
        private const string stringJoinSeparator_baseListTypes = ",";
        private const string stringJoinSeparator_parameters = ", ";
        private const string stringJoinSeparator_typeParameters = ", ";
        private const string stringJoinSeparator_variableDeclarators = ", ";

        private const string PlantUML_abstract = "abstract";
        private const string PlantUML_anchor = "+--";
        private const string PlantUML_annotation = "annotation";
        private const string PlantUML_class = "class";
        private const string PlantUML_entity = "entity";
        private const string PlantUML_enum = "enum";
        private const string PlantUML_extension = "<|--";
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

        public PlantUMLClassDiagramCSharpSyntaxWalker(
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
            string command,
            string typeName)
        {
            if (this.Diagram.Types.ContainsKey(typeName))
            {
                this.Diagram.Types.Where(w => w.Key == typeName).Select(w => w.Value).SingleOrDefault().Add(command);
            }
            else
            {
                this.Diagram.Types.Add(typeName, new List<string>());

                this.Diagram.Types.Where(w => w.Key == typeName).Select(w => w.Value).SingleOrDefault().Add(command);
            }
        }

        private void AddHeader(
            string title)
        {
            List<string> currentHeader = new List<string>();

            currentHeader.Add($"{PlantUML_title} {title}");

            this.Diagram.Header.AddRange(currentHeader);
        }

        private string BuildClassDeclarationCommand(
            string className,
            string joinedBaseListTypes,
            string joinedConstraintClauses,
            string joinedModifiers,
            string joinedTypeParameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(PlantUML_class);

            sb.Append(" ");

            sb.Append(className);

            if (joinedTypeParameters.Length > 0)
            {
                sb.Append($"<{joinedConstraintClauses}>");
            }

            sb.Append(" ");

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            if (joinedBaseListTypes.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedBaseListTypes}");

                sb.Append(" ");
            }

            sb.Append(PlantUML_leftBrace);

            return sb.ToString();
        }

        private string BuildConstructorDeclarationCommand(
            string constructorName,
            string joinedModifiers,
            string joinedParameters)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(constructorName);

            sb.Append(" ");

            sb.Append(PlantUML_leftParenthesis);

            sb.Append(joinedParameters);

            sb.Append(PlantUML_rightParenthesis);

            return sb.ToString();
        }

        private string BuildEventDeclarationCommand(
            string eventName,
            string eventTypeName,
            string explicitInterfaceSpecifierTypeName,
            string joinedAccessors,
            string joinedModifiers)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(stereotype_event);

            sb.Append(" ");

            sb.Append(eventTypeName);

            sb.Append(" ");

            if (explicitInterfaceSpecifierTypeName.Length > 0)
            {
                sb.Append($"{explicitInterfaceSpecifierTypeName}.");
            }

            sb.Append(eventName);

            sb.Append(" ");

            sb.Append(":");

            sb.Append(" ");

            if (joinedAccessors.Length > 0)
            {
                sb.Append(joinedAccessors);

                sb.Append(" ");
            }

            return sb.ToString();
        }

        private string BuildEventFieldDeclarationCommand(
            string eventFieldTypeName,
            string joinedModifiers,
            string joinedVariables)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(stereotype_event);

            sb.Append(" ");

            sb.Append(eventFieldTypeName);

            sb.Append(" ");

            if (joinedVariables.Length > 0)
            {
                sb.Append(joinedVariables);
            }

            return sb.ToString();   
        }

        private string BuildFieldDeclarationCommand(
            string fieldTypeName,
            string joinedModifiers,
            string joinedVariables)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(fieldTypeName);

            sb.Append(" ");

            if (joinedVariables.Length > 0)
            {
                sb.Append(joinedVariables);
            }

            return sb.ToString();
        }

        private string BuildInterfaceDeclarationCommand(
            string interfaceName,
            string joinedBaseListTypes,
            string joinedConstraintClauses,
            string joinedModifiers,
            string joinedTypeParameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(PlantUML_interface);

            sb.Append(" ");

            sb.Append(interfaceName);

            if (joinedTypeParameters.Length > 0)
            {
                sb.Append($"<{joinedConstraintClauses}>");
            }

            sb.Append(" ");

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            if (joinedBaseListTypes.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedBaseListTypes}");

                sb.Append(" ");
            }

            sb.Append(PlantUML_leftBrace);

            return sb.ToString();
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

                sb.Append(" ");
            }

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

        private string BuildPropertyDeclarationCommand(
            string explicitInterfaceSpecifierTypeName,
            string expression,
            string initializer,
            string joinedAccessors,
            string joinedModifiers,
            string propertyName,
            string propertyTypeName)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

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

            if (joinedAccessors.Length > 0)
            {
                sb.Append(joinedAccessors);

                sb.Append(" ");
            }

            if (expression.Length > 0)
            {
                sb.Append(expression);
            }

            if (initializer.Length > 0)
            {
                sb.Append(initializer);
            }

            return sb.ToString();
        }

        private string BuildStructDeclarationCommand(
            string joinedBaseListTypes,
            string joinedConstraintClauses,
            string joinedModifiers,
            string joinedTypeParameters,
            string structName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(stereotype_struct);

            sb.Append(" ");

            sb.Append(structName);

            if (joinedTypeParameters.Length > 0)
            {
                sb.Append($"<{joinedConstraintClauses}>");
            }

            sb.Append(" ");

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            if (joinedBaseListTypes.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedBaseListTypes}");

                sb.Append(" ");
            }

            sb.Append(PlantUML_leftBrace);

            return sb.ToString();
        }

        private string GetAccessor(
            AccessorDeclarationSyntax accessorDeclaration)
        {
            return accessorDeclaration.Keyword.ValueText switch
            {
                "get" => stereotype_get,

                "set" => stereotype_set,

                _ => throw new Exception(nameof(accessorDeclaration.Keyword))
            };
        }

        private List<string> GetAnchorRelationships(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> relationships = new List<string>();

            if (typeDeclaration.Ancestors().OfType<TypeDeclarationSyntax>().Count() > 0)
            {
                foreach (TypeDeclarationSyntax item in typeDeclaration.AncestorsAndSelf().OfType<TypeDeclarationSyntax>())
                {
                    TypeDeclarationSyntax parent = item.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();

                    if (item is not null && parent is not null)
                    {
                        relationships.Add($"{this.GetJoinedNamespaceTypeName(parent)} {PlantUML_anchor} {this.GetJoinedNamespaceTypeName(item)}");
                    }
                }
            }

            return relationships;
        }

        private string GetBaseListType(
            BaseTypeSyntax baseType)
        {
            return this.GetTypeNameOrFallback(
                baseType.Type.ToString(),
                baseType.Type,
                baseType.SyntaxTree);
        }

        private string GetConstraint<T>(
            TypeParameterConstraintSyntax typeParameterConstraint)
            where T : SyntaxNode
        {
            return typeParameterConstraint is TypeConstraintSyntax typeConstraint
                ? this.GetTypeNameOrFallback(
                    typeConstraint.Type.ToString(),
                    typeConstraint.Type,
                    typeConstraint.FirstAncestorOrSelf<T>().SyntaxTree)
                : typeParameterConstraint.ToString();
        }

        private List<string> GetConstraintClauses(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> constraintClauses = new List<string>();

            if (methodDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in methodDeclaration.ConstraintClauses.ToList())
                {
                    string constraintClauseName = constraintClause.Name.Identifier.ValueText;

                    string joinedConstraints = this.GetJoinedConstraints(
                        constraintClause,
                        methodDeclaration);

                    constraintClauses.Add($"{constraintClauseName} : {joinedConstraints}");
                }
            }

            return constraintClauses;
        }

        private List<string> GetConstraintClauses(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> constraintClauses = new List<string>();

            if (typeDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in typeDeclaration.ConstraintClauses.ToList())
                {
                    string constraintClauseName = constraintClause.Name.Identifier.ValueText;

                    string joinedConstraints = this.GetJoinedConstraints(
                        constraintClause,
                        typeDeclaration);

                    constraintClauses.Add($"{constraintClauseName} : {joinedConstraints}");
                }
            }

            return constraintClauses;
        }

        private List<string> GetConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                constraints.Add(
                    this.GetConstraint<MethodDeclarationSyntax>(
                        typeParameterConstraint));
            }

            return constraints;
        }

        private List<string> GetConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                constraints.Add(
                    this.GetConstraint<TypeDeclarationSyntax>(
                        typeParameterConstraint));
            }

            return constraints;
        }

        private string GetExplicitInterfaceSpecifierTypeName(
            BasePropertyDeclarationSyntax basePropertyDeclaration)
        {
            return basePropertyDeclaration.ExplicitInterfaceSpecifier is not null
                ? this.GetTypeNameOrFallback(
                    basePropertyDeclaration.ExplicitInterfaceSpecifier.Name.ToString(),
                    basePropertyDeclaration.ExplicitInterfaceSpecifier.Name,
                    basePropertyDeclaration.SyntaxTree)
                : String.Empty;
        }

        private string GetExplicitInterfaceSpecifierTypeName(
            MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.ExplicitInterfaceSpecifier is not null
                ? this.GetTypeNameOrFallback(
                    methodDeclaration.ExplicitInterfaceSpecifier.Name.ToString(),
                    methodDeclaration.ExplicitInterfaceSpecifier.Name,
                    methodDeclaration.SyntaxTree)
                : String.Empty;
        }

        private List<string> GetInheritanceRelationships(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> relationships = new List<string>();

            if (typeDeclaration.BaseList is not null)
            {
                foreach (BaseTypeSyntax item in typeDeclaration.BaseList.Types.ToList())
                {
                    string itemName = this.GetTypeNameOrFallback(
                        item.Type.ToString(),
                        item.Type,
                        typeDeclaration.SyntaxTree);

                    string typeName = this.GetJoinedNamespaceTypeName(
                        typeDeclaration);

                    relationships.Add($"{itemName} {PlantUML_extension} {typeName}");
                }
            }

            return relationships;
        }

        private string GetExpression(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.ExpressionBody is not null
                ? stereotype_arrowExpression
                : String.Empty;
        }

        private string GetInitializer(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.Initializer is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetInitializer(
            VariableDeclaratorSyntax variableDeclarator)
        {
            return variableDeclarator.Initializer is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetJoinedAccessors(
            BasePropertyDeclarationSyntax basePropertyDeclaration)
        {
            return basePropertyDeclaration.AccessorList is not null
                ? String.Join(
                    stringJoinSeparator_accessors,
                    basePropertyDeclaration.AccessorList?.Accessors.ToList().Select(w => this.GetAccessor(w)))
                : String.Empty;
        }

        private string GetJoinedBaseListTypes(
            TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_baseListTypes,
                    typeDeclaration.BaseList?.Types.ToList().Select(w => this.GetBaseListType(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> constraintClauses = this.GetConstraintClauses(
                methodDeclaration);

            return String.Join(
                ", ",
                constraintClauses);
        }

        private string GetJoinedConstraintClauses(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> constraintClauses = this.GetConstraintClauses(
                typeDeclaration);

            return String.Join(
                ", ",
                constraintClauses);
        }

        private string GetJoinedConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> constraints = this.GetConstraints(
                constraintClause,
                methodDeclaration);

            return String.Join(
                ", ",
                constraints.Select(w => w.ToString()));
        }

        private string GetJoinedConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> constraints = this.GetConstraints(
                constraintClause,
                typeDeclaration);

            return String.Join(
                ", ",
                constraints.Select(w => w.ToString()));
        }

        private string GetJoinedModifiers(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                classDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedModifiers(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                constructorDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedModifiers(
            EventDeclarationSyntax eventDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                eventDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        private string GetJoinedModifiers(
            EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                eventFieldDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
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
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                interfaceDeclaration);

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

        private string GetJoinedModifiers(
            StructDeclarationSyntax structDeclaration)
        {
            List<string> PlantUMLModifiers = this.GetModifiers(
                structDeclaration);

            return String.Join(
                " ",
                PlantUMLModifiers);
        }

        // TODO: If multiple types are defined in the same file, then it uses the name of the first one
        private string GetJoinedNamespaceTypeName(
            TypeDeclarationSyntax typeDeclaration)
        {
            string namespaceName = String.Empty;

            if (
                typeDeclaration.Ancestors().Count() > 0
                &&
                typeDeclaration.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>() is not null
                &&
                typeDeclaration.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().Count() > 0)
            {
                namespaceName = typeDeclaration.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().SingleOrDefault().Name.ToString();
            }

            string typeName = this.GetTypeDeclarationTypeName(
                typeDeclaration);

            return $"{namespaceName}.{typeName}";
        }

        private string GetJoinedParameters(
            BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            return baseMethodDeclaration.ParameterList.Parameters.Count > 0
                ? String.Join(
                    stringJoinSeparator_parameters,
                    baseMethodDeclaration.ParameterList.Parameters.Select(w => this.GetParameter(baseMethodDeclaration, w)).ToList())
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    methodDeclaration.TypeParameterList.Parameters.ToList().Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    typeDeclaration.TypeParameterList.Parameters.ToList().Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        // TODO: Account for ArgumentList
        private string GetJoinedVariables(
            BaseFieldDeclarationSyntax baseFieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_variableDeclarators,
                baseFieldDeclaration.Declaration.Variables.ToList().Select(w => this.GetVariable(w)));
        }

        private List<string> GetModifiers(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> PlantUMLModifiers = new List<string>();

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

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in constructorDeclaration.Modifiers.Select(w => w.ValueText))
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "internal" => stereotype_internal,

                    "protected" => stereotype_protected,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "static" => stereotype_static,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            EventDeclarationSyntax eventDeclaration)
        {
            List<string> CSharpModifiers = eventDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "internal" => stereotype_internal,

                    "new" => stereotype_new,

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

        private List<string> GetModifiers(
            EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            List<string> CSharpModifiers = eventFieldDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
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

        private List<string> GetModifiers(
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> CSharpModifiers = fieldDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "const" => stereotype_const,

                    "fixed" => stereotype_fixed,

                    "internal" => stereotype_internal,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "readonly" => stereotype_readonly,

                    "static" => modifier_static,
                    
                    "volatile" => stereotype_volatile,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in interfaceDeclaration.Modifiers.Select(w => w.ValueText).ToList())
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

                    "async" => stereotype_async,

                    "extern" => stereotype_extern,

                    "internal" => stereotype_internal,

                    "new" => stereotype_new,

                    "override" => stereotype_override,

                    "partial" => stereotype_partial,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "static" => modifier_static,

                    "unsafe" => stereotype_unsafe,

                    "virtual" => stereotype_virtual,

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

                    "new" => stereotype_new,

                    "override" => stereotype_override,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "static" => modifier_static,

                    "virtual" => stereotype_virtual,

                    _ => throw new Exception("")
                };

                PlantUMLModifiers.Add(PlantUMLModifier);
            }

            return PlantUMLModifiers;
        }

        private List<string> GetModifiers(
            StructDeclarationSyntax structDeclaration)
        {
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

            return PlantUMLModifiers;
        }

        private string GetParameter(
            BaseMethodDeclarationSyntax baseMethodDeclaration,
            ParameterSyntax parameter)
        {
            string parameterName = parameter.Identifier.ValueText;

            string parameterTypeName = this.GetTypeNameOrFallback(
                parameter.Type.ToString(),
                parameter.Type,
                baseMethodDeclaration.SyntaxTree);

            return $"{parameterTypeName} {parameterName}";
        }

        private SemanticModel GetSemanticModelOrDefault(
            SyntaxTree syntaxTree)
        {
            return this.solution.GetDocument(syntaxTree).GetSemanticModelAsync().Result;
        }

        private TypeDeclarationSyntax GetFirstTypeDeclarationOrDefault(
            SyntaxNode syntaxNode,
            SyntaxTree syntaxTree)
        {
            TypeDeclarationSyntax typeDeclaration = null;

            SemanticModel semanticModel = this.GetSemanticModelOrDefault(
                syntaxTree);

            if (semanticModel is not null)
            {
                if (ModelExtensions.GetTypeInfo(semanticModel, syntaxNode).Type is INamedTypeSymbol targetType)
                {
                    if (targetType.DeclaringSyntaxReferences.Length > 0)
                    {
                        if (targetType.DeclaringSyntaxReferences.First().GetSyntax() is TypeDeclarationSyntax typeDeclarationSyntax)
                        {
                            typeDeclaration = typeDeclarationSyntax;
                        }
                    }
                }
            }

            return typeDeclaration;
        }

        private string GetTypeDeclarationTypeName(
            TypeDeclarationSyntax typeDeclaration)
        {
            return String.Join(
                ".",
                typeDeclaration.Ancestors().OfType<TypeDeclarationSyntax>().Select(w => w.Identifier.ValueText))
                +
                typeDeclaration.Identifier.ValueText;
        }

        private string GetTypeNameOrFallback(
            string fallback,
            SyntaxNode syntaxNode,
            SyntaxTree syntaxTree)
        {
            string name = String.Empty;

            SemanticModel semanticModel = this.GetSemanticModelOrDefault(
                syntaxTree);

            if (semanticModel is not null)
            {
                if (ModelExtensions.GetTypeInfo(semanticModel, syntaxNode).Type is INamedTypeSymbol targetType)
                {
                    name = targetType.ToString();
                }
                else
                {
                    name = fallback;
                }
            }
            else
            {
                name = fallback;
            }

            return name;
        }

        private string GetVariable(
            VariableDeclaratorSyntax variableDeclarator)
        {
            string variableName = variableDeclarator.Identifier.ValueText;

            string initializer = this.GetInitializer(
                variableDeclarator);

            return $"{variableName} {initializer}";
        }

        private void StartDiagram(
            TypeDeclarationSyntax typeDeclaration)
        {
            this.currentTitle = this.GetJoinedNamespaceTypeName(
                typeDeclaration);

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

        /// <summary>
        /// This visits a node in the syntax tree.
        /// </summary>
        /// <param name="node">Node</param>
        public override void Visit(
            SyntaxNode node)
        {
            switch (node)
            {
                case BaseListSyntax baseList:
                    this.Visit(baseList);
                    break;
                case ClassDeclarationSyntax classDeclaration:
                    this.Visit(classDeclaration);
                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    this.Visit(constructorDeclaration);
                    break;
                case EventDeclarationSyntax eventDeclaration:
                    this.Visit(eventDeclaration);
                    break;
                case EventFieldDeclarationSyntax eventFieldDeclaration:
                    this.Visit(eventFieldDeclaration);
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

        private void Visit(
            BaseListSyntax baseList)
        {
            foreach (BaseTypeSyntax baseType in baseList.Types)
            {
                TypeDeclarationSyntax baseTypeDeclaration = this.GetFirstTypeDeclarationOrDefault(
                    baseType.Type,
                    baseType.SyntaxTree);

                if (baseTypeDeclaration is not null)
                {
                    this.Visit(
                        baseTypeDeclaration);
                }
            }

            base.Visit(
                baseList);
        }
        
        private void Visit(
            ClassDeclarationSyntax classDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (classDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    classDeclaration);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                classDeclaration);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildClassDeclarationCommand(
                className: this.GetJoinedNamespaceTypeName(
                    classDeclaration),
                joinedBaseListTypes: this.GetJoinedBaseListTypes(
                    classDeclaration),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    classDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    classDeclaration),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    classDeclaration));

            this.AddCommand(
                command: command,
                typeName: classDeclaration.Identifier.ValueText);

            base.Visit(
                classDeclaration);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: classDeclaration.Identifier.ValueText);
        }

        private void Visit(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            string command = this.BuildConstructorDeclarationCommand(
                constructorName: constructorDeclaration.Identifier.ValueText,
                joinedModifiers: this.GetJoinedModifiers(
                    constructorDeclaration),
                joinedParameters: this.GetJoinedParameters(
                    constructorDeclaration));

            this.AddCommand(
                command: command,
                typeName: constructorDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                constructorDeclaration);
        }

        private void Visit(
            EventDeclarationSyntax eventDeclaration)
        {
            string command = this.BuildEventDeclarationCommand(
                eventName: eventDeclaration.Identifier.ValueText,
                eventTypeName: this.GetTypeNameOrFallback(
                    eventDeclaration.Type.ToString(),
                    eventDeclaration.Type,
                    eventDeclaration.SyntaxTree),
                explicitInterfaceSpecifierTypeName: this.GetExplicitInterfaceSpecifierTypeName(
                    eventDeclaration),
                joinedAccessors: this.GetJoinedAccessors(
                    eventDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    eventDeclaration));

            this.AddCommand(
                command: command,
                typeName: eventDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                eventDeclaration);
        }

        private void Visit(
            EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            string command = this.BuildEventFieldDeclarationCommand(
                eventFieldTypeName: this.GetTypeNameOrFallback(
                    eventFieldDeclaration.Declaration.Type.ToString(),
                    eventFieldDeclaration.Declaration,
                    eventFieldDeclaration.SyntaxTree),
                joinedModifiers: this.GetJoinedModifiers(
                    eventFieldDeclaration),
                joinedVariables: this.GetJoinedVariables(
                    eventFieldDeclaration));

            this.AddCommand(
                command: command,
                typeName: eventFieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                eventFieldDeclaration);
        }

        private void Visit(
            FieldDeclarationSyntax fieldDeclaration)
        {
            string command = this.BuildFieldDeclarationCommand(
                fieldTypeName: this.GetTypeNameOrFallback(
                    fieldDeclaration.Declaration.Type.ToString(),
                    fieldDeclaration.Declaration,
                    fieldDeclaration.SyntaxTree),
                joinedModifiers: this.GetJoinedModifiers(
                    fieldDeclaration),
                joinedVariables: this.GetJoinedVariables(
                    fieldDeclaration));

            this.AddCommand(
                command: command,
                typeName: fieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                fieldDeclaration);
        }

        private void Visit(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (interfaceDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    interfaceDeclaration);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                interfaceDeclaration);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(anchorRelationships);
            }

            string command = this.BuildInterfaceDeclarationCommand(
                interfaceName: this.GetJoinedNamespaceTypeName(
                    interfaceDeclaration),
                joinedBaseListTypes: this.GetJoinedBaseListTypes(
                    interfaceDeclaration),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    interfaceDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    interfaceDeclaration),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    interfaceDeclaration));

            this.AddCommand(
                command: command,
                typeName: interfaceDeclaration.Identifier.ValueText);

            base.Visit(
                interfaceDeclaration);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: interfaceDeclaration.Identifier.ValueText);
        }

        private void Visit(
            MethodDeclarationSyntax methodDeclaration)
        {
            string command = this.BuildMethodDeclarationCommand(
                explicitInterfaceSpecifierTypeName: this.GetExplicitInterfaceSpecifierTypeName(
                    methodDeclaration),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    methodDeclaration),
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

            this.AddCommand(
                command: command,
                typeName: methodDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                methodDeclaration);
        }

        private void Visit(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            string command = this.BuildPropertyDeclarationCommand(
                explicitInterfaceSpecifierTypeName: this.GetExplicitInterfaceSpecifierTypeName(
                    propertyDeclaration),
                expression: this.GetExpression(
                    propertyDeclaration),
                initializer: this.GetInitializer(
                    propertyDeclaration),
                joinedAccessors: this.GetJoinedAccessors(
                    propertyDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    propertyDeclaration),
                propertyName: propertyDeclaration.Identifier.ValueText,
                propertyTypeName: this.GetTypeNameOrFallback(
                    propertyDeclaration.Type.ToString(),
                    propertyDeclaration.Type,
                    propertyDeclaration.SyntaxTree));

            this.AddCommand(
                command: command,
                typeName: propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);           

            base.Visit(
                propertyDeclaration);
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

            List<string> anchorRelationships = this.GetAnchorRelationships(
                structDeclaration);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(anchorRelationships);
            }

            string command = this.BuildStructDeclarationCommand(
                joinedBaseListTypes: this.GetJoinedBaseListTypes(
                    structDeclaration),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    structDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    structDeclaration),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    structDeclaration),
                structName: this.GetJoinedNamespaceTypeName(
                    structDeclaration));

            this.AddCommand(
                command: command,
                typeName: structDeclaration.Identifier.ValueText);

            base.Visit(
                structDeclaration);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: structDeclaration.Identifier.ValueText);
        }
    }
}