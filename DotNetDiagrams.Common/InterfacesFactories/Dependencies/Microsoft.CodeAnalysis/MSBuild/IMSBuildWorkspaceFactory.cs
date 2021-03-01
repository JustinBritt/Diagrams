namespace DotNetDiagrams.Common.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using global::Microsoft.CodeAnalysis.MSBuild;

    public interface IMSBuildWorkspaceFactory
    {
        MSBuildWorkspace CreateAndRegisterDefaults();
    }
}