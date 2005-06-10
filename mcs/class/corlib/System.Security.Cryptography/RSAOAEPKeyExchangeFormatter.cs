//
// RSAOAEPKeyExchangeFormatter.cs - Handles OAEP keyex encryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
	public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter {
	
		private RSA rsa;
		private RandomNumberGenerator random;
		private byte[] param;
	
		public RSAOAEPKeyExchangeFormatter () 
		{
			rsa = null;
		}
	
		public RSAOAEPKeyExchangeFormatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public byte[] Parameter {
			get { return param; }
			set { param = value; }
		}
	
		public override string Parameters {
			get { return null; }
		}
	
		public RandomNumberGenerator Rng {
			get { return random; }
			set { random = value; }
		}
	
		public override byte[] CreateKeyExchange (byte[] rgbData) 
		{
			if (random == null)
				random = RandomNumberGenerator.Create ();  // create default
#if NET_2_0
			if (rsa == null) {
				string msg = Locale.GetText ("No RSA key specified");
				throw new CryptographicUnexpectedOperationException (msg);
			}
#endif
			SHA1 sha1 = SHA1.Create ();
			return PKCS1.Encrypt_OAEP (rsa, sha1, random, rgbData);
		}
	
		public override byte[] CreateKeyExchange (byte[] rgbData, Type symAlgType) 
		{
			// documentation says that symAlgType is not used !?!
			return CreateKeyExchange (rgbData);
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA) key;
		}
	}
}
