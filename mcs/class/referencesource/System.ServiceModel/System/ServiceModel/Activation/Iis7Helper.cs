//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Win32;

    static class Iis7Helper
    {
        static int iisVersion;
        static bool isIis7;

        static Iis7Helper()
        {
            isIis7 = GetIsIis7();
        }

        internal static int IisVersion
        {
            get { return iisVersion; }
        }

        internal static bool IsIis7
        {
            get { return isIis7; }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method to get version info from registry.",
            Safe = "Processes registry info into a safe bool for return.")]
        [SecuritySafeCritical]
        static bool GetIsIis7()
        {
            iisVersion = -1;
            object majorVersion = UnsafeGetMajorVersionFromRegistry();
            if (majorVersion != null && majorVersion.GetType().Equals(typeof(int)))
            {
                iisVersion = (int)majorVersion;
            }
            return iisVersion >= 7;
        }

        const string subKey = @"Software\Microsoft\InetSTP";

        [Fx.Tag.SecurityNote(Critical = "Asserts registry access to get a single value from the registry, caller should not leak value.")]
        [SecurityCritical]
        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + subKey)]
        static object UnsafeGetMajorVersionFromRegistry()
        {
            using (RegistryKey localMachine = Registry.LocalMachine)
            {
                using (RegistryKey versionKey = localMachine.OpenSubKey(subKey))
                {
                    return versionKey != null ? versionKey.GetValue("MajorVersion") : null;
                }
            }
        }
    }
}
