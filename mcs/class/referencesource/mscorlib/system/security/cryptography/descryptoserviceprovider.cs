// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// DESCryptoServiceProvider.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class DESCryptoServiceProvider : DES {
        //
        // public constructors
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public DESCryptoServiceProvider () {
            if (!Utils.HasAlgorithm(Constants.CALG_DES, 0)) 
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_AlgorithmNotAvailable"));
            // Since the CSP only supports a CFB feedback of 8, make that the default
            FeedbackSizeValue = 8;
        }

        //
        // public methods
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) {
            if (IsWeakKey(rgbKey))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"),"DES");
            if (IsSemiWeakKey(rgbKey))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_SemiWeak"),"DES");

            return _NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, CryptoAPITransformMode.Encrypt);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) {
            if (IsWeakKey(rgbKey))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"),"DES");
            if (IsSemiWeakKey(rgbKey))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_SemiWeak"),"DES");

            return _NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, CryptoAPITransformMode.Decrypt);
        }

        public override void GenerateKey () {
            KeyValue = new byte[8];
            Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
            // Never hand back a weak or semi-weak key
            while (DES.IsWeakKey(KeyValue) || DES.IsSemiWeakKey(KeyValue)) {
                Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
            }
        }

        public override void GenerateIV () {
            IVValue = new byte[8];
            Utils.StaticRandomNumberGenerator.GetBytes(IVValue);
        }

        //
        // private methods
        //

        [System.Security.SecurityCritical]  // auto-generated
        private ICryptoTransform _NewEncryptor (byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize, CryptoAPITransformMode encryptMode) {
            int cArgs = 0;
            int[] rgArgIds = new int[10];
            Object[] rgArgValues = new Object[10];

            // Check for bad values
            // 1) we don't support OFB mode in DESCryptoServiceProvider
            if (mode == CipherMode.OFB)
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_OFBNotSupported"));
            // 2) we only support CFB with a feedback size of 8 bits
            if ((mode == CipherMode.CFB) && (feedbackSize != 8)) 
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_CFBSizeNotSupported"));

            // Build the key if one does not already exist
            if (rgbKey == null) {
                rgbKey = new byte[8];
                Utils.StaticRandomNumberGenerator.GetBytes(rgbKey);
            }

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

            // Create the encryptpr/decryptor object
            return new CryptoAPITransform(Constants.CALG_DES, cArgs, rgArgIds, 
                                          rgArgValues, rgbKey, PaddingValue,
                                          mode, BlockSizeValue, feedbackSize, false,
                                          encryptMode);
        }
    }
}
