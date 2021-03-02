namespace DotNetDiagrams.ClassDiagrams.Factories.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.ClassDiagrams.Classes.Generators;
    using DotNetDiagrams.ClassDiagrams.Interfaces.Generators;
    using DotNetDiagrams.ClassDiagrams.InterfacesFactories.Generators;

    internal sealed class PlantUMLClassDiagramGeneratorFactory : IPlantUMLClassDiagramGeneratorFactory
    {
        public IClassDiagramGenerator Create()
        {
            IClassDiagramGenerator instance = null;

            try
            {
                instance = new PlantUMLClassDiagramGenerator();
            }
            catch (Exception exception)
            {
            }

            return instance;
        }
    }
}