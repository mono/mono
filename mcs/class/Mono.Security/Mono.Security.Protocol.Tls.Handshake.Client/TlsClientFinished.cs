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
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientFinished : HandshakeMessage
	{
		#region Constructors

		public TlsClientFinished(Context context) 
			: base(context, HandshakeType.Finished)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Reset();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			// Compute handshake messages hashes
			HashAlgorithm hash = new SslHandshakeHash(this.Context.MasterSecret);

			TlsStream data = new TlsStream();
			data.Write(this.Context.HandshakeMessages.ToArray());
			data.Write((int)0x434C4E54);
			
			hash.TransformFinalBlock(data.ToArray(), 0, (int)data.Length);

			this.Write(hash.Hash);

			data.Reset();
		}

		protected override void ProcessAsTls1()
		{
			// Compute handshake messages hash
			HashAlgorithm hash = new MD5SHA1();
			hash.ComputeHash(
				this.Context.HandshakeMessages.ToArray(),
				0,
				(int)this.Context.HandshakeMessages.Length);

			// Write message
			Write(this.Context.Cipher.PRF(this.Context.MasterSecret, "client finished", hash.Hash, 12));
		}

		#endregion
	}
}
