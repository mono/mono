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
				return GetFallback ($"XA_HTTP_CLIENT_HANDLER_TYPE is empty");

			Type handlerType = Type.GetType (envvar, false);
			if (handlerType == null && !envvar.Contains (", "))
			{
				// if assembly was not specified - look for it in Mono.Android too 
				// (e.g. AndroidHttpHandler is there)
				handlerType = Type.GetType (envvar + ", Mono.Android", false);
			}

			if (handlerType == null)
				return GetFallback ($"'{envvar}' type was not found");

			object handlerObj = Activator.CreateInstance (handlerType);

			if (handlerObj is MonoWebRequestHandler mwrh)
				return new HttpClientHandler (mwrh);

			if (handlerObj is HttpMessageHandler hmh)
				return hmh;

			return GetFallback ($"{handlerObj?.GetType ()} is not a valid HttpMessageHandler or MonoWebRequestHandler");
		}

		static HttpMessageHandler GetFallback (string message)
		{
			Console.WriteLine (message + ". Defaulting to System.Net.Http.HttpClientHandler");
			return new HttpClientHandler ();
		}
	}
}
