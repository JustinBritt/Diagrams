// 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetDiagrams {
    internal class PlantUmlDiagramGenerator : RoslynDiagramGenerator
    {
        public PlantUmlDiagramGenerator(string solutionPath) : base(solutionPath)
        {
        }

        protected override IEnumerable<string> InternalGetDiagramNames()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> InternalGetDiagram(string diagramName)
        {
            throw new NotImplementedException();
        }

        protected override Task<DiagramResult> GenerateDiagrams()
        {
            throw new NotImplementedException();
        }
    }
}
