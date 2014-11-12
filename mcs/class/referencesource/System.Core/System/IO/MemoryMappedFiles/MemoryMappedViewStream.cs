// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:    MemoryMappedViewStream
**
** Purpose:  View stream for managed MemoryMappedFiles.
**
** Date:  February 7, 2007
**
===========================================================*/

using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles {

    public sealed class MemoryMappedViewStream : UnmanagedMemoryStream {

        private MemoryMappedView m_view;

        [System.Security.SecurityCritical]
        internal unsafe MemoryMappedViewStream(MemoryMappedView view) {
            Debug.Assert(view != null, "view is null");

            m_view = view;
            Initialize(m_view.ViewHandle, m_view.PointerOffset, m_view.Size, MemoryMappedFile.GetFileAccess(m_view.Access));
        }

        public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle {

            [System.Security.SecurityCritical]
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get {
                return m_view != null ? m_view.ViewHandle : null; 
            }
        }

        public override void SetLength(long value) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_MMViewStreamsFixedLength));
        }

        public long PointerOffset
        {
            get
            {
                if (m_view == null)
                {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_ViewIsNull));
                }

                return m_view.PointerOffset;
            }
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing) {
            try {
                if (disposing && m_view != null && !m_view.IsClosed) {
                    Flush();
                }
            }
            finally {
                try {
                    if (m_view != null) {
                        m_view.Dispose();
                    }
                }
                finally {
                    base.Dispose(disposing);
                }
            }
        }

        // Flushes the changes such that they are in [....] with the FileStream bits (ones obtained
        // with the win32 ReadFile and WriteFile functions).  Need to call FileStream's Flush to 
        // flush to the disk.
        // NOTE: This will flush all bytes before and after the view up until an offset that is a 
        // multiple of SystemPageSize.
        [System.Security.SecurityCritical]
        public override void Flush() {
            if (!CanSeek) {
                __Error.StreamIsClosed();
            }

            unsafe {
                if (m_view != null) {
                    m_view.Flush((IntPtr)Capacity);
                }
            }
        }

    }
}
