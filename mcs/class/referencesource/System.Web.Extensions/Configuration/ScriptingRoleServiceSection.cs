//------------------------------------------------------------------------------
// <copyright file="ScriptingRoleServiceSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Configuration;

    // 

    public sealed class ScriptingRoleServiceSection : ConfigurationSection {

        private static readonly ConfigurationProperty _propEnabled =
            new ConfigurationProperty("enabled",
                                        typeof(bool),
                                        false);

        private static ConfigurationPropertyCollection _properties = BuildProperties();

        private static ConfigurationPropertyCollection BuildProperties() {
            ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
            properties.Add(_propEnabled);
            return properties;
        }

        internal static ScriptingRoleServiceSection GetConfigurationSection() {
            return (ScriptingRoleServiceSection)WebConfigurationManager.GetWebApplicationSection("system.web.extensions/scripting/webServices/roleService");
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled {
            get {
                return (bool)this[_propEnabled];
            }
            set {
                this[_propEnabled] = value;
            }
        }
    }
}
