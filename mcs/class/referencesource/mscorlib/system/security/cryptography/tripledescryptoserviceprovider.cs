// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// TripleDESCryptoServiceProvider.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class TripleDESCryptoServiceProvider : TripleDES {
        //
        // public constructors
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public TripleDESCryptoServiceProvider () {
            if (!Utils.HasAlgorithm(Constants.CALG_3DES, 0)) 
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
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"),"TripleDES");
            return _NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, CryptoAPITransformMode.Encrypt);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) {
            if (IsWeakKey(rgbKey))
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKey_Weak"),"TripleDES");
            return _NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, CryptoAPITransformMode.Decrypt);
        }

        public override void GenerateKey () {
            KeyValue = new byte[KeySizeValue/8];
            Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
            // Never hand back a weak or semi-weak key
            while (TripleDES.IsWeakKey(KeyValue)) {
                Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
            }
        }

        public override void GenerateIV () {
            // IV is always 8 bytes/64 bits because block size is always 64 bits
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
            int algid = Constants.CALG_3DES;

            // Check for bad values
            // 1) we don't support OFB mode in TripleDESCryptoServiceProvider
            if (mode == CipherMode.OFB)
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_OFBNotSupported"));
            // 2) we only support CFB with a feedback size of 8 bits
            if ((mode == CipherMode.CFB) && (feedbackSize != 8)) 
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_CFBSizeNotSupported"));

            // Build the key if one does not already exist
            if (rgbKey == null) {
                rgbKey = new byte[KeySizeValue/8];
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

            // If the size of rgbKey is 16 bytes, then we're doing two-key 3DES, so switch algids
            // Note that we assume that if a CSP supports CALG_3DES then it also supports CALG_3DES_112
            if (rgbKey.Length == 16) {
                algid = Constants.CALG_3DES_112;
            }

            // Create the encryptor object
            return new CryptoAPITransform(algid, cArgs, rgArgIds, 
                                          rgArgValues, rgbKey, PaddingValue,
                                          mode, BlockSizeValue, feedbackSize, false,
                                          encryptMode);
        }
    }
}
