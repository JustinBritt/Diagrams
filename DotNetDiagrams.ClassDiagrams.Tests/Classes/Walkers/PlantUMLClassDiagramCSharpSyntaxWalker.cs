namespace DotNetDiagrams.ClassDiagrams.Tests.Classes.Walkers
{
    using System.Linq;

    using Microsoft.Build.Locator;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Editing;
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

            Project project = solution.Projects.Where(w => w.Language is LanguageNames.CSharp).First();

            Compilation compilation = project.GetCompilationAsync().GetAwaiter().GetResult();

            SyntaxTree syntaxTree = compilation.SyntaxTrees.First();

            DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
                compilation: compilation,
                syntaxTree: syntaxTree,
                solution: solution,
                project: project);

            // Act
            walker.Visit(syntaxTree.GetRoot());

            // Assert
            Assert.AreEqual(1, solution.Projects.Count());
        }
    }
}