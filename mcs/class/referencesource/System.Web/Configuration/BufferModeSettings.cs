//------------------------------------------------------------------------------
// <copyright file="BufferModeSettings.cs" company="Microsoft">
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

    public sealed class BufferModeSettings : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(BufferModeSettings), Validate));

        const int DefaultMaxBufferThreads = 1;

        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

        private static readonly ConfigurationProperty _propMaxBufferSize =
            new ConfigurationProperty("maxBufferSize",
                                        typeof(int),
                                        int.MaxValue,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propMaxFlushSize =
            new ConfigurationProperty("maxFlushSize",
                                        typeof(int),
                                        int.MaxValue,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propUrgentFlushThreshold =
            new ConfigurationProperty("urgentFlushThreshold",
                                        typeof(int),
                                        int.MaxValue,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propRegularFlushInterval =
            new ConfigurationProperty("regularFlushInterval",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds(1),
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propUrgentFlushInterval =
            new ConfigurationProperty("urgentFlushInterval",
                                        typeof(TimeSpan),
                                        TimeSpan.Zero,
                                        StdValidatorsAndConverters.InfiniteTimeSpanConverter,
                                        null,
                                        ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _propMaxBufferThreads =
            new ConfigurationProperty("maxBufferThreads",
                                        typeof(int),
                                        DefaultMaxBufferThreads,
                                        new InfiniteIntConverter(),
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);

        static BufferModeSettings() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
            _properties.Add(_propMaxBufferSize);
            _properties.Add(_propMaxFlushSize);
            _properties.Add(_propUrgentFlushThreshold);
            _properties.Add(_propRegularFlushInterval);
            _properties.Add(_propUrgentFlushInterval);
            _properties.Add(_propMaxBufferThreads);
        }

        internal BufferModeSettings() {
        }

        public BufferModeSettings(String name, int maxBufferSize, int maxFlushSize,
                        int urgentFlushThreshold, TimeSpan regularFlushInterval,
                        TimeSpan urgentFlushInterval, int maxBufferThreads)
            : this() {
            Name = name;
            MaxBufferSize = maxBufferSize;
            MaxFlushSize = maxFlushSize;
            UrgentFlushThreshold = urgentFlushThreshold;
            RegularFlushInterval = regularFlushInterval;
            UrgentFlushInterval = urgentFlushInterval;
            MaxBufferThreads = maxBufferThreads;
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

        [ConfigurationProperty("maxBufferSize", IsRequired = true, DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferSize {
            get {
                return (int)base[_propMaxBufferSize];
            }
            set {
                base[_propMaxBufferSize] = value;
            }
        }

        [ConfigurationProperty("maxFlushSize", IsRequired = true, DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 1)]
        public int MaxFlushSize {
            get {
                return (int)base[_propMaxFlushSize];
            }
            set {
                base[_propMaxFlushSize] = value;
            }
        }

        [ConfigurationProperty("urgentFlushThreshold", IsRequired = true, DefaultValue = int.MaxValue)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 1)]
        public int UrgentFlushThreshold {
            get {
                return (int)base[_propUrgentFlushThreshold];
            }
            set {
                base[_propUrgentFlushThreshold] = value;
            }
        }

        [ConfigurationProperty("regularFlushInterval", IsRequired = true, DefaultValue = "00:00:01")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan RegularFlushInterval {
            get {
                return (TimeSpan)base[_propRegularFlushInterval];
            }
            set {
                base[_propRegularFlushInterval] = value;
            }
        }

        [ConfigurationProperty("urgentFlushInterval", IsRequired = true, DefaultValue = "00:00:00")]
        [TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan UrgentFlushInterval {
            get {
                return (TimeSpan)base[_propUrgentFlushInterval];
            }
            set {
                base[_propUrgentFlushInterval] = value;
            }
        }

        [ConfigurationProperty("maxBufferThreads", DefaultValue = DefaultMaxBufferThreads)]
        [TypeConverter(typeof(InfiniteIntConverter))]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferThreads {
            get {
                return (int)base[_propMaxBufferThreads];
            }
            set {
                base[_propMaxBufferThreads] = value;
            }
        }
        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }
        private static void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("bufferMode");
            }
            Debug.Assert(value is BufferModeSettings);

            BufferModeSettings elem = (BufferModeSettings)value;

            if (!(elem.UrgentFlushThreshold <= elem.MaxBufferSize)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_attribute1_must_less_than_or_equal_attribute2,
                        elem.UrgentFlushThreshold.ToString(CultureInfo.InvariantCulture),
                        "urgentFlushThreshold",
                        elem.MaxBufferSize.ToString(CultureInfo.InvariantCulture),
                        "maxBufferSize"),
                    elem.ElementInformation.Properties["urgentFlushThreshold"].Source, elem.ElementInformation.Properties["urgentFlushThreshold"].LineNumber);
            }

            if (!(elem.MaxFlushSize <= elem.MaxBufferSize)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_attribute1_must_less_than_or_equal_attribute2,
                        elem.MaxFlushSize.ToString(CultureInfo.InvariantCulture),
                        "maxFlushSize",
                        elem.MaxBufferSize.ToString(CultureInfo.InvariantCulture),
                        "maxBufferSize"),
                    elem.ElementInformation.Properties["maxFlushSize"].Source, elem.ElementInformation.Properties["maxFlushSize"].LineNumber);
            }

            if (!(elem.UrgentFlushInterval < elem.RegularFlushInterval)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_attribute1_must_less_than_attribute2,
                        elem.UrgentFlushInterval.ToString(),
                        "urgentFlushInterval",
                        elem.RegularFlushInterval.ToString(),
                        "regularFlushInterval"),
                    elem.ElementInformation.Properties["urgentFlushInterval"].Source, elem.ElementInformation.Properties["urgentFlushInterval"].LineNumber);
            }

        }
    }
}
