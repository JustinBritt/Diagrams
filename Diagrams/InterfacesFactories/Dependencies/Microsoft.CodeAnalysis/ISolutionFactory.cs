namespace DotNetDiagrams.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using global::Microsoft.CodeAnalysis;
    using global::Microsoft.CodeAnalysis.MSBuild;

    public interface ISolutionFactory
    {
        Solution Create(
            MSBuildWorkspace MSBuildWorkspace,
            string solutionPath);
    }
}