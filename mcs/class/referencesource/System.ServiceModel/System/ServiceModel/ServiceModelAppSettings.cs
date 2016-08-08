// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // Due to friend relationships with other assemblies, naming this class as AppSettings causes ambiguity when building those assemblies
    internal static class ServiceModelAppSettings
    {
        internal const string HttpTransportPerFactoryConnectionPoolString = "wcf:httpTransportBinding:useUniqueConnectionPoolPerFactory";
        internal const string EnsureUniquePerformanceCounterInstanceNamesString = "wcf:ensureUniquePerformanceCounterInstanceNames";
        internal const string UseConfiguredTransportSecurityHeaderLayoutString = "wcf:useConfiguredTransportSecurityHeaderLayout";
        internal const string UseBestMatchNamedPipeUriString = "wcf:useBestMatchNamedPipeUri";
        const bool DefaultHttpTransportPerFactoryConnectionPool = false;
        const bool DefaultEnsureUniquePerformanceCounterInstanceNames = false;
        const bool DefaultUseConfiguredTransportSecurityHeaderLayout = false;
        const bool DefaultUseBestMatchNamedPipeUri = false;
        static bool httpTransportPerFactoryConnectionPool;
        static bool ensureUniquePerformanceCounterInstanceNames;
        static bool useConfiguredTransportSecurityHeaderLayout;
        static bool useBestMatchNamedPipeUri;
        static volatile bool settingsInitalized = false;
        static object appSettingsLock = new object();

        internal static bool HttpTransportPerFactoryConnectionPool
        {
            get
            {
                EnsureSettingsLoaded();

                return httpTransportPerFactoryConnectionPool;
            }
        }

        internal static bool EnsureUniquePerformanceCounterInstanceNames
        {
            get
            {
                EnsureSettingsLoaded();

                return ensureUniquePerformanceCounterInstanceNames;
            }
        }

        internal static bool UseConfiguredTransportSecurityHeaderLayout
        {
            get
            {
                EnsureSettingsLoaded();

                return useConfiguredTransportSecurityHeaderLayout;
            }
        }

        internal static bool UseBestMatchNamedPipeUri
        {
            get
            {
                EnsureSettingsLoaded();

                return useBestMatchNamedPipeUri;
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
                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[HttpTransportPerFactoryConnectionPoolString], out httpTransportPerFactoryConnectionPool))
                            {
                                httpTransportPerFactoryConnectionPool = DefaultHttpTransportPerFactoryConnectionPool;
                            }

                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[EnsureUniquePerformanceCounterInstanceNamesString], out ensureUniquePerformanceCounterInstanceNames))
                            {
                                ensureUniquePerformanceCounterInstanceNames = DefaultEnsureUniquePerformanceCounterInstanceNames;
                            }

                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[UseConfiguredTransportSecurityHeaderLayoutString], out useConfiguredTransportSecurityHeaderLayout))
                            {
                                useConfiguredTransportSecurityHeaderLayout = DefaultUseConfiguredTransportSecurityHeaderLayout;
                            }

                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[UseBestMatchNamedPipeUriString], out useBestMatchNamedPipeUri))
                            {
                                useBestMatchNamedPipeUri = DefaultUseBestMatchNamedPipeUri;
                            }

                            settingsInitalized = true;
                        }
                    }
                }
            }
        }
    }
}
