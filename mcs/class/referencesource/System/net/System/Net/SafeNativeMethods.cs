//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

//
// remove above #if statement if we ever need to put some
// PInvoke declaration in this bucket, until it remains empty
// it would just be waisting space to compile it.
//
namespace System.Net {
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

#if TRAVE
    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    internal struct SYSTEMTIME {
        public ushort wYear;  
        public ushort wMonth;  
        public ushort wDayOfWeek;  
        public ushort wDay;  
        public ushort wHour;  
        public ushort wMinute;  
        public ushort wSecond;  
        public ushort wMilliseconds;

        public override String ToString() {
            return wYear + "-" + wMonth + "-" + wDay + " " + wHour + ":" + wMinute + ":" + wSecond + ":" + wMilliseconds;
        }
    }
#endif

    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static class SafeNclNativeMethods {
        
#if TRAVE
        [DllImport("kernel32.dll")]
        internal static extern bool FileTimeToSystemTime([In] ref FILETIME ft, [In, Out]ref SYSTEMTIME st);
#endif
    
#if USE_WINIET_AUTODETECT_CACHE
        [DllImport("kernel32.dll")]
        internal static extern bool GetSystemTimeAsFileTime([In, Out]ref FILETIME ft);
#endif
    }; // class SafeNativeMethods


} // namespace System.Net

