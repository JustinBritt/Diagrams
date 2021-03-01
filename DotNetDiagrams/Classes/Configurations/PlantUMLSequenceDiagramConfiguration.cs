namespace DotNetDiagrams.Classes.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Interfaces.Configurations;

    internal sealed class PlantUMLSequenceDiagramConfiguration : IPlantUMLSequenceDiagramConfiguration
    {
        public PlantUMLSequenceDiagramConfiguration(
            bool autoactivate = true,
            bool footbox = true)
        {
            this.Autoactivate = autoactivate;

            this.Footbox = footbox;
        }

        public bool Autoactivate { get; }

        public bool Footbox { get; }
    }
}