namespace DotNetDiagrams.ClassDiagrams.Tests.Classes.Walkers
{
    using Microsoft.Build.Locator;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using DotNetDiagrams.ClassDiagrams.Classes.Walkers;

    [TestClass]
    public class PlantUMLClassDiagramCSharpSyntaxWalker
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            MSBuildLocator.RegisterDefaults();

            MSBuildWorkspace MSBuildWorkspace = MSBuildWorkspace.Create();

            string solutionPath = "";

            Solution solution = MSBuildWorkspace.OpenSolutionAsync(solutionPath).GetAwaiter().GetResult();

            Project project = null;

            Compilation compilation = null;

            SyntaxTree syntaxTree = null;

            DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
                compilation: compilation,
                syntaxTree: syntaxTree,
                solution: solution,
                project: project);

            // Act

            // Assert
        }
    }
}