using System.Reflection;

namespace System.Net.Http
{
	partial class HttpClientHandler : HttpMessageHandler
	{
		static IMonoHttpClientHandler CreateDefaultHandler ()
		{
			string envvar = Environment.GetEnvironmentVariable ("XA_HTTP_CLIENT_HANDLER_TYPE")?.Trim ();
			if (envvar?.StartsWith("System.Net.Http.MonoWebRequestHandler", StringComparison.InvariantCulture) == true)
			{
				Type monoWrhType = Type.GetType (envvar, false);
				if (monoWrhType != null)
					return (IMonoHttpClientHandler) Activator.CreateInstance (monoWrhType);
			}

			// Ignore other types of handlers here (e.g. AndroidHttpHandler) to keep the old behavior
			// and always create SocketsHttpHandler for code like this if MonoWebRequestHandler was not specified:
			//
			//    var handler = new HttpClientHandler { Credentials = ... };
			//    var httpClient = new HttpClient (handler);
			//
			// AndroidHttpHandler is used only when we use the parameterless ctor of HttpClient
			return new SocketsHttpHandler (); 
		}
	}
}
