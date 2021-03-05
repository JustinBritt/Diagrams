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

        public PlantUMLClassDiagram()
        {
            this.Body = new List<string>();

            this.Code = new List<string>();

            this.Header = new List<string>();
        }

        public List<string> Body { get; set; }

        public List<string> Code { get; set; }

        public string End { get; set; }

        public List<string> Header { get; set; }

        public string Start { get; set; }

        public string Title { get; set; }

        public void EndDiagram()
        {
            this.Code.Add(PlantUML_startuml);

            this.Code.AddRange(this.Header);

            this.Code.AddRange(this.Body);

            this.Code.Add(PlantUML_enduml);
        }
    }
}