// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
	public class HttpClientHandler : HttpMessageHandler
	{
		HttpMessageHandler platformHandler;

		public HttpClientHandler ()
		{
			// Ensure GetHttpMessageHandler does not recursively call new HttpClientHandler ()
			platformHandler = ObjCRuntime.RuntimeOptions.GetHttpMessageHandler ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (platformHandler != null) {
					platformHandler.Dispose ();
					platformHandler = null;
				}
			}
			base.Dispose (disposing);
		}

		public virtual bool SupportsAutomaticDecompression => false;

		public virtual bool SupportsProxy => false;

		public virtual bool SupportsRedirectConfiguration => false;

		public bool UseCookies {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public CookieContainer CookieContainer {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public ClientCertificateOption ClientCertificateOptions {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public X509CertificateCollection ClientCertificates {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool CheckCertificateRevocationList {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public SslProtocols SslProtocols {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public DecompressionMethods AutomaticDecompression {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool UseProxy {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public IWebProxy Proxy {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public ICredentials DefaultProxyCredentials {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool PreAuthenticate {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool UseDefaultCredentials {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public ICredentials Credentials {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool AllowAutoRedirect {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public int MaxAutomaticRedirections {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public int MaxConnectionsPerServer {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public int MaxResponseHeadersLength {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public long MaxRequestContentBufferSize {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public IDictionary<string, object> Properties => throw new PlatformNotSupportedException ();

		public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator { get; } = delegate { return true; };

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (platformHandler == null)
				throw new ObjectDisposedException (GetType().ToString());

			return platformHandler.SendAsync (request, cancellationToken);
		}
	}
}