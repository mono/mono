using System;
using System.Reflection;

namespace System.Net.Http {
	public partial class HttpClient {
		static HttpMessageHandler CreateDefaultHandler ()
		{
			return ObjCRuntime.RuntimeOptions.GetHttpMessageHandler ();
		}
	}
}
