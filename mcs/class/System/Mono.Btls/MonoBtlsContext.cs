//
// MonoBtlsContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Runtime.InteropServices;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using MNS = Mono.Net.Security;

namespace Mono.Btls
{
	class MonoBtlsContext : MNS.MobileTlsContext, IMonoBtlsBioMono
	{
		X509Certificate remoteCertificate;
		X509Certificate clientCertificate;
		X509CertificateImplBtls nativeServerCertificate;
		X509CertificateImplBtls nativeClientCertificate;
		MonoBtlsSslCtx ctx;
		MonoBtlsSsl ssl;
		MonoBtlsBio bio;
		MonoBtlsBio errbio;

		MonoTlsConnectionInfo connectionInfo;
		bool certificateValidated;
		bool isAuthenticated;
		bool connected;

		public MonoBtlsContext (
			MNS.MobileAuthenticatedStream parent,
			bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
			: base (parent, serverMode, targetHost, enabledProtocols,
			        serverCertificate, clientCertificates, askForClientCert)
		{
			if (serverMode)
				nativeServerCertificate = GetPrivateCertificate (serverCertificate);
		}

		static X509CertificateImplBtls GetPrivateCertificate (X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplBtls;
			if (impl != null)
				return (X509CertificateImplBtls)impl.Clone ();

			var password = Guid.NewGuid ().ToString ();
			var buffer = certificate.Export (X509ContentType.Pfx, password);

			impl = new X509CertificateImplBtls ();
			impl.Import (buffer, password, X509KeyStorageFlags.DefaultKeySet);
			return impl;
		}

		new public MonoBtlsProvider Provider {
			get { return (MonoBtlsProvider)base.Provider; }
		}

		int VerifyCallback (MonoBtlsX509StoreCtx storeCtx)
		{
			using (var chainImpl = new X509ChainImplBtls (storeCtx))
			using (var managedChain = new X509Chain (chainImpl)) {
				var leaf = managedChain.ChainElements[0].Certificate;
				var result = ValidateCertificate (leaf, managedChain);
				certificateValidated = true;
				return result ? 1 : 0;
			}
		}

		int SelectCallback ()
		{
			Debug ("SELECT CALLBACK!");

			GetPeerCertificate ();
			if (remoteCertificate == null)
				throw new TlsException (AlertDescription.InternalError, "Cannot request client certificate before receiving one from the server.");

			var clientCert = SelectClientCertificate (remoteCertificate, null);
			Debug ("SELECT CALLBACK #1: {0}", clientCert);
			if (clientCert == null)
				return 1;

			nativeClientCertificate = GetPrivateCertificate (clientCert);
			Debug ("SELECT CALLBACK #2: {0}", nativeClientCertificate);
			clientCertificate = new X509Certificate (nativeClientCertificate);
			SetPrivateCertificate (nativeClientCertificate);
			return 1;
		}

		public override void StartHandshake ()
		{
			InitializeConnection ();

			ssl = new MonoBtlsSsl (ctx);

			bio = new MonoBtlsBioMono (this);
			ssl.SetBio (bio);

			if (IsServer) {
				SetPrivateCertificate (nativeServerCertificate);
			} else {
				ssl.SetServerName (TargetHost);
			}
		}

		void SetPrivateCertificate (X509CertificateImplBtls privateCert)
		{
			Debug ("SetPrivateCertificate: {0}", privateCert);
			ssl.SetCertificate (privateCert.X509);
			ssl.SetPrivateKey (privateCert.NativePrivateKey);
			var intermediate = privateCert.IntermediateCertificates;
			if (intermediate == null)
				return;
			for (int i = 0; i < intermediate.Count; i++) {
				var impl = (X509CertificateImplBtls)intermediate [i];
				Debug ("SetPrivateCertificate - add intermediate: {0}", impl);
				ssl.AddIntermediateCertificate (impl.X509);
			}
		}

		Exception GetException (MonoBtlsSslError status)
		{
			var error = MonoBtlsError.GetError ();
			if (error == null)
				return new MonoBtlsException (status);

			var text = MonoBtlsError.GetErrorString (error);
			return new MonoBtlsException ("{0} {1}", status, text);
		}

		public override bool ProcessHandshake ()
		{
			var done = false;
			while (!done) {
				Debug ("ProcessHandshake");
				MonoBtlsError.ClearError ();
				var status = DoProcessHandshake ();
				Debug ("ProcessHandshake #1: {0}", status);

				switch (status) {
				case MonoBtlsSslError.None:
					if (connected)
						done = true;
					else
						connected = true;
					break;
				case MonoBtlsSslError.WantRead:
				case MonoBtlsSslError.WantWrite:
					return false;
				default:
					throw GetException (status);
				}
			}

			ssl.PrintErrors ();

			return true;
		}

		MonoBtlsSslError DoProcessHandshake ()
		{
			if (connected)
				return ssl.Handshake ();
			else if (IsServer)
				return ssl.Accept ();
			else
				return ssl.Connect ();
		}

		public override void FinishHandshake ()
		{
			InitializeSession ();

			isAuthenticated = true;
		}

		void SetupCertificateStore ()
		{
			MonoBtlsProvider.SetupCertificateStore (ctx.CertificateStore);

			if (Settings != null && Settings.TrustAnchors != null) {
				var trust = IsServer ? MonoBtlsX509TrustKind.TRUST_CLIENT : MonoBtlsX509TrustKind.TRUST_SERVER;
				ctx.CertificateStore.AddCollection (Settings.TrustAnchors, trust);
			}
		}

		void InitializeConnection ()
		{
			ctx = new MonoBtlsSslCtx ();

#if MARTIN_DEBUG
			errbio = MonoBtlsBio.CreateMonoStream (Console.OpenStandardError ());
			ctx.SetDebugBio (errbio);
#endif

			SetupCertificateStore ();

			if (!IsServer || AskForClientCertificate)
				ctx.SetVerifyCallback (VerifyCallback, false);
			if (!IsServer)
				ctx.SetSelectCallback (SelectCallback);

			var host = TargetHost;
			if (!string.IsNullOrEmpty (host)) {
				var pos = TargetHost.IndexOf (':');
				if (pos > 0)
					host = host.Substring (0, pos);
			}

			ctx.SetVerifyParam (MonoBtlsProvider.GetVerifyParam (host, IsServer));

			TlsProtocolCode minProtocol, maxProtocol;
			GetProtocolVersions (out minProtocol, out maxProtocol);

			ctx.SetMinVersion ((int)minProtocol);
			ctx.SetMaxVersion ((int)maxProtocol);

			if (Settings != null && Settings.EnabledCiphers != null) {
				var ciphers = new short [Settings.EnabledCiphers.Length];
				for (int i = 0; i < ciphers.Length; i++)
					ciphers [i] = (short)Settings.EnabledCiphers [i];
				ctx.SetCiphers (ciphers, true);
			}
		}

		void GetPeerCertificate ()
		{
			if (remoteCertificate != null)
				return;
			using (var remoteCert = ssl.GetPeerCertificate ()) {
				if (remoteCert != null)
					remoteCertificate = MonoBtlsProvider.CreateCertificate (remoteCert);
			}
		}

		void InitializeSession ()
		{
			GetPeerCertificate ();

			if (IsServer && AskForClientCertificate && !certificateValidated) {
				if (!ValidateCertificate (null, null))
					throw new TlsException (AlertDescription.CertificateUnknown);
			}

			var cipher = (CipherSuiteCode)ssl.GetCipher ();
			var protocol = (TlsProtocolCode)ssl.GetVersion ();
			Debug ("GET CONNECTION INFO: {0:x}:{0} {1:x}:{1} {2}", cipher, protocol, (TlsProtocolCode)protocol);

			connectionInfo = new MonoTlsConnectionInfo {
				CipherSuiteCode = cipher,
				ProtocolVersion = GetProtocol (protocol)
			};
		}

		static TlsProtocols GetProtocol (TlsProtocolCode protocol)
		{
			switch (protocol) {
			case TlsProtocolCode.Tls10:
				return TlsProtocols.Tls10;
			case TlsProtocolCode.Tls11:
				return TlsProtocols.Tls11;
			case TlsProtocolCode.Tls12:
				return TlsProtocols.Tls12;
			default:
				throw new NotSupportedException ();
			}
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int size, out bool wantMore)
		{
			Debug ("Read: {0} {1} {2}", buffer.Length, offset, size);

			var data = Marshal.AllocHGlobal (size);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			try {
				MonoBtlsError.ClearError ();
				var status = ssl.Read (data, ref size);
				Debug ("Read done: {0} {1}", status, size);

				if (status == MonoBtlsSslError.WantRead) {
					wantMore = true;
					return 0;
				} else if (status != MonoBtlsSslError.None) {
					throw GetException (status);
				}

				if (size > 0)
					Marshal.Copy (data, buffer, offset, size);

				wantMore = false;
				return size;
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public override int Write (byte[] buffer, int offset, int size, out bool wantMore)
		{
			Debug ("Write: {0} {1} {2}", buffer.Length, offset, size);

			var data = Marshal.AllocHGlobal (size);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			try {
				MonoBtlsError.ClearError ();
				Marshal.Copy (buffer, offset, data, size);
				var status = ssl.Write (data, ref size);
				Debug ("Write done: {0} {1}", status, size);

				if (status == MonoBtlsSslError.WantWrite) {
					wantMore = true;
					return 0;
				} else if (status != MonoBtlsSslError.None) {
					throw GetException (status);
				}

				wantMore = false;
				return size;
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public override void Close ()
		{
			Debug ("Close!");
			ssl.Dispose ();
		}

		void Dispose<T> (ref T disposable)
			where T : class, IDisposable
		{
			try {
				if (disposable != null)
					disposable.Dispose ();
			} catch {
				;
			} finally {
				disposable = null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (disposing) {
					Dispose (ref remoteCertificate);
					Dispose (ref nativeServerCertificate);
					Dispose (ref nativeClientCertificate);
					Dispose (ref clientCertificate);
					Dispose (ref ctx);
					Dispose (ref ssl);
					Dispose (ref bio);
					Dispose (ref errbio);
				}
			} finally {
				base.Dispose (disposing);
			}
		}

		int IMonoBtlsBioMono.Read (byte[] buffer, int offset, int size, out bool wantMore)
		{
			Debug ("InternalRead: {0} {1}", offset, size);
			var ret = Parent.InternalRead (buffer, offset, size, out wantMore);
			Debug ("InternalReadDone: {0} {1}", ret, wantMore);
			return ret;
		}

		bool IMonoBtlsBioMono.Write (byte[] buffer, int offset, int size)
		{
			Debug ("InternalWrite: {0} {1}", offset, size);
			var ret = Parent.InternalWrite (buffer, offset, size);
			Debug ("InternalWrite done: {0}", ret);
			return ret;
		}

		void IMonoBtlsBioMono.Flush ()
		{
			;
		}

		void IMonoBtlsBioMono.Close ()
		{
			;
		}

		public override bool HasContext {
			get { return ssl != null && ssl.IsValid; }
		}
		public override bool IsAuthenticated {
			get { return isAuthenticated; }
		}
		public override MonoTlsConnectionInfo ConnectionInfo {
			get { return connectionInfo; }
		}
		internal override bool IsRemoteCertificateAvailable {
			get { return remoteCertificate != null; }
		}
		internal override X509Certificate LocalClientCertificate {
			get { return clientCertificate; }
		}
		public override X509Certificate RemoteCertificate {
			get { return remoteCertificate; }
		}
		public override TlsProtocols NegotiatedProtocol {
			get { return connectionInfo.ProtocolVersion; }
		}
	}
}
#endif
