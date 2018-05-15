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
		ICertificateValidator2 certificateValidator;

		protected MobileTlsContext (MobileAuthenticatedStream parent, MonoSslAuthenticationOptions options)
		{
			Parent = parent;
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

			certificateValidator = CertificateValidationHelper.GetInternalValidator (
				parent.Settings, parent.Provider);
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

		protected string TargetHost {
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
		}

		internal abstract bool IsRemoteCertificateAvailable {
			get;
		}

		internal abstract X509Certificate LocalClientCertificate {
			get;
		}

		public abstract X509Certificate RemoteCertificate {
			get;
		}

		public abstract TlsProtocols NegotiatedProtocol {
			get;
		}

		public abstract void Flush ();

		public abstract (int ret, bool wantMore) Read (byte[] buffer, int offset, int count);

		public abstract (int ret, bool wantMore) Write (byte[] buffer, int offset, int count);

		public abstract void Shutdown ();

		protected bool ValidateCertificate (X509Certificate leaf, X509Chain chain)
		{
			var result = certificateValidator.ValidateCertificate (TargetHost, IsServer, leaf, chain);
			return result != null && result.Trusted && !result.UserDenied;
		}

		protected bool ValidateCertificate (X509CertificateCollection certificates)
		{
			var result = certificateValidator.ValidateCertificate (TargetHost, IsServer, certificates);
			return result != null && result.Trusted && !result.UserDenied;
		}

		protected X509Certificate SelectClientCertificate (X509Certificate serverCertificate, string[] acceptableIssuers)
		{
			X509Certificate certificate;
			var selected = certificateValidator.SelectClientCertificate (
				TargetHost, ClientCertificates, serverCertificate, acceptableIssuers, out certificate);
			if (selected)
				return certificate;

			if (ClientCertificates == null || ClientCertificates.Count == 0)
				return null;

			if (ClientCertificates.Count == 1)
				return ClientCertificates [0];

			// FIXME: select onne.
			throw new NotImplementedException ();
		}

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
