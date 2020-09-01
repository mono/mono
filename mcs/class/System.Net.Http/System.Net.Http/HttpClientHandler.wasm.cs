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
		System.Net.Http.WebAssemblyHttpHandler wasmHandler;

		public HttpClientHandler ()
		{
			wasmHandler = new System.Net.Http.WebAssemblyHttpHandler ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (wasmHandler != null) {
					wasmHandler.Dispose ();
					wasmHandler = null;
				}
			}
			base.Dispose (disposing);
		}

		public virtual bool SupportsAutomaticDecompression => false;

		public virtual bool SupportsProxy => false;

		public virtual bool SupportsRedirectConfiguration => false;

		public bool UseCookies {
			get => throw new PlatformNotSupportedException ("Property UseCookies is not supported.");
			set => throw new PlatformNotSupportedException ("Property UseCookies is not supported.");
		}

		public CookieContainer CookieContainer {
			get => throw new PlatformNotSupportedException ("Property CookieContainer is not supported.");
			set => throw new PlatformNotSupportedException ("Property CookieContainer is not supported.");
		}

		public ClientCertificateOption ClientCertificateOptions {
			get => throw new PlatformNotSupportedException ("Property ClientCertificateOptions is not supported.");
			set => throw new PlatformNotSupportedException ("Property ClientCertificateOptions is not supported.");
		}

		public X509CertificateCollection ClientCertificates {
			get => throw new PlatformNotSupportedException ("Property ClientCertificates is not supported.");
			set => throw new PlatformNotSupportedException ("Property ClientCertificates is not supported.");
		}

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback {
			get => throw new PlatformNotSupportedException ("Property ServerCertificateCustomValidationCallback is not supported.");
			set => throw new PlatformNotSupportedException ("Property ServerCertificateCustomValidationCallback is not supported.");
		}

		public bool CheckCertificateRevocationList {
			get => throw new PlatformNotSupportedException ("Property CheckCertificateRevocationList is not supported.");
			set => throw new PlatformNotSupportedException ("Property CheckCertificateRevocationList is not supported.");
		}

		public SslProtocols SslProtocols {
			get => throw new PlatformNotSupportedException ("Property SslProtocols is not supported.");
			set => throw new PlatformNotSupportedException ("Property SslProtocols is not supported.");
		}

		public DecompressionMethods AutomaticDecompression {
			get => throw new PlatformNotSupportedException ("Property AutomaticDecompression is not supported.");
			set => throw new PlatformNotSupportedException ("Property AutomaticDecompression is not supported.");
		}

		public bool UseProxy {
			get => throw new PlatformNotSupportedException ("Property UseProxy is not supported.");
			set => throw new PlatformNotSupportedException ("Property UseProxy is not supported.");
		}

		public IWebProxy Proxy {
			get => throw new PlatformNotSupportedException ("Property Proxy is not supported.");
			set => throw new PlatformNotSupportedException ("Property Proxy is not supported.");
		}

		public ICredentials DefaultProxyCredentials {
			get => throw new PlatformNotSupportedException ("Property DefaultProxyCredentials is not supported.");
			set => throw new PlatformNotSupportedException ("Property DefaultProxyCredentials is not supported.");
		}

		public bool PreAuthenticate {
			get => throw new PlatformNotSupportedException ("Property PreAuthenticate is not supported.");
			set => throw new PlatformNotSupportedException ("Property PreAuthenticate is not supported.");
		}

		public bool UseDefaultCredentials {
			get => throw new PlatformNotSupportedException ("Property UseDefaultCredentials is not supported.");
			set => throw new PlatformNotSupportedException ("Property UseDefaultCredentials is not supported.");
		}

		public ICredentials Credentials {
			get => throw new PlatformNotSupportedException ("Property Credentials is not supported.");
			set => throw new PlatformNotSupportedException ("Property Credentials is not supported.");
		}

		public bool AllowAutoRedirect {
			get => throw new PlatformNotSupportedException ("Property AllowAutoRedirect is not supported.");
			set => throw new PlatformNotSupportedException ("Property AllowAutoRedirect is not supported.");
		}

		public int MaxAutomaticRedirections {
			get => throw new PlatformNotSupportedException ("Property MaxAutomaticRedirections is not supported.");
			set => throw new PlatformNotSupportedException ("Property MaxAutomaticRedirections is not supported.");
		}

		public int MaxConnectionsPerServer {
			get => throw new PlatformNotSupportedException ("Property MaxConnectionsPerServer is not supported.");
			set => throw new PlatformNotSupportedException ("Property MaxConnectionsPerServer is not supported.");
		}

		public int MaxResponseHeadersLength {
			get => throw new PlatformNotSupportedException ("Property MaxResponseHeadersLength is not supported.");
			set => throw new PlatformNotSupportedException ("Property MaxResponseHeadersLength is not supported.");
		}

		public long MaxRequestContentBufferSize {
			get => throw new PlatformNotSupportedException ("Property MaxRequestContentBufferSize is not supported.");
			set => throw new PlatformNotSupportedException ("Property MaxRequestContentBufferSize is not supported.");
		}

		public IDictionary<string, object> Properties => throw new PlatformNotSupportedException ("Property Properties is not supported.");

		public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator { get; } = delegate { return true; };

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (wasmHandler == null)
				throw new ObjectDisposedException (GetType().ToString());

			return wasmHandler.SendAsync (request, cancellationToken);
		}
	}
}
