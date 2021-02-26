﻿namespace DotNetDiagrams.Interfaces.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Interfaces.Diagrams;

    public interface IDiagramGenerator
    {
        Dictionary<string, List<string>> Diagrams { get; }

        void Process();
    }
}