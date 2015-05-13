namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Security.Permissions;

    //[SecurityCritical(SecurityCriticalScope.Everything), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
	[SecurityCritical, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLibraryHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Microsoft.Win32.UnsafeNativeMethods.FreeLibrary(base.handle);
        }
    }
}

