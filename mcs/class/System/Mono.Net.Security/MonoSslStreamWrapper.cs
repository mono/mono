//
// MonoSslStreamImpl.cs
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
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;

using System.Threading.Tasks;

namespace Mono.Net.Security.Private
{
	class MonoSslStreamWrapper : IMonoSslStream
	{
		MSI.IMonoSslStream impl;

		internal MSI.IMonoSslStream Impl {
			get {
				CheckDisposed ();
				return impl;
			}
		}

		public MonoSslStreamWrapper (MSI.IMonoSslStream impl)
		{
			this.impl = impl;
		}

		public void AuthenticateAsClient (string targetHost)
		{
			Impl.AuthenticateAsClient (targetHost);
		}

		public void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsClient (asyncResult);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			Impl.AuthenticateAsServer (serverCertificate);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			Impl.AuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
		}

		public void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			Impl.EndAuthenticateAsServer (asyncResult);
		}

		public Task AuthenticateAsClientAsync (string targetHost)
		{
			return Impl.AuthenticateAsClientAsync (targetHost);
		}

		public Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsClientAsync (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Impl.AuthenticateAsServerAsync (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
		}

		public void Flush ()
		{
			Impl.Flush ();
		}

		public int Read (byte[] buffer, int offset, int count)
		{
			return Impl.Read (buffer, offset, count);
		}

		public void Write (byte[] buffer)
		{
			Impl.Write (buffer);
		}

		public void Write (byte[] buffer, int offset, int count)
		{
			Impl.Write (buffer, offset, count);
		}

		public IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginRead (buffer, offset, count, asyncCallback, asyncState);
		}

		public int EndRead (IAsyncResult asyncResult)
		{
			return Impl.EndRead (asyncResult);
		}

		public IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return Impl.BeginWrite (buffer, offset, count, asyncCallback, asyncState);
		}

		public void EndWrite (IAsyncResult asyncResult)
		{
			Impl.EndWrite (asyncResult);
		}

		public TransportContext TransportContext {
			get { return Impl.TransportContext; }
		}

		public bool IsAuthenticated {
			get { return Impl.IsAuthenticated; }
		}

		public bool IsMutuallyAuthenticated {
			get { return Impl.IsMutuallyAuthenticated; }
		}

		public bool IsEncrypted {
			get { return Impl.IsEncrypted; }
		}

		public bool IsSigned {
			get { return Impl.IsSigned; }
		}

		public bool IsServer {
			get { return Impl.IsServer; }
		}

		public CipherAlgorithmType CipherAlgorithm {
			get { return (CipherAlgorithmType)Impl.CipherAlgorithm; }
		}

		public int CipherStrength {
			get { return Impl.CipherStrength; }
		}

		public HashAlgorithmType HashAlgorithm {
			get { return (HashAlgorithmType)Impl.HashAlgorithm; }
		}

		public int HashStrength {
			get { return Impl.HashStrength; }
		}

		public ExchangeAlgorithmType KeyExchangeAlgorithm {
			get { return (ExchangeAlgorithmType)Impl.KeyExchangeAlgorithm; }
		}

		public int KeyExchangeStrength {
			get { return Impl.KeyExchangeStrength; }
		}

		public bool CanRead {
			get { return Impl.CanRead; }
		}

		public bool CanTimeout {
			get { return Impl.CanTimeout; }
		}

		public bool CanWrite {
			get { return Impl.CanWrite; }
		}

		public long Length {
			get { return Impl.Length; }
		}

		public long Position {
			get { return Impl.Position; }
		}

		public void SetLength (long value)
		{
			Impl.SetLength (value);
		}

		public AuthenticatedStream AuthenticatedStream {
			get { return Impl.AuthenticatedStream; }
		}

		public int ReadTimeout {
			get { return Impl.ReadTimeout; }
			set { Impl.ReadTimeout = value; }
		}

		public int WriteTimeout {
			get { return Impl.WriteTimeout; }
			set { Impl.WriteTimeout = value; }
		}

		public bool CheckCertRevocationStatus {
			get { return Impl.CheckCertRevocationStatus; }
		}

		X509Certificate IMonoSslStream.InternalLocalCertificate {
			get { return Impl.InternalLocalCertificate; }
		}

		public X509Certificate LocalCertificate {
			get { return Impl.LocalCertificate; }
		}

		public X509Certificate RemoteCertificate {
			get { return Impl.RemoteCertificate; }
		}

		public SslProtocols SslProtocol {
			get { return (SslProtocols)Impl.SslProtocol; }
		}

		public MSI.MonoTlsProvider Provider {
			get { return Impl.Provider; }
		}

		public MSI.MonoTlsConnectionInfo GetConnectionInfo ()
		{
			return Impl.GetConnectionInfo ();
		}

		void CheckDisposed ()
		{
			if (impl == null)
				throw new ObjectDisposedException ("MonoSslStream");
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected void Dispose (bool disposing)
		{
			if (impl != null && disposing) {
				impl.Dispose ();
				impl = null;
			}
		}
	}
}

#endif
