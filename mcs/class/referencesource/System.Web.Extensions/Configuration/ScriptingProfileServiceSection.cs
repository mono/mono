//------------------------------------------------------------------------------
// <copyright file="ScriptingProfileServiceSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Configuration;

    public sealed class ScriptingProfileServiceSection : ConfigurationSection {

        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled",
                                        typeof(bool),
                                        false);
        private static readonly ConfigurationProperty _propEnableForReading =
            new ConfigurationProperty("readAccessProperties",
                                        typeof(String[]),
                                        new string[0], new System.Web.UI.WebControls.StringArrayConverter(), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableForWriting =
            new ConfigurationProperty("writeAccessProperties",
                                        typeof(String[]),
                                        new string[0], new System.Web.UI.WebControls.StringArrayConverter(), null, ConfigurationPropertyOptions.None);

        private static ConfigurationPropertyCollection _properties = BuildProperties();

        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
            properties.Add(_propEnabled);
            properties.Add(_propEnableForReading);
            properties.Add(_propEnableForWriting);
            return properties;
        }

#pragma warning disable 0436
        internal static ScriptingProfileServiceSection GetConfigurationSection() {
            return (ScriptingProfileServiceSection)WebConfigurationManager.GetWebApplicationSection("system.web.extensions/scripting/webServices/profileService");
        }
#pragma warning restore 0436

        protected override ConfigurationPropertyCollection Properties {
            get { return _properties; }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get { return (bool) this[_propEnabled]; }
            set { this[_propEnabled] = value; }
        }

        [
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Base class requires array properties"),
        ConfigurationProperty("readAccessProperties", DefaultValue = null)
        ]
        public string[] ReadAccessProperties {
            get {
                string[] propertiesForReading = (string[])this[_propEnableForReading];
                return propertiesForReading == null ? null : (string[]) propertiesForReading.Clone();
            }
            set {
                if(value != null)
                    value = (string[]) value.Clone();
                this[_propEnableForReading] = value;
            }
        }

        [
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification="Base class requires array properties"),
        ConfigurationProperty("writeAccessProperties", DefaultValue = null)
        ]
        public string[] WriteAccessProperties {
            get {
                string[] propertiesForWriting = (string[]) this[_propEnableForWriting];
                return propertiesForWriting == null ? null : (string[]) propertiesForWriting.Clone();
            }
            set {
                if(value != null)
                    value = (string[]) value.Clone();
                this[_propEnableForWriting] = value;
            }
        }
    }
}
