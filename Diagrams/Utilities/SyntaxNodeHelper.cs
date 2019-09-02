// 

// 

using Microsoft.CodeAnalysis;

namespace DotNetDiagrams
{

   internal static class SyntaxNodeHelper
   {
      public static bool TryGetParentSyntax<T>(this SyntaxNode syntaxNode, out T result)
         where T : SyntaxNode
      {
         result = null;

         try
         {
            syntaxNode = syntaxNode?.Parent;

            if (syntaxNode == null)
               return false;

            if (syntaxNode.GetType() == typeof(T))
            {
               result = syntaxNode as T;
               return true;
            }

            return TryGetParentSyntax(syntaxNode, out result);
         }
         catch
         {
            return false;
         }
      }
   }

}
