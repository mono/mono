//
// RSAPKCS1SignatureDeformatter.cs - Handles PKCS#1 v.1.5 signature decryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
	
	[ComVisible (true)]
	public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter {
	
		private RSA rsa;
		private string hashName;
	
		public RSAPKCS1SignatureDeformatter ()
		{
		}
	
		public RSAPKCS1SignatureDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override void SetHashAlgorithm (string strName) 
		{
			if (strName == null)
				throw new ArgumentNullException ("strName");
			hashName = strName;
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			rsa = (RSA) key;
		}
	
		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
		{
			if (rsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("No public key available."));
			}
			if (hashName == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("Missing hash algorithm."));
			}
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");

			return PKCS1.Verify_v15 (rsa, hashName, rgbHash, rgbSignature);
		}
	}
}
