//------------------------------------------------------------------------------
// <copyright file="ProfileSettings.cs" company="Microsoft">
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
    using System.ComponentModel;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Compilation;
    using System.Security.Permissions;

    public sealed class ProfileSettings : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propMinInstances =
            new ConfigurationProperty("minInstances",
                                        typeof(int),
                                        RuleSettings.DEFAULT_MIN_INSTANCES,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxLimit =
            new ConfigurationProperty("maxLimit",
                                        typeof(int),
                                        RuleSettings.DEFAULT_MAX_LIMIT,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinInterval =
            new ConfigurationProperty("minInterval",
                                        typeof(TimeSpan),
                                        RuleSettings.DEFAULT_MIN_INTERVAL,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCustom =
            new ConfigurationProperty("custom", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.None);

        static ProfileSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propMinInstances);
            _properties.Add(_propMaxLimit);
            _properties.Add(_propMinInterval);
            _properties.Add(_propCustom);
        }

        internal ProfileSettings() {
        }

        public ProfileSettings(String name)
            : this() {
            Name = name;
        }

        public ProfileSettings(String name, int minInstances,
                                     int maxLimit, TimeSpan minInterval)
            : this(name) {
            MinInstances = minInstances;
            MaxLimit = maxLimit;
            MinInterval = minInterval;
        }

        public ProfileSettings(String name, int minInstances,
                                     int maxLimit, TimeSpan minInterval, string custom)
            : this(name) {
            MinInstances = minInstances;
            MaxLimit = maxLimit;
            MinInterval = minInterval;
            Custom = custom;
        }


        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
        [StringValidator(MinLength = 1)]
        public String Name {
            get {
                return (string)base[_propName];
            }
            set {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("minInstances", DefaultValue = 1)]
        [IntegerValidator(MinValue = 1)]
        public int MinInstances {
            get {
                return (int)base[_propMinInstances];
            }
            set {
                base[_propMinInstances] = value;
            }
        }

        [ConfigurationProperty("maxLimit", DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 0)]
        public int MaxLimit {
            get {
                return (int)base[_propMaxLimit];
            }
            set {
                base[_propMaxLimit] = value;
            }
        }

        [ConfigurationProperty("minInterval", DefaultValue = "00:00:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan MinInterval {
            get {
                return (TimeSpan)base[_propMinInterval];
            }
            set {
                base[_propMinInterval] = value;
            }
        }

        [ConfigurationProperty("custom", DefaultValue = "")]
        public String Custom {
            get {
                return (string)base[_propCustom];
            }
            set {
                base[_propCustom] = value;
            }
        }

    } // class ProfileSettings
}
