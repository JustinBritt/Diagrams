﻿namespace DotNetDiagrams.SequenceDiagrams.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public interface IPlantUMLSequenceDiagrams : IPlantUMLDiagrams
    {
        IPlantUMLSequenceDiagram GetSequenceDiagramAtTitleOrDefault(
            string title);
    }
}