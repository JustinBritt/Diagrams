namespace DotNetDiagrams.SequenceDiagrams.Classes.Walkers
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
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLSequenceDiagramWalker : CSharpSyntaxWalker, IPlantUMLSequenceDiagramWalker
    {
        private const string group_do = "group do";
        private const string group_doWhile = "group do/while";
        private const string group_for = "group for";
        private const string group_foreach = "group foreach";
        private const string group_while = "group while";

        private const string PlantUML_alt = "alt";
        private const string PlantUML_autoactivate = "autoactivate";
        private const string PlantUML_else = "else";
        private const string PlantUML_end = "end";
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_footbox = "footbox";
        private const string PlantUML_hide = "hide";
        private const string PlantUML_off = "off";
        private const string PlantUML_on = "on";
        private const string PlantUML_opt = "opt";
        private const string PlantUML_show = "show";
        private const string PlantUML_startuml = "@startuml";
        private const string PlantUML_title = "title";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private string currentTitle;

        private bool ignore;

        public PlantUMLSequenceDiagramWalker(
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

        private List<string> PlantUMLCode => Diagrams.GetCodeAtTitleOrDefault(currentTitle);

        private string AssemblyName { get { return project.AssemblyName; } }

        public IPlantUMLDiagrams Diagrams { get; }

        private void EndDiagram()
        {
            if (!string.IsNullOrEmpty(currentTitle))
            {
                if (PlantUMLCode.Count > 4) // minimum # of lines in header
                    AddCommand(PlantUML_enduml);
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

        private string DetermineTitle(
            MethodDeclarationSyntax methodDeclaration)
        {
            string namespaceName = this.GetNamespaceName(
                this.syntaxTree);

            string typeName = String.Empty;

            string methodName = methodDeclaration.Identifier.ValueText;

            if (methodDeclaration.GetParent<ClassDeclarationSyntax>() is not null)
            {
                typeName = methodDeclaration.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
            }
            else if (methodDeclaration.GetParent<InterfaceDeclarationSyntax>() is not null)
            {
                typeName = methodDeclaration.GetParent<InterfaceDeclarationSyntax>().Identifier.ValueText;
            }
            else if (methodDeclaration.GetParent<StructDeclarationSyntax>() is not null)
            {
                typeName = methodDeclaration.GetParent<StructDeclarationSyntax>().Identifier.ValueText;
            }

            return $"{namespaceName}.{typeName}.{methodName}";
        }

        private void StartDiagram(MethodDeclarationSyntax methodDeclaration)
        {
            currentTitle = DetermineTitle(
                methodDeclaration);

            if (currentTitle is not null && currentTitle != String.Empty)
            {
                if (!Diagrams.ContainsTitle(currentTitle))
                {
                    Diagrams.AddTitle(currentTitle);
                }

                AddCommand(PlantUML_startuml);
                AddCommand($"{PlantUML_title} {currentTitle}");
                AddCommand($"{PlantUML_autoactivate} on");
                AddCommand($"hide {PlantUML_footbox}");
            }
        }

        private void AddCommand(
            string command)
        {
            string currentLast = PlantUMLCode.LastOrDefault();

            List<string> cannotImmediatelyPrecedePlantUML_end = new List<string>() 
            {
                group_do,
                group_doWhile,
                group_for,
                group_foreach,
                group_while,
                PlantUML_alt,
                PlantUML_else,
                PlantUML_opt,
            };

            if (command == PlantUML_end && currentLast == PlantUML_else)
            {
                this.PlantUMLCode.RemoveAt(this.PlantUMLCode.Count - 1);

                currentLast = PlantUMLCode.LastOrDefault();

                if (currentLast == PlantUML_alt || currentLast == PlantUML_opt)
                {
                    this.PlantUMLCode.RemoveAt(this.PlantUMLCode.Count - 1);

                    return;
                }  
            }
            else if (command == PlantUML_end && cannotImmediatelyPrecedePlantUML_end.Contains(currentLast))
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
            //Debug.WriteLine("----------------------------------");
            //Debug.WriteLine(title);
            //Debug.WriteLine("   " + command);
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
                case CatchClauseSyntax catchClause:
                    this.Visit(catchClause);
                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    this.Visit(constructorDeclaration);
                    break;
                case DoStatementSyntax doStatement:
                    this.Visit(doStatement);
                    break;
                case ElseClauseSyntax elseClause:
                    this.Visit(elseClause);
                    break;
                case FinallyClauseSyntax finallyClause:
                    this.Visit(finallyClause);
                    break;
                case ForStatementSyntax forStatement:
                    this.Visit(forStatement);
                    break;
                case ForEachStatementSyntax forEachStatement:
                    this.Visit(forEachStatement);
                    break;
                case IfStatementSyntax ifStatement:
                    this.Visit(ifStatement);
                    break;
                case InvocationExpressionSyntax invocation:
                    this.Visit(invocation);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    this.Visit(memberAccess);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    this.Visit(methodDeclaration);
                    break;
                case TryStatementSyntax tryStatement:
                    this.Visit(tryStatement);
                    break;
                case WhileStatementSyntax whileStatement:
                    this.Visit(whileStatement);
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

        /// <summary>
        /// This visits a catch clause.
        /// Mapping: C# "catch" -> PlantUML "else"
        /// </summary>
        /// <param name="catchClause">Catch clause</param>
        private void Visit(CatchClauseSyntax catchClause)
        {
            string command1 = $"{PlantUML_else}";

            // TODO: Check
            if (catchClause.Block.Statements.Count > 0)
            {
                AddCommand(command1);
            }

            base.Visit(catchClause);

            if (catchClause.Parent is TryStatementSyntax)
            {
                if (((TryStatementSyntax)catchClause.Parent).Catches.Last() == catchClause)
                {
                    AddCommand(
                        PlantUML_end);
                }
            }
        }

        private void Visit(ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(constructorDeclaration);
        }

        private void Visit(DoStatementSyntax doStatement)
        {
            string groupMessage = "do/while";

            string command1 = $"group " + groupMessage;

            AddCommand(command1);

            base.Visit(doStatement);

            AddCommand(PlantUML_end);
        }

        /// <summary>
        /// This visits an else clause.
        /// Mapping: C# "else" and "else if" -> PlantUML "else"
        /// </summary>
        /// <param name="elseClause">Else clause/param>
        private void Visit(ElseClauseSyntax elseClause)
        {
            AddCommand(PlantUML_else);

            base.Visit(elseClause);

            if (elseClause.Statement is BlockSyntax)
            {
                AddCommand(PlantUML_end);
            }

            //string command1 = PlantUML_else;

            //// if else that might have statements
            //if (elseClause.Statement is IfStatementSyntax)
            //{
            //    // Case (i): if else with statements
            //    bool casei = elseClause.Statement.ChildNodes().OfType<BlockSyntax>().FirstOrDefault().Statements.Any();

            //    if (casei)
            //    {
            //        AddCommand(command1);
            //    }
            //}
            //// else that might have statements
            //else if (elseClause.Statement is BlockSyntax)
            //{
            //    if (elseClause.Statement.ChildNodes().Count() > 0)
            //    {
            //        // Case (iia): else with statements other than if or else
            //        bool caseiia = ((BlockSyntax)elseClause.Statement).Statements.Where(w => w.Kind() is not SyntaxKind.IfStatement && w.Kind() is not SyntaxKind.ElseClause).Count() > 0;

            //        // Case (iib): else with descendant nodes that have statements
            //        bool caseiib = elseClause.Statement.DescendantNodes().OfType<BlockSyntax>().Select(w => w.Statements).Where(w => w.Count() > 0).Count() > 0;

            //        if (caseiia || caseiib)
            //        {
            //            AddCommand(command1);
            //        }
            //    }
            //}

            //base.Visit(elseClause);

            //if (elseClause.Statement is BlockSyntax)
            //{
            //    AddCommand(PlantUML_end);
            //}
        }

        private void Visit(FinallyClauseSyntax finallyClause)
        {
            base.Visit(finallyClause);
        }

        private void Visit(ForStatementSyntax forStatement)
        {
            string groupMessage = "for";

            string command1 = $"group " + groupMessage;

            AddCommand(command1);

            base.Visit(forStatement);

            AddCommand(PlantUML_end);
        }

        private void Visit(ForEachStatementSyntax forEachStatement)
        {
            string groupMessage = "for";

            string command1 = $"group " + groupMessage;

            AddCommand(command1);

            base.Visit(forEachStatement);

            string command2 = $"{PlantUML_end}";

            AddCommand(PlantUML_end);
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
            else if (invocation is MemberAccessExpressionSyntax memberAccessExpression)
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

            string command = $"{callerTypeName} -> {targetTypeName}: {targetName}";

            AddCommand(command);

            base.Visit(invocation);

            command = $"{targetTypeName} --> {callerTypeName}: {returnTypeName}";

            AddCommand(command);
        }

        /// <summary>
        /// This visits an if statement.
        /// Mapping: C# "if" -> PlantUML "opt" or "alt"
        /// </summary>
        /// <param name="ifStatement">If statement</param>
        private void Visit(IfStatementSyntax ifStatement)
        {
            string command1;

            if (ifStatement.Else is null)
            {
                command1 = PlantUML_opt;
            }
            else
            {
                command1 = PlantUML_alt;
            }

            if (ifStatement.Parent is BlockSyntax)
            {
                AddCommand(command1);
            }

            base.Visit(ifStatement);

            if (ifStatement.Else is null)
            {
                AddCommand(PlantUML_end);
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
            if (tryStatement.Parent is BlockSyntax)
            {
                AddCommand(PlantUML_alt);
            }

            base.Visit(tryStatement);

            if (tryStatement.Catches.Count == 0)
            {
                AddCommand(PlantUML_end);
            }
        }

        private void Visit(WhileStatementSyntax whileStatement)
        {
            AddCommand(group_while);

            base.Visit(whileStatement);

            AddCommand(PlantUML_end);
        }
    }
}