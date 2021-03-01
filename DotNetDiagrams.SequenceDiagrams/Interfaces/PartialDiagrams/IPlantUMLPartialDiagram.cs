namespace DotNetDiagrams.SequenceDiagrams.Interfaces.PartialDiagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using NGenerics.DataStructures.Trees;

    public interface IPlantUMLPartialDiagram
    {
        ITree<string> Tree { get; set; }
    }
}