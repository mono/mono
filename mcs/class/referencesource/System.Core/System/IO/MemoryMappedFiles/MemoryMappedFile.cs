// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  MemoryMappedFile
**
** Purpose: Managed MemoryMappedFiles.
**
** Date:  February 7, 2007
**
===========================================================*/

using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;


namespace System.IO.MemoryMappedFiles {

    public class MemoryMappedFile : IDisposable {

        private SafeMemoryMappedFileHandle _handle;
        private bool _leaveOpen;
        private FileStream _fileStream;
        internal const int DefaultSize = 0;

        // Private constructors to be used by the factory methods.
        [System.Security.SecurityCritical]
        private MemoryMappedFile(SafeMemoryMappedFileHandle handle) {
            Debug.Assert(handle != null && !handle.IsClosed && !handle.IsInvalid, "handle is null, closed, or invalid");

            _handle = handle;
            _leaveOpen = true; // No FileStream to dispose of in this case.
        }

        [System.Security.SecurityCritical]
        private MemoryMappedFile(SafeMemoryMappedFileHandle handle, FileStream fileStream, bool leaveOpen) {
            Debug.Assert(handle != null && !handle.IsClosed && !handle.IsInvalid, "handle is null, closed, or invalid");
            Debug.Assert(fileStream != null, "fileStream is null");

            _handle = handle;
            _fileStream = fileStream;
            _leaveOpen = leaveOpen;
        }

        // Factory Method Group #1: Opens an existing named memory mapped file. The native OpenFileMapping call
        // will check the desiredAccessRights against the ACL on the memory mapped file.  Note that a memory 
        // mapped file created without an ACL will use a default ACL taken from the primary or impersonation token
        // of the creator.  On my machine, I always get ReadWrite access to it so I never have to use anything but
        // the first override of this method.  Note: having ReadWrite access to the object does not mean that we 
        // have ReadWrite access to the pages mapping the file.  The OS will check against the access on the pages
        // when a view is created. 
        public static MemoryMappedFile OpenExisting(string mapName) {
            return OpenExisting(mapName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
        }

        public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights) {
            return OpenExisting(mapName, desiredAccessRights, HandleInheritability.None);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile OpenExisting(string mapName, MemoryMappedFileRights desiredAccessRights,
                                                                    HandleInheritability inheritability) {

            if (mapName == null) {
                throw new ArgumentNullException("mapName", SR.GetString(SR.ArgumentNull_MapName));
            }

            if (mapName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_MapNameEmptyString));
            }

            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability");
            }

            if (((int)desiredAccessRights & ~((int)(MemoryMappedFileRights.FullControl | MemoryMappedFileRights.AccessSystemSecurity))) != 0) {
                throw new ArgumentOutOfRangeException("desiredAccessRights");
            }

