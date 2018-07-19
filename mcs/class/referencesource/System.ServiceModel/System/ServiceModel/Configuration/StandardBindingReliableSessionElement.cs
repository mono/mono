//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public partial class StandardBindingReliableSessionElement : ServiceModelConfigurationElement
    {
        public StandardBindingReliableSessionElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Ordered, DefaultValue = ReliableSessionDefaults.Ordered)]
        public bool Ordered
        {
            get { return (bool)base[ConfigurationStrings.Ordered]; }
            set { base[ConfigurationStrings.Ordered] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.InactivityTimeout, DefaultValue = ReliableSessionDefaults.InactivityTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanOneTick)]
        public TimeSpan InactivityTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.InactivityTimeout]; }
            set { base[ConfigurationStrings.InactivityTimeout] = value; }
        }

        public void InitializeFrom(System.ServiceModel.ReliableSession reliableSession)
        {
            if (null == reliableSession)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSession");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Ordered, reliableSession.Ordered);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.InactivityTimeout, reliableSession.InactivityTimeout);
        }

        public void ApplyConfiguration(System.ServiceModel.ReliableSession reliableSession)
        {
            if (null == reliableSession)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSession");
            }
            reliableSession.Ordered = this.Ordered;
            reliableSession.InactivityTimeout = this.InactivityTimeout;
        }
    }
}
