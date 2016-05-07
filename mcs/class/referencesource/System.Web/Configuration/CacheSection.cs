//------------------------------------------------------------------------------
// <copyright file="CacheSection.cs" company="Microsoft">
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
    using System.Web.Caching;
    using System.ComponentModel;
    using System.Security.Permissions;

    /* 
            <!--
            cache Attributes:
              cacheAPIEnabled="[true|false]" - Enable or disable the user Cache API
              disableMemoryCollection="[true|false]" - Enable or disable the cache memory collection
              disableExpiration="[true|false]" - Enable or disable the expiration of items from the cache
              privateBytesLimit="number" - Represents maximum private bytes (in bytes) allowed. If it's zero, Cache will use an auto-generated limit. Cache will collect memory when the private bytes is near the limit. This works on top of other memory indexes monitored by Cache.
              percentagePhysicalMemoryUsedLimit="number" - Represents percentage of physical memory process allowed. Cache will collect memory when the private bytes is near the limit. This works on top of other memory indexes monitored by Cache.
              privateBytesPollTime="timespan" - How often we poll the process memory by calling NtQuerySystemInformation. Default is 2 min.
            -->
            <cache cacheAPIEnabled="true" />
      */

    public sealed class CacheSection : ConfigurationSection {
        internal static TimeSpan DefaultPrivateBytesPollTime = new TimeSpan(0, 2, 0);

        private static ConfigurationPropertyCollection _properties;
#if NOT_UNTIL_LATER
        private static readonly ConfigurationProperty   _propCacheAPIEnabled;
        private static readonly ConfigurationProperty   _propDisableDependencies;
#endif
        private static readonly ConfigurationProperty _propDisableMemoryCollection;
        private static readonly ConfigurationProperty _propDisableExpiration;

        private static readonly ConfigurationProperty _propPrivateBytesLimit;
        private static readonly ConfigurationProperty _propPercentagePhysicalMemoryUsedLimit;
        private static readonly ConfigurationProperty _propPrivateBytesPollTime;


        static CacheSection() {
            // Property initialization
#if NOT_UNTIL_LATER
            _propCacheAPIEnabled = new ConfigurationProperty("cacheAPIEnabled", typeof(bool), true, ConfigurationPropertyOptions.None);
            _propDisableDependencies = new ConfigurationProperty("disableDependencies", typeof(bool), false, ConfigurationPropertyOptions.None);
#endif

            _propDisableMemoryCollection = 
                new ConfigurationProperty("disableMemoryCollection", 
                                            typeof(bool), 
                                            false, 
                                            ConfigurationPropertyOptions.None);
            _propDisableExpiration = 
                new ConfigurationProperty("disableExpiration", 
                                            typeof(bool), 
                                            false, 
                                            ConfigurationPropertyOptions.None);
            _propPrivateBytesLimit = 
                new ConfigurationProperty("privateBytesLimit",
                                            typeof(long),
                                            (long)0,
                                            null,
                                            new LongValidator(0, long.MaxValue),
                                            ConfigurationPropertyOptions.None);
            _propPercentagePhysicalMemoryUsedLimit =
                new ConfigurationProperty("percentagePhysicalMemoryUsedLimit",
                                            typeof(int),
                                            (int)0,
                                            null,
                                            new IntegerValidator(0, 100),
                                            ConfigurationPropertyOptions.None);
            _propPrivateBytesPollTime =
                new ConfigurationProperty("privateBytesPollTime",
                                            typeof(TimeSpan),
                                            DefaultPrivateBytesPollTime,
                                            StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                            StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                            ConfigurationPropertyOptions.None);

            _properties = new ConfigurationPropertyCollection();

#if NOT_UNTIL_LATER
            _properties.Add(_propCacheAPIEnabled);
            _properties.Add(_propDisableDependencies);
#endif

            _properties.Add(_propDisableMemoryCollection);
            _properties.Add(_propDisableExpiration);
            _properties.Add(_propPrivateBytesLimit);
            _properties.Add(_propPercentagePhysicalMemoryUsedLimit);
            _properties.Add(_propPrivateBytesPollTime);
        }

        public CacheSection() {
        }

#if NOT_UNTIL_LATER
        [ConfigurationProperty("cacheAPIEnabled", DefaultValue = true)]
        public bool CacheAPIEnabled
        {
            get
            {
                return (bool)base[_propCacheAPIEnabled];
            }
            set
            {
                base[_propCacheAPIEnabled] = value;
            }
        }
#endif
        [ConfigurationProperty("disableMemoryCollection", DefaultValue = false)]
        public bool DisableMemoryCollection {
            get {
                return (bool)base[_propDisableMemoryCollection];
            }
            set {
                base[_propDisableMemoryCollection] = value;
            }
        }

        [ConfigurationProperty("disableExpiration", DefaultValue = false)]
        public bool DisableExpiration {
            get {
                return (bool)base[_propDisableExpiration];
            }
            set {
                base[_propDisableExpiration] = value;
            }
        }

#if NOT_UNTIL_LATER
        [ConfigurationProperty("disableDependencies", DefaultValue = false)]
        public bool DisableDependencies
        {
            get
            {
                return (bool)base[_propDisableDependencies];
            }
            set
            {
                base[_propDisableDependencies] = value;
            }
        }
#endif

        [ConfigurationProperty("privateBytesLimit", DefaultValue = (long)0)]
        [LongValidator(MinValue = 0)]
        public long PrivateBytesLimit {
            get {
                return (long)base[_propPrivateBytesLimit];
            }
            set {
                base[_propPrivateBytesLimit] = value;
            }
        }

        [ConfigurationProperty("percentagePhysicalMemoryUsedLimit", DefaultValue = (int)0)]
        [IntegerValidator(MinValue = 0, MaxValue = 100)]
        public int PercentagePhysicalMemoryUsedLimit {
            get {
                return (int)base[_propPercentagePhysicalMemoryUsedLimit];
            }
            set {
                base[_propPercentagePhysicalMemoryUsedLimit] = value;
            }
        }


        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("privateBytesPollTime", DefaultValue = "00:02:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan PrivateBytesPollTime {
            get {
                return (TimeSpan)base[_propPrivateBytesPollTime];
            }
            set {
                base[_propPrivateBytesPollTime] = value;
            }
        }
    }
}
