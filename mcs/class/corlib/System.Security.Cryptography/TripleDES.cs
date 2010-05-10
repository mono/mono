//
// TripleDES.cs: Handles TripleDES (abstract class)
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Security.Cryptography;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 46-3: TripleDES
//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf
// b.	ANSI X9.52
//	not free :-(
//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E52%2D1998

[ComVisible (true)]
public abstract class TripleDES : SymmetricAlgorithm {

	protected TripleDES ()
	{
		// from SymmetricAlgorithm
		KeySizeValue = 192;
		BlockSizeValue = 64;
		FeedbackSizeValue = 8;

		LegalKeySizesValue = new KeySizes [1];
		LegalKeySizesValue [0] = new KeySizes (128, 192, 64);

		LegalBlockSizesValue = new KeySizes [1];
		LegalBlockSizesValue [0] = new KeySizes (64, 64, 0);
	}

	public override byte[] Key {
		get {
			if (KeyValue == null) {
				// generate keys as long as we get weak keys
				GenerateKey ();
				while (IsWeakKey (KeyValue))
					GenerateKey ();
			}
			return (byte[]) KeyValue.Clone ();
		}
		set { 
			if (value == null)
				throw new ArgumentNullException ("Key");
			// this will check for both key size and weak keys
			if (IsWeakKey (value))
				throw new CryptographicException (Locale.GetText ("Weak Key"));
			KeyValue = (byte[]) value.Clone (); 
		}
	}

	// Triple DES is DES in EDE = Encrypt - Decrypt - Encrypt
	// with 2 keys (a,b)
	//	EDE = Encrypt (a) - Decrypt (b) - Encrypt (a)
        //	if a == b then TripleDES == DES(a) (hence weak key)
        // with 3 keys (a,b,c)
	//	EDE = Encrypt (a) - Decrypt (b) - Encrypt (c)
	//	if ( a == b ) then TripleDES == DES(c) (hence weak key)
	//	if ( b == c ) then TripleDES == DES(a) (hence weak key)
	public static bool IsWeakKey (byte[] rgbKey)
	{
		if (rgbKey == null)
			throw new CryptographicException (Locale.GetText ("Null Key"));
		// 128 bits (16 bytes) is 3 DES with 2 keys
		if (rgbKey.Length == 16) {
			// weak if first half == second half
			for (int i = 0; i < 8; i++)
				if (rgbKey [i] != rgbKey [i+8])
					return false;
		}
		// 192 bits (24 bytes) is 3 DES with 3 keys
		else if (rgbKey.Length == 24) {
			bool bFirstCase = true;	
			// weak if first third == second third
			for (int i = 0; i < 8; i++) {
				if (rgbKey [i] != rgbKey [i+8]) {
					bFirstCase = false;
					break;
				}
			}
			// weak if second third == third third 
			if (!bFirstCase) {
				for (int i = 8; i < 16; i++)
					if (rgbKey [i] != rgbKey [i+8])
						return false;
			}
		}
		else
			throw new CryptographicException (Locale.GetText ("Wrong Key Length"));

		return true;
	}

	public static new TripleDES Create ()
	{
		return Create ("System.Security.Cryptography.TripleDES");
	}

	public static new TripleDES Create (string str)
	{
		return (TripleDES) CryptoConfig.CreateFromName (str);
	}
}

}

