//
// System.Security.Cryptography.RC2.cs
//
// Authors: 
//	Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//          

using System;

namespace System.Security.Cryptography {

	// References:
	// a.	IETF RFC2286: A Description of the RC2(r) Encryption Algorithm
	//	http://www.ietf.org/rfc/rfc2268.txt

	public abstract class RC2 : SymmetricAlgorithm {

		public static new RC2 Create () 
		{
			return Create ("System.Security.Cryptography.RC2");
		}
		
		public static new RC2 Create (string algName) 
		{
			return (RC2) CryptoConfig.CreateFromName (algName);
		}

		protected int EffectiveKeySizeValue;

		public virtual int EffectiveKeySize {
			get {
				if (EffectiveKeySizeValue == 0)
					return KeySizeValue;
				else
					return EffectiveKeySizeValue;
			}
			set {
				EffectiveKeySizeValue = value; 
			}
		}

		public override int KeySize {
			get { return base.KeySize; }
			set {
				base.KeySize = value;
				EffectiveKeySizeValue = value;
			}
		}
				
		public RC2 () 
		{
			KeySizeValue = 128;
			BlockSizeValue = 64;
			FeedbackSizeValue = 64;

			// The RFC allows keys of 1 to 128 bytes, but MS impl only supports
			// 40 to 128 bits, sigh.
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (40, 128, 8);

			LegalBlockSizesValue = new KeySizes [1];
			LegalBlockSizesValue [0] = new KeySizes (64, 64, 0);
		}
	}
}
