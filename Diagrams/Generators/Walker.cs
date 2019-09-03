using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetDiagrams
{
   internal class Walker : CSharpSyntaxWalker
   {
      private string assemblyName;
      private string className;
      private string methodName;
      private string target;

      private int Tabs = 0;

      public override void Visit(SyntaxNode node) 
      {
         if (node is ClassDeclarationSyntax classDeclaration)
         {
            try
            {
               className = classDeclaration.ChildNodesAndTokens().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText;
            }
            catch
            {
               className = null;
            }
         }
         else if (node is MethodDeclarationSyntax methodDeclaration)
         {
            try
            {
               methodName = methodDeclaration.ChildNodesAndTokens().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText;
            }
            catch
            {
               methodName = null;
            }
         }
         else if (node is IfStatementSyntax ifStatement)
         {

         }
         else if (node is MemberAccessExpressionSyntax memberAccessExpression)
         {
            try
            {
               target = memberAccessExpression.ChildNodes().OfType<IdentifierNameSyntax>().Last().Identifier.ValueText;
            }
            catch
            {
               target = null;
            }
         }
         else if (node is ForStatementSyntax forStatement) { }
         else if (node is ForEachStatementSyntax forEachStatement) { }
         else if (node is DoStatementSyntax doStatement) { }
         else if (node is WhileStatementSyntax whileStatement) { }

         Tabs++;
         string indents = new string(' ', Tabs);

         Console.WriteLine($"{Tabs:000} {indents}{node.Kind()} [{node.GetType().FullName}] - {node}");
         base.Visit(node);
         Tabs--;
      }

   }
}
