#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Runtime.InteropServices; 
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using Mono.Net.Security;
using Mono.Util;

namespace Mono.Unity
{
	unsafe internal class UnityTlsContext : MobileTlsContext
	{
		private const int MaxIOBufferSize = 16384;
		
		// States and certificates
		X509Certificate       m_LocalClientCertificate;
		X509Certificate2      m_RemoteCertificate;
		MonoTlsConnectionInfo m_Connectioninfo;
		bool                  m_IsAuthenticated = false;

		public UnityTlsContext (
			MobileAuthenticatedStream parent,
			bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
			: base (parent, serverMode, targetHost, enabledProtocols, serverCertificate, clientCertificates, askForClientCert)
		{
			// TODO
		}

		public override bool HasContext {
			get { return true; }
		}

		public override bool IsAuthenticated {
			get { return m_IsAuthenticated; }
		}

		public override MonoTlsConnectionInfo ConnectionInfo {
			get { return m_Connectioninfo; }
		}
		internal override bool IsRemoteCertificateAvailable {
			get { return m_RemoteCertificate != null; }
		}
		internal override X509Certificate LocalClientCertificate {
			get { return m_LocalClientCertificate; }
		}
		public override X509Certificate RemoteCertificate {
			get { return m_RemoteCertificate; }
		}
		public override TlsProtocols NegotiatedProtocol {
			get { return ConnectionInfo.ProtocolVersion; }
		}

		void SetPrivateCertificate (X509Certificate cert)
		{
			X509Certificate2 privateKeyCert = cert as X509Certificate2;
			if (privateKeyCert == null)
				return;
		}

		public override void Flush ()
		{
			// NO-OP
		}

		public override int Read (byte [] buffer, int offset, int count, out bool wouldBlock)
		{
			wouldBlock = false;

			int bufferSize = System.Math.Min(MaxIOBufferSize, count);
			// TODO
			return 0;
		}

		public override int Write (byte [] buffer, int offset, int count, out bool wouldBlock)
		{
			wouldBlock = false;

			int bufferSize = System.Math.Min(MaxIOBufferSize, count);
			// TODO
			return 0;
		}

		public override void Close ()
		{
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing)
				{
					// reset states
					m_LocalClientCertificate = null;
					m_RemoteCertificate = null;
					m_Connectioninfo = null;
					m_IsAuthenticated = false;
				}

			} finally {
				base.Dispose (disposing);
			}
		}

		public override void StartHandshake ()
		{
			// TODO
		}

		public override bool ProcessHandshake ()
		{
			// TODO
			return false;
		}

		public override void FinishHandshake ()
		{
			m_Connectioninfo = new MonoTlsConnectionInfo () {
				// TODO
				//CipherSuiteCode = ,
				//ProtocolVersion = ,
				PeerDomainName = ServerName
			};
			m_IsAuthenticated = true;
		}
	}
}
#endif