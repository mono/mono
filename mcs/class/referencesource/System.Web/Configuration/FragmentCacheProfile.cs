//------------------------------------------------------------------------------
// <copyright file="FragmentCacheProfile.cs" company="Microsoft">
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

    // OutputCacheProfileCollection

#if NOT_UNTIL_LATER
    public sealed class FragmentCacheProfile : ConfigurationElement {
        private const int ONE_YEAR_DURATION = 31536000;       // One year in seconds

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName;
        private static readonly ConfigurationProperty _propEnabled;
        private static readonly ConfigurationProperty _propDuration;
        private static readonly ConfigurationProperty _propLocation;
        private static readonly ConfigurationProperty _propShared;
        private static readonly ConfigurationProperty _propProvider;
        private static readonly ConfigurationProperty _propSqlDependency;
        private static readonly ConfigurationProperty _propVaryByCustom;
        private static readonly ConfigurationProperty _propVaryByContentEncoding;
        private static readonly ConfigurationProperty _propVaryByHeader;
        private static readonly ConfigurationProperty _propVaryByParam;

        static FragmentCacheProfile() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();

            _propName =
                new ConfigurationProperty("name",
                                            typeof(string),
                                            null,
                                            null,
                                            StdValidatorsAndConverters.NonEmptyStringValidator,
                                            ConfigurationPropertyOptions.IsRequired);
            _propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
            _propDuration = new ConfigurationProperty("duration", typeof(int), ONE_YEAR_DURATION, ConfigurationPropertyOptions.None);
            _propLocation = new ConfigurationProperty("location", typeof(OutputCacheLocation), -1, ConfigurationPropertyOptions.None);
            _propShared = new ConfigurationProperty("shared", typeof(bool), false, ConfigurationPropertyOptions.None);
            _propProvider = new ConfigurationProperty("provider", typeof(string), null, ConfigurationPropertyOptions.None);
            _propSqlDependency = new ConfigurationProperty("sqlDependency", typeof(string), null, ConfigurationPropertyOptions.None);
            _propVaryByCustom = new ConfigurationProperty("varyByCustom", typeof(string), null, ConfigurationPropertyOptions.None);
            _propVaryByContentEncoding = new ConfigurationProperty("varyByContentEncoding", typeof(string), null, ConfigurationPropertyOptions.None);
            _propVaryByHeader = new ConfigurationProperty("varyByHeader", typeof(string), null, ConfigurationPropertyOptions.None);
            _propVaryByParam = new ConfigurationProperty("varyByParam", typeof(string), null, ConfigurationPropertyOptions.None);

            _properties.Add(_propName);
            _properties.Add(_propEnabled);
            _properties.Add(_propDuration);
            _properties.Add(_propLocation);
            _properties.Add(_propShared);
            _properties.Add(_propProvider);
            _properties.Add(_propSqlDependency);
            _properties.Add(_propVaryByCustom);
            _properties.Add(_propVaryByContentEncoding);
            _properties.Add(_propVaryByHeader);
            _properties.Add(_propVaryByParam);
        }


        internal FragmentCacheProfile() {
        }

        public FragmentCacheProfile(string name) {
            Name = name;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, DefaultValue = "")]
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

        [ConfigurationProperty("duration", DefaultValue = ONE_YEAR_DURATION)]
        public int Duration {
            get {
                return (int)base[_propDuration];
            }
            set {
                base[_propDuration] = value;
            }
        }

        [ConfigurationProperty("location", DefaultValue = -1)]
        public OutputCacheLocation Location {
            get {
                return (OutputCacheLocation)base[_propLocation];
            }
            set {
                base[_propLocation] = value;
            }
        }

        [ConfigurationProperty("shared", DefaultValue = false)]
        public bool Shared {
            get {
                return (bool)base[_propShared];
            }
            set {
                base[_propShared] = value;
            }
        }

        [ConfigurationProperty("provider")]
        public string Provider {
            get {
                return (string)base[_propProvider];
            }
            set {
                base[_propProvider] = value;
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

    }
#endif
}
