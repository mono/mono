//
// SslStream.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using CipherAlgorithmType = System.Security.Authentication.CipherAlgorithmType;
using HashAlgorithmType = System.Security.Authentication.HashAlgorithmType;
using ExchangeAlgorithmType = System.Security.Authentication.ExchangeAlgorithmType;
#endif

using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using MNS = Mono.Net.Security;

namespace System.Net.Security
{
	public delegate X509Certificate ServerCertificateSelectionCallback (object sender, string hostName);

	/*
	 * Internal delegates from the referencesource / corefx.
	 */

	internal delegate bool RemoteCertValidationCallback (
		string host,
		X509Certificate certificate,
		X509Chain chain,
		SslPolicyErrors sslPolicyErrors);

	internal delegate X509Certificate LocalCertSelectionCallback (
		string targetHost,
		X509CertificateCollection localCertificates,
		X509Certificate remoteCertificate,
		string[] acceptableIssuers);

	internal delegate X509Certificate ServerCertSelectionCallback (string hostName);

	public class SslStream : AuthenticatedStream
	{
#if SECURITY_DEP
		MonoTlsProvider provider;
		MonoTlsSettings settings;
		RemoteCertificateValidationCallback validationCallback;
		LocalCertificateSelectionCallback selectionCallback;
		bool explicitSettings;
		IMonoSslStream impl;

		internal IMonoSslStream Impl {
			get {
				CheckDisposed ();
				return impl;
			}
		}

		IMonoSslStream2 Impl2 => (IMonoSslStream2)Impl;

		internal MonoTlsProvider Provider {
			get {
				CheckDisposed ();
				return provider;
			}
		}

		static MonoTlsProvider GetProvider ()
		{
			return MonoTlsProviderFactory.GetProvider ();
		}

