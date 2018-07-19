//
// WinHttpHandler.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Http
{
	public class WinHttpHandler : HttpMessageHandler
	{
		public WinHttpHandler() { throw new PlatformNotSupportedException (); }

		public DecompressionMethods AutomaticDecompression { get { throw new PlatformNotSupportedException ();  } set { throw new PlatformNotSupportedException (); } }

		public bool AutomaticRedirection { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public bool CheckCertificateRevocationList { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public ClientCertificateOption ClientCertificateOption { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public X509Certificate2Collection ClientCertificates { get { throw new PlatformNotSupportedException (); } }

		public CookieContainer CookieContainer { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public CookieUsePolicy CookieUsePolicy { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public ICredentials DefaultProxyCredentials { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public int MaxAutomaticRedirections { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public int MaxConnectionsPerServer { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public int MaxResponseDrainSize { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public int MaxResponseHeadersLength { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public bool PreAuthenticate { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public IDictionary<string, object> Properties { get { throw new PlatformNotSupportedException (); } }

		public IWebProxy Proxy { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public TimeSpan ReceiveDataTimeout { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public TimeSpan ReceiveHeadersTimeout { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public TimeSpan SendTimeout { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateValidationCallback { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public ICredentials ServerCredentials { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public SslProtocols SslProtocols { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		public WindowsProxyUsePolicy WindowsProxyUsePolicy { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); } }

		protected override void Dispose (bool disposing) { }

		protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, Threading.CancellationToken cancellationToken) { throw new PlatformNotSupportedException (); }
	}
}