namespace DotNetDiagrams.Common.InterfacesFactories.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public interface IPlantUMLDiagramFactory
    {
        IDiagram Create(
            string title,
            List<string> code);
    }
}