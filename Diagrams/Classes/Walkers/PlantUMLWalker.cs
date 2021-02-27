namespace DotNetDiagrams.Classes.Walkers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;

    using DotNetDiagrams.Classes.Diagrams;
    using DotNetDiagrams.Extensions;
    using DotNetDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Interfaces.Walkers;
    
    internal sealed class PlantUMLWalker : CSharpSyntaxWalker, IPlantUMLWalker
    {
        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private string currentTitle;

        private bool ignore;
        private int indent;

        public PlantUMLWalker(
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

        private string Indent
        {
            get { return new string(' ', indent * 2); }
        }

        private List<string> PlantUMLCode => Diagrams.GetCodeAtTitleOrDefault(currentTitle);

        private string AssemblyName { get { return project.AssemblyName; } }

        public IPlantUMLDiagrams Diagrams { get; }

        private void EndDiagram()
        {
            if (!string.IsNullOrEmpty(currentTitle))
            {
                if (PlantUMLCode.Count > 4) // minimum # of lines in header
                    AddCommand("@enduml");
                else
                    Diagrams.RemoveAtTitle(currentTitle);
            }
        }

        private bool HasCallers(MethodDeclarationSyntax methodDeclaration)
        {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);
            IMethodSymbol methodSymbol = ModelExtensions.GetDeclaredSymbol(model, methodDeclaration) as IMethodSymbol;
            IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(methodSymbol, solution).GetAwaiter().GetResult();

            return callers.Any();
        }

        private void StartDiagram(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.GetParent<ClassDeclarationSyntax>() is not null)
            {
                string className = methodDeclaration.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                string methodName = methodDeclaration.Identifier.ValueText;
                currentTitle = $"{AssemblyName}_{className}_{methodName}";

                if (!Diagrams.ContainsTitle(currentTitle))
                    Diagrams.AddTitle(currentTitle);

                AddCommand("@startuml");
                AddCommand($"title {currentTitle}");
                AddCommand("autoactivate on");
                AddCommand("hide footbox");
            } 
            else if (methodDeclaration.GetParent<InterfaceDeclarationSyntax>() is not null)
            {
                // Ignore method declarations in interfaces
            }
        }

        private void AddCommand(string command, string unlessFollowing = null)
        {
            // add the command unless the last thing on the list is the second parameter
            // if it is, remove that entry and don't add the command
            if (unlessFollowing != null && PlantUMLCode.LastOrDefault() == unlessFollowing)
            {
                this.PlantUMLCode.RemoveAt(this.PlantUMLCode.Count - 1);
                return;
            }

            this.WriteCommandToDebug(
                command,
                currentTitle);

            this.PlantUMLCode.Add(command);
        }

        private void WriteCommandToDebug(
            string command,
            string title)
        {
            Debug.WriteLine("----------------------------------");
            Debug.WriteLine(title);
            Debug.WriteLine("   " + command);
        }

        public override void Visit(SyntaxNode node)
        {
            if (ignore)
            {
                base.Visit(node);
                return;
            }

            switch (node)
            {
                case ConstructorDeclarationSyntax constructorDeclaration:
                    Visit(constructorDeclaration);
                    break;
                case InvocationExpressionSyntax invocation:
                    Visit(invocation);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    Visit(memberAccess);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    Visit(methodDeclaration);
                    break;
                case StatementSyntax statement:
                    Visit(statement);
                    break;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    Visit(namespaceDeclaration);
                    break;
                default:
                    base.Visit(node);
                    break;
            }

            //switch (node)
            //{
            //    case ConstructorDeclarationSyntax constructorDeclaration:
            //        Visit(constructorDeclaration);
            //        break;
            //    case DoStatementSyntax doStatement:
            //        Visit(doStatement);
            //        break;
            //    case ForStatementSyntax forStatement:
            //        Visit(forStatement);
            //        break;
            //    case ForEachStatementSyntax forEachStatement:
            //        Visit(forEachStatement);
            //        break;
            //    case IfStatementSyntax ifStatement:
            //        Visit(ifStatement);
            //        break;
            //    case InvocationExpressionSyntax invocation:
            //        Visit(invocation);
            //        break;
            //    case MemberAccessExpressionSyntax memberAccess:
            //        Visit(memberAccess);
            //        break;
            //    case MethodDeclarationSyntax methodDeclaration:
            //        Visit(methodDeclaration);
            //        break;
            //    case NamespaceDeclarationSyntax namespaceDeclaration:
            //        Visit(namespaceDeclaration);
            //        break;
            //    case WhileStatementSyntax whileStatement:
            //        Visit(whileStatement);
            //        break;
            //    default:
            //        base.Visit(node);
            //        break;
            //}
        }

        private void Visit(ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(constructorDeclaration);
        }

        private void Visit(StatementSyntax statement)
        {
            List<SyntaxKind> validStatementKinds = new List<SyntaxKind> 
            { 
                SyntaxKind.DoStatement,
                SyntaxKind.IfStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.WhileStatement};
            
            if (!validStatementKinds.Contains(statement.Kind()))
            {
                base.Visit(statement);

                return;
            }

            string group = statement switch
            {
                DoStatementSyntax => "do/while",

                IfStatementSyntax => "if",

                ForStatementSyntax => "for",

                ForEachStatementSyntax => "foreach",

                WhileStatementSyntax => "while",

                { } => throw new System.Exception(statement.Kind().ToString())
            };

            string command1 = $"{Indent}group " + group;

            AddCommand(command1);

            ++indent;

            base.Visit(statement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }

        private void Visit(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifierName)
            {
                string callerTypeName;
                SemanticModel semanticModel;

                MethodDeclarationSyntax methodHost = invocation.GetParent<MethodDeclarationSyntax>();
                ConstructorDeclarationSyntax constructorHost = invocation.GetParent<ConstructorDeclarationSyntax>();

                if (methodHost != null)
                {
                    callerTypeName = methodHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(methodHost.SyntaxTree, true);
                }
                else if (constructorHost != null)
                {
                    callerTypeName = constructorHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(constructorHost.SyntaxTree, true);
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string targetTypeName;
                string targetName;
                string returnTypeName;

                if (ModelExtensions.GetTypeInfo(semanticModel, identifierName).Type == null)
                {
                    // same type as caller
                    targetTypeName = callerTypeName;
                    targetName = identifierName.Identifier.ValueText;
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else if (ModelExtensions.GetTypeInfo(semanticModel, identifierName).Type is INamedTypeSymbol targetType)
                {
                    targetTypeName = targetType.ToString();
                    targetName = invocation.TryGetInferredMemberName();
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string command = $"{Indent}{callerTypeName} -> {targetTypeName}: {targetName}";
                AddCommand(command);

                base.Visit(invocation);

                command = $"{Indent}{targetTypeName} --> {callerTypeName}: {returnTypeName}";
                AddCommand(command);
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                string callerTypeName;

                SemanticModel semanticModel;

                MethodDeclarationSyntax methodHost = invocation.GetParent<MethodDeclarationSyntax>();
                ConstructorDeclarationSyntax constructorHost = invocation.GetParent<ConstructorDeclarationSyntax>();

                if (methodHost != null)
                {
                    callerTypeName = methodHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(methodHost.SyntaxTree, true);
                }
                else if (constructorHost != null)
                {
                    callerTypeName = constructorHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(constructorHost.SyntaxTree, true);
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string targetTypeName;
                string targetName;
                string returnTypeName;

                if (ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpression).Type == null)
                {
                    // same type as caller
                    targetTypeName = callerTypeName;
                    targetName = memberAccessExpression.Name.Identifier.ValueText;
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else if (ModelExtensions.GetTypeInfo(semanticModel, memberAccessExpression).Type is INamedTypeSymbol targetType)
                {
                    targetTypeName = targetType.ToString();
                    targetName = invocation.TryGetInferredMemberName();
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string command = $"{Indent}{callerTypeName} -> {targetTypeName}: {targetName}";
                AddCommand(command);

                base.Visit(invocation);

                command = $"{Indent}{targetTypeName} --> {callerTypeName}: {returnTypeName}";
                AddCommand(command);
            }
            else
            {
            }
        }

        private void Visit(MemberAccessExpressionSyntax invocation)
        {
            if (invocation.Expression is IdentifierNameSyntax identifierName)
            {
                string callerTypeName;
                SemanticModel semanticModel;

                MethodDeclarationSyntax methodHost = invocation.GetParent<MethodDeclarationSyntax>();
                ConstructorDeclarationSyntax constructorHost = invocation.GetParent<ConstructorDeclarationSyntax>();

                if (methodHost != null)
                {
                    callerTypeName = methodHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(methodHost.SyntaxTree, true);
                }
                else if (constructorHost != null)
                {
                    callerTypeName = constructorHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                    semanticModel = compilation.GetSemanticModel(constructorHost.SyntaxTree, true);
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string targetTypeName;
                string targetName;
                string returnTypeName;

                if (ModelExtensions.GetTypeInfo(semanticModel, identifierName).Type == null)
                {
                    // same type as caller
                    targetTypeName = callerTypeName;
                    targetName = identifierName.Identifier.ValueText;
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else if (ModelExtensions.GetTypeInfo(semanticModel, identifierName).Type is INamedTypeSymbol targetType)
                {
                    targetTypeName = targetType.ToString();
                    targetName = invocation.TryGetInferredMemberName();
                    returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
                }
                else
                {
                    base.Visit(invocation);
                    return;
                }

                string command = $"{Indent}{callerTypeName} -> {targetTypeName}: {targetName}";
                AddCommand(command);

                base.Visit(invocation);

                command = $"{Indent}{targetTypeName} --> {callerTypeName}: {returnTypeName}";
                AddCommand(command);
            }
            else if (invocation.Expression is BaseExpressionSyntax basexpression)
            {
            }
            else if (invocation.Expression is ThisExpressionSyntax thisExpression)
            {
            }
            else if (invocation.Expression is PredefinedTypeSyntax predefinedType)
            {
            }
            else if (invocation.Expression is InvocationExpressionSyntax invocationExpression)
            {
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
            }
            else
            {
            }
        }

        private void Visit(MethodDeclarationSyntax methodDeclaration)
        {
            // we only care about method declarations that don't have callers
            ignore = HasCallers(methodDeclaration);

            if (!ignore)
                StartDiagram(methodDeclaration);

            try
            {
                base.Visit(methodDeclaration);
            }
            finally
            {
                if (!ignore)
                    EndDiagram();

                ignore = false;
            }
        }

        private void Visit(NamespaceDeclarationSyntax namespaceDeclaration)
        {
            base.Visit(namespaceDeclaration);
        }
    }
}