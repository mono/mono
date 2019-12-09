//
// MobileTlsContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using System;
using System.IO;
using SD = System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Net.Security
{
	abstract class MobileTlsContext : IDisposable
	{
		ChainValidationHelper certificateValidator;

		protected MobileTlsContext (MobileAuthenticatedStream parent, MonoSslAuthenticationOptions options)
		{
			Parent = parent;
			Options = options;
			IsServer = options.ServerMode;
			EnabledProtocols = options.EnabledSslProtocols;

			if (options.ServerMode) {
				LocalServerCertificate = options.ServerCertificate;
				AskForClientCertificate = options.ClientCertificateRequired;
			} else {
				ClientCertificates = options.ClientCertificates;
				TargetHost = options.TargetHost;
				ServerName = options.TargetHost;
				if (!string.IsNullOrEmpty (ServerName)) {
					var pos = ServerName.IndexOf (':');
					if (pos > 0)
						ServerName = ServerName.Substring (0, pos);
				}
			}

			certificateValidator = ChainValidationHelper.GetInternalValidator (
				parent.SslStream, parent.Provider, parent.Settings);
		}

		internal MonoSslAuthenticationOptions Options {
			get;
		}

		internal MobileAuthenticatedStream Parent {
			get;
		}

		public MonoTlsSettings Settings => Parent.Settings;

		public MonoTlsProvider Provider => Parent.Provider;

		[SD.Conditional ("MONO_TLS_DEBUG")]
		protected void Debug (string message, params object[] args)
		{
			Parent.Debug ("{0}: {1}", GetType ().Name, string.Format (message, args));
		}

		public abstract bool HasContext {
			get;
		}

		public abstract bool IsAuthenticated {
			get;
		}

		public bool IsServer {
			get;
		}

		internal string TargetHost {
			get;
		}

		protected string ServerName {
			get;
		}

		protected bool AskForClientCertificate {
			get;
		}

		protected SslProtocols EnabledProtocols {
			get;
		}

		protected X509CertificateCollection ClientCertificates {
			get;
		}

		internal bool AllowRenegotiation {
			get { return false; }
		}

		protected void GetProtocolVersions (out TlsProtocolCode? min, out TlsProtocolCode? max)
		{
			if ((EnabledProtocols & SslProtocols.Tls) != 0)
				min = TlsProtocolCode.Tls10;
			else if ((EnabledProtocols & SslProtocols.Tls11) != 0)
				min = TlsProtocolCode.Tls11;
			else if ((EnabledProtocols & SslProtocols.Tls12) != 0)
				min = TlsProtocolCode.Tls12;
			else
				min = null;

			if ((EnabledProtocols & SslProtocols.Tls12) != 0)
				max = TlsProtocolCode.Tls12;
			else if ((EnabledProtocols & SslProtocols.Tls11) != 0)
				max = TlsProtocolCode.Tls11;
			else if ((EnabledProtocols & SslProtocols.Tls) != 0)
				max = TlsProtocolCode.Tls10;
			else
				max = null;
		}

		public abstract void StartHandshake ();

		public abstract bool ProcessHandshake ();

		public abstract void FinishHandshake ();

		public abstract MonoTlsConnectionInfo ConnectionInfo {
			get;
		}

		internal X509Certificate LocalServerCertificate {
			get;
			private set;
		}

		internal abstract bool IsRemoteCertificateAvailable {
			get;
		}

		internal abstract X509Certificate LocalClientCertificate {
			get;
		}

		public abstract X509Certificate2 RemoteCertificate {
			get;
		}

		public abstract TlsProtocols NegotiatedProtocol {
			get;
		}

		public abstract void Flush ();

		public abstract (int ret, bool wantMore) Read (byte[] buffer, int offset, int count);

		public abstract (int ret, bool wantMore) Write (byte[] buffer, int offset, int count);

		public abstract void Shutdown ();

		public abstract bool PendingRenegotiation ();

		protected bool ValidateCertificate (X509Certificate2 leaf, X509Chain chain)
		{
			var result = certificateValidator.ValidateCertificate (TargetHost, IsServer, leaf, chain);
			return result != null && result.Trusted && !result.UserDenied;
		}

		protected bool ValidateCertificate (X509Certificate2Collection certificates)
		{
			var result = certificateValidator.ValidateCertificate (TargetHost, IsServer, certificates);
			return result != null && result.Trusted && !result.UserDenied;
		}

		protected X509Certificate SelectServerCertificate (string serverIdentity)
		{
			// There are three options for selecting the server certificate. When
			// selecting which to use, we prioritize the new ServerCertSelectionDelegate
			// API. If the new API isn't used we call LocalCertSelectionCallback (for compat
			// with .NET Framework), and if neither is set we fall back to using ServerCertificate.

			if (Options.ServerCertSelectionDelegate != null) {
				LocalServerCertificate = Options.ServerCertSelectionDelegate (serverIdentity);

				if (LocalServerCertificate == null)
					throw new AuthenticationException (SR.net_ssl_io_no_server_cert);
			} else if (Settings.ClientCertificateSelectionCallback != null) {
				var tempCollection = new X509CertificateCollection ();
				tempCollection.Add (Options.ServerCertificate);
				// We pass string.Empty here to maintain strict compatability with .NET Framework.
				LocalServerCertificate = Settings.ClientCertificateSelectionCallback (string.Empty, tempCollection, null, Array.Empty<string>());
			} else {
				LocalServerCertificate = Options.ServerCertificate;
			}

			if (LocalServerCertificate == null)
				throw new NotSupportedException (SR.net_ssl_io_no_server_cert);

			return LocalServerCertificate;
		}

		protected X509Certificate SelectClientCertificate (string[] acceptableIssuers)
		{
			if (Settings.DisallowUnauthenticatedCertificateRequest && !IsAuthenticated)
				return null;

			if (RemoteCertificate == null)
				throw new TlsException (AlertDescription.InternalError, "Cannot request client certificate before receiving one from the server.");

			/*
			 * We need to pass null to the user selection callback during the initial handshake, to allow the callback to distinguish
			 * between an authenticated and unauthenticated session.
			 */
			X509Certificate certificate;
			var selected = certificateValidator.SelectClientCertificate (
				TargetHost, ClientCertificates, IsAuthenticated ? RemoteCertificate : null, acceptableIssuers, out certificate);
			if (selected)
				return certificate;

			if (ClientCertificates == null || ClientCertificates.Count == 0)
				return null;

			/*
			 * .NET actually scans the entire collection to ensure the selected certificate has a private key in it.
			 *
			 * However, since we do not support private key retrieval from the key store, we require all certificates
			 * to have a private key in them (explicitly or implicitly via OS X keychain lookup).
			 */
			if (acceptableIssuers == null || acceptableIssuers.Length == 0)
				return ClientCertificates [0];

			// Copied from the referencesource implementation in referencesource/System/net/System/Net/_SecureChannel.cs.
			for (int i = 0; i < ClientCertificates.Count; i++) {
				var certificate2 = ClientCertificates[i] as X509Certificate2;
				if (certificate2 == null)
					continue;

				X509Chain chain = null;
				try {
					chain = new X509Chain ();
					chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreInvalidName;
					chain.Build (certificate2);

					//
					// We ignore any errors happened with chain.
					// Consider: try to locate the "best" client cert that has no errors and the lognest validity internal
					//
					if (chain.ChainElements.Count == 0)
						continue;
					for (int ii=0; ii< chain.ChainElements.Count; ++ii) {
						var issuer = chain.ChainElements[ii].Certificate.Issuer;
						if (Array.IndexOf (acceptableIssuers, issuer) != -1)
							return certificate2;
					}
				} catch {
					; // ignore errors
				} finally {
					if (chain != null)
						chain.Reset ();
				}
			}

			// No certificate matches.
			return null;
		}

		public abstract bool CanRenegotiate {
			get;
		}

		public abstract void Renegotiate ();

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		~MobileTlsContext ()
		{
			Dispose (false);
		}
	}
}

#endif
