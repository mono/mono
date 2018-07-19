// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Security.Permissions;

    internal static class AppSettings
    {
        private const string EnableAutomaticEndpointsCompatabilityString = "wcf:webservicehost:enableautomaticendpointscompatability";
        private const string DisableHtmlErrorPageExceptionHtmlEncodingString = "wcf:web:HtmlErrorPage:DisableExceptionMessageHtmlEncoding";
        private static readonly object appSettingsLock = new object();
        private static bool enableAutomaticEndpointCompat = false;
        private static bool disableHtmlErrorPageExceptionHtmlEncoding = false;
        private static volatile bool settingsInitialized = false;        

        public static bool EnableAutomaticEndpointsCompatibility
        {
            get
            {
                EnsureSettingsLoaded();
                return enableAutomaticEndpointCompat;
            }
        }

        public static bool DisableHtmlErrorPageExceptionHtmlEncoding
        {
            get
            {
                EnsureSettingsLoaded();
                return disableHtmlErrorPageExceptionHtmlEncoding;
            }
        }
        
        [SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104:CaughtAndHandledExceptionsRule", 
            Justification = "Handle the configuration exceptions here to avoid regressions on customer's existing scenarios")]
        private static void EnsureSettingsLoaded()
        {
            if (!settingsInitialized)
            {
                lock (appSettingsLock)
                {
                    if (!settingsInitialized)
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
                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[EnableAutomaticEndpointsCompatabilityString], out enableAutomaticEndpointCompat))
                            {
                                enableAutomaticEndpointCompat = false;
                            }

                            if ((appSettingsSection == null) || !bool.TryParse(appSettingsSection[DisableHtmlErrorPageExceptionHtmlEncodingString], out disableHtmlErrorPageExceptionHtmlEncoding))
                            {
                                disableHtmlErrorPageExceptionHtmlEncoding = false;
                            }

                            settingsInitialized = true;
                        }
                    }
                }
            }
        }
    }
}
