// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     AES wrapper around the CAPI implementation.
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class AesCryptoServiceProvider : Aes {
        private static volatile KeySizes[] s_supportedKeySizes;
        private static volatile int s_defaultKeySize;

        [SecurityCritical]
        private SafeCspHandle m_cspHandle;

        // Note that keys are stored in CAPI rather than directly in the KeyValue property, which should not
        // be used to retrieve the key value directly.
        [SecurityCritical]
        private SafeCapiKeyHandle m_key;

        [System.Security.SecurityCritical]
        public AesCryptoServiceProvider () {
            Contract.Ensures(m_cspHandle != null && !m_cspHandle.IsInvalid && !m_cspHandle.IsClosed);

            // On Windows XP the AES CSP has the prototype name, but on newer operating systems it has the
            // standard name
            string providerName = CapiNative.ProviderNames.MicrosoftEnhancedRsaAes;
            if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1) {
                providerName = CapiNative.ProviderNames.MicrosoftEnhancedRsaAesPrototype;
            }

            m_cspHandle = CapiNative.AcquireCsp(null,
                                                providerName,
                                                CapiNative.ProviderType.RsaAes,
                                                CapiNative.CryptAcquireContextFlags.VerifyContext,
                                                true);

            // CAPI will not allow feedback sizes greater than 64 bits
            FeedbackSizeValue = 8;

            // Get the different AES key sizes supported by this platform, raising an error if there are no
            // supported key sizes.
            int defaultKeySize = 0;
            KeySizes[] keySizes = FindSupportedKeySizes(m_cspHandle, out defaultKeySize);
            if (keySizes.Length != 0) {
                Debug.Assert(defaultKeySize > 0, "defaultKeySize > 0");
                KeySizeValue = defaultKeySize;
            }
            else {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }
        }

        /// <summary>
        ///     Value of the symmetric key used for encryption / decryption
        /// </summary>
        public override byte[] Key {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
            get {
                Contract.Ensures(m_key != null && !m_key.IsInvalid && !m_key.IsClosed);
                Contract.Ensures(Contract.Result<byte[]>() != null &&
                                        Contract.Result<byte[]>().Length == KeySizeValue / 8);

                if (m_key == null || m_key.IsInvalid || m_key.IsClosed) {
                    GenerateKey();
                }

                // We don't hold onto a key value directly, so we need to export it from CAPI when the user
                // wants a byte array representation.
                byte[] keyValue =  CapiNative.ExportSymmetricKey(m_key);
                return keyValue;
            }

            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
            set {
                Contract.Ensures(m_key != null && !m_key.IsInvalid && !m_key.IsClosed);

                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                
                byte[] keyValue = (byte[])value.Clone();

                if (!ValidKeySize(keyValue.Length * 8)) {
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidKeySize));
                }

                // Import the key, then close any current key and replace with the new one. We need to make
                // sure the import is successful before closing the current key to avoid having an algorithm
                // with no valid keys.
                SafeCapiKeyHandle importedKey = CapiNative.ImportSymmetricKey(m_cspHandle,
                                                                              GetAlgorithmId(keyValue.Length * 8),
                                                                              keyValue);
                if (m_key != null) {
                    m_key.Dispose();
                }

                m_key = importedKey;
                KeySizeValue = keyValue.Length * 8;
            }
        }

        /// <summary>
        ///     Size, in bits, of the key
        /// </summary>
        public override int KeySize {
            get { return base.KeySize; }

            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
            set {
                base.KeySize = value;

                // Since the key size is being reset, we need to reset the key itself as well
                if (m_key != null) {
                    m_key.Dispose();
                }
            }
        }

        /// <summary>
        ///     Create an object to perform AES decryption with the current key and IV
        /// </summary>
        /// <returns></returns>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public override ICryptoTransform CreateDecryptor() {
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            if (m_key == null || m_key.IsInvalid || m_key.IsClosed) {
                throw new CryptographicException(SR.GetString(SR.Cryptography_DecryptWithNoKey));
            }

            return CreateDecryptor(m_key, IVValue);
        }

        /// <summary>
        ///     Create an object to perform AES decryption with the given key and IV
        /// </summary>
        [System.Security.SecurityCritical]
        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv) {
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (!ValidKeySize(key.Length * 8)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidKeySize), "key");
            }
            if (iv != null && iv.Length * 8 != BlockSizeValue) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidIVSize), "iv");
            }

            byte[] keyCopy = (byte[])key.Clone();
            byte[] ivCopy = null;
            if (iv != null) {
                ivCopy = (byte[])iv.Clone();
            }

            using (SafeCapiKeyHandle importedKey = CapiNative.ImportSymmetricKey(m_cspHandle, GetAlgorithmId(keyCopy.Length * 8), keyCopy)) {
                return CreateDecryptor(importedKey, ivCopy);
            }
        }

        /// <summary>
        ///     Create an object to perform AES decryption
        /// </summary>
        [System.Security.SecurityCritical]
        private ICryptoTransform CreateDecryptor(SafeCapiKeyHandle key, byte[] iv) {
            Contract.Requires(key != null);
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            return new CapiSymmetricAlgorithm(BlockSizeValue,
                                              FeedbackSizeValue,
                                              m_cspHandle,
                                              key,
                                              iv,
                                              Mode,
                                              PaddingValue,
                                              EncryptionMode.Decrypt);
        }

        /// <summary>
        ///     Create an object to do AES encryption with the current key and IV
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public override ICryptoTransform CreateEncryptor() {
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            if (m_key == null || m_key.IsInvalid || m_key.IsClosed) {
                GenerateKey();
            }

            // ECB is the only mode which does not require an IV -- generate one here if we don't have one yet.
            if (Mode != CipherMode.ECB && IVValue == null) {
                GenerateIV();
            }

            return CreateEncryptor(m_key, IVValue);
        }

        /// <summary>
        ///     Create an object to do AES encryption with the given key and IV
        /// </summary>
        [System.Security.SecurityCritical]
        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv) {
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (!ValidKeySize(key.Length * 8)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidKeySize), "key");
            }
            if (iv != null && iv.Length * 8 != BlockSizeValue) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidIVSize), "iv");
            }

            byte[] keyCopy = (byte[])key.Clone();
            byte[] ivCopy = null;
            if (iv != null) {
                ivCopy = (byte[])iv.Clone();
            }

            using (SafeCapiKeyHandle importedKey = CapiNative.ImportSymmetricKey(m_cspHandle, GetAlgorithmId(keyCopy.Length * 8), keyCopy)) {
                return CreateEncryptor(importedKey, ivCopy);
            }
        }

        /// <summary>
        ///     Create an object to perform AES encryption
        /// </summary>
        [System.Security.SecurityCritical]
        private ICryptoTransform CreateEncryptor(SafeCapiKeyHandle key, byte[] iv) {
            Contract.Requires(key != null);
            Contract.Ensures(Contract.Result<ICryptoTransform>() != null);

            return new CapiSymmetricAlgorithm(BlockSizeValue,
                                              FeedbackSizeValue,
                                              m_cspHandle,
                                              key,
                                              iv,
                                              Mode,
                                              PaddingValue,
                                              EncryptionMode.Encrypt);
        }

        /// <summary>
        ///     Release any CAPI handles we're holding onto
        /// </summary>
        [System.Security.SecurityCritical]
        protected override void Dispose(bool disposing) {
            Contract.Ensures(!disposing || m_key == null || m_key.IsClosed);
            Contract.Ensures(!disposing || m_cspHandle == null || m_cspHandle.IsClosed);

            try {
                if (disposing) {
                    if (m_key != null) {
                        m_key.Dispose();
                    }

                    if (m_cspHandle != null) {
                        m_cspHandle.Dispose();
                    }
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Get the size of AES keys supported by the given CSP, and which size should be used by default.
        /// 
        ///     We assume that the same CSP will always be used by all instances of the AesCryptoServiceProvider
        ///     in the current AppDomain.  If we add the ability for users to choose which CSP to use on a
        ///     per-instance basis, we need to update the code to account for the CSP when checking the cached
        ///     key size values.
        /// </summary>
        [System.Security.SecurityCritical]
        private static KeySizes[] FindSupportedKeySizes(SafeCspHandle csp, out int defaultKeySize) {
            Contract.Requires(csp != null);
            Contract.Ensures(Contract.Result<KeySizes[]>() != null);

            // If this platform has any supported algorithm sizes, then the default key size should be set to a
            // reasonable value. 
            Contract.Ensures(Contract.Result<KeySizes[]>().Length == 0 ||
                             (Contract.ValueAtReturn<int>(out defaultKeySize) > 0 && Contract.ValueAtReturn<int>(out defaultKeySize) % 8 == 0));

            if (s_supportedKeySizes == null) {
                List<KeySizes> keySizes = new List<KeySizes>();
                int maxKeySize = 0;

                //
                // Enumerate the CSP's supported algorithms to see what key sizes it supports for AES
                //

                CapiNative.PROV_ENUMALGS algorithm =
                    CapiNative.GetProviderParameterStruct<CapiNative.PROV_ENUMALGS>(csp,
                                                                                   CapiNative.ProviderParameter.EnumerateAlgorithms,
                                                                                   CapiNative.ProviderParameterFlags.RestartEnumeration);

                // Translate between CAPI AES algorithm IDs and supported key sizes
                while (algorithm.aiAlgId != CapiNative.AlgorithmId.None) {
                    switch (algorithm.aiAlgId) {
                        case CapiNative.AlgorithmId.Aes128:
                            keySizes.Add(new KeySizes(128, 128, 0));
                            if (128 > maxKeySize) {
                                maxKeySize = 128;
                            }

                            break;

                        case CapiNative.AlgorithmId.Aes192:
                            keySizes.Add(new KeySizes(192, 192, 0));
                            if (192 > maxKeySize) {
                                maxKeySize = 192;
                            }
                            break;

                        case CapiNative.AlgorithmId.Aes256:
                            keySizes.Add(new KeySizes(256, 256, 0));
                            if (256 > maxKeySize) {
                                maxKeySize = 256;
                            }
                            break;

                        default:
                            break;
                    }

                    algorithm = CapiNative.GetProviderParameterStruct<CapiNative.PROV_ENUMALGS>(csp,
                                                                                               CapiNative.ProviderParameter.EnumerateAlgorithms,
                                                                                               CapiNative.ProviderParameterFlags.None);
                }

                s_supportedKeySizes = keySizes.ToArray();
                s_defaultKeySize = maxKeySize;
            }

            defaultKeySize = s_defaultKeySize;
            return s_supportedKeySizes;
        }

        /// <summary>
        ///     Generate a new random key
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public override void GenerateKey() {
            Contract.Ensures(m_key != null && !m_key.IsInvalid & !m_key.IsClosed);
            Contract.Assert(m_cspHandle != null);

            SafeCapiKeyHandle key = null;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                if (!CapiNative.UnsafeNativeMethods.CryptGenKey(m_cspHandle,
                                                                GetAlgorithmId(KeySizeValue),
                                                                CapiNative.KeyFlags.Exportable,
                                                                out key)) {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally {
                if (key != null && !key.IsInvalid) {
                    key.SetParentCsp(m_cspHandle);
                }
            }

            if (m_key != null) {
                m_key.Dispose();
            }

            m_key = key;
        }

        /// <summary>
        ///     Generate a random initialization vector
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public override void GenerateIV() {
            Contract.Ensures(IVValue != null && IVValue.Length == BlockSizeValue / 8);
            Contract.Assert(m_cspHandle != null);
            Contract.Assert(BlockSizeValue % 8 == 0);

            byte[] iv = new byte[BlockSizeValue / 8];
            if (!CapiNative.UnsafeNativeMethods.CryptGenRandom(m_cspHandle, iv.Length, iv)) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            IVValue = iv;
        }

        /// <summary>
        ///     Map an AES key size to the corresponding CAPI algorithm ID
        /// </summary>
        private static CapiNative.AlgorithmId GetAlgorithmId(int keySize) {
            // We should always return either a data encryption algorithm ID or None if we don't recognize the key size
            Contract.Ensures(
                ((((int)Contract.Result<CapiNative.AlgorithmId>()) & (int)CapiNative.AlgorithmClass.DataEncryption) == (int)CapiNative.AlgorithmClass.DataEncryption) ||
                Contract.Result<CapiNative.AlgorithmId>() == CapiNative.AlgorithmId.None);

            switch (keySize) {
                case 128:
                    return CapiNative.AlgorithmId.Aes128;

                case 192:
                    return CapiNative.AlgorithmId.Aes192;

                case 256:
                    return CapiNative.AlgorithmId.Aes256;

                default:
                    return CapiNative.AlgorithmId.None;
            }
        }
    }
}
