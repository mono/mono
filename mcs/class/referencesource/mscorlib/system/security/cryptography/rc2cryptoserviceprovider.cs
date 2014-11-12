// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// RC2CryptoServiceProvider.cs
//

namespace System.Security.Cryptography {

    using System.Globalization;
    using System.Diagnostics.Contracts;

    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class RC2CryptoServiceProvider : RC2 {
        private bool m_use40bitSalt = false;

        private static  KeySizes[] s_legalKeySizes = {
            new KeySizes(40, 128, 8)  // cryptoAPI implementation only goes up to 128
        };

        //
        // public constructors
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public RC2CryptoServiceProvider () {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            Contract.EndContractBlock();
            if (!Utils.HasAlgorithm(Constants.CALG_RC2, 0))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgorithmNotAvailable"));

            // Acquire a Type 1 provider. This will be the Enhanced provider if available, otherwise 
            // it will be the base provider.
            LegalKeySizesValue = s_legalKeySizes;

            // Since the CSP only supports a CFB feedback of 8, make that the default
            FeedbackSizeValue = 8;
        }

        //
        // public methods
        //

        public override int EffectiveKeySize {
            get {
                return KeySizeValue;
            }
            set {
                if (value != KeySizeValue)
                    throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_RC2_EKSKS2"));
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public bool UseSalt {
            get {
                return m_use40bitSalt;
            }
            set {
                m_use40bitSalt = value;
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) {
            return _NewEncryptor(rgbKey, ModeValue, rgbIV, EffectiveKeySizeValue, 
                                 FeedbackSizeValue, CryptoAPITransformMode.Encrypt);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) {
            return _NewEncryptor(rgbKey, ModeValue, rgbIV, EffectiveKeySizeValue,
                                 FeedbackSizeValue, CryptoAPITransformMode.Decrypt);
        }

        public override void GenerateKey () {
            KeyValue = new byte[KeySizeValue/8];
            Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
        }

        public override void GenerateIV () {
            // block size is always 64 bits so IV is always 64 bits == 8 bytes
            IVValue = new byte[8];
            Utils.StaticRandomNumberGenerator.GetBytes(IVValue);
        }

        //
        // private methods
        //

        [System.Security.SecurityCritical]  // auto-generated
        private ICryptoTransform _NewEncryptor (byte[] rgbKey, CipherMode mode, byte[] rgbIV,
                                                int effectiveKeySize, int feedbackSize, CryptoAPITransformMode encryptMode) {
            int cArgs = 0;
            int[] rgArgIds = new int[10];
            Object[] rgArgValues = new Object[10];

            // Check for bad values
            // 1) we don't support OFB mode in RC2_CSP
            if (mode == CipherMode.OFB)
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_OFBNotSupported"));
            // 2) we only support CFB with a feedback size of 8 bits
            if ((mode == CipherMode.CFB) && (feedbackSize != 8)) 
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_CFBSizeNotSupported"));

            if (rgbKey == null) {
                rgbKey = new byte[KeySizeValue/8];
                Utils.StaticRandomNumberGenerator.GetBytes(rgbKey);
            }

            // Check the rgbKey size
            int keySizeValue = rgbKey.Length * 8;
            if (!ValidKeySize(keySizeValue))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));

            // Deal with effective key length questions
            rgArgIds[cArgs] = Constants.KP_EFFECTIVE_KEYLEN;
            if (EffectiveKeySizeValue == 0) {
                rgArgValues[cArgs] = keySizeValue;
            } else {
                rgArgValues[cArgs] = effectiveKeySize;
            }
            cArgs += 1;

            // Set the mode for the encryptor (defaults to CBC)
            if (mode != CipherMode.CBC) {
                rgArgIds[cArgs] = Constants.KP_MODE;
                rgArgValues[cArgs] = mode;
                cArgs += 1;
            }

            // If not ECB mode -- pass in an IV
            if (mode != CipherMode.ECB) {
                if (rgbIV == null) {
                    rgbIV = new byte[8];
                    Utils.StaticRandomNumberGenerator.GetBytes(rgbIV);
                }

                //
                // We truncate IV's that are longer than the block size to 8 bytes : this is
                // done to maintain backward compatibility with the behavior shipped in V1.x.
                // The call to set the IV in CryptoAPI will ignore any bytes after the first 8
                // bytes. We'll still reject IV's that are shorter than the block size though.
                //
                if (rgbIV.Length < 8)
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidIVSize"));

                rgArgIds[cArgs] = Constants.KP_IV;
                rgArgValues[cArgs] = rgbIV;
                cArgs += 1;
            }

            // If doing OFB or CFB, then we need to set the feed back loop size
            if ((mode == CipherMode.OFB) || (mode == CipherMode.CFB)) {
                rgArgIds[cArgs] = Constants.KP_MODE_BITS;
                rgArgValues[cArgs] = feedbackSize;
                cArgs += 1;
            }

            if (!Utils.HasAlgorithm(Constants.CALG_RC2, keySizeValue))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgKeySizeNotAvailable", keySizeValue));

            //  Create the encryptor/decryptor object
            return new CryptoAPITransform(Constants.CALG_RC2, cArgs, rgArgIds, 
                                          rgArgValues, rgbKey, PaddingValue, 
                                          mode, BlockSizeValue, feedbackSize, m_use40bitSalt,
                                          encryptMode);
        }
    }
}
