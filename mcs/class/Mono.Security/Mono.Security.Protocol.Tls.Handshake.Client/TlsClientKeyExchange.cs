// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientKeyExchange : HandshakeMessage
	{
		#region Constructors

		public TlsClientKeyExchange (Context context) : 
			base(context, HandshakeType.ClientKeyExchange)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			// a large chunk of code is common to both SSL3 and TLS1
			// SSL3 doesn't send the length of the buffer
			ProcessCommon (false);
		}

		protected override void ProcessAsTls1()
		{
			// a large chunk of code is common to both SSL3 and TLS1
			// TLS1 does send the length of the buffer
			ProcessCommon (true);
		}

		public void ProcessCommon (bool sendLength)
		{
			// Compute pre master secret
			byte[] preMasterSecret = this.Context.Negotiating.Cipher.CreatePremasterSecret ();

			// Create a new RSA key
			RSA rsa = null;
			if (this.Context.ServerSettings.ServerKeyExchange) 
			{
				// this is the case for "exportable" ciphers
				rsa = new RSAManaged ();
				rsa.ImportParameters (this.Context.ServerSettings.RsaParameters);
			}
			else
			{
				rsa = this.Context.ServerSettings.CertificateRSA;
			}

			// Encrypt premaster_sercret
			RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter (rsa);

			// Write the preMasterSecret encrypted
			byte[] buffer = formatter.CreateKeyExchange (preMasterSecret);
			if (sendLength)
				this.Write ((short) buffer.Length);
			this.Write (buffer);

			// Create master secret
			this.Context.Negotiating.Cipher.ComputeMasterSecret (preMasterSecret);

			// Create keys
			this.Context.Negotiating.Cipher.ComputeKeys ();

			// Clear resources
			rsa.Clear ();
		}

		#endregion
	}
}
