﻿namespace DotNetDiagrams.Classes.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetDiagrams.Interfaces.Diagrams;

    internal sealed class PlantUMLDiagrams : IPlantUMLDiagrams
    {
        public PlantUMLDiagrams()
        {
            this.Value = new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> Value { get; set; }
    }
}