using System;
using System.Reflection;
using System.Net.Http;

namespace MonoTests.System.Net.Http
{
	static class HttpClientTestHelpers
	{
#if LEGACY_HTTPCLIENT
		internal static bool UsingSocketsHandler => false;
#else
		internal static bool UsingSocketsHandler => true;
#endif

		internal static bool IsSocketsHandler (HttpClientHandler handler) => UsingSocketsHandler;

		internal static HttpClient CreateHttpClient ()
		{
			return new HttpClient (CreateHttpClientHandler ());
		}

		internal static HttpClientHandler CreateHttpClientHandler ()
		{
			return new HttpClientHandler ();
		}
	}
}
