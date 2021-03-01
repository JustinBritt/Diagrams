namespace DotNetDiagrams.SequenceDiagrams.Classes.PartialDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using NGenerics.DataStructures.Trees;

    using DotNetDiagrams.SequenceDiagrams.Interfaces.PartialDiagrams;

    internal sealed class PlantUMLPartialDiagram : IPlantUMLPartialDiagram
    {
        public PlantUMLPartialDiagram(
            string rootNode)
        {
            this.Tree = new GeneralTree<string>(rootNode);
        }

        public ITree<string> Tree { get; set; }
    }
}