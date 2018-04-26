// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography {

    internal enum AsymmetricPaddingMode {
        /// <summary>
        ///     No padding
        /// </summary>
        None = 1,                       // BCRYPT_PAD_NONE

        /// <summary>
        ///     PKCS #1 padding
        /// </summary>
        Pkcs1 = 2,                      // BCRYPT_PAD_PKCS1

        /// <summary>
        ///     Optimal Asymmetric Encryption Padding
        /// </summary>
        Oaep = 4,                       // BCRYPT_PAD_OAEP

        /// <summary>
        ///     Probabilistic Signature Scheme padding
        /// </summary>
        Pss = 8                         // BCRYPT_PAD_PSS
    }
#if !MONO
    [StructLayout(LayoutKind.Sequential)]
    internal struct BCRYPT_DSA_KEY_BLOB_V2
    {
        public BCryptNative.KeyBlobMagicNumber dwMagic;  // BCRYPT_DSA_PUBLIC_MAGIC_V2 or BCRYPT_DSA_PRIVATE_MAGIC_V2
        public int cbKey;               // key lengths in BYTES (e.g. for a 3072-bit key, cbKey = 3072/8 = 384)
        public HASHALGORITHM_ENUM hashAlgorithm;
        public DSAFIPSVERSION_ENUM standardVersion;
        public int cbSeedLength;        // size (in bytes) of the seed value
        public int cbGroupSize;         // size (in bytes) of the Q value
        public byte Count3;             // # of iterations used to generate Q. In big-endian format.
        public byte Count2;
        public byte Count1;
        public byte Count0;
    }
