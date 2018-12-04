using System;
using System.Security;
using System.Runtime.InteropServices;

namespace MS.Win32
{
internal static class WinInet
{
    /// <summary>
    /// Will return the location of the internet cache folder.
    /// </summary>
    /// <returns>The location of the internet cache folder.</returns>
    /// <SecurityNote>
    /// Critical: 
    ///  1) Calls several Marshal methods which have a link demand on them.
    ///  2) Calls NativeMethods.GetUrlCacheConfigInfo which is SecurityCritical.
    /// Not Safe:
    ///  2) Returns a Path that may leak information about the system.
    /// </SecurityNote>
    internal static Uri InternetCacheFolder
    {
        [SecurityCritical]
        get
        {
            // copied value 260 from orginal implementation in BitmapDownload.cs 
            const int maxPathSize = 260;
            const UInt32 fieldControl = (UInt32)maxPathSize;

            NativeMethods.InternetCacheConfigInfo icci =
                new NativeMethods.InternetCacheConfigInfo();

            icci.CachePath = new string(new char[maxPathSize]);

            UInt32 size = (UInt32)Marshal.SizeOf(icci);
            icci.dwStructSize = size;
            
            bool passed = UnsafeNativeMethods.GetUrlCacheConfigInfo(
                ref icci,
                ref size,
                fieldControl);

            if (!passed)
            {
                int hr = Marshal.GetHRForLastWin32Error();

                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            return new Uri(icci.CachePath);
        }
    }
}
}