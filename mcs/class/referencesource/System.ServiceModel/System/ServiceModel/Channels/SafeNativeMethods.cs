//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        public const string KERNEL32 = "kernel32.dll";

        [DllImport(KERNEL32, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        static extern uint GetSystemTimeAdjustment(
            [Out] out int adjustment,
            [Out] out uint increment,
            [Out] out uint adjustmentDisabled
        );
        
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern void GetSystemTimeAsFileTime([Out] out FILETIME time);

        public static void GetSystemTimeAsFileTime(out long time) {
            FILETIME fileTime;
            GetSystemTimeAsFileTime(out fileTime);
            time = 0;
            time |= (uint)fileTime.dwHighDateTime;
            time <<= sizeof(uint) * 8;
            time |= (uint)fileTime.dwLowDateTime;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method GetSystemTimeAdjustment.",
            Safe = "Method is a SafeNativeMethod.")]
        [SecuritySafeCritical]
        internal static long GetSystemTimeResolution()
        {
            int dummyAdjustment;
            uint increment;
            uint dummyAdjustmentDisabled;

            if (GetSystemTimeAdjustment(out dummyAdjustment, out increment, out dummyAdjustmentDisabled) != 0)
            {
                return (long)increment;
            }

            // Assume the default, which is around 15 milliseconds.
            return 15 * TimeSpan.TicksPerMillisecond;
        }
    }
}