            SafeMemoryMappedFileHandle handle = OpenCore(mapName, inheritability, (int)desiredAccessRights, false);
            return new MemoryMappedFile(handle);
        }

        // Factory Method Group #2: Creates a new memory mapped file where the content is taken from an existing 
        // file on disk.  This file must be opened by a FileStream before given to us.  Specifying DefaultSize to 
        // the capacity will make the capacity of the memory mapped file match the size of the file.  Specifying
        // a value larger than the size of the file will enlarge the new file to this size.  Note that in such a
        // case, the capacity (and there for the size of the file) will be rounded up to a multiple of the system
        // page size.  One can use FileStream.SetLength to bring the length back to a desirable size. By default, 
        // the MemoryMappedFile will close the FileStream object when it is disposed.  This behavior can be 
        // changed by the leaveOpen boolean argument.
        public static MemoryMappedFile CreateFromFile(String path) {
            return CreateFromFile(path, FileMode.Open, null, DefaultSize, MemoryMappedFileAccess.ReadWrite);
        }
        public static MemoryMappedFile CreateFromFile(String path, FileMode mode) {
            return CreateFromFile(path, mode, null, DefaultSize, MemoryMappedFileAccess.ReadWrite);
        }

        public static MemoryMappedFile CreateFromFile(String path, FileMode mode, String mapName) {
            return CreateFromFile(path, mode, mapName, DefaultSize, MemoryMappedFileAccess.ReadWrite);
        }

        public static MemoryMappedFile CreateFromFile(String path, FileMode mode, String mapName, Int64 capacity) {
            return CreateFromFile(path, mode, mapName, capacity, MemoryMappedFileAccess.ReadWrite);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateFromFile(String path, FileMode mode, String mapName, Int64 capacity,
                                                                        MemoryMappedFileAccess access) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }

            if (mapName != null && mapName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_MapNameEmptyString));
            }

            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_PositiveOrDefaultCapacityRequired));
            }

            if (access < MemoryMappedFileAccess.ReadWrite ||
                access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (mode == FileMode.Append) {
                throw new ArgumentException(SR.GetString(SR.Argument_NewMMFAppendModeNotAllowed), "mode");
            }
            if (access == MemoryMappedFileAccess.Write) {
                throw new ArgumentException(SR.GetString(SR.Argument_NewMMFWriteAccessNotAllowed), "access");
            }

            bool existed = File.Exists(path);
            FileStream fileStream = new FileStream(path, mode, GetFileStreamFileSystemRights(access), FileShare.None, 0x1000, FileOptions.None);

            if (capacity == 0 && fileStream.Length == 0) {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentException(SR.GetString(SR.Argument_EmptyFile));
            }

            if (access == MemoryMappedFileAccess.Read && capacity > fileStream.Length) {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentException(SR.GetString(SR.Argument_ReadAccessWithLargeCapacity));
            }

            if (capacity == DefaultSize) {
                capacity = fileStream.Length;
            }

            // one can always create a small view if they do not want to map an entire file 
            if (fileStream.Length > capacity) {
                CleanupFile(fileStream, existed, path);
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_CapacityGEFileSizeRequired));
            }

            SafeMemoryMappedFileHandle handle = null;
            try {
                handle = CreateCore(fileStream.SafeFileHandle, mapName, HandleInheritability.None, null,
                    access, MemoryMappedFileOptions.None, capacity);
            }
            catch {
                CleanupFile(fileStream, existed, path);
                throw;
            }

            Debug.Assert(handle != null && !handle.IsInvalid);
            return new MemoryMappedFile(handle, fileStream, false);
        }

        public static MemoryMappedFile CreateFromFile(FileStream fileStream, String mapName, Int64 capacity,
                                                        MemoryMappedFileAccess access,
                                                        HandleInheritability inheritability, bool leaveOpen) {
            return CreateFromFile(fileStream, mapName, capacity, access, null, inheritability, leaveOpen);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateFromFile(FileStream fileStream, String mapName, Int64 capacity,
                                                        MemoryMappedFileAccess access, MemoryMappedFileSecurity memoryMappedFileSecurity, 
                                                        HandleInheritability inheritability, bool leaveOpen) {

            if (fileStream == null) {
                throw new ArgumentNullException("fileStream", SR.GetString(SR.ArgumentNull_FileStream));
            }

            if (mapName != null && mapName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_MapNameEmptyString));
            }

            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_PositiveOrDefaultCapacityRequired));
            }

            if (capacity == 0 && fileStream.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_EmptyFile));
            }

            if (access < MemoryMappedFileAccess.ReadWrite ||
                access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (access == MemoryMappedFileAccess.Write) {
                throw new ArgumentException(SR.GetString(SR.Argument_NewMMFWriteAccessNotAllowed), "access");
            }

            if (access == MemoryMappedFileAccess.Read && capacity > fileStream.Length) {
                throw new ArgumentException(SR.GetString(SR.Argument_ReadAccessWithLargeCapacity));
            }

            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability");
            }

            // flush any bytes written to the FileStream buffer so that we can see them in our MemoryMappedFile
            fileStream.Flush();

            if (capacity == DefaultSize) {
                capacity = fileStream.Length;
            }

            // one can always create a small view if they do not want to map an entire file 
            if (fileStream.Length > capacity) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_CapacityGEFileSizeRequired));
            }

            SafeMemoryMappedFileHandle handle = CreateCore(fileStream.SafeFileHandle, mapName, inheritability, memoryMappedFileSecurity, 
                access, MemoryMappedFileOptions.None, capacity);

            return new MemoryMappedFile(handle, fileStream, leaveOpen);
        }

        // Factory Method Group #3: Creates a new empty memory mapped file.  Such memory mapped files are ideal 
        // for IPC, when mapName != null. 
        public static MemoryMappedFile CreateNew(String mapName, Int64 capacity) {
            return CreateNew(mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, null,
                   HandleInheritability.None);
        }

        public static MemoryMappedFile CreateNew(String mapName, Int64 capacity, MemoryMappedFileAccess access) {
            return CreateNew(mapName, capacity, access, MemoryMappedFileOptions.None, null,
                   HandleInheritability.None);
        }

        public static MemoryMappedFile CreateNew(String mapName, Int64 capacity, MemoryMappedFileAccess access,
                                                    MemoryMappedFileOptions options,
                                                    HandleInheritability inheritability) {
            return CreateNew(mapName, capacity, access, options, null, inheritability);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateNew(String mapName, Int64 capacity, MemoryMappedFileAccess access,
                                                    MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity,
                                                    HandleInheritability inheritability) {

            if (mapName != null && mapName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_MapNameEmptyString));
            }

            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_NeedPositiveNumber));
            }

            if (IntPtr.Size == 4 && capacity > UInt32.MaxValue) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed));
            }

            if (access < MemoryMappedFileAccess.ReadWrite ||
                access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (access == MemoryMappedFileAccess.Write) {
                throw new ArgumentException(SR.GetString(SR.Argument_NewMMFWriteAccessNotAllowed), "access");
            }

            if (((int)options & ~((int)(MemoryMappedFileOptions.DelayAllocatePages ))) != 0) {
                throw new ArgumentOutOfRangeException("options");
            }

            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability");
            }

            SafeMemoryMappedFileHandle handle = CreateCore(new SafeFileHandle(new IntPtr(-1), true), mapName, inheritability, 
                memoryMappedFileSecurity, access, options, capacity);

            return new MemoryMappedFile(handle);
        }

        // Factory Method Group #4: Creates a new empty memory mapped file or opens an existing
        // memory mapped file if one exists with the same name.  The capacity, options, and 
        // memoryMappedFileSecurity arguments will be ignored in the case of the later.
        // This is ideal for P2P style IPC.
        public static MemoryMappedFile CreateOrOpen(String mapName, Int64 capacity) {
            return CreateOrOpen(mapName, capacity, MemoryMappedFileAccess.ReadWrite,
                MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        public static MemoryMappedFile CreateOrOpen(String mapName, Int64 capacity,
                                                    MemoryMappedFileAccess access) {
            return CreateOrOpen(mapName, capacity, access, MemoryMappedFileOptions.None, null, HandleInheritability.None);
        }

        public static MemoryMappedFile CreateOrOpen(String mapName, Int64 capacity,
                                                    MemoryMappedFileAccess access, MemoryMappedFileOptions options,
                                                    HandleInheritability inheritability) {
            return CreateOrOpen(mapName, capacity, access, options, null, inheritability);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static MemoryMappedFile CreateOrOpen(String mapName, Int64 capacity,
                                                    MemoryMappedFileAccess access, MemoryMappedFileOptions options,
                                                    MemoryMappedFileSecurity memoryMappedFileSecurity, 
                                                    HandleInheritability inheritability) {

            if (mapName == null) {
                throw new ArgumentNullException("mapName", SR.GetString(SR.ArgumentNull_MapName));
            }

            if (mapName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_MapNameEmptyString));
            }

            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_NeedPositiveNumber));
            }

            if (IntPtr.Size == 4 && capacity > UInt32.MaxValue) {
                throw new ArgumentOutOfRangeException("capacity", SR.GetString(SR.ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed));
            }

            if (access < MemoryMappedFileAccess.ReadWrite ||
                access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (((int)options & ~((int)(MemoryMappedFileOptions.DelayAllocatePages))) != 0) {
                throw new ArgumentOutOfRangeException("options");
            }

            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability");
            }

            SafeMemoryMappedFileHandle handle;
            // special case for write access; create will never succeed
            if (access == MemoryMappedFileAccess.Write) {
                handle = OpenCore(mapName, inheritability, GetFileMapAccess(access), true);
            }
            else {
                handle = CreateOrOpenCore(new SafeFileHandle(new IntPtr(-1), true), mapName, inheritability,
                    memoryMappedFileSecurity, access, options, capacity);
            }

            return new MemoryMappedFile(handle);
        }

        // Used by the 2 Create factory method groups.  A -1 fileHandle specifies that the 
        // memory mapped file should not be associated with an exsiting file on disk (ie start
        // out empty).
        [System.Security.SecurityCritical]
        private static SafeMemoryMappedFileHandle CreateCore(SafeFileHandle fileHandle, String mapName,
                                                    HandleInheritability inheritability,
                                                    MemoryMappedFileSecurity memoryMappedFileSecurity,
                                                    MemoryMappedFileAccess access, MemoryMappedFileOptions options,
                                                    Int64 capacity) {

            SafeMemoryMappedFileHandle handle = null;
            Object pinningHandle;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(inheritability, memoryMappedFileSecurity, out pinningHandle);
            
            // split the long into two ints
            Int32 capacityLow = (Int32)(capacity & 0x00000000FFFFFFFFL);
            Int32 capacityHigh = (Int32)(capacity >> 32);

            try {

                handle = UnsafeNativeMethods.CreateFileMapping(fileHandle, secAttrs, GetPageAccess(access) | (int)options,
                    capacityHigh, capacityLow, mapName);

                Int32 errorCode = Marshal.GetLastWin32Error();
                if (!handle.IsInvalid && errorCode == UnsafeNativeMethods.ERROR_ALREADY_EXISTS) {
                    handle.Dispose();
                    __Error.WinIOError(errorCode, String.Empty);
                }
                else if (handle.IsInvalid) {
                    __Error.WinIOError(errorCode, String.Empty);
                }
            }
            finally {
                if (pinningHandle != null) {
                    GCHandle pinHandle = (GCHandle)pinningHandle;
                    pinHandle.Free();
                }
            }
            return handle;
        }

        // Used by the OpenExisting factory method group and by CreateOrOpen if access is write.
        // We'll throw an ArgumentException if the file mapping object didn't exist and the
        // caller used CreateOrOpen since Create isn't valid with Write access
        [System.Security.SecurityCritical]
        private static SafeMemoryMappedFileHandle OpenCore(String mapName, HandleInheritability inheritability, 
                                                                int desiredAccessRights, bool createOrOpen) {

            SafeMemoryMappedFileHandle handle = UnsafeNativeMethods.OpenFileMapping(desiredAccessRights,
                (inheritability & HandleInheritability.Inheritable) != 0, mapName);
            Int32 lastError = Marshal.GetLastWin32Error();

            if (handle.IsInvalid) {
                if (createOrOpen && (lastError == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)) {
                    throw new ArgumentException(SR.GetString(SR.Argument_NewMMFWriteAccessNotAllowed), "access");
                }
                else {
                    __Error.WinIOError(lastError, String.Empty);
                }
            }
            return handle;
        }


        // Used by the CreateOrOpen factory method groups.  A -1 fileHandle specifies that the 
        // memory mapped file should not be associated with an existing file on disk (ie start
        // out empty).
        //
        // Try to open the file if it exists -- this requires a bit more work. Loop until we can
        // either create or open a memory mapped file up to a timeout. CreateFileMapping may fail
        // if the file exists and we have non-null security attributes, in which case we need to
        // use OpenFileMapping.  But, there exists a race condition because the memory mapped file
        // may have closed inbetween the two calls -- hence the loop. 
        // 
        // This uses similar retry/timeout logic as in performance counter. It increases the wait
        // time each pass through the loop and times out in approximately 1.4 minutes. If after 
        // retrying, a MMF handle still hasn't been opened, throw an InvalidOperationException.
        //
        [System.Security.SecurityCritical]
        private static SafeMemoryMappedFileHandle CreateOrOpenCore(SafeFileHandle fileHandle, String mapName, 
                                                                HandleInheritability inheritability, 
                                                                MemoryMappedFileSecurity memoryMappedFileSecurity,
                                                                MemoryMappedFileAccess access, MemoryMappedFileOptions options,
                                                                Int64 capacity) {

            Debug.Assert(access != MemoryMappedFileAccess.Write, "Callers requesting write access shouldn't try to create a mmf");

            SafeMemoryMappedFileHandle handle = null;
            Object pinningHandle;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(inheritability, memoryMappedFileSecurity, out pinningHandle);
            
            // split the long into two ints
            Int32 capacityLow = (Int32)(capacity & 0x00000000FFFFFFFFL);
            Int32 capacityHigh = (Int32)(capacity >> 32);

            try {

                int waitRetries = 14;   //((2^13)-1)*10ms == approximately 1.4mins
                int waitSleep = 0;

                // keep looping until we've exhausted retries or break as soon we we get valid handle
                while (waitRetries > 0) {

                    // try to create
                    handle = UnsafeNativeMethods.CreateFileMapping(fileHandle, secAttrs, 
                        GetPageAccess(access) | (int)options, capacityHigh, capacityLow, mapName);

                    Int32 createErrorCode = Marshal.GetLastWin32Error();
                    if (!handle.IsInvalid) {
                        break;
                    }
                    else {
                        if (createErrorCode != UnsafeNativeMethods.ERROR_ACCESS_DENIED) {
                            __Error.WinIOError(createErrorCode, String.Empty);
                        }

                        // the mapname exists but our ACL is preventing us from opening it with CreateFileMapping.  
                        // Let's try to open it with OpenFileMapping.
                        handle.SetHandleAsInvalid();
                    }

                    // try to open
                    handle = UnsafeNativeMethods.OpenFileMapping(GetFileMapAccess(access), (inheritability &
                            HandleInheritability.Inheritable) != 0, mapName);

                    Int32 openErrorCode = Marshal.GetLastWin32Error();

                    // valid handle
                    if (!handle.IsInvalid) {
                        break;
                    }
                    // didn't get valid handle; have to retry
                    else {

                        if (openErrorCode != UnsafeNativeMethods.ERROR_FILE_NOT_FOUND) {
                            __Error.WinIOError(openErrorCode, String.Empty);
                        }

                        // increase wait time
                        --waitRetries;
                        if (waitSleep == 0) {
                            waitSleep = 10;
                        }
                        else {
                            System.Threading.Thread.Sleep(waitSleep);
                            waitSleep *= 2;
                        }
                    }
                }

                // finished retrying but couldn't create or open
                if (handle == null || handle.IsInvalid) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_CantCreateFileMapping));
                }
            }

            finally {
                if (pinningHandle != null) {
                    GCHandle pinHandle = (GCHandle)pinningHandle;
                    pinHandle.Free();
                }
            }
            return handle;
        }
   
        // Creates a new view in the form of a stream.
        public MemoryMappedViewStream CreateViewStream() {
            return CreateViewStream(0, DefaultSize, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewStream CreateViewStream(Int64 offset, Int64 size) {
            return CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public MemoryMappedViewStream CreateViewStream(Int64 offset, Int64 size, MemoryMappedFileAccess access) {

            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }

            if (size < 0) {
                throw new ArgumentOutOfRangeException("size", SR.GetString(SR.ArgumentOutOfRange_PositiveOrDefaultSizeRequired));
            }

            if (access < MemoryMappedFileAccess.ReadWrite || access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (IntPtr.Size == 4 && size > UInt32.MaxValue) {
                throw new ArgumentOutOfRangeException("size", SR.GetString(SR.ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed));
            }

            MemoryMappedView view = MemoryMappedView.CreateView(_handle, access, offset, size);
            return new MemoryMappedViewStream(view);
        }

        // Creates a new view in the form of an accessor.  Accessors are for random access.
        public MemoryMappedViewAccessor CreateViewAccessor() {
            return CreateViewAccessor(0, DefaultSize, MemoryMappedFileAccess.ReadWrite);
        }

        public MemoryMappedViewAccessor CreateViewAccessor(Int64 offset, Int64 size) {
            return CreateViewAccessor(offset, size, MemoryMappedFileAccess.ReadWrite);
        }

        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public MemoryMappedViewAccessor CreateViewAccessor(Int64 offset, Int64 size, MemoryMappedFileAccess access) {

            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }

            if (size < 0) {
                throw new ArgumentOutOfRangeException("size", SR.GetString(SR.ArgumentOutOfRange_PositiveOrDefaultSizeRequired));
            }

            if (access < MemoryMappedFileAccess.ReadWrite || access > MemoryMappedFileAccess.ReadWriteExecute) {
                throw new ArgumentOutOfRangeException("access");
            }

            if (IntPtr.Size == 4 && size > UInt32.MaxValue) {
                throw new ArgumentOutOfRangeException("size", SR.GetString(SR.ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed));
            }

            MemoryMappedView view = MemoryMappedView.CreateView(_handle, access, offset, size);
            return new MemoryMappedViewAccessor(view);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing) {
            try {
                if (_handle != null && !_handle.IsClosed) {
                    _handle.Dispose();
                }
            }
            finally {
                if (_fileStream != null && _leaveOpen == false) {
                    _fileStream.Dispose();
                }
            }
        }

        public SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle {
            [SecurityCritical]
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get { 
                return _handle; 
            }
        }

        [System.Security.SecurityCritical]
        public MemoryMappedFileSecurity GetAccessControl() {
            if (_handle.IsClosed) {
                __Error.FileNotOpen();
            }
            return new MemoryMappedFileSecurity(_handle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [System.Security.SecurityCritical]
        public void SetAccessControl(MemoryMappedFileSecurity memoryMappedFileSecurity) {

            if (memoryMappedFileSecurity == null) {
                throw new ArgumentNullException("memoryMappedFileSecurity");
            }

            if (_handle.IsClosed) {
                __Error.FileNotOpen();
            }

            memoryMappedFileSecurity.PersistHandle(_handle);
        }

        // We don't need to expose this now that we have created views that can start at any address. 
        [System.Security.SecurityCritical]
        internal static Int32 GetSystemPageAllocationGranularity() {
            UnsafeNativeMethods.SYSTEM_INFO info = new UnsafeNativeMethods.SYSTEM_INFO();
            UnsafeNativeMethods.GetSystemInfo(ref info);

            return info.dwAllocationGranularity;
        }

        // This converts a MemoryMappedFileAccess to it's corresponding native PAGE_XXX value to be used by the 
        // factory methods that construct a new memory mapped file object. MemoryMappedFileAccess.Write is not 
        // valid here since there is no corresponding PAGE_XXX value.
        internal static Int32 GetPageAccess(MemoryMappedFileAccess access) {

            if (access == MemoryMappedFileAccess.Read) {
                return UnsafeNativeMethods.PAGE_READONLY;
            }
            else if (access == MemoryMappedFileAccess.ReadWrite) {
                return UnsafeNativeMethods.PAGE_READWRITE;
            }
            else if (access == MemoryMappedFileAccess.CopyOnWrite) {
                return UnsafeNativeMethods.PAGE_WRITECOPY;
            }
            else if (access == MemoryMappedFileAccess.ReadExecute) {
                return UnsafeNativeMethods.PAGE_EXECUTE_READ;
            }
            else if (access == MemoryMappedFileAccess.ReadWriteExecute) {
                return UnsafeNativeMethods.PAGE_EXECUTE_READWRITE;
            }

            // If we reached here, access was invalid.
            throw new ArgumentOutOfRangeException("access");
        }

        // This converts a MemoryMappedFileAccess to its corresponding native FILE_MAP_XXX value to be used when 
        // creating new views.  
        internal static Int32 GetFileMapAccess(MemoryMappedFileAccess access) {

            if (access == MemoryMappedFileAccess.Read) {
                return UnsafeNativeMethods.FILE_MAP_READ;
            }
            else if (access == MemoryMappedFileAccess.Write) {
                return UnsafeNativeMethods.FILE_MAP_WRITE;
            }
            else if (access == MemoryMappedFileAccess.ReadWrite) {
                return UnsafeNativeMethods.FILE_MAP_READ | UnsafeNativeMethods.FILE_MAP_WRITE;
            }
            else if (access == MemoryMappedFileAccess.CopyOnWrite) {
                return UnsafeNativeMethods.FILE_MAP_COPY;
            }
            else if (access == MemoryMappedFileAccess.ReadExecute) {
                return UnsafeNativeMethods.FILE_MAP_EXECUTE | UnsafeNativeMethods.FILE_MAP_READ;
            }
            else if (access == MemoryMappedFileAccess.ReadWriteExecute) {
                return UnsafeNativeMethods.FILE_MAP_EXECUTE | UnsafeNativeMethods.FILE_MAP_READ |
                       UnsafeNativeMethods.FILE_MAP_WRITE;
            }

            // If we reached here, access was invalid.
            throw new ArgumentOutOfRangeException("access");
        }

        // This converts a MemoryMappedFileAccess to a FileSystemRights for use by FileStream 
        private static FileSystemRights GetFileStreamFileSystemRights(MemoryMappedFileAccess access) {
            switch (access) {
                case MemoryMappedFileAccess.Read:
                case MemoryMappedFileAccess.CopyOnWrite:
                    return FileSystemRights.ReadData;

                case MemoryMappedFileAccess.ReadWrite:
                    return FileSystemRights.ReadData | FileSystemRights.WriteData;

                case MemoryMappedFileAccess.Write:
                    return FileSystemRights.WriteData;

                case MemoryMappedFileAccess.ReadExecute:
                    return FileSystemRights.ReadData | FileSystemRights.ExecuteFile;

                case MemoryMappedFileAccess.ReadWriteExecute:
                    return FileSystemRights.ReadData | FileSystemRights.WriteData | FileSystemRights.ExecuteFile;

                default:
                    // If we reached here, access was invalid.
                    throw new ArgumentOutOfRangeException("access");
            }
        }


        // This converts a MemoryMappedFileAccess to a FileAccess. MemoryMappedViewStream and 
        // MemoryMappedViewAccessor subclass UnmanagedMemoryStream and UnmanagedMemoryAccessor, which both use 
        // FileAccess to determine whether they are writable and/or readable.  
        internal static FileAccess GetFileAccess(MemoryMappedFileAccess access) {

            if (access == MemoryMappedFileAccess.Read) {
                return FileAccess.Read;
            }
            if (access == MemoryMappedFileAccess.Write) {
                return FileAccess.Write;
            }
            else if (access == MemoryMappedFileAccess.ReadWrite) {
                return FileAccess.ReadWrite;
            }
            else if (access == MemoryMappedFileAccess.CopyOnWrite) {
                return FileAccess.ReadWrite;
            }
            else if (access == MemoryMappedFileAccess.ReadExecute) {
                return FileAccess.Read;
            }
            else if (access == MemoryMappedFileAccess.ReadWriteExecute) {
                return FileAccess.ReadWrite;
            }

            // If we reached here, access was invalid.
            throw new ArgumentOutOfRangeException("access");
        }

        // Helper method used to extract the native binary security descriptor from the MemoryMappedFileSecurity
        // type. If pinningHandle is not null, caller must free it AFTER the call to CreateFile has returned.
        [System.Security.SecurityCritical]
        private unsafe static UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, 
                                        MemoryMappedFileSecurity memoryMappedFileSecurity, out Object pinningHandle) {
            
            pinningHandle = null;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = null;
            if ((inheritability & HandleInheritability.Inheritable) != 0 ||
                memoryMappedFileSecurity != null) {
                secAttrs = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (Int32)Marshal.SizeOf(secAttrs);

                if ((inheritability & HandleInheritability.Inheritable) != 0) {
                    secAttrs.bInheritHandle = 1;
                }

                // For ACLs, get the security descriptor from the MemoryMappedFileSecurity.
                if (memoryMappedFileSecurity != null) {
                    byte[] sd = memoryMappedFileSecurity.GetSecurityDescriptorBinaryForm();
                    pinningHandle = GCHandle.Alloc(sd, GCHandleType.Pinned);
                    fixed (byte* pSecDescriptor = sd)
                        secAttrs.pSecurityDescriptor = pSecDescriptor;
                }
            }
            return secAttrs;
        }

        // clean up: close file handle and delete files we created
        private static void CleanupFile(FileStream fileStream, bool existed, String path) {
            fileStream.Close();
            if (!existed) {
                File.Delete(path);
            }
        }
    }

}
