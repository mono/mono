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
	public partial class HttpClientHandler : HttpMessageHandler
	{
		HttpMessageHandler wasmHandler;

		public HttpClientHandler () : this (HttpClient.CreateDefaultHandler ()) { }

		HttpClientHandler (HttpMessageHandler wasmHandler)
		{
			this.wasmHandler = wasmHandler;
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

		const string EXCEPTION_MESSAGE = "System.Net.Http.HttpClientHandler is not supported on the current platform.";

		public virtual bool SupportsAutomaticDecompression => false;

		public virtual bool SupportsProxy => false;

		public virtual bool SupportsRedirectConfiguration => false;

		public bool UseCookies {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public CookieContainer CookieContainer {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public ClientCertificateOption ClientCertificateOptions {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public X509CertificateCollection ClientCertificates {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool CheckCertificateRevocationList {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SslProtocols SslProtocols {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public DecompressionMethods AutomaticDecompression {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool UseProxy {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IWebProxy Proxy {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public ICredentials DefaultProxyCredentials {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool PreAuthenticate {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool UseDefaultCredentials {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public ICredentials Credentials {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool AllowAutoRedirect {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int MaxAutomaticRedirections {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int MaxConnectionsPerServer {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int MaxResponseHeadersLength {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public long MaxRequestContentBufferSize {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IDictionary<string, object> Properties => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (wasmHandler == null)
				throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			return wasmHandler.SendAsync (request, cancellationToken);
		}
	}
}
