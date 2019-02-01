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
		// Only one of these two handlers will be initialized.
		private readonly IMonoHttpClientHandler _monoHandler;
		private readonly SocketsHttpHandler _socketsHttpHandler;
		private ClientCertificateOption _clientCertificateOptions;

		public HttpClientHandler () : this (null) { }

		internal HttpClientHandler (IMonoHttpClientHandler monoHandler)
		{
			if (monoHandler != null) {
				_monoHandler = monoHandler;
			} else {
				_socketsHttpHandler = new SocketsHttpHandler ();
				ClientCertificateOptions = ClientCertificateOption.Manual;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				((IDisposable)_monoHandler ?? _socketsHttpHandler).Dispose ();
			}
			base.Dispose (disposing);
		}

		public virtual bool SupportsAutomaticDecompression => _monoHandler == null || _monoHandler.SupportsAutomaticDecompression;

		public virtual bool SupportsProxy => true;

		public virtual bool SupportsRedirectConfiguration => true;

		public bool UseCookies {
			get => _monoHandler != null ? _monoHandler.UseCookies : _socketsHttpHandler.UseCookies;
			set {
				if (_monoHandler != null) {
					_monoHandler.UseCookies = value;
				} else {
					_socketsHttpHandler.UseCookies = value;
				}
			}
		}

		public CookieContainer CookieContainer {
			get => _monoHandler != null ? _monoHandler.CookieContainer : _socketsHttpHandler.CookieContainer;
			set {
				if (_monoHandler != null) {
					_monoHandler.CookieContainer = value;
				} else {
					_socketsHttpHandler.CookieContainer = value;
				}
			}
		}

		public ClientCertificateOption ClientCertificateOptions {
			get {
				if (_monoHandler != null) {
					return _monoHandler.ClientCertificateOptions;
				} else {
					return _clientCertificateOptions;
				}
			}
			set {
				if (_monoHandler != null) {
					_monoHandler.ClientCertificateOptions = value;
				} else {
					switch (value) {
					case ClientCertificateOption.Manual:
						ThrowForModifiedManagedSslOptionsIfStarted ();
						_clientCertificateOptions = value;
						_socketsHttpHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate (ClientCertificates);
						break;

					case ClientCertificateOption.Automatic:
						ThrowForModifiedManagedSslOptionsIfStarted ();
						_clientCertificateOptions = value;
						_socketsHttpHandler.SslOptions.LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => CertificateHelper.GetEligibleClientCertificate ();
						break;

					default:
						throw new ArgumentOutOfRangeException (nameof (value));
					}
				}
			}
		}

		public X509CertificateCollection ClientCertificates {
			get {
				if (_monoHandler != null) {
					return _monoHandler.ClientCertificates;
				} else {
					if (ClientCertificateOptions != ClientCertificateOption.Manual) {
						throw new InvalidOperationException (SR.Format (SR.net_http_invalid_enable_first, nameof (ClientCertificateOptions), nameof (ClientCertificateOption.Manual)));
					}

					return _socketsHttpHandler.SslOptions.ClientCertificates ??
					    (_socketsHttpHandler.SslOptions.ClientCertificates = new X509CertificateCollection ());
				}
			}
		}

		public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback {
			get {
				return _monoHandler != null ?
				    _monoHandler.ServerCertificateCustomValidationCallback :
				    (_socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback?.Target as ConnectHelper.CertificateCallbackMapper)?.FromHttpClientHandler;
			}
			set {
				if (_monoHandler != null) {
					_monoHandler.ServerCertificateCustomValidationCallback = value;
				} else {
					ThrowForModifiedManagedSslOptionsIfStarted ();
					_socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = value != null ?
					    new ConnectHelper.CertificateCallbackMapper (value).ForSocketsHttpHandler :
					    null;
				}
			}
		}

		public bool CheckCertificateRevocationList {
			get => _monoHandler != null ? _monoHandler.CheckCertificateRevocationList : _socketsHttpHandler.SslOptions.CertificateRevocationCheckMode == X509RevocationMode.Online;
			set {
				if (_monoHandler != null) {
					_monoHandler.CheckCertificateRevocationList = value;
				} else {
					ThrowForModifiedManagedSslOptionsIfStarted ();
					_socketsHttpHandler.SslOptions.CertificateRevocationCheckMode = value ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
				}
			}
		}

		public SslProtocols SslProtocols {
			get => _monoHandler != null ? _monoHandler.SslProtocols : _socketsHttpHandler.SslOptions.EnabledSslProtocols;
			set {
				if (_monoHandler != null) {
					_monoHandler.SslProtocols = value;
				} else {
					ThrowForModifiedManagedSslOptionsIfStarted ();
					_socketsHttpHandler.SslOptions.EnabledSslProtocols = value;
				}
			}
		}

		public DecompressionMethods AutomaticDecompression {
			get => _monoHandler != null ? _monoHandler.AutomaticDecompression : _socketsHttpHandler.AutomaticDecompression;
			set {
				if (_monoHandler != null) {
					_monoHandler.AutomaticDecompression = value;
				} else {
					_socketsHttpHandler.AutomaticDecompression = value;
				}
			}
		}

		public bool UseProxy {
			get => _monoHandler != null ? _monoHandler.UseProxy : _socketsHttpHandler.UseProxy;
			set {
				if (_monoHandler != null) {
					_monoHandler.UseProxy = value;
				} else {
					_socketsHttpHandler.UseProxy = value;
				}
			}
		}

		public IWebProxy Proxy {
			get => _monoHandler != null ? _monoHandler.Proxy : _socketsHttpHandler.Proxy;
			set {
				if (_monoHandler != null) {
					_monoHandler.Proxy = value;
				} else {
					_socketsHttpHandler.Proxy = value;
				}
			}
		}

		public ICredentials DefaultProxyCredentials {
			get => _monoHandler != null ? _monoHandler.DefaultProxyCredentials : _socketsHttpHandler.DefaultProxyCredentials;
			set {
				if (_monoHandler != null) {
					_monoHandler.DefaultProxyCredentials = value;
				} else {
					_socketsHttpHandler.DefaultProxyCredentials = value;
				}
			}
		}

		public bool PreAuthenticate {
			get => _monoHandler != null ? _monoHandler.PreAuthenticate : _socketsHttpHandler.PreAuthenticate;
			set {
				if (_monoHandler != null) {
					_monoHandler.PreAuthenticate = value;
				} else {
					_socketsHttpHandler.PreAuthenticate = value;
				}
			}
		}

		public bool UseDefaultCredentials {
			// Either read variable from curlHandler or compare .Credentials as socketsHttpHandler does not have separate prop.
			get => _monoHandler != null ? _monoHandler.UseDefaultCredentials : _socketsHttpHandler.Credentials == CredentialCache.DefaultCredentials;
			set {
				if (_monoHandler != null) {
					_monoHandler.UseDefaultCredentials = value;
				} else {
					if (value) {
						_socketsHttpHandler.Credentials = CredentialCache.DefaultCredentials;
					} else {
						if (_socketsHttpHandler.Credentials == CredentialCache.DefaultCredentials) {
							// Only clear out the Credentials property if it was a DefaultCredentials.
							_socketsHttpHandler.Credentials = null;
						}
					}
				}
			}
		}

		public ICredentials Credentials {
			get => _monoHandler != null ? _monoHandler.Credentials : _socketsHttpHandler.Credentials;
			set {
				if (_monoHandler != null) {
					_monoHandler.Credentials = value;
				} else {
					_socketsHttpHandler.Credentials = value;
				}
			}
		}

		public bool AllowAutoRedirect {
			get => _monoHandler != null ? _monoHandler.AllowAutoRedirect : _socketsHttpHandler.AllowAutoRedirect;
			set {
				if (_monoHandler != null) {
					_monoHandler.AllowAutoRedirect = value;
				} else {
					_socketsHttpHandler.AllowAutoRedirect = value;
				}
			}
		}

		public int MaxAutomaticRedirections {
			get => _monoHandler != null ? _monoHandler.MaxAutomaticRedirections : _socketsHttpHandler.MaxAutomaticRedirections;
			set {
				if (_monoHandler != null) {
					_monoHandler.MaxAutomaticRedirections = value;
				} else {
					_socketsHttpHandler.MaxAutomaticRedirections = value;
				}
			}
		}

		public int MaxConnectionsPerServer {
			get => _monoHandler != null ? _monoHandler.MaxConnectionsPerServer : _socketsHttpHandler.MaxConnectionsPerServer;
			set {
				if (_monoHandler != null) {
					_monoHandler.MaxConnectionsPerServer = value;
				} else {
					_socketsHttpHandler.MaxConnectionsPerServer = value;
				}
			}
		}

		public int MaxResponseHeadersLength {
			get => _monoHandler != null ? _monoHandler.MaxResponseHeadersLength : _socketsHttpHandler.MaxResponseHeadersLength;
			set {
				if (_monoHandler != null) {
					_monoHandler.MaxResponseHeadersLength = value;
				} else {
					_socketsHttpHandler.MaxResponseHeadersLength = value;
				}
			}
		}

		public IDictionary<string, object> Properties => _monoHandler != null ?
		    _monoHandler.Properties :
		    _socketsHttpHandler.Properties;

		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken) =>
		    _monoHandler != null ? _monoHandler.SendAsync (request, cancellationToken) :
		    _socketsHttpHandler.SendAsync (request, cancellationToken);
	}
}
