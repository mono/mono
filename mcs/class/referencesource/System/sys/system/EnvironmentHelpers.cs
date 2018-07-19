using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System
{
    internal static class EnvironmentHelpers
    {
        private static volatile bool s_IsAppContainerProcess;
        private static volatile bool s_IsAppContainerProcessInitalized;

        public static bool IsAppContainerProcess {
            get {
                if(!s_IsAppContainerProcessInitalized) {
                   if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
                       s_IsAppContainerProcess = false;
                   } else if(Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor <= 1)) {
                       // Windows 7 or older.
                       s_IsAppContainerProcess = false;
                   } else {
                       s_IsAppContainerProcess = HasAppContainerToken();
                   }

                    s_IsAppContainerProcessInitalized = true;
                }

                return s_IsAppContainerProcess;
            }
        }

        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode | SecurityPermissionFlag.ControlPrincipal)]
        private static unsafe bool HasAppContainerToken() {
            int* dwIsAppContainerPtr = stackalloc int[1];
            uint dwLength = 0;

            using (WindowsIdentity wi = WindowsIdentity.GetCurrent(TokenAccessLevels.Query)) {
                if (!UnsafeNativeMethods.GetTokenInformation(wi.Token, UnsafeNativeMethods.TokenIsAppContainer, new IntPtr(dwIsAppContainerPtr), sizeof(int), out dwLength)) {
                    throw new Win32Exception();
                }
            }

            return (*dwIsAppContainerPtr != 0);
        }

        internal static bool IsWindowsVistaOrAbove()
        {
            // Method should match the logic of the internal Environment.IsWindowsVista property in mscorlib
            // If this method turns out to be heavily used we might want to cache at least part of this value.
            OperatingSystem os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT && os.Version.Major >= 6;
        }
    }
}
