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
		private const bool ActivateTracing = false;

		// Native UnityTls objects
		private UnityTls.unitytls_tlsctx*   m_TlsContext = null;

		// States and certificates
		X509Certificate       m_LocalClientCertificate;
		X509Certificate2      m_RemoteCertificate;
		MonoTlsConnectionInfo m_Connectioninfo;
		bool                  m_IsAuthenticated = false;
		bool                  m_HasContext = false;

		// Memory-buffer
		byte [] m_WriteBuffer;
		byte [] m_ReadBuffer;

		GCHandle m_handle;

		public UnityTlsContext (
			MobileAuthenticatedStream parent,
			bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
			: base (parent, serverMode, targetHost, enabledProtocols, serverCertificate, clientCertificates, askForClientCert)
		{
			// Need GCHandle to get a consistent pointer to this instance
			m_handle = GCHandle.Alloc (this);

			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();

			// Map selected protocols as best as we can.
			UnityTls.unitytls_tlsctx_protocolrange protocolRange = new UnityTls.unitytls_tlsctx_protocolrange {
				min = UnityTlsConversions.GetMinProtocol (enabledProtocols),
				max = UnityTlsConversions.GetMaxProtocol (enabledProtocols),
			};

			UnityTls.unitytls_tlsctx_callbacks callbacks = new UnityTls.unitytls_tlsctx_callbacks {
				write = WriteCallback,
				read = ReadCallback,
				data = (void*)(IntPtr)m_handle,
			};

			if (serverMode) {
				if (serverCertificate == null)
					throw new ArgumentNullException ("serverCertificate");
				X509Certificate2 serverCertificate2 = serverCertificate as X509Certificate2;
				if (serverCertificate2 == null || serverCertificate2.PrivateKey == null)
					throw new ArgumentException ("serverCertificate does not have a private key", "serverCertificate");


				UnityTls.unitytls_x509list* serverCerts = null;
				UnityTls.unitytls_key* serverPrivateKey = null;
				try {
					serverCerts = UnityTls.NativeInterface.unitytls_x509list_create (&errorState);
					CertHelper.AddCertificateToNativeChain (serverCerts, serverCertificate, &errorState);
					var serverCertsRef = UnityTls.NativeInterface.unitytls_x509list_get_ref (serverCerts, &errorState);

					byte[] privateKeyDer = PKCS8.PrivateKeyInfo.Encode (serverCertificate2.PrivateKey);
					fixed(byte* privateKeyDerPtr = privateKeyDer) {
						serverPrivateKey = UnityTls.NativeInterface.unitytls_key_parse_der (privateKeyDerPtr, privateKeyDer.Length, null, 0, &errorState);
					}
					var serverKeyRef = UnityTls.NativeInterface.unitytls_key_get_ref (serverPrivateKey, &errorState);

					Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to parse server key/certificate");

					m_TlsContext = UnityTls.NativeInterface.unitytls_tlsctx_create_server (protocolRange, callbacks, serverCertsRef, serverKeyRef, &errorState);
				} finally {
					UnityTls.NativeInterface.unitytls_x509list_free (serverCerts);
					UnityTls.NativeInterface.unitytls_key_free (serverPrivateKey);
				}
			}
			else {
				byte [] targetHostUtf8 = Encoding.UTF8.GetBytes (targetHost);
				fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
					m_TlsContext = UnityTls.NativeInterface.unitytls_tlsctx_create_client (protocolRange, callbacks, targetHostUtf8Ptr, targetHostUtf8.Length, &errorState);
				}
			}

			UnityTls.NativeInterface.unitytls_tlsctx_set_x509verify_callback (m_TlsContext, VerifyCallback, (void*)(IntPtr)m_handle, &errorState);

			Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to create UnityTls context");

			if (ActivateTracing) {
				UnityTls.NativeInterface.unitytls_tlsctx_set_trace_callback (m_TlsContext, TraceCallback, null, &errorState);
				Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to set trace callback");
			}

			m_HasContext = true;
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

		public override (int ret, bool wantMore) Read (byte[] buffer, int offset, int count)
		{
			bool wouldBlock = false;
			int numBytesRead = 0;

			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			fixed (byte* bufferPtr = buffer) {
				numBytesRead = UnityTls.NativeInterface.unitytls_tlsctx_read (m_TlsContext, bufferPtr + offset, count, &errorState);
			}

			if (errorState.code == UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK)
				wouldBlock = true;
			else
				Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to read data from TLS context");

			return (numBytesRead, wouldBlock);
		}

		public override (int ret, bool wantMore) Write (byte[] buffer, int offset, int count)
		{
			bool wouldBlock = false;
			int numBytesWritten = 0;

			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			fixed (byte* bufferPtr = buffer) {
				numBytesWritten = UnityTls.NativeInterface.unitytls_tlsctx_write (m_TlsContext, bufferPtr + offset, count, &errorState);
			}

			if (errorState.code == UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK)
				wouldBlock = true;
			else
				Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to write data to TLS context");

			return (numBytesWritten, wouldBlock);
		}

		public override void Shutdown ()
		{
			// Destroy native UnityTls objects
			UnityTls.NativeInterface.unitytls_tlsctx_free (m_TlsContext);
			m_TlsContext = null;

			m_HasContext = false;
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing)
				{
					Shutdown();

					// reset states
					m_LocalClientCertificate = null;
					m_RemoteCertificate = null;
					m_Connectioninfo = null;
					m_IsAuthenticated = false;
					m_HasContext = false;
				}

				m_handle.Free();

			} finally {
				base.Dispose (disposing);
			}
		}

		public override void StartHandshake ()
		{
			// TODO: Client->Server authentification is not supported by UnityTls as of writing
			if (IsServer && AskForClientCertificate) {
				throw new NotImplementedException ("No support for server-sided client certificate check yet.");
			}

			if (Settings != null && Settings.EnabledCiphers != null) {
				var ciphers = new UnityTls.unitytls_ciphersuite [Settings.EnabledCiphers.Length];
				for (int i = 0; i < ciphers.Length; i++)
					ciphers [i] = (UnityTls.unitytls_ciphersuite)Settings.EnabledCiphers [i];
				
				var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
				fixed (UnityTls.unitytls_ciphersuite* ciphersPtr = ciphers)
					UnityTls.NativeInterface.unitytls_tlsctx_set_supported_ciphersuites (m_TlsContext, ciphersPtr, ciphers.Length, &errorState);
				Unity.Debug.CheckAndThrow (errorState, "Failed to set list of supported ciphers", AlertDescription.HandshakeFailure);
			}
		}

		public override bool ProcessHandshake ()
		{
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			var result = UnityTls.NativeInterface.unitytls_tlsctx_process_handshake (m_TlsContext, &errorState);
			if (errorState.code == UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK)
				return false;

			// Not done is not an error if we are server and don't ask for ClientCertificate
			if (result == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_NOT_DONE && IsServer && !AskForClientCertificate)
				Unity.Debug.CheckAndThrow (errorState, "Handshake failed", AlertDescription.HandshakeFailure);
			else
				Unity.Debug.CheckAndThrow (errorState, result, "Handshake failed", AlertDescription.HandshakeFailure);

			// .Net implementation gives the server a verification callback (with null cert) even if AskForClientCertificate is false.
			// We stick to this behavior here.
			if (IsServer && !AskForClientCertificate) {
				if (!ValidateCertificate (null, null))
					throw new TlsException (AlertDescription.HandshakeFailure, "Verification failure during handshake");
			}

			return true;
		}

		public override void FinishHandshake ()
		{
			// Query some data. Ignore errors on the way since failure is not crucial.
			var cipherSuite = UnityTls.NativeInterface.unitytls_tlsctx_get_ciphersuite(m_TlsContext, null);
			var protocolVersion = UnityTls.NativeInterface.unitytls_tlsctx_get_protocol(m_TlsContext, null);

			m_Connectioninfo = new MonoTlsConnectionInfo () {
				CipherSuiteCode = (CipherSuiteCode)cipherSuite,
				ProtocolVersion = UnityTlsConversions.ConvertProtocolVersion(protocolVersion),
				PeerDomainName = ServerName

				// TODO:
				// The following properties can be deducted from CipherSuiteCode.
				// It looks though like as of writing no Mono implemention fills it out and there is also no mechanism that does that automatically
				//
				//CipherAlgorithmType
				//HashAlgorithmType
				//ExchangeAlgorithmType
			};
			m_IsAuthenticated = true;
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_write_callback))]
		static private size_t WriteCallback (void* userData, byte* data, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			var handle = (GCHandle)(IntPtr)userData;
			var context = (UnityTlsContext)handle.Target;
			return context.WriteCallback (data, bufferLen, errorState);
		}

		private size_t WriteCallback (byte* data, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			try {
				if (m_WriteBuffer == null || m_WriteBuffer.Length < bufferLen)
					m_WriteBuffer = new byte[bufferLen];
				Marshal.Copy ((IntPtr)data, m_WriteBuffer, 0, bufferLen);

				if (!Parent.InternalWrite (m_WriteBuffer, 0, bufferLen)) {
					UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_WRITE_FAILED);
					return 0;
				}

				return bufferLen;
			} catch { // handle all exceptions since we don't want to let them go through native code.
				UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_UNKNOWN_ERROR);
				return 0;
			}
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_read_callback))]
		static private size_t ReadCallback (void* userData, byte* buffer, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			var handle = (GCHandle)(IntPtr)userData;
			var context = (UnityTlsContext)handle.Target;
			return context.ReadCallback (buffer, bufferLen, errorState);
		}
		
		private size_t ReadCallback (byte* buffer, size_t bufferLen, UnityTls.unitytls_errorstate* errorState)
		{
			try {
				if (m_ReadBuffer == null || m_ReadBuffer.Length < bufferLen)
					m_ReadBuffer = new byte [bufferLen];

				bool wouldBlock;
				int numBytesRead = Parent.InternalRead (m_ReadBuffer, 0, bufferLen, out wouldBlock);
				if (wouldBlock) {
					UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK);
					return 0;
				}
				if (numBytesRead < 0) {
					UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_READ_FAILED);
					return 0;
				}

				Marshal.Copy (m_ReadBuffer, 0, (IntPtr)buffer, bufferLen);
				return numBytesRead;
			} catch { // handle all exceptions since we don't want to let them go through native code.
				UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_UNKNOWN_ERROR);
				return 0;
			}
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_x509verify_callback))]
		static private UnityTls.unitytls_x509verify_result VerifyCallback (void* userData, UnityTls.unitytls_x509list_ref chain, UnityTls.unitytls_errorstate* errorState)
		{
			var handle = (GCHandle)(IntPtr)userData;
			var context = (UnityTlsContext)handle.Target;
			return context.VerifyCallback (chain, errorState);
		}

		private UnityTls.unitytls_x509verify_result VerifyCallback (UnityTls.unitytls_x509list_ref chain, UnityTls.unitytls_errorstate* errorState)
		{
			try {
				X509CertificateCollection certificates = CertHelper.NativeChainToManagedCollection (chain, errorState);

				if (ValidateCertificate (certificates))
					return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS;
				else
					return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_NOT_TRUSTED;
			} catch { // handle all exceptions since we don't want to let them go through native code.
				return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FATAL_ERROR;
			}
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_trace_callback))]
		static private void TraceCallback (void* userData, UnityTls.unitytls_tlsctx* ctx, byte* traceMessage, size_t traceMessageLen)
		{
			string message = Encoding.UTF8.GetString (traceMessage, traceMessageLen);
			System.Console.Write (message);
		}
	}
}
#endif