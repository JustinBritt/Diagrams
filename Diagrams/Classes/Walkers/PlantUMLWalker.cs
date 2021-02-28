﻿namespace DotNetDiagrams.Classes.Walkers
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
                case DoStatementSyntax doStatement:
                    Visit(doStatement);
                    break;
                case ElseClauseSyntax elseClause:
                    Visit(elseClause);
                    break;
                case ForStatementSyntax forStatement:
                    Visit(forStatement);
                    break;
                case ForEachStatementSyntax forEachStatement:
                    Visit(forEachStatement);
                    break;
                case IfStatementSyntax ifStatement:
                    Visit(ifStatement);
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
                case TryStatementSyntax tryStatement:
                    Visit(tryStatement);
                    break;
                case WhileStatementSyntax whileStatement:
                    Visit(whileStatement);
                    break;
                default:
                    base.Visit(node);
                    break;
            }
        }

        // TODO: Finish
        private void Visit(BreakStatementSyntax breakStatement)
        {
            base.Visit(breakStatement);
        }

        // TODO: Finish
        private void Visit(CatchClauseSyntax catchClause)
        {
            base.Visit(catchClause);
        }

        private void Visit(ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(constructorDeclaration);
        }

        private void Visit(DoStatementSyntax doStatement)
        {
            string groupMessage = "do/while";

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(doStatement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }

        /// <summary>
        /// This visits an else clause.
        /// Mapping: C# "else" and "else if" -> PlantUML "else"
        /// </summary>
        /// <param name="elseClause">Else clause/param>
        private void Visit(ElseClauseSyntax elseClause)
        {
            string command1 = "else";
            
            // if else that might have statements
            if (elseClause.Statement is IfStatementSyntax)
            {
                // Case (i): if else with statements
                bool casei = elseClause.Statement.ChildNodes().OfType<BlockSyntax>().FirstOrDefault().Statements.Any();

                if (casei)
                {
                    AddCommand(command1);
                }
            }
            // else that might have statements
            else if (elseClause.Statement is BlockSyntax)
            {
                if (elseClause.Statement.ChildNodes().Count() > 0)
                {
                    // Case (iia): else with statements other than if or else
                    bool caseiia = ((BlockSyntax)elseClause.Statement).Statements.Where(w => w.Kind() is not SyntaxKind.IfStatement && w.Kind() is not SyntaxKind.ElseClause).Count() > 0;

                    // Case (iib): else with descendant nodes that have statements
                    bool caseiib = elseClause.Statement.DescendantNodes().OfType<BlockSyntax>().Select(w => w.Statements).Where(w => w.Count() > 0).Count() > 0;

                    if (caseiia || caseiib)
                    {
                        AddCommand(command1);
                    }
                }            
            }

            base.Visit(elseClause);

            if (elseClause.Statement is BlockSyntax)
            {
                string command2 = $"end";

                AddCommand(command2, command1);

                --indent;
            }
        }

        private void Visit(ForStatementSyntax forStatement)
        {
            string groupMessage = "for";

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(forStatement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }

        private void Visit(ForEachStatementSyntax forEachStatement)
        {
            string groupMessage = "for";

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(forEachStatement);

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

        /// <summary>
        /// This visits an if statement.
        /// Mapping: C# "if" -> PlantUML "alt"
        /// </summary>
        /// <param name="ifStatement">If statement</param>
        private void Visit(IfStatementSyntax ifStatement)
        {
            string command1 = "alt";

            if (ifStatement.Parent is BlockSyntax)
            {
                AddCommand(command1);

                ++indent;
            }

            base.Visit(ifStatement);

            if (ifStatement.Else is null)
            {
                string command2 = $"end";

                AddCommand(command2, command1);

                --indent;
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

        /// <summary>
        /// This visits a try statement.
        /// Mapping: C# "Try" -> PlantUML "alt"
        /// </summary>
        /// <param name="tryStatement">Try statement</param>
        private void Visit(TryStatementSyntax tryStatement)
        {
            string command1 = "alt [try]";

            if (tryStatement.Parent is BlockSyntax)
            {
                AddCommand(command1);

                ++indent;
            }
            else
            {
                throw new Exception("");
            }

            base.Visit(tryStatement);

            if (tryStatement.Catches.Count == 0 && tryStatement.Finally == null)
            {
                string command2 = $"end";

                AddCommand(command2, command1);

                --indent;
            }
        }

        private void Visit(WhileStatementSyntax whileStatement)
        {
            string groupMessage = "while";

            string command1 = $"{Indent}group " + groupMessage;

            AddCommand(command1);

            ++indent;

            base.Visit(whileStatement);

            --indent;

            string command2 = $"{Indent}end";

            AddCommand(command2, command1);
        }
    }
}