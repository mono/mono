//------------------------------------------------------------------------------
// <copyright file="RuleSettings.cs" company="Microsoft">
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

    public sealed class RuleSettings : ConfigurationElement {
        internal static int DEFAULT_MIN_INSTANCES = 1;
        internal static int DEFAULT_MAX_LIMIT = int.MaxValue;
        internal static TimeSpan DEFAULT_MIN_INTERVAL = TimeSpan.Zero;
        internal static string DEFAULT_CUSTOM_EVAL = null;

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | 
                                        ConfigurationPropertyOptions.IsKey);
        private static readonly ConfigurationProperty _propEventName =
            new ConfigurationProperty("eventName", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propProvider =
            new ConfigurationProperty("provider", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propProfile =
            new ConfigurationProperty("profile", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMinInstances =
            new ConfigurationProperty("minInstances",
                                        typeof(int),
                                        DEFAULT_MIN_INSTANCES,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMaxLimit =
            new ConfigurationProperty("maxLimit",
                                        typeof(int),
                                        DEFAULT_MAX_LIMIT,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMinInterval =
            new ConfigurationProperty("minInterval",
                                        typeof(TimeSpan),
                                        DEFAULT_MIN_INTERVAL,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCustom =
            new ConfigurationProperty("custom", 
                                        typeof(string), 
                                        String.Empty, 
                                        ConfigurationPropertyOptions.None);

        static RuleSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propEventName);
            _properties.Add(_propProvider);
            _properties.Add(_propProfile);
            _properties.Add(_propMinInstances);
            _properties.Add(_propMaxLimit);
            _properties.Add(_propMinInterval);
            _properties.Add(_propCustom);
        }

        internal RuleSettings() {
        }

        public RuleSettings(String name, String eventName, String provider)
            : this() {
            Name = name;
            EventName = eventName;
            Provider = provider;
        }

        public RuleSettings(String name, String eventName, String provider, String profile, int minInstances, int maxLimit, TimeSpan minInterval)
            : this(name, eventName, provider) {
            Profile = profile;
            MinInstances = minInstances;
            MaxLimit = maxLimit;
            MinInterval = minInterval;
        }

        public RuleSettings(String name, String eventName, String provider, String profile, int minInstances, int maxLimit, TimeSpan minInterval, string custom)
            : this(name, eventName, provider) {
            Profile = profile;
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

        [ConfigurationProperty("eventName", IsRequired = true, DefaultValue = "")]
        public String EventName {
            get {
                return (string)base[_propEventName];
            }
            set {
                base[_propEventName] = value;
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

        [ConfigurationProperty("profile", DefaultValue = "")]
        public String Profile {
            get {
                return (string)base[_propProfile];
            }
            set {
                base[_propProfile] = value;
            }
        }

        [ConfigurationProperty("provider", DefaultValue = "")]
        public String Provider {
            get {
                return (string)base[_propProvider];
            }
            set {
                base[_propProvider] = value;
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
    }
}
