﻿namespace DotNetDiagrams.Common.Factories.Dependencies.Microsoft.CodeAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using global::Microsoft.CodeAnalysis;
    using global::Microsoft.CodeAnalysis.MSBuild;

    using DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis;

    public sealed class SolutionFactory : ISolutionFactory
    {
        public SolutionFactory()
        {
        }

        public Solution Create(
            MSBuildWorkspace MSBuildWorkspace,
            string solutionPath)
        {
            return MSBuildWorkspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();
        }
    }
}