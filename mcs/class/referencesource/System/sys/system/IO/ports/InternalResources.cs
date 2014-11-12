// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  InternalResources
**
** Date:  August 2002
**
===========================================================*/

using System.IO;
using System.Security;
using System.Text;
using Marshal=System.Runtime.InteropServices.Marshal;
using Microsoft.Win32;

namespace System.IO.Ports
{
    internal static class InternalResources
    {
        // Beginning of static Error methods
#if !FEATURE_NETCORE
        internal static void EndOfFile() 
        {
            throw new EndOfStreamException(SR.GetString(SR.IO_EOF_ReadBeyondEOF));
        }
#endif
    
#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        internal static String GetMessage(int errorCode) 
        {
            StringBuilder sb = new StringBuilder(512);
            int result = SafeNativeMethods.FormatMessage(NativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS |
                NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM | NativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                IntPtr.Zero, (uint) errorCode, 0, sb, sb.Capacity, null);
            if (result != 0) 
            {
                // result is the # of characters copied to the StringBuilder on NT,
                // but on Win9x, it appears to be the number of MBCS bytes.
                // Just give up and return the String as-is...
                String s = sb.ToString();
                return s;
            }
            else 
            {
                return SR.GetString(SR.IO_UnknownError, errorCode);
            }
        }

#if !FEATURE_NETCORE
        internal static void FileNotOpen() 
        {
            throw new ObjectDisposedException(null, SR.GetString(SR.Port_not_open));
        }

        internal static void WrongAsyncResult() 
        {
            throw new ArgumentException(SR.GetString(SR.Arg_WrongAsyncResult));
        }

        internal static void EndReadCalledTwice() 
        {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and SerialStream without some work
            throw new ArgumentException(SR.GetString(SR.InvalidOperation_EndReadCalledMultiple));
        }

        internal static void EndWriteCalledTwice() 
        {
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and SerialStream without some work
            throw new ArgumentException(SR.GetString(SR.InvalidOperation_EndWriteCalledMultiple));
        }
#endif

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        internal static void WinIOError() 
        {
            int errorCode = Marshal.GetLastWin32Error();
            WinIOError(errorCode, String.Empty);
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        internal static void WinIOError(string str) 
        {
            int errorCode = Marshal.GetLastWin32Error();
            WinIOError(errorCode, str);
        }
        
        // After calling GetLastWin32Error(), it clears the last error field,
        // so you must save the HResult and pass it to this method.  This method
        // will determine the appropriate exception to throw dependent on your 
        // error, and depending on the error, insert a string into the message 
        // gotten from the ResourceManager.
        internal static void WinIOError(int errorCode, String str) 
        {
            switch (errorCode) 
            {
                case NativeMethods.ERROR_FILE_NOT_FOUND:
                case NativeMethods.ERROR_PATH_NOT_FOUND:
                    if (str.Length == 0)
                        throw new IOException(SR.GetString(SR.IO_PortNotFound));
                    else
                        throw new IOException(SR.GetString(SR.IO_PortNotFoundFileName, str));
                
                case NativeMethods.ERROR_ACCESS_DENIED:
                    if (str.Length == 0)
                        throw new UnauthorizedAccessException(SR.GetString(SR.UnauthorizedAccess_IODenied_NoPathName));
                    else
                        throw new UnauthorizedAccessException(SR.GetString(SR.UnauthorizedAccess_IODenied_Path, str));

                case NativeMethods.ERROR_FILENAME_EXCED_RANGE:
                    throw new PathTooLongException(SR.GetString(SR.IO_PathTooLong));

                case NativeMethods.ERROR_SHARING_VIOLATION:
                    // error message.
                    if (str.Length == 0)
                        throw new IOException(SR.GetString(SR.IO_SharingViolation_NoFileName));
                    else
                        throw new IOException(SR.GetString(SR.IO_SharingViolation_File, str));

                default:
                    throw new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode));
            }
        }
    
        // Use this to translate error codes like the above into HRESULTs like
        // 0x80070006 for ERROR_INVALID_HANDLE
        internal static int MakeHRFromErrorCode(int errorCode)
        {
            return unchecked(((int)0x80070000) | errorCode);
        }

    }
}
    
    
