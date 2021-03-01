namespace DotNetDiagrams.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDiagram
    {
        string Title { get; set; }

        List<string> Code { get; set; }
    }
}