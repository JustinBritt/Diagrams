// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDiagrams {
    public static class StringEx
    {
        public static string RemoveNewLines(this string stringWithNewLines, bool cleanWhitespace = false)
        {
            string stringWithoutNewLines = null;
            List<char> splitElementList = Environment.NewLine.ToCharArray().ToList();

            if (cleanWhitespace)
            {
                splitElementList.AddRange(" ".ToCharArray().ToList());
            }

            char[] splitElements = splitElementList.ToArray();

            string[] stringElements = stringWithNewLines.Split(splitElements, StringSplitOptions.RemoveEmptyEntries);

            if (stringElements.Any())
            {
                stringWithoutNewLines = stringElements.Aggregate(stringWithoutNewLines
                                                               , (current, element) => current +
                                                                                       (current == null
                                                                                            ? element
                                                                                            : " " + element));
            }

            return stringWithoutNewLines ?? stringWithNewLines;
        }
    }
}
