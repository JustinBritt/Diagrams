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

            this.Body = new List<string>();

            this.End = string.Empty;

            this.Header = new List<string>();

            this.Start = String.Empty;
        }

        public List<string> Body { get; set; }

        public List<string> Code { get; set; }

        public string End { get; set; }

        public List<string> Header { get; set; }

        public string Start { get; set; }

        public string Title { get; set; }
    }
}