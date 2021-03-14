namespace DotNetDiagrams.SequenceDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;
    using DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLSequenceDiagrams : IPlantUMLSequenceDiagrams
    {
        public PlantUMLSequenceDiagrams()
        {
            this.Value = new List<IDiagram>();
        }

        public List<IDiagram> Value { get; set; }

        public void AddTitle(
            string title)
        {
            this.Value.Add(new PlantUMLSequenceDiagram(title));
        }

        public bool ContainsTitle(
            string title)
        {
            return this.Value.Select(w => w.Title).Contains(title);
        }

        public List<string> GetCodeAtTitleOrDefault(
            string title)
        {
            return this.Value.Select(w => w.Title).Contains(title)
                ? this.Value.Where(w => w.Title == title).SingleOrDefault().Code
                : new List<string>();
        }

        public IPlantUMLSequenceDiagram GetSequenceDiagramAtTitleOrDefault(
            string title)
        {
            return (IPlantUMLSequenceDiagram)(this.Value.Select(w => w.Title).Contains(title)
                ? this.Value.Where(w => w.Title == title).SingleOrDefault()
                : null);
        }

        public bool RemoveAtTitle(
            string title)
        {
            return this.Value.Remove(this.Value.Where(w => w.Title == title).SingleOrDefault());
        }
    }
}