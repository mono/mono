// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
#if FEATURE_CORESYSTEM
using System.Core;
#endif
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Native interop with CAPI. Native definitions can be found in wincrypt.h or msaxlapi.h
    /// </summary>
    internal static class CapiNative {
        internal enum AlgorithmClass {
            DataEncryption = (3 << 13),         // ALG_CLASS_DATA_ENCRYPT
            Hash = (4 << 13)                    // ALG_CLASS_HASH
        }

        internal enum AlgorithmType {
            Any = (0 << 9),                     // ALG_TYPE_ANY
            Block = (3 << 9)                    // ALG_TYPE_BLOCK
        }

        internal enum AlgorithmSubId {
            MD5 = 3,                            // ALG_SID_MD5
            Sha1 = 4,                           // ALG_SID_SHA1
            Sha256 = 12,                        // ALG_SID_SHA_256
            Sha384 = 13,                        // ALG_SID_SHA_384
            Sha512 = 14,                        // ALG_SID_SHA_512

            Aes128 = 14,                        // ALG_SID_AES_128
            Aes192 = 15,                        // ALG_SID_AES_192
            Aes256 = 16                         // ALG_SID_AES_256
        }

        internal enum AlgorithmId {
            None = 0,

            Aes128 = (AlgorithmClass.DataEncryption | AlgorithmType.Block | AlgorithmSubId.Aes128),     // CALG_AES_128
            Aes192 = (AlgorithmClass.DataEncryption | AlgorithmType.Block | AlgorithmSubId.Aes192),     // CALG_AES_192
            Aes256 = (AlgorithmClass.DataEncryption | AlgorithmType.Block | AlgorithmSubId.Aes256),     // CALG_AES_256

            MD5 = (AlgorithmClass.Hash | AlgorithmType.Any | AlgorithmSubId.MD5),                       // CALG_MD5
            Sha1 = (AlgorithmClass.Hash | AlgorithmType.Any | AlgorithmSubId.Sha1),                     // CALG_SHA1
            Sha256 = (AlgorithmClass.Hash | AlgorithmType.Any | AlgorithmSubId.Sha256),                 // CALG_SHA_256
            Sha384 = (AlgorithmClass.Hash | AlgorithmType.Any | AlgorithmSubId.Sha384),                 // CALG_SHA_384
            Sha512 = (AlgorithmClass.Hash | AlgorithmType.Any | AlgorithmSubId.Sha512)                  // CALG_SHA_512
        }

        /// <summary>
        ///     Flags for the CryptAcquireContext API
        /// </summary>
        [Flags]
        internal enum CryptAcquireContextFlags {
            None = 0x00000000,
            VerifyContext = unchecked((int)0xF0000000)      // CRYPT_VERIFYCONTEXT
        }

        /// <summary>
        ///     Error codes returned from CAPI
        /// </summary>
        internal enum ErrorCode {
            Success = 0x00000000,                                       // ERROR_SUCCESS
            MoreData = 0x00000ea,                                       // ERROR_MORE_DATA
            NoMoreItems = 0x00000103,                                   // ERROR_NO_MORE_ITEMS
            BadData = unchecked((int)0x80090005),                       // NTE_BAD_DATA
            BadAlgorithmId = unchecked((int)0x80090008),                // NTE_BAD_ALGID
            ProviderTypeNotDefined = unchecked((int)0x80090017),        // NTE_PROV_TYPE_NOT_DEF
            KeysetNotDefined = unchecked((int)0x80090019)               // NTE_KEYSET_NOT_DEF
        }

        /// <summary>
        ///     Parameters that GetHashParam can query
        /// </summary>
        internal enum HashParameter {
            None = 0x0000,
            AlgorithmId = 0x0001,           // HP_ALGID
            HashValue = 0x0002,             // HP_HASHVAL
            HashSize = 0x0004               // HP_HASHSIZE
        }

        /// <summary>
        ///     Formats of blobs that keys can appear in
        /// </summary>
        internal enum KeyBlobType : byte {
            PlainText = 0x8                 // PLAINTEXTKEYBLOB
        }

        /// <summary>
        ///     Flags for CryptGenKey and CryptImportKey
        /// </summary>
        [Flags]
        internal enum KeyFlags {
            None = 0x0000,
            Exportable = 0x0001             // CRYPT_EXPORTABLE
        }

        /// <summary>
        ///     Parameters of a cryptographic key used by SetKeyParameter 
        /// </summary>
        internal enum KeyParameter {
            None = 0,
            IV = 1,                         // KP_IV
            Mode = 4,                       // KP_MODE
            ModeBits = 5                    // KP_MODE_BITS
        }

        /// <summary>
        ///     Well-known names of crypto service providers
        /// </summary>
        internal static class ProviderNames {
            // MS_ENH_RSA_AES_PROV
            public const string MicrosoftEnhancedRsaAes = "Microsoft Enhanced RSA and AES Cryptographic Provider";
            public const string MicrosoftEnhancedRsaAesPrototype = "Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype)";
        }

        /// <summary>
        ///     Parameters exposed by a CSP
        /// </summary>
        internal enum ProviderParameter {
            None = 0,
            EnumerateAlgorithms = 1             // PP_ENUMALGS
        }

        /// <summary>
        ///     Flags controlling information retrieved about a provider parameter
        /// </summary>
        [Flags]
        internal enum ProviderParameterFlags {
            None = 0x00000000,
            RestartEnumeration = 0x00000001     // CRYPT_FIRST
        }

        /// <summary>
        ///     Provider type accessed in a crypto service provider. These provide the set of algorithms
        ///     available to use for an application.
        /// </summary>
        internal enum ProviderType {
            None = 0,
            RsaAes = 24         // PROV_RSA_AES
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BLOBHEADER {
            public KeyBlobType bType;
            public byte bVersion;
            public short reserved;
            public AlgorithmId aiKeyAlg;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPTOAPI_BLOB {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_DSS_PARAMETERS
        {
            public CRYPTOAPI_BLOB p;
            public CRYPTOAPI_BLOB q;
            public CRYPTOAPI_BLOB g;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct PROV_ENUMALGS {
            public AlgorithmId aiAlgId;
            public int dwBitLen;
            public int dwNameLen;
            public fixed byte szName[20];
        }

        internal const uint ALG_CLASS_SIGNATURE = (1 << 13);
        internal const uint ALG_TYPE_RSA = (2 << 9);
        internal const uint ALG_SID_RSA_ANY = 0;
        internal const uint ALG_SID_DSS_ANY = 0;
        internal const uint ALG_TYPE_DSS = (1 << 9);
        internal const uint ALG_CLASS_KEY_EXCHANGE = (5 << 13);

        internal const uint CALG_RSA_SIGN = (ALG_CLASS_SIGNATURE | ALG_TYPE_RSA | ALG_SID_RSA_ANY);
        internal const uint CALG_DSS_SIGN = (ALG_CLASS_SIGNATURE | ALG_TYPE_DSS | ALG_SID_DSS_ANY);
        internal const uint CALG_RSA_KEYX = (ALG_CLASS_KEY_EXCHANGE | ALG_TYPE_RSA | ALG_SID_RSA_ANY);
        internal const uint CNG_RSA_PUBLIC_KEY_BLOB = 72;
        internal const uint X509_DSS_PUBLICKEY = 38;
        internal const uint X509_DSS_PARAMETERS = 39;

        internal const uint X509_ASN_ENCODING = 0x00000001;
        internal const uint PKCS_7_ASN_ENCODING = 0x00010000;

        internal const uint CRYPT_OID_INFO_OID_KEY = 1;

        internal const uint LMEM_FIXED = 0x0000;
        internal const uint LMEM_ZEROINIT = 0x0040;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPT_OID_INFO {
            internal CRYPT_OID_INFO(int size) {
                cbSize = (uint)size;
                pszOID = null;
                pwszName = null;
                dwGroupId = 0;
                Algid = 0;
                ExtraInfo = new CRYPTOAPI_BLOB();
            }
            internal uint cbSize;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszOID;
            internal string pwszName;
            internal uint dwGroupId;
            internal uint Algid;
            internal CRYPTOAPI_BLOB ExtraInfo;
        }


#if FEATURE_CORESYSTEM
        [SecurityCritical]
#else
#pragma warning disable 618    // Have not migrated to v4 transparency yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
#endif
        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods {
            /// <summary>
            ///     Calculate the public key token for a given public key
            /// </summary>
            [DllImport("clr")]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern int _AxlPublicKeyBlobToPublicKeyToken(ref CRYPTOAPI_BLOB pCspPublicKeyBlob,
                                                                       [Out] out SafeAxlBufferHandle ppwszPublicKeyToken);

            /// <summary>
            ///     Open a crypto service provider, if a key container is specified KeyContainerPermission
            ///     should be demanded.
            /// </summary>
            [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptAcquireContext([Out] out SafeCspHandle phProv,
                                                          string pszContainer,
                                                          string pszProvider,
                                                          ProviderType dwProvType,
                                                          CryptAcquireContextFlags dwFlags);

            /// <summary>
            ///     Prepare a new hash algorithm for use
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptCreateHash(SafeCspHandle hProv,
                                                      AlgorithmId Algid,
                                                      SafeCapiKeyHandle hKey,
                                                      int dwFlags,
                                                      [Out] out SafeCapiHashHandle phHash);

            /// <summary>
            ///     Decrypt a block of data
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptDecrypt(SafeCapiKeyHandle hKey,
                                                   SafeCapiHashHandle hHash,
                                                   [MarshalAs(UnmanagedType.Bool)] bool Final,
                                                   int dwFlags,
                                                   IntPtr pbData, // BYTE *
                                                   [In, Out] ref int pdwDataLen);

            /// <summary>
            ///     Duplicate a key handle
            /// </summary>
            [DllImport("advapi32")]
#if !FEATURE_CORESYSTEM
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptDuplicateKey(SafeCapiKeyHandle hKey,
                                                        IntPtr pdwReserved,
                                                        int dwFlags,
                                                        [Out] out SafeCapiKeyHandle phKey);

            /// <summary>
            ///     Encrypt a block of data
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptEncrypt(SafeCapiKeyHandle hKey,
                                                   SafeCapiHashHandle hHash,
                                                   [MarshalAs(UnmanagedType.Bool)] bool Final,
                                                   int dwFlags,
                                                   IntPtr pbData, // BYTE *
                                                   [In, Out] ref int pdwDataLen,
                                                   int dwBufLen);
                                                   
            /// <summary>
            ///     Export a key into a byte array
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptExportKey(SafeCapiKeyHandle hKey,
                                                     SafeCapiKeyHandle hExpKey,
                                                     int dwBlobType,            // (int)KeyBlobType
                                                     int dwExportFlags,
                                                     [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                                     [In, Out] ref int pdwDataLen);
            /// <summary>
            ///     Generate a random key
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptGenKey(SafeCspHandle hProv,
                                                  AlgorithmId Algid,
                                                  KeyFlags dwFlags,
                                                  [Out] out SafeCapiKeyHandle phKey);

            /// <summary>
            ///     Fill a buffer with cryptographically random bytes
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptGenRandom(SafeCspHandle hProv,
                                                     int dwLen,
                                                     [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbBuffer);

            /// <summary>
            ///     Get information about a hash algorithm, including the current value
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptGetHashParam(SafeCapiHashHandle hHash,
                                                        HashParameter dwParam,
                                                        [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                                        [In, Out] ref int pdwDataLen,
                                                        int dwFlags);

            /// <summary>
            ///     Get information about a CSP
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptGetProvParam(SafeCspHandle hProv,
                                                        ProviderParameter dwParam,
                                                        IntPtr pbData,
                                                        [In, Out] ref int pdwDataLen,
                                                        ProviderParameterFlags dwFlags);

            /// <summary>
            ///     Add a block of data to a hash
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptHashData(SafeCapiHashHandle hHash,
                                                    [MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                                    int dwDataLen,
                                                    int dwFlags);

            /// <summary>
            ///     Import a key into a CSP
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptImportKey(SafeCspHandle hProv,
                                                     [MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                                     int dwDataLen,
                                                     SafeCapiKeyHandle hPubKey,
                                                     KeyFlags dwFlags,
                                                     [Out] out SafeCapiKeyHandle phKey);

            /// <summary>
            ///     Set a property of a key
            /// </summary>
            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
#if FEATURE_CORESYSTEM
            [SecurityCritical]
#endif
            public static extern bool CryptSetKeyParam(SafeCapiKeyHandle hKey,
                                                       KeyParameter dwParam,
                                                       [MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
                                                       int dwFlags);

            //Added for X509Certificate extension support 
#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
            [DllImport("CRYPT32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            internal extern static
            IntPtr CryptFindOIDInfo(
                [In]     uint dwKeyType,
                [In]     IntPtr pvKey,
                [In]     OidGroup dwGroupId);

#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
            [DllImport("CRYPT32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            internal extern static
            IntPtr CryptFindOIDInfo(
                [In]     uint dwKeyType,
                [In]     SafeLocalAllocHandle pvKey,
                [In]     OidGroup dwGroupId);


#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
            [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern
            bool CryptDecodeObject(
                [In]     uint dwCertEncodingType,
                [In]     IntPtr lpszStructType,
                [In]     IntPtr pbEncoded,
                [In]     uint cbEncoded,
                [In]     uint dwFlags,
                [In, Out] SafeLocalAllocHandle pvStructInfo,
                [In, Out] IntPtr pcbStructInfo);

#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
            [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern
            bool CryptDecodeObject(
                [In]     uint dwCertEncodingType,
                [In]     IntPtr lpszStructType,
                [In]     byte[] pbEncoded,
                [In]     uint cbEncoded,
                [In]     uint dwFlags,
                [In, Out] SafeLocalAllocHandle pvStructInfo,
                [In, Out] IntPtr pcbStructInfo);
#if FEATURE_CORESYSTEM
        [SecurityCritical]
#endif
            [DllImport("KERNEL32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern
            SafeLocalAllocHandle LocalAlloc(
                [In] uint uFlags,
                [In] IntPtr sizetdwBytes);
        }

        //
        // Utility and wrapper functions
        //

        /// <summary>
        ///     Acquire a crypto service provider
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        internal static SafeCspHandle AcquireCsp(string keyContainer,
                                                 string providerName,
                                                 ProviderType providerType,
                                                 CryptAcquireContextFlags flags,
                                                 bool throwPlatformException) {
            Contract.Ensures(Contract.Result<SafeCspHandle>() != null &&
                             !Contract.Result<SafeCspHandle>().IsInvalid &&
                             !Contract.Result<SafeCspHandle>().IsClosed);

            SafeCspHandle cspHandle = null;
            if (!UnsafeNativeMethods.CryptAcquireContext(out cspHandle,
                                                         keyContainer,
                                                         providerName,
                                                         providerType,
                                                         flags)) {
                // If the platform doesn't have the specified CSP, we'll either get a ProviderTypeNotDefined
                // or a KeysetNotDefined error depending on the CAPI version.
                int error = Marshal.GetLastWin32Error();
                if (throwPlatformException && (error == (int)CapiNative.ErrorCode.ProviderTypeNotDefined ||
                                               error == (int)CapiNative.ErrorCode.KeysetNotDefined)) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
                }
                else {
                    throw new CryptographicException(error);
                }
            }

            return cspHandle;
        }

        /// <summary>
        ///     Export a symmetric algorithm key into a byte array
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        internal static byte[] ExportSymmetricKey(SafeCapiKeyHandle key) {
            Contract.Requires(key != null);
            Contract.Ensures(Contract.Result<byte[]>() != null && Contract.Result<byte[]>().Length > 0);

            //
            // Figure out how big the key blob is, and export it
            //

            int keySize = 0;
            if (!UnsafeNativeMethods.CryptExportKey(key,
                                                    SafeCapiKeyHandle.InvalidHandle,
                                                    (int)KeyBlobType.PlainText,
                                                    0,
                                                    null,
                                                    ref keySize)) {
                int error = Marshal.GetLastWin32Error();

                if (error != (int)ErrorCode.MoreData) {
                    throw new CryptographicException(error);
                }
            }

            byte[] keyBlob = new byte[keySize];
            if (!UnsafeNativeMethods.CryptExportKey(key,
                                                    SafeCapiKeyHandle.InvalidHandle,
                                                    (int)KeyBlobType.PlainText,
                                                    0,
                                                    keyBlob,
                                                    ref keySize)) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            //
            // Strip the headers from the key to access the raw data
            //
            // A PLAINTEXTBLOB is laid out as follows:
            //   BLOBHEADER hdr
            //   DWORD      cbKeySize
            //   BYTE       rbgKeyData[]
            //

            int keyDataOffset = Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int));
            Debug.Assert(keyBlob.Length > keyDataOffset, "Key blob is in an unexpected format.");

            int keyLength = BitConverter.ToInt32(keyBlob, Marshal.SizeOf(typeof(BLOBHEADER)));
            Debug.Assert(keyLength > 0, "Unexpected key length.");
            Debug.Assert(keyBlob.Length >= keyDataOffset + keyLength, "Key blob is in an unexpected format.");

            byte[] keyData = new byte[keyLength];
            Buffer.BlockCopy(keyBlob, keyDataOffset, keyData, 0, keyData.Length);
            return keyData;
        }

        /// <summary>
        ///     Map an algorithm ID to a string name
        /// </summary>
        internal static string GetAlgorithmName(AlgorithmId algorithm) {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            return algorithm.ToString().ToUpper(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Get the value of a specific hash parameter
        /// </summary>
        [System.Security.SecurityCritical]
        internal static byte[] GetHashParameter(SafeCapiHashHandle hashHandle, CapiNative.HashParameter parameter) {
            Contract.Requires(hashHandle != null);
            Contract.Requires(CapiNative.HashParameter.AlgorithmId <= parameter && parameter <= CapiNative.HashParameter.HashSize);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            //
            // Determine the maximum size of the parameter and retrieve it
            //

            int parameterSize = 0;
            if (!CapiNative.UnsafeNativeMethods.CryptGetHashParam(hashHandle, parameter, null, ref parameterSize, 0)) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            Debug.Assert(0 < parameterSize, "Invalid parameter size returned");
            byte[] parameterValue = new byte[parameterSize];
            if (!CapiNative.UnsafeNativeMethods.CryptGetHashParam(hashHandle, parameter, parameterValue, ref parameterSize, 0)) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            // CAPI may have asked for a larger buffer than it used, so only copy the used bytes
            if (parameterSize != parameterValue.Length) {
                byte[] realValue = new byte[parameterSize];
                Buffer.BlockCopy(parameterValue, 0, realValue, 0, parameterSize);
                parameterValue = realValue;
            }

            return parameterValue;
        }

        /// <summary>
        ///     Get information about a CSP. This should only be used for calls where the returned information
        ///     is in the form of a structure.
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        internal static T GetProviderParameterStruct<T>(SafeCspHandle provider,
                                                        ProviderParameter parameter,
                                                        ProviderParameterFlags flags) where T : struct {
            Contract.Requires(provider != null);
            Contract.Requires(parameter == ProviderParameter.EnumerateAlgorithms);

            // Figure out how big the parameter is
            int bufferSize = 0;
            IntPtr buffer = IntPtr.Zero;

            if (!UnsafeNativeMethods.CryptGetProvParam(provider, parameter, buffer, ref bufferSize, flags)) {
                int errorCode = Marshal.GetLastWin32Error();

                // NoMoreItems means that we've finished the enumeration we're currently working on, this is
                // not a real error, so return an empty structure to mark the end.
                if (errorCode == (int)ErrorCode.NoMoreItems) {
                    return new T();
                }
                else if (errorCode != (int)ErrorCode.MoreData) {
                    throw new CryptographicException(errorCode);
                }
            }

            Debug.Assert(Marshal.SizeOf(typeof(T)) <= bufferSize, "Buffer size does not match structure size");

            //
            // Pull the parameter back and marshal it into the return structure
            //

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                // Allocate in a CER because we could fail between the alloc and the assignment
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally {
                    buffer = Marshal.AllocCoTaskMem(bufferSize);
                }

                if (!UnsafeNativeMethods.CryptGetProvParam(provider, parameter, buffer, ref bufferSize, flags)) {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }

                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally {
                if (buffer != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }
        }

        /// <summary>
        ///     Map a verification result to a matching HRESULT
        /// </summary>
        internal static int HResultForVerificationResult(SignatureVerificationResult verificationResult) {
            switch (verificationResult) {
                case SignatureVerificationResult.AssemblyIdentityMismatch:
                case SignatureVerificationResult.PublicKeyTokenMismatch:
                case SignatureVerificationResult.PublisherMismatch:
                    return (int)SignatureVerificationResult.BadSignatureFormat;

                case SignatureVerificationResult.ContainingSignatureInvalid:
                    return (int)SignatureVerificationResult.BadDigest;

                default:
                    return (int)verificationResult;
            }
        }

        /// <summary>
        ///     Import a symmetric key into a CSP
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        internal static SafeCapiKeyHandle ImportSymmetricKey(SafeCspHandle provider, AlgorithmId algorithm, byte[] key) {
            Contract.Requires(provider != null);
            Contract.Requires(((int)algorithm & (int)AlgorithmClass.DataEncryption) == (int)AlgorithmClass.DataEncryption);
            Contract.Requires(key != null);
            Contract.Ensures(Contract.Result<SafeCapiKeyHandle>() != null &&
                             !Contract.Result<SafeCapiKeyHandle>().IsInvalid &&
                             !Contract.Result<SafeCapiKeyHandle>().IsClosed);

            //
            // Setup a PLAINTEXTKEYBLOB (v2) which has the following format:
            //   BLOBHEADER hdr
            //   DWORD      cbKeySize
            //   BYTE       rbgKeyData[]
            //

            int blobSize = Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int)) + key.Length;
            byte[] keyBlob = new byte[blobSize];

            unsafe {
                fixed (byte *pBlob = keyBlob) {
                    BLOBHEADER* pHeader = (BLOBHEADER*)pBlob;
                    pHeader->bType = KeyBlobType.PlainText;
                    pHeader->bVersion = 2;
                    pHeader->reserved = 0;
                    pHeader->aiKeyAlg = algorithm;

                    int* pSize = (int *)(pBlob + Marshal.SizeOf(*pHeader));
                    *pSize = key.Length;
                }
            }

            Buffer.BlockCopy(key, 0, keyBlob, Marshal.SizeOf(typeof(BLOBHEADER)) + Marshal.SizeOf(typeof(int)), key.Length);

            // Import the PLAINTEXTKEYBLOB into the CSP
            SafeCapiKeyHandle importedKey = null;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                if (!UnsafeNativeMethods.CryptImportKey(provider,
                                                        keyBlob,
                                                        keyBlob.Length,
                                                        SafeCapiKeyHandle.InvalidHandle,
                                                        KeyFlags.Exportable,
                                                        out importedKey)) {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally {
                if (importedKey != null && !importedKey.IsInvalid) {
                    importedKey.SetParentCsp(provider);
                }
            }
            return importedKey;
        }

        /// <summary>
        ///     Set a DWORD key parameter (KP_MODE and KP_MODE_BITS)
        /// </summary>
        [System.Security.SecurityCritical]
        internal static void SetKeyParameter(SafeCapiKeyHandle key, KeyParameter parameter, int value) {
            Contract.Requires(key != null);
            Contract.Requires(parameter == KeyParameter.Mode || parameter == KeyParameter.ModeBits);

            SetKeyParameter(key, parameter, BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     Set the value of one of a key's parameters
        /// </summary>
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        internal static void SetKeyParameter(SafeCapiKeyHandle key, KeyParameter parameter, byte[] value) {
            Contract.Requires(key != null && !key.IsInvalid && !key.IsClosed);
            Contract.Requires(value != null);

            if (!UnsafeNativeMethods.CryptSetKeyParam(key, parameter, value, 0)) {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        //Wrapper methods for certificate extensions

        /// <summary>
        /// Local alloc wrapper. 
        /// </summary>
        /// <param name="uFlags"></param>
        /// <param name="sizetdwBytes"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static SafeLocalAllocHandle LocalAlloc(uint uFlags, IntPtr sizetdwBytes) {
            SafeLocalAllocHandle safeLocalAllocHandle = UnsafeNativeMethods.LocalAlloc(uFlags, sizetdwBytes);
            if (safeLocalAllocHandle == null || safeLocalAllocHandle.IsInvalid) {
                throw new OutOfMemoryException();
            }
            return safeLocalAllocHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszStructType"></param>
        /// <param name="pbEncoded"></param>
        /// <param name="cbEncoded"></param>
        /// <param name="decodedValue"></param>
        /// <param name="cbDecodedValue"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static unsafe bool DecodeObject(IntPtr pszStructType,
                  IntPtr pbEncoded,
                  uint cbEncoded,
                  out SafeLocalAllocHandle decodedValue,
                  out uint cbDecodedValue)
        {
            // Initialize out parameters
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;

            // Decode
            uint cbDecoded = 0;
            SafeLocalAllocHandle ptr = SafeLocalAllocHandle.InvalidHandle;
            if (!UnsafeNativeMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                        pszStructType,
                                                        pbEncoded,
                                                        cbEncoded,
                                                        0,
                                                        ptr,
                                                        new IntPtr(&cbDecoded))) {
                return false;
            }
            ptr = LocalAlloc(LMEM_FIXED, new IntPtr(cbDecoded));

            if (!UnsafeNativeMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                                   pszStructType,
                                                   pbEncoded,
                                                   cbEncoded,
                                                   0,
                                                   ptr,
                                                   new IntPtr(&cbDecoded))) {
                return false;
            }
            // Return decoded values
            decodedValue = ptr;
            cbDecodedValue = cbDecoded;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszStructType"></param>
        /// <param name="pbEncoded"></param>
        /// <param name="decodedValue"></param>
        /// <param name="cbDecodedValue"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static unsafe bool DecodeObject(IntPtr pszStructType,
                          byte[] pbEncoded,
                          out SafeLocalAllocHandle decodedValue,
                          out uint cbDecodedValue)
        {
            // Initialize out parameters
            decodedValue = SafeLocalAllocHandle.InvalidHandle;
            cbDecodedValue = 0;

            // Decode
            uint cbDecoded = 0;
            SafeLocalAllocHandle pbDecoded = SafeLocalAllocHandle.InvalidHandle;

            if (!UnsafeNativeMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               pszStructType,
                                               pbEncoded,
                                               (uint)pbEncoded.Length,
                                               0,
                                               pbDecoded,
                                               new IntPtr(&cbDecoded))) {
                return false;
            }
            pbDecoded = LocalAlloc(LMEM_FIXED, new IntPtr(cbDecoded));
            if (!UnsafeNativeMethods.CryptDecodeObject(X509_ASN_ENCODING | PKCS_7_ASN_ENCODING,
                                               pszStructType,
                                               pbEncoded,
                                               (uint)pbEncoded.Length,
                                               0,
                                               pbDecoded,
                                               new IntPtr(&cbDecoded))) {
                return false;
            }
            // Return decoded values
            decodedValue = pbDecoded;
            cbDecodedValue = cbDecoded;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwKeyType"></param>
        /// <param name="pvKey"></param>
        /// <param name="dwGroupId"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static CRYPT_OID_INFO CryptFindOIDInfo(
            [In]    uint dwKeyType,
            [In]    IntPtr pvKey,
            [In]    OidGroup dwGroupId) {

            if (pvKey == IntPtr.Zero) {
                throw new ArgumentNullException("pvKey");
            }
            CRYPT_OID_INFO pOIDInfo = new CRYPT_OID_INFO(Marshal.SizeOf(typeof(CRYPT_OID_INFO)));
            IntPtr pv = UnsafeNativeMethods.CryptFindOIDInfo(dwKeyType,
                                                     pvKey,
                                                     dwGroupId);

            if (pv != IntPtr.Zero) {
                pOIDInfo = (CRYPT_OID_INFO)Marshal.PtrToStructure(pv, typeof(CRYPT_OID_INFO));
            }
            return pOIDInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwKeyType"></param>
        /// <param name="pvKey"></param>
        /// <param name="dwGroupId"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static CRYPT_OID_INFO CryptFindOIDInfo(
            [In]    uint dwKeyType,
            [In]    SafeLocalAllocHandle pvKey,
            [In]    OidGroup dwGroupId) {

            if (pvKey == null) {
                throw new ArgumentNullException("pvKey");
            }
            if (pvKey.IsInvalid) {
                throw new CryptographicException("SR.GetString(SR.Cryptography_InvalidHandle)", "pvKey");
            }
            CRYPT_OID_INFO pOIDInfo = new CRYPT_OID_INFO(Marshal.SizeOf(typeof(CRYPT_OID_INFO)));
            IntPtr pv = UnsafeNativeMethods.CryptFindOIDInfo(dwKeyType,
                                                     pvKey,
                                                     dwGroupId);

            if (pv != IntPtr.Zero) {
                pOIDInfo = (CRYPT_OID_INFO)Marshal.PtrToStructure(pv, typeof(CRYPT_OID_INFO));
            }
            return pOIDInfo;
        }
    }

    /// <summary>
    /// Safe local handle class
    /// </summary>
    internal sealed class SafeLocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid {
        [SecuritySafeCritical]
        private SafeLocalAllocHandle()
            : base(true) {
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [SecuritySafeCritical]
        internal T Read<T>(int offset) where T : struct {
            bool addedRef = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                DangerousAddRef(ref addedRef);
                unsafe {
                    IntPtr pBase = new IntPtr((byte*)handle.ToPointer() + offset);
                    return (T)Marshal.PtrToStructure(pBase, typeof(T));
                }
            }
            finally {
                if (addedRef) {
                    DangerousRelease();
                }
            }
        }

        [SecuritySafeCritical]
        protected override bool ReleaseHandle() {
            return LocalFree(handle) == IntPtr.Zero;
        }

        [SecuritySafeCritical]
        internal SafeLocalAllocHandle(IntPtr handle)
            : base(true) {
            SetHandle(handle);
        }

        internal static SafeLocalAllocHandle InvalidHandle {
            [SecuritySafeCritical]
            get { return new SafeLocalAllocHandle(IntPtr.Zero); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class X509Utils {
        
        [SecuritySafeCritical]
        internal static SafeLocalAllocHandle StringToAnsiPtr(string s) {
            byte[] arr = new byte[s.Length + 1];
            Encoding.ASCII.GetBytes(s, 0, s.Length, arr, 0);
            SafeLocalAllocHandle pb = CapiNative.LocalAlloc(CapiNative.LMEM_FIXED, new IntPtr(arr.Length));
            Marshal.Copy(arr, 0, pb.DangerousGetHandle(), arr.Length);
            return pb;
        }
    }
}
