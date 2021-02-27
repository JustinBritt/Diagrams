namespace DotNetDiagrams.Factories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Classes.Generators;
    using DotNetDiagrams.Interfaces.Generators;
    using DotNetDiagrams.InterfacesFactories.Generators;

    internal sealed class PlantUMLDiagramGeneratorFactory : IPlantUMLDiagramGeneratorFactory
    {
        public PlantUMLDiagramGeneratorFactory()
        {
        }

        public IDiagramGenerator Create()
        {
            IDiagramGenerator instance = null;

            try
            {
                instance = new PlantUMLDiagramGenerator();
            }
            catch (Exception exception)
            {
            }

            return instance;
        }
    }
}