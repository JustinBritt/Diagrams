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
        private const string modifier_static = "{static}";

        private const string PlantUML_anchor = "+--";
        private const string PlantUML_class = "class";
        private const string PlantUML_enum = "enum";
        private const string PlantUML_extends = "extends";
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

        private const string stereotype_add = "<<add>>";
        private const string stereotype_async = "<<async>>";
        private const string stereotype_const = "<<const>>";
        private const string stereotype_equals = "<<=>>";
        private const string stereotype_event = "<<event>>";
        private const string stereotype_get = "<<get>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_optional = "<<optional>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_params = "<<params>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_raise = "<<raise>>";
        private const string stereotype_readonly = "<<readonly>>";
        private const string stereotype_ref = "<<ref>>";
        private const string stereotype_remove = "<<remove>>";
        private const string stereotype_sealed = "<<sealed>>";
        private const string stereotype_set = "<<set>>";
        private const string stereotype_static = "<<static>>";
        private const string stereotype_struct = "<<struct>>";

        private const string stringConcatSeparator_namespaceTypeNames = ".";

        private const string stringJoinSeparator_accessors = " ";
        private const string stringJoinSeparator_constraintClauses = ", ";
        private const string stringJoinSeparator_declarationStatementTypeNames = ".";
        private const string stringJoinSeparator_extends = ",";
        private const string stringJoinSeparator_implements = ",";
        private const string stringJoinSeparator_modifiers = " ";
        private const string stringJoinSeparator_parameters = ", ";
        private const string stringJoinSeparator_typeParameters = ", ";
        private const string stringJoinSeparator_variableDeclarators = ", ";

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

        private void AddCommand(
            string command,
            string typeName)
        {
            if (this.Diagram is not null && this.Diagram.Types is not null)
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
        }

        private void AddHeader(
            string title)
        {
            List<string> currentHeader = new List<string>();

            currentHeader.Add($"{PlantUML_title} {title}");

            this.Diagram.Header.AddRange(currentHeader);
        }

        private string BuildClassBlockCommand(
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

        private string BuildConstructorBlockCommand(
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

        private string BuildEnumBlockCommand(
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
            string initializer)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(enumMemberName);

            sb.Append(" ");

            if (initializer.Length > 0)
            {
                sb.Append(initializer);
            }

            return sb.ToString();
        }

        private string BuildEventBlockCommand(
            string eventName,
            string eventTypeName,
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

        private string BuildFieldDeclarationCommand(
            string joinedModifiers,
            string joinedVariables)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);

                sb.Append(" ");
            }

            if (joinedVariables.Length > 0)
            {
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

        private string BuildMethodBlockCommand(
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

        private string BuildPropertyBlockCommand(
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

            sb.Append(propertyName);

            sb.Append(" : ");

            if (joinedAccessors.Length > 0)
            {
                sb.Append(joinedAccessors);
            }

            if (initializer.Length > 0)
            {
                sb.Append(initializer);
            }

            return sb.ToString();
        }

        private string BuildStructureBlockCommand(
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

        // TODO: Update
        private List<string> GetAnchorRelationships(
            DeclarationStatementSyntax declarationStatement)
        {
            List<string> relationships = new List<string>();

            if (declarationStatement.Ancestors().OfType<DeclarationStatementSyntax>().Count() > 0)
            {
                foreach (DeclarationStatementSyntax item in declarationStatement.AncestorsAndSelf().OfType<DeclarationStatementSyntax>())
                {
                    DeclarationStatementSyntax parent = item.Ancestors().OfType<DeclarationStatementSyntax>().FirstOrDefault();

                    if (item is not null && parent is not null)
                    {
                        relationships.Add($"{this.GetJoinedNamespaceTypeName(parent)} {PlantUML_anchor} {this.GetJoinedNamespaceTypeName(item)}");
                    }
                }
            }

            return relationships;
        }

        // TODO: Use pattern matching?
        private string GetAsClauseTypeName(
            AsClauseSyntax asClause)
        {
            string typeName = String.Empty;

            if(asClause.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault() is not null)
            {
                typeName = this.GetTypeName(
                    syntaxNode: asClause.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault(),
                    syntaxTree: asClause.SyntaxTree);
            }
            else if (asClause.ChildNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault() is not null)
            {
                typeName = this.GetTypeName(
                    syntaxNode: asClause.ChildNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault(),
                    syntaxTree: asClause.SyntaxTree);
            }
            else if (asClause.ChildNodes().OfType<PredefinedTypeSyntax>().FirstOrDefault() is not null)
            {
                typeName = this.GetTypeName(
                    syntaxNode: asClause.ChildNodes().OfType<PredefinedTypeSyntax>().FirstOrDefault(),
                    syntaxTree: asClause.SyntaxTree);
            }
            else if (asClause.ChildNodes().OfType<QualifiedNameSyntax>().FirstOrDefault() is not null)
            {
                typeName = this.GetTypeName(
                    syntaxNode: asClause.ChildNodes().OfType<QualifiedNameSyntax>().FirstOrDefault(),
                    syntaxTree: asClause.SyntaxTree);
            }
            else
            {
                throw new Exception("");
            }

            return typeName;
        }

        private string GetConstraint(
            ConstraintSyntax constraint)
        {
            return constraint.Kind() switch
            {
                SyntaxKind.ClassConstraint => "Class",

                SyntaxKind.NewConstraint => "New",

                SyntaxKind.StructureConstraint => "Structure",

                SyntaxKind.TypeConstraint => this.GetTypeName(
                    ((TypeConstraintSyntax)constraint).Type,
                    constraint.SyntaxTree),

                _ => throw new Exception("")
            };
        }

        private List<string> GetConstraintClauses(
            TypeParameterConstraintClauseSyntax constraintClause)
        {
            List<string> constraintClauses = new List<string>();

            if (constraintClause is TypeParameterSingleConstraintClauseSyntax single)
            {
                constraintClauses.Add(
                    this.GetConstraint(
                        single.Constraint));
            }
            else if (constraintClause is TypeParameterMultipleConstraintClauseSyntax multiple)
            {
                constraintClauses.AddRange(
                    multiple.Constraints.Select(w => this.GetConstraint(w)));
            }

            return constraintClauses;
        }

        // TODO: Finish
        private string GetDeclarationStatementIdentifier(
            DeclarationStatementSyntax declarationStatement)
        {
            return declarationStatement switch
            {
                ClassBlockSyntax classBlock => classBlock.ClassStatement.Identifier.ValueText,

                InterfaceBlockSyntax interfaceBlock => interfaceBlock.InterfaceStatement.Identifier.ValueText,

                NamespaceBlockSyntax namespaceBlock => String.Empty,

                StructureBlockSyntax structureBlock => structureBlock.StructureStatement.Identifier.ValueText,

                _ => throw new Exception("")
            };
        }

        private string GetDeclarationStatementTypeName(
            DeclarationStatementSyntax declarationStatement)
        {
            return String.Join(
                stringJoinSeparator_declarationStatementTypeNames,
                declarationStatement.AncestorsAndSelf().OfType<DeclarationStatementSyntax>().Reverse().Select(w => this.GetDeclarationStatementIdentifier(w)));
        }

        private string GetDefault(
            ParameterSyntax parameter)
        {
            return parameter.Default is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetEventTypeName(
            EventBlockSyntax eventBlock)
        {
            return eventBlock.EventStatement.AsClause is not null
                ? this.GetAsClauseTypeName(
                    eventBlock.EventStatement.AsClause)
                : this.GetTypeName(
                    syntaxNode: eventBlock.EventStatement,
                    syntaxTree: eventBlock.SyntaxTree);
        }

        // TODO: Finish
        private string GetFirstTypeAncestorTypeName(
            DeclarationStatementSyntax declarationStatement)
        {
            string typeName = String.Empty;

            if (declarationStatement.FirstAncestorOrSelf<ClassBlockSyntax>() is not null)
            {
                typeName = declarationStatement.FirstAncestorOrSelf<ClassBlockSyntax>().ClassStatement.Identifier.ValueText;
            }
            else if (declarationStatement.FirstAncestorOrSelf<ModuleBlockSyntax>() is not null)
            {
                typeName = declarationStatement.FirstAncestorOrSelf<ModuleBlockSyntax>().ModuleStatement.Identifier.ValueText;
            }
            else if (declarationStatement.FirstAncestorOrSelf<StructureBlockSyntax>() is not null)
            {
                typeName = declarationStatement.FirstAncestorOrSelf<StructureBlockSyntax>().StructureStatement.Identifier.ValueText;
            }
            else
            {
                throw new Exception("");
            }

            return typeName;
        }

        private string GetInitializer(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            return enumMemberDeclaration.Initializer is not null
                ? stereotype_equals
                : String.Empty;
        }

        private string GetInitializer(
            PropertyBlockSyntax propertyBlock)
        {
            return propertyBlock.PropertyStatement.Initializer is not null
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
            EventBlockSyntax eventBlock)
        {
            return eventBlock.Accessors.Count() > 0
                ? String.Join(
                    stringJoinSeparator_accessors,
                    eventBlock.Accessors.Select(w => this.GetAccessor(w.AccessorStatement)))
                : String.Empty;
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

        private string GetJoinedConstraintClauses(
            ClassBlockSyntax classBlock)
        {
            return classBlock.ClassStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    classBlock.ClassStatement.TypeParameterList.Parameters.Select(w => w.TypeParameterConstraintClause).SelectMany(w => this.GetConstraintClauses(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            InterfaceBlockSyntax interfaceBlock)
        {
            return interfaceBlock.InterfaceStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    interfaceBlock.InterfaceStatement.TypeParameterList.Parameters.Select(w => w.TypeParameterConstraintClause).SelectMany(w => this.GetConstraintClauses(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            MethodBlockSyntax methodBlock)
        {
            return methodBlock.SubOrFunctionStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    methodBlock.SubOrFunctionStatement.TypeParameterList.Parameters.Select(w => w.TypeParameterConstraintClause).SelectMany(w => this.GetConstraintClauses(w)))
                : String.Empty;
        }

        private string GetJoinedConstraintClauses(
            StructureBlockSyntax structureBlock)
        {
            return structureBlock.StructureStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_constraintClauses,
                    structureBlock.StructureStatement.TypeParameterList.Parameters.Select(w => w.TypeParameterConstraintClause).SelectMany(w => this.GetConstraintClauses(w)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            ClassBlockSyntax classBlock)
        {
            return classBlock.Inherits.Count() > 0
                ? String.Join(
                    stringJoinSeparator_extends,
                    classBlock.Inherits.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, classBlock.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            InterfaceBlockSyntax interfaceBlock)
        {
            return interfaceBlock.Inherits.Count() > 0
                ? String.Join(
                    stringJoinSeparator_extends,
                    interfaceBlock.Inherits.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, interfaceBlock.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedExtends(
            StructureBlockSyntax structureBlock)
        {
            return structureBlock.Inherits.Count() > 0
                ? String.Join(
                    stringJoinSeparator_extends,
                    structureBlock.Inherits.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, structureBlock.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedImplements(
            ClassBlockSyntax classBlock)
        {
            return classBlock.Implements.Count() > 0
                ? String.Join(
                    stringJoinSeparator_implements,
                    classBlock.Implements.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, classBlock.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedImplements(
            InterfaceBlockSyntax interfaceBlock)
        {
            return interfaceBlock.Implements.Count() > 0
                ? String.Join(
                    stringJoinSeparator_implements,
                    interfaceBlock.Implements.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, interfaceBlock.SyntaxTree)))
                : String.Empty;
        }

        private string GetJoinedImplements(
            StructureBlockSyntax structureBlock)
        {
            return structureBlock.Implements.Count() > 0
                ? String.Join(
                    stringJoinSeparator_implements,
                    structureBlock.Implements.SelectMany(w => w.Types).Select(w => this.GetTypeName(w, structureBlock.SyntaxTree)))
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

        private string GetJoinedModifiers(
            EventStatementSyntax eventStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                eventStatement.Modifiers
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
        // TODO: Should WithEvents be mapped to an empty string?
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

                    "Friend" => PlantUML_packagePrivate,

                    "Private" => PlantUML_private,

                    "Protected" => PlantUML_protected,

                    "Public" => PlantUML_public,

                    "Shared" => modifier_static,

                    "WithEvents" => String.Empty,

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

        // TODO: Should Overrides map to stereotype_override?
        private string GetJoinedModifiers(
            MethodStatementSyntax methodStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                methodStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Async" => stereotype_async,

                    "Overrides" => stereotype_override,

                    "Friend" => PlantUML_packagePrivate,

                    "Private" => PlantUML_private,

                    "Protected" => PlantUML_protected,

                    "Public" => PlantUML_public,

                    "Shared" => modifier_static,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            ModuleStatementSyntax moduleStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                moduleStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "Public" => stereotype_public,

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
                    "ByRef" => stereotype_ref,

                    "ByVal" => String.Empty,

                    "Optional" => stereotype_optional,

                    "ParamArray" => stereotype_params,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            PropertyStatementSyntax propertyStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                propertyStatement.Modifiers
                .Select(w => w.ValueText switch
                {
                    "Friend" => stereotype_internal,

                    "Private" => stereotype_private,

                    "Protected" => stereotype_protected,

                    "Public" => stereotype_public,

                    "ReadOnly" => stereotype_readonly,

                    "Shared" => stereotype_static,

                    _ => throw new Exception("")
                }));
        }

        private string GetJoinedModifiers(
            StructureStatementSyntax structureStatement)
        {
            return String.Join(
                stringJoinSeparator_modifiers,
                structureStatement.Modifiers
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

        private string GetJoinedNamespaceTypeName(
            DeclarationStatementSyntax declarationStatement)
        {
            string namespaceName = String.Empty;

            if (declarationStatement.FirstAncestorOrSelf<NamespaceBlockSyntax>() is not null)
            {
                namespaceName = declarationStatement.FirstAncestorOrSelf<NamespaceBlockSyntax>().NamespaceStatement.Name.ToString();
            }
            else
            {
                namespaceName = project.Name;
            }

            string typeName = this.GetDeclarationStatementTypeName(
                declarationStatement);

            return String.Concat(
                namespaceName,
                stringConcatSeparator_namespaceTypeNames,
                typeName);
        }

        private string GetJoinedParameters(
            ConstructorBlockSyntax constructorBlock)
        {
            return constructorBlock.SubNewStatement.ParameterList.Parameters.Count > 0
                ? String.Join(
                    stringJoinSeparator_parameters,
                    constructorBlock.SubNewStatement.ParameterList.Parameters.Select(w => this.GetParameter(constructorBlock, w)))
                : String.Empty;
        }

        private string GetJoinedParameters(
            MethodBlockSyntax methodBlock)
        {
            return methodBlock.SubOrFunctionStatement.ParameterList.Parameters.Count > 0
                ? String.Join(
                    stringJoinSeparator_parameters,
                    methodBlock.SubOrFunctionStatement.ParameterList.Parameters.Select(w => this.GetParameter(methodBlock, w)))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            ClassStatementSyntax classStatement)
        {
            return classStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    classStatement.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            InterfaceStatementSyntax interfaceStatement)
        {
            return interfaceStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    interfaceStatement.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            MethodStatementSyntax methodStatement)
        {
            return methodStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    methodStatement.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedTypeParameters(
            StructureStatementSyntax structureStatement)
        {
            return structureStatement.TypeParameterList is not null
                ? String.Join(
                    stringJoinSeparator_typeParameters,
                    structureStatement.TypeParameterList.Parameters.Select(w => w.Identifier.ValueText))
                : String.Empty;
        }

        private string GetJoinedVariables(
            FieldDeclarationSyntax fieldDeclaration)
        {
            return String.Join(
                stringJoinSeparator_variableDeclarators,
                fieldDeclaration.Declarators.Select(w => this.GetVariable(w)));
        }

        private string GetParameter(
            ConstructorBlockSyntax constructorBlock,
            ParameterSyntax parameter)
        {
            return this.BuildParameter(
                defaultEquals: this.GetDefault(
                    parameter),
                joinedModifiers: this.GetJoinedModifiers(
                    parameter),
                parameterName: parameter.Identifier.Identifier.ValueText,
                parameterTypeName: this.GetParameterTypeName(
                    parameter));
        }

        private string GetParameter(
            MethodBlockSyntax methodBlock,
            ParameterSyntax parameter)
        {
            return this.BuildParameter(
                defaultEquals: this.GetDefault(
                    parameter),
                joinedModifiers: this.GetJoinedModifiers(
                    parameter),
                parameterName: parameter.Identifier.Identifier.ValueText,
                parameterTypeName: this.GetParameterTypeName(
                    parameter));
        }

        private string GetParameterTypeName(
            ParameterSyntax parameter)
        {
            return parameter.AsClause is not null
                ? this.GetTypeName(
                    syntaxNode: parameter.AsClause.Type,
                    syntaxTree: parameter.SyntaxTree)
                : this.GetTypeName(
                    syntaxNode: parameter,
                    syntaxTree: parameter.SyntaxTree);
        }

        private string GetPropertyTypeName(
            PropertyBlockSyntax propertyBlock)
        {
            return propertyBlock.PropertyStatement.AsClause is not null
                ? this.GetAsClauseTypeName(
                    propertyBlock.PropertyStatement.AsClause)
                : this.GetTypeName(
                    syntaxNode: propertyBlock.PropertyStatement,
                    syntaxTree: propertyBlock.SyntaxTree);
        }

        // TODO: Add const for void
        private string GetReturnTypeName(
            MethodBlockSyntax methodBlock)
        {
            return methodBlock.SubOrFunctionStatement.AsClause is not null
                ? this.GetAsClauseTypeName(
                    methodBlock.SubOrFunctionStatement.AsClause)
                : "void";
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
            string variableTypeName = this.GetVariableTypeName(
                variableDeclarator);   

            string variableNames = String.Join(
                stringJoinSeparator_variableDeclarators,
                variableDeclarator.Names.Select(w => w.Identifier.ValueText));

            string initializer = this.GetInitializer(
                variableDeclarator);

            return initializer.Length > 0
                ? $"{variableTypeName} {variableNames} {initializer}"
                : $"{variableTypeName} {variableNames}";
        }

        // TODO: Check
        private string GetVariableTypeName(
            VariableDeclaratorSyntax variableDeclarator)
        {
            return variableDeclarator.AsClause is not null
                ? this.GetAsClauseTypeName(
                    variableDeclarator.AsClause)
                : this.GetTypeName(
                    syntaxNode: variableDeclarator,
                    syntaxTree: variableDeclarator.SyntaxTree);
        }

        // TODO: Update types
        private bool ShouldDiagramBeStarted(
            DeclarationStatementSyntax declarationStatement)
        {
            bool result = false;

            List<DeclarationStatementSyntax> declaredTypes = new List<DeclarationStatementSyntax>();
            declaredTypes.AddRange(this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<ClassBlockSyntax>());
            declaredTypes.AddRange(this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<EnumBlockSyntax>());
            declaredTypes.AddRange(this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<InterfaceBlockSyntax>());
            declaredTypes.AddRange(this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<ModuleBlockSyntax>());
            declaredTypes.AddRange(this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<StructureBlockSyntax>());

            if (declarationStatement == declaredTypes.First())
            {
                result = true;
            }

            return result;
        }

        private void StartDiagram(
            DeclarationStatementSyntax declarationStatement)
        {
            this.currentTitle = this.GetJoinedNamespaceTypeName(
                declarationStatement);

            if (!String.IsNullOrEmpty(this.currentTitle))
            {
                if (!this.Diagrams.ContainsTitle(this.currentTitle))
                {
                    this.Diagrams.AddTitle(
                        this.currentTitle);

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
                case MethodBlockSyntax methodBlock:
                    this.Visit(methodBlock);
                    break;
                case ModuleBlockSyntax moduleBlock:
                    this.Visit(moduleBlock);
                    break;
                case PropertyBlockSyntax propertyBlock:
                    this.Visit(propertyBlock);
                    break;
                case StructureBlockSyntax structureBlock:
                    this.Visit(structureBlock);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        private void Visit(
            ClassBlockSyntax classBlock)
        {
            if (this.ShouldDiagramBeStarted(classBlock))
            {
                this.StartDiagram(
                    classBlock);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                classBlock);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildClassBlockCommand(
                className: this.GetJoinedNamespaceTypeName(
                    classBlock),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    classBlock),
                joinedExtends: this.GetJoinedExtends(
                    classBlock),
                joinedImplements: this.GetJoinedImplements(
                    classBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    classBlock.ClassStatement),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    classBlock.ClassStatement));

            this.AddCommand(
                command: command,
                typeName: classBlock.ClassStatement.Identifier.ValueText);

            base.Visit(
                classBlock);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: classBlock.ClassStatement.Identifier.ValueText);
        }

        private void Visit(
            ConstructorBlockSyntax constructorBlock)
        {
            string command = this.BuildConstructorBlockCommand(
                constructorName: this.GetFirstTypeAncestorTypeName(
                    constructorBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    constructorBlock.SubNewStatement),
                joinedParameters: this.GetJoinedParameters(
                    constructorBlock));

            this.AddCommand(
                command: command,
                typeName: this.GetFirstTypeAncestorTypeName(
                    constructorBlock));

            base.Visit(
                constructorBlock);
        }

        private void Visit(
            EnumBlockSyntax enumBlock)
        {
            if (this.ShouldDiagramBeStarted(enumBlock))
            {
                this.StartDiagram(
                    enumBlock);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                enumBlock);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildEnumBlockCommand(
                enumName: this.GetJoinedNamespaceTypeName(
                    enumBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    enumBlock.EnumStatement));

            this.AddCommand(
                command: command,
                typeName: enumBlock.EnumStatement.Identifier.ValueText);

            base.Visit(
                enumBlock);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: enumBlock.EnumStatement.Identifier.ValueText);
        }

        private void Visit(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            string command = this.BuildEnumMemberDeclarationCommand(
                enumMemberName: enumMemberDeclaration.Identifier.ValueText,
                initializer: this.GetInitializer(
                    enumMemberDeclaration));

            this.AddCommand(
                command: command,
                typeName: enumMemberDeclaration.FirstAncestorOrSelf<EnumBlockSyntax>().EnumStatement.Identifier.ValueText);

            base.Visit(
                enumMemberDeclaration);
        }

        private void Visit(
            EventBlockSyntax eventBlock)
        {
            string command = this.BuildEventBlockCommand(
                eventName: eventBlock.EventStatement.Identifier.ValueText,
                eventTypeName: this.GetEventTypeName(
                    eventBlock),
                joinedAccessors: this.GetJoinedAccessors(
                    eventBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    eventBlock.EventStatement));

            this.AddCommand(
                command: command,
                typeName: this.GetFirstTypeAncestorTypeName(
                    eventBlock));

            base.Visit(
                eventBlock);
        }

        private void Visit(
            FieldDeclarationSyntax fieldDeclaration)
        {
            string command = this.BuildFieldDeclarationCommand(
                joinedModifiers: this.GetJoinedModifiers(
                    fieldDeclaration),
                joinedVariables: this.GetJoinedVariables(
                    fieldDeclaration));

            this.AddCommand(
                command: command,
                typeName: this.GetFirstTypeAncestorTypeName(
                    fieldDeclaration));

            base.Visit(
                fieldDeclaration);
        }

        private void Visit(
            InterfaceBlockSyntax interfaceBlock)
        {
            if (this.ShouldDiagramBeStarted(interfaceBlock))
            {
                this.StartDiagram(
                    interfaceBlock);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                interfaceBlock);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildInterfaceDeclarationCommand(
                interfaceName: this.GetJoinedNamespaceTypeName(
                    interfaceBlock),
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    interfaceBlock),
                joinedExtends: this.GetJoinedExtends(
                    interfaceBlock),
                joinedImplements: this.GetJoinedImplements(
                    interfaceBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    interfaceBlock.InterfaceStatement),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    interfaceBlock.InterfaceStatement));

            this.AddCommand(
                command: command,
                typeName: interfaceBlock.InterfaceStatement.Identifier.ValueText);

            base.Visit(
                interfaceBlock);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: interfaceBlock.InterfaceStatement.Identifier.ValueText);
        }

        private void Visit(
            MethodBlockSyntax methodBlock)
        {
            string command = this.BuildMethodBlockCommand(
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    methodBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    methodBlock.SubOrFunctionStatement),
                joinedParameters: this.GetJoinedParameters(
                    methodBlock),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    methodBlock.SubOrFunctionStatement),
                methodName: methodBlock.SubOrFunctionStatement.Identifier.ValueText,
                returnTypeName: this.GetReturnTypeName(
                    methodBlock));

            this.AddCommand(
                command: command,
                typeName: this.GetFirstTypeAncestorTypeName(
                    methodBlock));

            base.Visit(
                methodBlock);
        }

        // TODO: Finish
        private void Visit(
            ModuleBlockSyntax moduleBlock)
        {
        }

        private void Visit(
            PropertyBlockSyntax propertyBlock)
        {
            string command = this.BuildPropertyBlockCommand(
                initializer: this.GetInitializer(
                    propertyBlock),
                joinedAccessors: this.GetJoinedAccessors(
                    propertyBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    propertyBlock.PropertyStatement),
                propertyName: propertyBlock.PropertyStatement.Identifier.ValueText,
                propertyTypeName: this.GetPropertyTypeName(
                    propertyBlock));

            this.AddCommand(
                command: command,
                typeName: this.GetFirstTypeAncestorTypeName(
                    propertyBlock));

            base.Visit(
                propertyBlock);
        }

        private void Visit(
            StructureBlockSyntax structureBlock)
        {
            if (this.ShouldDiagramBeStarted(structureBlock))
            {
                this.StartDiagram(
                    structureBlock);
            }

            List<string> anchorRelationships = this.GetAnchorRelationships(
                structureBlock);

            if (anchorRelationships.Count() > 0)
            {
                this.Diagram.Relationships.AddRange(
                    anchorRelationships);
            }

            string command = this.BuildStructureBlockCommand(
                joinedConstraintClauses: this.GetJoinedConstraintClauses(
                    structureBlock),
                joinedExtends: this.GetJoinedExtends(
                    structureBlock),
                joinedImplements: this.GetJoinedImplements(
                    structureBlock),
                joinedModifiers: this.GetJoinedModifiers(
                    structureBlock.StructureStatement),
                joinedTypeParameters: this.GetJoinedTypeParameters(
                    structureBlock.StructureStatement),
                structName: this.GetJoinedNamespaceTypeName(
                    structureBlock));

            this.AddCommand(
                command: command,
                typeName: structureBlock.StructureStatement.Identifier.ValueText);

            base.Visit(
                structureBlock);

            this.AddCommand(
                command: $"{PlantUML_rightBrace}",
                typeName: structureBlock.StructureStatement.Identifier.ValueText);
        }
    }
}