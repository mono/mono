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
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsSslHandshakeHash : System.Security.Cryptography.HashAlgorithm
	{
		#region FIELDS

		private HashAlgorithm	md5;
		private HashAlgorithm	sha;
		private bool			hashing;
		private byte[]			secret;
		private byte[]			innerPadMD5;
		private byte[]			outerPadMD5;
		private byte[]			innerPadSHA;
		private byte[]			outerPadSHA;

		#endregion

		#region CONSTRUCTORS

		public TlsSslHandshakeHash(byte[] secret)
		{
			// Create md5 and sha1 hashes
			this.md5 = HashAlgorithm.Create("MD5");
			this.sha = HashAlgorithm.Create("SHA1");
			
			// Set HashSizeValue
			this.HashSizeValue = md5.HashSize + sha.HashSize;

			// Update secret
			this.secret = secret;

			this.Initialize();
		}

		#endregion

		#region METHODS

		public override void Initialize()
		{
			md5.Initialize();
			sha.Initialize();
			initializePad();
			hashing = false;
		}

		protected override byte[] HashFinal()
		{
			if (!hashing)
			{
				hashing = true;
			}

			// Finalize the md5 hash
			md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			md5.TransformFinalBlock(this.innerPadMD5, 0, this.innerPadMD5.Length);

			byte[] firstResultMD5 = md5.Hash;

			md5.Initialize();
			md5.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			md5.TransformBlock(this.outerPadMD5, 0, this.outerPadMD5.Length, this.outerPadMD5, 0);
			md5.TransformFinalBlock(firstResultMD5, 0, firstResultMD5.Length);
			
			// Finalize the sha1 hash
			sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			sha.TransformFinalBlock(this.innerPadSHA, 0, this.innerPadSHA.Length);

			byte[] firstResultSHA = sha.Hash;
			
			sha.Initialize();
			sha.TransformBlock(this.secret, 0, this.secret.Length, this.secret, 0);
			sha.TransformBlock(this.outerPadSHA, 0, this.outerPadSHA.Length, this.outerPadSHA, 0);
			sha.TransformFinalBlock(firstResultSHA, 0, firstResultSHA.Length);

			this.Initialize();

			byte[] result = new byte[36];

			System.Array.Copy(md5.Hash, 0, result, 0, 16);
			System.Array.Copy(sha.Hash, 0, result, 16, 20);

			return result;
		}

		protected override void HashCore(
			byte[] array,
			int ibStart,
			int cbSize)
		{
			if (!hashing)
			{
				hashing = true;
			}
			md5.TransformBlock(array, ibStart, cbSize, array, ibStart);
			sha.TransformBlock(array, ibStart, cbSize, array, ibStart);
		}

		#endregion

		#region PRIVATE_METHODS

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
