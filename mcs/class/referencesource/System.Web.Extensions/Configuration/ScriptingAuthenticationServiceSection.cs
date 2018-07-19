//------------------------------------------------------------------------------
// <copyright file="ScriptingAuthenticationServiceSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;

    public sealed class ScriptingAuthenticationServiceSection : ConfigurationSection {

        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled",
                                        typeof(bool),
                                        false);

        private static readonly ConfigurationProperty _propRequireSSL =
            new ConfigurationProperty("requireSSL",
                                        typeof(bool),
                                        false);

        private static ConfigurationPropertyCollection _properties = BuildProperties();

        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
            properties.Add(_propEnabled);
            properties.Add(_propRequireSSL);
            return properties;
        }

        internal static ScriptingAuthenticationServiceSection GetConfigurationSection() {
            return (ScriptingAuthenticationServiceSection)WebConfigurationManager.GetWebApplicationSection("system.web.extensions/scripting/webServices/authenticationService");
        }

        protected override ConfigurationPropertyCollection Properties {
            get { return _properties; }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get { return (bool)this[_propEnabled]; }
            set { this[_propEnabled] = value; }
        }

        [ConfigurationProperty("requireSSL", DefaultValue = false)]
        public bool RequireSSL {
            get { return (bool) this[_propRequireSSL]; }
            set { this[_propRequireSSL] = value; }
        }
    }
}
