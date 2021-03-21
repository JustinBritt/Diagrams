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

        private const string PlantUML_abstract = "abstract";
        private const string PlantUML_anchor = "+--";
        private const string PlantUML_annotation = "annotation";
        private const string PlantUML_class = "class";
        private const string PlantUML_entity = "entity";
        private const string PlantUML_enum = "enum";
        private const string PlantUML_extends = "extends";
        private const string PlantUML_extension = "<|--";
        private const string PlantUML_implements = "implements";
        private const string PlantUML_interface = "interface";
        private const string PlantUML_leftBrace = "{";
        private const string PlantUML_leftParenthesis = "(";
        private const string PlantUML_packagePrivate = "~";
        private const string PlantUML_private = "-";
        private const string PlantUML_protected = "#";
        private const string PlantUML_public = "+";
        private const string PlantUML_rightBrace = "}";
        private const string PlantUML_rightParenthesis = ")";
        private const string PlantUML_title = "title";

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
        private const string stereotype_out = "<<out>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_params = "<<params>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_readonly = "<<readonly>>";
        private const string stereotype_record = "<<record>>";
        private const string stereotype_ref = "<<ref>>";
        private const string stereotype_remove = "<<remove>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_set = "<<set>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";
        private const string stereotype_this = "<<this>>";
        private const string stereotype_unsafe = "<<unsafe>>";
        private const string stereotype_virtual = "<<virtual>>";
        private const string stereotype_volatile = "<<volatile>>";

        private const string stringConcatSeparator_constraintClause = " : ";
        private const string stringConcatSeparator_namespaceTypeNames = ".";

        private const string stringJoinSeparator_accessors = " ";
        private const string stringJoinSeparator_constraintClauses = ", ";
        private const string stringJoinSeparator_constraints = ", ";
        private const string stringJoinSeparator_extends = ",";
        private const string stringJoinSeparator_implements = ",";
        private const string stringJoinSeparator_modifiers = " ";
        private const string stringJoinSeparator_parameters = ", ";
        private const string stringJoinSeparator_typeDeclarationTypeNames = ".";
        private const string stringJoinSeparator_typeParameters = ", ";
        private const string stringJoinSeparator_variableDeclarators = ", ";

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
            string joinedConstraintClauses,
            string joinedExtends,
            string joinedImplements,
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

            if (joinedExtends.Length > 0)
            {
                sb.Append($"{PlantUML_extends} {joinedExtends}");

                sb.Append(" ");
            }

            if (joinedImplements.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedImplements}");

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

            sb.Append(PlantUML_leftParenthesis);

            sb.Append(joinedParameters);

            sb.Append(PlantUML_rightParenthesis);

            return sb.ToString();
        }

        private string BuildEnumDeclarationCommand(
            string enumName,
            string joinedModifiers)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(PlantUML_enum);

            sb.Append(" ");

            sb.Append(enumName);

            sb.Append(" ");

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(PlantUML_leftBrace);

            return sb.ToString();
        }

        private string BuildEnumMemberDeclarationCommand(
            string enumMemberName,
            string equalsValue)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(enumMemberName);

            sb.Append(" ");

            if (equalsValue.Length > 0)
            {
                sb.Append(equalsValue);
            }

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

            if (joinedVariables.Length > 0)
            {
                sb.Append(" ");

                sb.Append(joinedVariables);
            }

            return sb.ToString();
        }

        private string BuildInterfaceDeclarationCommand(
            string interfaceName,
            string joinedConstraintClauses,
            string joinedExtends,
            string joinedImplements,
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

            if (joinedExtends.Length > 0)
            {
                sb.Append($"{PlantUML_extends} {joinedExtends}");

                sb.Append(" ");
            }

            if (joinedImplements.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedImplements}");

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

        private string BuildParameter(
            string defaultEquals,
            string joinedModifiers,
            string parameterName,
            string parameterTypeName)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            sb.Append(parameterTypeName);

            sb.Append(" ");

            sb.Append(parameterName);

            if (defaultEquals.Length > 0)
            {
                sb.Append(
                    defaultEquals);
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

            sb.Append(" : ");

            if (joinedAccessors.Length > 0)
            {
                sb.Append(joinedAccessors);
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

        // TODO: Finish
        private string BuildRecordDeclarationCommand(           
            string joinedConstraintClauses,
            string joinedExtends,
            string joinedImplements,
            string joinedModifiers,
            string joinedTypeParameters,
            string recordName)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        private string BuildStructDeclarationCommand(
            string joinedConstraintClauses,
            string joinedExtends,
            string joinedImplements,
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

            if (joinedExtends.Length > 0)
            {
                sb.Append($"{PlantUML_extends} {joinedExtends}");

                sb.Append(" ");
            }

            if (joinedImplements.Length > 0)
            {
                sb.Append($"{PlantUML_implements} {joinedImplements}");

                sb.Append(" ");
            }

            sb.Append(PlantUML_leftBrace);

            return sb.ToString();
        }

        private string GetAccessor(
            AccessorDeclarationSyntax accessorDeclaration)
        {
            return String.Concat(
                this.GetJoinedModifiers(accessorDeclaration),
                accessorDeclaration.Keyword.ValueText switch
            {
                "add" => stereotype_add,
                
                "get" => stereotype_get,
                
                "remove" => stereotype_remove,
                
                "set" => stereotype_set,

                _ => throw new Exception(nameof(accessorDeclaration.Keyword))
            });
        }

        private List<string> GetAnchorRelationships(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            List<string> relationships = new List<string>();

            if (baseTypeDeclaration.Ancestors().OfType<BaseTypeDeclarationSyntax>().Count() > 0)
            {
                foreach (BaseTypeDeclarationSyntax item in baseTypeDeclaration.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>())
                {
                    BaseTypeDeclarationSyntax parent = item.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();

                    if (item is not null && parent is not null)
                    {
                        relationships.Add($"{this.GetJoinedNamespaceTypeName(parent)} {PlantUML_anchor} {this.GetJoinedNamespaceTypeName(item)}");
                    }
                }
            }

            return relationships;
        }

        private string GetBaseTypeDeclarationTypeName(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            return String.Join(
                stringJoinSeparator_typeDeclarationTypeNames,
                baseTypeDeclaration.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(w => w.Identifier.ValueText));
        }

        private string GetConstraint<T>(
            TypeParameterConstraintSyntax typeParameterConstraint)
            where T : SyntaxNode
        {
            return typeParameterConstraint is TypeConstraintSyntax typeConstraint
                ? this.GetTypeName(
                    typeConstraint.Type,
                    typeConstraint.FirstAncestorOrSelf<T>().SyntaxTree)
                : typeParameterConstraint.ToString();
        }

        private string GetConstraintClause<T>(
            TypeParameterConstraintClauseSyntax constraintClause)
            where T : SyntaxNode
        {
            string constraintClauseName = constraintClause.Name.Identifier.ValueText;

            string joinedConstraints = String.Join(
                stringJoinSeparator_constraints,
                constraintClause.Constraints.Select(w => this.GetConstraint<T>(w)));

            return String.Concat(
                constraintClauseName,
                stringConcatSeparator_constraintClause,
                joinedConstraints);
        }

        private string GetDefault(
            ParameterSyntax parameter)
        {
            return parameter.Default is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetEqualsValue(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            return enumMemberDeclaration.EqualsValue is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetExplicitInterfaceSpecifierTypeName(
            BasePropertyDeclarationSyntax basePropertyDeclaration)
        {
            return basePropertyDeclaration.ExplicitInterfaceSpecifier is not null
                ? this.GetTypeName(
                    basePropertyDeclaration.ExplicitInterfaceSpecifier.Name,
                    basePropertyDeclaration.SyntaxTree)
                : String.Empty;
        }

        private string GetExplicitInterfaceSpecifierTypeName(
            MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.ExplicitInterfaceSpecifier is not null
                ? this.GetTypeName(
                    methodDeclaration.ExplicitInterfaceSpecifier.Name,
                    methodDeclaration.SyntaxTree)
                : String.Empty;
        }

        private string GetExpression(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.ExpressionBody is not null
                ? stereotype_arrowExpression
                : String.Empty;
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

        private List<string> GetInheritanceRelationships(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<string> relationships = new List<string>();

            if (typeDeclaration.BaseList is not null)
            {
                foreach (BaseTypeSyntax item in typeDeclaration.BaseList.Types)
                {
                    string itemName = this.GetTypeName(
                        item.Type,
                        typeDeclaration.SyntaxTree);

                    string typeName = this.GetJoinedNamespaceTypeName(
                        typeDeclaration);

                    relationships.Add($"{itemName} {PlantUML_extension} {typeName}");
                }
            }

            return relationships;
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
                    basePropertyDeclaration.AccessorList?.Accessors.Select(w => this.GetAccessor(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.ConstraintClauses.Count() > 0
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    methodDeclaration.ConstraintClauses.Select(w => this.GetConstraintClause<MethodDeclarationSyntax>(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.ConstraintClauses.Count() > 0
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    typeDeclaration.ConstraintClauses.Select(w => this.GetConstraintClause<TypeDeclarationSyntax>(w)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_extends,
                    classDeclaration.BaseList?.Types.Where(w => this.GetSemanticModelOrDefault(classDeclaration.SyntaxTree).GetTypeInfo(w.Type).Type.TypeKind is TypeKind.Class).Select(w => this.GetTypeName(w.Type, w.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return interfaceDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_extends,
                    interfaceDeclaration.BaseList?.Types.Where(w => this.GetSemanticModelOrDefault(interfaceDeclaration.SyntaxTree).GetTypeInfo(w.Type).Type.TypeKind is TypeKind.Class).Select(w => this.GetTypeName(w.Type, w.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            RecordDeclarationSyntax recordDeclaration)
        {
            return recordDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_extends,
                    recordDeclaration.BaseList?.Types.Where(w => this.GetSemanticModelOrDefault(recordDeclaration.SyntaxTree).GetTypeInfo(w.Type).Type.IsRecord).Select(w => this.GetTypeName(w.Type, w.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            StructDeclarationSyntax structDeclaration)
        {
            return structDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_extends,
                    structDeclaration.BaseList?.Types.Where(w => this.GetSemanticModelOrDefault(structDeclaration.SyntaxTree).GetTypeInfo(w.Type).Type.TypeKind is TypeKind.Class).Select(w => this.GetTypeName(w.Type, w.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedImplements(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            return baseTypeDeclaration.BaseList is not null
                ? String.Join(
                    stringJoinSeparator_implements,
                    baseTypeDeclaration.BaseList?.Types.Where(w => this.GetSemanticModelOrDefault(baseTypeDeclaration.SyntaxTree).GetTypeInfo(w.Type).Type.TypeKind is TypeKind.Interface).Select(w => this.GetTypeName(w.Type, w.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedModifiers(
            AccessorDeclarationSyntax accessorDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                accessorDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            ClassDeclarationSyntax classDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                classDeclaration.Modifiers
                .Select(w => w.ValueText switch
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
                }));
        }

        private string GetJoinedModifiers(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                constructorDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "protected" => stereotype_protected,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "static" => stereotype_static,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            EnumDeclarationSyntax enumDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                enumDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "new" => stereotype_new,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            EventDeclarationSyntax eventDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                eventDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "new" => stereotype_new,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "static" => stereotype_static,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                eventFieldDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => PlantUML_packagePrivate,

                    "private" => PlantUML_private,

                    "protected" => PlantUML_protected,

                    "public" => PlantUML_public,

                    "static" => modifier_static,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            FieldDeclarationSyntax fieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                fieldDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "const" => stereotype_const,

                    "fixed" => stereotype_fixed,

                    "internal" => PlantUML_packagePrivate,

                    "private" => PlantUML_private,

                    "protected" => PlantUML_protected,

                    "public" => PlantUML_public,

                    "readonly" => stereotype_readonly,

                    "static" => modifier_static,

                    "volatile" => stereotype_volatile,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                interfaceDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            MethodDeclarationSyntax methodDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                methodDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "abstract" => modifier_abstract,

                    "async" => stereotype_async,

                    "extern" => stereotype_extern,

                    "internal" => PlantUML_packagePrivate,

                    "new" => stereotype_new,

                    "override" => stereotype_override,

                    "partial" => stereotype_partial,

                    "private" => PlantUML_private,

                    "protected" => PlantUML_protected,

                    "public" => PlantUML_public,

                    "static" => modifier_static,

                    "unsafe" => stereotype_unsafe,

                    "virtual" => stereotype_virtual,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            ParameterSyntax parameter)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                parameter.Modifiers
                .Select(w => w.ValueText switch
                {
                    "out" => stereotype_out,

                    "params" => stereotype_params,

                    "ref" => stereotype_ref,

                    "this" => stereotype_this,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            PropertyDeclarationSyntax propertyDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                propertyDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "abstract" => stereotype_abstract,

                    "internal" => stereotype_internal,

                    "new" => stereotype_new,

                    "override" => stereotype_override,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "static" => stereotype_static,

                    "virtual" => stereotype_virtual,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            RecordDeclarationSyntax recordDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                recordDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "abstract" => stereotype_abstract,

                    "internal" => stereotype_internal,

                    "partial" => stereotype_partial,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "sealed" => stereotype_sealed,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            StructDeclarationSyntax structDeclaration)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                structDeclaration.Modifiers
                .Select(w => w.ValueText switch
                {
                    "internal" => stereotype_internal,

                    "partial" => stereotype_partial,

                    "private" => stereotype_private,

                    "protected" => stereotype_protected,

                    "public" => stereotype_public,

                    "unsafe" => stereotype_unsafe,

                    _ => throw new Exception("")
                }));
        }

        // TODO: If multiple types are defined in the same file, then it uses the name of the first one
        private string GetJoinedNamespaceTypeName(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            string namespaceName = String.Empty;

            if (baseTypeDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>() is not null)
            {
                namespaceName = baseTypeDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();
            }

            string typeName = this.GetBaseTypeDeclarationTypeName(
                baseTypeDeclaration);

            return String.Concat(
                namespaceName,
                stringConcatSeparator_namespaceTypeNames,
                typeName);
        }

        private string GetJoinedParameters(
            BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            return baseMethodDeclaration.ParameterList.Parameters.Count > 0
                ? String.Join(
                    stringJoinSeparator_parameters,
                    baseMethodDeclaration.ParameterList.Parameters.Select(w => this.GetParameter(baseMethodDeclaration, w)))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    methodDeclaration.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    typeDeclaration.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        // TODO: Account for ArgumentList
        private string GetJoinedVariables(
            BaseFieldDeclarationSyntax baseFieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_variableDeclarators,
                baseFieldDeclaration.Declaration.Variables.Select(w => this.GetVariable(w)));
        }

        private string GetParameter(
            BaseMethodDeclarationSyntax baseMethodDeclaration,
            ParameterSyntax parameter)
        {
            return this.BuildParameter(
                defaultEquals: this.GetDefault(
                    parameter),
                joinedModifiers: this.GetJoinedModifiers(
                    parameter),
                parameterName: parameter.Identifier.ValueText,
                parameterTypeName: this.GetTypeName(
                    parameter.Type,
                    baseMethodDeclaration.SyntaxTree));
        }

        private SemanticModel GetSemanticModelOrDefault(
            SyntaxTree syntaxTree)
        {
            return this.solution.GetDocument(syntaxTree).GetSemanticModelAsync().Result;
        }

        private string GetTypeName(
            SyntaxNode syntaxNode,
            SyntaxTree syntaxTree)
        {
            return this.GetSemanticModelOrDefault(syntaxTree) is not null && ModelExtensions.GetTypeInfo(this.GetSemanticModelOrDefault(syntaxTree), syntaxNode).Type is INamedTypeSymbol targetType
                ? targetType.ToString()
                : syntaxNode.ToString();
        }

        private string GetVariable(
            VariableDeclaratorSyntax variableDeclarator)
        {
            string variableName = variableDeclarator.Identifier.ValueText;

            string initializer = this.GetInitializer(
                variableDeclarator);

            return initializer.Length > 0
                ? $"{variableName} {initializer}"
                : $"{variableName}";
        }

        private void StartDiagram(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            this.currentTitle = this.GetJoinedNamespaceTypeName(
                baseTypeDeclaration);

            if (!String.IsNullOrEmpty(this.currentTitle))
            {
                if (!this.Diagrams.ContainsTitle(this.currentTitle))
                {
                    this.Diagrams.AddTitle(this.currentTitle);

                    this.AddHeader(
                        this.currentTitle);
                }
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
                case EnumDeclarationSyntax enumDeclaration:
                    this.Visit(enumDeclaration);
                    break;
                case EnumMemberDeclarationSyntax enumMemberDeclaration:
                    this.Visit(enumMemberDeclaration);
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
                case RecordDeclarationSyntax recordDeclaration:
                    this.Visit(recordDeclaration);
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
            List<BaseTypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<BaseTypeDeclarationSyntax>().ToList();

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
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    classDeclaration),
                joinedExtends: this.GetJoinedExtends(
                    classDeclaration),
                joinedImplements: this.GetJoinedImplements(
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
            EnumDeclarationSyntax enumDeclaration)
        {
            List<BaseTypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<BaseTypeDeclarationSyntax>().ToList();

            if (enumDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    enumDeclaration);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                enumDeclaration);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildEnumDeclarationCommand(
                enumName: this.GetJoinedNamespaceTypeName(
                    enumDeclaration),
                joinedModifiers: this.GetJoinedModifiers(
                    enumDeclaration));

            this.AddCommand(
                command: command,
                typeName: enumDeclaration.Identifier.ValueText);

            base.Visit(
                enumDeclaration);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: enumDeclaration.Identifier.ValueText);
        }

        private void Visit(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            string command = this.BuildEnumMemberDeclarationCommand(
                enumMemberName: enumMemberDeclaration.Identifier.ValueText,
                equalsValue: this.GetEqualsValue(
                    enumMemberDeclaration));

            this.AddCommand(
                command: command,
                typeName: enumMemberDeclaration.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>().Identifier.ValueText);

            base.Visit(
                enumMemberDeclaration);
        }

        private void Visit(
            EventDeclarationSyntax eventDeclaration)
        {
            string command = this.BuildEventDeclarationCommand(
                eventName: eventDeclaration.Identifier.ValueText,
                eventTypeName: this.GetTypeName(
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
                eventFieldTypeName: this.GetTypeName(
                    eventFieldDeclaration.Declaration.Type,
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
                fieldTypeName: this.GetTypeName(
                    fieldDeclaration.Declaration.Type,
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
            List<BaseTypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<BaseTypeDeclarationSyntax>().ToList();

            if (interfaceDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    interfaceDeclaration);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                interfaceDeclaration);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildInterfaceDeclarationCommand(
                interfaceName: this.GetJoinedNamespaceTypeName(
                    interfaceDeclaration),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    interfaceDeclaration),
                joinedExtends: this.GetJoinedExtends(
                    interfaceDeclaration),
                joinedImplements: this.GetJoinedImplements(
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
                returnTypeName: this.GetTypeName(
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
                propertyTypeName: this.GetTypeName(
                    propertyDeclaration.Type,
                    propertyDeclaration.SyntaxTree));

            this.AddCommand(
                command: command,
                typeName: propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);           

            base.Visit(
                propertyDeclaration);
        }

        // TODO: Finish
        private void Visit(
            RecordDeclarationSyntax recordDeclaration)
        {
            string joinedConstraintClauses = this.GetJoinedConstraintClauses(
                recordDeclaration);

            string joinedExtends = this.GetJoinedExtends(
                recordDeclaration);

            string joinedImplements = this.GetJoinedImplements(
                recordDeclaration);

            string joinedModifiers = this.GetJoinedModifiers(
                recordDeclaration);

            string joinedTypeParameters = this.GetJoinedTypeParameters(
                recordDeclaration);

            string recordName = this.GetJoinedNamespaceTypeName(
                    recordDeclaration);

            base.Visit(
                recordDeclaration);
        }

        private void Visit(
            StructDeclarationSyntax structDeclaration)
        {
            List<BaseTypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<BaseTypeDeclarationSyntax>().ToList();

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
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    structDeclaration),
                joinedExtends: this.GetJoinedExtends(
                    structDeclaration),
                joinedImplements: this.GetJoinedImplements(
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