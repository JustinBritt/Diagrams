namespace DotNetDiagrams.SequenceDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLSequenceDiagram : IPlantUMLSequenceDiagram
    {
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_startuml = "@startuml";

        public PlantUMLSequenceDiagram(
            string title)
        {
            this.Body = new List<string>();

            this.Code = new List<string>();

            this.End = PlantUML_enduml;

            this.Header = new List<string>();

            this.Start = PlantUML_startuml;

            this.Title = title;
        }

        public List<string> Body { get; set; }

        public List<string> Code { get; set; }

        public string End { get; set; }

        public List<string> Header { get; set; }

        public string Start { get; set; }

        public string Title { get; set; }

        public void EndDiagram()
        {
            this.Code.Add(this.Start);

            this.Code.AddRange(this.Header);

            this.Code.AddRange(this.Body);

            this.Code.Add(this.End);
        }
    }
}