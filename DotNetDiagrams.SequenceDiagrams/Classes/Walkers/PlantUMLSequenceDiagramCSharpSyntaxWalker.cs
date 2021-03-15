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

    using DotNetDiagrams.Common.Extensions;
    using DotNetDiagrams.SequenceDiagrams.Classes.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Walkers;

    internal sealed class PlantUMLSequenceDiagramCSharpSyntaxWalker : CSharpSyntaxWalker, IPlantUMLSequenceDiagramCSharpSyntaxWalker
    {
        private const string group_do = "group do";
        private const string group_doWhile = "group do/while";
        private const string group_for = "group for";
        private const string group_foreach = "group foreach";
        private const string group_while = "group while";

        private const string PlantUML_alt = "alt";
        private const string PlantUML_arrow = "->";
        private const string PlantUML_autoactivate = "autoactivate";
        private const string PlantUML_dottedArrow = "-->";
        private const string PlantUML_else = "else";
        private const string PlantUML_end = "end";
        private const string PlantUML_footbox = "footbox";
        private const string PlantUML_hide = "hide";
        private const string PlantUML_off = "off";
        private const string PlantUML_on = "on";
        private const string PlantUML_opt = "opt";
        private const string PlantUML_show = "show";
        private const string PlantUML_title = "title";

        private const string stringConcatSeparator_namespaceTypeMethodNames = ".";

        private const string stringJoinSeparator_typeDeclarationTypeNames = ".";

        private readonly Compilation compilation;
        private readonly Project project;
        private readonly Solution solution;
        private readonly SyntaxTree syntaxTree;

        private string currentTitle;

        private bool ignore;

        public PlantUMLSequenceDiagramCSharpSyntaxWalker(
            Compilation compilation,
            SyntaxTree syntaxTree,
            Solution solution,
            Project project)
        {
            this.Diagrams = new PlantUMLSequenceDiagrams();

            this.compilation = compilation;

            this.syntaxTree = syntaxTree;

            this.solution = solution;

            this.project = project;
        }

        public IPlantUMLSequenceDiagram Diagram => Diagrams.GetSequenceDiagramAtTitleOrDefault(currentTitle);

        public IPlantUMLSequenceDiagrams Diagrams { get; }

        private void AddCommand(
            string command)
        {
            if (this.Diagram is not null)
            {
                string currentLast = this.Diagram.Body.LastOrDefault();

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
                    this.Diagram.Body.RemoveAt(this.Diagram.Body.Count - 1);

                    currentLast = this.Diagram.Body.LastOrDefault();

                    if (currentLast == PlantUML_alt || currentLast == PlantUML_opt)
                    {
                        this.Diagram.Body.RemoveAt(this.Diagram.Body.Count - 1);

                        return;
                    }
                }
                else if (command == PlantUML_end && cannotImmediatelyPrecedePlantUML_end.Contains(currentLast))
                {
                    this.Diagram.Body.RemoveAt(this.Diagram.Body.Count - 1);

                    return;
                }

                this.Diagram.Body.Add(command);
            }
        }

        private void AddHeader(
            bool autoactivate,
            bool footbox,
            string title)
        {
            List<string> currentHeader = new List<string>();

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

            this.Diagram?.Body.AddRange(
                currentHeader);
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

        private string GetBaseTypeDeclarationTypeName(
            BaseTypeDeclarationSyntax baseTypeDeclaration)
        {
            return String.Join(
                stringJoinSeparator_typeDeclarationTypeNames,
                baseTypeDeclaration.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(w => w.Identifier.ValueText));
        }

        private string GetJoinedNamespaceTypeMethodName(
            MethodDeclarationSyntax methodDeclaration)
        {
            string namespaceName = String.Empty;

            if (methodDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>() is not null)
            {
                namespaceName = methodDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();
            }

            string typeName = this.GetBaseTypeDeclarationTypeName(
                methodDeclaration.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>());

            string methodName = methodDeclaration.Identifier.ValueText;

            return String.Concat(
                namespaceName,
                stringConcatSeparator_namespaceTypeMethodNames,
                typeName,
                stringConcatSeparator_namespaceTypeMethodNames,
                methodName);
        }

        private bool HasCallers(
            MethodDeclarationSyntax methodDeclaration)
        {
            SemanticModel model = this.compilation.GetSemanticModel(this.syntaxTree);

            IMethodSymbol methodSymbol = ModelExtensions.GetDeclaredSymbol(model, methodDeclaration) as IMethodSymbol;

            IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(methodSymbol, this.solution).GetAwaiter().GetResult();

            return callers.Any();
        }

        private void StartDiagram(
            MethodDeclarationSyntax methodDeclaration)
        {
            this.currentTitle = this.GetJoinedNamespaceTypeMethodName(
                methodDeclaration);

            if (!String.IsNullOrEmpty(currentTitle))
            {
                if (!this.Diagrams.ContainsTitle(this.currentTitle))
                {
                    this.Diagrams.AddTitle(this.currentTitle);
                }

                this.AddHeader(
                    autoactivate: true,
                    footbox: true,
                    title: this.currentTitle);
            }
        }

        public override void Visit(
            SyntaxNode node)
        {
            if (this.ignore)
            {
                base.Visit(node);
                return;
            }

            switch (node)
            {
                case CatchClauseSyntax catchClause:
                    this.Visit(catchClause);
                    break;
                case DoStatementSyntax doStatement:
                    this.Visit(doStatement);
                    break;
                case ElseClauseSyntax elseClause:
                    this.Visit(elseClause);
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
                this.AddCommand(PlantUML_else);
            }

            base.Visit(catchClause);

            if (catchClause.Parent is TryStatementSyntax)
            {
                if (((TryStatementSyntax)catchClause.Parent).Catches.Last() == catchClause)
                {
                    this.AddCommand(
                        PlantUML_end);
                }
            }
        }

        private void Visit(
            DoStatementSyntax doStatement)
        {
            this.AddCommand(group_doWhile);

            base.Visit(doStatement);

            this.AddCommand(PlantUML_end);
        }

        /// <summary>
        /// This visits an else clause.
        /// Mapping: C# "else" and "else if" -> PlantUML "else"
        /// </summary>
        /// <param name="elseClause">Else clause/param>
        private void Visit(
            ElseClauseSyntax elseClause)
        {
            this.AddCommand(PlantUML_else);

            base.Visit(elseClause);

            if (elseClause.Statement is BlockSyntax)
            {
                this.AddCommand(PlantUML_end);
            }
        }

        private void Visit(
            ForStatementSyntax forStatement)
        {
            this.AddCommand(group_for);

            base.Visit(forStatement);

            this.AddCommand(PlantUML_end);
        }

        private void Visit(
            ForEachStatementSyntax forEachStatement)
        {
            this.AddCommand(group_foreach);

            base.Visit(forEachStatement);

            this.AddCommand(PlantUML_end);
        }

        private void Visit(
            ExpressionSyntax invocation)
        {
            SemanticModel semanticModel = this.solution.GetDocument(this.syntaxTree).GetSemanticModelAsync().Result;

            string callerTypeName = String.Empty;

            if (invocation.GetParent<TypeDeclarationSyntax>() is not null)
            {
                callerTypeName = invocation.GetParent<TypeDeclarationSyntax>().Identifier.ValueText;
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
                    AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name.Identifier.ValueText,

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

            string command = $"{callerTypeName} {PlantUML_arrow} {targetTypeName} : {targetName}";

            this.AddCommand(
                command);

            base.Visit(
                invocation);

            command = $"{targetTypeName} {PlantUML_dottedArrow} {callerTypeName} : {returnTypeName}";

            this.AddCommand(
                command);
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
                this.AddCommand(command1);
            }

            base.Visit(ifStatement);

            if (ifStatement.Else is null)
            {
                this.AddCommand(PlantUML_end);
            }
        }

        private void Visit(
            MethodDeclarationSyntax methodDeclaration)
        {
            // we only care about method declarations that don't have callers
            this.ignore = this.HasCallers(
                methodDeclaration);

            if (!this.ignore)
                this.StartDiagram(
                    methodDeclaration);

            try
            {
                base.Visit(
                    methodDeclaration);
            }
            finally
            {
                this.ignore = false;
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
                this.AddCommand(PlantUML_alt);
            }

            base.Visit(tryStatement);

            if (tryStatement.Catches.Count == 0)
            {
                this.AddCommand(PlantUML_end);
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
            this.AddCommand(group_while);

            base.Visit(whileStatement);

            this.AddCommand(PlantUML_end);
        }
    }
}