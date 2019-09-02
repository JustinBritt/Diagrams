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
         List<char> splitElementList = cleanWhitespace ? new List<char> { '\r', '\n', ' ' } :  new List<char> { '\r', '\n' };
         string[] stringElements = stringWithNewLines.Split(splitElementList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
         return stringElements.Any() ? string.Join(" ", stringElements) : stringWithNewLines;
      }
   }

}
