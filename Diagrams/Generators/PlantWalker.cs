using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace DotNetDiagrams
{

   internal class PlantWalker : CSharpSyntaxWalker
   {
      private static readonly Dictionary<string, List<string>> diagrams;
      private readonly Compilation compilation;
      private readonly Project project;
      private readonly Solution solution;
      private readonly SyntaxTree syntaxTree;

      private string currentTitle;

      private bool ignore;
      private int indent;

      static PlantWalker()
      {
         diagrams = new Dictionary<string, List<string>>(); // title, code
      }

      public PlantWalker(Compilation compilation
                       , SyntaxTree syntaxTree
                       , Solution solution
                       , Project project)
      {
         this.compilation = compilation;
         this.syntaxTree = syntaxTree;
         this.solution = solution;
         this.project = project;
      }

      private string Indent
      {
         get { return new string(' ', indent * 2); }
      }

      private List<string> PlantCode
      {
         get
         {
            return Diagrams.ContainsKey(currentTitle)
                      ? Diagrams[currentTitle]
                      : new List<string>();
         }
      }

      private string AssemblyName
      {
         get
         {
            return project.AssemblyName;
         }
      }

      public static Dictionary<string, List<string>> Diagrams
      {
         get
         {
            if (diagrams.Any() && diagrams.Last().Value.Any() && !diagrams.Last().Value.Last().EndsWith("@enduml"))
               diagrams.Last().Value.Add("@enduml");

            return diagrams;
         }
      }

      private void EndDiagram()
      {
         if (!string.IsNullOrEmpty(currentTitle))
         {
            if (PlantCode.Any())
               PlantCode.Add("@endum");
            else 
               Diagrams.Remove(currentTitle);
         }
      }

      private bool HasCallers(MethodDeclarationSyntax methodDeclaration)
      {
         SemanticModel model = compilation.GetSemanticModel(syntaxTree);
         IMethodSymbol methodSymbol = model.GetDeclaredSymbol(methodDeclaration);
         IEnumerable<SymbolCallerInfo> callers = SymbolFinder.FindCallersAsync(methodSymbol, solution).GetAwaiter().GetResult();

         return callers.Any();
      }

      private void StartDiagram(MethodDeclarationSyntax methodDeclaration)
      {
         currentTitle = $"{AssemblyName}_{methodDeclaration.GetParent<ClassDeclarationSyntax>().Identifier.ValueText}_{methodDeclaration.Identifier.ValueText}";

         if (!Diagrams.ContainsKey(currentTitle))
            Diagrams.Add(currentTitle, new List<string>());

         PlantCode.Add("@startuml");
         PlantCode.Add($"title {currentTitle}");
         PlantCode.Add("autoactivate on");
         PlantCode.Add("hide footbox");
      }

      private void StartDiagram(ConstructorDeclarationSyntax constructorDeclaration)
      {
         currentTitle = $"{AssemblyName}_{constructorDeclaration.GetParent<ClassDeclarationSyntax>().Identifier.ValueText}_{constructorDeclaration.Identifier.ValueText}";

         if (!Diagrams.ContainsKey(currentTitle))
            Diagrams.Add(currentTitle, new List<string>());

         PlantCode.Add("@startuml");
         PlantCode.Add($"title {currentTitle}");
         PlantCode.Add("autoactivate on");
         PlantCode.Add("hide footbox");
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
            case MethodDeclarationSyntax methodDeclaration:
               Visit(methodDeclaration);
               break;
            case ConstructorDeclarationSyntax constructorDeclaration:
               Visit(constructorDeclaration);
               break;
            case MemberAccessExpressionSyntax invocation:
               Visit(invocation);
               break;
            case IfStatementSyntax ifStatement:
               Visit(ifStatement);
               break;
            case ForStatementSyntax forStatement:
               Visit(forStatement);
               break;
            case ForEachStatementSyntax forEachStatement:
               Visit(forEachStatement);
               break;
            case DoStatementSyntax doStatement:
               Visit(doStatement);
               break;
            case WhileStatementSyntax whileStatement:
               Visit(whileStatement);
               break;
            default:
               base.Visit(node);
               break;
         }
      }

      private void Visit(WhileStatementSyntax whileStatement)
      {
         string command = $"{Indent}group while";
         PlantCode.Add(command);
         ++indent;
         base.Visit(whileStatement);
         --indent;
         command = $"{Indent}end";
         PlantCode.Add(command);
      }

      private void Visit(DoStatementSyntax doStatement)
      {
         string command = $"{Indent}group do/while";
         PlantCode.Add(command);
         ++indent;
         base.Visit(doStatement);
         --indent;
         command = $"{Indent}end";
         PlantCode.Add(command);
      }

      private void Visit(ForEachStatementSyntax forEachStatement)
      {
         string command = $"{Indent}group foreach";
         PlantCode.Add(command);
         ++indent;
         base.Visit(forEachStatement);
         --indent;
         command = $"{Indent}end";
         PlantCode.Add(command);
      }

      private void Visit(ForStatementSyntax forStatement)
      {
         string command = $"{Indent}group for";
         PlantCode.Add(command);
         ++indent;
         base.Visit(forStatement);
         --indent;
         command = $"{Indent}end";
         PlantCode.Add(command);
      }

      private void Visit(IfStatementSyntax ifStatement)
      {
         string command = $"{Indent}group if";
         PlantCode.Add(command);
         ++indent;
         base.Visit(ifStatement);
         --indent;
         command = $"{Indent}end";
         PlantCode.Add(command);
      }

      private void Visit(MemberAccessExpressionSyntax invocation)
      {
         if (invocation.Expression is IdentifierNameSyntax identifierName)
         {
            string callerTypeName;
            SemanticModel semanticModel;

            MethodDeclarationSyntax hostMethod = invocation.GetParent<MethodDeclarationSyntax>();

            if (hostMethod != null)
            {
               callerTypeName = hostMethod.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
               semanticModel = compilation.GetSemanticModel(hostMethod.SyntaxTree, true);
            }
            else
            {
               ConstructorDeclarationSyntax hostConstructor = invocation.GetParent<ConstructorDeclarationSyntax>();

               if (hostConstructor == null)
               {
                  base.Visit(invocation);
                  return;
               }

               callerTypeName = hostConstructor.GetParent<ClassDeclarationSyntax>().Identifier.ValueText;
               semanticModel = compilation.GetSemanticModel(hostConstructor.SyntaxTree, true);
            }
            
            if (semanticModel.GetTypeInfo(identifierName).Type is INamedTypeSymbol targetType)
            {
               string targetTypeName = targetType.ToString();
               string targetName = invocation.Name.Identifier.ValueText;
               string returnTypeName = semanticModel.GetTypeInfo(invocation).Type?.ToString().Split('.').Last() ?? string.Empty;

               string command = $"{Indent}{callerTypeName} -> {targetTypeName}: {targetName}";
               PlantCode.Add(command);

               base.Visit(invocation);

               command = $"{Indent}{targetTypeName} --> {callerTypeName}: {returnTypeName}";
               PlantCode.Add(command);
            }
         }
      }

      private void Visit(MethodDeclarationSyntax methodDeclaration)
      { 
         // we only care about method declarations that don't have callers
         if (HasCallers(methodDeclaration))
         {
            ignore = true;

            try
            {
               base.Visit(methodDeclaration);
            }
            finally
            {
               ignore = false;
            }
         }
         else
         {
            EndDiagram();
            StartDiagram(methodDeclaration);
         }
      }

      private void Visit(ConstructorDeclarationSyntax constructorDeclaration)
      {
         EndDiagram();
         StartDiagram(constructorDeclaration);
      }
   }
}