		public SslStream (Stream innerStream)
			: this (innerStream, false)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen)
			: base (innerStream, leaveInnerStreamOpen)
		{
			provider = GetProvider ();
			settings = MonoTlsSettings.CopyDefaultSettings ();
			impl = provider.CreateSslStreamInternal (this, innerStream, leaveInnerStreamOpen, settings);
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
			: this (innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, null)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
			: base (innerStream, leaveInnerStreamOpen)
		{
			provider = GetProvider ();
			settings = MonoTlsSettings.CopyDefaultSettings ();
			SetAndVerifyValidationCallback (userCertificateValidationCallback);
			SetAndVerifySelectionCallback (userCertificateSelectionCallback);
			impl = provider.CreateSslStream (innerStream, leaveInnerStreamOpen, settings);
		}

		[MonoLimitation ("encryptionPolicy is ignored")]
		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
			: this (innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback)
		{
		}

		internal SslStream (Stream innerStream, bool leaveInnerStreamOpen, MonoTlsProvider provider, MonoTlsSettings settings)
			: base (innerStream, leaveInnerStreamOpen)
		{
			this.provider = provider;
			this.settings = settings.Clone ();
			explicitSettings = true;
			impl = provider.CreateSslStreamInternal (this, innerStream, leaveInnerStreamOpen, settings);
		}

		internal static IMonoSslStream CreateMonoSslStream (Stream innerStream, bool leaveInnerStreamOpen, MonoTlsProvider provider, MonoTlsSettings settings)
		{
			var sslStream = new SslStream (innerStream, leaveInnerStreamOpen, provider, settings);
			return sslStream.Impl;
		}

		void SetAndVerifyValidationCallback (RemoteCertificateValidationCallback callback)
		{
			if (validationCallback == null) {
				validationCallback = callback;
				settings.RemoteCertificateValidationCallback = MNS.Private.CallbackHelpers.PublicToMono (callback);
			} else if ((callback != null && validationCallback != callback) || (explicitSettings & settings.RemoteCertificateValidationCallback != null)) {
				throw new InvalidOperationException (SR.Format (SR.net_conflicting_options, nameof (RemoteCertificateValidationCallback)));
			}
		}

		void SetAndVerifySelectionCallback (LocalCertificateSelectionCallback callback)
		{
			if (selectionCallback == null) {
				selectionCallback = callback;
				if (callback == null)
					settings.ClientCertificateSelectionCallback = null;
				else
					settings.ClientCertificateSelectionCallback = (t, lc, rc, ai) => callback (this, t, lc, rc, ai);
			} else if ((callback != null && selectionCallback != callback) || (explicitSettings && settings.ClientCertificateSelectionCallback != null)) {
				throw new InvalidOperationException (SR.Format (SR.net_conflicting_options, nameof (LocalCertificateSelectionCallback)));
			}
		}

		MNS.MonoSslServerAuthenticationOptions CreateAuthenticationOptions (SslServerAuthenticationOptions sslServerAuthenticationOptions)
		{
			if (sslServerAuthenticationOptions.ServerCertificate == null && sslServerAuthenticationOptions.ServerCertificateSelectionCallback == null && selectionCallback == null)
				throw new ArgumentNullException (nameof (sslServerAuthenticationOptions.ServerCertificate));

			if ((sslServerAuthenticationOptions.ServerCertificate != null || selectionCallback != null) && sslServerAuthenticationOptions.ServerCertificateSelectionCallback != null)
				throw new InvalidOperationException (SR.Format (SR.net_conflicting_options, nameof (ServerCertificateSelectionCallback)));

			var options = new MNS.MonoSslServerAuthenticationOptions (sslServerAuthenticationOptions);

			var serverSelectionCallback = sslServerAuthenticationOptions.ServerCertificateSelectionCallback;
			if (serverSelectionCallback != null)
				options.ServerCertSelectionDelegate = (x) => serverSelectionCallback (this, x);

			return options;
		}

		public virtual void AuthenticateAsClient (string targetHost)
		{
			Impl.AuthenticateAsClient (targetHost);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsClient (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsClient (asyncResult);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			Impl.AuthenticateAsServer (serverCertificate);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsServer (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsServer (asyncResult);
		}

		public TransportContext TransportContext => null;

		public virtual Task AuthenticateAsClientAsync (string targetHost)
		{
			return Impl.AuthenticateAsClientAsync (targetHost);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsClientAsync (targetHost, clientCertificates, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsClientAsync (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		public Task AuthenticateAsClientAsync (SslClientAuthenticationOptions sslClientAuthenticationOptions, CancellationToken cancellationToken)
		{
			SetAndVerifyValidationCallback (sslClientAuthenticationOptions.RemoteCertificateValidationCallback);
			SetAndVerifySelectionCallback (sslClientAuthenticationOptions.LocalCertificateSelectionCallback);
			return Impl2.AuthenticateAsClientAsync (new MNS.MonoSslClientAuthenticationOptions (sslClientAuthenticationOptions), cancellationToken);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, SecurityProtocol.SystemDefaultSecurityProtocols, checkCertificateRevocation);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
		}

		public Task AuthenticateAsServerAsync (SslServerAuthenticationOptions sslServerAuthenticationOptions, CancellationToken cancellationToken)
		{
			return Impl2.AuthenticateAsServerAsync (CreateAuthenticationOptions (sslServerAuthenticationOptions), cancellationToken);
		}

		public virtual Task ShutdownAsync ()
		{
			return Impl.ShutdownAsync ();
		}

		public override bool IsAuthenticated {
			get { return Impl.IsAuthenticated; }
		}

		public override bool IsMutuallyAuthenticated {
			get { return Impl.IsMutuallyAuthenticated; }
		}

		public override bool IsEncrypted {
			get { return Impl.IsEncrypted; }
		}

		public override bool IsSigned {
			get { return Impl.IsSigned; }
		}

		public override bool IsServer {
			get { return Impl.IsServer; }
		}

		public virtual SslProtocols SslProtocol {
			get { return (SslProtocols)Impl.SslProtocol; }
		}

		public virtual bool CheckCertRevocationStatus {
			get { return Impl.CheckCertRevocationStatus; }
		}

		public virtual X509Certificate LocalCertificate {
			get { return Impl.LocalCertificate; }
		}

		public virtual X509Certificate RemoteCertificate {
			get { return Impl.RemoteCertificate; }
		}

		public virtual CipherAlgorithmType CipherAlgorithm {
			get { return (CipherAlgorithmType)Impl.CipherAlgorithm; }
		}

		public virtual int CipherStrength {
			get { return Impl.CipherStrength; }
		}

		public virtual HashAlgorithmType HashAlgorithm {
			get { return (HashAlgorithmType)Impl.HashAlgorithm; }
		}

		public virtual int HashStrength {
			get { return Impl.HashStrength; }
		}

		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm {
			get { return (ExchangeAlgorithmType)Impl.KeyExchangeAlgorithm; }
		}

		public virtual int KeyExchangeStrength {
			get { return Impl.KeyExchangeStrength; }
		}

		public SslApplicationProtocol NegotiatedApplicationProtocol {
			get {
				throw new PlatformNotSupportedException ("https://github.com/mono/mono/issues/12880");
			}
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanRead {
			get { return impl != null && impl.CanRead; }
		}

		public override bool CanTimeout {
			get { return InnerStream.CanTimeout; }
		}

		public override bool CanWrite {
			get { return impl != null && impl.CanWrite; }
		}

		public override int ReadTimeout {
			get { return Impl.ReadTimeout; }
			set { Impl.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return Impl.WriteTimeout; }
			set { Impl.WriteTimeout = value; }
		}

		public override long Length {
			get { return Impl.Length; }
		}

		public override long Position {
			get { return Impl.Position; }
			set {
				throw new NotSupportedException (SR.GetString (SR.net_noseek));
			}
		}

		public override void SetLength (long value)
		{
			Impl.SetLength (value);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException (SR.GetString (SR.net_noseek));
		}

		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			return InnerStream.FlushAsync (cancellationToken);
		}

		public override void Flush ()
		{
			InnerStream.Flush ();
		}

		void CheckDisposed ()
		{
			if (impl == null)
				throw new ObjectDisposedException ("SslStream");
		}

		protected override void Dispose (bool disposing)
		{
			try {
				if (impl != null && disposing) {
					impl.Dispose ();
					impl = null;
				}
			} finally {
				base.Dispose (disposing);
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return Impl.Read (buffer, offset, count);
		}

		public void Write (byte[] buffer)
		{
			Impl.Write (buffer);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			Impl.Write (buffer, offset, count);
		}

		// [HostProtection (ExternalThreading=true)]
		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginRead (buffer, offset, count, asyncCallback, asyncState);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return Impl.EndRead (asyncResult);
		}

		// [HostProtection (ExternalThreading=true)]
		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginWrite (buffer, offset, count, asyncCallback, asyncState);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			Impl.EndWrite (asyncResult);
		}
#else // !SECURITY_DEP
		const string EXCEPTION_MESSAGE = "System.Net.Security.SslStream is not supported on the current platform.";

		public SslStream (Stream innerStream)
			: this (innerStream, false)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen)
			: base (innerStream, leaveInnerStreamOpen)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
			: this (innerStream, leaveInnerStreamOpen)
		{
		}

		public virtual void AuthenticateAsClient (string targetHost)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TransportContext TransportContext {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool IsAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsMutuallyAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsEncrypted {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsSigned {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsServer {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual SslProtocols SslProtocol {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual bool CheckCertRevocationStatus {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual X509Certificate LocalCertificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual X509Certificate RemoteCertificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual CipherAlgorithmType CipherAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int CipherStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual HashAlgorithmType HashAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int HashStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual int KeyExchangeStrength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SslApplicationProtocol NegotiatedApplicationProtocol {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanSeek {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanRead {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool CanWrite {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int ReadTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int WriteTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long Length {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long Position {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void SetLength (long value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Flush ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void Dispose (bool disposing)
		{
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Write (byte[] buffer)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
#endif
	}
}
