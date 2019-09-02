// 

using System.Collections.Generic;

namespace DotNetDiagrams {
    public interface IDiagramGenerator
    {
        IEnumerable<string> GetDiagramNames();
        IEnumerable<string> GetDiagram(string diagramName);
    }
}
