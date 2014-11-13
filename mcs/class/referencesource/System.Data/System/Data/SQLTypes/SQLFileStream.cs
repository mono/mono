//------------------------------------------------------------------------------
// <copyright file="SqlFileStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Security;
using System.Runtime.Versioning;

namespace System.Data.SqlTypes 
{
    sealed public class SqlFileStream : System.IO.Stream
    {
        // NOTE: if we ever unseal this class, be sure to specify the Name, SafeFileHandle, and 
        //   TransactionContext accessors as virtual methods. Doing so now on a sealed class
        //   generates a compiler error (CS0549)

        // For BID tracing output
        private  static int     _objectTypeCount; // Bid counter
        internal readonly int   ObjectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        // from System.IO.FileStream implementation
        //   DefaultBufferSize = 4096;
        // SQLBUVSTS# 193123 - disable lazy flushing of written data in order to prevent
        //   potential exceptions during Close/Finalization. Since System.IO.FileStream will
        //   not allow for a zero byte buffer, we'll create a one byte buffer which, in normal
        //   usage, will not be used and the user buffer will automatically flush directly to 
        //   the disk cache. In pathological scenarios where the client is writing a single 
        //   byte at a time, we'll explicitly call flush ourselves.
        internal const int DefaultBufferSize = 1;
        
        private const ushort IoControlCodeFunctionCode = 2392;

        private System.IO.FileStream m_fs;
        private string m_path;
        private byte[] m_txn;
        private bool m_disposed;
        
        public SqlFileStream
            (
                string path, 
                byte[] transactionContext,
                System.IO.FileAccess access
            )
            : this ( path, transactionContext, access, System.IO.FileOptions.None, 0 )
        {
        }

        public SqlFileStream
            (
                string path, 
                byte[] transactionContext,
                System.IO.FileAccess access, 
                System.IO.FileOptions options,
                Int64 allocationSize
            )
        {

            IntPtr hscp;
            Bid.ScopeEnter ( out hscp, "<sc.SqlFileStream.ctor|API> %d# access=%d options=%d path='%ls' ", ObjectID, (int) access, (int) options, path );
            
            try
            {
                //-----------------------------------------------------------------
                // precondition validation

                if (transactionContext == null)
                    throw ADP.ArgumentNull("transactionContext");

                if ( path == null )
                    throw ADP.ArgumentNull ( "path" );

                //-----------------------------------------------------------------

                m_disposed = false;
                m_fs = null;

                OpenSqlFileStream(path, transactionContext, access, options, allocationSize);

                // only set internal state once the file has actually been successfully opened
                this.Name = path;
                this.TransactionContext = transactionContext;
            }
            finally
            {
                Bid.ScopeLeave(ref hscp);
            }
        }

        #region destructor/dispose code

