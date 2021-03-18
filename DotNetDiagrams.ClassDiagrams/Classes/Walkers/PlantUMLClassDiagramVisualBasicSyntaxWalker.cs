﻿namespace DotNetDiagrams.ClassDiagrams.Classes.Walkers
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
        private const string stereotype_get = "<<get>>";
        private const string stereotype_internal = "<<internal>>";
        private const string stereotype_override = "<<override>>";
        private const string stereotype_partial = "<<partial>>";
        private const string stereotype_private = "<<private>>";
        private const string stereotype_protected = "<<protected>>";
        private const string stereotype_public = "<<public>>";
        private const string stereotype_raise = "<<raise>>";
        private const string stereotype_readonly = "<<readonly>>";
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

        // TODO: Update
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

        private string GetInitializer(
            EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            return enumMemberDeclaration.Initializer is not null
                ? stereotype_equals
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

        // TODO: Account for Modifiers, Default, AsClause
        private string GetParameter(
            ConstructorBlockSyntax constructorBlock,
            ParameterSyntax parameter)
        {
            string parameterName = parameter.Identifier.Identifier.ValueText;

            string parameterTypeName = this.GetTypeName(
                parameter,
                constructorBlock.SyntaxTree);

            return $"{parameterTypeName} {parameterName}";
        }

        // TODO: Account for Modifiers, Default, AsClause
        private string GetParameter(
            MethodBlockSyntax methodBlock,
            ParameterSyntax parameter)
        {
            string parameterName = parameter.Identifier.Identifier.ValueText;

            string parameterTypeName = this.GetTypeName(
                parameter,
                methodBlock.SyntaxTree);

            return $"{parameterTypeName} {parameterName}";
        }

        // TODO: Add const for void
        private string GetReturnTypeName(
            MethodBlockSyntax methodBlock)
        {
            return methodBlock.SubOrFunctionStatement.AsClause is not null
                ? this.GetTypeName(
                    syntaxNode: methodBlock.SubOrFunctionStatement.AsClause,
                    syntaxTree: methodBlock.SyntaxTree)
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

        // TODO: Finish
        private void Visit(
            ConstructorBlockSyntax constructorBlock)
        {
            // TODO: Update name
            string typeName = String.Empty;

            if (constructorBlock.FirstAncestorOrSelf<ClassBlockSyntax>() is not null)
            {
                typeName = constructorBlock.FirstAncestorOrSelf<ClassBlockSyntax>().ClassStatement.Identifier.ValueText;
            }
            else if (constructorBlock.FirstAncestorOrSelf<ModuleBlockSyntax>() is not null)
            {
                typeName = constructorBlock.FirstAncestorOrSelf<ModuleBlockSyntax>().ModuleStatement.Identifier.ValueText;
            }
            else if (constructorBlock.FirstAncestorOrSelf<StructureBlockSyntax>() is not null)
            {
                typeName = constructorBlock.FirstAncestorOrSelf<StructureBlockSyntax>().StructureStatement.Identifier.ValueText;
            }
            else
            {
                throw new Exception("");
            }

            // TODO: Update constructor name
            string constructorName = String.Join(
                " ",
                constructorBlock.SubNewStatement.SubKeyword.ValueText,
                constructorBlock.SubNewStatement.NewKeyword.ValueText);

            string command = this.BuildConstructorBlockCommand(
                constructorName: constructorName,
                joinedModifiers: this.GetJoinedModifiers(
                    constructorBlock.SubNewStatement),
                joinedParameters: this.GetJoinedParameters(
                    constructorBlock));

            this.AddCommand(
                command: command,
                typeName: typeName);

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

        // TODO: Finish
        private void Visit(
            EventBlockSyntax eventBlock)
        {
            string joinedModifiers = this.GetJoinedModifiers(
                   eventBlock.EventStatement);

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

        // TODO: Finish
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

            // TODO: Change typeName
            this.AddCommand(
                command: command,
                typeName: Guid.NewGuid().ToString());

            base.Visit(
                methodBlock);
        }

        // TODO: Finish
        private void Visit(
            PropertyBlockSyntax propertyBlock)
        {
            string joinedAccessors = this.GetJoinedAccessors(
                propertyBlock);

            string joinedModifiers = this.GetJoinedModifiers(
                propertyBlock.PropertyStatement);

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

        // TODO: Finish
        private void Visit(
            SubNewStatementSyntax subNewStatement)
        {
            base.Visit(
                subNewStatement);
        }
    }
}