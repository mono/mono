// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Activation.Configuration
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    internal static class AppSettings
    {
        private const string UseClassicReadEntityBodyModeString = "wcf:serviceHostingEnvironment:useClassicReadEntityBodyMode";
        private static bool useClassicReadEntityBodyMode = false;

        private static volatile bool settingsInitalized = false;
        private static object appSettingsLock = new object();

        internal static bool UseClassicReadEntityMode
        {
            get
            {
                EnsureSettingsLoaded();
                return useClassicReadEntityBodyMode;
            }
        }

        [SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104:CaughtAndHandledExceptionsRule",
            Justification = "Handle the configuration exceptions here to avoid regressions on customer's existing scenarios")]
        private static void EnsureSettingsLoaded()
        {
            if (!settingsInitalized)
            {
                lock (appSettingsLock)
                {
                    if (!settingsInitalized)
                    {
                        NameValueCollection appSettingsSection = null;
                        try
                        {
                            appSettingsSection = ConfigurationManager.AppSettings;
                        }
                        catch (ConfigurationErrorsException)
                        {
                        }
                        finally
                        {
                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[UseClassicReadEntityBodyModeString], out useClassicReadEntityBodyMode))
                            {
                                useClassicReadEntityBodyMode = false; 
                            }

                            settingsInitalized = true;
                        }
                    }
                }
            }
        }
    }
}
