namespace DotNetDiagrams.SequenceDiagrams.Factories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.SequenceDiagrams.Classes.Generators;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Generators;
    using DotNetDiagrams.SequenceDiagrams.InterfacesFactories.Generators;

    public sealed class PlantUMLSequenceDiagramGeneratorFactory : IPlantUMLSequenceDiagramGeneratorFactory
    {
        public PlantUMLSequenceDiagramGeneratorFactory()
        {
        }

        public ISequenceDiagramGenerator Create()
        {
            ISequenceDiagramGenerator instance = null;

            try
            {
                instance = new PlantUMLSequenceDiagramGenerator();
            }
            catch (Exception exception)
            {
            }

            return instance;
        }
    }
}