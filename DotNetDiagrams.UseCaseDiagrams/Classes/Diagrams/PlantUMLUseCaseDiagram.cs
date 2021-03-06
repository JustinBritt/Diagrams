﻿namespace DotNetDiagrams.UseCaseDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.UseCaseDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLUseCaseDiagram : IPlantUMLUseCaseDiagram
    {
        private const string PlantUML_enduml = "@enduml";
        private const string PlantUML_startuml = "@startuml";

        public PlantUMLUseCaseDiagram(
            string title)
        {
            this.Body = new List<string>();

            this.Code = new List<string>();

            this.End = PlantUML_enduml;

            this.CSharpEntryPoints = new Dictionary<string, string>();

            this.Header = new List<string>();

            this.Start = PlantUML_startuml;

            this.Title = title;
        }

        public List<string> Body { get; set; }
        
        public string End { get; set; }

        public Dictionary<string, string> CSharpEntryPoints { get; set; }

        public List<string> Header { get; set; }
        
        public string Start { get; set; }

        public List<string> Code { get; set; }

        public string Title { get; set; }

        public void EndDiagram()
        {
            this.Code.Add(
                this.Start);

            this.Code.AddRange(
                this.Header);

            this.Code.AddRange(
                this.Body);

            this.Code.Add(
                this.End);
        }
    }
}