        // NOTE: this destructor will only be called only if the Dispose
        //   method is not called by a client, giving the class a chance
        //   to finalize properly (i.e., free unmanaged resources)
        ~SqlFileStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (m_fs != null)
                            {
                                m_fs.Close();
                                m_fs = null;
                            }
                        }
                    }
                    finally
                    {
                        m_disposed = true;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
        
        public string Name
        {
            get
            {
                // assert that path has been properly processed via GetFullPathInternal
                //   (e.g. m_path hasn't been set directly)
                AssertPathFormat ( m_path );
                return m_path;
            }
            [ResourceExposure(ResourceScope.None)] // SxS: the file name is not exposed
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            private set
            {
                // should be validated by callers of this method
                Debug.Assert ( value != null );
                Debug.Assert ( !m_disposed );

                m_path = GetFullPathInternal ( value );
            }
        }

        public byte[] TransactionContext
        {
            get
            {
                if ( m_txn == null )
                    return null;
                
                return (byte[]) m_txn.Clone();
            }
            private set
            {
                // should be validated by callers of this method
                Debug.Assert ( value != null );
                Debug.Assert ( !m_disposed );
                
                m_txn = (byte[]) value.Clone();
            }
        }

        #region System.IO.Stream methods

        public override bool CanRead 
        {
            get
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.CanRead;
            }
        }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public override bool CanSeek 
        {
            get
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.CanSeek;
            }
        }

        [ComVisible(false)]
        public override bool CanTimeout 
        {
            get 
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.CanTimeout;
            }
        }
        
        public override bool CanWrite 
        {
            get
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.CanWrite;
            }
        }

        public override long Length 
        {
            get
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.Length;
            }
        }

        public override long Position 
        {
            get
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.Position;
            }
            set
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                m_fs.Position = value;
            }
        }

        [ComVisible(false)]
        public override int ReadTimeout 
        {
            get 
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.ReadTimeout;
            }
            set 
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                m_fs.ReadTimeout = value;
            }
        }

        [ComVisible(false)]
        public override int WriteTimeout 
        {
            get 
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                return m_fs.WriteTimeout;
            }
            set 
            {
                if ( m_disposed )
                    throw ADP.ObjectDisposed ( this );
                
                m_fs.WriteTimeout = value;
            }
        }

        public override void Flush()
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            m_fs.Flush();
        }
        
        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            return m_fs.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            return m_fs.EndRead(asyncResult);
        }

        [HostProtection(ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            IAsyncResult asyncResult = m_fs.BeginWrite(buffer, offset, count, callback, state);

            // SQLBUVSTS# 193123 - disable lazy flushing of written data in order to prevent
            //   potential exceptions during Close/Finalization. Since System.IO.FileStream will
            //   not allow for a zero byte buffer, we'll create a one byte buffer which, in normal
            //   usage, will not be used and the user buffer will automatically flush directly to 
            //   the disk cache. In pathological scenarios where the client is writing a single 
            //   byte at a time, we'll explicitly call flush ourselves.
            if ( count == 1 )
            {
                // calling flush here will mimic the internal control flow of System.IO.FileStream
                m_fs.Flush();
            }

            return asyncResult;
        }        
        
        public override void EndWrite(IAsyncResult asyncResult)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            m_fs.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            return m_fs.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            m_fs.SetLength(value);
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            return m_fs.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            return m_fs.ReadByte();
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            m_fs.Write(buffer, offset, count);

            // SQLBUVSTS# 193123 - disable lazy flushing of written data in order to prevent
            //   potential exceptions during Close/Finalization. Since System.IO.FileStream will
            //   not allow for a zero byte buffer, we'll create a one byte buffer which, in normal
            //   usage, will cause System.IO.FileStream to utilize the user-supplied buffer and
            //   automatically flush the data directly to the disk cache. In pathological scenarios 
            //   where the user is writing a single byte at a time, we'll explicitly call flush
            //   ourselves.
            if ( count == 1 )
            {
                // calling flush here will mimic the internal control flow of System.IO.FileStream
                m_fs.Flush();
            }
        }

        public override void WriteByte(byte value)
        {
            if ( m_disposed )
                throw ADP.ObjectDisposed ( this );
            
            m_fs.WriteByte(value);

            // SQLBUVSTS# 193123 - disable lazy flushing of written data in order to prevent
            //   potential exceptions during Close/Finalization. Since our internal buffer is
            //   only a single byte in length, the provided user data will always be cached.
            //   As a result, we need to be sure to flush the data to disk ourselves.

            // calling flush here will mimic the internal control flow of System.IO.FileStream
            m_fs.Flush();
        }

        #endregion

        static private readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        // path length limitations:
        // 1. path length storage (in bytes) in UNICODE_STRING is limited to UInt16.MaxValue bytes = Int16.MaxValue chars
        // 2. GetFullPathName API of kernel32 does not accept paths with length (in chars) greater than 32766
        //    (32766 is actually Int16.MaxValue - 1, while (-1) is for NULL termination)
        // We must check for the lowest value between the the two
        static private readonly int MaxWin32PathLength = Int16.MaxValue - 1;

        [ConditionalAttribute("DEBUG")]
        static private void AssertPathFormat(string path)
        {
            Debug.Assert ( path != null );
            Debug.Assert ( path == path.Trim() );
            Debug.Assert ( path.Length > 0 );
            Debug.Assert(path.Length <= MaxWin32PathLength);
            Debug.Assert(path.IndexOfAny(InvalidPathChars) < 0);
            Debug.Assert ( path.StartsWith ( @"\\", StringComparison.OrdinalIgnoreCase ) );
            Debug.Assert ( !path.StartsWith ( @"\\.\", StringComparison.Ordinal ) );
        }

        // SQLBUVSTS01 bugs 192677 and 193221: we cannot use System.IO.Path.GetFullPath for two reasons:
        // * it requires PathDiscovery permissions, which is unnecessary for SqlFileStream since we 
        //   are dealing with network path
        // * it is limited to 260 length while in our case file path can be much longer
        // To overcome the above limitations we decided to use GetFullPathName function from kernel32.dll
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        static private string GetFullPathInternal(string path)
        {
            //-----------------------------------------------------------------
            // precondition validation

            // should be validated by callers of this method
            // NOTE: if this method moves elsewhere, this assert should become an actual runtime check
            //   as the implicit assumptions here cannot be relied upon in an inter-class context
            Debug.Assert ( path != null );

            // remove leading and trailing whitespace
            path = path.Trim();
            if (path.Length == 0)
            {
                throw ADP.Argument(Res.GetString(Res.SqlFileStream_InvalidPath), "path");
            }

            // check for the path length before we normalize it with GetFullPathName
            if (path.Length > MaxWin32PathLength)
            {
                // cannot use PathTooLongException here since our length limit is 32K while
                // PathTooLongException error message states that the path should be limited to 260
                throw ADP.Argument(Res.GetString(Res.SqlFileStream_InvalidPath), "path");
            }
            
            // GetFullPathName does not check for invalid characters so we still have to validate them before
            if (path.IndexOfAny(InvalidPathChars) >= 0)
            {
                throw ADP.Argument(Res.GetString(Res.SqlFileStream_InvalidPath), "path");
            }

            // make sure path is a UNC path
            if (!path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                throw ADP.Argument(Res.GetString(Res.SqlFileStream_InvalidPath), "path");
            }

            //-----------------------------------------------------------------

            // normalize the path
            path = UnsafeNativeMethods.SafeGetFullPathName(path);

            // we do not expect windows API to return invalid paths
            Debug.Assert(path.Length <= MaxWin32PathLength, "GetFullPathName returns path longer than max expected!");

            // CONSIDER: is this a precondition validation that can be done above? Or must the path be normalized first?
            // after normalization, we have to ensure that the path does not attempt to refer to a root device, etc.
            if (path.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw ADP.Argument(Res.GetString(Res.SqlFileStream_PathNotValidDiskResource), "path");
            }

            return path;
        }

        static private void DemandAccessPermission
            (
                string path,
                System.IO.FileAccess access
            )
        {
            // ensure we demand on valid path
            AssertPathFormat ( path );

            FileIOPermissionAccess demandPermissions;
            switch (access)
            {
                case FileAccess.Read:
                    demandPermissions = FileIOPermissionAccess.Read;
                    break;

                case FileAccess.Write:
                    demandPermissions = FileIOPermissionAccess.Write;
                    break;

                case FileAccess.ReadWrite:
                default:
                    // the caller have to validate the value of 'access' parameter
                    Debug.Assert(access == System.IO.FileAccess.ReadWrite);
                    demandPermissions = FileIOPermissionAccess.Read | FileIOPermissionAccess.Write;
                    break;
            }

            FileIOPermission filePerm;
            bool pathTooLong = false;

            // check for read and/or write permissions
            try
            {
                filePerm = new FileIOPermission(demandPermissions, path);
                filePerm.Demand();                
            }
            catch (PathTooLongException e)
            {
                pathTooLong = true;
                ADP.TraceExceptionWithoutRethrow(e);
            }

            if (pathTooLong)
            {
                // SQLBUVSTS bugs 192677 and 203422: currently, FileIOPermission does not support path longer than MAX_PATH (260)
                // so we cannot demand permissions for long files. We are going to open bug for FileIOPermission to
                // support this.

                // In the meanwhile, we agreed to have try-catch block on the permission demand instead of checking the path length.
                // This way, if/when the 260-chars limitation is fixed in FileIOPermission, we will not need to change our code

                // since we do not want to relax security checks, we have to demand this permission for AllFiles in order to continue!
                // Note: demand for AllFiles will fail in scenarios where the running code does not have this permission (such as ASP.Net)
                // and the only workaround will be reducing the total path length, which means reducing the length of SqlFileStream path
                // components, such as instance name, table name, etc.. to fit into 260 characters
                filePerm = new FileIOPermission(PermissionState.Unrestricted);
                filePerm.AllFiles = demandPermissions;

                filePerm.Demand();
            }
        }

        // SxS: SQL File Stream is a database resource, not a local machine one
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void OpenSqlFileStream
            (
                string path, 
                byte[] transactionContext,
                System.IO.FileAccess access, 
                System.IO.FileOptions options,
                Int64 allocationSize
            )
        {
            //-----------------------------------------------------------------
            // precondition validation

            // these should be checked by any caller of this method

            // ensure we have validated and normalized the path before
            Debug.Assert ( path != null );
            Debug.Assert (transactionContext != null);

            if (access != FileAccess.Read && access != FileAccess.Write && access != FileAccess.ReadWrite)
                throw ADP.ArgumentOutOfRange ("access");

            // FileOptions is a set of flags, so AND the given value against the set of values we do not support
            if ( ( options & ~( FileOptions.WriteThrough | FileOptions.Asynchronous | FileOptions.RandomAccess | FileOptions.SequentialScan ) ) != 0 )
                throw ADP.ArgumentOutOfRange ( "options" );

            //-----------------------------------------------------------------

            // normalize the provided path
            //   * compress path to remove any occurences of '.' or '..'
            //   * trim whitespace from the beginning and end of the path
            //   * ensure that the path starts with '\\'
            //   * ensure that the path does not start with '\\.\'
            //   * ensure that the path is not longer than Int16.MaxValue
            path = GetFullPathInternal ( path );

            // ensure the running code has permission to read/write the file
            DemandAccessPermission(path, access);

            FileFullEaInformation eaBuffer = null;
            SecurityQualityOfService qos = null;
            UnicodeString objectName = null;

            Microsoft.Win32.SafeHandles.SafeFileHandle hFile = null;

            int nDesiredAccess = UnsafeNativeMethods.FILE_READ_ATTRIBUTES | UnsafeNativeMethods.SYNCHRONIZE;

            UInt32 dwCreateOptions = 0;
            UInt32 dwCreateDisposition = 0;

            System.IO.FileShare shareAccess = System.IO.FileShare.None;

            switch (access)
            {
                case System.IO.FileAccess.Read:
                    nDesiredAccess |= UnsafeNativeMethods.FILE_READ_DATA;
                    shareAccess = System.IO.FileShare.Delete | System.IO.FileShare.ReadWrite;
                    dwCreateDisposition = (uint) UnsafeNativeMethods.CreationDisposition.FILE_OPEN;
                    break;

                case System.IO.FileAccess.Write:
                    nDesiredAccess |= UnsafeNativeMethods.FILE_WRITE_DATA;
                    shareAccess = System.IO.FileShare.Delete | System.IO.FileShare.Read;
                    dwCreateDisposition = (uint) UnsafeNativeMethods.CreationDisposition.FILE_OVERWRITE;
                    break;

                case System.IO.FileAccess.ReadWrite:
                default:
                    // we validate the value of 'access' parameter in the beginning of this method
                    Debug.Assert(access == System.IO.FileAccess.ReadWrite);

                    nDesiredAccess |= UnsafeNativeMethods.FILE_READ_DATA | UnsafeNativeMethods.FILE_WRITE_DATA;
                    shareAccess = System.IO.FileShare.Delete | System.IO.FileShare.Read;
                    dwCreateDisposition = (uint) UnsafeNativeMethods.CreationDisposition.FILE_OVERWRITE;
                    break;
            }

            if ((options & System.IO.FileOptions.WriteThrough) != 0)
            {
                dwCreateOptions |= (uint) UnsafeNativeMethods.CreateOption.FILE_WRITE_THROUGH;
            }

            if ((options & System.IO.FileOptions.Asynchronous) == 0)
            {
                dwCreateOptions |= (uint) UnsafeNativeMethods.CreateOption.FILE_SYNCHRONOUS_IO_NONALERT;
            }

            if ((options & System.IO.FileOptions.SequentialScan) != 0)
            {
                dwCreateOptions |= (uint) UnsafeNativeMethods.CreateOption.FILE_SEQUENTIAL_ONLY;
            }

            if ( (options & System.IO.FileOptions.RandomAccess) != 0)
            {
                dwCreateOptions |= (uint) UnsafeNativeMethods.CreateOption.FILE_RANDOM_ACCESS;
            }

            try
            {
                eaBuffer = new FileFullEaInformation(transactionContext);

                qos = new SecurityQualityOfService(UnsafeNativeMethods.SecurityImpersonationLevel.SecurityAnonymous, 
                    false, false);

                // NOTE: the Name property is intended to reveal the publicly available moniker for the
                //   FILESTREAM attributed column data. We will not surface the internal processing that
                //   takes place to create the mappedPath.
                string mappedPath = InitializeNtPath(path);
                objectName = new UnicodeString(mappedPath);

                UnsafeNativeMethods.OBJECT_ATTRIBUTES oa;
                    oa.length = Marshal.SizeOf(typeof(UnsafeNativeMethods.OBJECT_ATTRIBUTES));
                oa.rootDirectory = IntPtr.Zero;
                oa.attributes = (int)UnsafeNativeMethods.Attributes.CaseInsensitive;
                oa.securityDescriptor = IntPtr.Zero;
                oa.securityQualityOfService = qos;
                oa.objectName = objectName;

                UnsafeNativeMethods.IO_STATUS_BLOCK ioStatusBlock;

                uint oldMode;
                uint retval = 0;
                
                UnsafeNativeMethods.SetErrorModeWrapper ( UnsafeNativeMethods.SEM_FAILCRITICALERRORS, out oldMode );
                try
                {
                    Bid.Trace("<sc.SqlFileStream.OpenSqlFileStream|ADV> %d#, desiredAccess=0x%08x, allocationSize=%I64d, fileAttributes=0x%08x, shareAccess=0x%08x, dwCreateDisposition=0x%08x, createOptions=0x%08x\n",
                        ObjectID, (int) nDesiredAccess, allocationSize, 0, (int) shareAccess, dwCreateDisposition, dwCreateOptions );
                    
                    retval = UnsafeNativeMethods.NtCreateFile(out hFile, nDesiredAccess, 
                        ref oa, out ioStatusBlock, ref allocationSize, 
                        0, shareAccess, dwCreateDisposition, dwCreateOptions, 
                        eaBuffer, (uint) eaBuffer.Length);
                }
                finally
                {
                    UnsafeNativeMethods.SetErrorModeWrapper( oldMode, out oldMode );
                }

                switch ( retval )
                {
                    case 0:
                        break;

                    case UnsafeNativeMethods.STATUS_SHARING_VIOLATION:
                        throw ADP.InvalidOperation ( Res.GetString ( Res.SqlFileStream_FileAlreadyInTransaction ) );

                    case UnsafeNativeMethods.STATUS_INVALID_PARAMETER:
                        throw ADP.Argument ( Res.GetString ( Res.SqlFileStream_InvalidParameter ) );

                    case UnsafeNativeMethods.STATUS_OBJECT_NAME_NOT_FOUND:
                        {
                            System.IO.DirectoryNotFoundException e = new System.IO.DirectoryNotFoundException();
                            ADP.TraceExceptionAsReturnValue ( e );
                            throw e;
                        }
                    default:
                        {
                            uint error = UnsafeNativeMethods.RtlNtStatusToDosError ( retval );
                            if ( error == UnsafeNativeMethods.ERROR_MR_MID_NOT_FOUND )
                            {
                                // status code could not be mapped to a Win32 error code 
                                error = retval;
                            }
                            
                            System.ComponentModel.Win32Exception e = new System.ComponentModel.Win32Exception ( unchecked ( (int) error ) );
                            ADP.TraceExceptionAsReturnValue ( e );
                            throw e;
                        }
                }

                if ( hFile.IsInvalid )
                {
                    System.ComponentModel.Win32Exception e = new System.ComponentModel.Win32Exception ( UnsafeNativeMethods.ERROR_INVALID_HANDLE );
                    ADP.TraceExceptionAsReturnValue ( e );
                    throw e;
                }

                UnsafeNativeMethods.FileType fileType = UnsafeNativeMethods.GetFileType(hFile);
                if (fileType != UnsafeNativeMethods.FileType.Disk) 
                {
                    hFile.Dispose();
                    throw ADP.Argument ( Res.GetString ( Res.SqlFileStream_PathNotValidDiskResource ) );
                }

                // if the user is opening the SQL FileStream in read/write mode, we assume that they want to scan
                //   through current data and then append new data to the end, so we need to tell SQL Server to preserve
                //   the existing file contents.
                if ( access == System.IO.FileAccess.ReadWrite )
                {
                    uint ioControlCode = UnsafeNativeMethods.CTL_CODE ( UnsafeNativeMethods.FILE_DEVICE_FILE_SYSTEM, 
                        IoControlCodeFunctionCode, (byte) UnsafeNativeMethods.Method.METHOD_BUFFERED, 
                        (byte) UnsafeNativeMethods.Access.FILE_ANY_ACCESS);
                    uint cbBytesReturned = 0;
                    
                    if ( !UnsafeNativeMethods.DeviceIoControl ( hFile, ioControlCode, IntPtr.Zero, 0, IntPtr.Zero, 0, out cbBytesReturned, IntPtr.Zero ) )
                    {
                        System.ComponentModel.Win32Exception e = new System.ComponentModel.Win32Exception ( Marshal.GetLastWin32Error() );
                        ADP.TraceExceptionAsReturnValue ( e );
                        throw e;
                    }
                }
                
                // now that we've successfully opened a handle on the path and verified that it is a file,
                //   use the SafeFileHandle to initialize our internal System.IO.FileStream instance
                // NOTE: need to assert UnmanagedCode permissions for this constructor. This is relatively benign
                //   in that we've done much the same validation as in the FileStream(string path, ...) ctor case
                //   most notably, validating that the handle type corresponds to an on-disk file.
                bool bRevertAssert = false;
                try
                {
                    SecurityPermission sp = new SecurityPermission ( SecurityPermissionFlag.UnmanagedCode );
                    sp.Assert();
                    bRevertAssert = true;
                    
                    System.Diagnostics.Debug.Assert ( m_fs == null );
                    m_fs = new System.IO.FileStream ( hFile, access, DefaultBufferSize, ( ( options & System.IO.FileOptions.Asynchronous ) != 0 ) );
                }
                finally
                {
                    if ( bRevertAssert )
                        SecurityPermission.RevertAssert();
                }
                
            }
            catch
            {
                if ( hFile != null && !hFile.IsInvalid )
                    hFile.Dispose();

                throw;
            }
            finally
            {
                if (eaBuffer != null)
                {
                    eaBuffer.Dispose();
                    eaBuffer = null;
                }

                if (qos != null)
                {
                    qos.Dispose();
                    qos = null;
                }

                if (objectName != null)
                {
                    objectName.Dispose();
                    objectName = null;
                }
            }
        }

        #region private helper methods

        // This method exists to ensure that the requested path name is unique so that SMB/DNS is prevented
        //   from collapsing a file open request to a file handle opened previously. In the SQL FILESTREAM case,
        //   this would likely be a file open in another transaction, so this mechanism ensures isolation.
        static private string InitializeNtPath(string path)
        {
            // ensure we have validated and normalized the path before
            AssertPathFormat ( path );

            string formatPath = @"\??\UNC\{0}\{1}";
            
            string uniqueId = Guid.NewGuid().ToString("N");
            return String.Format ( CultureInfo.InvariantCulture, formatPath, path.Trim('\\'), uniqueId);
        }

        #endregion

    }

    //-------------------------------------------------------------------------
    // UnicodeString
    //
    // Description: this class encapsulates the marshalling of data from a
    //   managed representation of the UNICODE_STRING struct into native code.
    //   As part of this task, it manages memory that is allocated in the
    //   native heap into which the managed representation is blitted. The
    //   class also implements a SafeHandle pattern to ensure that memory is
    //   not leaked in "exceptional" circumstances such as Thread.Abort().
    //
    //-------------------------------------------------------------------------
    
    internal class UnicodeString : SafeHandleZeroOrMinusOneIsInvalid
    {
        public UnicodeString(string path)
            : base (true)
        {
            Initialize(path);
        }

        // NOTE: SafeHandle's critical finalizer will call ReleaseHandle for us
        protected override bool ReleaseHandle()
        {
            if (base.handle == IntPtr.Zero)
                return true;

            Marshal.FreeHGlobal(base.handle);
            base.handle = IntPtr.Zero;
            
            return true;
        }

        private void Initialize(string path)
        {
            // pre-condition should be validated in public interface
            System.Diagnostics.Debug.Assert( path.Length <= ( UInt16.MaxValue / sizeof(char) ) );

            UnsafeNativeMethods.UNICODE_STRING objectName;
            objectName.length = (UInt16)(path.Length * sizeof(char));
            objectName.maximumLength = (UInt16)(path.Length * sizeof(char));
            objectName.buffer = path;

            IntPtr pbBuffer = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try 
            {
            } 
            finally
            {
                pbBuffer = Marshal.AllocHGlobal ( Marshal.SizeOf ( objectName ) );
                if ( pbBuffer != IntPtr.Zero )
                    SetHandle ( pbBuffer );
            }

            bool mustRelease = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                DangerousAddRef ( ref mustRelease );
                IntPtr ptr = DangerousGetHandle();
                
                Marshal.StructureToPtr ( objectName, ptr, false );                
            }
            finally
            {
                if ( mustRelease )
                    DangerousRelease();
            }
        }
    }

    //-------------------------------------------------------------------------
    // SecurityQualityOfService
    //
    // Description: this class encapsulates the marshalling of data from a
    //   managed representation of the SECURITY_QUALITY_OF_SERVICE struct into 
    //   native code. As part of this task, it pins the struct in the managed
    //   heap to ensure that it is not moved around (since the struct consists
    //   of simple types, the type does not need to be blitted into native
    //   memory). The class also implements a SafeHandle pattern to ensure that 
    //   the struct is unpinned in "exceptional" circumstances such as 
    //   Thread.Abort().
    //
    //-------------------------------------------------------------------------
    
    internal class SecurityQualityOfService : SafeHandleZeroOrMinusOneIsInvalid
    {
        UnsafeNativeMethods.SECURITY_QUALITY_OF_SERVICE m_qos;
        private GCHandle m_hQos;

        public SecurityQualityOfService
            (
                UnsafeNativeMethods.SecurityImpersonationLevel impersonationLevel,
                bool effectiveOnly,
                bool dynamicTrackingMode
            )
            : base (true)
        {
            Initialize (impersonationLevel, effectiveOnly, dynamicTrackingMode);
        }

        protected override bool ReleaseHandle()
        {
            if ( m_hQos.IsAllocated )
                m_hQos.Free();

            base.handle = IntPtr.Zero;

            return true;
        }

        internal void Initialize
            (
                UnsafeNativeMethods.SecurityImpersonationLevel impersonationLevel, 
                bool effectiveOnly, 
                bool dynamicTrackingMode
            )
        {
            m_qos.length = (uint)Marshal.SizeOf(typeof(UnsafeNativeMethods.SECURITY_QUALITY_OF_SERVICE));
            // VSTFDevDiv # 547461 [Backport SqlFileStream fix on Win7 to QFE branch]
            // Win7 enforces correct values for the _SECURITY_QUALITY_OF_SERVICE.qos member.
            m_qos.impersonationLevel = (int)impersonationLevel;

            m_qos.effectiveOnly = effectiveOnly ? (byte) 1 : (byte) 0;
            m_qos.contextDynamicTrackingMode = dynamicTrackingMode ? (byte) 1 : (byte) 0;

            IntPtr pbBuffer = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try 
            {
            }
            finally
            {
                // pin managed objects
                m_hQos = GCHandle.Alloc(m_qos, GCHandleType.Pinned);

                pbBuffer = m_hQos.AddrOfPinnedObject();
                
                if ( pbBuffer != IntPtr.Zero )
                    SetHandle ( pbBuffer );
            }
        }
    }

    //-------------------------------------------------------------------------
    // FileFullEaInformation
    //
    // Description: this class encapsulates the marshalling of data from a
    //   managed representation of the FILE_FULL_EA_INFORMATION struct into 
    //   native code. As part of this task, it manages memory that is allocated 
    //   in the native heap into which the managed representation is blitted. 
    //   The class also implements a SafeHandle pattern to ensure that memory
    //   is not leaked in "exceptional" circumstances such as Thread.Abort().
    //
    //-------------------------------------------------------------------------
    
    internal class FileFullEaInformation : SafeHandleZeroOrMinusOneIsInvalid
    {
        private string EA_NAME_STRING = "Filestream_Transaction_Tag";
        private int m_cbBuffer;

        public FileFullEaInformation(byte[] transactionContext)
            : base (true)
        {
            m_cbBuffer = 0;
            InitializeEaBuffer(transactionContext);
        }

        protected override bool ReleaseHandle()
        {
            m_cbBuffer = 0;
            
            if (base.handle == IntPtr.Zero)
                return true;

            Marshal.FreeHGlobal(base.handle);
            base.handle = IntPtr.Zero;

            return true;
        }

        public int Length
        {
            get
            {
                return m_cbBuffer;
            }
        }

        private void InitializeEaBuffer(byte[] transactionContext)
        {
            if (transactionContext.Length >= UInt16.MaxValue)
                throw ADP.ArgumentOutOfRange ( "transactionContext" );

            UnsafeNativeMethods.FILE_FULL_EA_INFORMATION eaBuffer;
            eaBuffer.nextEntryOffset = 0;
            eaBuffer.flags = 0;
            eaBuffer.EaName = 0;
            
            // string will be written as ANSI chars, so Length == ByteLength in this case
            eaBuffer.EaNameLength = (byte) EA_NAME_STRING.Length;
            eaBuffer.EaValueLength = (ushort) transactionContext.Length;

            // allocate sufficient memory to contain the FILE_FULL_EA_INFORMATION struct and
            //   the contiguous name/value pair in eaName (note: since the struct already
            //   contains one byte for eaName, we don't need to allocate a byte for the 
            //   null character separator).
            m_cbBuffer = Marshal.SizeOf(eaBuffer) + eaBuffer.EaNameLength + eaBuffer.EaValueLength;

            IntPtr pbBuffer = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try 
            {
            }
            finally
            {
                pbBuffer = Marshal.AllocHGlobal(m_cbBuffer);
                if ( pbBuffer != IntPtr.Zero )
                    SetHandle ( pbBuffer );
            }

            bool mustRelease = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                DangerousAddRef ( ref mustRelease );
                IntPtr ptr = DangerousGetHandle();
                
                // write struct into buffer
                Marshal.StructureToPtr(eaBuffer, ptr, false);

                // write property name into buffer
                System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
                byte [] asciiName = ascii.GetBytes(EA_NAME_STRING);

                // calculate offset at which to write the name/value pair
                System.Diagnostics.Debug.Assert(Marshal.OffsetOf(typeof(UnsafeNativeMethods.FILE_FULL_EA_INFORMATION), "EaName").ToInt64() <= (Int64) Int32.MaxValue);
                int cbOffset = Marshal.OffsetOf(typeof(UnsafeNativeMethods.FILE_FULL_EA_INFORMATION), "EaName").ToInt32();
                for (int i = 0; cbOffset < m_cbBuffer && i < eaBuffer.EaNameLength; i++, cbOffset++)
                {
                    Marshal.WriteByte(ptr, cbOffset, asciiName[i]);
                }

                System.Diagnostics.Debug.Assert ( cbOffset < m_cbBuffer );

                // write null character separator
                Marshal.WriteByte(ptr, cbOffset, 0);
                cbOffset++;

                System.Diagnostics.Debug.Assert ( cbOffset < m_cbBuffer || transactionContext.Length == 0 && cbOffset == m_cbBuffer );

                // write transaction context ID
                for (int i = 0; cbOffset < m_cbBuffer && i < eaBuffer.EaValueLength; i++, cbOffset++)
                {
                    Marshal.WriteByte(ptr, cbOffset, transactionContext[i]);
                }
            }
            finally
            {
                if ( mustRelease )
                    DangerousRelease();
            }
        }
    }
}
