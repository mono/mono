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
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Security.X509;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsServerSettings
	{
		#region Fields

		private X509CertificateCollection	certificates;
		private RSA							certificateRSA;
		private RSAParameters				rsaParameters;
		private byte[]						signedParams;
		private string[]					distinguisedNames;
		private bool						serverKeyExchange;
		private bool						certificateRequest;
		private	ClientCertificateType[]		certificateTypes;

		#endregion

		#region Properties
		
		public bool	ServerKeyExchange
		{
			get { return this.serverKeyExchange; }
			set { this.serverKeyExchange = value; }
		}		

		public X509CertificateCollection Certificates
		{
			get { return this.certificates; }
			set { this.certificates = value; }
		}

		public RSA CertificateRSA
		{
			get { return this.certificateRSA; }
		}
		
		public RSAParameters RsaParameters
		{
			get { return this.rsaParameters; }
			set { this.rsaParameters = value; }
		}

		public byte[] SignedParams
		{
			get { return this.signedParams; }
			set { this.signedParams = value; }
		}

		public bool	CertificateRequest
		{
			get { return this.certificateRequest; }
			set { this.certificateRequest = value; }
		}
		
		public ClientCertificateType[] CertificateTypes
		{
			get { return this.certificateTypes; }
			set { this.certificateTypes = value; }
		}

		public string[] DistinguisedNames
		{
			get { return this.distinguisedNames; }
			set { this.distinguisedNames = value; }
		}
		
		#endregion

		#region Constructors

		public TlsServerSettings()
		{
		}

		#endregion

		#region Methods

		public void UpdateCertificateRSA()
		{
			if (this.certificates == null ||
				this.certificates.Count == 0)
			{
				this.certificateRSA = null;
			}
			else
			{
				this.certificateRSA = new RSAManaged(
					this.certificates[0].RSA.KeySize);

				this.certificateRSA.ImportParameters(
					this.certificates[0].RSA.ExportParameters(false));
			}
		}

		#endregion
	}
}
