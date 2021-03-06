﻿namespace DotNetDiagrams.Common.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public sealed class PlantUMLDiagrams : IPlantUMLDiagrams
    {
        public PlantUMLDiagrams()
        {
            this.Value = new List<IDiagram>();
        }

        public List<IDiagram> Value { get; set; }

        public void AddTitle(
            string title)
        {
            this.Value.Add(new PlantUMLDiagram(title, new List<string>()));
        }

        public bool ContainsTitle(
            string title)
        {
            return this.Value.Select(w => w.Title).Contains(title);
        }

        // TODO: Remove
        public List<string> GetCodeAtTitleOrDefault(
            string title)
        {
            return this.Value.Select(w => w.Title).Contains(title)
                ? this.Value.Where(w => w.Title == title).FirstOrDefault().Code
                : new List<string>();
        }

        public bool RemoveAtTitle(
            string title)
        {
            return this.Value.Remove(this.Value.Where(w => w.Title == title).SingleOrDefault());
        }
    }
}