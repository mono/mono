//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Runtime.CompilerServices;

    // When adding a quirk, name it such that false is new behavior and true is old behavior.
    // You are opting IN to old behavior. The new behavior is default.
    // For example, we want to enable the functionality to explicitly add a connection close header
    // in 4.6 and above. So we set DisableExplicitConnectionCloseHeader to true if running 4.5.2 or less.
    internal static class LocalAppContextSwitches
    {
        private const string EnableCachedEmptyDefaultAuthorizationContextString = "Switch.System.IdentityModel.EnableCachedEmptyDefaultAuthorizationContext";
        private const string DisableMultipleDNSEntriesInSANCertificateString = "Switch.System.IdentityModel.DisableMultipleDNSEntriesInSANCertificate";
        private const string DisableUpdatingRsaProviderTypeString = "Switch.System.IdentityModel.DisableUpdatingRsaProviderType";

        private static int enableCachedEmptyDefaultAuthorizationContext;
        private static int disableMultipleDNSEntriesInSANCertificate;
        private static int disableUpdatingRsaProviderType;

        public static bool EnableCachedEmptyDefaultAuthorizationContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(EnableCachedEmptyDefaultAuthorizationContextString, ref enableCachedEmptyDefaultAuthorizationContext);
            }
        }

        public static bool DisableMultipleDNSEntriesInSANCertificate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableMultipleDNSEntriesInSANCertificateString, ref disableMultipleDNSEntriesInSANCertificate);
            }
        }

        public static bool DisableUpdatingRsaProviderType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableUpdatingRsaProviderTypeString, ref disableUpdatingRsaProviderType);
            }
        }

        public static void SetDefaultsLessOrEqual_452()
        {
#pragma warning disable BCL0012
            // Define the switches that should be true for 4.5.2 or less, false for 4.6+.
            LocalAppContext.DefineSwitchDefault(EnableCachedEmptyDefaultAuthorizationContextString, true);
#pragma warning restore BCL0012
        }

        public static void SetDefaultsLessOrEqual_46()
        {
#pragma warning disable BCL0012
            // Define the switches that should be true for 4.6 or less, false for 4.6.1+.
            LocalAppContext.DefineSwitchDefault(DisableMultipleDNSEntriesInSANCertificateString, true);
#pragma warning restore BCL0012
        }
    }
}
