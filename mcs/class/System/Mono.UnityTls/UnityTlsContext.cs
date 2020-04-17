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

using Int8 = System.Byte;
using size_t = System.IntPtr;

namespace Mono.Unity
{
	unsafe internal class UnityTlsContext : MobileTlsContext
	{
		private const bool ActivateTracing = false;

		// Native UnityTls objects
		UnityTls.unitytls_tlsctx*   tlsContext = null;
		UnityTls.unitytls_x509list*	requestedClientCertChain = null;
		UnityTls.unitytls_key*		requestedClientKey = null;

		// Delegates we passed to native to ensure they are not garbage collected
		UnityTls.unitytls_tlsctx_read_callback readCallback;
		UnityTls.unitytls_tlsctx_write_callback writeCallback;
		UnityTls.unitytls_tlsctx_trace_callback traceCallback;
		UnityTls.unitytls_tlsctx_certificate_callback certificateCallback;
		UnityTls.unitytls_tlsctx_x509verify_callback verifyCallback;

		// States and certificates
		X509Certificate       localClientCertificate;
		X509Certificate2       remoteCertificate;
		MonoTlsConnectionInfo connectioninfo;
		bool                  isAuthenticated = false;
		bool                  hasContext = false;
		bool                  closedGraceful = false;

		// Memory-buffer
		byte [] writeBuffer;
		byte [] readBuffer;

		GCHandle handle;
		Exception lastException;

		public UnityTlsContext (MobileAuthenticatedStream parent, MonoSslAuthenticationOptions options)
			: base (parent, options)
		{
			// Need GCHandle to get a consistent pointer to this instance
			handle = GCHandle.Alloc (this);

			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();

			// Map selected protocols as best as we can.
			UnityTls.unitytls_tlsctx_protocolrange protocolRange = new UnityTls.unitytls_tlsctx_protocolrange {
				min = UnityTlsConversions.GetMinProtocol (options.EnabledSslProtocols),
				max = UnityTlsConversions.GetMaxProtocol (options.EnabledSslProtocols),
			};

			readCallback = ReadCallback;
			writeCallback = WriteCallback;
			UnityTls.unitytls_tlsctx_callbacks callbacks = new UnityTls.unitytls_tlsctx_callbacks {
				write = writeCallback,
				read = readCallback,
				data = (void*)(IntPtr)handle,
			};

			if (options.ServerMode) {
				ExtractNativeKeyAndChainFromManagedCertificate(options.ServerCertificate, &errorState, out var serverCerts, out var serverPrivateKey);
				try {
					var serverCertsRef = UnityTls.NativeInterface.unitytls_x509list_get_ref (serverCerts, &errorState);
					var serverKeyRef = UnityTls.NativeInterface.unitytls_key_get_ref (serverPrivateKey, &errorState);
					Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to parse server key/certificate");

					tlsContext = UnityTls.NativeInterface.unitytls_tlsctx_create_server (protocolRange, callbacks, serverCertsRef.handle, serverKeyRef.handle, &errorState);

					if (AskForClientCertificate) {
						UnityTls.unitytls_x509list* clientAuthCAList = null;
						try {
							clientAuthCAList = UnityTls.NativeInterface.unitytls_x509list_create (&errorState);
							var clientAuthCAListRef = UnityTls.NativeInterface.unitytls_x509list_get_ref (clientAuthCAList, &errorState);
							UnityTls.NativeInterface.unitytls_tlsctx_server_require_client_authentication (tlsContext, clientAuthCAListRef, &errorState);
						} finally {
							UnityTls.NativeInterface.unitytls_x509list_free (clientAuthCAList);
						}
					}
				} finally {
					UnityTls.NativeInterface.unitytls_x509list_free (serverCerts);
					UnityTls.NativeInterface.unitytls_key_free (serverPrivateKey);
				}
			}
			else {
				byte [] targetHostUtf8 = Encoding.UTF8.GetBytes (options.TargetHost);
				fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
					tlsContext = UnityTls.NativeInterface.unitytls_tlsctx_create_client (protocolRange, callbacks, targetHostUtf8Ptr, (size_t)targetHostUtf8.Length, &errorState);
				}

				certificateCallback = CertificateCallback;
				UnityTls.NativeInterface.unitytls_tlsctx_set_certificate_callback (tlsContext, certificateCallback, (void*)(IntPtr)handle, &errorState);				
			}

			verifyCallback = VerifyCallback;
			UnityTls.NativeInterface.unitytls_tlsctx_set_x509verify_callback (tlsContext, verifyCallback, (void*)(IntPtr)handle, &errorState);

			Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to create UnityTls context");
			
