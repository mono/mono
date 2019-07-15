using Xunit;

namespace System.Net.Http.Functional.Tests
{
	[SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework, "NetEventSource is only part of .NET Core.")]
	[SkipOnTargetFramework(TargetFrameworkMonikers.Mono, "NetEventSource is only part of .NET Core.")]
	public abstract class DiagnosticsTest : HttpClientTestBase
	{
	}
}
