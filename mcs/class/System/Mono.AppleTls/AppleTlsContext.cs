//
// AppleTlsContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SSA = System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using Mono.Net;
using Mono.Net.Security;

using ObjCRuntimeInternal;

namespace Mono.AppleTls
{
	class AppleTlsContext : MobileTlsContext
	{
		public const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

		GCHandle handle;
		IntPtr context;

		SslReadFunc readFunc;
		SslWriteFunc writeFunc;

		SafeSecIdentityHandle serverIdentity;
		SafeSecIdentityHandle clientIdentity;

		X509Certificate2 remoteCertificate;
		X509Certificate localClientCertificate;
		MonoTlsConnectionInfo connectionInfo;
		bool isAuthenticated;
		bool handshakeFinished;
		bool renegotiating;
		int handshakeStarted;

		bool closed;
		bool disposed;
		bool closedGraceful;
		int pendingIO;

		Exception lastException;

		public AppleTlsContext (MobileAuthenticatedStream parent, MonoSslAuthenticationOptions options)
			: base (parent, options)
		{
			handle = GCHandle.Alloc (this, GCHandleType.Weak);
			readFunc = NativeReadCallback;
			writeFunc = NativeWriteCallback;
		}

		public IntPtr Handle {
			get {
				if (!HasContext)
					throw new ObjectDisposedException ("AppleTlsContext");
				return context;
			}
		}

		public override bool HasContext {
			get { return !disposed && context != IntPtr.Zero; }
		}

		void CheckStatusAndThrow (SslStatus status, params SslStatus[] acceptable)
		{
			var last = Interlocked.Exchange (ref lastException, null);
			if (last != null)
				throw last;

			if (status == SslStatus.Success || Array.IndexOf (acceptable, status) > -1)
				return;

			switch (status) {
			case SslStatus.ClosedAbort:
				throw new IOException ("Connection closed.");

			case SslStatus.BadCert:
				throw new TlsException (AlertDescription.BadCertificate);

			case SslStatus.UnknownRootCert:
			case SslStatus.NoRootCert:
			case SslStatus.XCertChainInvalid:
				throw new TlsException (AlertDescription.CertificateUnknown, status.ToString ());

			case SslStatus.CertExpired:
			case SslStatus.CertNotYetValid:
				throw new TlsException (AlertDescription.CertificateExpired);

			case SslStatus.Protocol:
				throw new TlsException (AlertDescription.ProtocolVersion);

			case SslStatus.PeerNoRenegotiation:
				throw new TlsException (AlertDescription.NoRenegotiation);

			case SslStatus.PeerUnexpectedMsg:
				throw new TlsException (AlertDescription.UnexpectedMessage);

			default:
				throw new TlsException (AlertDescription.InternalError, "Unknown Secure Transport error `{0}'.", status);
			}
		}

		#region Handshake

		public override bool IsAuthenticated {
			get { return isAuthenticated; }
		}

		public override void StartHandshake ()
		{
			Debug ("StartHandshake: {0}", IsServer);

			if (Interlocked.CompareExchange (ref handshakeStarted, 1, 1) != 0)
				throw new InvalidOperationException ();

			InitializeConnection ();

			/*
			 * SecureTransport is bugged OS X 10.5.8+ - renegotiation after
			 * calling SetCertificate() will not work.
			 *
			 * We also cannot change options after the handshake has started,
			 * so if you want to request a client certificate, it will happen
			 * both during the initial handshake and during renegotiation.
			 *
			 * You may check 'SslStream.IsAuthenticated' (which will be false
			 * during the initial handshake) from within your
			 * 'LocalCertificateSelectionCallback' and return null to have the
			 * callback invoked again during renegotiation.
			 *
			 * However, the first time your selection callback returns a client
			 * certificate, that certificate will be used for the rest of the
			 * session.
			 */

			SetSessionOption (SslSessionOption.BreakOnCertRequested, true);
			SetSessionOption (SslSessionOption.BreakOnClientAuth, true);
			SetSessionOption (SslSessionOption.BreakOnServerAuth, true);

			if (IsServer) {
				// If we have any of the server certificate selection callbacks, the we need to break on
				// the ClientHello to pass the requested server name to the callback.
				if (Options.ServerCertSelectionDelegate != null || Settings.ClientCertificateSelectionCallback != null)
					SetSessionOption (SslSessionOption.BreakOnClientHello, true);
				else if (LocalServerCertificate != null)
					SetServerIdentity (LocalServerCertificate);
				else
					throw new SSA.AuthenticationException (SR.net_ssl_io_no_server_cert);
			}
		}

