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

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerHello : HandshakeMessage
	{
		#region Fields

		private SecurityCompressionType	compressionMethod;
		private byte[]					random;
		private byte[]					sessionId;
		private CipherSuite	cipherSuite;
		
		#endregion

		#region Constructors

		public TlsServerHello(Context context, byte[] buffer) 
			: base(context, HandshakeType.ServerHello, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			this.Context.SessionId			= this.sessionId;
			this.Context.ServerRandom		= this.random;
			this.Context.Negotiating.Cipher = this.cipherSuite;
			this.Context.CompressionMethod	= this.compressionMethod;
			this.Context.ProtocolNegotiated	= true;

			DebugHelper.WriteLine("Selected Cipher Suite {0}", this.cipherSuite.Name);
			DebugHelper.WriteLine("Client random", this.Context.ClientRandom);
			DebugHelper.WriteLine("Server random", this.Context.ServerRandom);
			
			// Compute ClientRandom + ServerRandom
			int clen = this.Context.ClientRandom.Length;
			int slen = this.Context.ServerRandom.Length;
			int rlen = clen + slen;
			byte[] cs = new byte[rlen];
			Buffer.BlockCopy (this.Context.ClientRandom, 0, cs, 0, clen);
			Buffer.BlockCopy (this.Context.ServerRandom, 0, cs, clen, slen);
			this.Context.RandomCS = cs;
			
			// Server Random + Client Random
			byte[] sc = new byte[rlen];
			Buffer.BlockCopy (this.Context.ServerRandom, 0, sc, 0, slen);
			Buffer.BlockCopy (this.Context.ClientRandom, 0, sc, slen, clen);
			this.Context.RandomSC = sc;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			// Read protocol version
			this.processProtocol(this.ReadInt16());
			
			// Read random  - Unix time + Random bytes
			this.random	= this.ReadBytes(32);

			// Read Session id
			int length = (int) ReadByte ();
			if (length > 0)
			{
				this.sessionId = this.ReadBytes(length);
				ClientSessionCache.Add (this.Context.ClientSettings.TargetHost, this.sessionId);
				this.Context.AbbreviatedHandshake = Compare (this.sessionId, this.Context.SessionId);
			} 
			else
			{
				this.Context.AbbreviatedHandshake = false;
			}

			// Read cipher suite
			short cipherCode = this.ReadInt16();
			if (this.Context.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid cipher suite received from server");
			}
			this.cipherSuite = this.Context.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			this.compressionMethod = (SecurityCompressionType)this.ReadByte();
		}

		#endregion

		#region Private Methods

		private void processProtocol(short protocol)
		{
			SecurityProtocolType serverProtocol = this.Context.DecodeProtocolCode(protocol);

			if ((serverProtocol & this.Context.SecurityProtocolFlags) == serverProtocol ||
				(this.Context.SecurityProtocolFlags & SecurityProtocolType.Default) == SecurityProtocolType.Default)
			{
				this.Context.SecurityProtocol = serverProtocol;
				this.Context.SupportedCiphers.Clear();
				this.Context.SupportedCiphers = null;
				this.Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(serverProtocol);

				DebugHelper.WriteLine("Selected protocol {0}", serverProtocol);
			}
			else
			{
				throw new TlsException(
					AlertDescription.ProtocolVersion,
					"Incorrect protocol version received from server");
			}
		}

		#endregion
	}
}
