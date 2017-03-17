using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Xunit.Performance
{
	[TraitDiscoverer("Microsoft.Xunit.Performance.BenchmarkDiscoverer", "Xunit.NetCore.Extensions")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class BenchmarkAttribute : Attribute, ITraitAttribute
	{
		public long InnerIterationCount { get; set; }
	}
}
