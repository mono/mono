//------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// AppSettings.cs
//

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    internal static class AppSettings {
        private static volatile bool _settingsInitialized = false;
        private static object _appSettingsLock = new object();
        private static void EnsureSettingsLoaded() {
            if (!_settingsInitialized) {
                lock (_appSettingsLock) {
                    if (!_settingsInitialized) {
                        NameValueCollection settings = null;

                        try {
                            settings = ConfigurationManager.AppSettings;
                        }
                        catch (ConfigurationErrorsException) { }
                        finally {
                            if (settings == null || !Boolean.TryParse(settings["dataAnnotations:dataTypeAttribute:disableRegEx"], out _disableRegEx))
                                _disableRegEx = false;

                            _settingsInitialized = true;
                        }
                    }
                }
            }
        }

        private static bool _disableRegEx;
        internal static bool DisableRegEx {
            get {
                EnsureSettingsLoaded();
                return _disableRegEx;
            }
        }
    }
}
