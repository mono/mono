//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public sealed partial class UdpRetransmissionSettingsElement : ServiceModelConfigurationElement
    {
        public UdpRetransmissionSettingsElement()
        {
        }

        internal void ApplyConfiguration(UdpRetransmissionSettings udpRetransmissionSettings)
        {
            udpRetransmissionSettings.DelayLowerBound = this.DelayLowerBound;
            udpRetransmissionSettings.DelayUpperBound = this.DelayUpperBound;
            udpRetransmissionSettings.MaxDelayPerRetransmission = this.MaxDelayPerRetransmission;
            udpRetransmissionSettings.MaxMulticastRetransmitCount = this.MaxMulticastRetransmitCount;
            udpRetransmissionSettings.MaxUnicastRetransmitCount = this.MaxUnicastRetransmitCount;
        }

        internal void InitializeFrom(UdpRetransmissionSettings udpRetransmissionSettings)
        {
            if (udpRetransmissionSettings == null)
            {
                throw FxTrace.Exception.ArgumentNull("udpRetransmissionSettings");
            }

            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.DelayLowerBound, udpRetransmissionSettings.DelayLowerBound);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.DelayUpperBound, udpRetransmissionSettings.DelayUpperBound);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxDelayPerRetransmission, udpRetransmissionSettings.MaxDelayPerRetransmission);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxMulticastRetransmitCount, udpRetransmissionSettings.MaxMulticastRetransmitCount);
            this.SetPropertyValueIfNotDefaultValue(UdpTransportConfigurationStrings.MaxUnicastRetransmitCount, udpRetransmissionSettings.MaxUnicastRetransmitCount);
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.DelayLowerBound, DefaultValue = UdpConstants.Defaults.DelayLowerBound)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = UdpConstants.TimeSpanZero)]
        public TimeSpan DelayLowerBound
        {
            get { return (TimeSpan)base[UdpTransportConfigurationStrings.DelayLowerBound]; }
            set { base[UdpTransportConfigurationStrings.DelayLowerBound] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.DelayUpperBound, DefaultValue = UdpConstants.Defaults.DelayUpperBound)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = UdpConstants.TimeSpanZero)]
        public TimeSpan DelayUpperBound
        {
            get { return (TimeSpan)base[UdpTransportConfigurationStrings.DelayUpperBound]; }
            set { base[UdpTransportConfigurationStrings.DelayUpperBound] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxDelayPerRetransmission, DefaultValue = UdpConstants.Defaults.MaxDelayPerRetransmission)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = UdpConstants.TimeSpanZero)]
        public TimeSpan MaxDelayPerRetransmission
        {
            get { return (TimeSpan)base[UdpTransportConfigurationStrings.MaxDelayPerRetransmission]; }
            set { base[UdpTransportConfigurationStrings.MaxDelayPerRetransmission] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxMulticastRetransmitCount, DefaultValue = UdpConstants.Defaults.MaxMulticastRetransmitCount)]
        [IntegerValidator(MinValue = 0)]
        public int MaxMulticastRetransmitCount
        {
            get { return (int)base[UdpTransportConfigurationStrings.MaxMulticastRetransmitCount]; }
            set { base[UdpTransportConfigurationStrings.MaxMulticastRetransmitCount] = value; }
        }

        [ConfigurationProperty(UdpTransportConfigurationStrings.MaxUnicastRetransmitCount, DefaultValue = UdpConstants.Defaults.MaxUnicastRetransmitCount)]
        [IntegerValidator(MinValue = 0)]
        public int MaxUnicastRetransmitCount
        {
            get { return (int)base[UdpTransportConfigurationStrings.MaxUnicastRetransmitCount]; }
            set { base[UdpTransportConfigurationStrings.MaxUnicastRetransmitCount] = value; }
        }

    }
}
