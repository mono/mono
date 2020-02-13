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
				IMonoHttpClientHandler legacyHandler = CreateMonoWebRequestHandler ();
				if (legacyHandler != null)
					return legacyHandler;
				else
					Console.WriteLine ($"{envvar} type was not found.");
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

		internal static IMonoHttpClientHandler CreateMonoWebRequestHandler ()
		{
			Type monoWrhType = Type.GetType ("System.Net.Http.MonoWebRequestHandler", false);
			if (monoWrhType != null)
			{
				var bflags = BindingFlags.NonPublic | BindingFlags.Instance;
				return (IMonoHttpClientHandler) Activator.CreateInstance (monoWrhType, bflags, null, null, null);
			}
			return null;
		}
	}
}
