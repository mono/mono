// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  LogStream
**
===========================================================*/
using System;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Versioning;

using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace System.IO {

// This stream has very limited support to enable EventSchemaTraceListener
// Eventually we might want to add more functionality and expose this type 
internal class LogStream : BufferedStream2
{
    internal const long DefaultFileSize = 10*1000*1024;
    internal const int DefaultNumberOfFiles = 2;
    internal const LogRetentionOption DefaultRetention = LogRetentionOption.SingleFileUnboundedSize;

    // Retention policy
    private const int _retentionRetryThreshold = 2;
    private LogRetentionOption _retention;
    private long _maxFileSize = DefaultFileSize;
    private int _maxNumberOfFiles = DefaultNumberOfFiles; 
    private int _currentFileNum = 1;
    bool _disableLogging;
    int _retentionRetryCount;

    private bool _canRead;
    private bool _canWrite;
    private bool _canSeek;
    [SecurityCritical]
    private SafeFileHandle _handle;
    
    private String _fileName;       // Fully qualified file name.
    string _fileNameWithoutExt;
    string _fileExt;
    
    // Save input for retention 
    string _pathSav;
    int _fAccessSav;
    FileShare _shareSav; 
    UnsafeNativeMethods.SECURITY_ATTRIBUTES _secAttrsSav; 
    FileIOPermissionAccess _secAccessSav;
    FileMode _modeSav; 
    int _flagsAndAttributesSav;
    bool _seekToEndSav;

    private readonly object m_lockObject = new Object();

    //Limited to immediate internal need from EventSchemaTraceListener
    //Not param validation done!!
    [ResourceExposure(ResourceScope.Machine)]
    [ResourceConsumption(ResourceScope.Machine)]
    [System.Security.SecurityCritical]
    internal LogStream(String path, int bufferSize, LogRetentionOption retention, long maxFileSize, int maxNumOfFiles) 
    {
        Debug.Assert(!String.IsNullOrEmpty(path));

        // Get absolute path - Security needs this to prevent something
        // like trying to create a file in c:\tmp with the name 
        // "..\WinNT\System32\ntoskrnl.exe".  Store it for user convenience.
        //String filePath = Path.GetFullPathInternal(path);
        String filePath = Path.GetFullPath(path);
        _fileName = filePath;

        // Prevent access to your disk drives as raw block devices.
        if (filePath.StartsWith("\\\\.\\", StringComparison.Ordinal))
            throw new NotSupportedException(SR.GetString(SR.NotSupported_IONonFileDevices));

        UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(FileShare.Read);
        
        // For mitigating local elevation of privilege attack through named pipes
        // make sure we always call CreateFile with SECURITY_ANONYMOUS so that the
        // named pipe server can't impersonate a high privileged client security context
        int flagsAndAttributes = (int)FileOptions.None | (UnsafeNativeMethods.SECURITY_SQOS_PRESENT | UnsafeNativeMethods.SECURITY_ANONYMOUS);
        
        // Only write is enabled
        //_canRead = false;
        //_canSeek = false;
        _canWrite = true;

        _pathSav = filePath;
        _fAccessSav = UnsafeNativeMethods.GENERIC_WRITE;
        _shareSav = FileShare.Read;
        _secAttrsSav = secAttrs;
        _secAccessSav = FileIOPermissionAccess.Write;
        _modeSav = (retention != LogRetentionOption.SingleFileUnboundedSize)? FileMode.Create : FileMode.OpenOrCreate;
        _flagsAndAttributesSav = flagsAndAttributes;
        _seekToEndSav = (retention != LogRetentionOption.SingleFileUnboundedSize)? false : true;
        
        this.bufferSize = bufferSize;
        _retention = retention;
        _maxFileSize = maxFileSize;
        _maxNumberOfFiles = maxNumOfFiles;

        _Init(filePath, _fAccessSav, _shareSav, _secAttrsSav, _secAccessSav, _modeSav, _flagsAndAttributesSav, _seekToEndSav);
    }

    [System.Security.SecurityCritical]
    internal void _Init(String path, int fAccess, FileShare share, UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs, FileIOPermissionAccess secAccess, 
                        FileMode mode, int flagsAndAttributes, bool seekToEnd)
    {
        String filePath = Path.GetFullPath(path);
        _fileName = filePath;

        new FileIOPermission(secAccess, new String[] { filePath }).Demand();

        // Don't pop up a dialog for reading from an emtpy floppy drive
        int oldMode = UnsafeNativeMethods.SetErrorMode(UnsafeNativeMethods.SEM_FAILCRITICALERRORS);
        try {
            _handle = UnsafeNativeMethods.SafeCreateFile(filePath, fAccess, share, secAttrs, mode, flagsAndAttributes, UnsafeNativeMethods.NULL);
            int errorCode = Marshal.GetLastWin32Error();

            if (_handle.IsInvalid) {
                // Return a meaningful exception, using the RELATIVE path to
                // the file to avoid returning extra information to the caller
                // unless they have path discovery permission, in which case
                // the full path is fine & useful.

                // We need to give an exception, and preferably it would include
                // the fully qualified path name.  Do security check here.  If
                // we fail, give back the msgPath, which should not reveal much.
                // While this logic is largely duplicated in 
                // __Error.WinIOError, we need this for 
                // IsolatedStorageLogFileStream.
                bool canGiveFullPath = false;

                try {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new String[] { _fileName }).Demand();
                    canGiveFullPath = true;
                }
                catch(SecurityException) {}

                if (canGiveFullPath)
                    __Error.WinIOError(errorCode, _fileName);
                else
                    __Error.WinIOError(errorCode, Path.GetFileName(_fileName));
            }
        }
        finally {
            UnsafeNativeMethods.SetErrorMode(oldMode);
        }
        Debug.Assert(UnsafeNativeMethods.GetFileType(_handle) == UnsafeNativeMethods.FILE_TYPE_DISK, "did someone accidentally removed the device type check from SafeCreateFile P/Invoke wrapper?"); 

        pos = 0;

        // For Append mode...
        if (seekToEnd) {
            SeekCore(0, SeekOrigin.End);
        }
    }

    public override bool CanRead {
        [Pure]
        get { return _canRead; }
    }

    public override bool CanWrite {
        [Pure]
        get { return _canWrite; }
    }

    public override bool CanSeek {
        [Pure]
        get { return _canSeek; }
    }

    public override long Length {
        get {
            throw new NotSupportedException();
        }
    }

    public override long Position {
        get {
            throw new NotSupportedException();
        }
        set {
            throw new NotSupportedException();
        }
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin) 
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] array, int offset, int count) 
    {
        throw new NotSupportedException();
    }

    [System.Security.SecurityCritical]
    protected override unsafe void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite, out long streamPos) {
        Debug.Assert(CanWrite, "CanWrite");
        Debug.Assert(buffer != null, "buffer != null");
        Debug.Assert(offset >= 0, "offset is negative");
        Debug.Assert(count >= 0, "count is negative");
        
        int hr = 0;
        int r = WriteFileNative(buffer, offset, count, null, out hr);
        if (r == -1) {
            // For pipes, ERROR_NO_DATA is not an error, but the pipe is closing.
            if (hr == UnsafeNativeMethods.ERROR_NO_DATA) {
                r = 0;
            }
            else {
                // ERROR_INVALID_PARAMETER may be returned for writes
                // where the position is too large (ie, writing at Int64.MaxValue 
                // on Win9x) OR for synchronous writes to a handle opened 
                // asynchronously.
                if (hr == UnsafeNativeMethods.ERROR_INVALID_PARAMETER)
                    throw new IOException(SR.GetString(SR.IO_FileTooLongOrHandleNotSync));
                __Error.WinIOError(hr, String.Empty);
            }
        }
        Debug.Assert(r >= 0, "WriteCore is likely broken.");
        // update cached position 
        streamPos = AddUnderlyingStreamPosition((long)r);
        EnforceRetentionPolicy(_handle, streamPos);
        streamPos = pos;
        return;
    }

    [System.Security.SecurityCritical]
    unsafe private int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr) {
        if (_handle.IsClosed) __Error.FileNotOpen();
        
        if (_disableLogging) { 
            hr = 0;
            return 0;
        }

        Debug.Assert(offset >= 0, "offset >= 0");
        Debug.Assert(count >= 0, "count >= 0");
        Debug.Assert(bytes != null, "bytes != null");

        // Don't corrupt memory when multiple threads are erroneously writing
        // to this stream simultaneously.  (the OS is reading from
        // the array we pass to WriteFile, but if we read beyond the end and
        // that memory isn't allocated, we could get an AV.)
        if (bytes.Length - offset < count)
            throw new IndexOutOfRangeException(SR.GetString(SR.IndexOutOfRange_IORaceCondition));

        // You can't use the fixed statement on an array of length 0.
        if (bytes.Length==0) {
            hr = 0;
            return 0;
        }

        int numBytesWritten = 0;
        int r = 0;
        
        fixed(byte* p = bytes) {
            r = UnsafeNativeMethods.WriteFile(_handle, p + offset, count, out numBytesWritten, overlapped);
        }

        if (r == 0) {
            // We should never silently swallow an error here without some
            // extra work.  We must make sure that BeginWriteCore won't return an 
            // IAsyncResult that will cause EndWrite to block, since the OS won't
            // call AsyncFSCallback for us.  
            hr = Marshal.GetLastWin32Error();

            // For invalid handles, detect the error and mark our handle
            // as closed to give slightly better error messages.  Also
            // help ensure we avoid handle recycling bugs.
            if (hr == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
                _handle.SetHandleAsInvalid();

            return -1;
        }
        else
            hr = 0;
        return numBytesWritten;          
    }

    // This doesn't do argument checking.  Necessary for SetLength, which must
    // set the file pointer beyond the end of the file. This will update the 
    // internal position
    [System.Security.SecurityCritical]
    private long SeekCore(long offset, SeekOrigin origin) 
    {
        Debug.Assert(!_handle.IsClosed, "!_handle.IsClosed");
        Debug.Assert(origin>=SeekOrigin.Begin && origin<=SeekOrigin.End, "origin>=SeekOrigin.Begin && origin<=SeekOrigin.End");
        int hr = 0;
        long ret = 0;
        
        ret = UnsafeNativeMethods.SetFilePointer(_handle, offset, origin, out hr);
        if (ret == -1) {
            // For invalid handles, detect the error and mark our handle
            // as closed to give slightly better error messages.  Also
            // help ensure we avoid handle recycling bugs.
            if (hr == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
                _handle.SetHandleAsInvalid();
            __Error.WinIOError(hr, String.Empty);
        }
        
        UnderlyingStreamPosition = ret;
        return ret;
    }

    [System.Security.SecurityCritical]
    protected override void Dispose(bool disposing)
    {
        // Nothing will be done differently based on whether we are 
        // disposing vs. finalizing.  This is taking advantage of the
        // weak ordering between normal finalizable objects & critical
        // finalizable objects, which I included in the SafeHandle 
        // design for LogStream, which would often "just work" when 
        // finalized.
        try {
            if (_handle == null || _handle.IsClosed) {
                // Make sure BufferedStream doesn't try to flush data on a closed handle
                DiscardBuffer();
            }
        }
        finally {
            try {
                // Cleanup base streams
                base.Dispose(disposing);
            }
            finally {
                if (_handle != null && !_handle.IsClosed)
                    _handle.Dispose();

                _handle = null;
                _canRead = false;
                _canWrite = false;
                _canSeek = false;
            }
        }
    }

    [System.Security.SecurityCritical]
    ~LogStream()
    {
        if (_handle != null) {
            Dispose(false);
        }
    }

    [System.Security.SecurityCritical]
    private void EnforceRetentionPolicy(SafeFileHandle handle, long lastPos) 
    {
        switch (_retention) {
        case LogRetentionOption.LimitedSequentialFiles:
        case LogRetentionOption.UnlimitedSequentialFiles:
        case LogRetentionOption.LimitedCircularFiles:
            if ((lastPos >= _maxFileSize) && (handle == _handle)){
                lock (m_lockObject) {
                    if ((handle != _handle) || (lastPos < _maxFileSize))
                        return;
                    
                    _currentFileNum++;
                    if ((_retention == LogRetentionOption.LimitedCircularFiles) && (_currentFileNum > _maxNumberOfFiles)) { 
                        _currentFileNum = 1;
                    }
                    else if ((_retention == LogRetentionOption.LimitedSequentialFiles) && (_currentFileNum > _maxNumberOfFiles)) {
                        _DisableLogging();
                        return;
                    }

                    if (_fileNameWithoutExt == null) {
                        _fileNameWithoutExt = Path.Combine(Path.GetDirectoryName(_pathSav), Path.GetFileNameWithoutExtension(_pathSav));
                        _fileExt = Path.GetExtension(_pathSav);
                    }

                    string path = (_currentFileNum == 1)?_pathSav: _fileNameWithoutExt + _currentFileNum.ToString(CultureInfo.InvariantCulture) + _fileExt;
                    try {
                        _Init(path, _fAccessSav, _shareSav, _secAttrsSav, _secAccessSav, _modeSav, _flagsAndAttributesSav, _seekToEndSav);
                        
                        // Dispose the old handle and release the file write lock
                        // No need to flush the buffer as we just came off a write
                        if (handle != null && !handle.IsClosed) {
                            handle.Dispose();
                        }
                    }
                    catch (IOException ) { 
                        // Should we do this only for ERROR_SHARING_VIOLATION?
                        //if (UnsafeNativeMethods.MakeErrorCodeFromHR(Marshal.GetHRForException(ioexc)) != InternalResources.ERROR_SHARING_VIOLATION) break;
                        
                        // Possible sharing violation - ----? Let the next iteration try again
                        // For now revert the handle to the original one
                        _handle = handle;
                        
                        _retentionRetryCount++;
                        if (_retentionRetryCount >= _retentionRetryThreshold) {
                            _DisableLogging();
                        }
#if DEBUG
                        throw; 
#endif  
                    }
                    catch (UnauthorizedAccessException ) { 
                        // Indicative of ACL issues
                        _DisableLogging();
#if DEBUG
                        throw; 
#endif  
                    }
                    catch (Exception ) {
                        _DisableLogging();
#if DEBUG
                        throw; 
#endif  
                    }
                }
            }
            break;
        
        case LogRetentionOption.SingleFileBoundedSize:
            if (lastPos >= _maxFileSize) 
                _DisableLogging();
            break;

        case LogRetentionOption.SingleFileUnboundedSize:
            break;
        }
    }

    // When we enable this class widely, we need to raise an 
    // event when we disable logging due to rention policy or
    // error such as ACL that is preventing retention
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    private void _DisableLogging() 
    {
        // Discard write buffer?
        _disableLogging = true;
    }

    [System.Security.SecurityCritical]
    private static UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share)
    {
        UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = null;
        if ((share & FileShare.Inheritable) != 0) {
            secAttrs = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
            secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);

            secAttrs.bInheritHandle = 1;
        }
        return secAttrs;
    }
}
}


