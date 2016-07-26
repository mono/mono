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

#if !MONO_FEATURE_NEW_TLS
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

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security.Cryptography;

using System.Threading.Tasks;

using MNS = Mono.Net.Security;

namespace System.Net.Security
{
	/*
	 * These two are defined by the referencesource; add them heere to make
	 * it easy to switch between the two implementations.
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

	public class SslStream : AuthenticatedStream, MNS.IMonoSslStream
	{
		MonoTlsProvider provider;
		IMonoSslStream impl;

		internal IMonoSslStream Impl {
			get {
				CheckDisposed ();
				return impl;
			}
		}

		internal MonoTlsProvider Provider {
			get {
				CheckDisposed ();
				return provider;
			}
		}

		static MonoTlsProvider GetProvider ()
		{
			return MonoTlsProviderFactory.GetDefaultProvider ();
		}

		public SslStream (Stream innerStream)
			: this (innerStream, false)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen)
			: base (innerStream, leaveInnerStreamOpen)
		{
			provider = GetProvider ();
			impl = provider.CreateSslStream (innerStream, leaveInnerStreamOpen);
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
			: this (innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, null)
		{
		}

		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
			: base (innerStream, leaveInnerStreamOpen)
		{
			provider = GetProvider ();
			var settings = MonoTlsSettings.CopyDefaultSettings ();
			settings.RemoteCertificateValidationCallback = MNS.Private.CallbackHelpers.PublicToMono (userCertificateValidationCallback);
			settings.ClientCertificateSelectionCallback = MNS.Private.CallbackHelpers.PublicToMono (userCertificateSelectionCallback);
			impl = provider.CreateSslStream (innerStream, leaveInnerStreamOpen, settings);
		}

		[MonoLimitation ("encryptionPolicy is ignored")]
		public SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
		: this (innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback)
		{
		}

		internal SslStream (Stream innerStream, bool leaveInnerStreamOpen, IMonoSslStream impl)
			: base (innerStream, leaveInnerStreamOpen)
		{
			this.impl = impl;
		}

		public virtual void AuthenticateAsClient (string targetHost)
		{
			Impl.AuthenticateAsClient (targetHost);
		}

		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		// [HostProtection (ExternalThreading=true)]
		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, asyncCallback, asyncState);
		}

		// [HostProtection (ExternalThreading=true)]
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

		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
		}

		// [HostProtection (ExternalThreading=true)]
		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, asyncCallback, asyncState);
		}

		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsServer (asyncResult);
		}

		public TransportContext TransportContext {
			get {
				throw new NotSupportedException();
			}
		}

		// [HostProtection (ExternalThreading=true)]
		public virtual Task AuthenticateAsClientAsync (string targetHost)
		{
			return Impl.AuthenticateAsClientAsync (targetHost);
		}

		public virtual Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsClientAsync (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate);
		}

		public virtual Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
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

		X509Certificate MNS.IMonoSslStream.InternalLocalCertificate {
			get { return Impl.InternalLocalCertificate; }
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

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanRead {
			get { return Impl.CanRead; }
		}

		public override bool CanTimeout {
			get { return Impl.CanTimeout; }
		}

		public override bool CanWrite {
			get { return Impl.CanWrite; }
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

		public override void Flush ()
		{
			Impl.Flush ();
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

		AuthenticatedStream MNS.IMonoSslStream.AuthenticatedStream {
			get { return this; }
		}

		MonoTlsProvider MNS.IMonoSslStream.Provider {
			get { return provider; }
		}

		MonoTlsConnectionInfo MNS.IMonoSslStream.GetConnectionInfo ()
		{
			return Impl.GetConnectionInfo ();
		}
	}
}
#else // !SECURITY_DEP
namespace System.Net.Security
{
	public class SslStream
	{
	}
}
#endif

#endif
