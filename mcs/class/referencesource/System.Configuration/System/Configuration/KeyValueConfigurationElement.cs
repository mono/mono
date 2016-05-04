//------------------------------------------------------------------------------
// <copyright file="KeyValueConfigurationElement.cs" company="Microsoft">
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

    public class KeyValueConfigurationElement : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propKey =
            new ConfigurationProperty("key", typeof(string), String.Empty, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propValue =
            new ConfigurationProperty("value", typeof(string), String.Empty, ConfigurationPropertyOptions.None);

        static KeyValueConfigurationElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propKey);
            _properties.Add(_propValue);
        }

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        bool _needsInit;
        string _initKey;
        string _initValue;

        //
        // Constructor
        //
        internal KeyValueConfigurationElement() {
        }

        public KeyValueConfigurationElement(string key, string value) {
            _needsInit = true;
            _initKey = key;
            _initValue = value;
        }

        protected internal override void Init() {
            base.Init();

            // We cannot initialize configuration properties in the constructor,
            // because Properties is an overridable virtual property that 
            // hence may not be available in the constructor.
            if (_needsInit) {
                _needsInit = false;
                base[_propKey] = _initKey;
                Value = _initValue;
            }
        }

        //
        // Properties
        //

        [ConfigurationProperty("key", Options = ConfigurationPropertyOptions.IsKey, DefaultValue = "")]
        public string Key {
            get {
                return (string)base[_propKey];
            }
        }

        [ConfigurationProperty("value", DefaultValue = "")]
        public string Value {
            get {
                return (string)base[_propValue];
            }
            set {
                base[_propValue] = value;
            }
        }
    }
}
