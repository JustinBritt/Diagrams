﻿namespace DotNetDiagrams.Interfaces.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Interfaces.Diagrams;

    public interface IDiagramGenerator
    {
        IDiagrams Process(
            Solution solution);
    }
}