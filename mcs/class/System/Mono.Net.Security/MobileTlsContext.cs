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
		MobileAuthenticatedStream parent;
		bool serverMode;
		string targetHost;
		SslProtocols enabledProtocols;
		X509Certificate serverCertificate;
		X509CertificateCollection clientCertificates;
		bool askForClientCert;
		ICertificateValidator2 certificateValidator;

		public MobileTlsContext (
			MobileAuthenticatedStream parent, bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
		{
			this.parent = parent;
			this.serverMode = serverMode;
			this.targetHost = targetHost;
			this.enabledProtocols = enabledProtocols;
			this.serverCertificate = serverCertificate;
			this.clientCertificates = clientCertificates;
			this.askForClientCert = askForClientCert;

			certificateValidator = CertificateValidationHelper.GetInternalValidator (
				parent.Settings, parent.Provider);
		}

		internal MobileAuthenticatedStream Parent {
			get { return parent; }
		}

		public MonoTlsSettings Settings {
			get { return parent.Settings; }
		}

		public MonoTlsProvider Provider {
			get { return parent.Provider; }
		}

		[SD.Conditional ("MARTIN_DEBUG")]
		protected void Debug (string message, params object[] args)
		{
			Console.Error.WriteLine ("{0}: {1}", GetType ().Name, string.Format (message, args));
		}

		public abstract bool HasContext {
			get;
		}

		public abstract bool IsAuthenticated {
			get;
		}

		public bool IsServer {
			get { return serverMode; }
		}

		protected string TargetHost {
			get { return targetHost; }
		}

		protected bool AskForClientCertificate {
			get { return askForClientCert; }
		}

		protected SslProtocols EnabledProtocols {
			get { return enabledProtocols; }
		}

		protected X509CertificateCollection ClientCertificates {
			get { return clientCertificates; }
		}

		protected void GetProtocolVersions (out TlsProtocolCode min, out TlsProtocolCode max)
		{
			if ((enabledProtocols & SslProtocols.Tls) != 0)
				min = TlsProtocolCode.Tls10;
			else if ((enabledProtocols & SslProtocols.Tls11) != 0)
				min = TlsProtocolCode.Tls11;
			else
				min = TlsProtocolCode.Tls12;

			if ((enabledProtocols & SslProtocols.Tls12) != 0)
				max = TlsProtocolCode.Tls12;
			else if ((enabledProtocols & SslProtocols.Tls11) != 0)
				max = TlsProtocolCode.Tls11;
			else
				max = TlsProtocolCode.Tls10;
		}

		public abstract void StartHandshake ();

		public abstract bool ProcessHandshake ();

		public abstract void FinishHandshake ();

		public abstract MonoTlsConnectionInfo ConnectionInfo {
			get;
		}

		internal X509Certificate LocalServerCertificate {
			get { return serverCertificate; }
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

		public abstract int Read (byte[] buffer, int offset, int count, out bool wantMore);

		public abstract int Write (byte[] buffer, int offset, int count, out bool wantMore);

		public abstract void Close ();

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

			if (clientCertificates == null || clientCertificates.Count == 0)
				return null;

			if (clientCertificates.Count == 1)
				return clientCertificates [0];

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
