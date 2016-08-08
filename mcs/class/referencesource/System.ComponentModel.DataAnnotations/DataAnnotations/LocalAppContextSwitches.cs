// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ComponentModel.DataAnnotations {
    using System;
    using System.Runtime.CompilerServices;

    // When adding a quirk, name it such that false is new behavior and true is old behavior.
    // You are opting IN to old behavior. The new behavior is default.
    // For example, we don't want to use legacy regex timeout for RegularExpressionAttribute in 4.6.1+.
    // So we set UseLegacyRegExTimeout to true if running 4.6 or less.
    internal static class LocalAppContextSwitches {
        private const string UseLegacyRegExTimeoutString = "Switch.System.ComponentModel.DataAnnotations.RegularExpressionAttribute.UseLegacyRegExTimeout";
        private static int useLegacyRegExTimeout;

        public static bool UseLegacyRegExTimeout {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return LocalAppContext.GetCachedSwitchValue(UseLegacyRegExTimeoutString, ref useLegacyRegExTimeout);
            }
        }

        public static void SetDefaultsLessOrEqual_46() {
#pragma warning disable BCL0012 //disable warning about AppContextDefaults not following the recommended pattern
            // Define the switches that should be true for 4.6 or less, false for 4.6.1+.
            LocalAppContext.DefineSwitchDefault(UseLegacyRegExTimeoutString, true);
#pragma warning restore BCL0012
        }
    }
}
