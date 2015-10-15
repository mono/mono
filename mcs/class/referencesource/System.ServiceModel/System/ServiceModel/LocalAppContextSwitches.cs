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
        private static int disableExplicitConnectionCloseHeader;

        public static bool DisableExplicitConnectionCloseHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableExplicitConnectionCloseHeaderString, ref disableExplicitConnectionCloseHeader);
            }
        }

        public static void SetDefaultsLessOrEqual_452()
        {
            // Define the switches that should be true for 4.5.2 or less, false for 4.6+.
            LocalAppContext.DefineSwitchDefault(DisableExplicitConnectionCloseHeaderString, true);
        }
    }
}
