//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;

    public sealed partial class HostTimeoutsElement : ConfigurationElement
    {
        public HostTimeoutsElement() : base() { }

        [ConfigurationProperty(ConfigurationStrings.CloseTimeout, DefaultValue = ServiceDefaults.ServiceHostCloseTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan CloseTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.CloseTimeout]; }
            set { base[ConfigurationStrings.CloseTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.OpenTimeout, DefaultValue = ServiceDefaults.OpenTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan OpenTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.OpenTimeout]; }
            set { base[ConfigurationStrings.OpenTimeout] = value; }
        }
    }
}
