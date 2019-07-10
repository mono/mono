using System;
using System.Reflection;

namespace System.Net.Http {
	public partial class HttpClient {

#if MARTIN_FIXME && MONOTOUCH_WATCH
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
