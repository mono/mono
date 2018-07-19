//------------------------------------------------------------------------------
// <copyright file="ProfilePropertySettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Security.Permissions;

    // ProfilePropertySettingsCollection
    public sealed class ProfilePropertySettings : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        ProfilePropertyNameValidator.SingletonInstance,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propReadOnly =
            new ConfigurationProperty("readOnly",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSerializeAs =
            new ConfigurationProperty("serializeAs",
                                        typeof(SerializationMode),
                                        SerializationMode.ProviderSpecific,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviderName =
            new ConfigurationProperty("provider",
                                        typeof(string),
                                        "",
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultValue =
            new ConfigurationProperty("defaultValue",
                                        typeof(string),
                                        "",
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propType =
            new ConfigurationProperty("type",
                                        typeof(string),
                                        "string",
                                        ConfigurationPropertyOptions.IsTypeStringTransformationRequired);
        private static readonly ConfigurationProperty _propAllowAnonymous =
            new ConfigurationProperty("allowAnonymous",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCustomProviderData =
            new ConfigurationProperty("customProviderData",
                                        typeof(string),
                                        "",
                                        ConfigurationPropertyOptions.None);

        static ProfilePropertySettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propReadOnly);
            _properties.Add(_propSerializeAs);
            _properties.Add(_propProviderName);
            _properties.Add(_propDefaultValue);
            _properties.Add(_propType);
            _properties.Add(_propAllowAnonymous);
            _properties.Add(_propCustomProviderData);
        }

        private Type _type;
        private SettingsProvider _providerInternal;

        internal ProfilePropertySettings() {
        }

        public ProfilePropertySettings(string name) {
            Name = name;
        }

        public ProfilePropertySettings(string name, bool readOnly, SerializationMode serializeAs,
                                       string providerName, string defaultValue, string profileType,
                                       bool allowAnonymous, string customProviderData) {
            Name = name;
            ReadOnly = readOnly;
            SerializeAs = serializeAs;
            Provider = providerName;
            DefaultValue = defaultValue;
            Type = profileType;
            AllowAnonymous = allowAnonymous;
            CustomProviderData = customProviderData;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("readOnly", DefaultValue = false)]
        public bool ReadOnly {
            get {
                return (bool)base[_propReadOnly];
            }
            set {
                base[_propReadOnly] = value;
            }
        }

        [ConfigurationProperty("serializeAs", DefaultValue = SerializationMode.ProviderSpecific)]
        public SerializationMode SerializeAs {
            get {
                return (SerializationMode)base[_propSerializeAs];
            }
            set {
                base[_propSerializeAs] = value;
            }
        }

        [ConfigurationProperty("provider", DefaultValue = "")]
        public string Provider {
            get {
                return (string)base[_propProviderName];
            }
            set {
                base[_propProviderName] = value;
            }
        }

        internal SettingsProvider ProviderInternal {
            get {
                return _providerInternal;
            }
            set {
                _providerInternal = value;
            }
        }

        [ConfigurationProperty("defaultValue", DefaultValue = "")]
        public string DefaultValue {
            get {
                return (string)base[_propDefaultValue];
            }
            set {
                base[_propDefaultValue] = value;
            }
        }

        [ConfigurationProperty("type", DefaultValue = "string")]
        public string Type {
            get {
                return (string)base[_propType];
            }
            set {
                base[_propType] = value;
            }
        }

        internal Type TypeInternal {
            get {
                return _type;
            }
            set {
                _type = value;
            }
        }

        [ConfigurationProperty("allowAnonymous", DefaultValue = false)]
        public bool AllowAnonymous {
            get {
                return (bool)base[_propAllowAnonymous];
            }
            set {
                base[_propAllowAnonymous] = value;
            }
        }

        [ConfigurationProperty("customProviderData", DefaultValue = "")]
        public string CustomProviderData {
            get {
                return (string)base[_propCustomProviderData];
            }
            set {
                base[_propCustomProviderData] = value;
            }
        }
    } // class ProfilePropertySettings
}
