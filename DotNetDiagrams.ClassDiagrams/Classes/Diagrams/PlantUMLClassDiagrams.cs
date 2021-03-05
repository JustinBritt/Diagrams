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
            throw new NotImplementedException();
        }

        public bool ContainsTitle(
            string title)
        {
            throw new NotImplementedException();
        }

        public List<string> GetCodeAtTitleOrDefault(
            string title)
        {
            throw new NotImplementedException();
        }

        public bool RemoveAtTitle(
            string title)
        {
            throw new NotImplementedException();
        }
    }
}