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

namespace Mono.Security.Protocol.Tls
{
	internal class SslHandshakeHash : System.Security.Cryptography.HashAlgorithm
	{
		#region Fields

		private HashAlgorithm	md5;
		private HashAlgorithm	sha;
		private bool			hashing;
		private byte[]			secret;
		private byte[]			innerPadMD5;
		private byte[]			outerPadMD5;
		private byte[]			innerPadSHA;
		private byte[]			outerPadSHA;

		#endregion

		#region Constructors

		public SslHandshakeHash(byte[] secret)
		{
			// Create md5 and sha1 hashes
			this.md5 = MD5.Create ();
			this.sha = SHA1.Create ();
			
			// Set HashSizeValue
			this.HashSizeValue = md5.HashSize + sha.HashSize;

			// Update secret
			this.secret = secret;

			this.Initialize();
		}

		#endregion

		#region Methods

		public override void Initialize()
		{
			this.md5.Initialize();
			this.sha.Initialize();
			this.initializePad();
			this.hashing = false;
		}

		protected override byte[] HashFinal()
		{
			if (!this.hashing)
			{
				this.hashing = true;
			}

			// Finalize the md5 hash
			this.md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			this.md5.TransformFinalBlock(this.innerPadMD5, 0, this.innerPadMD5.Length);

			byte[] firstResultMD5 = this.md5.Hash;

			this.md5.Initialize();
			this.md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			this.md5.TransformBlock(this.outerPadMD5, 0, this.outerPadMD5.Length, this.outerPadMD5, 0);
			this.md5.TransformFinalBlock(firstResultMD5, 0, firstResultMD5.Length);
			
			// Finalize the sha1 hash
			this.sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			this.sha.TransformFinalBlock(this.innerPadSHA, 0, this.innerPadSHA.Length);

			byte[] firstResultSHA = this.sha.Hash;
			
			this.sha.Initialize();
			this.sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			this.sha.TransformBlock(this.outerPadSHA, 0, this.outerPadSHA.Length, this.outerPadSHA, 0);
			this.sha.TransformFinalBlock(firstResultSHA, 0, firstResultSHA.Length);

			this.Initialize();

			byte[] result = new byte[36];

			Buffer.BlockCopy(this.md5.Hash, 0, result, 0, 16);
			Buffer.BlockCopy(this.sha.Hash, 0, result, 16, 20);

			return result;
		}

		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			if (!this.hashing)
			{
				this.hashing = true;
			}

			this.md5.TransformBlock(array, ibStart, cbSize, array, ibStart);
			this.sha.TransformBlock(array, ibStart, cbSize, array, ibStart);
		}

		public byte[] CreateSignature(RSA rsa) 
		{
			if (rsa == null)
			{
				throw new CryptographicUnexpectedOperationException ("missing key");
			}

			RSASslSignatureFormatter f = new RSASslSignatureFormatter(rsa);
			f.SetHashAlgorithm("MD5SHA1");

			return f.CreateSignature(this.Hash);
		}

		public bool VerifySignature(RSA rsa, byte[] rgbSignature) 
		{
			if (rsa == null)
			{
				throw new CryptographicUnexpectedOperationException ("missing key");
			}
			if (rgbSignature == null)
			{
				throw new ArgumentNullException ("rgbSignature");
			}

			RSASslSignatureDeformatter d = new RSASslSignatureDeformatter(rsa);
			d.SetHashAlgorithm("MD5SHA1");

			return d.VerifySignature(this.Hash, rgbSignature);
		}

		#endregion

		#region Private Methods

		private void initializePad()
		{
			// Fill md5 arrays
			this.innerPadMD5 = new byte[48];
			this.outerPadMD5 = new byte[48];

			/* Pad the key for inner and outer digest */
			for (int i = 0; i < 48; ++i) 
			{
				this.innerPadMD5[i] = 0x36;
				this.outerPadMD5[i] = 0x5C;
			}

			// Fill sha arrays
			this.innerPadSHA = new byte[40];
			this.outerPadSHA = new byte[40];

			/* Pad the key for inner and outer digest */
			for (int i = 0; i < 40; ++i) 
			{
				this.innerPadSHA[i] = 0x36;
				this.outerPadSHA[i] = 0x5C;
			}
		}

		#endregion
	}
}
