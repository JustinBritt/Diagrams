namespace DotNetDiagrams.Interfaces.Diagrams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IPlantUMLDiagrams
    {
        Dictionary<string, List<string>> Value { get; set; }
    }
}