namespace DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public interface IPlantUMLClassDiagram : IPlantUMLDiagram
    {
        List<string> Body { get; set; }

        List<string> End { get; set; }

        List<string> Header { get; set; }

        List<string> Start { get; set; }
    }
}