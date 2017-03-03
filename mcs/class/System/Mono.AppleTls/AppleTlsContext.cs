#if XAMARIN_APPLETLS
//
// AppleTlsContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SSA = System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using SD = System.Diagnostics;
using MX = Mono.Security.X509;
using Mono.Security.Interface;
using Mono.Net.Security;

using XamCore.Security;
using XamCore.Foundation;
using XamCore.CoreFoundation;
using XamCore.ObjCRuntime;

namespace XamCore.Security.Tls
{
	class AppleTlsContext : MobileTlsContext
	{
		GCHandle handle;
		IntPtr context;
		IntPtr connectionId;
		SslReadFunc readFunc;
		SslWriteFunc writeFunc;

		SecIdentity serverIdentity;
		SecIdentity clientIdentity;

		X509Certificate remoteCertificate;
		X509Certificate localClientCertificate;
		MonoTlsConnectionInfo connectionInfo;
		bool havePeerTrust;
		bool isAuthenticated;
		bool handshakeFinished;
		int handshakeStarted;

		bool closed;
		bool disposed;
		bool closedGraceful;
		int pendingIO;

		Exception lastException;

		public AppleTlsContext (
			MobileAuthenticatedStream parent, bool serverMode, string targetHost,
			SSA.SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
			: base (parent, serverMode, targetHost, enabledProtocols,
				serverCertificate, clientCertificates, askForClientCert)
		{
			handle = GCHandle.Alloc (this);
			connectionId = GCHandle.ToIntPtr (handle);
			readFunc = NativeReadCallback;
			writeFunc = NativeWriteCallback;

			if (IsServer) {
				if (serverCertificate == null)
					throw new ArgumentNullException ("serverCertificate");
			}
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

		[SD.Conditional ("MARTIN_DEBUG")]
		protected void Debug (string message, params object[] args)
		{
			Console.Error.WriteLine ("MobileTlsStream({0}): {1}", Parent.ID, string.Format (message, args));
		}

		void CheckStatusAndThrow (SslStatus status, params SslStatus[] acceptable)
		{
			var last = Interlocked.Exchange (ref lastException, null);
			if (last != null)
				throw last;

			if (status == SslStatus.Success || acceptable.Contains (status))
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

			default:
				throw new TlsException (AlertDescription.InternalError, "Unknown Secure Transport error `{0}'.", status);
			}
		}

		void SetException (string message, Exception exception)
		{
			var ioex = exception as IOException;
			if (ioex == null)
				ioex = new IOException (message, exception);
			exception = ioex;

			if (lastException == null) {
				lastException = exception;
				return;
			}

			var aggregate = lastException as AggregateException;
			if (aggregate != null) {
				var list = new List<Exception> (aggregate.InnerExceptions);
				list.Add (exception);
				lastException = new AggregateException (aggregate.Message, list);
			} else {
				lastException = new AggregateException (message, lastException, exception);
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

			SetSessionOption (SslSessionOption.BreakOnCertRequested, true);
			SetSessionOption (SslSessionOption.BreakOnClientAuth, true);
			SetSessionOption (SslSessionOption.BreakOnServerAuth, true);

			if (IsServer) {
				SecCertificate[] intermediateCerts;
				serverIdentity = AppleCertificateHelper.GetIdentity (LocalServerCertificate, out intermediateCerts);
				if (serverIdentity == null)
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
			if (handshakeFinished)
				throw new NotSupportedException ("Handshake already finished.");

			while (true) {
				lastException = null;
				var status = SSLHandshake (Handle);
				Debug ("Handshake: {0} - {0:x}", status);

				CheckStatusAndThrow (status, SslStatus.WouldBlock, SslStatus.PeerAuthCompleted, SslStatus.PeerClientCertRequested);

				if (status == SslStatus.PeerAuthCompleted) {
					RequirePeerTrust ();
				} else if (status == SslStatus.PeerClientCertRequested) {
					RequirePeerTrust ();
					if (remoteCertificate == null)
						throw new TlsException (AlertDescription.InternalError, "Cannot request client certificate before receiving one from the server.");
					localClientCertificate = SelectClientCertificate (remoteCertificate, null);
					if (localClientCertificate == null)
						continue;
					clientIdentity = AppleCertificateHelper.GetIdentity (localClientCertificate);
					if (clientIdentity == null)
						throw new TlsException (AlertDescription.CertificateUnknown);
					SetCertificate (clientIdentity, new SecCertificate [0]);
				} else if (status == SslStatus.WouldBlock) {
					return false;
				} else if (status == SslStatus.Success) {
					handshakeFinished = true;
					return true;
				}
			}
		}

		void RequirePeerTrust ()
		{
			if (!havePeerTrust) {
				EvaluateTrust ();
				havePeerTrust = true;
			}
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

			var trust = GetPeerTrust (!IsServer);
			X509CertificateCollection certificates;

			if (trust == null || trust.Count == 0) {
				remoteCertificate = null;
				if (!IsServer)
					throw new TlsException (AlertDescription.CertificateUnknown);
				certificates = null;
			} else {
				if (trust.Count > 1)
					Debug ("WARNING: Got multiple certificates in SecTrust!");

				certificates = new X509CertificateCollection ();
				for (int i = 0; i < trust.Count; i++)
					certificates.Add (trust [i].ToX509Certificate ());

				remoteCertificate = certificates [0];
				Debug ("Got peer trust: {0}", remoteCertificate);
			}

			bool ok;
			try {
				ok = ValidateCertificate (certificates);
			} catch (Exception ex) {
				Debug ("Certificate validation failed: {0}", ex);
				throw new TlsException (AlertDescription.CertificateUnknown, "Certificate validation threw exception.");
			}

			if (!ok)
				throw new TlsException (AlertDescription.CertificateUnknown);
		}

		void InitializeConnection ()
		{
			context = SSLCreateContext (IntPtr.Zero, IsServer ? SslProtocolSide.Server : SslProtocolSide.Client, SslConnectionType.Stream);

			var result = SSLSetIOFuncs (Handle, readFunc, writeFunc);
			CheckStatusAndThrow (result);

			result = SSLSetConnection (Handle, connectionId);
			CheckStatusAndThrow (result);

			if ((EnabledProtocols & SSA.SslProtocols.Tls) != 0)
				MinProtocol = SslProtocol.Tls_1_0;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls11) != 0)
				MinProtocol = SslProtocol.Tls_1_1;
			else
				MinProtocol = SslProtocol.Tls_1_2;

			if ((EnabledProtocols & SSA.SslProtocols.Tls12) != 0)
				MaxProtocol = SslProtocol.Tls_1_2;
			else if ((EnabledProtocols & SSA.SslProtocols.Tls11) != 0)
				MaxProtocol = SslProtocol.Tls_1_1;
			else
				MaxProtocol = SslProtocol.Tls_1_0;

			foreach (var c in GetSupportedCiphers ())
				Debug ("  {0} SslCipherSuite.{1} {2:x} {3}", IsServer ? "Server" : "Client", c, (int)c, (CipherSuiteCode)c);

			if (Settings != null && Settings.EnabledCiphers != null) {
				var ciphers = Settings.EnabledCiphers.Select (c => (SslCipherSuite)c).ToArray ();
				SetEnabledCiphers (ciphers);
			}

			if (AskForClientCertificate)
				SetClientSideAuthenticate (SslAuthenticate.Try);

			IPAddress address;
			if (!IsServer && !string.IsNullOrEmpty (TargetHost) &&
			    !IPAddress.TryParse (TargetHost, out address)) {
				PeerDomainName = ServerName;
			}
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

		public override X509Certificate RemoteCertificate {
			get { return remoteCertificate; }
		}

		public override TlsProtocols NegotiatedProtocol {
			get { return connectionInfo.ProtocolVersion; }
		}

		#endregion

		#region General P/Invokes

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetProtocolVersionMax (/* SSLContextRef */ IntPtr context, out SslProtocol maxVersion);

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
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

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetProtocolVersionMin (/* SSLContextRef */ IntPtr context, out SslProtocol minVersion);

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
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

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetNegotiatedProtocolVersion (/* SSLContextRef */ IntPtr context, out SslProtocol protocol);

		public SslProtocol GetNegotiatedProtocolVersion ()
		{
			SslProtocol value;
			var result = SSLGetNegotiatedProtocolVersion (Handle, out value);
			CheckStatusAndThrow (result);
			return value;
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetSessionOption (/* SSLContextRef */ IntPtr context, SslSessionOption option, out bool value);

		public bool GetSessionOption (SslSessionOption option)
		{
			bool value;
			var result = SSLGetSessionOption (Handle, option, out value);
			CheckStatusAndThrow (result);
			return value;
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetSessionOption (/* SSLContextRef */ IntPtr context, SslSessionOption option, bool value);

		public void SetSessionOption (SslSessionOption option, bool value)
		{
			var result = SSLSetSessionOption (Handle, option, value);
			CheckStatusAndThrow (result);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetClientSideAuthenticate (/* SSLContextRef */ IntPtr context, SslAuthenticate auth);

		public void SetClientSideAuthenticate (SslAuthenticate auth)
		{
			var result = SSLSetClientSideAuthenticate (Handle, auth);
			CheckStatusAndThrow (result);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLHandshake (/* SSLContextRef */ IntPtr context);

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLGetSessionState (/* SSLContextRef */ IntPtr context, ref SslSessionState state);

		public SslSessionState SessionState {
			get {
				var value = SslSessionState.Invalid;
				var result = SSLGetSessionState (Handle, ref value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerID (/* SSLContextRef */ IntPtr context, /* const void** */ out IntPtr peerID, /* size_t* */ out nint peerIDLen);

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetPeerID (/* SSLContextRef */ IntPtr context, /* const void* */ byte* peerID, /* size_t */ nint peerIDLen);

		public unsafe byte[] PeerId {
			get {
				nint length;
				IntPtr id;
				var result = SSLGetPeerID (Handle, out id, out length);
				CheckStatusAndThrow (result);
				if ((result != SslStatus.Success) || (length == 0))
					return null;
				var data = new byte [length];
				Marshal.Copy (id, data, 0, (int) length);
				return data;
			}
			set {
				SslStatus result;
				nint length = (value == null) ? 0 : value.Length;
				fixed (byte *p = value) {
					result = SSLSetPeerID (Handle, p, length);
				}
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetBufferedReadSize (/* SSLContextRef */ IntPtr context, /* size_t* */ out nint bufSize);

		public nint BufferedReadSize {
			get {
				nint value;
				var result = SSLGetBufferedReadSize (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNumberSupportedCiphers (/* SSLContextRef */ IntPtr context, /* size_t* */ out nint numCiphers);

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetSupportedCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t* */ ref nint numCiphers);

		public unsafe IList<SslCipherSuite> GetSupportedCiphers ()
		{
			nint n;
			var result = SSLGetNumberSupportedCiphers (Handle, out n);
			CheckStatusAndThrow (result);
			if ((result != SslStatus.Success) || (n <= 0))
				return null;

			var ciphers = new SslCipherSuite [n];
			fixed (SslCipherSuite *p = ciphers) {
				result = SSLGetSupportedCiphers (Handle, p, ref n);
			}
			CheckStatusAndThrow (result);
			return new List<SslCipherSuite> (ciphers);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNumberEnabledCiphers (/* SSLContextRef */ IntPtr context, /* size_t* */ out nint numCiphers);

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetEnabledCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t* */ ref nint numCiphers);

		public unsafe IList<SslCipherSuite> GetEnabledCiphers ()
		{
			nint n;
			var result = SSLGetNumberEnabledCiphers (Handle, out n);
			CheckStatusAndThrow (result);
			if ((result != SslStatus.Success) || (n <= 0))
				return null;

			var ciphers = new SslCipherSuite [n];
			fixed (SslCipherSuite *p = ciphers) {
				result = SSLGetEnabledCiphers (Handle, p, ref n);
			}
			CheckStatusAndThrow (result);
			return new List<SslCipherSuite> (ciphers);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetEnabledCiphers (/* SSLContextRef */ IntPtr context, SslCipherSuite *ciphers, /* size_t */ nint numCiphers);

		public unsafe void SetEnabledCiphers (IEnumerable<SslCipherSuite> ciphers)
		{
			if (ciphers == null)
				throw new ArgumentNullException ("ciphers");

			SslStatus result;
			var array = ciphers.ToArray ();
			fixed (SslCipherSuite *p = array)
				result = SSLSetEnabledCiphers (Handle, p, ciphers.Count ());
			CheckStatusAndThrow (result);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetNegotiatedCipher (/* SSLContextRef */ IntPtr context, /* SslCipherSuite* */ out SslCipherSuite cipherSuite);

		public SslCipherSuite NegotiatedCipher {
			get {
				SslCipherSuite value;
				var result = SSLGetNegotiatedCipher (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerDomainNameLength (/* SSLContextRef */ IntPtr context, /* size_t* */ out nint peerNameLen);

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetPeerDomainName (/* SSLContextRef */ IntPtr context, /* char* */ byte[] peerName, /* size_t */ ref nint peerNameLen);

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetPeerDomainName (/* SSLContextRef */ IntPtr context, /* char* */ byte[] peerName, /* size_t */ nint peerNameLen);

		public string PeerDomainName {
			get {
				nint length;
				var result = SSLGetPeerDomainNameLength (Handle, out length);
				CheckStatusAndThrow (result);
				if (result != SslStatus.Success || length == 0)
					return String.Empty;
				var bytes = new byte [length];
				result = SSLGetPeerDomainName (Handle, bytes, ref length);
				CheckStatusAndThrow (result);
				if (result != SslStatus.Success)
					return string.Empty;
				if (length > 0 && bytes [length-1] == 0)
					length--;
				return Encoding.UTF8.GetString (bytes, 0, (int)length);
			}
			set {
				SslStatus result;
				if (value == null) {
					result = SSLSetPeerDomainName (Handle, null, 0);
				} else {
					var bytes = Encoding.UTF8.GetBytes (value);
					result = SSLSetPeerDomainName (Handle, bytes, bytes.Length);
				}
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLSetCertificate (/* SSLContextRef */ IntPtr context, /* CFArrayRef */ IntPtr certRefs);

		NSArray Bundle (SecIdentity identity, IEnumerable<SecCertificate> certificates)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			int i = 0;
			int n = certificates == null ? 0 : certificates.Count ();
			var ptrs = new IntPtr [n + 1];
			ptrs [0] = identity.Handle;
			foreach (var certificate in certificates)
				ptrs [++i] = certificate.Handle;
			return NSArray.FromIntPtrs (ptrs);
		}

		public void SetCertificate (SecIdentity identify, IEnumerable<SecCertificate> certificates)
		{
			using (var array = Bundle (identify, certificates)) {
				var result = SSLSetCertificate (Handle, array.Handle);
				CheckStatusAndThrow (result);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLGetClientCertificateState (/* SSLContextRef */ IntPtr context, out SslClientCertificateState clientState);

		public SslClientCertificateState ClientCertificateState {
			get {
				SslClientCertificateState value;
				var result = SSLGetClientCertificateState (Handle, out value);
				CheckStatusAndThrow (result);
				return value;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
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
			return (value == IntPtr.Zero) ? null : new SecTrust (value);
		}

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* CFType */ IntPtr SSLContextGetTypeID ();

		[Mac (10,8)]
		public static IntPtr GetTypeId ()
		{
			return SSLContextGetTypeID ();
		}

		#endregion

		#region IO Functions

		[Mac (10,8)]
		[DllImport (Constants.SecurityLibrary)]
		extern static /* SSLContextRef */ IntPtr SSLCreateContext (/* CFAllocatorRef */ IntPtr alloc, SslProtocolSide protocolSide, SslConnectionType connectionType);

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetConnection (/* SSLContextRef */ IntPtr context, /* SSLConnectionRef */ IntPtr connection);

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLSetIOFuncs (/* SSLContextRef */ IntPtr context, /* SSLReadFunc */ SslReadFunc readFunc, /* SSLWriteFunc */ SslWriteFunc writeFunc);

		[MonoPInvokeCallback (typeof (SslReadFunc))]
		static SslStatus NativeReadCallback (IntPtr ptr, IntPtr data, ref nint dataLength)
		{
			var handle = GCHandle.FromIntPtr (ptr);
			if (!handle.IsAllocated)
				return SslStatus.Internal;

			var context = (AppleTlsContext) handle.Target;
			if (context.disposed)
				return SslStatus.ClosedAbort;

			try {
				return context.NativeReadCallback (data, ref dataLength);
			} catch (Exception ex) {
				if (context.lastException == null)
					context.lastException = ex;
				return SslStatus.Internal;
			}
		}

		[MonoPInvokeCallback (typeof (SslWriteFunc))]
		static SslStatus NativeWriteCallback (IntPtr ptr, IntPtr data, ref nint dataLength)
		{
			var handle = GCHandle.FromIntPtr (ptr);
			if (!handle.IsAllocated)
				return SslStatus.Internal;

			var context = (AppleTlsContext) handle.Target;
			if (context.disposed)
				return SslStatus.ClosedAbort;

			try {
				return context.NativeWriteCallback (data, ref dataLength);
			} catch (Exception ex) {
				if (context.lastException == null)
					context.lastException = ex;
				return SslStatus.Internal;
			}
		}

		SslStatus NativeReadCallback (IntPtr data, ref nint dataLength)
		{
			if (closed || disposed || Parent == null)
				return SslStatus.ClosedAbort;

			var len = (int)dataLength;
			var readBuffer = new byte [len];

			Debug ("NativeReadCallback: {0} {1}", dataLength, len);

			bool wantMore;
			var ret = Parent.InternalRead (readBuffer, 0, len, out wantMore);
			dataLength = ret;

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

		SslStatus NativeWriteCallback (IntPtr data, ref nint dataLength)
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

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLRead (/* SSLContextRef */ IntPtr context, /* const void* */ byte* data, /* size_t */ nint dataLength, /* size_t* */ out nint processed);

		public override unsafe int Read (byte[] buffer, int offset, int count, out bool wantMore)
		{
			if (Interlocked.Exchange (ref pendingIO, 1) == 1)
				throw new InvalidOperationException ();

			Debug ("Read: {0},{1}", offset, count);

			lastException = null;

			try {
				nint processed;
				SslStatus status;

				fixed (byte *d = &buffer [offset])
					status = SSLRead (Handle, d, count, out processed);

				Debug ("Read done: {0} {1} {2}", status, count, processed);

				if (closedGraceful && (status == SslStatus.ClosedAbort || status == SslStatus.ClosedGraceful)) {
					/*
					 * This is really ugly, but unfortunately SSLRead() also returns 'SslStatus.ClosedAbort'
					 * when the first inner Read() returns 0.  MobileAuthenticatedStream.InnerRead() attempts
					 * to distinguish between a graceful close and abnormal termination of connection.
					 */
					wantMore = false;
					return 0;
				}

				CheckStatusAndThrow (status, SslStatus.WouldBlock, SslStatus.ClosedGraceful);
				wantMore = status == SslStatus.WouldBlock;
				return (int)processed;
			} catch (Exception ex) {
				Debug ("Read error: {0}", ex);
				throw;
			} finally {
				pendingIO = 0;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern unsafe static /* OSStatus */ SslStatus SSLWrite (/* SSLContextRef */ IntPtr context, /* const void* */ byte* data, /* size_t */ nint dataLength, /* size_t* */ out nint processed);

		public override unsafe int Write (byte[] buffer, int offset, int count, out bool wantMore)
		{
			if (Interlocked.Exchange (ref pendingIO, 1) == 1)
				throw new InvalidOperationException ();

			Debug ("Write: {0},{1}", offset, count);

			lastException = null;

			try {
				SslStatus status = SslStatus.ClosedAbort;
				nint processed =  -1;

				fixed (byte *d = &buffer [offset])
					status = SSLWrite (Handle, d, count, out processed);

				Debug ("Write done: {0} {1}", status, processed);

				CheckStatusAndThrow (status, SslStatus.WouldBlock);

				wantMore = status == SslStatus.WouldBlock;
				return (int)processed;
			} finally {
				pendingIO = 0;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static /* OSStatus */ SslStatus SSLClose (/* SSLContextRef */ IntPtr context);

		public override void Close ()
		{
			if (Interlocked.Exchange (ref pendingIO, 1) == 1)
				throw new InvalidOperationException ();

			Debug ("Close");

			lastException = null;

			try {
				if (closed || disposed)
					return;

				var status = SSLClose (Handle);
				Debug ("Close done: {0}", status);
				CheckStatusAndThrow (status);
			} finally {
				closed = true;
				pendingIO = 0;
			}
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
#endif
