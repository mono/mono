//
// RC4.cs: RC4(tm) symmetric stream cipher
//	RC4 is a trademark of RSA Security
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

public abstract class RC4 : SymmetricAlgorithm {

	private static KeySizes[] s_legalBlockSizes = {
		new KeySizes (64, 64, 0)
	};

	private static KeySizes[] s_legalKeySizes = {
		new KeySizes (40, 2048, 8)  
	};

	public RC4() 
	{
		KeySizeValue = 128;
		BlockSizeValue = 64;
		FeedbackSizeValue = BlockSizeValue;
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
	}

	new static public RC4 Create() 
	{
		return Create ("RC4");
	}

	new static public RC4 Create (string algName) 
	{
		object o = CryptoConfig.CreateFromName (algName);
		// in case machine.config isn't configured to use 
		// any RC4 implementation
		if (o == null) {
			o = new ARC4Managed ();
		}
		return (RC4) o;
	}
}

}
