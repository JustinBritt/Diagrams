namespace DotNetDiagrams.SequenceDiagrams.InterfacesFactories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.SequenceDiagrams.Interfaces.Generators;

    public interface IPlantUMLSequenceDiagramGeneratorFactory
    {
        ISequenceDiagramGenerator Create();
    }
}