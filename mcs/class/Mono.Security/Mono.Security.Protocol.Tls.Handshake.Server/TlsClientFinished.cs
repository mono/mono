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

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsClientFinished : HandshakeMessage
	{
		#region Constructors

		public TlsClientFinished(Context context, byte[] buffer)
			: base(context, HandshakeType.Finished,  buffer)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			bool decryptError = false;

			// Compute handshake messages hashes
			HashAlgorithm hash = new SslHandshakeHash(this.Context.MasterSecret);

			TlsStream data = new TlsStream();
			data.Write(this.Context.HandshakeMessages.ToArray());
			data.Write((int)0x434C4E54);
			
			hash.TransformFinalBlock(data.ToArray(), 0, (int)data.Length);

			data.Reset();

			byte[] clientHash	= this.ReadBytes((int)Length);			
			byte[] serverHash	= hash.Hash;
			
			// Check client prf against server prf
			if (clientHash.Length != serverHash.Length)
			{
				decryptError = true;
			}
			else
			{
				for (int i = 0; i < clientHash.Length; i++)
				{
					if (clientHash[i] != serverHash[i])
					{
						decryptError = true;
						break;
					}
				}
			}

			if (decryptError)
			{
				throw new TlsException(AlertDescription.DecryptError, "Decrypt error.");
			}
		}

		protected override void ProcessAsTls1()
		{
			byte[]			clientPRF		= this.ReadBytes((int)this.Length);
			HashAlgorithm	hash			= new MD5SHA1();
			bool			decryptError	= false;

			hash.ComputeHash(
				this.Context.HandshakeMessages.ToArray(), 
				0,
				(int)this.Context.HandshakeMessages.Length);

			byte[] serverPRF = this.Context.Cipher.PRF(
				this.Context.MasterSecret, "client finished", hash.Hash, 12);

			// Check client prf against server prf
			if (clientPRF.Length != serverPRF.Length)
			{
				decryptError = true;
			}
			else
			{
				for (int i = 0; i < serverPRF.Length; i++)
				{
					if (clientPRF[i] != serverPRF[i])
					{
						decryptError = true;
						break;
					}
				}
			}

			if (decryptError)
			{
				throw new TlsException(AlertDescription.DecryptError, "Decrypt error.");
			}
		}

		#endregion
	}
}
