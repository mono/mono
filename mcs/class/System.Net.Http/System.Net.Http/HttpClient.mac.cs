using System;
using System.Reflection;

[assembly:System.Runtime.CompilerServices.InternalsVisibleTo ("Xamarin.Mac, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]

namespace System.Net.Http {
	public partial class HttpClient {

		public HttpClient ()
			: this (GetDefaultHandler (), true)
		{
		}

		// note: the linker will re-write ObjCRuntime.RuntimeOptions.GetHttpMessageHandler to return the correct type
		// unlike, XI where this method itself gets rewritten during linking
		static HttpMessageHandler GetDefaultHandler ()
		{
			Type type = Type.GetType("ObjCRuntime.RuntimeOptions, Xamarin.Mac");
			var method = type.GetMethod ("GetHttpMessageHandler", BindingFlags.Static | BindingFlags.NonPublic);
			return (HttpMessageHandler)method.Invoke (null, null);
		}
	}
}
