namespace DotNetDiagrams.ClassDiagrams.Interfaces.Generators
{
    using Microsoft.CodeAnalysis;

    using DotNetDiagrams.Common.Interfaces.Diagrams;

    public interface IClassDiagramGenerator
    {
        IDiagrams Process(
            Solution solution);
    }
}