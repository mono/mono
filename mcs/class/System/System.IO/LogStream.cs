namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal class LogStream : BufferedStream2
    {
        private bool _canRead;
        private bool _canSeek;
        private bool _canWrite;
        private int _currentFileNum = 1;
        private bool _disableLogging;
        private int _fAccessSav;
        private string _fileExt;
        private string _fileName;
        private string _fileNameWithoutExt;
        private int _flagsAndAttributesSav;
        [SecurityCritical]
        private SafeFileHandle _handle;
        private long _maxFileSize = 0x9c4000L;
        private int _maxNumberOfFiles = 2;
        private FileMode _modeSav;
        private string _pathSav;
        private LogRetentionOption _retention;
        private int _retentionRetryCount;
        private const int _retentionRetryThreshold = 2;
        private FileIOPermissionAccess _secAccessSav;
        private Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES _secAttrsSav;
        private bool _seekToEndSav;
        private FileShare _shareSav;
        internal const long DefaultFileSize = 0x9c4000L;
        internal const int DefaultNumberOfFiles = 2;
        internal const LogRetentionOption DefaultRetention = LogRetentionOption.SingleFileUnboundedSize;
        private readonly object m_lockObject = new object();

        [SecurityCritical]
        internal LogStream(string path, int bufferSize, LogRetentionOption retention, long maxFileSize, int maxNumOfFiles)
        {
            string fullPath = Path.GetFullPath(path);
            this._fileName = fullPath;
            if (fullPath.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_IONonFileDevices"));
            }
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(FileShare.Read);
            int num = 0x100000;
            this._canWrite = true;
            this._pathSav = fullPath;
            this._fAccessSav = 0x40000000;
            this._shareSav = FileShare.Read;
            this._secAttrsSav = secAttrs;
            this._secAccessSav = FileIOPermissionAccess.Write;
            this._modeSav = (retention != LogRetentionOption.SingleFileUnboundedSize) ? FileMode.Create : FileMode.OpenOrCreate;
            this._flagsAndAttributesSav = num;
            this._seekToEndSav = retention == LogRetentionOption.SingleFileUnboundedSize;
            base.bufferSize = bufferSize;
            this._retention = retention;
            this._maxFileSize = maxFileSize;
            this._maxNumberOfFiles = maxNumOfFiles;
            this._Init(fullPath, this._fAccessSav, this._shareSav, this._secAttrsSav, this._secAccessSav, this._modeSav, this._flagsAndAttributesSav, this._seekToEndSav);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void _DisableLogging()
        {
            this._disableLogging = true;
        }

        [SecurityCritical]
        internal void _Init(string path, int fAccess, FileShare share, Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs, FileIOPermissionAccess secAccess, FileMode mode, int flagsAndAttributes, bool seekToEnd)
        {
            string fullPath = Path.GetFullPath(path);
            this._fileName = fullPath;
            new FileIOPermission(secAccess, new string[] { fullPath }).Demand();
            int newMode = Microsoft.Win32.UnsafeNativeMethods.SetErrorMode(1);
            try
            {
                this._handle = Microsoft.Win32.UnsafeNativeMethods.SafeCreateFile(fullPath, fAccess, share, secAttrs, mode, flagsAndAttributes, Microsoft.Win32.UnsafeNativeMethods.NULL);
                int errorCode = Marshal.GetLastWin32Error();
                if (this._handle.IsInvalid)
                {
                    bool flag = false;
                    try
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { this._fileName }).Demand();
                        flag = true;
                    }
                    catch (SecurityException)
                    {
                    }
                    if (flag)
                    {
                        System.IO.__Error.WinIOError(errorCode, this._fileName);
                    }
                    else
                    {
                        System.IO.__Error.WinIOError(errorCode, Path.GetFileName(this._fileName));
                    }
                }
            }
            finally
            {
                Microsoft.Win32.UnsafeNativeMethods.SetErrorMode(newMode);
            }
            base.pos = 0L;
            if (seekToEnd)
            {
                this.SeekCore(0L, SeekOrigin.End);
            }
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((this._handle == null) || this._handle.IsClosed)
                {
                    base.DiscardBuffer();
                }
            }
            finally
            {
                try
                {
                    base.Dispose(disposing);
                }
                finally
                {
                    if ((this._handle != null) && !this._handle.IsClosed)
                    {
                        this._handle.Dispose();
                    }
                    this._handle = null;
                    this._canRead = false;
                    this._canWrite = false;
                    this._canSeek = false;
                }
            }
        }

        [SecurityCritical]
        private void EnforceRetentionPolicy(SafeFileHandle handle, long lastPos)
        {
            switch (this._retention)
            {
                case LogRetentionOption.UnlimitedSequentialFiles:
                case LogRetentionOption.LimitedCircularFiles:
                case LogRetentionOption.LimitedSequentialFiles:
                    if ((lastPos < this._maxFileSize) || (handle != this._handle))
                    {
                        return;
                    }
                    lock (this.m_lockObject)
                    {
                        if ((handle == this._handle) && (lastPos >= this._maxFileSize))
                        {
                            this._currentFileNum++;
                            if ((this._retention == LogRetentionOption.LimitedCircularFiles) && (this._currentFileNum > this._maxNumberOfFiles))
                            {
                                this._currentFileNum = 1;
                            }
                            else if ((this._retention == LogRetentionOption.LimitedSequentialFiles) && (this._currentFileNum > this._maxNumberOfFiles))
                            {
                                this._DisableLogging();
                                return;
                            }
                            if (this._fileNameWithoutExt == null)
                            {
                                this._fileNameWithoutExt = Path.Combine(Path.GetDirectoryName(this._pathSav), Path.GetFileNameWithoutExtension(this._pathSav));
                                this._fileExt = Path.GetExtension(this._pathSav);
                            }
                            string path = (this._currentFileNum == 1) ? this._pathSav : (this._fileNameWithoutExt + this._currentFileNum.ToString(CultureInfo.InvariantCulture) + this._fileExt);
                            try
                            {
                                this._Init(path, this._fAccessSav, this._shareSav, this._secAttrsSav, this._secAccessSav, this._modeSav, this._flagsAndAttributesSav, this._seekToEndSav);
                                if ((handle != null) && !handle.IsClosed)
                                {
                                    handle.Dispose();
                                }
                            }
                            catch (IOException)
                            {
                                this._handle = handle;
                                this._retentionRetryCount++;
                                if (this._retentionRetryCount >= 2)
                                {
                                    this._DisableLogging();
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                this._DisableLogging();
                            }
                            catch (Exception)
                            {
                                this._DisableLogging();
                            }
                        }
                        return;
                    }
                    /*break; */

                case LogRetentionOption.SingleFileUnboundedSize:
                    return;

                case LogRetentionOption.SingleFileBoundedSize:
                    break;

                default:
                    return;
            }
            if (lastPos >= this._maxFileSize)
            {
                this._DisableLogging();
            }
        }

        [SecurityCritical]
        ~LogStream()
        {
            if (this._handle != null)
            {
                this.Dispose(false);
            }
        }

        [SecurityCritical]
        private static Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share)
        {
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES structure = null;
            if ((share & FileShare.Inheritable) != FileShare.None)
            {
                structure = new Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure),
                    bInheritHandle = 1
                };
            }
            return structure;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        [SecurityCritical]
        private long SeekCore(long offset, SeekOrigin origin)
        {
            int hr = 0;
            long num2 = 0L;
            num2 = Microsoft.Win32.UnsafeNativeMethods.SetFilePointer(this._handle, offset, origin, out hr);
            if (num2 == -1L)
            {
                if (hr == 6)
                {
                    this._handle.SetHandleAsInvalid();
                }
                System.IO.__Error.WinIOError(hr, string.Empty);
            }
            base.UnderlyingStreamPosition = num2;
            return num2;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        [SecurityCritical]
		protected override void WriteCore (byte[] buffer, int offset, int count, bool blockForWrite, out long streamPos)
		{
			int hr = 0;
			int num2 = 0;
			streamPos = 0;
			unsafe {
				this.WriteFileNative (buffer, offset, count, null, out hr);
			}
            if (num2 == -1)
            {
                switch (hr)
                {
                    case 0xe8:
                        num2 = 0;
                        goto Label_0040;

                    case 0x57:
                        throw new IOException(System.SR.GetString("IO_FileTooLongOrHandleNotSync"));
                }
                System.IO.__Error.WinIOError(hr, string.Empty);
            }
        Label_0040:
            streamPos = base.AddUnderlyingStreamPosition((long) num2);
            this.EnforceRetentionPolicy(this._handle, streamPos);
            streamPos = base.pos;
        }

        [SecurityCritical]
        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if (this._handle.IsClosed)
            {
                System.IO.__Error.FileNotOpen();
            }
            if (this._disableLogging)
            {
                hr = 0;
                return 0;
            }
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(System.SR.GetString("IndexOutOfRange_IORaceCondition"));
            }
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int numBytesWritten = 0;
            int num2 = 0;
            fixed (byte* numRef = bytes)
            {
                num2 = Microsoft.Win32.UnsafeNativeMethods.WriteFile(this._handle, numRef + offset, count, out numBytesWritten, overlapped);
            }
            if (num2 == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == 6)
                {
                    this._handle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesWritten;
        }

        public override bool CanRead
        {
            get
            {
                return this._canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._canWrite;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

