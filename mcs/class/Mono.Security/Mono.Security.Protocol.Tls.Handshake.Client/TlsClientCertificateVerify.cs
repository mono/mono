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
using System.Security.Cryptography.X509Certificates;

using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientCertificateVerify : TlsHandshakeMessage
	{
		#region CONSTRUCTORS

		public TlsClientCertificateVerify(TlsSession session) 
			: base(session, TlsHandshakeType.Finished, TlsContentType.Handshake)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Reset();
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			throw new NotSupportedException();
		}

		protected override void ProcessAsTls1()
		{
			foreach (X509Certificate cert in Session.Settings.Certificates)
			{
				MD5SHA1CryptoServiceProvider hash = new MD5SHA1CryptoServiceProvider();
				X509.X509Certificate	c	 = new X509.X509Certificate(cert.GetRawCertData());
				RSA						rsa	 = c.RSA;
				RSAParameters			p	 = rsa.ExportParameters(false);
				TlsStream				data = new TlsStream();

				data.Write(this.Session.Context.RandomCS);
				data.Write((short)p.Modulus.Length);
				data.Write(p.Modulus);
				data.Write((short)p.Exponent.Length);
				data.Write(p.Exponent);

				hash.ComputeHash(data.ToArray(), 0, (int)data.Length);

				data.Reset();

				// Write the signature
				Write(hash.CreateSignature(rsa));
			}
		}

		#endregion
	}
}
