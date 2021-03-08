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
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_startuml = "@startuml";

        public PlantUMLClassDiagram(
            string title)
        {
            this.Body = new List<string>();

            this.Code = new List<string>();

            this.End = PlantUML_enduml;

            this.Header = new List<string>();

            this.Relationships = new List<string>();

            this.Start = PlantUML_startuml;

            this.Title = title;

            this.Types = new List<Tuple<string, int, List<string>>>();
        }

        public List<string> Body { get; set; }

        public List<string> Code { get; set; }

        public string End { get; set; }

        public List<string> Header { get; set; }

        public List<string> Relationships { get; set; }

        public List<Tuple<string, int, List<string>>> Types { get; set; }

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