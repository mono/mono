//
// System.Security.Cryptography.RC2.cs
//
// Authors: 
//	Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (spouliot@motus.com)
//          

using System;

namespace System.Security.Cryptography {

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
				if (!IsLegalKeySize (LegalKeySizesValue, value))
					throw new CryptographicException ("key size not supported by algorithm");
				EffectiveKeySizeValue = value; 
			}
		}

		// Overridden, which makes me suspect it changes effective keysize too?
		public override int KeySize {
			get { return KeySizeValue; }
			set { KeySizeValue = value; }
		}
				
		public RC2 () 
		{
			KeySizeValue = 128;
			BlockSizeValue = 64;
			FeedbackSizeValue = 64;

			// The RFC allows keys of 1 to 128 bytes, but MS impl only supports
			// 40 to 128 bits, sigh.
			LegalKeySizesValue = new KeySizes[1];
			LegalKeySizesValue[0] = new KeySizes(40, 128, 8);

			LegalBlockSizesValue = new KeySizes[1];
			LegalBlockSizesValue[0] = new KeySizes(64, 64, 0);
		}
	}
}
