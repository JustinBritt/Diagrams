namespace DotNetDiagrams.Common.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public sealed class PlantUMLDiagram : IPlantUMLDiagram
    {
        public PlantUMLDiagram(
            string title,
            List<string> code)
        {
            this.Title = title;

            this.Code = code;
        }

        public string Title { get; set; }

        public List<string> Code { get; set; }
    }
}