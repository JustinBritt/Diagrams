namespace DotNetDiagrams.ClassDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.ClassDiagrams.Interfaces.Diagrams;
    using DotNetDiagrams.Common.Interfaces.Diagrams;

    internal sealed class PlantUMLClassDiagrams : IPlantUMLClassDiagrams
    {
        public PlantUMLClassDiagrams()
        {
        }

        public List<IDiagram> Value { get; set; }

        public void AddTitle(
            string title)
        {
            this.Value.Add(new PlantUMLClassDiagram(title));
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

        public bool RemoveAtTitle(
            string title)
        {
            return this.Value.Remove(this.Value.Where(w => w.Title == title).SingleOrDefault());
        }
    }
}