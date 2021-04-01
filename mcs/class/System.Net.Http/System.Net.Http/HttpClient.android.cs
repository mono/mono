using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http {
	public partial class HttpClient {

		static HttpMessageHandler CreateDefaultHandler ()
		{
			string envvar = Environment.GetEnvironmentVariable ("XA_HTTP_CLIENT_HANDLER_TYPE")?.Trim ();

			if (string.IsNullOrEmpty (envvar))
				return new HttpClientHandler ();

			if (envvar?.StartsWith ("System.Net.Http.MonoWebRequestHandler", StringComparison.InvariantCulture) == true)
			{
				Type monoWrhType = Type.GetType (envvar, false);
				if (monoWrhType != null)
					return new HttpClientHandler ((IMonoHttpClientHandler) Activator.CreateInstance (monoWrhType));

				return new HttpClientHandler ();
			}

			Type handlerType = Type.GetType (envvar, false);
			if (handlerType == null && !envvar.Contains (", "))
			{
				// if assembly was not specified - look for it in Mono.Android too 
				// (e.g. AndroidHttpHandler is there)
				handlerType = Type.GetType (envvar + ", Mono.Android", false);
			}

			if (handlerType == null)
				return new HttpClientHandler ();

			if (Activator.CreateInstance (handlerType) is HttpMessageHandler msgHandler)
				return msgHandler;

			return new HttpClientHandler ();
		}
	}
}
