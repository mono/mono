//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;

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
        public static extern void GetSystemTimeAsFileTime(out long time);

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
