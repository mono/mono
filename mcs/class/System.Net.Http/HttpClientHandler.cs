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
		readonly IMonoHttpClientHandler _delegatingHandler;
		ClientCertificateOption _clientCertificateOptions;

		public HttpClientHandler () : this (CreateDefaultHandler ()) { }

		internal HttpClientHandler (IMonoHttpClientHandler handler)
		{
			_delegatingHandler = handler;
			ClientCertificateOptions = ClientCertificateOption.Manual;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				_delegatingHandler.Dispose ();
			}
			base.Dispose (disposing);
		}

		public virtual bool SupportsAutomaticDecompression => _delegatingHandler.SupportsAutomaticDecompression;

		public virtual bool SupportsProxy => true;

		public virtual bool SupportsRedirectConfiguration => true;

		public bool UseCookies {
			get => _delegatingHandler.UseCookies;
			set => _delegatingHandler.UseCookies = value;
		}

		public CookieContainer CookieContainer {
			get => _delegatingHandler.CookieContainer;
			set => _delegatingHandler.CookieContainer = value;
		}

		void ThrowForModifiedManagedSslOptionsIfStarted ()
		{
			// Hack to trigger an InvalidOperationException if a property that's stored on
			// SslOptions is changed, since SslOptions itself does not do any such checks.
			_delegatingHandler.SslOptions = _delegatingHandler.SslOptions;
		}

		public ClientCertificateOption ClientCertificateOptions {
			get {
				return _clientCertificateOptions;
			}
			set {
				switch (value) {
				case ClientCertificateOption.Manual:
					ThrowForModifiedManagedSslOptionsIfStarted ();
					_clientCertificateOptions = value;
					_delegatingHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate (ClientCertificates);
					break;

				case ClientCertificateOption.Automatic:
					ThrowForModifiedManagedSslOptionsIfStarted ();
					_clientCertificateOptions = value;
					_delegatingHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate ();
					break;

				default:
					throw new ArgumentOutOfRangeException (nameof (value));
				}
			}
		}

		public X509CertificateCollection ClientCertificates {
			get {
				if (ClientCertificateOptions != ClientCertificateOption.Manual) {
					throw new InvalidOperationException (SR.Format (SR.net_http_invalid_enable_first, nameof (ClientCertificateOptions), nameof (ClientCertificateOption.Manual)));
				}

				return _delegatingHandler.SslOptions.ClientCertificates ??
				    (_delegatingHandler.SslOptions.ClientCertificates = new X509CertificateCollection ());
			}
		}

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback {
			get => (_delegatingHandler.SslOptions.RemoteCertificateValidationCallback?.Target as ConnectHelper.CertificateCallbackMapper)?.FromHttpClientHandler;
			set {
				ThrowForModifiedManagedSslOptionsIfStarted ();
				_delegatingHandler.SslOptions.RemoteCertificateValidationCallback = value != null ?
				    new ConnectHelper.CertificateCallbackMapper (value).ForSocketsHttpHandler :
				    null;
			}
		}

		public bool CheckCertificateRevocationList {
			get => _delegatingHandler.SslOptions.CertificateRevocationCheckMode == X509RevocationMode.Online;
			set {
				ThrowForModifiedManagedSslOptionsIfStarted ();
				_delegatingHandler.SslOptions.CertificateRevocationCheckMode = value ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
			}
		}

		public SslProtocols SslProtocols {
			get => _delegatingHandler.SslOptions.EnabledSslProtocols;
			set {
				ThrowForModifiedManagedSslOptionsIfStarted ();
				_delegatingHandler.SslOptions.EnabledSslProtocols = value;
			}
		}

		public DecompressionMethods AutomaticDecompression {
			get => _delegatingHandler.AutomaticDecompression;
			set => _delegatingHandler.AutomaticDecompression = value;
		}

		public bool UseProxy {
			get => _delegatingHandler.UseProxy;
			set => _delegatingHandler.UseProxy = value;
		}

		public IWebProxy Proxy {
			get => _delegatingHandler.Proxy;
			set => _delegatingHandler.Proxy = value;
		}

		public ICredentials DefaultProxyCredentials {
			get => _delegatingHandler.DefaultProxyCredentials;
			set => _delegatingHandler.DefaultProxyCredentials = value;
		}

		public bool PreAuthenticate {
			get => _delegatingHandler.PreAuthenticate;
			set => _delegatingHandler.PreAuthenticate = value;
		}

		public bool UseDefaultCredentials {
			// Either read variable from curlHandler or compare .Credentials as socketsHttpHandler does not have separate prop.
			get => _delegatingHandler.Credentials == CredentialCache.DefaultCredentials;
			set {
				if (value) {
					_delegatingHandler.Credentials = CredentialCache.DefaultCredentials;
				} else {
					if (_delegatingHandler.Credentials == CredentialCache.DefaultCredentials) {
						// Only clear out the Credentials property if it was a DefaultCredentials.
						_delegatingHandler.Credentials = null;
					}
				}
			}
		}

		public ICredentials Credentials {
			get => _delegatingHandler.Credentials;
			set => _delegatingHandler.Credentials = value;
		}

		public bool AllowAutoRedirect {
			get => _delegatingHandler.AllowAutoRedirect;
			set => _delegatingHandler.AllowAutoRedirect = value;
		}

		public int MaxAutomaticRedirections {
			get => _delegatingHandler.MaxAutomaticRedirections;
			set => _delegatingHandler.MaxAutomaticRedirections = value;
		}

		public int MaxConnectionsPerServer {
			get => _delegatingHandler.MaxConnectionsPerServer;
			set => _delegatingHandler.MaxConnectionsPerServer = value;
		}

		public int MaxResponseHeadersLength {
			get => _delegatingHandler.MaxResponseHeadersLength;
			set => _delegatingHandler.MaxResponseHeadersLength = value;
		}

		public long MaxRequestContentBufferSize {
			get => _delegatingHandler.MaxRequestContentBufferSize;
			set => _delegatingHandler.MaxRequestContentBufferSize = value;
		}

		public IDictionary<string, object> Properties => _delegatingHandler.Properties;

		// Only used in MonoWebRequestHandler and ignored by the other handlers.
		internal void SetWebRequestTimeout (TimeSpan timeout)
		{
			_delegatingHandler.SetWebRequestTimeout (timeout);
		}

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken) =>
		    _delegatingHandler.SendAsync (request, cancellationToken);
	}
}
