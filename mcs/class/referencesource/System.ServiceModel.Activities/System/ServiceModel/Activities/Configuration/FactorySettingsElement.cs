//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System.Runtime;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Description;

    public sealed class FactorySettingsElement : ConfigurationElement
    {
        ConfigurationPropertyCollection properties;
       
        public FactorySettingsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IdleTimeout, DefaultValue = ChannelCacheDefaults.DefaultIdleTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan IdleTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.IdleTimeout]; }
            set { base[ConfigurationStrings.IdleTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LeaseTimeout, DefaultValue = ChannelCacheDefaults.DefaultFactoryLeaseTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan LeaseTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.LeaseTimeout]; }
            set { base[ConfigurationStrings.LeaseTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxItemsInCache, DefaultValue = ChannelCacheDefaults.DefaultMaxItemsPerCacheString)]
        [IntegerValidator(MinValue = 0)]
        public int MaxItemsInCache
        {
            get { return (int)base[ConfigurationStrings.MaxItemsInCache]; }
            set { base[ConfigurationStrings.MaxItemsInCache] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.MaxItemsInCache, typeof(System.Int32), ChannelCacheDefaults.DefaultMaxItemsPerCache, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.IdleTimeout, typeof(System.TimeSpan), ChannelCacheDefaults.DefaultIdleTimeout, new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.LeaseTimeout, typeof(System.TimeSpan), ChannelCacheDefaults.DefaultChannelLeaseTimeoutString, new System.ServiceModel.Configuration.TimeSpanOrInfiniteConverter(), new System.ServiceModel.Configuration.TimeSpanOrInfiniteValidator(System.TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), System.TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}




