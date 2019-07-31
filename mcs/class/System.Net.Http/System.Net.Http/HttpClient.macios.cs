using System;
using System.Reflection;

namespace System.Net.Http {
	public partial class HttpClient {

		public HttpClient ()
			: this (CreateDefaultHandler ())
		{
		}

		static HttpMessageHandler CreateDefaultHandler ()
		{
			return ObjCRuntime.RuntimeOptions.GetHttpMessageHandler ();
		}
	}
}
