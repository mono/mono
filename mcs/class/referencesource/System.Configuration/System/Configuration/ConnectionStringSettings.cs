//------------------------------------------------------------------------------
// <copyright file="ConnectionStringSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;

    public sealed class ConnectionStringSettings : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty( "name", typeof(string), null, null,
                                        ConfigurationProperty.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propConnectionString =
            new ConfigurationProperty("connectionString", typeof(string), "", ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propProviderName =
            new ConfigurationProperty("providerName", typeof(string), String.Empty, ConfigurationPropertyOptions.None);

        static ConnectionStringSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propConnectionString);
            _properties.Add(_propProviderName);
        }
        public ConnectionStringSettings() {
        }

        public ConnectionStringSettings(String name, String connectionString)
            : this() {
            Name = name;
            ConnectionString = connectionString;
            // ProviderName = (string) _propProviderName.DefaultValue;
        }

        public ConnectionStringSettings(String name, String connectionString, String providerName)
            : this() {
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        internal string Key {
            get {
                return Name;
            }
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey, DefaultValue = "")]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("connectionString", Options = ConfigurationPropertyOptions.IsRequired, DefaultValue = "")]
        public string ConnectionString {
            get {
                return (string)base[_propConnectionString];
            }
            set {
                base[_propConnectionString] = value;
            }
        }

        public override string ToString() {
            return ConnectionString;
        }

        [ConfigurationProperty("providerName", DefaultValue = "System.Data.SqlClient")]
        public string ProviderName {
            get {
                return (string)base[_propProviderName];
            }
            set {
                base[_propProviderName] = value;
            }
        }

    } // class ConnectionStringSettings
}
