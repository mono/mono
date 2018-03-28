//------------------------------------------------------------------------------
// <copyright file="SynchronizationContextUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Web;

    internal static class SynchronizationContextUtil {

        public static SynchronizationContextMode CurrentMode {
            get {
                return (!AppSettings.UseTaskFriendlySynchronizationContext) ? SynchronizationContextMode.Legacy : SynchronizationContextMode.Normal;
            }
        }

        private static string FormatErrorMessage(string specificErrorMessage, SynchronizationContextMode requiredMode) {
            // The output of this function will be a message like this:
            // "What you're trying to do is unsupported. Follow these steps to resolve it. For more
            // info, see this URL."

            string resolutionMessage;
            if (HttpRuntime.TargetFramework < VersionUtil.Framework45 && requiredMode == SynchronizationContextMode.Normal) {
                // upgrade to 4.5 mode
                resolutionMessage = SR.SynchronizationContextUtil_UpgradeToTargetFramework45Instructions;
            }
            else if (HttpRuntime.TargetFramework >= VersionUtil.Framework45 && requiredMode == SynchronizationContextMode.Legacy) {
                // already in 4.5 mode, add the back-compat switch
                resolutionMessage = SR.SynchronizationContextUtil_AddDowngradeAppSettingsSwitch;
            }
            else {
                // remove the <appSettings> switch
                resolutionMessage = SR.SynchronizationContextUtil_RemoveAppSettingsSwitch;
            }

            return SR.GetString(specificErrorMessage)
                + " " + SR.GetString(resolutionMessage)
                + "\r\n" + SR.GetString(SR.SynchronizationContextUtil_ForMoreInformation);
        }

        public static void ValidateMode(SynchronizationContextMode currentMode, SynchronizationContextMode requiredMode, string specificErrorMessage) {
            Debug.Assert(specificErrorMessage != null);
            if (currentMode != requiredMode) {
                string errorMessage = FormatErrorMessage(specificErrorMessage, requiredMode);
                throw new InvalidOperationException(errorMessage);
            }
        }

        public static void ValidateModeForAspCompat() {
            ValidateMode(
                currentMode: CurrentMode,
                requiredMode: SynchronizationContextMode.Legacy,
                specificErrorMessage: SR.SynchronizationContextUtil_AspCompatModeNotCompatible);
        }

        public static void ValidateModeForPageAsyncVoidMethods() {
            ValidateMode(
                currentMode: CurrentMode,
                requiredMode: SynchronizationContextMode.Normal,
                specificErrorMessage: SR.SynchronizationContextUtil_PageAsyncVoidMethodsNotCompatible);
        }

        public static void ValidateModeForWebSockets() {
            ValidateMode(
                currentMode: CurrentMode,
                requiredMode: SynchronizationContextMode.Normal,
                specificErrorMessage: SR.SynchronizationContextUtil_WebSocketsNotCompatible);
        }

    }
}
