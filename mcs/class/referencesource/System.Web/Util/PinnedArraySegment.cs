//------------------------------------------------------------------------------
// <copyright file="PinnedArraySegment.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    // Utility class for pinning an ArraySegment<T> so that it can be passed to unmanaged code.
    //
    // This type is not thread safe.

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class PinnedArraySegment<T> : IDisposable {

        private int _count;
        private GCHandle _gcHandle;

        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"This is a memory pointer, not a handle. The handle is in the _gcHandle field, and its lifetime is controlled by the caller.")]
        private IntPtr _pointer;

        internal unsafe PinnedArraySegment(ArraySegment<T> segment) {
            // Structs - like ArraySegment<T> - can be "torn" by malicious users trying to take
            // advantage of race conditions that occur as a result of their copy-by-value semantics.
            // Since we'll pass the ArraySegment<T> to unmanaged code, we need to perform validation
            // to make sure that this hasn't happened. The ArraySegment<T> constructor can be used
            // to perform this validation. (MSRC 10170)
            segment = new ArraySegment<T>(segment.Array, segment.Offset, segment.Count);

            _gcHandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned); // pin the array so that unmanaged code can access it
            _pointer = Marshal.UnsafeAddrOfPinnedArrayElement(segment.Array, segment.Offset);
            _count = segment.Count;
        }

        public int Count {
            get {
                ThrowIfDisposed();
                return _count;
            }
        }

        public IntPtr Pointer {
            get {
                ThrowIfDisposed();
                return _pointer;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2216:DisposableTypesShouldDeclareFinalizer", Justification = @"We don't own native resources.")]
        public void Dispose() {
            if (_pointer != IntPtr.Zero) {
                // only free the GCHandle once
                _pointer = IntPtr.Zero;
                _gcHandle.Free();
            }
        }

        private void ThrowIfDisposed() {
            if (_pointer == IntPtr.Zero) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

    }
}
