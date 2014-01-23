
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
/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
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

namespace Mono.Security.Cryptography
{
   /*
	* References:
	*		RFC 2104 (http://www.ietf.org/rfc/rfc2104.txt)
	*		RFC 2202 (http://www.ietf.org/rfc/rfc2202.txt)
	* MSDN:
	* 
	*		Extending the KeyedHashAlgorithm Class (http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconextendingkeyedhashalgorithmclass.asp)
	*/
	internal class HMAC : System.Security.Cryptography.KeyedHashAlgorithm
	{
		#region Fields

		private HashAlgorithm	hash;
		private bool			hashing;

		private byte[]			innerPad;
		private byte[]			outerPad;

		#endregion

		#region Properties
        
		public override byte[] Key
		{
			get { return (byte[])KeyValue.Clone(); }
			set
			{
				if (hashing)
				{
					throw new Exception("Cannot change key during hash operation.");
				}

				/* if key is longer than 64 bytes reset it to rgbKey = Hash(rgbKey) */
				if (value.Length > 64)
				{
					KeyValue = hash.ComputeHash(value);
				}
				else
				{
					KeyValue = (byte[])value.Clone();
				}

				initializePad();
			}
		}

		#endregion

		#region Constructors

		public HMAC()
		{
			// Create the hash
			hash = MD5.Create();
			// Set HashSizeValue
			HashSizeValue = hash.HashSize;

			// Generate a radom key
			byte[] rgbKey = new byte[64];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetNonZeroBytes(rgbKey);

			KeyValue = (byte[])rgbKey.Clone();

			this.Initialize();
		}

		public HMAC (HashAlgorithm ha, byte[] rgbKey)
		{
			hash = ha;
			// Set HashSizeValue
			HashSizeValue = hash.HashSize;

			/* if key is longer than 64 bytes reset it to rgbKey = Hash(rgbKey) */
			if (rgbKey.Length > 64)
			{
				KeyValue = hash.ComputeHash(rgbKey);
			}
			else
			{
				KeyValue = (byte[])rgbKey.Clone();
			}

			this.Initialize();
		}

		#endregion

		#region Methods

		public override void Initialize()
		{
			hash.Initialize();
			initializePad();
			hashing = false;
		}

		protected override byte[] HashFinal()
		{
			if (!hashing)
			{
				hash.TransformBlock(innerPad, 0, innerPad.Length, innerPad, 0);
				hashing = true;
			}
			// Finalize the original hash
			hash.TransformFinalBlock(new byte[0], 0, 0);

			byte[] firstResult = hash.Hash;

			hash.Initialize();
			hash.TransformBlock(outerPad, 0, outerPad.Length, outerPad, 0);
			hash.TransformFinalBlock(firstResult, 0, firstResult.Length);
			
			Initialize();

			return hash.Hash;
		}

		protected override void HashCore(
			byte[] array,
			int ibStart,
			int cbSize)
		{
			if (!hashing)
			{
				hash.TransformBlock(innerPad, 0, innerPad.Length, innerPad, 0);
				hashing = true;
			}
			hash.TransformBlock(array, ibStart, cbSize, array, ibStart);
		}

		#endregion

		#region Private Methods

		private void initializePad()
		{
			// Fill pad arrays
			innerPad = new byte[64];
			outerPad = new byte[64];

			/* Pad the key for inner and outer digest */
			for (int i = 0 ; i < KeyValue.Length; ++i)
			{
				innerPad[i] = (byte)(KeyValue[i] ^ 0x36);
				outerPad[i] = (byte)(KeyValue[i] ^ 0x5C);
			}
			for (int i = KeyValue.Length; i < 64; ++i) 
			{
				innerPad[i] = 0x36;
				outerPad[i] = 0x5C;
			}
		}

		#endregion
	}
}
