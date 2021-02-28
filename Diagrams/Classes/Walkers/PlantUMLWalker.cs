namespace DotNetDiagrams.Classes.Walkers
{
    using System;
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

        private string GetNamespaceName(
            SyntaxTree syntaxTree)
        {
            string namespaceName;

            if (syntaxTree.GetRoot().IsKind(SyntaxKind.NamespaceDeclaration))
            {
                namespaceName = ((NamespaceDeclarationSyntax)syntaxTree.GetRoot()).Name.ToString();
            }
            else if (syntaxTree.GetRoot().DescendantNodes().Where(w => w.IsKind(SyntaxKind.NamespaceDeclaration)).Count() == 1)
            {
                namespaceName = ((NamespaceDeclarationSyntax)syntaxTree.GetRoot().DescendantNodes().Where(w => w.IsKind(SyntaxKind.NamespaceDeclaration)).SingleOrDefault()).Name.ToString();
            }
            else
            {
                namespaceName = AssemblyName;
            }

            return namespaceName;
        }

        private void StartDiagram(MethodDeclarationSyntax methodDeclaration)
        {
            string namespaceName = this.GetNamespaceName(
                this.syntaxTree);

            if (methodDeclaration.GetParent<ClassDeclarationSyntax>() is not null)
            {
                string className = methodDeclaration.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;

                string methodName = methodDeclaration.Identifier.ValueText;

                currentTitle = $"{namespaceName}.{className}.{methodName}";
            }
            else if (methodDeclaration.GetParent<InterfaceDeclarationSyntax>() is not null)
            {
                string interfaceName = methodDeclaration.GetParent<InterfaceDeclarationSyntax>().Identifier.ValueText;

                string methodName = methodDeclaration.Identifier.ValueText;

                currentTitle = $"{namespaceName}.{interfaceName}.{methodName}";
            }
            else if (methodDeclaration.GetParent<StructDeclarationSyntax>() is not null)
            {
                string structName = methodDeclaration.GetParent<StructDeclarationSyntax>().Identifier.ValueText;

                string methodName = methodDeclaration.Identifier.ValueText;

                currentTitle = $"{namespaceName}.{structName}.{methodName}";
            }

            if (currentTitle is not null && currentTitle != String.Empty)
            {
                if (!Diagrams.ContainsTitle(currentTitle))
                {
                    Diagrams.AddTitle(currentTitle);
                }

                AddCommand("@startuml");
                AddCommand($"title {currentTitle}");
                AddCommand("autoactivate on");
                AddCommand("hide footbox");
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
                case ElseClauseSyntax elseClause:
                    Visit(elseClause);
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
                default:
                    base.Visit(node);
                    break;
            }
        }

        private void Visit(ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(constructorDeclaration);
        }

        private void Visit(ElseClauseSyntax elseClause)
        {
            string groupMessage;

            if (elseClause.Statement is IfStatementSyntax)
            {
                groupMessage = "else if";
            }
            else if (elseClause.Statement is BlockSyntax)
            {
                groupMessage = "else";
            }
            else
            {
                return;
            }

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(elseClause.Statement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }

        private void Visit(ExpressionSyntax invocation)
        {
            string callerTypeName = String.Empty;

            SemanticModel semanticModel;

            ArrowExpressionClauseSyntax arrowClauseHost = invocation.GetParent<ArrowExpressionClauseSyntax>();

            MethodDeclarationSyntax methodHost = invocation.GetParent<MethodDeclarationSyntax>();

            ConstructorDeclarationSyntax constructorHost = invocation.GetParent<ConstructorDeclarationSyntax>();

            if (methodHost != null)
            {
                if (methodHost.GetParent<ClassDeclarationSyntax>() != null)
                {
                    callerTypeName = methodHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                }
                else if (methodHost.GetParent<InterfaceDeclarationSyntax>() != null)
                {
                    callerTypeName = methodHost.GetParent<InterfaceDeclarationSyntax>().Identifier.ValueText;
                }
                else if (methodHost.GetParent<StructDeclarationSyntax>() != null)
                {
                    callerTypeName = methodHost.GetParent<StructDeclarationSyntax>().Identifier.ValueText;
                }

                semanticModel = compilation.GetSemanticModel(methodHost.SyntaxTree, true);
            }
            else if (constructorHost != null)
            {
                if (constructorHost.GetParent<ClassDeclarationSyntax>() != null)
                {
                    callerTypeName = constructorHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                }
                else if (constructorHost.GetParent<InterfaceDeclarationSyntax>() != null)
                {
                    callerTypeName = constructorHost.GetParent<InterfaceDeclarationSyntax>().Identifier.ValueText;
                }
                else if (constructorHost.GetParent<StructDeclarationSyntax>() != null)
                {
                    callerTypeName = constructorHost.GetParent<StructDeclarationSyntax>().Identifier.ValueText;
                }

                semanticModel = compilation.GetSemanticModel(constructorHost.SyntaxTree, true);
            }
            else if (arrowClauseHost != null)
            {
                if (arrowClauseHost.GetParent<ClassDeclarationSyntax>() != null)
                {
                    callerTypeName = arrowClauseHost.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
                }
                else if (arrowClauseHost.GetParent<InterfaceDeclarationSyntax>() != null)
                {
                    callerTypeName = arrowClauseHost.GetParent<InterfaceDeclarationSyntax>().Identifier.ValueText;
                }
                else if (arrowClauseHost.GetParent<StructDeclarationSyntax>() != null)
                {
                    callerTypeName = arrowClauseHost.GetParent<StructDeclarationSyntax>().Identifier.ValueText;
                }

                semanticModel = compilation.GetSemanticModel(arrowClauseHost.SyntaxTree, true);
            }
            else
            {
                base.Visit(invocation);

                return;
            }

            string targetTypeName;
            string targetName;
            string returnTypeName;

            ExpressionSyntax expression;

            if (invocation is InvocationExpressionSyntax invocationExpression)
            {
                expression = invocationExpression.Expression;
            }
            else if(invocation is MemberAccessExpressionSyntax memberAccessExpression)
            {
                expression = memberAccessExpression.Expression;
            }
            else
            {
                throw new Exception("");
            }

            if (ModelExtensions.GetTypeInfo(semanticModel, expression).Type == null)
            {
                // same type as caller
                targetTypeName = callerTypeName;

                targetName = expression switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.ValueText,

                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,

                    MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Name.Identifier.ValueText,

                    MemberBindingExpressionSyntax memberBindingExpression => memberBindingExpression.Name.Identifier.ValueText,

                    { } => throw new Exception(expression.ToFullString())
                };

                returnTypeName = ModelExtensions.GetTypeInfo(semanticModel, invocation).Type?.ToString().Split('.').Last() ?? "void";
            }
            else if (ModelExtensions.GetTypeInfo(semanticModel, expression).Type is INamedTypeSymbol targetType)
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

        private void Visit(StatementSyntax statement)
        {
            List<SyntaxKind> validStatementKinds = new List<SyntaxKind>
            {
                SyntaxKind.DoStatement,
                SyntaxKind.IfStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.WhileStatement
            };

            if (!validStatementKinds.Contains(statement.Kind()))
            {
                base.Visit(statement);

                return;
            }

            string groupMessage = statement switch
            {
                DoStatementSyntax => "do/while",

                IfStatementSyntax => "if",

                ForStatementSyntax => "for",

                ForEachStatementSyntax => "foreach",

                WhileStatementSyntax => "while",

                { } => throw new System.Exception(statement.Kind().ToString())
            };

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(statement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }
    }
}