//
// RSAPKCS1KeyExchangeDeformatter.cs - Handles PKCS#1 v.1.5 keyex decryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

#if NET_2_0
	[ComVisible (true)]
#endif
	public class RSAPKCS1KeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {
	
		private RSA rsa;
//		private string param;
		private RandomNumberGenerator random;
	
		public RSAPKCS1KeyExchangeDeformatter () 
		{
		}
	
		public RSAPKCS1KeyExchangeDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override string Parameters {
			get { return null; }
			set { ; }
		}
	
		public RandomNumberGenerator RNG {
			get { return random; }
			set { random = value; }
		}
	
		public override byte[] DecryptKeyExchange (byte[] rgbIn) 
		{
			if (rsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("No key pair available."));
			}

			byte[] result = PKCS1.Decrypt_v15 (rsa, rgbIn);
			if (result != null)
				return result;

			throw new CryptographicException (Locale.GetText ("PKCS1 decoding error."));
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA)key;
		}
	}
}
