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

		private TlsProtocol				protocol;
		private TlsCompressionMethod	compressionMethod;
		private byte[]					random;
		private byte[]					sessionId;
		private CipherSuite	cipherSuite;
		
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

			this.Session.SetSessionId(this.sessionId);
			this.Session.Context.ServerRandom		= this.random;
			this.Session.Context.Cipher				= this.cipherSuite;
			this.Session.Context.CompressionMethod	= this.compressionMethod;
			this.Session.Context.Cipher.Context		= this.Session.Context;

			// Compute ClientRandom + ServerRandom
			TlsStream random = new TlsStream();
			random.Write(this.Session.Context.ClientRandom);
			random.Write(this.Session.Context.ServerRandom);
			this.Session.Context.RandomCS = random.ToArray();

			// Server Random + Client Random
			random.Reset();
			random.Write(this.Session.Context.ServerRandom);
			random.Write(this.Session.Context.ClientRandom);

			this.Session.Context.RandomSC = random.ToArray();
			random.Reset();
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			#warning "Check that the protocol sent by the server is supported"
			// Read protocol version
			this.protocol	= (TlsProtocol)this.ReadInt16();
			
			// Read random  - Unix time + Random bytes
			this.random		= this.ReadBytes(32);
			
			// Read Session id
			int length = (int)ReadByte();
			if (length > 0)
			{
				this.sessionId = this.ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = this.ReadInt16();
			if (this.Session.Context.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException("Invalid cipher suite received from server");
			}
			this.cipherSuite = this.Session.Context.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			this.compressionMethod = (TlsCompressionMethod)this.ReadByte();
		}

		protected override void ProcessAsTls1()
		{
			// Read protocol version
			this.protocol	= (TlsProtocol)this.ReadInt16();
			
			// Read random  - Unix time + Random bytes
			this.random		= this.ReadBytes(32);
			
			// Read Session id
			int length = (int)ReadByte();
			if (length > 0)
			{
				this.sessionId = this.ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = this.ReadInt16();
			if (this.Session.Context.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException("Invalid cipher suite received from server");
			}
			this.cipherSuite = this.Session.Context.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			this.compressionMethod = (TlsCompressionMethod)this.ReadByte();
		}

		#endregion
	}
}