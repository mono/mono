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
	internal class RSASslSignatureFormatter : AsymmetricSignatureFormatter
	{
		#region Fields

		private RSA				key;
		private HashAlgorithm	hash;

		#endregion

		#region Constructors

		public RSASslSignatureFormatter()
		{
		}

		public RSASslSignatureFormatter(AsymmetricAlgorithm key)
		{
			this.SetKey(key);
		}

		#endregion

		#region Methods

		public override byte[] CreateSignature(byte[] rgbHash)
		{
			if (this.key == null)
			{
				throw new CryptographicUnexpectedOperationException("The key is a null reference");
			}
			if (hash == null)
			{
				throw new CryptographicUnexpectedOperationException("The hash algorithm is a null reference.");
			}
			if (rgbHash == null)
			{
				throw new ArgumentNullException("The rgbHash parameter is a null reference.");
			}

			return Mono.Security.Cryptography.PKCS1.Sign_v15(
				this.key,
				this.hash,
				rgbHash);
		}

		public override void SetHashAlgorithm(string strName)
		{
#if INSIDE_SYSTEM
			hash = new Mono.Security.Cryptography.MD5SHA1 ();
#else
			switch (strName)
			{
				case "MD5SHA1":
					this.hash = new Mono.Security.Cryptography.MD5SHA1();
					break;

				default:
					this.hash = HashAlgorithm.Create(strName);
					break;
			}
#endif
		}

		public override void SetKey(AsymmetricAlgorithm key)
		{
			if (!(key is RSA))
			{
				throw new ArgumentException("Specfied key is not an RSA key");
			}

			this.key = key as RSA;
		}

		#endregion
	}
}
