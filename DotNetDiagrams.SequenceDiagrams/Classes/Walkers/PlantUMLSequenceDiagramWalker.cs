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

        private List<string> currentHeader;
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

        public IPlantUMLDiagrams Diagrams { get; }

        private void EndDiagram()
        {
            if (!string.IsNullOrEmpty(currentTitle))
            {
                if (PlantUMLCode.Count > currentHeader.Count) // minimum # of lines in header
                    AddCommand(PlantUML_enduml);
                else
                    Diagrams.RemoveAtTitle(currentTitle);
            }
        }

        private bool HasCallers(
            MethodDeclarationSyntax methodDeclaration)
        {
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);

            IMethodSymbol methodSymbol = ModelExtensions.GetDeclaredSymbol(model, methodDeclaration) as IMethodSymbol;

            IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(methodSymbol, solution).GetAwaiter().GetResult();

            return callers.Any();
        }

        private string DetermineTitle(
            MethodDeclarationSyntax methodDeclaration)
        {
            string namespaceName = this.syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().SingleOrDefault().Name.ToString();

            List<TypeDeclarationSyntax> parentTypes = new List<TypeDeclarationSyntax>();

            List<TypeDeclarationSyntax> declaredTypes = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().ToList();

            foreach (TypeDeclarationSyntax declaredType in declaredTypes)
            {
                if (declaredType.DescendantNodesAndSelf().Contains(methodDeclaration))
                {
                    parentTypes.Add(declaredType);
                }
            }

            string typeName = String.Join(".", parentTypes.Select(w => w.Identifier.ValueText));

            string methodName = methodDeclaration.Identifier.ValueText;

            return $"{namespaceName}.{typeName}.{methodName}";
        }

        private void StartDiagram(
            MethodDeclarationSyntax methodDeclaration)
        {
            currentTitle = DetermineTitle(
                methodDeclaration);

            if (!String.IsNullOrEmpty(currentTitle))
            {
                if (!Diagrams.ContainsTitle(currentTitle))
                {
                    Diagrams.AddTitle(currentTitle);
                }

                this.AddHeader(
                    autoactivate: true,
                    footbox: true,
                    title: currentTitle);
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

            this.PlantUMLCode.Add(command);
        }

        private void AddHeader(
            bool autoactivate,
            bool footbox,
            string title)
        {
            currentHeader = new List<string>();

            currentHeader.Add(PlantUML_startuml);

            currentHeader.Add($"{PlantUML_title} {title}");

            if (autoactivate)
            {
                currentHeader.Add($"{PlantUML_autoactivate} {PlantUML_on}");
            }
            else
            {
                currentHeader.Add($"{PlantUML_autoactivate} {PlantUML_off}");
            }

            if (footbox)
            {
                currentHeader.Add($"{PlantUML_show} {PlantUML_footbox}");
            }
            else
            {
                currentHeader.Add($"{PlantUML_hide} {PlantUML_footbox}");
            }

            currentHeader.ForEach(w => AddCommand(w));
        }

        private string EscapeGreaterThanLessThan(
            string value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                if (value.Contains("<") || value.Contains(">"))
                {
                    value = @"""" + $"{value}" + @"""";
                }
            }

            return value;
        }

        public override void Visit(
            SyntaxNode node)
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
        private void Visit(
            BreakStatementSyntax breakStatement)
        {
            base.Visit(breakStatement);
        }

        /// <summary>
        /// This visits a catch clause.
        /// Mapping: C# "catch" -> PlantUML "else"
        /// </summary>
        /// <param name="catchClause">Catch clause</param>
        private void Visit(
            CatchClauseSyntax catchClause)
        {
            if (catchClause.Block.Statements.Count > 0)
            {
                AddCommand(PlantUML_else);
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

        private void Visit(
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            base.Visit(constructorDeclaration);
        }

        private void Visit(
            DoStatementSyntax doStatement)
        {
            AddCommand(group_doWhile);

            base.Visit(doStatement);

            AddCommand(PlantUML_end);
        }

        /// <summary>
        /// This visits an else clause.
        /// Mapping: C# "else" and "else if" -> PlantUML "else"
        /// </summary>
        /// <param name="elseClause">Else clause/param>
        private void Visit(
            ElseClauseSyntax elseClause)
        {
            AddCommand(PlantUML_else);

            base.Visit(elseClause);

            if (elseClause.Statement is BlockSyntax)
            {
                AddCommand(PlantUML_end);
            }
        }

        private void Visit(
            FinallyClauseSyntax finallyClause)
        {
            base.Visit(finallyClause);
        }

        private void Visit(
            ForStatementSyntax forStatement)
        {
            AddCommand(group_for);

            base.Visit(forStatement);

            AddCommand(PlantUML_end);
        }

        private void Visit(
            ForEachStatementSyntax forEachStatement)
        {
            AddCommand(group_foreach);

            base.Visit(forEachStatement);

            AddCommand(PlantUML_end);
        }

        private void Visit(
            ExpressionSyntax invocation)
        {
            SemanticModel semanticModel = this.solution.GetDocument(this.syntaxTree).GetSemanticModelAsync().Result;

            string callerTypeName = invocation.GetParent<TypeDeclarationSyntax>().Identifier.ValueText;

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

            callerTypeName = this.EscapeGreaterThanLessThan(
                callerTypeName);

            returnTypeName = this.EscapeGreaterThanLessThan(
                returnTypeName);

            targetName = this.EscapeGreaterThanLessThan(
                targetName);

            targetTypeName = this.EscapeGreaterThanLessThan(
                targetTypeName);

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
        private void Visit(
            IfStatementSyntax ifStatement)
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

        private void Visit(
            MethodDeclarationSyntax methodDeclaration)
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
        private void Visit(
            TryStatementSyntax tryStatement)
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

        /// <summary>
        /// This visits a while statement.
        /// Mapping: C# "while" -> PlantUML "group while"
        /// </summary>
        /// <param name="whileStatement"></param>
        private void Visit(
            WhileStatementSyntax whileStatement)
        {
            AddCommand(group_while);

            base.Visit(whileStatement);

            AddCommand(PlantUML_end);
        }
    }
}