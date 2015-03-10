//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;

    internal class MachineSettingsSection : ConfigurationSection
    {
        static bool enableLoggingKnownPii;
        static bool hasInitialized = false;
        static object syncRoot = new object();

        const string enableLoggingKnownPiiKey = "enableLoggingKnownPii";
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(MachineSettingsSection.enableLoggingKnownPiiKey, typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        public static bool EnableLoggingKnownPii
        {
            get
            {
                if (!hasInitialized)
                {
                    lock (syncRoot)
                    {
                        if (!hasInitialized)
                        {
                            MachineSettingsSection machineSettingsSection = (MachineSettingsSection)ConfigurationManager.GetSection("system.serviceModel/machineSettings");
                            enableLoggingKnownPii = (bool)machineSettingsSection[MachineSettingsSection.enableLoggingKnownPiiKey];
                            hasInitialized = true;
                        }
                    }
                }

                return enableLoggingKnownPii;
            }
        }
    }
}


