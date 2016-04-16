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

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif
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
	interface IMonoSslStream : IDisposable
	{
		AuthenticatedStream AuthenticatedStream {
			get;
		}

		void AuthenticateAsClient (string targetHost);

		void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates,
		                                        SslProtocols enabledSslProtocols, bool checkCertificateRevocation,
		                                        AsyncCallback asyncCallback, object asyncState);

		void EndAuthenticateAsClient (IAsyncResult asyncResult);

		void AuthenticateAsServer (X509Certificate serverCertificate);

		void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired,
		                          SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState);

		IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired,
		                                        SslProtocols enabledSslProtocols, bool checkCertificateRevocation,
		                                        AsyncCallback asyncCallback,
		                                        object asyncState);

		void EndAuthenticateAsServer (IAsyncResult asyncResult);

		TransportContext TransportContext {
			get;
		}

		Task AuthenticateAsClientAsync (string targetHost);

		Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		Task AuthenticateAsServerAsync (X509Certificate serverCertificate);

		Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation);

		//
		//
		// Base class properties
		//
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

		//
		//
		//SSL specific properties
		//
		//
		SslProtocols SslProtocol {
			get;
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

		//
		// More informational properties
		//
		CipherAlgorithmType CipherAlgorithm {
			get;
		}

		int CipherStrength {
			get;
		}

		HashAlgorithmType HashAlgorithm {
			get;
		}

		int HashStrength {
			get;
		}

		ExchangeAlgorithmType KeyExchangeAlgorithm {
			get;
		}

		int KeyExchangeStrength {
			get;
		}

		//
		//
		// Stream contract implementation
		//
		//
		//
		bool CanRead {
			get;
		}

		bool CanTimeout {
			get;
		}

		bool CanWrite {
			get;
		}

		int ReadTimeout {
			get;
			set;
		}

		int WriteTimeout {
			get;
			set;
		}

		long Length {
			get;
		}

		long Position {
			get;
		}

		void SetLength (long value);

		void Flush ();

		int Read (byte[] buffer, int offset, int count);

		void Write (byte[] buffer);

		void Write (byte[] buffer, int offset, int count);

		IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		int EndRead (IAsyncResult asyncResult);

		IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState);

		void EndWrite (IAsyncResult asyncResult);

#if SECURITY_DEP
		MSI.MonoTlsProvider Provider {
			get;
		}

		MSI.MonoTlsConnectionInfo GetConnectionInfo ();
#endif
	}
}
