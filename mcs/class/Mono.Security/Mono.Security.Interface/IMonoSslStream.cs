//
// IMonoSslStream.cs
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
using System.Threading;
using System.Threading.Tasks;
using SSA = System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Cryptography;

namespace Mono.Security.Interface
{
	public interface IMonoSslStream : IDisposable
	{
		SslStream SslStream {
			get;
		}

		void AuthenticateAsClient (string targetHost);

		void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation);

		void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		void EndAuthenticateAsClient (IAsyncResult asyncResult);

		void AuthenticateAsServer (X509Certificate serverCertificate);

		void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation);

		void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState);

		void EndAuthenticateAsServer (IAsyncResult asyncResult);

		Task AuthenticateAsClientAsync (string targetHost);

		Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation);

		Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		Task AuthenticateAsServerAsync (X509Certificate serverCertificate);

		Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation);

		Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SSA.SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		int Read (byte[] buffer, int offset, int count);

		void Write (byte[] buffer);

		void Write (byte[] buffer, int offset, int count);

		IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		int EndRead (IAsyncResult asyncResult);

		IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		void EndWrite (IAsyncResult asyncResult);

		Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken);

		Task ShutdownAsync ();

		TransportContext TransportContext {
			get;
		}

		bool IsAuthenticated {
			get;
		}

		bool IsMutuallyAuthenticated {
			get;
		}

		bool IsEncrypted {
			get;
		}

		bool IsSigned {
			get;
		}

		bool IsServer {
			get;
		}

		SSA.CipherAlgorithmType CipherAlgorithm {
			get;
		}

		int CipherStrength {
			get;
		}

		SSA.HashAlgorithmType HashAlgorithm {
			get;
		}

		int HashStrength {
			get;
		}

		SSA.ExchangeAlgorithmType KeyExchangeAlgorithm {
			get;
		}

		int KeyExchangeStrength {
			get;
		}

		bool CanRead {
			get;
		}

		bool CanTimeout {
			get;
		}

		bool CanWrite {
			get;
		}

		long Length {
			get;
		}

		long Position {
			get;
		}

		void SetLength (long value);

		AuthenticatedStream AuthenticatedStream {
			get;
		}

		int ReadTimeout {
			get; set;
		}

		int WriteTimeout {
			get; set;
		}

		bool CheckCertRevocationStatus {
			get;
		}

		X509Certificate InternalLocalCertificate {
			get;
		}

		X509Certificate LocalCertificate {
			get;
		}

		X509Certificate RemoteCertificate {
			get;
		}

		SSA.SslProtocols SslProtocol {
			get;
		}

		MonoTlsProvider Provider {
			get;
		}


		MonoTlsConnectionInfo GetConnectionInfo ();

		bool CanRenegotiate {
			get;
		}

		Task RenegotiateAsync (CancellationToken cancellationToken);
	}

	interface IMonoSslStream2 : IMonoSslStream
	{
		Task AuthenticateAsClientAsync (IMonoSslClientAuthenticationOptions sslClientAuthenticationOptions, CancellationToken cancellationToken);

		Task AuthenticateAsServerAsync (IMonoSslServerAuthenticationOptions sslServerAuthenticationOptions, CancellationToken cancellationToken);
	}
}