		void SetServerIdentity (X509Certificate certificate)
		{
			using (var serverIdentity = AppleCertificateHelper.GetIdentity (certificate, out var intermediateCerts)) {
				if (serverIdentity.IsInvalid)
					throw new SSA.AuthenticationException ("Unable to get server certificate from keychain.");

				SetCertificate (serverIdentity, intermediateCerts);
				for (int i = 0; i < intermediateCerts.Length; i++)
					intermediateCerts [i].Dispose ();
			}
		}

		public override void FinishHandshake ()
		{
			InitializeSession ();

			isAuthenticated = true;
		}

		public override void Flush ()
		{
		}

		public override bool ProcessHandshake ()
		{
			if (handshakeFinished && !renegotiating)
				throw new NotSupportedException ("Handshake already finished.");

			while (true) {
				lastException = null;
				var status = SSLHandshake (Handle);
				Debug ("Handshake: {0} - {0:x}", status);

				CheckStatusAndThrow (status, SslStatus.WouldBlock, SslStatus.PeerAuthCompleted, SslStatus.PeerClientCertRequested, SslStatus.ClientHelloReceived);

				switch (status) {
				case SslStatus.PeerAuthCompleted:
					EvaluateTrust ();
					break;
				case SslStatus.PeerClientCertRequested:
					ClientCertificateRequested ();
					break;
				case SslStatus.ClientHelloReceived:
					ClientHelloReceived ();
					break;
				case SslStatus.WouldBlock:
					return false;
				case SslStatus.Success:
					Debug ("Handshake complete!");
					handshakeFinished = true;
					renegotiating = false;
					return true;
				}
			}
		}

		void ClientCertificateRequested ()
		{
			EvaluateTrust ();
			var acceptableIssuers = CopyDistinguishedNames ();
			localClientCertificate = SelectClientCertificate (acceptableIssuers);
			if (localClientCertificate == null)
				return;
			clientIdentity = AppleCertificateHelper.GetIdentity (localClientCertificate);
			if (clientIdentity.IsInvalid)
				throw new TlsException (AlertDescription.CertificateUnknown);
			SetCertificate (clientIdentity, new SafeSecCertificateHandle [0]);
		}

		void ClientHelloReceived ()
		{
			var peerName = GetRequestedPeerName ();
			Debug ($"Handshake - received ClientHello ({peerName})");
			SetServerIdentity (SelectServerCertificate (peerName));
		}

		void EvaluateTrust ()
		{
			InitializeSession ();

			/*
			 * We're using .NET's SslStream semantics here.
			 * 
			 * A server must always provide a valid certificate.
			 * 
			 * However, in server mode, "ask for client certificate" means that
			 * we ask the client to provide a certificate, then invoke the client
			 * certificate validator - passing 'null' if the client didn't provide
			 * any.
			 * 
			 */

			bool ok;
			SecTrust trust = null;
			X509Certificate2Collection certificates = null;

			try {
				trust = GetPeerTrust (!IsServer);

				if (trust == null || trust.Count == 0) {
					remoteCertificate = null;
					if (!IsServer)
						throw new TlsException (AlertDescription.CertificateUnknown);
					certificates = null;
				} else {
					if (trust.Count > 1)
						Debug ("WARNING: Got multiple certificates in SecTrust!");

					certificates = new X509Certificate2Collection ();
					for (int i = 0; i < trust.Count; i++)
						certificates.Add (trust.GetCertificate (i));

					remoteCertificate = new X509Certificate2 (certificates [0]);
					Debug ("Got peer trust: {0}", remoteCertificate);
				}

				ok = ValidateCertificate (certificates);
			} catch (Exception ex) {
				Debug ("Certificate validation failed: {0}", ex);
				throw;
			} finally {
				if (trust != null)
					trust.Dispose ();
				if (certificates != null) {
					for (int i = 0; i < certificates.Count; i++)
						certificates [i].Dispose ();
				}
			}

			if (!ok)
				throw new TlsException (AlertDescription.CertificateUnknown);
		}

