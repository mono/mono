using System;
using System.Threading;
using System.Reflection;
using System.Net.Http;

namespace MonoTests.System.Net.Http
{
	static class HttpClientTestHelpers
	{
		static bool initialized;
		static bool usingSocketsHandler;
		static object syncLock;

		internal static bool UsingSocketsHandler {
			get {
				LazyInitializer.EnsureInitialized (
					ref usingSocketsHandler, ref initialized, ref syncLock,
					() => typeof (HttpClient).Assembly.GetType ("System.Net.Http.SocketsHttpHandler") != null);
				return usingSocketsHandler;
			}
		}

		internal static bool IsSocketsHandler (HttpClientHandler handler) => UsingSocketsHandler;

		internal static HttpClient CreateHttpClientWithHttpClientHandler ()
		{
			return new HttpClient (CreateHttpClientHandler ());
		}

		internal static HttpClientHandler CreateHttpClientHandler ()
		{
			return new HttpClientHandler ();
		}
	}
}