#endif
    internal enum HASHALGORITHM_ENUM : int
    {
        DSA_HASH_ALGORITHM_SHA1 = 0,
        DSA_HASH_ALGORITHM_SHA256 = 1,
        DSA_HASH_ALGORITHM_SHA512 = 2,
    }

    internal enum DSAFIPSVERSION_ENUM : int
    {
        DSA_FIPS186_2 = 0,
        DSA_FIPS186_3 = 1,
    }

    /// <summary>
    ///     Native interop with CNG's BCrypt layer. Native definitions can be found in bcrypt.h
    /// </summary>
    internal static class BCryptNative {
        /// <summary>
        ///     Well known algorithm names
        /// </summary>
        internal static class AlgorithmName {
            public const string ECDH = "ECDH";                  // BCRYPT_ECDH_ALGORITHM
            public const string ECDHP256 = "ECDH_P256";         // BCRYPT_ECDH_P256_ALGORITHM
            public const string ECDHP384 = "ECDH_P384";         // BCRYPT_ECDH_P384_ALGORITHM
            public const string ECDHP521 = "ECDH_P521";         // BCRYPT_ECDH_P521_ALGORITHM
            public const string ECDsa = "ECDSA";                // BCRYPT_ECDSA_ALGORITHM
            public const string ECDsaP256 = "ECDSA_P256";       // BCRYPT_ECDSA_P256_ALGORITHM
            public const string ECDsaP384 = "ECDSA_P384";       // BCRYPT_ECDSA_P384_ALGORITHM
            public const string ECDsaP521 = "ECDSA_P521";       // BCRYPT_ECDSA_P521_ALGORITHM
            public const string MD5 = "MD5";                    // BCRYPT_MD5_ALGORITHM
            public const string Sha1 = "SHA1";                  // BCRYPT_SHA1_ALGORITHM
            public const string Sha256 = "SHA256";              // BCRYPT_SHA256_ALGORITHM
            public const string Sha384 = "SHA384";              // BCRYPT_SHA384_ALGORITHM
            public const string Sha512 = "SHA512";              // BCRYPT_SHA512_ALGORITHM
            internal const string Rsa = "RSA";                  // BCRYPT_RSA_ALGORITHM
        }
#if !MONO
        /// <summary>
        ///     Well known key blob tyes
        /// </summary>
        internal static class KeyBlobType {
            //During Win8 Windows introduced  BCRYPT_PUBLIC_KEY_BLOB L"PUBLICBLOB"  
            //and #define BCRYPT_PRIVATE_KEY_BLOB L"PRIVATEBLOB". We should use the 
            //same on ProjectN and ProjectK 
            internal const string RsaFullPrivateBlob = "RSAFULLPRIVATEBLOB";    // BCRYPT_RSAFULLPRIVATE_BLOB
            internal const string RsaPrivateBlob = "RSAPRIVATEBLOB";            // BCRYPT_RSAPRIVATE_BLOB
            internal const string RsaPublicBlob = "RSAPUBLICBLOB";              // BCRYPT_PUBLIC_KEY_BLOB
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BCRYPT_RSAKEY_BLOB {
            internal KeyBlobMagicNumber Magic;
            internal int BitLength;
            internal int cbPublicExp;
            internal int cbModulus;
            internal int cbPrime1;
            internal int cbPrime2;
        }

        /// <summary>
        ///     Result codes from BCrypt APIs
        /// </summary>
        internal enum ErrorCode {
            Success = 0x00000000,                               // STATUS_SUCCESS
            BufferToSmall = unchecked((int)0xC0000023),         // STATUS_BUFFER_TOO_SMALL
            ObjectNameNotFound = unchecked((int)0xC0000034)     // SATUS_OBJECT_NAME_NOT_FOUND
        }

        /// <summary>
        ///     Well known BCrypt hash property names
        /// </summary>
        internal static class HashPropertyName {
            public const string HashLength = "HashDigestLength";        // BCRYPT_HASH_LENGTH
        }

        /// <summary>
        ///     Magic numbers identifying blob types
        /// </summary>
        internal enum KeyBlobMagicNumber {
            DsaPublic = 0x42505344,                             // BCRYPT_DSA_PUBLIC_MAGIC for key lengths <= 1024 bits
            DsaPublicV2 = 0x32425044,                           // BCRYPT_DSA_PUBLIC_MAGIC_V2 for key lengths > 1024 bits
            DsaPrivate = 0x56505344,                            // BCRYPT_DSA_PRIVATE_MAGIC for key lengths <= 1024 bits
            DsaPrivateV2 = 0x32565044,                          // BCRYPT_DSA_PRIVATE_MAGIC_V2 for key lengths > 1024 bits
            ECDHPublicP256 = 0x314B4345,                        // BCRYPT_ECDH_PUBLIC_P256_MAGIC
            ECDHPublicP384 = 0x334B4345,                        // BCRYPT_ECDH_PUBLIC_P384_MAGIC
            ECDHPublicP521 = 0x354B4345,                        // BCRYPT_ECDH_PUBLIC_P521_MAGIC
            ECDsaPublicP256 = 0x31534345,                       // BCRYPT_ECDSA_PUBLIC_P256_MAGIC
            ECDsaPublicP384 = 0x33534345,                       // BCRYPT_ECDSA_PUBLIC_P384_MAGIC
            ECDsaPublicP521 = 0x35534345,                       // BCRYPT_ECDSA_PUBLIC_P521_MAGIC
            RsaPublic = 0x31415352,                             // BCRYPT_RSAPUBLIC_MAGIC
            RsaPrivate = 0x32415352,                            // BCRYPT_RSAPRIVATE_MAGIC
            RsaFullPrivateMagic = 0x33415352,                    //BCRYPT_RSAFULLPRIVATE_MAGIC   
            KeyDataBlob = 0x4d42444b                            // BCRYPT_KEY_DATA_BLOB_MAGIC
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BCRYPT_OAEP_PADDING_INFO {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszAlgId;

            internal IntPtr pbLabel;

            internal int cbLabel;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BCRYPT_PKCS1_PADDING_INFO {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszAlgId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BCRYPT_PSS_PADDING_INFO {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszAlgId;

            internal int cbSalt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BCRYPT_KEY_DATA_BLOB_HEADER
        {
            public uint dwMagic;
            public uint dwVersion;
            public uint cbKeyData;

            public const uint BCRYPT_KEY_DATA_BLOB_MAGIC = 0x4d42444b;
            public const uint BCRYPT_KEY_DATA_BLOB_VERSION1 = 0x1;
        }

        /// <summary>
        ///     Well known KDF names
        /// </summary>
        internal static class KeyDerivationFunction {
            public const string Hash = "HASH";                  // BCRYPT_KDF_HASH
            public const string Hmac = "HMAC";                  // BCRYPT_KDF_HMAC
            public const string Tls = "TLS_PRF";                // BCRYPT_KDF_TLS_PRF
        }

        internal const string BCRYPT_ECCPUBLIC_BLOB = "ECCPUBLICBLOB";
        internal const string BCRYPT_ECCPRIVATE_BLOB = "ECCPRIVATEBLOB";

        internal const string BCRYPT_ECC_CURVE_NISTP256 = "nistP256";
        internal const string BCRYPT_ECC_CURVE_NISTP384 = "nistP384";
        internal const string BCRYPT_ECC_CURVE_NISTP521 = "nistP521";

        /// <summary>
        ///     Well known BCrypt provider names
        /// </summary>
        internal static class ProviderName {
            public const string MicrosoftPrimitiveProvider = "Microsoft Primitive Provider";    // MS_PRIMITIVE_PROVIDER
        }

        /// <summary>
        ///     Well known BCrypt object property names
        /// </summary>
        internal static class ObjectPropertyName {
            public const string ObjectLength = "ObjectLength";          // BCRYPT_OBJECT_LENGTH
        }

#pragma warning disable 618    // Have not migrated to v4 transparency yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods {
            /// <summary>
            ///     Create a hash object
            /// </summary>
            [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptCreateHash(SafeBCryptAlgorithmHandle hAlgorithm,
                                                              [Out] out SafeBCryptHashHandle phHash,
                                                              IntPtr pbHashObject,
                                                              int cbHashObject,
                                                              IntPtr pbSecret,
                                                              int cbSecret,
                                                              int dwFlags);

            /// <summary>
            ///     Get a property from a BCrypt algorithm object
            /// </summary>
            [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptGetProperty(SafeBCryptAlgorithmHandle hObject,
                                                               string pszProperty,
                                                               [MarshalAs(UnmanagedType.LPArray), In, Out] byte[] pbOutput,
                                                               int cbOutput,
                                                               [In, Out] ref int pcbResult,
                                                               int flags);

            /// <summary>
            ///     Get a property from a BCrypt algorithm object
            /// </summary>
            [DllImport("bcrypt.dll", EntryPoint = "BCryptGetProperty", CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptGetAlgorithmProperty(SafeBCryptAlgorithmHandle hObject,
                                                                        string pszProperty,
                                                                        [MarshalAs(UnmanagedType.LPArray), In, Out] byte[] pbOutput,
                                                                        int cbOutput,
                                                                        [In, Out] ref int pcbResult,
                                                                        int flags);

            /// <summary>
            ///     Get a property from a BCrypt hash object
            /// </summary>
            [DllImport("bcrypt.dll", EntryPoint = "BCryptGetProperty", CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptGetHashProperty(SafeBCryptHashHandle hObject,
                                                                   string pszProperty,
                                                                   [MarshalAs(UnmanagedType.LPArray), In, Out] byte[] pbOutput,
                                                                   int cbOutput,
                                                                   [In, Out] ref int pcbResult,
                                                                   int flags);

            /// <summary>
            ///     Get the hash value of the data
            /// </summary>
            [DllImport("bcrypt.dll")]
            internal static extern ErrorCode BCryptFinishHash(SafeBCryptHashHandle hHash,
                                                              [MarshalAs(UnmanagedType.LPArray), Out] byte[] pbInput,
                                                              int cbInput,
                                                              int dwFlags);

            /// <summary>
            ///     Hash a block of data
            /// </summary>
            [DllImport("bcrypt.dll")]
            internal static extern ErrorCode BCryptHashData(SafeBCryptHashHandle hHash,
                                                            [MarshalAs(UnmanagedType.LPArray), In] byte[] pbInput,
                                                            int cbInput,
                                                            int dwFlags);

            /// <summary>
            ///     Get a handle to an algorithm provider
            /// </summary>
            [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptOpenAlgorithmProvider([Out] out SafeBCryptAlgorithmHandle phAlgorithm,
                                                                         string pszAlgId,             // BCryptAlgorithm
                                                                         string pszImplementation,    // ProviderNames
                                                                         int dwFlags);

            [DllImport("bcrypt.dll", SetLastError = true)]
            internal static extern ErrorCode BCryptExportKey([In]SafeBCryptKeyHandle hKey,
                                                             [In]IntPtr hExportKey,
                                                             [In][MarshalAs(UnmanagedType.LPWStr)] string pszBlobType,
                                                             [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput,
                                                             [In]int cbOutput,
                                                             [In]ref int pcbResult,
                                                             [In] int dwFlags);

            [DllImport("Crypt32.dll", SetLastError = true)]
            internal static extern int CryptImportPublicKeyInfoEx2([In] uint dwCertEncodingType,
                                                   [In] ref X509Native.CERT_PUBLIC_KEY_INFO pInfo,
                                                   [In] int dwFlags,
                                                   [In] IntPtr pvAuxInfo,
                                                   [Out] out SafeBCryptKeyHandle phKey);

            [DllImport("bcrypt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ErrorCode BCryptImportKey(
                SafeBCryptAlgorithmHandle hAlgorithm,
                IntPtr hImportKey,
                string pszBlobType,
                out SafeBCryptKeyHandle hKey,
                IntPtr pbKeyObject,
                int cbKeyObject,
                byte[] pbInput,
                int cbInput,
                int dwFlags);

            [DllImport("bcrypt.dll", SetLastError = true)]
            public static extern unsafe ErrorCode BCryptEncrypt(
                SafeBCryptKeyHandle hKey,
                byte* pbInput,
                int cbInput,
                IntPtr paddingInfo,
                [In, Out] byte[] pbIV,
                int cbIV,
                byte* pbOutput,
                int cbOutput,
                out int cbResult,
                int dwFlags);

            [DllImport("bcrypt.dll", SetLastError = true)]
            public static extern unsafe ErrorCode BCryptDecrypt(
                SafeBCryptKeyHandle hKey,
                byte* pbInput,
                int cbInput,
                IntPtr paddingInfo,
                [In, Out] byte[] pbIV,
                int cbIV,
                byte* pbOutput,
                int cbOutput,
                out int cbResult,
                int dwFlags);

            [DllImport("bcrypt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern ErrorCode BCryptSetProperty(
                SafeBCryptAlgorithmHandle hObject,
                string pszProperty,
                string pbInput,
                int cbInput,
                int dwFlags);
        }

        [SecuritySafeCritical]
        internal static class AesBCryptModes
        {
            [SecurityCritical]
            private static readonly SafeBCryptAlgorithmHandle s_hAlgCbc = OpenAesAlgorithm(Interop.BCrypt.BCRYPT_CHAIN_MODE_CBC);

            [SecurityCritical]
            private static readonly SafeBCryptAlgorithmHandle s_hAlgEcb = OpenAesAlgorithm(Interop.BCrypt.BCRYPT_CHAIN_MODE_ECB);

            internal static SafeBCryptAlgorithmHandle GetSharedHandle(CipherMode cipherMode)
            {
                // Windows 8 added support to set the CipherMode value on a key,
                // but Windows 7 requires that it be set on the algorithm before key creation.
                switch (cipherMode)
                {
                    case CipherMode.CBC:
                        return s_hAlgCbc;
                    case CipherMode.ECB:
                        return s_hAlgEcb;
                    default:
                        throw new NotSupportedException();
                }
            }

            private static SafeBCryptAlgorithmHandle OpenAesAlgorithm(string cipherMode)
            {
                const string BCRYPT_AES_ALGORITHM = "AES";
                SafeBCryptAlgorithmHandle hAlg = OpenAlgorithm(BCRYPT_AES_ALGORITHM, null);
                SetCipherMode(hAlg, cipherMode);

                return hAlg;
            }
        }

        [SecuritySafeCritical]
        internal static class TripleDesBCryptModes
        {
            [SecurityCritical]
            private static readonly SafeBCryptAlgorithmHandle s_hAlgCbc = OpenAesAlgorithm(Interop.BCrypt.BCRYPT_CHAIN_MODE_CBC);

            [SecurityCritical]
            private static readonly SafeBCryptAlgorithmHandle s_hAlgEcb = OpenAesAlgorithm(Interop.BCrypt.BCRYPT_CHAIN_MODE_ECB);

            internal static SafeBCryptAlgorithmHandle GetSharedHandle(CipherMode cipherMode)
            {
                // Windows 8 added support to set the CipherMode value on a key,
                // but Windows 7 requires that it be set on the algorithm before key creation.
                switch (cipherMode)
                {
                    case CipherMode.CBC:
                        return s_hAlgCbc;
                    case CipherMode.ECB:
                        return s_hAlgEcb;
                    default:
                        throw new NotSupportedException();
                }
            }

            private static SafeBCryptAlgorithmHandle OpenAesAlgorithm(string cipherMode)
            {
                const string BCRYPT_3DES_ALGORITHM = "3DES";
                SafeBCryptAlgorithmHandle hAlg = OpenAlgorithm(BCRYPT_3DES_ALGORITHM, null);
                SetCipherMode(hAlg, cipherMode);

                return hAlg;
            }
        }

        //
        // Wrapper and utility functions
        //

        /// <summary>
        ///     Adapter to wrap specific BCryptGetProperty P/Invokes with a generic handle type
        /// </summary>
#pragma warning disable 618 // System.Core.dll still uses SecurityRuleSet.Level1
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        private delegate ErrorCode BCryptPropertyGetter<T>(T hObject,
                                                           string pszProperty,
                                                           byte[] pbOutput,
                                                           int cbOutput,
                                                           ref int pcbResult,
                                                           int dwFlags) where T : SafeHandle;

        private static volatile bool s_haveBcryptSupported;
        private static volatile bool s_bcryptSupported;

        /// <summary>
        ///     Determine if BCrypt is supported on the current machine
        /// </summary>
        internal static bool BCryptSupported {
            [SecuritySafeCritical]
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
            get {
                if (!s_haveBcryptSupported)
                {
                    // Attempt to load bcrypt.dll to see if the BCrypt CNG APIs are available on the machine
                    using (SafeLibraryHandle bcrypt = Microsoft.Win32.UnsafeNativeMethods.LoadLibraryEx("bcrypt", IntPtr.Zero, 0)) {
                        s_bcryptSupported = !bcrypt.IsInvalid;
                        s_haveBcryptSupported = true;
                    }
                }

                return s_bcryptSupported;
            }
        }

        /// <summary>
        ///     Get the value of a DWORD property of a BCrypt object
        /// </summary>
        [System.Security.SecurityCritical]
        internal static int GetInt32Property<T>(T algorithm, string property) where T : SafeHandle {
            Contract.Requires(algorithm != null);
            Contract.Requires(property == HashPropertyName.HashLength ||
                              property == ObjectPropertyName.ObjectLength);

            return BitConverter.ToInt32(GetProperty(algorithm, property), 0);
        }

        /// <summary>
        ///     Get the value of a property of a BCrypt object
        /// </summary>
        [System.Security.SecurityCritical]
        internal static byte[] GetProperty<T>(T algorithm, string property) where T : SafeHandle {
            Contract.Requires(algorithm != null);
            Contract.Requires(!String.IsNullOrEmpty(property));
            Contract.Ensures(Contract.Result<byte[]>() != null);

            BCryptPropertyGetter<T> getter = null;
            if (typeof(T) == typeof(SafeBCryptAlgorithmHandle)) {
                getter = new BCryptPropertyGetter<SafeBCryptAlgorithmHandle>(UnsafeNativeMethods.BCryptGetAlgorithmProperty)
                    as BCryptPropertyGetter<T>;
            }
            else if (typeof(T) == typeof(SafeBCryptHashHandle)) {
                getter = new BCryptPropertyGetter<SafeBCryptHashHandle>(UnsafeNativeMethods.BCryptGetHashProperty)
                    as BCryptPropertyGetter<T>;
            }

            Debug.Assert(getter != null, "Unknown handle type");

            // Figure out how big the property is
            int bufferSize = 0;
            ErrorCode error = getter(algorithm, property, null, 0, ref bufferSize, 0);

            if (error != ErrorCode.BufferToSmall && error != ErrorCode.Success) {
                throw new CryptographicException((int)error);
            }

            // Allocate the buffer, and return the property
            Debug.Assert(bufferSize > 0, "bufferSize > 0");
            byte[] buffer = new byte[bufferSize];
            error = getter(algorithm, property, buffer, buffer.Length, ref bufferSize, 0);

            if (error != ErrorCode.Success) {
                throw new CryptographicException((int)error);
            }

            return buffer;
        }


        /// <summary>
        ///     Map an algorithm identifier to a key size and magic number
        /// </summary>
        internal static void MapAlgorithmIdToMagic(string algorithm,
                                                   out KeyBlobMagicNumber algorithmMagic,
                                                   out int keySize) {
            Contract.Requires(!String.IsNullOrEmpty(algorithm));

            switch (algorithm) {
                case AlgorithmName.ECDHP256:
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP256;
                    keySize = 256;
                    break;

                case AlgorithmName.ECDHP384:
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP384;
                    keySize = 384;
                    break;

                case AlgorithmName.ECDHP521:
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP521;
                    keySize = 521;
                    break;

                case AlgorithmName.ECDsaP256:
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP256;
                    keySize = 256;
                    break;

                case AlgorithmName.ECDsaP384:
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP384;
                    keySize = 384;
                    break;

                case AlgorithmName.ECDsaP521:
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP521;
                    keySize = 521;
                    break;

                default:
                    throw new ArgumentException(SR.GetString(SR.Cryptography_UnknownEllipticCurveAlgorithm));
            }
        }

        /// <summary>
        ///     Open a handle to an algorithm provider
        /// </summary>
        [System.Security.SecurityCritical]
        internal static SafeBCryptAlgorithmHandle OpenAlgorithm(string algorithm, string implementation) {
            Contract.Requires(!String.IsNullOrEmpty(algorithm));
            Contract.Requires(!String.IsNullOrEmpty(implementation));
            Contract.Ensures(Contract.Result<SafeBCryptAlgorithmHandle>() != null &&
                             !Contract.Result<SafeBCryptAlgorithmHandle>().IsInvalid &&
                             !Contract.Result<SafeBCryptAlgorithmHandle>().IsClosed);

            SafeBCryptAlgorithmHandle algorithmHandle = null;
            ErrorCode error = UnsafeNativeMethods.BCryptOpenAlgorithmProvider(out algorithmHandle,
                                                                              algorithm,
                                                                              implementation,
                                                                              0);

            if (error != ErrorCode.Success) {
                throw new CryptographicException((int)error);
            }

            return algorithmHandle;
        }

        [SecuritySafeCritical]
        internal static SafeBCryptKeyHandle ImportAsymmetricPublicKey(X509Native.CERT_PUBLIC_KEY_INFO certPublicKeyInfo, int dwFlag) {
            SafeBCryptKeyHandle keyHandle = null;            
            int error = UnsafeNativeMethods.CryptImportPublicKeyInfoEx2(
                                                        X509Native.X509_ASN_ENCODING,
                                                        ref certPublicKeyInfo,
                                                        dwFlag,
                                                        IntPtr.Zero,
                                                        out keyHandle);
            if (error == 0) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return keyHandle;
        }

        [SecuritySafeCritical]
        internal static byte[] ExportBCryptKey(SafeBCryptKeyHandle hKey, string blobType) {
            byte[] keyBlob = null;
            int length = 0;

            ErrorCode error = UnsafeNativeMethods.BCryptExportKey(hKey, IntPtr.Zero, blobType, null, 0, ref length, 0);

            if (error != ErrorCode.BufferToSmall && error != ErrorCode.Success)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            keyBlob = new byte[length];
            error = UnsafeNativeMethods.BCryptExportKey(hKey, IntPtr.Zero, blobType, keyBlob, length, ref length, 0);
            if (error != ErrorCode.Success) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return keyBlob;
        }


        [SecuritySafeCritical]
        internal static SafeBCryptKeyHandle BCryptImportKey(SafeBCryptAlgorithmHandle hAlg, byte[] key)
        {
            unsafe
            {
                const String BCRYPT_KEY_DATA_BLOB = "KeyDataBlob";
                int keySize = key.Length;
                int blobSize = sizeof(BCRYPT_KEY_DATA_BLOB_HEADER) keySize;
                byte[] blob = new byte[blobSize];
                fixed (byte* pbBlob = blob)
                {
                    BCRYPT_KEY_DATA_BLOB_HEADER* pBlob = (BCRYPT_KEY_DATA_BLOB_HEADER*)pbBlob;
                    pBlob->dwMagic = BCRYPT_KEY_DATA_BLOB_HEADER.BCRYPT_KEY_DATA_BLOB_MAGIC;
                    pBlob->dwVersion = BCRYPT_KEY_DATA_BLOB_HEADER.BCRYPT_KEY_DATA_BLOB_VERSION1;
                    pBlob->cbKeyData = (uint)keySize;
                }
                Buffer.BlockCopy(key, 0, blob, sizeof(BCRYPT_KEY_DATA_BLOB_HEADER), keySize);
                SafeBCryptKeyHandle hKey;

                ErrorCode error = UnsafeNativeMethods.BCryptImportKey(
                    hAlg,
                    IntPtr.Zero,
                    BCRYPT_KEY_DATA_BLOB,
                    out hKey,
                    IntPtr.Zero,
                    0,
                    blob,
                    blobSize,
                    0);

                if (error != ErrorCode.Success)
                    throw new CryptographicException((int)error);

                return hKey;
            }
        }

        // Note: input and output are allowed to be the same buffer.
        // BCryptEncrypt will correctly do the encryption in place according to CNG documentation.
        [SecuritySafeCritical]
        public static int BCryptEncrypt(
            SafeBCryptKeyHandle hKey,
            byte[] input,
            int inputOffset,
            int inputCount,
            byte[] iv,
            byte[] output,
            int outputOffset,
            int outputCount)
        {
            Debug.Assert(input != null);
            Debug.Assert(inputOffset >= 0);
            Debug.Assert(inputCount >= 0);
            Debug.Assert(inputCount <= input.Length - inputOffset);
            Debug.Assert(output != null);
            Debug.Assert(outputOffset >= 0);
            Debug.Assert(outputCount >= 0);
            Debug.Assert(outputCount <= output.Length - outputOffset);

            unsafe
            {
                fixed (byte* pbInput = input)
                {
                    fixed (byte* pbOutput = output)
                    {
                        int cbResult;
                        ErrorCode error = UnsafeNativeMethods.BCryptEncrypt(
                            hKey,
                            pbInput inputOffset,
                            inputCount,
                            IntPtr.Zero,
                            iv,
                            iv == null ? 0 : iv.Length,
                            pbOutput outputOffset,
                            outputCount,
                            out cbResult,
                            0);

                        if (error != ErrorCode.Success)
                            throw new CryptographicException((int)error);

                        return cbResult;
                    }
                }
            }
        }

        // Note: input and output are allowed to be the same buffer.
        // BCryptDecrypt will correctly do the decryption in place according to CNG documentation.
        [SecuritySafeCritical]
        public static int BCryptDecrypt(
            SafeBCryptKeyHandle hKey,
            byte[] input,
            int inputOffset,
            int inputCount,
            byte[] iv,
            byte[] output,
            int outputOffset,
            int outputCount)
        {
            Debug.Assert(input != null);
            Debug.Assert(inputOffset >= 0);
            Debug.Assert(inputCount >= 0);
            Debug.Assert(inputCount <= input.Length - inputOffset);
            Debug.Assert(output != null);
            Debug.Assert(outputOffset >= 0);
            Debug.Assert(outputCount >= 0);
            Debug.Assert(outputCount <= output.Length - outputOffset);

            unsafe
            {
                fixed (byte* pbInput = input)
                {
                    fixed (byte* pbOutput = output)
                    {
                        int cbResult;
                        ErrorCode error = UnsafeNativeMethods.BCryptDecrypt(
                            hKey,
                            pbInput inputOffset,
                            inputCount,
                            IntPtr.Zero,
                            iv,
                            iv == null ? 0 : iv.Length,
                            pbOutput outputOffset,
                            outputCount,
                            out cbResult,
                             0);
 
                        if (error != ErrorCode.Success)
                            throw new CryptographicException((int)error);
 
                        return cbResult;
                    }
                }
            }
        }
 
        [SecurityCritical]
        public static void SetCipherMode(SafeBCryptAlgorithmHandle hAlg, string cipherMode)
        {
            const string BCRYPT_CHAINING_MODE = "ChainingMode";
 
            ErrorCode error = UnsafeNativeMethods.BCryptSetProperty(
                hAlg,
                BCRYPT_CHAINING_MODE,
                cipherMode,
                // Explicit \0 terminator, UCS-2
                (cipherMode.Length 1) * 2,
                0);
 
            if (error != ErrorCode.Success)
            {
                throw new CryptographicException((int)error);
            }
        }
#endif
    }
}
