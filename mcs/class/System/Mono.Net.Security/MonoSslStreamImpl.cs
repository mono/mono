//
// MonoSslStream.cs
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

#if MONO_X509_ALIAS
extern alias PrebuiltSystem;
#endif
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
#if MONO_X509_ALIAS
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;

using XTransportContext = PrebuiltSystem::System.Net.TransportContext;
using XAuthenticatedStream = PrebuiltSystem::System.Net.Security.AuthenticatedStream;

using XCipherAlgorithmType = PrebuiltSystem::System.Security.Authentication.CipherAlgorithmType;
using XHashAlgorithmType = PrebuiltSystem::System.Security.Authentication.HashAlgorithmType;
using XExchangeAlgorithmType = PrebuiltSystem::System.Security.Authentication.ExchangeAlgorithmType;
using XSslProtocols = PrebuiltSystem::System.Security.Authentication.SslProtocols;
#else
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;

using XTransportContext = System.Net.TransportContext;
using XAuthenticatedStream = System.Net.Security.AuthenticatedStream;

using XCipherAlgorithmType = System.Security.Authentication.CipherAlgorithmType;
using XHashAlgorithmType = System.Security.Authentication.HashAlgorithmType;
using XExchangeAlgorithmType = System.Security.Authentication.ExchangeAlgorithmType;
using XSslProtocols = System.Security.Authentication.SslProtocols;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;

namespace Mono.Net.Security
{
	class MonoSslStreamImpl : MonoSslStream
	{
		IMonoSslStream impl;

		internal IMonoSslStream Impl {
			get {
				CheckDisposed ();
				return impl;
			}
		}

		public MonoSslStreamImpl (IMonoSslStream impl)
		{
			this.impl = impl;
		}

		public override void AuthenticateAsClient (string targetHost)
		{
			Impl.AuthenticateAsClient (targetHost);
		}

		public override void AuthenticateAsClient (string targetHost, XX509CertificateCollection clientCertificates, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsClient (targetHost, clientCertificates, (SslProtocols)enabledSslProtocols, checkCertificateRevocation);
		}

		public override IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginAuthenticateAsClient (string targetHost, XX509CertificateCollection clientCertificates, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, clientCertificates, (SslProtocols)enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public override void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsClient (asyncResult);
		}

		public override void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			Impl.AuthenticateAsServer (serverCertificate);
		}

		public override void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsServer (serverCertificate, clientCertificateRequired, (SslProtocols)enabledSslProtocols, checkCertificateRevocation);
		}

		public override IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, (SslProtocols)enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public override void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsServer (asyncResult);
		}

		public override Task AuthenticateAsClientAsync (string targetHost)
		{
			return Impl.AuthenticateAsClientAsync (targetHost);
		}

		public override Task AuthenticateAsClientAsync (string targetHost, XX509CertificateCollection clientCertificates, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsClientAsync (targetHost, clientCertificates, (SslProtocols)enabledSslProtocols, checkCertificateRevocation);
		}

		public override Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate);
		}

		public override Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, XSslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, (SslProtocols)enabledSslProtocols, checkCertificateRevocation);
		}

		public override void Flush ()
		{
			Impl.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return Impl.Read (buffer, offset, count);
		}

		public override void Write (byte[] buffer)
		{
			Impl.Write (buffer);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			Impl.Write (buffer, offset, count);
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginRead (buffer, offset, count, asyncCallback, asyncState);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return Impl.EndRead (asyncResult);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginWrite (buffer, offset, count, asyncCallback, asyncState);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			Impl.EndWrite (asyncResult);
		}

		public override XTransportContext TransportContext {
			get { return (XTransportContext)(object)Impl.TransportContext; }
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

		public override XCipherAlgorithmType CipherAlgorithm {
			get { return (XCipherAlgorithmType)Impl.CipherAlgorithm; }
		}

		public override int CipherStrength {
			get { return Impl.CipherStrength; }
		}

		public override XHashAlgorithmType HashAlgorithm {
			get { return (XHashAlgorithmType)Impl.HashAlgorithm; }
		}

		public override int HashStrength {
			get { return Impl.HashStrength; }
		}

		public override XExchangeAlgorithmType KeyExchangeAlgorithm {
			get { return (XExchangeAlgorithmType)Impl.KeyExchangeAlgorithm; }
		}

		public override int KeyExchangeStrength {
			get { return KeyExchangeStrength; }
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

		public override long Length {
			get { return Impl.Length; }
		}

		public override long Position {
			get { return Impl.Position; }
		}

		public override void SetLength (long value)
		{
			Impl.SetLength (value);
		}

		public override XAuthenticatedStream AuthenticatedStream {
			get { return (XAuthenticatedStream)(Stream)Impl.AuthenticatedStream; }
		}

		public override int ReadTimeout {
			get { return Impl.ReadTimeout; }
			set { Impl.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return Impl.WriteTimeout; }
			set { Impl.WriteTimeout = value; }
		}

		public override bool CheckCertRevocationStatus {
			get { return Impl.CheckCertRevocationStatus; }
		}

		public override X509Certificate InternalLocalCertificate {
			get { return Impl.InternalLocalCertificate; }
		}

		public override X509Certificate LocalCertificate {
			get { return Impl.LocalCertificate; }
		}

		public override X509Certificate RemoteCertificate {
			get { return Impl.RemoteCertificate; }
		}

		public override XSslProtocols SslProtocol {
			get { return (XSslProtocols)Impl.SslProtocol; }
		}

		void CheckDisposed ()
		{
			if (impl == null)
				throw new ObjectDisposedException ("MonoSslStream");
		}

		protected override void Dispose (bool disposing)
		{
			if (impl != null && disposing) {
				impl.Dispose ();
				impl = null;
			}
		}
	}
}
#endif

