//------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    internal static class AppSettings {
        private const string UseLegacySerializerGenerationAppSettingsString = "System:Xml:Serialization:UseLegacySerializerGeneration";
        private static bool? useLegacySerializerGeneration;
        private static volatile bool settingsInitalized = false;
        private static object appSettingsLock = new object();

        internal static bool? UseLegacySerializerGeneration {
            get {
                EnsureSettingsLoaded();
                return useLegacySerializerGeneration;
            }
        }

        static void EnsureSettingsLoaded() {
            if (!settingsInitalized) {
                lock (appSettingsLock) {
                    if (!settingsInitalized) {
                        NameValueCollection appSettingsSection = null;
                        try {
                            appSettingsSection = ConfigurationManager.AppSettings;
                        }
                        catch (ConfigurationErrorsException) {
                        }
                        finally {
                            bool tempUseLegacySerializerGeneration;
                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[UseLegacySerializerGenerationAppSettingsString], out tempUseLegacySerializerGeneration)) {
                                useLegacySerializerGeneration = null;
                            } 
                            else {
                                useLegacySerializerGeneration = (bool?)tempUseLegacySerializerGeneration;
                            }

                            settingsInitalized = true;
                        }
                    }
                }
            }
        }
    }
}
