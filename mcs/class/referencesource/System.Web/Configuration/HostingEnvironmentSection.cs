//------------------------------------------------------------------------------
// <copyright file="HostingEnvironmentSection.cs" company="Microsoft">
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
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    public sealed class HostingEnvironmentSection : ConfigurationSection {
        internal const int DefaultShutdownTimeout = 30;
        internal static readonly TimeSpan DefaultIdleTimeout = TimeSpan.MaxValue; // default is Infinite
        internal static readonly TimeSpan DefaultUrlMetadataSlidingExpiration = TimeSpan.FromMinutes(1); //default is 00:01:00

        internal const String sectionName = "system.web/hostingEnvironment";

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propIdleTimeout =
            new ConfigurationProperty("idleTimeout",
                                        typeof(TimeSpan),
                                        DefaultIdleTimeout,
                                        StdValidatorsAndConverters.TimeSpanMinutesOrInfiniteConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propShutdownTimeout =
            new ConfigurationProperty("shutdownTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((double)DefaultShutdownTimeout),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propShadowCopyBinAssemblies =
            new ConfigurationProperty("shadowCopyBinAssemblies", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUrlMetadataSlidingExpiration =
            new ConfigurationProperty("urlMetadataSlidingExpiration",
                                        typeof(TimeSpan),
                                        DefaultUrlMetadataSlidingExpiration,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        new TimeSpanValidator(TimeSpan.Zero, TimeSpan.MaxValue),
                                        ConfigurationPropertyOptions.None);

                /*         <!--
                hostingEnvironment attributes:
                    idleTimeout="[Infinite|minutes]" - idle timeout in minutes to unload this application
                    shutdownTimout="[seconds]" - time given for g----ful shutdown of this application
                -->
                <hostingEnvironment
                    idleTimeout="20"
                    shutdownTimeout="30"
                />

 */
        static HostingEnvironmentSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propIdleTimeout);
            _properties.Add(_propShutdownTimeout);
            _properties.Add(_propShadowCopyBinAssemblies);
            _properties.Add(_propUrlMetadataSlidingExpiration);
        }



        public HostingEnvironmentSection() {
        }
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("shutdownTimeout", DefaultValue = "00:00:30")]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan ShutdownTimeout {
            get {
                return (TimeSpan)base[_propShutdownTimeout];
            }
            set {
                base[_propShutdownTimeout] = value;
            }
        }

        [ConfigurationProperty("idleTimeout", DefaultValue = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanMinutesOrInfiniteConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan IdleTimeout {
            get {
                return (TimeSpan)base[_propIdleTimeout];
            }
            set {
                base[_propIdleTimeout] = value;
            }
        }

        [ConfigurationProperty("shadowCopyBinAssemblies", DefaultValue = true)]
        public bool ShadowCopyBinAssemblies {
            get {
                return (bool)base[_propShadowCopyBinAssemblies];
            }
            set {
                base[_propShadowCopyBinAssemblies] = value;
            }
        }

        [ConfigurationProperty("urlMetadataSlidingExpiration", DefaultValue = "00:01:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan UrlMetadataSlidingExpiration {
            get {
                return (TimeSpan)base[_propUrlMetadataSlidingExpiration];
            }
            set {
                base[_propUrlMetadataSlidingExpiration] = value;
            }
        }
    }
}
