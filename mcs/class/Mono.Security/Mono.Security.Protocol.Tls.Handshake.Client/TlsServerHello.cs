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

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerHello : TlsHandshakeMessage
	{
		#region FIELDS

		private TlsProtocol			protocol;
		private byte[]				random;
		private byte[]				sessionId;
		private TlsCipherSuite		cipherSuite;
		private byte				compressionMethod;

		#endregion

		#region CONSTRUCTORS

		public TlsServerHello(TlsSession session, byte[] buffer) 
			: base(session, TlsHandshakeType.ServerHello, buffer)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();

			Session.SetSessionId(this.sessionId);
			Session.Context.ServerRandom		= this.random;
			Session.Context.Cipher				= this.cipherSuite;
			Session.Context.CompressionMethod	= this.compressionMethod;
			Session.Context.Cipher.SessionState	= Session.Context;
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void Parse()
		{
			// Read protocol version
			this.protocol	= (TlsProtocol)ReadInt16();
			
			// Read random  - Unix time + Random bytes
			this.random		= ReadBytes(32);
			
			// Read Session id
			int length = (int)ReadByte();
			if (length > 0)
			{
				this.sessionId = ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = ReadInt16();
			if (Session.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException("Invalid cipher suite received from server");
			}
			cipherSuite = Session.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			compressionMethod = ReadByte();
		}

		#endregion
	}
}