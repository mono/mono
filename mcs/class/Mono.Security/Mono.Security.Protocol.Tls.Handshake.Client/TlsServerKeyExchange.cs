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

using Mono.Security.Cryptography;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerKeyExchange : TlsHandshakeMessage
	{
		#region FIELDS

		private RSAParameters	rsaParams;
		private byte[]			signedParams;

		#endregion

		#region CONSTRUCTORS

		public TlsServerKeyExchange(TlsContext context, byte[] buffer)
			: base(context, TlsHandshakeType.ServerKeyExchange, buffer)
		{
			this.verifySignature();
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();

			this.Context.ServerSettings.ServerKeyExchange	= true;
			this.Context.ServerSettings.RsaParameters		= this.rsaParams;
			this.Context.ServerSettings.SignedParams		= this.signedParams;
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			this.rsaParams = new RSAParameters();
			
			// Read modulus
			rsaParams.Modulus	= this.ReadBytes(this.ReadInt16());

			// Read exponent
			rsaParams.Exponent	= this.ReadBytes(this.ReadInt16());

			// Read signed params
			signedParams		= this.ReadBytes(this.ReadInt16());
		}

		#endregion

		#region PRIVATE_METHODS

		private void verifySignature()
		{
			MD5SHA1 hash = new MD5SHA1();

			// Create server params array
			TlsStream stream = new TlsStream();

			stream.Write(this.Context.RandomCS);
			stream.Write(rsaParams.Modulus.Length);
			stream.Write(rsaParams.Modulus);
			stream.Write(rsaParams.Exponent.Length);
			stream.Write(rsaParams.Exponent);

			hash.ComputeHash(stream.ToArray());

			stream.Reset();

			// Verify Signature
			X509Certificate certificate = this.Context.ServerSettings.Certificates[0];

			RSA rsa = RSA.Create();
			
			rsa.KeySize = rsaParams.Modulus.Length << 3;
			rsa.ImportParameters(rsaParams);

			byte[] sign = hash.CreateSignature(rsa);
			hash.VerifySignature(rsa, this.signedParams);
		}

		#endregion
	}
}
