//------------------------------------------------------------------------------
// <copyright file="MemoryCacheSettingsSettingsCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Runtime.Caching.Configuration {
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;

    public sealed class MemoryCacheElement : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propName;
        private static readonly ConfigurationProperty _propPhysicalMemoryLimitPercentage;
        private static readonly ConfigurationProperty _propCacheMemoryLimitMegabytes;
        private static readonly ConfigurationProperty _propPollingInterval;

        static MemoryCacheElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            
            _propName = 
                new ConfigurationProperty("name",
                                          typeof(string),
                                          null,
                                          new WhiteSpaceTrimStringConverter(),
                                          new StringValidator(1),
                                          ConfigurationPropertyOptions.IsRequired | 
                                          ConfigurationPropertyOptions.IsKey);
            
            _propPhysicalMemoryLimitPercentage = 
                new ConfigurationProperty("physicalMemoryLimitPercentage",
                                          typeof(int),
                                          (int)0,
                                          null,
                                          new IntegerValidator(0, 100),
                                          ConfigurationPropertyOptions.None);

            _propCacheMemoryLimitMegabytes = 
                new ConfigurationProperty("cacheMemoryLimitMegabytes", 
                                          typeof(int),
                                          (int)0,
                                          null,
                                          new IntegerValidator(0, Int32.MaxValue),
                                          ConfigurationPropertyOptions.None);

            _propPollingInterval = 
                new ConfigurationProperty("pollingInterval",
                                          typeof(TimeSpan),
                                          TimeSpan.FromMilliseconds(ConfigUtil.DefaultPollingTimeMilliseconds),
                                          new InfiniteTimeSpanConverter(),
                                          new PositiveTimeSpanValidator(),
                                          ConfigurationPropertyOptions.None);
            
            _properties.Add(_propName);
            _properties.Add(_propPhysicalMemoryLimitPercentage);
            _properties.Add(_propCacheMemoryLimitMegabytes);
            _properties.Add(_propPollingInterval);
        }

        internal MemoryCacheElement() {
        }

        public MemoryCacheElement(string name) {
            Name = name;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true, IsKey = true)]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base["name"];
            }
            set {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("physicalMemoryLimitPercentage", DefaultValue = (int)0)]
        [IntegerValidator(MinValue = 0, MaxValue = 100)]
        public int PhysicalMemoryLimitPercentage {
            get {
                return (int)base["physicalMemoryLimitPercentage"];
            }
            set {
                base["physicalMemoryLimitPercentage"] = value;
            }
        }

        [ConfigurationProperty("cacheMemoryLimitMegabytes", DefaultValue = (int)0)]
        [IntegerValidator(MinValue = 0)]
        public int CacheMemoryLimitMegabytes {
            get {
                return (int)base["cacheMemoryLimitMegabytes"];
            }
            set {
                base["cacheMemoryLimitMegabytes"] = value;
            }
        }

        [ConfigurationProperty("pollingInterval", DefaultValue = "00:02:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan PollingInterval {
            get {
                return (TimeSpan)base["pollingInterval"];
            }
            set {
                base["pollingInterval"] = value;
            }
        }
    }
}
