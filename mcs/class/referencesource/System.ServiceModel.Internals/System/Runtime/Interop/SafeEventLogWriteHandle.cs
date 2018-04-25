//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Diagnostics;
    using System.Security;

    [Fx.Tag.SecurityNote(Critical = "Usage of SafeHandleZeroOrMinusOneIsInvalid, which is protected by a LinkDemand and InheritanceDemand")]
    [SecurityCritical]
    sealed class SafeEventLogWriteHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Note: RegisterEventSource returns 0 on failure
        [Fx.Tag.SecurityNote(Critical = "Usage of SafeHandleZeroOrMinusOneIsInvalid, which is protected by a LinkDemand and InheritanceDemand")]
        [SecurityCritical]
        SafeEventLogWriteHandle() : base(true) { }

        [ResourceConsumption(ResourceScope.Machine)]
        [Fx.Tag.SecurityNote(Critical = "Usage of SafeHandleZeroOrMinusOneIsInvalid, which is protected by a LinkDemand and InheritanceDemand")]
        [SecurityCritical]
        public static SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName)
        {
            SafeEventLogWriteHandle retval = UnsafeNativeMethods.RegisterEventSource(uncServerName, sourceName);
            int error = Marshal.GetLastWin32Error();
            if (retval.IsInvalid)
            {
                Debug.Print("SafeEventLogWriteHandle::RegisterEventSource[" + uncServerName + ", " + sourceName + "] Failed. Last Error: " +
                    error.ToString(CultureInfo.InvariantCulture));
            }

            return retval;
        }

        [DllImport("advapi32", SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        static extern bool DeregisterEventSource(IntPtr hEventLog);

        [Fx.Tag.SecurityNote(Critical = "Usage of SafeHandleZeroOrMinusOneIsInvalid, which is protected by a LinkDemand and InheritanceDemand")]
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return DeregisterEventSource(this.handle);
        }
    }
}
