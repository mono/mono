//------------------------------------------------------------------------------
// <copyright file="EnableViewStateMacRegistryHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    // See DevDiv #461378 for a description of why we authored the EnableViewStateMac patch using this helper class.
    internal static class EnableViewStateMacRegistryHelper {

        // Returns 'true' if the EnableViewStateMac patch (DevDiv #461378) is enabled,
        // meaning that we always enforce EnableViewStateMac=true. Returns 'false' if
        // the patch hasn't been activated on this machine.
        public static readonly bool EnforceViewStateMac;

        // Returns 'true' if all MAC validation errors should be considered harmless
        // and ----ed.
        public static readonly bool SuppressMacValidationErrorsAlways;

        // Returns 'true' if we should suppress MAC validation errors from cross-page
        // postbacks.
        public static readonly bool SuppressMacValidationErrorsFromCrossPagePostbacks;

        // Returns 'true' if we should write out a __VIEWSTATEGENERATOR field alongside
        // each __VIEWSTATE field.
        public static readonly bool WriteViewStateGeneratorField;

        static EnableViewStateMacRegistryHelper() {
            // If the reg key is applied, change the default values.
            bool regKeyIsActive = IsMacEnforcementEnabledViaRegistry();
            if (regKeyIsActive) {
                EnforceViewStateMac = true;
                SuppressMacValidationErrorsFromCrossPagePostbacks = true;
            }

            // Override the defaults with what the developer specified.
            if (AppSettings.AllowInsecureDeserialization.HasValue) {
                EnforceViewStateMac = !AppSettings.AllowInsecureDeserialization.Value;

                // Exception: MAC errors from cross-page postbacks should be suppressed
                // if either the <appSettings> switch is set or the reg key is set.
                SuppressMacValidationErrorsFromCrossPagePostbacks |= !AppSettings.AllowInsecureDeserialization.Value;
            }

            SuppressMacValidationErrorsAlways = AppSettings.AlwaysIgnoreViewStateValidationErrors;
            if (SuppressMacValidationErrorsAlways) {
                // Cross-page postbacks fall under the "always" umbrella
                SuppressMacValidationErrorsFromCrossPagePostbacks = true;
            }
            else {
                // Need to write the __VIEWSTATEGENERATOR field to differentiate between cross-page
                // and same-page postback scenarios.
                if (SuppressMacValidationErrorsFromCrossPagePostbacks) {
                    WriteViewStateGeneratorField = true;
                }
            }
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        private static bool IsMacEnforcementEnabledViaRegistry() {
            try {
                string keyName = String.Format(CultureInfo.InvariantCulture, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v{0}", Environment.Version.ToString(3));
                int rawValue = (int)Registry.GetValue(keyName, "AspNetEnforceViewStateMac", defaultValue: 0 /* disabled by default */);
                return (rawValue != 0);
            }
            catch {
                // If we cannot read the registry for any reason, fail safe and assume enforcement is enabled.
                return true;
            }
        }
    }
}
