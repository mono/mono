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
	internal class TlsClientHello : TlsHandshakeMessage
	{
		#region FIELDS

		private byte[] random;

		#endregion

		#region CONSTRUCTORS

		public TlsClientHello(TlsSession session) 
			: base(session, TlsHandshakeType.ClientHello, TlsContentType.Handshake)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();

			Session.Context.ClientRandom = random;

			random = null;
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			// Client Version
			this.Write((short)this.Session.Context.Protocol);
								
			// Random bytes - Unix time + Radom bytes [28]
			TlsStream clientRandom = new TlsStream();
			clientRandom.Write(this.Session.Context.GetUnixTime());
			clientRandom.Write(this.Session.Context.GetSecureRandomBytes(28));
			this.random = clientRandom.ToArray();
			clientRandom.Reset();

			this.Write(this.random);

			// Session id
			// Send the session ID empty
			if (this.Session.SessionId != null)
			{
				this.Write((byte)this.Session.SessionId.Length);
				if (this.Session.SessionId.Length > 0)
				{
					this.Write(this.Session.SessionId);
				}
			}
			else
			{
				this.Write((byte)0);
			}
			
			// Write length of Cipher suites			
			this.Write((short)(this.Session.Context.SupportedCiphers.Count*2));

			// Write Supported Cipher suites
			for (int i = 0; i < this.Session.Context.SupportedCiphers.Count; i++)
			{
				this.Write((short)this.Session.Context.SupportedCiphers[i].Code);
			}

			// Compression methods length
			this.Write((byte)1);
			
			// Compression methods ( 0 = none )
			this.Write((byte)this.Session.Context.CompressionMethod);
		}

		#endregion
	}
}