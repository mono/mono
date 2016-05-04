// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  __Error
**
**
** Purpose: Centralized error methods. Used for translating 
** Win32 HRESULTs into meaningful error strings & exceptions.
**
**
===========================================================*/

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnsafeNativeMethods = Microsoft.Win32.UnsafeNativeMethods;

namespace System.IO {

    // Only static data; no need to serialize
    internal static class __Error {

        internal static void EndOfFile() {
            throw new EndOfStreamException(SR.GetString(SR.IO_EOF_ReadBeyondEOF));
        }

        internal static void FileNotOpen() {
            throw new ObjectDisposedException(null, SR.GetString(SR.ObjectDisposed_FileClosed));
        }

        internal static void PipeNotOpen() {
            throw new ObjectDisposedException(null, SR.GetString(SR.ObjectDisposed_PipeClosed));
        }

        internal static void StreamIsClosed() {
            throw new ObjectDisposedException(null, SR.GetString(SR.ObjectDisposed_StreamIsClosed));
        }

        internal static void ReadNotSupported() {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_UnreadableStream));
        }

        internal static void SeekNotSupported() {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_UnseekableStream));
        }

        internal static void WrongAsyncResult() {
            throw new ArgumentException(SR.GetString(SR.Argument_WrongAsyncResult));
        }

        internal static void EndReadCalledTwice() {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and FileStream without some work
            throw new ArgumentException(SR.GetString(SR.InvalidOperation_EndReadCalledMultiple));
        }

        internal static void EndWriteCalledTwice() {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and FileStream without some work
            throw new ArgumentException(SR.GetString(SR.InvalidOperation_EndWriteCalledMultiple));
        }

        internal static void EndWaitForConnectionCalledTwice() {
            // Should ideally be InvalidOperationExc but we can't maitain parity with Stream and FileStream without some work
            throw new ArgumentException(SR.GetString(SR.InvalidOperation_EndWaitForConnectionCalledMultiple));
        }

        /// <summary>
        /// Given a possible fully qualified path, ensure that we have path discovery permission
        /// to that path. If we do not, return just the file name. If we know it is a directory, 
        /// then don't return the directory name.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isInvalidPath"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        internal static String GetDisplayablePath(String path, bool isInvalidPath) {
            if (String.IsNullOrEmpty(path)) {
                return path;
            }

            // Is it a fully qualified path?
            bool isFullyQualified = false;
            if (path.Length < 2) {
                return path;
            }

            if ((path[0] == Path.DirectorySeparatorChar) && (path[1] == Path.DirectorySeparatorChar)) {
                isFullyQualified = true;
            }
            else if (path[1] == Path.VolumeSeparatorChar) {
                isFullyQualified = true;
            }

            if (!isFullyQualified && !isInvalidPath) {
                return path;
            }

            bool safeToReturn = false;
            try {
                if (!isInvalidPath) {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new String[] { path }).Demand();
                    safeToReturn = true;
                }
            }
            catch (SecurityException) {
            }
            catch (ArgumentException) {
                // ? and * characters cause ArgumentException to be thrown from HasIllegalCharacters
                // inside FileIOPermission.AddPathList
            }
            catch (NotSupportedException) {
                // paths like "!Bogus\\dir:with/junk_.in it" can cause NotSupportedException to be thrown
                // from Security.Util.StringExpressionSet.CanonicalizePath when ':' is found in the path
                // beyond string index position 1.  
            }

            if (!safeToReturn) {
                if ((path[path.Length - 1]) == Path.DirectorySeparatorChar) {
                    path = SR.GetString(SR.IO_IO_NoPermissionToDirectoryName);
                }
                else {
                    path = Path.GetFileName(path);
                }
            }

            return path;
        }

        [System.Security.SecurityCritical]
        internal static void WinIOError() {
            int errorCode = Marshal.GetLastWin32Error();
            WinIOError(errorCode, String.Empty);
        }

        // After calling GetLastWin32Error(), it clears the last error field, so you must save the
        // HResult and pass it to this method.  This method will determine the appropriate 
        // exception to throw dependent on your error, and depending on the error, insert a string
        // into the message gotten from the ResourceManager.
        [System.Security.SecurityCritical]
        internal static void WinIOError(int errorCode, String maybeFullPath) {

            // This doesn't have to be perfect, but is a perf optimization.
            bool isInvalidPath = errorCode == UnsafeNativeMethods.ERROR_INVALID_NAME || errorCode == UnsafeNativeMethods.ERROR_BAD_PATHNAME;
            String str = GetDisplayablePath(maybeFullPath, isInvalidPath);

            switch (errorCode) {
                case UnsafeNativeMethods.ERROR_FILE_NOT_FOUND:
                    if (str.Length == 0) {
                        throw new FileNotFoundException(SR.GetString(SR.IO_FileNotFound));
                    }
                    else {
                        throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.IO_FileNotFound_FileName), str), str);
                    }

                case UnsafeNativeMethods.ERROR_PATH_NOT_FOUND:
                    if (str.Length == 0) {
                        throw new DirectoryNotFoundException(SR.GetString(SR.IO_PathNotFound_NoPathName));
                    }
                    else {
                        throw new DirectoryNotFoundException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.IO_PathNotFound_Path), str));
                    }

                case UnsafeNativeMethods.ERROR_ACCESS_DENIED:
                    if (str.Length == 0) {
                        throw new UnauthorizedAccessException(SR.GetString(SR.UnauthorizedAccess_IODenied_NoPathName));
                    }
                    else {
                        throw new UnauthorizedAccessException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.UnauthorizedAccess_IODenied_Path), str));
                    }

                case UnsafeNativeMethods.ERROR_ALREADY_EXISTS:
                    if (str.Length == 0) {
                        goto default;
                    }
                    throw new IOException(SR.GetString(SR.IO_IO_AlreadyExists_Name, str), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

                case UnsafeNativeMethods.ERROR_FILENAME_EXCED_RANGE:
                    throw new PathTooLongException(SR.GetString(SR.IO_PathTooLong));

                case UnsafeNativeMethods.ERROR_INVALID_DRIVE:
                    throw new DriveNotFoundException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.IO_DriveNotFound_Drive), str));

                case UnsafeNativeMethods.ERROR_INVALID_PARAMETER:
                    throw new IOException(UnsafeNativeMethods.GetMessage(errorCode), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

                case UnsafeNativeMethods.ERROR_SHARING_VIOLATION:
                    if (str.Length == 0) {
                        throw new IOException(SR.GetString(SR.IO_IO_SharingViolation_NoFileName), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
                    }
                    else {
                        throw new IOException(SR.GetString(SR.IO_IO_SharingViolation_File, str), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
                    }

                case UnsafeNativeMethods.ERROR_FILE_EXISTS:
                    if (str.Length == 0) {
                        goto default;
                    }
                    throw new IOException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.IO_IO_FileExists_Name), str), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

                case UnsafeNativeMethods.ERROR_OPERATION_ABORTED:
                    throw new OperationCanceledException();

                default:
                    throw new IOException(UnsafeNativeMethods.GetMessage(errorCode), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
            }
        }

        internal static void WriteNotSupported() {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_UnwritableStream));
        }

        internal static void OperationAborted() {
            throw new IOException(SR.GetString(SR.IO_OperationAborted));
        }
    }
}
