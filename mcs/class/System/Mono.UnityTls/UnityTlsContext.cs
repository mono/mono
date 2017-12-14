#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices; 
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MonoSecurity::Mono.Security.Cryptography;
#else
using Mono.Security.Interface;
using Mono.Security.Cryptography;
#endif

using Mono.Net.Security;
using Mono.Util;

namespace Mono.Unity
{
	unsafe internal class UnityTlsContext : MobileTlsContext
	{
		private const int MaxIOBufferSize = 16384;
		
		// Native UnityTls objects
		private UnityTls.unitytls_tlsctx*   m_TlsContext = null;
		private UnityTls.unitytls_x509list* m_ServerCerts = null;
		private UnityTls.unitytls_key*      m_ServerPrivateKey = null;

		// States and certificates
		X509Certificate       m_LocalClientCertificate;
		X509Certificate2      m_RemoteCertificate;
		MonoTlsConnectionInfo m_Connectioninfo;
		bool                  m_IsAuthenticated = false;
		bool                  m_HasContext = false;

		public UnityTlsContext (
			MobileAuthenticatedStream parent,
			bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
			: base (parent, serverMode, targetHost, enabledProtocols, serverCertificate, clientCertificates, askForClientCert)
		{
			UnityTls.unitytls_errorstate errorState = UnityTls.unitytls_errorstate_create();

			// Map selected protocols as best as we can.
			UnityTls.unitytls_tlsctx_protocolrange protocolRange = new UnityTls.unitytls_tlsctx_protocolrange {
				min = GetMinProtocol(enabledProtocols),
				max = GetMaxProtocol(enabledProtocols),
			};

			UnityTls.unitytls_tlsctx_callbacks callbacks = new UnityTls.unitytls_tlsctx_callbacks {
				write = WriteCallback,
				read = ReadCallback,
				data = null,
			};

			if (serverMode) {
				if (serverCertificate == null)
					throw new ArgumentNullException ("serverCertificate");
				X509Certificate2 privateKeyCert = serverCertificate as X509Certificate2;
				if (privateKeyCert == null || privateKeyCert.PrivateKey == null)
					throw new ArgumentException ("serverCertificate does not have a private key", "serverCertificate");

				m_ServerCerts = UnityTls.unitytls_x509list_create(&errorState);
				byte[] certDer = serverCertificate.GetRawCertData();
				fixed(byte* certDerPtr = certDer) {
					UnityTls.unitytls_x509list_append_der(m_ServerCerts, certDerPtr, certDer.Length, &errorState);
				}

				byte[] privateKeyDer = PKCS8.PrivateKeyInfo.Encode(privateKeyCert.PrivateKey);
				fixed(byte* privateKeyDerPtr = privateKeyDer) {
					m_ServerPrivateKey = UnityTls.unitytls_key_parse_der(privateKeyDerPtr, privateKeyDer.Length, null, 0, &errorState);
				}

				Mono.Unity.Debug.CheckAndThrow(errorState, "Failed to parse server key/certificate");

				UnityTls.unitytls_x509list_ref serverCertsRef = UnityTls.unitytls_x509list_get_ref(m_ServerCerts, &errorState);
				UnityTls.unitytls_key_ref serverKeyRef = UnityTls.unitytls_key_get_ref(m_ServerPrivateKey, &errorState);
				m_TlsContext = UnityTls.unitytls_tlsctx_create_server(protocolRange, callbacks, serverCertsRef, serverKeyRef, &errorState);
			}
			else {
				byte[] targetHostUtf8 = Encoding.UTF8.GetBytes(targetHost);
				fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
					m_TlsContext = UnityTls.unitytls_tlsctx_create_client(protocolRange, callbacks, targetHostUtf8Ptr, targetHostUtf8.Length, &errorState);
				}
			}

			Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to create UnityTls context");
			m_HasContext = true;
		}

		static private UnityTls.unitytls_protocol GetMinProtocol(SslProtocols protocols)
		{
			if (protocols.HasFlag(SslProtocols.Tls))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;
			if (protocols.HasFlag(SslProtocols.Tls11))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_1;
			if (protocols.HasFlag(SslProtocols.Tls12))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;
			return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;	// Behavior as in AppleTlsContext
		}

		static private UnityTls.unitytls_protocol GetMaxProtocol(SslProtocols protocols)
		{
			if (protocols.HasFlag(SslProtocols.Tls12))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_2;
			if (protocols.HasFlag(SslProtocols.Tls11))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_1;
			if (protocols.HasFlag(SslProtocols.Tls))
				return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;
			return UnityTls.unitytls_protocol.UNITYTLS_PROTOCOL_TLS_1_0;	// Behavior as in AppleTlsContext
		}


		public override bool HasContext {
			get { return m_HasContext; }
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
					// Destroy native UnityTls objects
					UnityTls.unitytls_tlsctx_free(m_TlsContext);
					m_TlsContext = null;
					UnityTls.unitytls_x509list_free(m_ServerCerts);
					m_ServerCerts = null;
					UnityTls.unitytls_key_free(m_ServerPrivateKey);
					m_ServerPrivateKey = null;

					// reset states
					m_LocalClientCertificate = null;
					m_RemoteCertificate = null;
					m_Connectioninfo = null;
					m_IsAuthenticated = false;
					m_HasContext = false;
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

		private size_t WriteCallback(void* userData, byte* data, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			// TODO
			return 0;
		}

		private size_t ReadCallback(void* userData, byte* buffer, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			// TODO
			return 0;
		}
	}
}
#endif