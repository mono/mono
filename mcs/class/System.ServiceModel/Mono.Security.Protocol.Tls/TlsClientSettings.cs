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
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Cryptography;
using X509 = Mono.Security.X509;

namespace Mono.Security.Protocol.Tls
{
	internal sealed class TlsClientSettings
	{
		#region Fields

		private string						targetHost;
		private X509CertificateCollection	certificates;
		//private SecurityCompressionType		compressionMethod;
		private X509Certificate				clientCertificate;
		private RSAManaged					certificateRSA;
	
		#endregion

		#region Properties

		public string TargetHost
		{
			get { return this.targetHost; }
			set { this.targetHost = value; }
		}

		public X509CertificateCollection Certificates
		{
			get { return this.certificates; }
			set { this.certificates = value; }
		}
		
		public X509Certificate ClientCertificate
		{
			get { return this.clientCertificate; }
			set 
			{ 
				this.clientCertificate = value; 
				this.UpdateCertificateRSA();
			}
		}

		public RSAManaged CertificateRSA
		{
			get { return this.certificateRSA; }
		}

		/*
		public SecurityCompressionType CompressionMethod
		{
			get { return this.compressionMethod; }
			set 
			{ 
				if (value != SecurityCompressionType.None)
				{
					throw new NotSupportedException("Specified compression method is not supported");
				}
				this.compressionMethod = value; 
			}
		}
		*/

		#endregion

		#region Constructors

		public TlsClientSettings()
		{
			// this.compressionMethod	= SecurityCompressionType.None;
			this.certificates		= new X509CertificateCollection();
			this.targetHost			= String.Empty;
		}

		#endregion

		#region Methods

		public void UpdateCertificateRSA()
		{
			if (this.clientCertificate == null)
			{
				this.certificateRSA = null;
			}
			else
			{
				X509.X509Certificate cert = new X509.X509Certificate(this.clientCertificate.GetRawCertData());

				this.certificateRSA = new RSAManaged(
					cert.RSA.KeySize);

				this.certificateRSA.ImportParameters(
					cert.RSA.ExportParameters(false));
			}
		}

		#endregion
	}
}
