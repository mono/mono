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
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;
using Mono.Net.Security;

namespace Mono.Security.Interface
{
	public abstract class MonoSslStream
	{
		public abstract void AuthenticateAsClient (string targetHost);

		public abstract void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		public abstract IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState);

		public abstract IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		public abstract void EndAuthenticateAsClient (IAsyncResult asyncResult);

		public abstract void AuthenticateAsServer (X509Certificate serverCertificate);

		public abstract void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		public abstract IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState);

		public abstract IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		public abstract void EndAuthenticateAsServer (IAsyncResult asyncResult);

		public abstract Task AuthenticateAsClientAsync (string targetHost);

		public abstract Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		public abstract Task AuthenticateAsServerAsync (X509Certificate serverCertificate);

		public abstract Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		public abstract void Flush ();

		public abstract int Read (byte[] buffer, int offset, int count);

		public abstract void Write (byte[] buffer);

		public abstract void Write (byte[] buffer, int offset, int count);

		public abstract IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		public abstract int EndRead (IAsyncResult asyncResult);

		public abstract IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		public abstract void EndWrite (IAsyncResult asyncResult);

		public abstract TransportContext TransportContext {
			get;
		}

		public abstract bool IsAuthenticated {
			get;
		}

		public abstract bool IsMutuallyAuthenticated {
			get;
		}

		public abstract bool IsEncrypted {
			get;
		}

		public abstract bool IsSigned {
			get;
		}

		public abstract bool IsServer {
			get;
		}

		public abstract CipherAlgorithmType CipherAlgorithm {
			get;
		}

		public abstract int CipherStrength {
			get;
		}

		public abstract HashAlgorithmType HashAlgorithm {
			get;
		}

		public abstract int HashStrength {
			get;
		}

		public abstract ExchangeAlgorithmType KeyExchangeAlgorithm {
			get;
		}

		public abstract int KeyExchangeStrength {
			get;
		}

		public abstract bool CanRead {
			get;
		}

		public abstract bool CanTimeout {
			get;
		}

		public abstract bool CanWrite {
			get;
		}

		public abstract long Length {
			get;
		}

		public abstract long Position {
			get;
		}

		public abstract void SetLength (long value);

		public abstract AuthenticatedStream AuthenticatedStream {
			get;
		}

		public abstract int ReadTimeout {
			get; set;
		}

		public abstract int WriteTimeout {
			get; set;
		}

		public abstract bool CheckCertRevocationStatus {
			get;
		}

		public abstract X509Certificate InternalLocalCertificate {
			get;
		}

		public abstract X509Certificate LocalCertificate {
			get;
		}

		public abstract X509Certificate RemoteCertificate {
			get;
		}

		public abstract SslProtocols SslProtocol {
			get;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}
	}
}

