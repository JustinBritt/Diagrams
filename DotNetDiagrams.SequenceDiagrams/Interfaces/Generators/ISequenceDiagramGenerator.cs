namespace DotNetDiagrams.SequenceDiagrams.Interfaces.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public interface ISequenceDiagramGenerator
    {
        IDiagrams Process(
            Solution solution);
    }
}