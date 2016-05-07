//------------------------------------------------------------------------------
// <copyright file="ConnectionStringsSection.cs" company="Microsoft">
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

    public sealed class ConnectionStringsSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propConnectionStrings =
            new ConfigurationProperty(null, typeof(ConnectionStringSettingsCollection), null, 
                                      ConfigurationPropertyOptions.IsDefaultCollection);

        static ConnectionStringsSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propConnectionStrings);
        }

        public ConnectionStringsSection() {
        }

        protected internal override object GetRuntimeObject() {
            SetReadOnly();
            return this;            // return the read only object
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ConnectionStringSettingsCollection ConnectionStrings {
            get {
                return (ConnectionStringSettingsCollection)base[_propConnectionStrings];
            }
        }
    }
}
