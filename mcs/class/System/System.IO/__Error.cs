namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class __Error
    {
        internal static void EndOfFile()
        {
            throw new EndOfStreamException(System.SR.GetString("IO_EOF_ReadBeyondEOF"));
        }

        internal static void EndReadCalledTwice()
        {
            throw new ArgumentException(System.SR.GetString("InvalidOperation_EndReadCalledMultiple"));
        }

        internal static void EndWaitForConnectionCalledTwice()
        {
            throw new ArgumentException(System.SR.GetString("InvalidOperation_EndWaitForConnectionCalledMultiple"));
        }

        internal static void EndWriteCalledTwice()
        {
            throw new ArgumentException(System.SR.GetString("InvalidOperation_EndWriteCalledMultiple"));
        }

        internal static void FileNotOpen()
        {
            throw new ObjectDisposedException(null, System.SR.GetString("ObjectDisposed_FileClosed"));
        }

        [SecuritySafeCritical]
        internal static string GetDisplayablePath(string path, bool isInvalidPath)
        {
            if (!string.IsNullOrEmpty(path))
            {
                bool flag = false;
                if (path.Length < 2)
                {
                    return path;
                }
                if ((path[0] == Path.DirectorySeparatorChar) && (path[1] == Path.DirectorySeparatorChar))
                {
                    flag = true;
                }
                else if (path[1] == Path.VolumeSeparatorChar)
                {
                    flag = true;
                }
                if (!flag && !isInvalidPath)
                {
                    return path;
                }
                bool flag2 = false;
                try
                {
                    if (!isInvalidPath)
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { path }).Demand();
                        flag2 = true;
                    }
                }
                catch (SecurityException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
                if (flag2)
                {
                    return path;
                }
                if (path[path.Length - 1] == Path.DirectorySeparatorChar)
                {
                    path = System.SR.GetString("IO_IO_NoPermissionToDirectoryName");
                    return path;
                }
                path = Path.GetFileName(path);
            }
            return path;
        }

        internal static void PipeNotOpen()
        {
            throw new ObjectDisposedException(null, System.SR.GetString("ObjectDisposed_PipeClosed"));
        }

        internal static void ReadNotSupported()
        {
            throw new NotSupportedException(System.SR.GetString("NotSupported_UnreadableStream"));
        }

        internal static void SeekNotSupported()
        {
            throw new NotSupportedException(System.SR.GetString("NotSupported_UnseekableStream"));
        }

        internal static void StreamIsClosed()
        {
            throw new ObjectDisposedException(null, System.SR.GetString("ObjectDisposed_StreamIsClosed"));
        }

        [SecurityCritical]
        internal static void WinIOError()
        {
            WinIOError(Marshal.GetLastWin32Error(), string.Empty);
        }

        [SecurityCritical]
        internal static void WinIOError(int errorCode, string maybeFullPath)
        {
            bool isInvalidPath = (errorCode == 0x7b) || (errorCode == 0xa1);
            string displayablePath = GetDisplayablePath(maybeFullPath, isInvalidPath);
            switch (errorCode)
            {
                case 0x20:
                    if (displayablePath.Length == 0)
                    {
                        throw new IOException(System.SR.GetString("IO_IO_SharingViolation_NoFileName"), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
                    }
                    throw new IOException(System.SR.GetString("IO_IO_SharingViolation_File", new object[] { displayablePath }), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

                case 80:
                    if (displayablePath.Length != 0)
                    {
                        throw new IOException(string.Format(CultureInfo.CurrentCulture, System.SR.GetString("IO_IO_FileExists_Name"), new object[] { displayablePath }), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
                    }
                    break;

                case 2:
                    if (displayablePath.Length == 0)
                    {
                        throw new FileNotFoundException(System.SR.GetString("IO_FileNotFound"));
                    }
                    throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, System.SR.GetString("IO_FileNotFound_FileName"), new object[] { displayablePath }), displayablePath);

                case 3:
                    if (displayablePath.Length == 0)
                    {
                        throw new DirectoryNotFoundException(System.SR.GetString("IO_PathNotFound_NoPathName"));
                    }
                    throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, System.SR.GetString("IO_PathNotFound_Path"), new object[] { displayablePath }));

                case 5:
                    if (displayablePath.Length == 0)
                    {
                        throw new UnauthorizedAccessException(System.SR.GetString("UnauthorizedAccess_IODenied_NoPathName"));
                    }
                    throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, System.SR.GetString("UnauthorizedAccess_IODenied_Path"), new object[] { displayablePath }));

                case 15:
                    throw new DriveNotFoundException(string.Format(CultureInfo.CurrentCulture, System.SR.GetString("IO_DriveNotFound_Drive"), new object[] { displayablePath }));

                case 0x57:
                    throw new IOException(Microsoft.Win32.UnsafeNativeMethods.GetMessage(errorCode), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

                case 0xb7:
                    if (displayablePath.Length != 0)
                    {
                        throw new IOException(System.SR.GetString("IO_IO_AlreadyExists_Name", new object[] { displayablePath }), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
                    }
                    break;

                case 0xce:
                    throw new PathTooLongException(System.SR.GetString("IO_PathTooLong"));

                case 0x3e3:
                    throw new OperationCanceledException();
            }
            throw new IOException(Microsoft.Win32.UnsafeNativeMethods.GetMessage(errorCode), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
        }

        internal static void WriteNotSupported()
        {
            throw new NotSupportedException(System.SR.GetString("NotSupported_UnwritableStream"));
        }

        internal static void WrongAsyncResult()
        {
            throw new ArgumentException(System.SR.GetString("Argument_WrongAsyncResult"));
        }
    }
}

