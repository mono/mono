//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract partial class ConnectionOrientedTransportElement : TransportElement
    {
        internal ConnectionOrientedTransportElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ConnectionBufferSize, DefaultValue = ConnectionOrientedTransportDefaults.ConnectionBufferSize)]
        [IntegerValidator(MinValue = 1)]
        public int ConnectionBufferSize
        {
            get { return (int)base[ConfigurationStrings.ConnectionBufferSize]; }
            set { base[ConfigurationStrings.ConnectionBufferSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HostNameComparisonMode, DefaultValue = ConnectionOrientedTransportDefaults.HostNameComparisonMode)]
        [ServiceModelEnumValidatorAttribute(typeof(HostNameComparisonModeHelper))]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return (HostNameComparisonMode)base[ConfigurationStrings.HostNameComparisonMode]; }
            set { base[ConfigurationStrings.HostNameComparisonMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ChannelInitializationTimeout, DefaultValue = ConnectionOrientedTransportDefaults.ChannelInitializationTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanOneTick)]
        public TimeSpan ChannelInitializationTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.ChannelInitializationTimeout]; }
            set { base[ConfigurationStrings.ChannelInitializationTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferSize, DefaultValue = TransportDefaults.MaxBufferSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferSize
        {
            get { return (int)base[ConfigurationStrings.MaxBufferSize]; }
            set { base[ConfigurationStrings.MaxBufferSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingConnections, DefaultValue = ConnectionOrientedTransportDefaults.MaxPendingConnectionsConst)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPendingConnections
        {
            get { return (int)base[ConfigurationStrings.MaxPendingConnections]; }
            set { base[ConfigurationStrings.MaxPendingConnections] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxOutputDelay, DefaultValue = ConnectionOrientedTransportDefaults.MaxOutputDelayString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxOutputDelay
        {
            get { return (TimeSpan)base[ConfigurationStrings.MaxOutputDelay]; }
            set { base[ConfigurationStrings.MaxOutputDelay] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingAccepts, DefaultValue = ConnectionOrientedTransportDefaults.MaxPendingAcceptsConst)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPendingAccepts
        {
            get { return (int)base[ConfigurationStrings.MaxPendingAccepts]; }
            set { base[ConfigurationStrings.MaxPendingAccepts] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransferMode, DefaultValue = ConnectionOrientedTransportDefaults.TransferMode)]
        [ServiceModelEnumValidator(typeof(TransferModeHelper))]
        public TransferMode TransferMode
        {
            get { return (TransferMode)base[ConfigurationStrings.TransferMode]; }
            set { base[ConfigurationStrings.TransferMode] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            ConnectionOrientedTransportBindingElement binding = (ConnectionOrientedTransportBindingElement)bindingElement;
            binding.ConnectionBufferSize = this.ConnectionBufferSize;
            binding.HostNameComparisonMode = this.HostNameComparisonMode;
            binding.ChannelInitializationTimeout = this.ChannelInitializationTimeout;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                binding.MaxBufferSize = this.MaxBufferSize;
            }
            if (this.MaxPendingConnections != ConnectionOrientedTransportDefaults.MaxPendingConnectionsConst)
            {
                binding.MaxPendingConnections = this.MaxPendingConnections;
            }
            binding.MaxOutputDelay = this.MaxOutputDelay;
            if (this.MaxPendingAccepts != ConnectionOrientedTransportDefaults.MaxPendingAcceptsConst)
            {
                binding.MaxPendingAccepts = this.MaxPendingAccepts;
            }
            binding.TransferMode = this.TransferMode;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ConnectionOrientedTransportElement source = (ConnectionOrientedTransportElement)from;
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            this.ConnectionBufferSize = source.ConnectionBufferSize;
            this.HostNameComparisonMode = source.HostNameComparisonMode;
            this.ChannelInitializationTimeout = source.ChannelInitializationTimeout;
            this.MaxBufferSize = source.MaxBufferSize;
            this.MaxPendingConnections = source.MaxPendingConnections;
            this.MaxOutputDelay = source.MaxOutputDelay;
            this.MaxPendingAccepts = source.MaxPendingAccepts;
            this.TransferMode = source.TransferMode;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            ConnectionOrientedTransportBindingElement binding = (ConnectionOrientedTransportBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ConnectionBufferSize, binding.ConnectionBufferSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, binding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ChannelInitializationTimeout, binding.ChannelInitializationTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, binding.MaxBufferSize);
            if (binding.IsMaxPendingConnectionsSet)
            {
                ConfigurationProperty maxPendingConnectionsProperty = this.Properties[ConfigurationStrings.MaxPendingConnections];
                SetPropertyValue(maxPendingConnectionsProperty, binding.MaxPendingConnections, /*ignoreLocks = */ false);
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxOutputDelay, binding.MaxOutputDelay);
            if (binding.IsMaxPendingAcceptsSet)
            {
                ConfigurationProperty maxPendingAcceptsProperty = this.Properties[ConfigurationStrings.MaxPendingAccepts];
                SetPropertyValue(maxPendingAcceptsProperty, binding.MaxPendingAccepts, /*ignoreLocks = */ false);
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, binding.TransferMode);
        }
    }
}



