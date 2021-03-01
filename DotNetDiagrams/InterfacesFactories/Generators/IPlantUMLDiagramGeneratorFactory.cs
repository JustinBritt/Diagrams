namespace DotNetDiagrams.InterfacesFactories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Interfaces.Generators;

    public interface IPlantUMLDiagramGeneratorFactory
    {
        IDiagramGenerator Create();
    }
}