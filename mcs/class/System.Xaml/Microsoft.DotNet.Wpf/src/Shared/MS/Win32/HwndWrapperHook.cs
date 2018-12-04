using System;
using System.Security;
using System.Security.Permissions;

#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use FriendAccessAllowedAttribute from an unknown assembly.
using MS.Internal.YourAssemblyName;
#endif

namespace MS.Win32
{
    /// <SecurityNote>
    ///     Critical: This can be used to inject hooks into avalon
    /// </SecurityNote>
    [SecurityCritical]
    [FriendAccessAllowed]
    internal delegate IntPtr HwndWrapperHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
}
