//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Diagnostics;
    using System.Security;
    using System.Runtime;

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
        internal static SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName)
        {
            SafeEventLogWriteHandle retval = NativeMethods.RegisterEventSource(uncServerName, sourceName);
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
        private static extern bool DeregisterEventSource(IntPtr hEventLog);

#pragma warning disable 56523
        [Fx.Tag.SecurityNote(Critical = "Usage of SafeHandleZeroOrMinusOneIsInvalid, which is protected by a LinkDemand and InheritanceDemand")]
        [SecurityCritical]
        override protected bool ReleaseHandle()
        {
            return DeregisterEventSource(this.handle);
        }
#pragma warning restore 56523

    }
}
