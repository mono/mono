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

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsServerHello : HandshakeMessage
	{
		#region Private Fields

		private int		unixTime;
		private byte[]	random;

		#endregion

		#region Constructors

		public TlsServerHello(Context context) 
			: base(context, HandshakeType.ServerHello)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			TlsStream random = new TlsStream();

			// Compute Server Random
			random.Write(this.unixTime);
			random.Write(this.random);

			this.Context.ServerRandom = random.ToArray();

			// Compute ClientRandom + ServerRandom
			random.Reset();
			random.Write(this.Context.ClientRandom);
			random.Write(this.Context.ServerRandom);

			this.Context.RandomCS = random.ToArray();

			// Server Random + Client Random
			random.Reset();
			random.Write(this.Context.ServerRandom);
			random.Write(this.Context.ClientRandom);

			this.Context.RandomSC = random.ToArray();

			random.Reset();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			// Write protocol version
			this.Write(this.Context.Protocol);
			
			// Write Unix time
			this.unixTime = this.Context.GetUnixTime();
			this.Write(this.unixTime);

			// Write Random bytes
			random = this.Context.GetSecureRandomBytes(28);
			this.Write(this.random);
						
			if (this.Context.SessionId == null)
			{
				this.WriteByte(0);
			}
			else
			{
				// Write Session ID length
				this.WriteByte((byte)this.Context.SessionId.Length);

				// Write Session ID
				this.Write(this.Context.SessionId);
			}

			// Write selected cipher suite
			this.Write(this.Context.Cipher.Code);
			
			// Write selected compression method
			this.WriteByte((byte)this.Context.CompressionMethod);
		}

		#endregion
	}
}