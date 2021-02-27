namespace DotNetDiagrams.Factories.Dependencies.Microsoft.CodeAnalysis.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using global::Microsoft.Build.Locator;
    using global::Microsoft.CodeAnalysis.MSBuild;

    using global::DotNetDiagrams.InterfacesFactories.Dependencies.Microsoft.CodeAnalysis.MSBuild;

    internal sealed class MSBuildWorkspaceFactory : IMSBuildWorkspaceFactory
    {
        public MSBuildWorkspaceFactory()
        {
        }

        public MSBuildWorkspace CreateAndRegisterDefaults()
        {
            MSBuildLocator.RegisterDefaults();

            return MSBuildWorkspace.Create();
        }
    }
}