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


namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerKeyExchange : TlsHandshakeMessage
	{
		#region FIELDS

		private RSAParameters	rsaParams;
		private byte[]			signedParams;

		#endregion

		#region CONSTRUCTORS

		public TlsServerKeyExchange(TlsSession session, byte[] buffer)
			: base(session, TlsHandshakeType.ServerKeyExchange, buffer)
		{
			verify();
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();

			this.Session.Context.ServerSettings.ServerKeyExchange = true;
			this.Session.Context.ServerSettings.RsaParameters		= this.rsaParams;
			this.Session.Context.ServerSettings.SignedParams		= this.signedParams;
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			throw new NotSupportedException();
		}

		protected override void ProcessAsTls1()
		{
			rsaParams = new RSAParameters();
			
			// Read modulus
			short length		= ReadInt16();
			rsaParams.Modulus	= ReadBytes(length);

			// Read exponent
			length				= ReadInt16();
			rsaParams.Exponent	= ReadBytes(length);

			// Read signed params
			length				= ReadInt16();
			signedParams		= ReadBytes(length);
		}

		#endregion

		#region PRIVATE_METHODS

		private void verify()
		{
			MD5CryptoServiceProvider	md5 = new MD5CryptoServiceProvider();
			SHA1CryptoServiceProvider	sha = new SHA1CryptoServiceProvider();

			// Create server params array
			TlsStream stream = new TlsStream();

			stream.Write(Session.Context.ClientRandom);
			stream.Write(Session.Context.ServerRandom);
			stream.Write(rsaParams.Modulus.Length);
			stream.Write(rsaParams.Modulus);
			stream.Write(rsaParams.Exponent.Length);
			stream.Write(rsaParams.Exponent);
			byte[] serverParams = stream.ToArray();
			stream.Reset();

			// Compute md5 and sha hashes
			byte[] md5Hash = md5.ComputeHash(serverParams, 0, serverParams.Length);
			byte[] shaHash = sha.ComputeHash(serverParams, 0, serverParams.Length);

			// Calculate signature
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(rsaParams.Modulus.Length << 3);
			rsa.ImportParameters(rsaParams);

			#warning "Verify Signature here"

			// RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
		}

		#endregion
	}
}
