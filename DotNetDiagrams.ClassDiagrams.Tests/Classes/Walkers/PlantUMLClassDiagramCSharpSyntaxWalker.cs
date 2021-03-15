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
            // Arrange
            DotNetDiagrams.ClassDiagrams.Classes.Walkers.PlantUMLClassDiagramCSharpSyntaxWalker walker = new(
                compilation: null,
                syntaxTree: null,
                solution: null,
                project: null);

            // Act

            // Assert
        }
    }
}