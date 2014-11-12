using System;
using System.Globalization;
using System.Runtime.CompilerServices;

#if FEATURE_WIN_DB_APPCOMPAT
namespace System.Runtime.Versioning
{
    public static class CompatibilitySwitch
    {
        /* This class contains two sets of api:
                * 1. internal apis : These apis are supposed to be used by mscorlib.dll and other assemblies which use the <runtime> section in config 
                *                          These apis query for the value of quirk not only in windows quirk DB but also in runtime section of config files,  
                *                          registry and environment vars.
                * 2. public apis : These apis are supposed to be used by FX assemblies which do not read the runtime section of config files and have
                *                       have their own section in config files or do not use configs at all.
                *
                *     These apis are for internal use only for FX assmeblies. It has not been decided if they can be used by OOB components due to EULA restrictions
                */
        [System.Security.SecurityCritical]
        public static bool IsEnabled(string compatibilitySwitchName)
        {
            return IsEnabledInternalCall(compatibilitySwitchName, true);
        }

        [System.Security.SecurityCritical]
        public static string GetValue(string compatibilitySwitchName)
        {
            return GetValueInternalCall(compatibilitySwitchName, true);
        }

        [System.Security.SecurityCritical]
        internal static bool IsEnabledInternal(string compatibilitySwitchName)
        {
            return IsEnabledInternalCall(compatibilitySwitchName, false);
        }

        [System.Security.SecurityCritical]
        internal static string GetValueInternal(string compatibilitySwitchName)
        {
            return GetValueInternalCall(compatibilitySwitchName, false);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool IsEnabledInternalCall(string compatibilitySwitchName, bool onlyDB);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string GetValueInternalCall(string compatibilitySwitchName, bool onlyDB);

    }
}
#endif
