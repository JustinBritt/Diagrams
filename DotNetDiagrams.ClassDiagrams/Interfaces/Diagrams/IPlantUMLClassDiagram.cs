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
        List<string> Relationships { get; set; }

        List<Tuple<int, string>> Types { get; set; }

        void EndDiagram();
    }
}