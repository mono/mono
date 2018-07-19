//------------------------------------------------------------------------------
// <copyright file="OutputCacheProfile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.ComponentModel;
    using System.Security.Permissions;

    // class OutputCacheSettingsSection

    public sealed class OutputCacheProfile : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName;
        private static readonly ConfigurationProperty _propEnabled;
        private static readonly ConfigurationProperty _propDuration;
        private static readonly ConfigurationProperty _propLocation;
        private static readonly ConfigurationProperty _propSqlDependency;
        private static readonly ConfigurationProperty _propVaryByCustom;
        private static readonly ConfigurationProperty _propVaryByContentEncoding;
        private static readonly ConfigurationProperty _propVaryByHeader;
        private static readonly ConfigurationProperty _propVaryByParam;
        private static readonly ConfigurationProperty _propNoStore;
        private static readonly ConfigurationProperty _propVaryByControl;

        static OutputCacheProfile() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _propName =
                new ConfigurationProperty("name",
                                            typeof(string),
                                            null,
                                            StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                            StdValidatorsAndConverters.NonEmptyStringValidator,
                                            ConfigurationPropertyOptions.IsRequired | 
                                            ConfigurationPropertyOptions.IsKey);
            _propEnabled = new ConfigurationProperty("enabled", 
                                            typeof(bool), 
                                            true, 
                                            ConfigurationPropertyOptions.None);
            _propDuration = new ConfigurationProperty("duration", 
                                            typeof(int), 
                                            -1, 
                                            ConfigurationPropertyOptions.None);
            _propLocation = new ConfigurationProperty("location", 
                                            typeof(OutputCacheLocation), 
                                            (OutputCacheLocation)(-1), 
                                            ConfigurationPropertyOptions.None);
            _propSqlDependency = new ConfigurationProperty("sqlDependency", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propVaryByCustom = new ConfigurationProperty("varyByCustom", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propVaryByControl = new ConfigurationProperty("varyByControl", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propVaryByContentEncoding = new ConfigurationProperty("varyByContentEncoding", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propVaryByHeader = new ConfigurationProperty("varyByHeader", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propVaryByParam = new ConfigurationProperty("varyByParam", 
                                            typeof(string), 
                                            null, 
                                            ConfigurationPropertyOptions.None);
            _propNoStore = new ConfigurationProperty("noStore", 
                                            typeof(bool), 
                                            false, 
                                            ConfigurationPropertyOptions.None);

            _properties.Add(_propName);
            _properties.Add(_propEnabled);
            _properties.Add(_propDuration);
            _properties.Add(_propLocation);
            _properties.Add(_propSqlDependency);
            _properties.Add(_propVaryByCustom);
            _properties.Add(_propVaryByControl);
            _properties.Add(_propVaryByContentEncoding);
            _properties.Add(_propVaryByHeader);
            _properties.Add(_propVaryByParam);
            _properties.Add(_propNoStore);
        }


        internal OutputCacheProfile() {
        }

        public OutputCacheProfile(string name) {
            Name = name;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled {
            get {
                return (bool)base[_propEnabled];
            }
            set {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("duration", DefaultValue = -1)]
        public int Duration {
            get {
                return (int)base[_propDuration];
            }
            set {
                base[_propDuration] = value;
            }
        }

        [ConfigurationProperty("location")]
        public OutputCacheLocation Location {
            get {
                return (OutputCacheLocation)base[_propLocation];
            }
            set {
                base[_propLocation] = value;
            }
        }

        [ConfigurationProperty("sqlDependency")]
        public string SqlDependency {
            get {
                return (string)base[_propSqlDependency];
            }
            set {
                base[_propSqlDependency] = value;
            }
        }

        [ConfigurationProperty("varyByCustom")]
        public string VaryByCustom {
            get {
                return (string)base[_propVaryByCustom];
            }
            set {
                base[_propVaryByCustom] = value;
            }
        }

        [ConfigurationProperty("varyByControl")]
        public string VaryByControl {
            get {
                return (string)base[_propVaryByControl];
            }
            set {
                base[_propVaryByControl] = value;
            }
        }

        [ConfigurationProperty("varyByContentEncoding")]
        public string VaryByContentEncoding {
            get {
                return (string)base[_propVaryByContentEncoding];
            }
            set {
                base[_propVaryByContentEncoding] = value;
            }
        }

        [ConfigurationProperty("varyByHeader")]
        public string VaryByHeader {
            get {
                return (string)base[_propVaryByHeader];
            }
            set {
                base[_propVaryByHeader] = value;
            }
        }

        [ConfigurationProperty("varyByParam")]
        public string VaryByParam {
            get {
                return (string)base[_propVaryByParam];
            }
            set {
                base[_propVaryByParam] = value;
            }
        }

        [ConfigurationProperty("noStore", DefaultValue = false)]
        public bool NoStore {
            get {
                return (bool)base[_propNoStore];
            }
            set {
                base[_propNoStore] = value;
            }
        }
    }
}
