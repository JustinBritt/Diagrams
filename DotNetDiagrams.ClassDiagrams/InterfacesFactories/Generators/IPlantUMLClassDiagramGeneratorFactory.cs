namespace DotNetDiagrams.ClassDiagrams.InterfacesFactories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;

    public interface IPlantUMLClassDiagramGeneratorFactory
    {
        IClassDiagramGenerator Create();
    }
}