		void InitializeConnection ()
		{
			context = SSLCreateContext (IntPtr.Zero, IsServer ? SslProtocolSide.Server : SslProtocolSide.Client, SslConnectionType.Stream);

			var result = SSLSetIOFuncs (Handle, readFunc, writeFunc);
			CheckStatusAndThrow (result);

			result = SSLSetConnection (Handle, GCHandle.ToIntPtr (handle));
			CheckStatusAndThrow (result);

			/*
			 * If 'EnabledProtocols' is zero, then we use the system default values.
			 *
			 * In CoreFX, 'ServicePointManager.SecurityProtocol' defaults to
			 * 'SecurityProtocolType.SystemDefault', which is zero.
			 */

			if ((EnabledProtocols & SSA.SslProtocols.Tls) != 0)
				MinProtocol = SslProtocol.Tls_1_0;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls11) != 0)
				MinProtocol = SslProtocol.Tls_1_1;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls12) != 0)
				MinProtocol = SslProtocol.Tls_1_2;

			if ((EnabledProtocols & SSA.SslProtocols.Tls12) != 0)
				MaxProtocol = SslProtocol.Tls_1_2;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls11) != 0)
				MaxProtocol = SslProtocol.Tls_1_1;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls) != 0)
				MaxProtocol = SslProtocol.Tls_1_0;

			if (Settings != null && Settings.EnabledCiphers != null) {
				SslCipherSuite [] ciphers = new SslCipherSuite [Settings.EnabledCiphers.Length];
				for (int i = 0 ; i < Settings.EnabledCiphers.Length; ++i)
					ciphers [i] = (SslCipherSuite)Settings.EnabledCiphers[i];
				SetEnabledCiphers (ciphers);
			}

			if (IsServer && AskForClientCertificate)
				SetClientSideAuthenticate (SslAuthenticate.Try);

			if (IsServer && Settings?.ClientCertificateIssuers != null) {
				Debug ("Set client certificate issuers.");
				foreach (var issuer in Settings.ClientCertificateIssuers) {
					AddDistinguishedName (issuer);
				}
			}

			IPAddress address;
			if (!IsServer && !string.IsNullOrEmpty (TargetHost) &&
			    !IPAddress.TryParse (TargetHost, out address)) {
				PeerDomainName = ServerName;
			}

			if (Options.AllowRenegotiation && IsRenegotiationSupported ())
				SetSessionOption (SslSessionOption.AllowRenegotiation, true);
		}

		static bool IsRenegotiationSupported ()
		{
#if MONOTOUCH
			return false;
#else
			return Environment.OSVersion.Version >= new Version (16, 6);
#endif
		}

		void InitializeSession ()
		{
			if (connectionInfo != null)
				return;

			var cipher = NegotiatedCipher;
			var protocol = GetNegotiatedProtocolVersion ();
			Debug ("GET CONNECTION INFO: {0:x}:{0} {1:x}:{1} {2}", cipher, protocol, (TlsProtocolCode)protocol);

			connectionInfo = new MonoTlsConnectionInfo {
				CipherSuiteCode = (CipherSuiteCode)cipher,
				ProtocolVersion = GetProtocol (protocol),
				PeerDomainName = PeerDomainName
			};
		}

		static TlsProtocols GetProtocol (SslProtocol protocol)
		{
			switch (protocol) {
			case SslProtocol.Tls_1_0:
				return TlsProtocols.Tls10;
			case SslProtocol.Tls_1_1:
				return TlsProtocols.Tls11;
			case SslProtocol.Tls_1_2:
				return TlsProtocols.Tls12;
			default:
				throw new NotSupportedException ();
			}
		}

		public override MonoTlsConnectionInfo ConnectionInfo {
			get { return connectionInfo; }
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
			get { return connectionInfo.ProtocolVersion; }
		}

		#endregion

		#region General P/Invokes

		[DllImport (SecurityLibrary )]
		extern static /* OSStatus */ SslStatus SSLGetProtocolVersionMax (/* SSLContextRef */ IntPtr context, out SslProtocol maxVersion);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetProtocolVersionMax (/* SSLContextRef */ IntPtr context, SslProtocol maxVersion);

		public SslProtocol MaxProtocol {
			get {
				SslProtocol value;
				var result = SSLGetProtocolVersionMax (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
			set {
				var result = SSLSetProtocolVersionMax (Handle, value);
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetProtocolVersionMin (/* SSLContextRef */ IntPtr context, out SslProtocol minVersion);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetProtocolVersionMin (/* SSLContextRef */ IntPtr context, SslProtocol minVersion);

		public SslProtocol MinProtocol {
			get {
				SslProtocol value;
				var result = SSLGetProtocolVersionMin (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
			set {
				var result = SSLSetProtocolVersionMin (Handle, value);
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetNegotiatedProtocolVersion (/* SSLContextRef */ IntPtr context, out SslProtocol protocol);

		public SslProtocol GetNegotiatedProtocolVersion ()
		{
			SslProtocol value;
			var result = SSLGetNegotiatedProtocolVersion (Handle, out value);
			CheckStatusAndThrow (result);
			return value;
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetSessionOption (/* SSLContextRef */ IntPtr context, SslSessionOption option, out bool value);

		public bool GetSessionOption (SslSessionOption option)
		{
			bool value;
			var result = SSLGetSessionOption (Handle, option, out value);
			CheckStatusAndThrow (result);
			return value;
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetSessionOption (/* SSLContextRef */ IntPtr context, SslSessionOption option, bool value);

		public void SetSessionOption (SslSessionOption option, bool value)
		{
			var result = SSLSetSessionOption (Handle, option, value);
			CheckStatusAndThrow (result);
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetClientSideAuthenticate (/* SSLContextRef */ IntPtr context, SslAuthenticate auth);

		public void SetClientSideAuthenticate (SslAuthenticate auth)
		{
			var result = SSLSetClientSideAuthenticate (Handle, auth);
			CheckStatusAndThrow (result);
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLHandshake (/* SSLContextRef */ IntPtr context);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetSessionState (/* SSLContextRef */ IntPtr context, ref SslSessionState state);

		public SslSessionState SessionState {
			get {
				var value = SslSessionState.Invalid;
				var result = SSLGetSessionState (Handle, ref value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		SslSessionState GetSessionState ()
		{
			var value = SslSessionState.Invalid;
			var result = SSLGetSessionState (Handle, ref value);
			return result == SslStatus.Success ? value : SslSessionState.Invalid;
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerID (/* SSLContextRef */ IntPtr context, /* const void** */ out IntPtr peerID, /* size_t* */ out IntPtr peerIDLen);

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetPeerID (/* SSLContextRef */ IntPtr context, /* const void* */ byte* peerID, /* size_t */ IntPtr peerIDLen);

		public unsafe byte[] PeerId {
			get {
				IntPtr length;
				IntPtr id;
				var result = SSLGetPeerID (Handle, out id, out length);
				CheckStatusAndThrow (result);
				if ((result != SslStatus.Success) || ((int)length == 0))
					return null;
				var data = new byte [(int)length];
				Marshal.Copy (id, data, 0, (int) length);
				return data;
			}
			set {
				SslStatus result;
				IntPtr length = (value == null) ? IntPtr.Zero : (IntPtr)value.Length;
				fixed (byte *p = value) {
					result = SSLSetPeerID (Handle, p, length);
				}
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetBufferedReadSize (/* SSLContextRef */ IntPtr context, /* size_t* */ out IntPtr bufSize);

		public IntPtr BufferedReadSize {
			get {
				IntPtr value;
				var result = SSLGetBufferedReadSize (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNumberSupportedCiphers (/* SSLContextRef */ IntPtr context, /* size_t* */ out IntPtr numCiphers);

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetSupportedCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t* */ ref IntPtr numCiphers);

		public unsafe IList<SslCipherSuite> GetSupportedCiphers ()
		{
			IntPtr n;
			var result = SSLGetNumberSupportedCiphers (Handle, out n);
			CheckStatusAndThrow (result);
			if ((result != SslStatus.Success) || ((int)n <= 0))
				return null;

			var ciphers = new SslCipherSuite [(int)n];
			fixed (SslCipherSuite *p = ciphers) {
				result = SSLGetSupportedCiphers (Handle, p, ref n);
			}
			CheckStatusAndThrow (result);
			return ciphers;
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNumberEnabledCiphers (/* SSLContextRef */ IntPtr context, /* size_t* */ out IntPtr numCiphers);

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetEnabledCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t* */ ref IntPtr numCiphers);

		public unsafe IList<SslCipherSuite> GetEnabledCiphers ()
		{
			IntPtr n;
			var result = SSLGetNumberEnabledCiphers (Handle, out n);
			CheckStatusAndThrow (result);
			if ((result != SslStatus.Success) || ((int)n <= 0))
				return null;

			var ciphers = new SslCipherSuite [(int)n];
			fixed (SslCipherSuite *p = ciphers) {
				result = SSLGetEnabledCiphers (Handle, p, ref n);
			}
			CheckStatusAndThrow (result);
			return ciphers;
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetEnabledCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t */ IntPtr numCiphers);

		public unsafe void SetEnabledCiphers (SslCipherSuite [] ciphers)
		{
			if (ciphers == null)
				throw new ArgumentNullException ("ciphers");

			SslStatus result;

			fixed (SslCipherSuite *p = ciphers)
				result = SSLSetEnabledCiphers (Handle, p, (IntPtr)ciphers.Length);
			CheckStatusAndThrow (result);
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNegotiatedCipher (/* SSLContextRef */ IntPtr context, /* SslCipherSuite* */ out SslCipherSuite cipherSuite);

		public SslCipherSuite NegotiatedCipher {
			get {
				SslCipherSuite value;
				var result = SSLGetNegotiatedCipher (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerDomainNameLength (/* SSLContextRef */ IntPtr context, /* size_t* */ out IntPtr peerNameLen);

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerDomainName (/* SSLContextRef */ IntPtr context, /* char* */ byte[] peerName, /* size_t */ ref IntPtr peerNameLen);

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetPeerDomainName (/* SSLContextRef */ IntPtr context, /* char* */ byte[] peerName, /* size_t */ IntPtr peerNameLen);

		public string PeerDomainName {
			get {
				IntPtr length;
				var result = SSLGetPeerDomainNameLength (Handle, out length);
				CheckStatusAndThrow (result);
				if (result != SslStatus.Success || (int)length == 0)
					return String.Empty;
				var bytes = new byte [(int)length];
				result = SSLGetPeerDomainName (Handle, bytes, ref length);
				CheckStatusAndThrow (result);

				int peerDomainLength = (int)length;

				if (result != SslStatus.Success)
					return string.Empty;
				if (peerDomainLength > 0 && bytes [peerDomainLength-1] == 0)
					peerDomainLength = peerDomainLength - 1;
				return Encoding.UTF8.GetString (bytes, 0, peerDomainLength);
			}
			set {
				SslStatus result;
				if (value == null) {
					result = SSLSetPeerDomainName (Handle, null, (IntPtr)0);
				} else {
					var bytes = Encoding.UTF8.GetBytes (value);
					result = SSLSetPeerDomainName (Handle, bytes, (IntPtr)bytes.Length);
				}
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLCopyRequestedPeerNameLength (/* SSLContextRef */ IntPtr context, /* size_t * */ out IntPtr peerNameLen);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLCopyRequestedPeerName (/* SSLContextRef */ IntPtr context, /* char * */ byte[] peerName, /* size_t * */ ref IntPtr peerNameLen);

		public string GetRequestedPeerName ()
		{
			IntPtr length;
			var result = SSLCopyRequestedPeerNameLength (Handle, out length);
			CheckStatusAndThrow (result);
			if (result != SslStatus.Success || (int)length == 0)
				return String.Empty;
			var bytes = new byte [(int)length];
			result = SSLCopyRequestedPeerName (Handle, bytes, ref length);
			CheckStatusAndThrow (result);

			int requestedPeerNameLength = (int)length;

			if (result != SslStatus.Success)
				return string.Empty;
			if (requestedPeerNameLength > 0 && bytes [requestedPeerNameLength-1] == 0)
				requestedPeerNameLength = requestedPeerNameLength - 1;
			return Encoding.UTF8.GetString (bytes, 0, requestedPeerNameLength);
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetCertificate (/* SSLContextRef */ IntPtr context, /* CFArrayRef */ IntPtr certRefs);

		CFArray Bundle (SafeSecIdentityHandle identity, IList<SafeSecCertificateHandle> certificates)
		{
			if (identity == null || identity.IsInvalid)
				throw new ArgumentNullException (nameof (identity));
			if (certificates == null)
				throw new ArgumentNullException (nameof (certificates));

			var ptrs = new IntPtr [certificates.Count + 1];
			ptrs [0] = identity.DangerousGetHandle ();
			for (int i = 0; i < certificates.Count; i++)
				ptrs [i + 1] = certificates [i].DangerousGetHandle ();
			return CFArray.CreateArray (ptrs);
		}

		void SetCertificate (SafeSecIdentityHandle identify, IList<SafeSecCertificateHandle> certificates)
		{
			using (var array = Bundle (identify, certificates)) {
				var result = SSLSetCertificate (Handle, array.Handle);
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetClientCertificateState (/* SSLContextRef */ IntPtr context, out SslClientCertificateState clientState);

		public SslClientCertificateState ClientCertificateState {
			get {
				SslClientCertificateState value;
				var result = SSLGetClientCertificateState (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLCopyPeerTrust (/* SSLContextRef */ IntPtr context, /* SecTrustRef */ out IntPtr trust);

		public SecTrust GetPeerTrust (bool requireTrust)
		{
			IntPtr value;
			var result = SSLCopyPeerTrust (Handle, out value);
			if (requireTrust) {
				CheckStatusAndThrow (result);
				if (value == IntPtr.Zero)
					throw new TlsException (AlertDescription.CertificateUnknown);
			}
			return (value == IntPtr.Zero) ? null : new SecTrust (value, true);
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLAddDistinguishedName (/* SSLContextRef */ IntPtr context, /* const void * */ byte[] derDN, /* size_t */ IntPtr derDNLen);

		void AddDistinguishedName (string name)
		{
			var dn = new X500DistinguishedName (name);
			var bytes = dn.RawData;
			var result = SSLAddDistinguishedName (Handle, bytes, (IntPtr)bytes.Length);
			CheckStatusAndThrow (result);
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLCopyDistinguishedNames (/* SSLContextRef */ IntPtr context, /* CFArrayRef  _Nullable * */ out IntPtr names);

		string[] CopyDistinguishedNames ()
		{
			IntPtr arrayPtr;
			var result = SSLCopyDistinguishedNames (Handle, out arrayPtr);
			CheckStatusAndThrow (result);

			if (arrayPtr == IntPtr.Zero)
				return new string[0];

			using (var array = new CFArray (arrayPtr, true)) {
				var names = new string [array.Count];
				for (int i = 0; i < array.Count; i++) {
					using (var data = new CFData (array[i], false)) {
						var buffer = new byte [(int)data.Length];
						Marshal.Copy (data.Bytes, buffer, 0, buffer.Length);
						var dn = new X500DistinguishedName (buffer);
						names[i] = dn.Name;
					}
				}
				return names;
			}
		}

		#endregion

		#region IO Functions

		[DllImport (SecurityLibrary)]
		extern static /* SSLContextRef */ IntPtr SSLCreateContext (/* CFAllocatorRef */ IntPtr alloc, SslProtocolSide protocolSide, SslConnectionType connectionType);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetConnection (/* SSLContextRef */ IntPtr context, /* SSLConnectionRef */ IntPtr connection);

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetIOFuncs (/* SSLContextRef */ IntPtr context, /* SSLReadFunc */ SslReadFunc readFunc, /* SSLWriteFunc */ SslWriteFunc writeFunc);

		[Mono.Util.MonoPInvokeCallback (typeof (SslReadFunc))]
		static SslStatus NativeReadCallback (IntPtr ptr, IntPtr data, ref IntPtr dataLength)
		{
			AppleTlsContext context = null;
			try {
				var weakHandle = GCHandle.FromIntPtr (ptr);
				if (!weakHandle.IsAllocated)
					return SslStatus.Internal;

				context = (AppleTlsContext) weakHandle.Target;
				if (context == null || context.disposed)
					return SslStatus.ClosedAbort;

				return context.NativeReadCallback (data, ref dataLength);
			} catch (Exception ex) {
				if (context != null && context.lastException == null)
					context.lastException = ex;
				return SslStatus.Internal;
			}
		}

		[Mono.Util.MonoPInvokeCallback (typeof (SslWriteFunc))]
		static SslStatus NativeWriteCallback (IntPtr ptr, IntPtr data, ref IntPtr dataLength)
		{
			AppleTlsContext context = null;
			try {
				var weakHandle = GCHandle.FromIntPtr (ptr);
				if (!weakHandle.IsAllocated)
					return SslStatus.Internal;

				context = (AppleTlsContext) weakHandle.Target;
				if (context == null || context.disposed)
					return SslStatus.ClosedAbort;

				return context.NativeWriteCallback (data, ref dataLength);
			} catch (Exception ex) {
				if (context != null && context.lastException == null)
					context.lastException = ex;
				return SslStatus.Internal;
			}
		}

		SslStatus NativeReadCallback (IntPtr data, ref IntPtr dataLength)
		{
			if (closed || disposed || Parent == null)
				return SslStatus.ClosedAbort;

			var len = (int)dataLength;
			var readBuffer = new byte [len];

			Debug ("NativeReadCallback: {0} {1}", dataLength, len);

			bool wantMore;
			var ret = Parent.InternalRead (readBuffer, 0, len, out wantMore);
			dataLength = (IntPtr)ret;

			Debug ("NativeReadCallback #1: {0} - {1} {2}", len, ret, wantMore);

			if (ret < 0)
				return SslStatus.ClosedAbort;

			Marshal.Copy (readBuffer, 0, data, ret);

			if (ret > 0)
				return SslStatus.Success;
			else if (wantMore)
				return SslStatus.WouldBlock;
			else if (ret == 0) {
				closedGraceful = true;
				return SslStatus.ClosedGraceful;
			} else {
				return SslStatus.Success;
			}
		}

		SslStatus NativeWriteCallback (IntPtr data, ref IntPtr dataLength)
		{
			if (closed || disposed || Parent == null)
				return SslStatus.ClosedAbort;

			var len = (int)dataLength;
			var writeBuffer = new byte [len];

			Marshal.Copy (data, writeBuffer, 0, len);

			Debug ("NativeWriteCallback: {0}", len);

			var ok = Parent.InternalWrite (writeBuffer, 0, len);

			Debug ("NativeWriteCallback done: {0} {1}", len, ok);

			return ok ? SslStatus.Success : SslStatus.ClosedAbort;
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLRead (/* SSLContextRef */ IntPtr context, /* const void* */ byte* data, /* size_t */ IntPtr dataLength, /* size_t* */ out IntPtr processed);

		public override unsafe (int ret, bool wantMore) Read (byte[] buffer, int offset, int count)
		{
			if (Interlocked.Exchange (ref pendingIO, 1) == 1)
				throw new InvalidOperationException ();

			Debug ("Read: {0},{1}", offset, count);

			lastException = null;

			try {
				IntPtr processed;
				SslStatus status;

				fixed (byte *d = &buffer [offset])
					status = SSLRead (Handle, d, (IntPtr)count, out processed);

				Debug ("Read done: {0} {1} {2}", status, count, processed);

				if (closedGraceful && (status == SslStatus.ClosedAbort || status == SslStatus.ClosedGraceful)) {
					/*
					 * This is really ugly, but unfortunately SSLRead() also returns 'SslStatus.ClosedAbort'
					 * when the first inner Read() returns 0.  MobileAuthenticatedStream.InnerRead() attempts
					 * to distinguish between a graceful close and abnormal termination of connection.
					 */
					return (0, false);
				}

				CheckStatusAndThrow (status, SslStatus.WouldBlock, SslStatus.ClosedGraceful,
				                     SslStatus.PeerAuthCompleted, SslStatus.PeerClientCertRequested);

				if (status == SslStatus.PeerAuthCompleted) {
					Debug ($"Renegotiation complete: {GetSessionState ()}");
					EvaluateTrust ();
					return (0, true);
				} else if (status == SslStatus.PeerClientCertRequested) {
					Debug ($"Renegotiation asked for client certificate: {GetSessionState ()}");
					ClientCertificateRequested ();
					return (0, true);
				}

				var wantMore = status == SslStatus.WouldBlock;
				return ((int)processed, wantMore);
			} catch (Exception ex) {
				Debug ("Read error: {0}", ex);
				throw;
			} finally {
				pendingIO = 0;
			}
		}

		[DllImport (SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLWrite (/* SSLContextRef */ IntPtr context, /* const void* */ byte* data, /* size_t */ IntPtr dataLength, /* size_t* */ out IntPtr processed);

		public override unsafe (int ret, bool wantMore) Write (byte[] buffer, int offset, int count)
		{
			if (Interlocked.Exchange (ref pendingIO, 1) == 1)
				throw new InvalidOperationException ();

			Debug ("Write: {0},{1}", offset, count);

			lastException = null;

			try {
				SslStatus status = SslStatus.ClosedAbort;
				IntPtr processed = (IntPtr)(-1);

				fixed (byte *d = &buffer [offset])
					status = SSLWrite (Handle, d, (IntPtr)count, out processed);

				Debug ("Write done: {0} {1}", status, processed);

				CheckStatusAndThrow (status, SslStatus.WouldBlock,
				                     SslStatus.PeerAuthCompleted, SslStatus.PeerClientCertRequested);

				if (status == SslStatus.PeerAuthCompleted) {
					Debug ($"Renegotiation complete: {GetSessionState ()}");
					EvaluateTrust ();
				} else if (status == SslStatus.PeerClientCertRequested) {
					Debug ($"Renegotiation asked for client certificate: {GetSessionState ()}");
					ClientCertificateRequested ();
				}

				var wantMore = status == SslStatus.WouldBlock;
				return ((int)processed, wantMore);
			} finally {
				pendingIO = 0;
			}
		}

#if !MONOTOUCH
		// Available on macOS 10.12+ and iOS 10.0+.
		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLReHandshake (/* SSLContextRef */ IntPtr context);
#endif

		public override bool CanRenegotiate => IsServer && IsRenegotiationSupported ();

		public override void Renegotiate ()
		{
#if MONOTOUCH
			throw new NotSupportedException ();
#else
			if (!CanRenegotiate)
				throw new NotSupportedException ();

			var status = SSLReHandshake (Handle);
			CheckStatusAndThrow (status);
			renegotiating = true;
#endif
		}

		[DllImport (SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLClose (/* SSLContextRef */ IntPtr context);

		public override void Shutdown ()
		{
			closed = true;
		}

		public override bool PendingRenegotiation ()
		{
			return GetSessionState () == SslSessionState.Handshake;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposed)
					return;
				if (disposing) {
					disposed = true;
					if (serverIdentity != null) {
						serverIdentity.Dispose ();
						serverIdentity = null;
					}
					if (clientIdentity != null) {
						clientIdentity.Dispose ();
						clientIdentity = null;
					}
					if (remoteCertificate != null) {
						remoteCertificate.Dispose ();
						remoteCertificate = null;
					}
				}
			} finally {
				disposed = true;
				if (context != IntPtr.Zero) {
					CFObject.CFRelease (context);
					context = IntPtr.Zero;
				}
				base.Dispose (disposing);
			}
		}
	}
}
