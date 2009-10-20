// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Handshake;
using MonoX509 = Mono.Security.X509;

namespace Mono.Security.Protocol.Tls
{
	internal class ServerContext : Context
	{
		#region Fields

		private SslServerStream sslStream;
		private bool request_client_certificate;
		private bool			clientCertificateRequired;

		#endregion

		#region Properties

		public SslServerStream SslStream
		{
			get { return this.sslStream; }
		}

		public bool	ClientCertificateRequired
		{
			get { return this.clientCertificateRequired; }
		}

		public bool RequestClientCertificate {
			get { return request_client_certificate; }
		}

		#endregion

		#region Constructors

		public ServerContext(
			SslServerStream			stream,
			SecurityProtocolType	securityProtocolType,
			X509Certificate			serverCertificate,
			bool					clientCertificateRequired,
			bool					requestClientCertificate)
			: base(securityProtocolType)
		{
			this.sslStream					= stream;
			this.clientCertificateRequired	= clientCertificateRequired;
			this.request_client_certificate	= requestClientCertificate;

			// Convert the System.Security cert to a Mono Cert
			MonoX509.X509Certificate cert = new MonoX509.X509Certificate(serverCertificate.GetRawCertData());

			// Add server certificate to the certificate collection
			this.ServerSettings.Certificates = new MonoX509.X509CertificateCollection();
			this.ServerSettings.Certificates.Add(cert);

			this.ServerSettings.UpdateCertificateRSA();

			// Add requested certificate types
			this.ServerSettings.CertificateTypes = new ClientCertificateType[1];
			this.ServerSettings.CertificateTypes[0] = ClientCertificateType.RSA;

			// Add certificate authorities
			MonoX509.X509CertificateCollection trusted = MonoX509.X509StoreManager.TrustedRootCertificates;
			string[] list = new string [trusted.Count];
			int i = 0;
			foreach (MonoX509.X509Certificate root in trusted)
			{
				list [i++] = root.IssuerName;
			}
			this.ServerSettings.DistinguisedNames = list;
		}

		#endregion
	}
}
