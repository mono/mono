// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;

namespace Microsoft.Win32.SafeHandles {
    /// <summary>
    ///     SafeHandle representing a BCRYPT_ALG_HANDLE
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeBCryptAlgorithmHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeBCryptAlgorithmHandle() : base(true) {
        }

        [DllImport("bcrypt")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        private static extern BCryptNative.ErrorCode BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, int flags);

        protected override bool ReleaseHandle() {
            return BCryptCloseAlgorithmProvider(handle, 0) == BCryptNative.ErrorCode.Success;
        }
    }

    /// <summary>
    ///     Safe handle representing a BCRYPT_HASH_HANDLE and the associated buffer holding the hash object
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeBCryptHashHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private IntPtr m_hashObject;

        private SafeBCryptHashHandle() : base(true) {
        }

        /// <summary>
        ///     Buffer holding the hash object. This buffer should be allocated with Marshal.AllocCoTaskMem.
        /// </summary>
        internal IntPtr HashObject {
            get { return m_hashObject; }

            set {
                Contract.Requires(value != IntPtr.Zero);
                m_hashObject = value;
            }
        }


        [DllImport("bcrypt")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        private static extern BCryptNative.ErrorCode BCryptDestroyHash(IntPtr hHash);

        protected override bool ReleaseHandle() {
            bool success = BCryptDestroyHash(handle) == BCryptNative.ErrorCode.Success;

            // The hash object buffer must be released only after destroying the hash handle
            if (m_hashObject != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(m_hashObject);
            }

            return success;
        }
    }
}
