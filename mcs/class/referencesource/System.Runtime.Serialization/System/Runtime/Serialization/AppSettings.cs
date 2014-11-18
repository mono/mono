// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    static class AppSettings
    {
        internal const string MaxMimePartsAppSettingsString = "microsoft:xmldictionaryreader:maxmimeparts";
        const int DefaultMaxMimeParts = 1000;
        static int maxMimeParts;
        static volatile bool settingsInitalized = false;
        static object appSettingsLock = new object();

        internal static int MaxMimeParts
        {
            get
            {
                EnsureSettingsLoaded();

                return maxMimeParts;
            }
        }

        [SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104:CaughtAndHandledExceptionsRule", 
            Justification = "Handle the configuration exceptions here to avoid regressions on customer's existing scenarios")]
        static void EnsureSettingsLoaded()
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
                            if ((appSettingsSection == null) || !int.TryParse(appSettingsSection[MaxMimePartsAppSettingsString], out maxMimeParts))
                            {
                                maxMimeParts = DefaultMaxMimeParts;
                            }

                            settingsInitalized = true;
                        }
                    }
                }
            }
        }
    }
}
