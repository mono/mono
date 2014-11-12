// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace System.Security.Cryptography.X509Certificates {
    /// <summary>
    ///     Native interop layer for X509 certificate and Authenticode functions. Native definitions can be
    ///     found in wincrypt.h or msaxlapi.h
    /// </summary>
    internal static class X509Native {
        /// <summary>
        ///     Flags for CertVerifyAuthenticodeLicense
        /// </summary>
        [Flags]
        public enum AxlVerificationFlags {
            None                        = 0x00000000,
            NoRevocationCheck           = 0x00000001,   // AXL_REVOCATION_NO_
            RevocationCheckEndCertOnly  = 0x00000002,   // AXL_REVOCATION_
            RevocationCheckEntireChain  = 0x00000004,   // AXL_REVOCATION_
            UrlOnlyCacheRetrieval       = 0x00000008,   // AXL_URL_ONLY_CACHE_RETRIEVAL
            LifetimeSigning             = 0x00000010,   // AXL_LIFETIME_SIGNING
            TrustMicrosoftRootOnly      = 0x00000020    // AXL_TRUST_MICROSOFT_ROOT_ONLY
        }

        [StructLayout(LayoutKind.Sequential)]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
        public struct AXL_AUTHENTICODE_SIGNER_INFO {
            public int cbSize;
            public int dwError;
            public CapiNative.AlgorithmId algHash;

            // Each of the next fields are Unicode strings, however we need to manually marshal them since
            // they are allocated and freed by the native AXL code and should not have their memory handled
            // by the marshaller.
            public IntPtr pwszHash;
            public IntPtr pwszDescription;
            public IntPtr pwszDescriptionUrl;

            public IntPtr pChainContext;        // PCERT_CHAIN_CONTEXT
        }

        [StructLayout(LayoutKind.Sequential)]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
        public struct AXL_AUTHENTICODE_TIMESTAMPER_INFO {
            public int cbsize;
            public int dwError;
            public CapiNative.AlgorithmId algHash;
            public FILETIME ftTimestamp;
            public IntPtr pChainContext;        // PCERT_CHAIN_CONTEXT
        }

        [SuppressUnmanagedCodeSecurity]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#pragma warning disable 618 // System.Core.dll still uses SecurityRuleSet.Level1
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        public static class UnsafeNativeMethods {
            /// <summary>
            ///     Get the hash value of a key blob
            /// </summary>
            [DllImport("clr")]
            public static extern int _AxlGetIssuerPublicKeyHash(IntPtr pCertContext,
                                                                [Out]out SafeAxlBufferHandle ppwszPublicKeyHash);

            /// <summary>
            ///     Release any resources used to create an authenticode signer info structure
            /// </summary>
            [DllImport("clr")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public static extern int CertFreeAuthenticodeSignerInfo(ref AXL_AUTHENTICODE_SIGNER_INFO pSignerInfo);

            /// <summary>
            ///     Release any resources used to create an authenticode timestamper info structure
            /// </summary>
            [DllImport("clr")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public static extern int CertFreeAuthenticodeTimestamperInfo(ref AXL_AUTHENTICODE_TIMESTAMPER_INFO pTimestamperInfo);

            /// <summary>
            ///     Verify the authenticode signature on a manifest
            /// </summary>
            /// <remarks>
            ///     Code must have permission to open and enumerate certificate stores to use this API
            /// </remarks>
            [DllImport("clr")]
            public static extern int CertVerifyAuthenticodeLicense(ref CapiNative.CRYPTOAPI_BLOB pLicenseBlob,
                                                                   AxlVerificationFlags dwFlags,
                                                                   [In, Out] ref AXL_AUTHENTICODE_SIGNER_INFO pSignerInfo,
                                                                   [In, Out] ref AXL_AUTHENTICODE_TIMESTAMPER_INFO pTimestamperInfo);
        }
    }
}
