namespace DotNetDiagrams.ClassDiagrams.Tests.Classes.Walkers
{
    using Microsoft.CodeAnalysis;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using DotNetDiagrams.ClassDiagrams.Classes.Walkers;

    [TestClass]
    public class PlantUMLClassDiagramCSharpSyntaxWalker
    {
        [TestMethod]
        public void TestMethod1()
        {
            Solution solution = null;

            Project project = null;

            Compilation compilation = null;

            SyntaxTree syntaxTree = null;

            // Arrange
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