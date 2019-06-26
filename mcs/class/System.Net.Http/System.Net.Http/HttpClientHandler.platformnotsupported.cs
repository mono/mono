//
// System.Net.Http/HttpClientHandler.cs
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;

namespace System.Net.Http
{
	public class HttpClientHandler : HttpMessageHandler
	{
		const string EXCEPTION_MESSAGE = "System.Net.Http.HttpClientHandler is not supported on the current platform.";

		public HttpClientHandler ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool AllowAutoRedirect {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DecompressionMethods AutomaticDecompression {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ClientCertificateOption ClientCertificateOptions {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public CookieContainer CookieContainer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ICredentials Credentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaxAutomaticRedirections {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public long MaxRequestContentBufferSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool PreAuthenticate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IWebProxy Proxy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool SupportsAutomaticDecompression {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool SupportsProxy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool SupportsRedirectConfiguration {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UseCookies {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UseDefaultCredentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UseProxy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected override void Dispose (bool disposing)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		HttpResponseMessage CreateResponseMessage (HttpWebResponse wr, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected internal async override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool CheckCertificateRevocationList {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public X509CertificateCollection ClientCertificates {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ICredentials DefaultProxyCredentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaxConnectionsPerServer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaxResponseHeadersLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IDictionary<string,object> Properties {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Func<HttpRequestMessage,X509Certificate2,X509Chain,SslPolicyErrors,bool> ServerCertificateCustomValidationCallback {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SslProtocols SslProtocols {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		// Only used in MonoWebRequestHandler and ignored by the other handlers.
		internal void SetWebRequestTimeout (TimeSpan timeout) => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		// NS2.1:
		public static System.Func<System.Net.Http.HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator => throw new PlatformNotSupportedException ();
	}
}