			#pragma warning disable CS0162 // Disable unreachable code warning
			if (ActivateTracing) {
				traceCallback = TraceCallback;
				UnityTls.NativeInterface.unitytls_tlsctx_set_trace_callback (tlsContext, traceCallback, null, &errorState);
				Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to set trace callback");
			}
			#pragma warning restore CS0162 // Reenable unreachable code warning.

			hasContext = true;
		}

		static private void ExtractNativeKeyAndChainFromManagedCertificate(X509Certificate cert, UnityTls.unitytls_errorstate* errorState, out UnityTls.unitytls_x509list* nativeCertChain, out UnityTls.unitytls_key* nativeKey)
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");
			X509Certificate2 cert2 = cert as X509Certificate2;
			if (cert2 == null || cert2.PrivateKey == null)
				throw new ArgumentException ("Certificate does not have a private key", "cert");

			nativeCertChain = null;
			nativeKey = null;
			try {
				nativeCertChain = UnityTls.NativeInterface.unitytls_x509list_create (errorState);
				CertHelper.AddCertificateToNativeChain (nativeCertChain, cert, errorState);

				byte[] privateKeyDer = PKCS8.PrivateKeyInfo.Encode (cert2.PrivateKey);
				fixed(byte* privateKeyDerPtr = privateKeyDer) {
					nativeKey = UnityTls.NativeInterface.unitytls_key_parse_der (privateKeyDerPtr, (size_t)privateKeyDer.Length, null, (size_t)0, errorState);
				}
			} catch {
				UnityTls.NativeInterface.unitytls_x509list_free (nativeCertChain);
				UnityTls.NativeInterface.unitytls_key_free (nativeKey);
				throw;
			}
		}

		public override bool HasContext {
			get { return hasContext; }
		}
		public override bool IsAuthenticated {
			get { return isAuthenticated; }
		}
		public override MonoTlsConnectionInfo ConnectionInfo {
			get { return connectioninfo; }
		}
		internal override bool IsRemoteCertificateAvailable {
			get { return remoteCertificate != null; }
		}
		internal override X509Certificate LocalClientCertificate {
			get { return localClientCertificate; }
		}
		public override X509Certificate2 RemoteCertificate {
			get { return remoteCertificate; }
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
			int numBytesRead = 0;

			lastException = null;
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			fixed (byte* bufferPtr = buffer) {
				numBytesRead = (int)UnityTls.NativeInterface.unitytls_tlsctx_read (tlsContext, bufferPtr + offset, (size_t)count, &errorState);
			}
			if (lastException != null)
				throw lastException;

			switch (errorState.code)
			{
				case UnityTls.unitytls_error_code.UNITYTLS_SUCCESS:
					// In contrast to some other APIs (like Apple security) WOULD_BLOCK is not set if we did a partial write.
					// The Mono Api however requires us to set the wantMore flag also if we didn't read all the data.
					return (numBytesRead, numBytesRead < count);
				
				case UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK:
					return (numBytesRead, true);
				
				case UnityTls.unitytls_error_code.UNITYTLS_STREAM_CLOSED:
					return (0, false);	// According to Apple and Btls implementation this is how we should handle gracefully closed connections.

				default:
					if (!closedGraceful) {
						Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to read data to TLS context");
					}
					return (0, false);
			}
		}

		public override (int ret, bool wantMore) Write (byte[] buffer, int offset, int count)
		{
			int numBytesWritten = 0;

			lastException = null;
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			fixed (byte* bufferPtr = buffer) {
				numBytesWritten = (int)UnityTls.NativeInterface.unitytls_tlsctx_write (tlsContext, bufferPtr + offset, (size_t)count, &errorState);
			}
			if (lastException != null)
				throw lastException;

			switch (errorState.code)
			{
				case UnityTls.unitytls_error_code.UNITYTLS_SUCCESS:
					// In contrast to some other APIs (like Apple security) WOULD_BLOCK is not set if we did a partial write.
					// The Mono Api however requires us to set the wantMore flag also if we didn't write all the data.
					return (numBytesWritten, numBytesWritten < count);
				
				case UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK:
					return (numBytesWritten, true);
				
				case UnityTls.unitytls_error_code.UNITYTLS_STREAM_CLOSED:
					return (0, false);	// According to Apple and Btls implementation this is how we should handle gracefully closed connections.

				default:
					Mono.Unity.Debug.CheckAndThrow (errorState, "Failed to write data to TLS context");
					return (0, false);
			}
		}

		// TODO: Find out if mbedtls supports renegotiation
		public override bool CanRenegotiate {
			get {
				return false;
			}
		}

		public override void Renegotiate ()
		{
			throw new NotSupportedException ();
		}

		public override bool PendingRenegotiation ()
		{
			return false;
		}

		public override void Shutdown ()
		{
			if(Settings != null && Settings.SendCloseNotify) {
				var err = UnityTls.NativeInterface.unitytls_errorstate_create ();
				UnityTls.NativeInterface.unitytls_tlsctx_notify_close (tlsContext, &err);
			}

			// Destroy native UnityTls objects
			UnityTls.NativeInterface.unitytls_x509list_free (requestedClientCertChain);
			UnityTls.NativeInterface.unitytls_key_free (requestedClientKey);
			UnityTls.NativeInterface.unitytls_tlsctx_free (tlsContext);
			tlsContext = null;

			hasContext = false;
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing)
				{
					Shutdown();

					// reset states
					localClientCertificate = null;
					remoteCertificate = null;

					if (localClientCertificate != null) {
						localClientCertificate.Dispose ();
						localClientCertificate = null;
					}
					if (remoteCertificate != null) {
						remoteCertificate.Dispose ();
						remoteCertificate = null;
					}

					connectioninfo = null;
					isAuthenticated = false;
					hasContext = false;
				}

				handle.Free();

			} finally {
				base.Dispose (disposing);
			}
		}

		public override void StartHandshake ()
		{
			if (Settings != null && Settings.EnabledCiphers != null) {
				var ciphers = new UnityTls.unitytls_ciphersuite [Settings.EnabledCiphers.Length];
				for (int i = 0; i < ciphers.Length; i++)
					ciphers [i] = (UnityTls.unitytls_ciphersuite)Settings.EnabledCiphers [i];
				
				var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
				fixed (UnityTls.unitytls_ciphersuite* ciphersPtr = ciphers)
					UnityTls.NativeInterface.unitytls_tlsctx_set_supported_ciphersuites (tlsContext, ciphersPtr, (size_t)ciphers.Length, &errorState);
				Unity.Debug.CheckAndThrow (errorState, "Failed to set list of supported ciphers", AlertDescription.HandshakeFailure);
			}
		}

		public override bool ProcessHandshake ()
		{
			lastException = null;
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			var result = UnityTls.NativeInterface.unitytls_tlsctx_process_handshake (tlsContext, &errorState);
			if (errorState.code == UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK)
				return false;
			if (lastException != null)
				throw lastException;

			// Not done is only an error if we are a client. Even servers with AskForClientCertificate should ignore it since .Net client authentification is always optional.
			if (IsServer && result == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_NOT_DONE) {
				Unity.Debug.CheckAndThrow (errorState, "Handshake failed", AlertDescription.HandshakeFailure);
				
				// .Net implementation gives the server a verification callback (with null cert) even if AskForClientCertificate is false.
				// We stick to this behavior here.
				if (!ValidateCertificate (null, null))
					throw new TlsException (AlertDescription.HandshakeFailure, "Verification failure during handshake");
			}
			else
				Unity.Debug.CheckAndThrow (errorState, result, "Handshake failed", AlertDescription.HandshakeFailure);

			return true;
		}

		public override void FinishHandshake ()
		{
			// Query some data. Ignore errors on the way since failure is not crucial.
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
			var cipherSuite = UnityTls.NativeInterface.unitytls_tlsctx_get_ciphersuite(tlsContext, &errorState);
			var protocolVersion = UnityTls.NativeInterface.unitytls_tlsctx_get_protocol(tlsContext, &errorState);

			connectioninfo = new MonoTlsConnectionInfo () {
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
			isAuthenticated = true;
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
				if (writeBuffer == null || writeBuffer.Length < (int)bufferLen)
					writeBuffer = new byte[(int)bufferLen];
				Marshal.Copy ((IntPtr)data, writeBuffer, 0, (int)bufferLen);

				if (!Parent.InternalWrite (writeBuffer, 0, (int)bufferLen)) {
					UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_WRITE_FAILED);
					return (size_t)0;
				}

				return bufferLen;
			} catch (Exception ex) { // handle all exceptions and store them for later since we don't want to let them go through native code.
				UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_UNKNOWN_ERROR);
				if (lastException == null)
					lastException = ex;
				return (size_t)0;
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
				if (readBuffer == null || readBuffer.Length < (int)bufferLen)
					readBuffer = new byte [(int)bufferLen];

				bool wouldBlock;
				int numBytesRead = Parent.InternalRead (readBuffer, 0, (int)bufferLen, out wouldBlock);

				// Non graceful exit.
				if (numBytesRead < 0) {
					UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_READ_FAILED);
				} else if (numBytesRead > 0) {
					Marshal.Copy (readBuffer, 0, (IntPtr)buffer, (int)bufferLen);
				} else  { // numBytesRead == 0
					// careful when rearranging this: wouldBlock might be true even if stream was closed abruptly. 
					if (wouldBlock) {
						UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_WOULD_BLOCK);
					} 
					// indicates graceful exit.
					// UnityTls only accepts an exit as gracful, if it was closed via a special TLS protocol message.
					// Both .Net and MobileTlsContext have a different idea of this concept though!
					else {
						closedGraceful = true;
						UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_READ_FAILED);
					}
				}

				// Note that UnityTls ignores this number when raising an error.
				return (size_t)numBytesRead;
			} catch (Exception ex) { // handle all exceptions and store them for later since we don't want to let them go through native code.
				UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_UNKNOWN_ERROR);
				if (lastException == null)
					lastException = ex;
				return (size_t)0;
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
				using (var chainImpl = new X509ChainImplUnityTls(chain))
				using (var managedChain = new X509Chain (chainImpl)) {
					remoteCertificate = managedChain.ChainElements[0].Certificate;

					// Note that the overload of this method that takes a X509CertificateCollection will not pass on the chain to 
					// user callbacks like ServicePointManager.ServerCertificateValidationCallback which can cause issues along the line.
					if (ValidateCertificate (remoteCertificate, managedChain))
						return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS;
					else
						return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FLAG_NOT_TRUSTED;
				}
			} catch (Exception ex) { // handle all exceptions and store them for later since we don't want to let them go through native code.
				if (lastException == null)
					lastException = ex;
				return UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_FATAL_ERROR;
			}
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_certificate_callback))]
		static private void CertificateCallback (void* userData, UnityTls.unitytls_tlsctx* ctx, Int8* cn, size_t cnLen, UnityTls.unitytls_x509name* caList, size_t caListLen, UnityTls.unitytls_x509list_ref* chain, UnityTls.unitytls_key_ref* key, UnityTls.unitytls_errorstate* errorState)
		{
			var handle = (GCHandle)(IntPtr)userData;
			var context = (UnityTlsContext)handle.Target;
			context.CertificateCallback (ctx, cn, cnLen, caList, caListLen, chain, key, errorState);
		}

		private void CertificateCallback (UnityTls.unitytls_tlsctx* ctx, Int8* cn, size_t cnLen, UnityTls.unitytls_x509name* caList, size_t caListLen, UnityTls.unitytls_x509list_ref* chain, UnityTls.unitytls_key_ref* key, UnityTls.unitytls_errorstate* errorState)
		{
			try { 
				if (remoteCertificate == null)
					throw new TlsException (AlertDescription.InternalError, "Cannot request client certificate before receiving one from the server.");
				
				localClientCertificate = SelectClientCertificate (null);
				
				if (localClientCertificate == null) {
					*chain = new UnityTls.unitytls_x509list_ref { handle = UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE };
					*key = new UnityTls.unitytls_key_ref { handle = UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE };
				} else {
					// Need to create native objects for client chain/key. Need to keep them cached.
					// Make sure we don't have old native objects still around.
					UnityTls.NativeInterface.unitytls_x509list_free (requestedClientCertChain);
					UnityTls.NativeInterface.unitytls_key_free (requestedClientKey);

					ExtractNativeKeyAndChainFromManagedCertificate(localClientCertificate, errorState, out requestedClientCertChain, out requestedClientKey);
					*chain = UnityTls.NativeInterface.unitytls_x509list_get_ref (requestedClientCertChain, errorState);
					*key = UnityTls.NativeInterface.unitytls_key_get_ref (requestedClientKey, errorState);
				}

				Unity.Debug.CheckAndThrow (*errorState, "Failed to retrieve certificates on request.", AlertDescription.HandshakeFailure);
			} catch (Exception ex) { // handle all exceptions and store them for later since we don't want to let them go through native code.
				UnityTls.NativeInterface.unitytls_errorstate_raise_error (errorState, UnityTls.unitytls_error_code.UNITYTLS_USER_UNKNOWN_ERROR);
				if (lastException == null)
					lastException = ex;
			}
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_tlsctx_trace_callback))]
		static private void TraceCallback (void* userData, UnityTls.unitytls_tlsctx* ctx, byte* traceMessage, size_t traceMessageLen)
		{
			string message = Encoding.UTF8.GetString (traceMessage, (int)traceMessageLen);
			System.Console.Write (message);
		}
	}
}
#endif