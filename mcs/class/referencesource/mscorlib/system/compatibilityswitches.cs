// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

using System.Runtime;
using System.Runtime.CompilerServices;

namespace System
{
    [FriendAccessAllowed]
    internal static class CompatibilitySwitches
    {
        private static bool s_AreSwitchesSet;
#if FEATURE_CORECLR && !FEATURE_CORESYSTEM
        private static bool s_isAppEarlierThanSilverlight4;
#endif //FEATURE_CORECLR && !FEATURE_CORESYSTEM

#if FEATURE_LEGACYNETCF
        private static bool s_isAppEarlierThanWindowsPhone8;
        private static bool s_isAppEarlierThanWindowsPhoneMango;
#endif //FEATURE_LEGACYNETCF

#if !FEATURE_CORECLR
        private static bool s_isNetFx40TimeSpanLegacyFormatMode;
        private static bool s_isNetFx40LegacySecurityPolicy;
        private static bool s_isNetFx45LegacyManagedDeflateStream;
#endif //!FEATURE_CORECLR

        public static bool IsCompatibilityBehaviorDefined
        {
            get
            {
                return s_AreSwitchesSet;
            }
        }

        private static bool IsCompatibilitySwitchSet(string compatibilitySwitch)
        {
            bool? result = AppDomain.CurrentDomain.IsCompatibilitySwitchSet(compatibilitySwitch);
            return (result.HasValue && result.Value);
        }

        internal static void InitializeSwitches()
        {
#if FEATURE_CORECLR && !FEATURE_CORESYSTEM
            s_isAppEarlierThanSilverlight4 = IsCompatibilitySwitchSet("APP_EARLIER_THAN_SL4.0");
#endif //FEATURE_CORECLR && !FEATURE_CORESYSTEM

#if FEATURE_LEGACYNETCF
            s_isAppEarlierThanWindowsPhoneMango = IsCompatibilitySwitchSet("WindowsPhone_3.7.0.0");
            s_isAppEarlierThanWindowsPhone8 = s_isAppEarlierThanWindowsPhoneMango || 
                                                IsCompatibilitySwitchSet("WindowsPhone_3.8.0.0"); 
                    
#endif //FEATURE_LEGACYNETCF

#if !FEATURE_CORECLR
            s_isNetFx40TimeSpanLegacyFormatMode = IsCompatibilitySwitchSet("NetFx40_TimeSpanLegacyFormatMode");
            s_isNetFx40LegacySecurityPolicy = IsCompatibilitySwitchSet("NetFx40_LegacySecurityPolicy");
            s_isNetFx45LegacyManagedDeflateStream = IsCompatibilitySwitchSet("NetFx45_LegacyManagedDeflateStream");
#endif //FEATURE_CORECLR

            s_AreSwitchesSet = true;
        }

        public static bool IsAppEarlierThanSilverlight4
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if FEATURE_CORECLR && !FEATURE_CORESYSTEM
                return s_isAppEarlierThanSilverlight4;
#else
                return false;
#endif //FEATURE_CORECLR && !FEATURE_CORESYSTEM
            }
        }

        public static bool IsAppEarlierThanWindowsPhone8
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if FEATURE_LEGACYNETCF
                return s_isAppEarlierThanWindowsPhone8;
#else
                return false;
#endif //FEATURE_LEGACYNETCF
            }
        }

        public static bool IsAppEarlierThanWindowsPhoneMango
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if FEATURE_LEGACYNETCF
                return s_isAppEarlierThanWindowsPhoneMango;
#else
                return false;
#endif //FEATURE_LEGACYNETCF
            }
        }

        public static bool IsNetFx40TimeSpanLegacyFormatMode
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if !FEATURE_CORECLR
                return s_isNetFx40TimeSpanLegacyFormatMode;
#else
                return false;
#endif //!FEATURE_CORECLR
            }
        }

        public static bool IsNetFx40LegacySecurityPolicy
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if !FEATURE_CORECLR
                return s_isNetFx40LegacySecurityPolicy;
#else
                return false;
#endif //!FEATURE_CORECLR
            }
        }

        public static bool IsNetFx45LegacyManagedDeflateStream
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get
            {
#if !FEATURE_CORECLR
                return s_isNetFx45LegacyManagedDeflateStream;
#else
                return false;
#endif //!FEATURE_CORECLR
            }
        }
    }
}
