using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http {
	public partial class HttpClient {

		static HttpMessageHandler CreateDefaultHandler ()
		{
			Type type = Type.GetType("Android.Runtime.AndroidEnvironment, Mono.Android");
			if (type == null)
				return GetFallback ("Invalid Mono.Android assembly? Cannot find Android.Runtime.AndroidEnvironment");

			MethodInfo method = type.GetMethod ("GetHttpMessageHandler", BindingFlags.Static | BindingFlags.NonPublic);
			if (method == null)
				return GetFallback ("Your Xamarin.Android version does not support obtaining of the custom HttpClientHandler");

			object ret = method.Invoke (null, null);
			if (ret == null)
				return GetFallback ("Xamarin.Android returned no custom HttpClientHandler");

			var handler = ret as HttpMessageHandler;
			if (handler == null)
				return GetFallback ($"{ret?.GetType()} is not a valid HttpMessageHandler");
			return handler;
		}

		static HttpMessageHandler GetFallback (string message)
		{
			Console.WriteLine (message + ". Defaulting to System.Net.Http.HttpClientHandler");
			return new HttpClientHandler ();
		}
	}
}
