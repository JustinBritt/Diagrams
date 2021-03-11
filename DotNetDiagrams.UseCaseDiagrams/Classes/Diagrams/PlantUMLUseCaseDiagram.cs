namespace DotNetDiagrams.UseCaseDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.UseCaseDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLUseCaseDiagram : IPlantUMLUseCaseDiagram
    {
        public PlantUMLUseCaseDiagram()
        {
        }

        public List<string> Body { get; set; }
        
        public string End { get; set; }

        public List<string> Header { get; set; }
        
        public string Start { get; set; }

        public List<string> Code { get; set; }

        public string Title { get; set; }
    }
}