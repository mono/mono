//
// System.Security.Cryptography.Rijndael.cs
//
// Authors: Dan Lewis (dihlewis@yahoo.co.uk)
//          Andrew Birkett (andy@nobugs.org)
//
// (C) 2002
// (C) 2004 Novell (http://www.novell.com)
//

using System;

namespace System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 197: Advanced Encryption Standard
	//	http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf

	public abstract class Rijndael : SymmetricAlgorithm {

		public static new Rijndael Create () 
		{
			return Create ("System.Security.Cryptography.Rijndael");
		}

		public static new Rijndael Create (string algName) 
		{
			return (Rijndael) CryptoConfig.CreateFromName (algName);
		}
		
		public Rijndael () 
		{
			KeySizeValue = 256;
			BlockSizeValue = 128;
			FeedbackSizeValue = 128;
	
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (128, 256, 64);

			LegalBlockSizesValue = new KeySizes [1];
			LegalBlockSizesValue [0] = new KeySizes (128, 256, 64);
		}
	}
}
