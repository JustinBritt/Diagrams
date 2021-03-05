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

        public string Title { get; set; }

        public List<string> Code { get; set; }
    }
}