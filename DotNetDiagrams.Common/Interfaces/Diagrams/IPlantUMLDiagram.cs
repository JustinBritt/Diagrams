namespace DotNetDiagrams.Common.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IPlantUMLDiagram : IDiagram
    {
        List<string> Body { get; set; }

        string End { get; set; }

        List<string> Header { get; set; }

        string Start { get; set; }
    }
}