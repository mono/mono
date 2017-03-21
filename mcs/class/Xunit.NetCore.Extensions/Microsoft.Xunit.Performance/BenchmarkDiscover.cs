using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.Abstractions;

namespace Xunit.NetCore.Extensions
{
    public class BenchmarkDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Benchmark", "True");
        }
    }
}
