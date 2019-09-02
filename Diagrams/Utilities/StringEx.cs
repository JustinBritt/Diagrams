// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDiagrams
{

   public static class StringEx
   {
      public static string RemoveNewLines(this string stringWithNewLines, bool cleanWhitespace = false)
      {
         string result = null;

         if (stringWithNewLines != null)
         {
            List<char> splitElementList = new List<char> { '\r', '\n' };

            if (cleanWhitespace)
               splitElementList.Add(' ');

            string[] stringElements = stringWithNewLines.Split(splitElementList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            result = stringElements.Any() ? string.Join(" ", stringElements) : null;
         }

         return result ?? stringWithNewLines;
      }
   }

}
