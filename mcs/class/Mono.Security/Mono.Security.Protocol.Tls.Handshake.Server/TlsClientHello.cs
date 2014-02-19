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

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsClientHello : HandshakeMessage
	{
		#region Private Fields

		private byte[]	random;
		private byte[]	sessionId;
		private short[]	cipherSuites;
		private byte[]	compressionMethods;

		#endregion

		#region Constructors

		public TlsClientHello(Context context, byte[] buffer)
			: base(context, HandshakeType.ClientHello, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			this.selectCipherSuite();
			this.selectCompressionMethod();

			this.Context.SessionId			= this.sessionId;
			this.Context.ClientRandom		= this.random;
			this.Context.ProtocolNegotiated	= true;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			// Client Version
			this.processProtocol(this.ReadInt16());
								
			// Random bytes - Unix time + Radom bytes [28]
			this.random = this.ReadBytes(32);
			
			// Session id
			// Send the session ID empty
			this.sessionId = this.ReadBytes(this.ReadByte());
			
			// Read Supported Cipher Suites count
			this.cipherSuites = new short[this.ReadInt16()/2];

			// Read Cipher Suites
			for (int i = 0; i < this.cipherSuites.Length; i++)
			{
				this.cipherSuites[i] = this.ReadInt16();
			}

			// Compression methods length
			this.compressionMethods = new byte[this.ReadByte()];
			
			for (int i = 0; i < this.compressionMethods.Length; i++)
			{
				this.compressionMethods[i] = this.ReadByte();
			}
		}

		#endregion

		#region Private Methods

		private void processProtocol(short protocol)
		{
			// a server MUST reply with the hight version supported (`true` for fallback)
			// so a TLS 1.2 client (like Google Chrome) will be returned that the server uses TLS 1.0
			// instead of an alert about the protocol
			SecurityProtocolType clientProtocol = Context.DecodeProtocolCode (protocol, true);

			if ((clientProtocol & this.Context.SecurityProtocolFlags) == clientProtocol ||
				(this.Context.SecurityProtocolFlags & SecurityProtocolType.Default) == SecurityProtocolType.Default)
			{
				this.Context.SecurityProtocol = clientProtocol;
				this.Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (true, clientProtocol);
			}
			else
			{
				throw new TlsException(AlertDescription.ProtocolVersion, "Incorrect protocol version received from server");
			}
		}

		private void selectCipherSuite()
		{
			int index = 0;

			for (int i = 0; i < this.cipherSuites.Length; i++)
			{
				if ((index = this.Context.SupportedCiphers.IndexOf(this.cipherSuites[i])) != -1)	
				{
					this.Context.Negotiating.Cipher = this.Context.SupportedCiphers[index];
					break;
				}
			}

			if (this.Context.Negotiating.Cipher == null)
			{
				throw new TlsException(AlertDescription.InsuficientSecurity, "Insuficient Security");
			}
		}

		private void selectCompressionMethod()
		{
			this.Context.CompressionMethod = SecurityCompressionType.None;
		}

		#endregion
	}
}
