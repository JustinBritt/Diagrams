namespace Diagrams.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class StringExtensions
    {
        public static string RemoveNewLines(this string stringWithNewLines, bool cleanWhitespace = false)
        {
            List<char> splitElementList = cleanWhitespace ? new List<char> { '\r', '\n', ' ' } : new List<char> { '\r', '\n' };

            string[] stringElements = stringWithNewLines.Split(splitElementList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            
            return stringElements.Any() ? string.Join(" ", stringElements) : stringWithNewLines;
        }
    }
}