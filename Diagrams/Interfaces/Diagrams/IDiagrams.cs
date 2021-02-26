namespace DotNetDiagrams.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDiagrams
    {
        List<IDiagram> Value { get; set; }

        List<string> GetCodeAtTitleOrDefault(
            string title);

        bool RemoveAtTitle(
            string title);
    }
}