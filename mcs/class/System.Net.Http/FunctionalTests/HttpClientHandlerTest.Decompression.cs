using Xunit;

namespace System.Net.Http.Functional.Tests
{
	[SkipOnTargetFramework(TargetFrameworkMonikers.Mono, "We don't support BrotliStream yet.")]
	public abstract class HttpClientHandler_Decompression_Test : HttpClientTestBase
	{
	}
}
