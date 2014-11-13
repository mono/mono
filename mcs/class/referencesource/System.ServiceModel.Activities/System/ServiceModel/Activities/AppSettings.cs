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
        // <add key="workflowServices:DefaultAutomaticInstanceKeyDisassociation" value="true"/>
        //
        // NOTE - There is a similar setting in System.Activities because the changes affected by this are in both assemblies.
        private static bool defaultAutomaticInstanceKeyDisassociation;

        internal static bool DefaultAutomaticInstanceKeyDisassociation
        {
            get
            {
                EnsureSettingsLoaded();
                return defaultAutomaticInstanceKeyDisassociation;
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

                            settingsInitialized = true;
                        }
                    }
                }
            }
        }                       
    }
}
