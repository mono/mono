//
// TripleDES.cs: Handles TripleDES (abstract class)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 46-3: TripleDES
//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf
// b.	ANSI X9.52
//	not free :-(
//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E52%2D1998

public abstract class TripleDES : SymmetricAlgorithm {

	public TripleDES ()
	{
		// from SymmetricAlgorithm
		KeySizeValue = 192;
		BlockSizeValue = 64;
		FeedbackSizeValue = 64;

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
			return KeyValue;
		}
		set { 
			if (value == null)
				throw new ArgumentNullException ();
			// this will check for both key size and weak keys
			if (IsWeakKey (value))
				throw new CryptographicException ();
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
			throw new CryptographicException ();

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

