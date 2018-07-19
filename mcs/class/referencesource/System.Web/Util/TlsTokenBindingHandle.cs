//------------------------------------------------------------------------------
// <copyright file="TlsTokenBindingHandle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Web.Hosting;

    internal sealed class TlsTokenBindingHandle : HeapAllocHandle {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"Pointer is valid while this SafeHandle is valid.")]
        private readonly IntPtr _providedTokenBlob;
        private readonly uint _providedTokenBlobSize;
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"Pointer is valid while this SafeHandle is valid.")]
        private readonly IntPtr _referredTokenBlob;
        private readonly uint _referredtokenBlobSize;

        internal TlsTokenBindingHandle(IntPtr mgdContext) {
            int hr = UnsafeIISMethods.MgdGetTlsTokenBindingIdentifiers(
                mgdContext,
                ref handle,
                out _providedTokenBlob,
                out _providedTokenBlobSize,
                out _referredTokenBlob,
                out _referredtokenBlobSize);
            Misc.ThrowIfFailedHr(hr);
        }

        public byte[] GetProvidedToken() {
            return GetTokenImpl(_providedTokenBlob, _providedTokenBlobSize);
        }

        public byte[] GetReferredToken() {
            return GetTokenImpl(_referredTokenBlob, _referredtokenBlobSize);
        }

        private byte[] GetTokenImpl(IntPtr blob, uint blobSize) {
            if (blob == IntPtr.Zero || blobSize == 0) {
                return null;
            }
            else {
                byte[] retVal = new byte[blobSize];
                int length = retVal.Length; // checks for overflow
                bool refAdded = false;

                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    DangerousAddRef(ref refAdded);
                    Marshal.Copy(blob, retVal, 0, length);
                }
                finally
                {
                    if (refAdded)
                    {
                        DangerousRelease();
                    }
                }

                return retVal;
            }
        }
    }
}
