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
	internal class TlsServerFinished : HandshakeMessage
	{
		#region Constructors

		public TlsServerFinished(Context context, byte[] buffer) 
			: base(context, HandshakeType.Finished, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			// Reset Hahdshake messages information
			this.Context.HandshakeMessages.Reset();

			// Hahdshake is finished
			this.Context.HandshakeState = HandshakeState.Finished;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			// Compute handshake messages hashes
			HashAlgorithm hash = new SslHandshakeHash(this.Context.MasterSecret);

			TlsStream data = new TlsStream();
			data.Write(this.Context.HandshakeMessages.ToArray());
			data.Write((int)0x53525652);
			
			hash.TransformFinalBlock(data.ToArray(), 0, (int)data.Length);

			data.Reset();

			byte[] serverHash	= this.ReadBytes((int)Length);			
			byte[] clientHash	= hash.Hash;
			
			// Check server prf against client prf
			if (clientHash.Length != serverHash.Length)
			{
#warning Review that selected alert is correct
				throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid ServerFinished message received.");
			}
			for (int i = 0; i < serverHash.Length; i++)
			{
				if (clientHash[i] != serverHash[i])
				{
					throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid ServerFinished message received.");
				}
			}
		}

		protected override void ProcessAsTls1()
		{
			byte[]			serverPRF	= this.ReadBytes((int)Length);
			HashAlgorithm	hash		= new MD5SHA1();

			hash.ComputeHash(
				this.Context.HandshakeMessages.ToArray(), 
				0,
				(int)this.Context.HandshakeMessages.Length);

			byte[] clientPRF = this.Context.Cipher.PRF(this.Context.MasterSecret, "server finished", hash.Hash, 12);

			// Check server prf against client prf
			if (clientPRF.Length != serverPRF.Length)
			{
				throw new TlsException("Invalid ServerFinished message received.");
			}
			for (int i = 0; i < serverPRF.Length; i++)
			{
				if (clientPRF[i] != serverPRF[i])
				{
					throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid ServerFinished message received.");
				}
			}
		}

		#endregion
	}
}
