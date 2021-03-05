namespace DotNetDiagrams.ClassDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLClassDiagram : IPlantUMLClassDiagram
    {
        public PlantUMLClassDiagram()
        {
        }

        public List<string> Body { get; set; }

        public List<string> Code { get; set; }

        public string End { get; set; }

        public List<string> Header { get; set; }

        public string Start { get; set; }

        public string Title { get; set; }
    }
}