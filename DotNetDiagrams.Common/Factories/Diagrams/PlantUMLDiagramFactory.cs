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

    internal sealed class PlantUMLDiagramFactory : IPlantUMLDiagramFactory
    {
        public PlantUMLDiagramFactory()
        {
        }

        public IDiagram Create(
            string title,
            List<string> code)
        {
            IPlantUMLDiagram instance = null;

            try
            {
                instance = new PlantUMLDiagram(
                    title,
                    code);
            }
            catch (Exception exception)
            {
            }

            return instance;
        }
    }
}