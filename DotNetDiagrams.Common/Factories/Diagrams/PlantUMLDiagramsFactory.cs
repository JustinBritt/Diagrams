namespace DotNetDiagrams.Common.Factories.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Classes.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.Common.InterfacesFactories.Diagrams;

    internal sealed class PlantUMLDiagramsFactory : IPlantUMLDiagramsFactory
    {
        public PlantUMLDiagramsFactory()
        {
        }

        public IDiagrams Create()
        {
            IPlantUMLDiagrams instance = null;

            try
            {
                instance = new PlantUMLDiagrams();
            }
            catch (Exception exception)
            {
            }

            return instance;
        }
    }
}