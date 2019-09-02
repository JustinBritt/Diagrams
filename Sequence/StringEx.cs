// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetDiagrams {
    public static class StringEx
    {
        public static string RemoveNewLines(this string stringWithNewLines, bool cleanWhitespace = false)
        {
            char[] splitElements = cleanWhitespace
                                       ? new[] {'\r', '\n', ' '}
                                       : new[] {'\r', '\n'};

            return string.Join(" ", stringWithNewLines.Split(splitElements, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
