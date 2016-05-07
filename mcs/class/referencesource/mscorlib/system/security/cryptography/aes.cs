// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.CompilerServices;

namespace System.Security.Cryptography { 
    /// <summary>
    ///     Abstract base class for implementations of the AES algorithm
    /// </summary>
#if !FEATURE_CORECLR && !MOBILE
    [TypeForwardedFrom("System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089")]
#else // FEATURE_CORECLR
    [TypeForwardedFrom("System.Core, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e")]
#endif // !FEATURE_CORECLR
    public abstract class Aes : SymmetricAlgorithm {
        private static KeySizes[] s_legalBlockSizes = { new KeySizes(128, 128, 0) };
        private static KeySizes[] s_legalKeySizes = { new KeySizes(128, 256, 64) };

        /// <summary>
        ///     Setup the default values for AES encryption
        /// </summary>
        protected Aes() {
            LegalBlockSizesValue = s_legalBlockSizes;
            LegalKeySizesValue = s_legalKeySizes;

            BlockSizeValue = 128;
            FeedbackSizeValue = 8;
            KeySizeValue = 256;
            ModeValue = CipherMode.CBC;
        }

        public static new Aes Create() {
#if MONOTOUCH
            // The Aes base class was moved from System.Core to mscorlib - so we can't just return a new AesCryptoServiceProvider instance
            // note: the linker is aware of this condition
            return (Aes) Activator.CreateInstance (Type.GetType ("System.Security.Cryptography.AesManaged, " + Consts.AssemblySystem_Core));
#else
            return Create("AES");
#endif
        }

        public static new Aes Create(string algorithmName) {
            if (algorithmName == null) {
                throw new ArgumentNullException("algorithmName");
            }

            return CryptoConfig.CreateFromName(algorithmName) as Aes;
        }
    }
}
