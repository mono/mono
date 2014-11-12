// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// RC2.cs
//

namespace System.Security.Cryptography {
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class RC2 : SymmetricAlgorithm
    {
        protected int               EffectiveKeySizeValue;
        private static  KeySizes[] s_legalBlockSizes = {
          new KeySizes(64, 64, 0)
        };
        private static  KeySizes[] s_legalKeySizes = {
            new KeySizes(40, 1024, 8)  // 1024 bits is theoretical max according to the RFC
        };
      
        //
        // protected constructors
        //
    
        protected RC2() {
            KeySizeValue = 128;
            BlockSizeValue = 64;
            FeedbackSizeValue = BlockSizeValue;
            LegalBlockSizesValue = s_legalBlockSizes;
            LegalKeySizesValue = s_legalKeySizes;
        }
    
        //
        // public properties
        //

        public virtual int EffectiveKeySize {
            get {
                if (EffectiveKeySizeValue == 0) return KeySizeValue;
                return EffectiveKeySizeValue;
            }
            set {
                if (value > KeySizeValue) {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_RC2_EKSKS"));
                } else if (value == 0) {
                    EffectiveKeySizeValue = value;
                } else if (value < 40) {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_RC2_EKS40"));
                } else {
                    if (ValidKeySize(value))
                        EffectiveKeySizeValue = value;
                    else
                        throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
                }
            }
        }

        public override int KeySize {
            get { return KeySizeValue; }
            set { 
                if (value < EffectiveKeySizeValue) throw new CryptographicException(Environment.GetResourceString("Cryptography_RC2_EKSKS"));
                base.KeySize = value;
            }
        }
        
        //
        // public methods
        //

        new static public RC2 Create() {
            return Create("System.Security.Cryptography.RC2");
        }

        new static public RC2 Create(String AlgName) {
            return (RC2) CryptoConfig.CreateFromName(AlgName);
        }    
    }
}    
