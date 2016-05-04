// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;

    internal static class AppSettings
    {
        private static volatile bool settingsInitialized = false;
        private static object appSettingsLock = new object();

        // false [default] to NOT have InstanceKeys automatically disassociated by default, so that they stay around until the completion of the workflow.
        // true to have InstanceKeys automatically dissociated by default. Doing this affects the extensibility point WorkflowInstance.OnDisassociateKeys.
        // <add key="microsoft:WorkflowServices:DefaultAutomaticInstanceKeyDisassociation" value="true"/>
        //
        // NOTE - There is a similar setting in System.Activities because the changes affected by this are in both assemblies.
        private static bool defaultAutomaticInstanceKeyDisassociation;

        // The number of seconds to wait for non-protocol bookmarks if we receive a request, but the protocol bookmark for that Receive activity is not ready.
        // Default = 60 (1 minute).
        // A value of 0 implies that we should not wait at all and the "out of order" exception should be thrown, rather than the timeout exception.
        // <add key="microsoft:WorkflowServices:FilterResumeTimeoutInSeconds" value="60"/>
        private static int filterResumeTimeoutInSeconds;

        internal static bool DefaultAutomaticInstanceKeyDisassociation
        {
            get
            {
                EnsureSettingsLoaded();
                return defaultAutomaticInstanceKeyDisassociation;
            }
        }

        internal static int FilterResumeTimeoutInSeconds
        {
            get
            {
                EnsureSettingsLoaded();
                return filterResumeTimeoutInSeconds;
            }
        }

        private static void EnsureSettingsLoaded()
        {
            if (!settingsInitialized)
            {
                lock (appSettingsLock)
                {
                    if (!settingsInitialized)
                    {
                        NameValueCollection settings = null;

                        try
                        {
                            settings = ConfigurationManager.AppSettings;
                        }
                        finally
                        {
                            if (settings == null || !bool.TryParse(settings["microsoft:WorkflowServices:DefaultAutomaticInstanceKeyDisassociation"], out defaultAutomaticInstanceKeyDisassociation))
                            {
                                defaultAutomaticInstanceKeyDisassociation = false;
                            }

                            if (settings == null || !int.TryParse(settings["microsoft:WorkflowServices:FilterResumeTimeoutInSeconds"], out filterResumeTimeoutInSeconds) ||
                                (filterResumeTimeoutInSeconds < 0))
                            {
                                filterResumeTimeoutInSeconds = 60;
                            }

                            settingsInitialized = true;
                        }
                    }
                }
            }
        }
    }
}
