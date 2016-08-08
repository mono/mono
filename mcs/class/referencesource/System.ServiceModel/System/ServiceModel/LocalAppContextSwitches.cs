// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel
{
    using System;
    using System.Runtime.CompilerServices;

    // When adding a quirk, name it such that false is new behavior and true is old behavior.
    // You are opting IN to old behavior. The new behavior is default.
    // For example, we want to enable the functionality to explicitly add a connection close header
    // in 4.6 and above. So we set DisableExplicitConnectionCloseHeader to true if running 4.5.2 or less.
    internal static class LocalAppContextSwitches
    {
        private const string DisableExplicitConnectionCloseHeaderString = "Switch.System.ServiceModel.DisableExplicitConnectionCloseHeader";
        private const string AllowUnsignedToHeaderString = "Switch.System.ServiceModel.AllowUnsignedToHeader";
        private const string DisableCngCertificatesString = "Switch.System.ServiceModel.DisableCngCertificates";

        private static int disableExplicitConnectionCloseHeader;
        private static int allowUnsignedToHeader;
        private static int disableCngCertificates;

        public static bool DisableExplicitConnectionCloseHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableExplicitConnectionCloseHeaderString, ref disableExplicitConnectionCloseHeader);
            }
        }

        public static bool AllowUnsignedToHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(AllowUnsignedToHeaderString, ref allowUnsignedToHeader);
            }
        }

        public static bool DisableCngCertificates
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableCngCertificatesString, ref disableCngCertificates);
            }
        }

        public static void SetDefaultsLessOrEqual_452()
        {
#pragma warning disable BCL0012            
            // Define the switches that should be true for 4.5.2 or less, false for 4.6+.
            LocalAppContext.DefineSwitchDefault(DisableExplicitConnectionCloseHeaderString, true);
#pragma warning restore BCL0012
        }

        public static void SetDefaultsLessOrEqual_461()
        {
#pragma warning disable BCL0012            
            // Define the switches that should be true for 4.6.1 or less, false for 4.6.2+.
            LocalAppContext.DefineSwitchDefault(DisableCngCertificatesString, true);
#pragma warning restore BCL0012
        }
    }
}
