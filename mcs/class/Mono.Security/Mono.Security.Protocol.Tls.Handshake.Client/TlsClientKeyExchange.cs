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
using System.IO;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientKeyExchange : TlsHandshakeMessage
	{
		#region CONSTRUCTORS

		public TlsClientKeyExchange (TlsSession session) : 
			base(session,
				TlsHandshakeType.ClientKeyExchange, 
				TlsContentType.Handshake)
		{
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			// Compute pre master secret
			byte[] preMasterSecret = this.Session.Context.Cipher.CreatePremasterSecret();

			// Create a new RSA key
			RSA rsa = this.Session.Context.Cipher.CreateRSA();
			
			// Encrypt premaster_sercret
			RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(rsa);

			// Write the preMasterSecret encrypted
			byte[] buffer = formatter.CreateKeyExchange(preMasterSecret);
			this.Write(buffer);

			// Create master secret
			this.Session.Context.Cipher.ComputeMasterSecret(preMasterSecret);

			// Create keys
			this.Session.Context.Cipher.ComputeKeys();

			// Clear resources
			rsa.Clear();
		}

		protected override void ProcessAsTls1()
		{
			// Compute pre master secret
			byte[] preMasterSecret = this.Session.Context.Cipher.CreatePremasterSecret();

			// Create a new RSA key
			RSA rsa = this.Session.Context.Cipher.CreateRSA();
			
			// Encrypt premaster_sercret
			RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(rsa);

			// Write the preMasterSecret encrypted
			byte[] buffer = formatter.CreateKeyExchange(preMasterSecret);
			this.Write((short)buffer.Length);
			this.Write(buffer);

			// Create master secret
			this.Session.Context.Cipher.ComputeMasterSecret(preMasterSecret);

			// Create keys
			this.Session.Context.Cipher.ComputeKeys();

			// Clear resources
			rsa.Clear();
		}

		#endregion
	}
}
