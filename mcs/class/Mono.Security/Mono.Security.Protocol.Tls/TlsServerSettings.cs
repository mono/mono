/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Security.Cryptography;

using Mono.Security.X509;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsServerSettings
	{
		#region FIELDS

		private X509CertificateCollection	certificates;
		private bool						serverKeyExchange;
		private bool						certificateRequest;
		private	TlsClientCertificateType[]	certificateTypes;
		private string[]					distinguisedNames;
		private RSAParameters				rsaParameters;
		private byte[]						signedParams;		

		#endregion

		#region PROPERTIES
		
		public bool	ServerKeyExchange
		{
			get { return serverKeyExchange; }
			set { serverKeyExchange = value; }
		}

		public RSAParameters RsaParameters
		{
			get { return rsaParameters; }
			set { rsaParameters = value; }
		}

		public byte[] SignedParams
		{
			get { return signedParams; }
			set { signedParams = value; }
		}

		public bool	CertificateRequest
		{
			get { return certificateRequest; }
			set { certificateRequest = value; }
		}
		
		public TlsClientCertificateType[] CertificateTypes
		{
			get { return certificateTypes; }
			set { certificateTypes = value; }
		}

		public string[] DistinguisedNames
		{
			get { return distinguisedNames; }
			set { distinguisedNames = value; }
		}
		
		public X509CertificateCollection Certificates
		{
			get { return certificates; }
			set { certificates = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsServerSettings()
		{
		}

		#endregion
	}
}
