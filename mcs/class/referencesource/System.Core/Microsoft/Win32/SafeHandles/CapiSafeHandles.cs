// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Microsoft.Win32.SafeHandles {
    /// <summary>
    ///     SafeHandle for buffers returned by the Axl APIs
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeAxlBufferHandle : SafeHandleZeroOrMinusOneIsInvalid {
        private SafeAxlBufferHandle() : base(true) {
            return;
        }

        [DllImport("kernel32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr GetProcessHeap();

        [DllImport("kernel32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool HeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem);

        protected override bool ReleaseHandle() {
            // _AxlFree is a wrapper around HeapFree on the process heap. Since it is not exported from mscorwks
            // we just call HeapFree directly. This needs to be updated if _AxlFree is ever changed.
            HeapFree(GetProcessHeap(), 0, handle);
            return true;
        }
    }

    /// <summary>
    ///     SafeHandle base class for CAPI handles (such as HCRYPTKEY and HCRYPTHASH) which must keep their
    ///     CSP alive as long as they stay alive as well. CAPI requires that all child handles belonging to a
    ///     HCRYPTPROV must be destroyed up before the reference count to the HCRYPTPROV drops to zero.
    ///     Since we cannot control the order of finalization between the two safe handles, SafeCapiHandleBase
    ///     maintains a native refcount on its parent HCRYPTPROV to ensure that if the corresponding
    ///     SafeCspKeyHandle is finalized first CAPI still keeps the provider alive.
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal abstract class SafeCapiHandleBase : SafeHandleZeroOrMinusOneIsInvalid {
        private IntPtr m_csp;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal SafeCapiHandleBase() : base(true) {
        }

        [DllImport("advapi32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptContextAddRef(IntPtr hProv,
                                                      IntPtr pdwReserved,
                                                      int dwFlags);

        [DllImport("advapi32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);


        protected IntPtr ParentCsp {
            get { return m_csp; }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)] 
            set {
                // We should not be resetting the parent CSP if it's already been set once - that will
                // lead to leaking the original handle.
                Debug.Assert(m_csp == IntPtr.Zero);
  
                int error = (int)CapiNative.ErrorCode.Success;
 
                // A successful call to CryptContextAddRef and an assignment of the handle value to our field
                // SafeHandle need to happen atomically, so we contain them within a CER. 
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally {
                    if (CryptContextAddRef(value, IntPtr.Zero, 0)) {
                        m_csp = value;
                    }
                    else {
                        error = Marshal.GetLastWin32Error();
                    }
                }
 
                if (error != (int)CapiNative.ErrorCode.Success) {
                    throw new CryptographicException(error);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void SetParentCsp(SafeCspHandle parentCsp) {
            bool addedRef = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                parentCsp.DangerousAddRef(ref addedRef);
                IntPtr rawParentHandle = parentCsp.DangerousGetHandle();
                ParentCsp = rawParentHandle;
            }
            finally {
                if (addedRef) {
                    parentCsp.DangerousRelease();
                }
            }
        }

        protected abstract bool ReleaseCapiChildHandle();

        protected override sealed bool ReleaseHandle() {
            // Order is important here - we must destroy the child handle before the parent CSP
            bool destroyedChild = ReleaseCapiChildHandle();
            bool releasedCsp = true;

            if (m_csp != IntPtr.Zero) {
                releasedCsp = CryptReleaseContext(m_csp, 0);
            }

            return destroyedChild && releasedCsp;
        }
    }

    /// <summary>
    ///     SafeHandle for CAPI hash algorithms (HCRYPTHASH)
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeCapiHashHandle : SafeCapiHandleBase {
        private SafeCapiHashHandle() {
        }

        /// <summary>
        ///     NULL hash handle
        /// </summary>
        public static SafeCapiHashHandle InvalidHandle {
            get {
                SafeCapiHashHandle handle = new SafeCapiHashHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }

        [DllImport("advapi32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptDestroyHash(IntPtr hHash);

        protected override bool ReleaseCapiChildHandle() {
            return CryptDestroyHash(handle);
        }
    }

    /// <summary>
    ///     SafeHandle for CAPI keys (HCRYPTKEY)
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeCapiKeyHandle : SafeCapiHandleBase {
        private SafeCapiKeyHandle()  {
        }

        /// <summary>
        ///     NULL key handle
        /// </summary>
        internal static SafeCapiKeyHandle InvalidHandle {
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
            get {
                SafeCapiKeyHandle handle = new SafeCapiKeyHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }

        [DllImport("advapi32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptDestroyKey(IntPtr hKey);

        /// <summary>
        ///     Make a copy of this key handle
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")] 
        internal SafeCapiKeyHandle Duplicate() {
            Contract.Requires(!IsInvalid && !IsClosed);
            Contract.Ensures(Contract.Result<SafeCapiKeyHandle>() != null && !Contract.Result<SafeCapiKeyHandle>().IsInvalid && !Contract.Result<SafeCapiKeyHandle>().IsClosed);

            SafeCapiKeyHandle duplicate = null;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                if (!CapiNative.UnsafeNativeMethods.CryptDuplicateKey(this, IntPtr.Zero, 0, out duplicate)) {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
            }
            finally {
                if (duplicate != null && !duplicate.IsInvalid && ParentCsp != IntPtr.Zero) {
                    duplicate.ParentCsp = ParentCsp;
                }
            }

            return duplicate;
        }

        protected override bool ReleaseCapiChildHandle() {
            return CryptDestroyKey(handle);
        }
    }

    /// <summary>
    ///     SafeHandle for crypto service providers (HCRYPTPROV)
    /// </summary>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal sealed class SafeCspHandle : SafeHandleZeroOrMinusOneIsInvalid {
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private SafeCspHandle() : base(true) {
            return;
        }

        [DllImport("advapi32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptContextAddRef(SafeCspHandle hProv,
                                                     IntPtr pdwReserved,
                                                     int dwFlags);

        [DllImport("advapi32")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);

        /// <summary>
        ///     Create a second SafeCspHandle which refers to the same HCRYPTPROV
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")] 
        public SafeCspHandle Duplicate() {
            Contract.Requires(!IsInvalid && !IsClosed);

            // In the window between the call to CryptContextAddRef and when the raw handle value is assigned
            // into this safe handle, there's a second reference to the original safe handle that the CLR does
            // not know about, so we need to bump the reference count around this entire operation to ensure
            // that we don't have the original handle closed underneath us.
            bool acquired = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                DangerousAddRef(ref acquired);
                IntPtr originalHandle = DangerousGetHandle();

                int error = (int)CapiNative.ErrorCode.Success;

                SafeCspHandle duplicate = new SafeCspHandle();

                // A successful call to CryptContextAddRef and an assignment of the handle value to the duplicate
                // SafeHandle need to happen atomically, so we contain them within a CER. 
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally {
                    if (!CryptContextAddRef(this, IntPtr.Zero, 0)) {
                        error = Marshal.GetLastWin32Error();
                    }
                    else {
                        duplicate.SetHandle(originalHandle);
                    }
                }

                // If we could not call CryptContextAddRef succesfully, then throw the error here otherwise
                // we should be in a valid state at this point.
                if (error != (int)CapiNative.ErrorCode.Success) {
                    duplicate.Dispose();
                    throw new CryptographicException(error);
                }
                else {
                    Debug.Assert(!duplicate.IsInvalid, "Failed to duplicate handle successfully");
                }

                return duplicate;
            }
            finally {
                if (acquired) {
                    DangerousRelease();
                }
            }
        }

        protected override bool ReleaseHandle() {
            return CryptReleaseContext(handle, 0);
        }
    }
}
