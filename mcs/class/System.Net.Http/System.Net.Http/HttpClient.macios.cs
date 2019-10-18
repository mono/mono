using System;
using System.Reflection;

namespace System.Net.Http {
	public partial class HttpClient {
#if !MONOTOUCH_WATCH
		public HttpClient ()
			: this (CreateDefaultHandler ())
		{
		}
#endif

		static HttpMessageHandler CreateDefaultHandler ()
		{
			return ObjCRuntime.RuntimeOptions.GetHttpMessageHandler ();
		}
	}
}
