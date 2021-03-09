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
        private const string stereotype_const = "<<const>>";
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

        // TODO: If multiple types are defined in the same file, then it uses the name of the first one
        private string DetermineTitle(
            TypeDeclarationSyntax typeDeclaration)
        {
            string namespaceName = String.Empty;

            if (this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>() is not null)
            {
                namespaceName = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().SingleOrDefault().Name.ToString();
            }
            
            string typeName = this.DetermineTypeDeclarationTypeName(typeDeclaration);

            return $"{namespaceName}.{typeName}";
        }

        private string DetermineTypeDeclarationTypeName(
            TypeDeclarationSyntax typeDeclaration)
        {
            List<TypeDeclarationSyntax> parentTypes = new List<TypeDeclarationSyntax>();

            List<TypeDeclarationSyntax> declaredTypes = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            foreach (TypeDeclarationSyntax declaredType in declaredTypes)
            {
                if (declaredType.DescendantNodesAndSelf().Contains(typeDeclaration))
                {
                    parentTypes.Add(declaredType);
                }
            }

            return String.Join(".", parentTypes.Select(w => w.Identifier.ValueText));
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

        private string BuildFieldDeclarationCommand(
            string fieldTypeName,
            string joinedModifiers,
            string joinedVariables)
        {
            StringBuilder sb = new StringBuilder();

            if (joinedModifiers.Length > 0)
            {
                sb.Append(joinedModifiers);
            }

            sb.Append(" ");

            sb.Append(fieldTypeName);

            sb.Append(" ");

            if (joinedVariables.Length > 0)
            {
                sb.Append(joinedVariables);
            }

            return sb.ToString();
        }

        // TODO: Finish
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

            sb.Append(" ");

            sb.Append(joinedModifiers);

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

        private List<string> GetBaseListTypes(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> baseTypeNames = new List<string>();

            if (classDeclaration.BaseList is not null)
            {
                foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
                {
                    baseTypeNames.Add(
                        this.GetTypeNameOrFallback(
                            baseType.Type.ToString(),
                            baseType.Type,
                            baseType.SyntaxTree));
                }
            }

            return baseTypeNames;
        }

        private List<string> GetBaseListTypes(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> baseTypeNames = new List<string>();

            if (interfaceDeclaration.BaseList is not null)
            {
                foreach (BaseTypeSyntax baseType in interfaceDeclaration.BaseList.Types)
                {
                    baseTypeNames.Add(
                        this.GetTypeNameOrFallback(
                            baseType.Type.ToString(),
                            baseType.Type,
                            baseType.SyntaxTree));
                }
            }

            return baseTypeNames;
        }

        private List<string> GetBaseListTypes(
            StructDeclarationSyntax structDeclaration)
        {
            List<string> baseTypeNames = new List<string>();

            if (structDeclaration.BaseList is not null)
            {
                foreach (BaseTypeSyntax baseType in structDeclaration.BaseList.Types)
                {
                    baseTypeNames.Add(
                        this.GetTypeNameOrFallback(
                            baseType.Type.ToString(),
                            baseType.Type,
                            baseType.SyntaxTree));
                }
            }

            return baseTypeNames;
        }

        private string GetConstraint(
            ClassDeclarationSyntax classDeclaration,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            string typeParameterConstraintName = String.Empty;

            if (typeParameterConstraint is TypeConstraintSyntax typeConstraint)
            {
                typeParameterConstraintName = this.GetTypeNameOrFallback(
                    typeConstraint.Type.ToString(),
                    typeConstraint.Type,
                    classDeclaration.SyntaxTree);
            }
            else
            {
                typeParameterConstraintName = typeParameterConstraint.ToString();
            }

            return typeParameterConstraintName;
        }

        private string GetConstraint(
            InterfaceDeclarationSyntax interfaceDeclaration,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            string typeParameterConstraintName = String.Empty;

            if (typeParameterConstraint is TypeConstraintSyntax typeConstraint)
            {
                typeParameterConstraintName = this.GetTypeNameOrFallback(
                    typeConstraint.Type.ToString(),
                    typeConstraint.Type,
                    interfaceDeclaration.SyntaxTree);
            }
            else
            {
                typeParameterConstraintName = typeParameterConstraint.ToString();
            }

            return typeParameterConstraintName;
        }

        private string GetConstraint(
            MethodDeclarationSyntax methodDeclaration,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            string typeParameterConstraintName = String.Empty;

            if (typeParameterConstraint is TypeConstraintSyntax typeConstraint)
            {
                typeParameterConstraintName = this.GetTypeNameOrFallback(
                    typeConstraint.Type.ToString(),
                    typeConstraint.Type,
                    methodDeclaration.SyntaxTree);
            }
            else
            {
                typeParameterConstraintName = typeParameterConstraint.ToString();
            }

            return typeParameterConstraintName;
        }

        private List<string> GetConstraintClauses(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> constraintClauses = new List<string>();

            if (classDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in classDeclaration.ConstraintClauses.ToList())
                {
                    string constraintClauseName = constraintClause.Name.Identifier.ValueText;

                    string joinedConstraints = this.GetJoinedConstraints(
                        classDeclaration,
                        constraintClause);

                    constraintClauses.Add($"{constraintClauseName} : {joinedConstraints}");
                }
            }

            return constraintClauses;
        }

        private List<string> GetConstraintClauses(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> constraintClauses = new List<string>();

            if (interfaceDeclaration.ConstraintClauses.Count() > 0)
            {
                foreach (TypeParameterConstraintClauseSyntax constraintClause in interfaceDeclaration.ConstraintClauses.ToList())
                {
                    string constraintClauseName = constraintClause.Name.Identifier.ValueText;

                    string joinedConstraints = this.GetJoinedConstraints(
                        constraintClause,
                        interfaceDeclaration);

                    constraintClauses.Add($"{constraintClauseName} : {joinedConstraints}");
                }
            }

            return constraintClauses;
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

        private List<string> GetConstraints(
            ClassDeclarationSyntax classDeclaration,
            TypeParameterConstraintClauseSyntax constraintClause)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                string typeParameterConstraintName = this.GetConstraint(
                    classDeclaration,
                    typeParameterConstraint);

                constraints.Add(
                    typeParameterConstraintName);
            }

            return constraints;
        }

        private List<string> GetConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                string typeParameterConstraintName = this.GetConstraint(
                    interfaceDeclaration,
                    typeParameterConstraint);

                constraints.Add(
                    typeParameterConstraintName);
            }

            return constraints;
        }

        private List<string> GetConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            MethodDeclarationSyntax methodDeclaration)
        {
            List<string> constraints = new List<string>();

            foreach (TypeParameterConstraintSyntax typeParameterConstraint in constraintClause.Constraints.ToList())
            {
                string typeParameterConstraintName = this.GetConstraint(
                    methodDeclaration,
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

        private string GetJoinedBaseListTypes(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> baseListTypes = this.GetBaseListTypes(
                classDeclaration);

            return String.Join(
                ",",
                baseListTypes);
        }

        private string GetJoinedBaseListTypes(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> baseListTypes = this.GetBaseListTypes(
                interfaceDeclaration);

            return String.Join(
                ",",
                baseListTypes);
        }

        private string GetJoinedBaseListTypes(
            StructDeclarationSyntax structDeclaration)
        {
            List<string> baseListTypes = this.GetBaseListTypes(
                structDeclaration);

            return String.Join(
                ",",
                baseListTypes);
        }

        private string GetJoinedConstraintClauses(
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> constraintClauses = this.GetConstraintClauses(
                classDeclaration);

            return String.Join(
                ", ",
                constraintClauses);
        }

        private string GetJoinedConstraintClauses(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> constraintClauses = this.GetConstraintClauses(
                interfaceDeclaration);

            return String.Join(
                ", ",
                constraintClauses);
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

        private string GetJoinedConstraints(
            ClassDeclarationSyntax classDeclaration,
            TypeParameterConstraintClauseSyntax constraintClause)
        {
            List<string> constraints = this.GetConstraints(
                classDeclaration,
                constraintClause);

            return String.Join(
                ", ",
                constraints.Select(w => w.ToString()));
        }

        private string GetJoinedConstraints(
            TypeParameterConstraintClauseSyntax constraintClause,
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> constraints = this.GetConstraints(
                constraintClause,
                interfaceDeclaration);

            return String.Join(
                ", ",
                constraints.Select(w => w.ToString()));
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
             ClassDeclarationSyntax classDeclaration)
        {
            List<string> typeParameters = this.GetTypeParameters(
                classDeclaration);

            return String.Join(
                ", ",
                typeParameters);
        }

        private string GetJoinedTypeParameters(
             InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> typeParameters = this.GetTypeParameters(
                interfaceDeclaration);

            return String.Join(
                ", ",
                typeParameters);
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

        private string GetJoinedVariables(
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> variables = this.GetVariables(
                fieldDeclaration);

            return String.Join(
                ", ",
                variables);
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
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> CSharpModifiers = fieldDeclaration.Modifiers.Select(w => w.ValueText).ToList();

            List<string> PlantUMLModifiers = new List<string>();

            foreach (string CSharpModifier in CSharpModifiers)
            {
                string PlantUMLModifier = CSharpModifier switch
                {
                    "const" => stereotype_const,

                    "private" => stereotype_private,

                    "public" => stereotype_public,

                    "readonly" => stereotype_readonly,

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
            ClassDeclarationSyntax classDeclaration)
        {
            List<string> typeParameters = new List<string>();

            if (classDeclaration.TypeParameterList is not null)
            {
                foreach (TypeParameterSyntax typeParameter in classDeclaration.TypeParameterList.Parameters.ToList())
                {
                    string typeParameterName = typeParameter.Identifier.ValueText;

                    typeParameters.Add(
                        typeParameterName);
                }
            }

            return typeParameters;
        }

        private List<string> GetTypeParameters(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<string> typeParameters = new List<string>();

            if (interfaceDeclaration.TypeParameterList is not null)
            {
                foreach (TypeParameterSyntax typeParameter in interfaceDeclaration.TypeParameterList.Parameters.ToList())
                {
                    string typeParameterName = typeParameter.Identifier.ValueText;

                    typeParameters.Add(
                        typeParameterName);
                }
            }

            return typeParameters;
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

        // TODO: Account for ArgumentList and Initializer
        private List<string> GetVariables(
            FieldDeclarationSyntax fieldDeclaration)
        {
            List<string> variableNames = new List<string>();

            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables.ToList())
            {
                string variableName = variable.Identifier.ValueText;

                // TODO: Finish
                if (variable.Initializer is not null)
                {
                    if (variable.Initializer is EqualsValueClauseSyntax equalsValueClause)
                    {
                        var variableInitializer = variable.Initializer;
                    }
                    else
                    {
                        throw new Exception("Remove");
                    }
                }

                // TODO: Finish
                if (variable.ArgumentList is not null)
                {
                    foreach (ArgumentSyntax argument in variable.ArgumentList.Arguments.ToList())
                    {
                        throw new Exception("Remove");
                    }
                }

                variableNames.Add(variableName);
            }

            return variableNames;
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

        // TODO: Account for Members?
        // TODO: Account for nesting; +-
        private void Visit(
            ClassDeclarationSyntax classDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (classDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    classDeclaration);
            }

            string command = this.BuildClassDeclarationCommand(
                className: classDeclaration.Identifier.ValueText,
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
            base.Visit(
                constructorDeclaration);
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

        // TODO: Finish
        // TODO: Account for Members
        private void Visit(
            InterfaceDeclarationSyntax interfaceDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (interfaceDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    interfaceDeclaration);
            }

            string command = this.BuildInterfaceDeclarationCommand(
                interfaceName: interfaceDeclaration.Identifier.ValueText,
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

            this.AddCommand(
                command: command,
                typeName: propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>().Identifier.ValueText);           

            base.Visit(
                propertyDeclaration);
        }

        // TODO: Finish
        // TODO: Account for ConstraintClauses, BaseList, TypeParameterList
        private void Visit(
            StructDeclarationSyntax structDeclaration)
        {
            List<TypeDeclarationSyntax> declaredTypes = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            if (structDeclaration == declaredTypes.First())
            {
                this.StartDiagram(
                    structDeclaration);
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
                structName: structDeclaration.Identifier.ValueText);

            this.AddCommand(
                command: command,
                typeName: structDeclaration.Identifier.ValueText);

            base.Visit(
                structDeclaration);
        }
    }
}