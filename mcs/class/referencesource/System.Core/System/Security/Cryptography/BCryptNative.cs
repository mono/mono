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

namespace System.Security.Cryptography {
    /// <summary>
    ///     Native interop with CNG's BCrypt layer. Native definitions can be found in bcrypt.h
    /// </summary>
    internal static class BCryptNative {
        /// <summary>
        ///     Well known algorithm names
        /// </summary>
        internal static class AlgorithmName {
            public const string ECDHP256 = "ECDH_P256";         // BCRYPT_ECDH_P256_ALGORITHM
            public const string ECDHP384 = "ECDH_P384";         // BCRYPT_ECDH_P384_ALGORITHM
            public const string ECDHP521 = "ECDH_P521";         // BCRYPT_ECDH_P521_ALGORITHM
            public const string ECDsaP256 = "ECDSA_P256";       // BCRYPT_ECDSA_P256_ALGORITHM
            public const string ECDsaP384 = "ECDSA_P384";       // BCRYPT_ECDSA_P384_ALGORITHM
            public const string ECDsaP521 = "ECDSA_P521";       // BCRYPT_ECDSA_P521_ALGORITHM
            public const string MD5 = "MD5";                    // BCRYPT_MD5_ALGORITHM
            public const string Sha1 = "SHA1";                  // BCRYPT_SHA1_ALGORITHM
            public const string Sha256 = "SHA256";              // BCRYPT_SHA256_ALGORITHM
            public const string Sha384 = "SHA384";              // BCRYPT_SHA384_ALGORITHM
            public const string Sha512 = "SHA512";              // BCRYPT_SHA512_ALGORITHM
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
            ECDHPublicP256 = 0x314B4345,                        // BCRYPT_ECDH_PUBLIC_P256_MAGIC
            ECDHPublicP384 = 0x334B4345,                        // BCRYPT_ECDH_PUBLIC_P384_MAGIC
            ECDHPublicP521 = 0x354B4345,                        // BCRYPT_ECDH_PUBLIC_P521_MAGIC
            ECDsaPublicP256 = 0x31534345,                       // BCRYPT_ECDSA_PUBLIC_P256_MAGIC
            ECDsaPublicP384 = 0x33534345,                       // BCRYPT_ECDSA_PUBLIC_P384_MAGIC
            ECDsaPublicP521 = 0x35534345                        // BCRYPT_ECDSA_PUBLIC_P521_MAGIC
        }

        /// <summary>
        ///     Well known KDF names
        /// </summary>
        internal static class KeyDerivationFunction {
            public const string Hash = "HASH";                  // BCRYPT_KDF_HASH
            public const string Hmac = "HMAC";                  // BCRYPT_KDF_HMAC
            public const string Tls = "TLS_PRF";                // BCRYPT_KDF_TLS_PRF
        }

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
    }
}
