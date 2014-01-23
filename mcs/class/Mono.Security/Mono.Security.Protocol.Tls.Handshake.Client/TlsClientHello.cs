// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.Net;
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientHello : HandshakeMessage
	{
		#region Fields

		private byte[] random;

		#endregion

		#region Constructors

		public TlsClientHello(Context context) 
			: base(context, HandshakeType.ClientHello)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			ClientContext context = (ClientContext)this.Context;

			base.Update();

			context.ClientRandom		= random;
			context.ClientHelloProtocol	= this.Context.Protocol;

			random = null;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			// Client Version
			this.Write(this.Context.Protocol);

			// Random bytes - Unix time + Radom bytes [28]
			TlsStream clientRandom = new TlsStream();
			clientRandom.Write(this.Context.GetUnixTime());
			clientRandom.Write(this.Context.GetSecureRandomBytes(28));
			this.random = clientRandom.ToArray();
			clientRandom.Reset();

			this.Write(this.random);

			// Session id
			// Check if we have a cache session we could reuse
			this.Context.SessionId = ClientSessionCache.FromHost (this.Context.ClientSettings.TargetHost);
			if (this.Context.SessionId != null)
			{
				this.Write((byte)this.Context.SessionId.Length);
				if (this.Context.SessionId.Length > 0)
				{
					this.Write(this.Context.SessionId);
				}
			}
			else
			{
				this.Write((byte)0);
			}
			
			// Write length of Cipher suites			
			this.Write((short)(this.Context.SupportedCiphers.Count*2));

			// Write Supported Cipher suites
			for (int i = 0; i < this.Context.SupportedCiphers.Count; i++)
			{
				this.Write((short)this.Context.SupportedCiphers[i].Code);
			}

			// Compression methods length
			this.Write((byte)1);
			
			// Compression methods ( 0 = none )
			this.Write((byte)this.Context.CompressionMethod);
		}

		protected override void ProcessAsTls1()
		{
			ProcessAsSsl3 ();

			// If applicable add the "server_name" extension to the hello message
			// http://www.ietf.org/rfc/rfc3546.txt
			string host = Context.ClientSettings.TargetHost;
			// Our TargetHost might be an address (not a host *name*) - see bug #8553
			// RFC3546 -> Literal IPv4 and IPv6 addresses are not permitted in "HostName".
			IPAddress addr;
			if (IPAddress.TryParse (host, out addr))
				return;

			TlsStream extensions = new TlsStream ();
			byte[] server_name = System.Text.Encoding.UTF8.GetBytes (host);
			extensions.Write ((short) 0x0000);			// ExtensionType: server_name (0)
			extensions.Write ((short) (server_name.Length + 5));	// ServerNameList (length)
			extensions.Write ((short) (server_name.Length + 3));	// ServerName (length)
			extensions.Write ((byte) 0x00);				// NameType: host_name (0)
			extensions.Write ((short) server_name.Length);		// HostName (length)
			extensions.Write (server_name);				// HostName (UTF8)
			this.Write ((short) extensions.Length);
			this.Write (extensions.ToArray ());
		}

		#endregion
	}
}