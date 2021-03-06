﻿namespace DotNetDiagrams.Common.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDiagram
    {
        List<string> Code { get; set; }

        string Title { get; set; }
    }
}