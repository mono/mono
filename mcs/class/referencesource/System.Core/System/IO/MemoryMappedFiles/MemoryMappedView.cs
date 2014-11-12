// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:   MemoryMappedView
**
** Purpose: Internal class representing MemoryMappedFile view
**
** Date:  February 7, 2007 
**
===========================================================*/

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles {

    internal class MemoryMappedView : IDisposable {

        private SafeMemoryMappedViewHandle m_viewHandle;
        private Int64 m_pointerOffset;
        private Int64 m_size;
        private MemoryMappedFileAccess m_access;

        // These control the retry behaviour when lock violation errors occur during Flush:
        private const Int32 MaxFlushWaits = 15;  // must be <=30
        private const Int32 MaxFlushRetriesPerWait = 20; 

        [System.Security.SecurityCritical]
        private unsafe MemoryMappedView(SafeMemoryMappedViewHandle viewHandle, Int64 pointerOffset, 
                                            Int64 size, MemoryMappedFileAccess access) {

            m_viewHandle = viewHandle;
            m_pointerOffset = pointerOffset;
            m_size = size;
            m_access = access;
        }

        internal SafeMemoryMappedViewHandle ViewHandle {
            [System.Security.SecurityCritical]
            get {
                return m_viewHandle;
            }
        }

        internal Int64 PointerOffset {
            get {
                return m_pointerOffset;
            }
        }

        internal Int64 Size {
            get {
                return m_size;
            }
        }

        internal MemoryMappedFileAccess Access {
            get {
                return m_access;
            }
        }

        // Callers must demand unmanaged code first
        [System.Security.SecurityCritical]
        internal unsafe static MemoryMappedView CreateView(SafeMemoryMappedFileHandle memMappedFileHandle,
                                            MemoryMappedFileAccess access, Int64 offset, Int64 size) {

            // MapViewOfFile can only create views that start at a multiple of the system memory allocation 
            // granularity. We decided to hide this restriction form the user by creating larger views than the
            // user requested and hiding the parts that the user did not request.  extraMemNeeded is the amount of
            // extra memory we allocate before the start of the requested view. MapViewOfFile will also round the 
            // capacity of the view to the nearest multiple of the system page size.  Once again, we hide this 
            // from the user by preventing them from writing to any memory that they did not request.
            ulong extraMemNeeded = (ulong)offset % (ulong)MemoryMappedFile.GetSystemPageAllocationGranularity();

            // newOffset takes into account the fact that we have some extra memory allocated before the requested view
            ulong newOffset = (ulong)offset - extraMemNeeded;
            Debug.Assert(newOffset >= 0, "newOffset = (offset - extraMemNeeded) < 0");

            // determine size to pass to MapViewOfFile
            ulong nativeSize;
            if (size != MemoryMappedFile.DefaultSize) {
                nativeSize = (ulong)size + (ulong)extraMemNeeded;
            }
            else {
                nativeSize = 0;
            }

            if (IntPtr.Size == 4 && nativeSize > UInt32.MaxValue) {
                throw new ArgumentOutOfRangeException("size", SR.GetString(SR.ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed));
            }

            // if request is >= than total virtual, then MapViewOfFile will fail with meaningless error message 
            // "the parameter is incorrect"; this provides better error message in advance
            UnsafeNativeMethods.MEMORYSTATUSEX memStatus = new UnsafeNativeMethods.MEMORYSTATUSEX();
            bool result = UnsafeNativeMethods.GlobalMemoryStatusEx(memStatus);
            ulong totalVirtual = memStatus.ullTotalVirtual; 
            if (nativeSize >= totalVirtual) {
                throw new IOException(SR.GetString(SR.IO_NotEnoughMemory));
            }

            // split the Int64 into two ints
            uint offsetLow = (uint)(newOffset & 0x00000000FFFFFFFFL);
            uint offsetHigh = (uint)(newOffset >> 32);

            // create the view
            SafeMemoryMappedViewHandle viewHandle = UnsafeNativeMethods.MapViewOfFile(memMappedFileHandle, 
                    MemoryMappedFile.GetFileMapAccess(access), offsetHigh, offsetLow, new UIntPtr(nativeSize));
            if (viewHandle.IsInvalid) {
                __Error.WinIOError(Marshal.GetLastWin32Error(), String.Empty);
            }

            // Query the view for its size and allocation type
            UnsafeNativeMethods.MEMORY_BASIC_INFORMATION viewInfo = new UnsafeNativeMethods.MEMORY_BASIC_INFORMATION();
            UnsafeNativeMethods.VirtualQuery(viewHandle, ref viewInfo, (IntPtr)Marshal.SizeOf(viewInfo)); 
            ulong viewSize = (ulong)viewInfo.RegionSize;
            

            // allocate the pages if we were using the MemoryMappedFileOptions.DelayAllocatePages option
            if ((viewInfo.State & UnsafeNativeMethods.MEM_RESERVE) != 0) {
                IntPtr tempHandle = UnsafeNativeMethods.VirtualAlloc(viewHandle, (UIntPtr)viewSize, UnsafeNativeMethods.MEM_COMMIT, 
                                                        MemoryMappedFile.GetPageAccess(access));
                int lastError = Marshal.GetLastWin32Error();
                if (viewHandle.IsInvalid) {
                    __Error.WinIOError(lastError, String.Empty);
                }
            }

            // if the user specified DefaultSize as the size, we need to get the actual size
            if (size == MemoryMappedFile.DefaultSize) {
                size = (Int64)(viewSize - extraMemNeeded);
            }
            else {
                Debug.Assert(viewSize >= (ulong)size, "viewSize < size");
            }

            viewHandle.Initialize((ulong)size + extraMemNeeded);
            MemoryMappedView mmv = new MemoryMappedView(viewHandle, (long)extraMemNeeded, size, access);
            return mmv;

        }

        // Flushes the changes such that they are in [....] with the FileStream bits (ones obtained
        // with the win32 ReadFile and WriteFile functions).  Need to call FileStream's Flush to 
        // flush to the disk.
        // NOTE: This will flush all bytes before and after the view up until an offset that is a multiple
        //       of SystemPageSize.
        [System.Security.SecurityCritical]
        public void Flush(IntPtr capacity) {

            if (m_viewHandle != null) {

                unsafe {
                    byte* firstPagePtr = null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                        m_viewHandle.AcquirePointer(ref firstPagePtr);

                        bool success = UnsafeNativeMethods.FlushViewOfFile(firstPagePtr, capacity);
                        if (success)
                            return; // This will visit the finally block.

                        // It is a known issue within the NTFS transaction log system that
                        // causes FlushViewOfFile to intermittently fail with ERROR_LOCK_VIOLATION
                        // [http://bugcheck/bugs/Windows8Bugs/152862].
                        // As a workaround, we catch this particular error and retry the flush operation 
                        // a few milliseconds later. If it does not work, we give it a few more tries with
                        // increasing intervals. Eventually, however, we need to give up. In ad-hoc tests
                        // this strategy successfully flushed the view after no more than 3 retries.

                        Int32 error = Marshal.GetLastWin32Error();
                        bool canRetry = (!success && error == UnsafeNativeMethods.ERROR_LOCK_VIOLATION);

                        for (Int32 w = 0; canRetry && w < MaxFlushWaits; w++) {

                            Int32 pause = (1 << w);  // MaxFlushRetries should never be over 30
                            Thread.Sleep(pause);

                            for (Int32 r = 0; canRetry && r < MaxFlushRetriesPerWait; r++) {

                                success = UnsafeNativeMethods.FlushViewOfFile(firstPagePtr, capacity);
                                if (success)
                                    return; // This will visit the finally block.

                                Thread.Sleep(0);

                                error = Marshal.GetLastWin32Error();
                                canRetry = (error == UnsafeNativeMethods.ERROR_LOCK_VIOLATION);
                            }
                        }

                        // We got too here, so there was no success:
                        __Error.WinIOError(error, String.Empty);                        
                    }
                    finally {
                        if (firstPagePtr != null) {
                            m_viewHandle.ReleasePointer();
                        }
                    }
                }

            }
        }
                

        [System.Security.SecurityCritical]
        protected virtual void Dispose(bool disposing) {

            if (m_viewHandle != null && !m_viewHandle.IsClosed) {
                m_viewHandle.Dispose();
            }
        }

        [System.Security.SecurityCritical]
        public void Dispose() {

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal bool IsClosed {
            [SecuritySafeCritical]
            get {
                return (m_viewHandle == null || m_viewHandle.IsClosed);
            }
        }

    }

}
