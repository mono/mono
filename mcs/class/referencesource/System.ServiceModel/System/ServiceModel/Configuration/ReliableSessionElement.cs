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

    public sealed partial class ReliableSessionElement : BindingElementExtensionElement
    {
        public ReliableSessionElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AcknowledgementInterval, DefaultValue = ReliableSessionDefaults.AcknowledgementIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanOneTick)]
        public TimeSpan AcknowledgementInterval
        {
            get { return (TimeSpan)base[ConfigurationStrings.AcknowledgementInterval]; }
            set { base[ConfigurationStrings.AcknowledgementInterval] = value; }
        }

        public override Type BindingElementType
        {
            get { return typeof(ReliableSessionBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.FlowControlEnabled, DefaultValue = true)]
        public bool FlowControlEnabled
        {
            get { return (bool)base[ConfigurationStrings.FlowControlEnabled]; }
            set { base[ConfigurationStrings.FlowControlEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.InactivityTimeout, DefaultValue = ReliableSessionDefaults.InactivityTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanOneTick)]
        public TimeSpan InactivityTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.InactivityTimeout]; }
            set { base[ConfigurationStrings.InactivityTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingChannels, DefaultValue = ReliableSessionDefaults.MaxPendingChannels)]
        [IntegerValidator(MinValue = 1, MaxValue = 16384)]
        public int MaxPendingChannels
        {
            get { return (int)base[ConfigurationStrings.MaxPendingChannels]; }
            set { base[ConfigurationStrings.MaxPendingChannels] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxRetryCount, DefaultValue = ReliableSessionDefaults.MaxRetryCount)]
        [IntegerValidator(MinValue = 1)]
        public int MaxRetryCount
        {
            get { return (int)base[ConfigurationStrings.MaxRetryCount]; }
            set { base[ConfigurationStrings.MaxRetryCount] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxTransferWindowSize, DefaultValue = ReliableSessionDefaults.MaxTransferWindowSize)]
        [IntegerValidator(MinValue = 1, MaxValue = 4096)]
        public int MaxTransferWindowSize
        {
            get { return (int)base[ConfigurationStrings.MaxTransferWindowSize]; }
            set { base[ConfigurationStrings.MaxTransferWindowSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Ordered, DefaultValue = ReliableSessionDefaults.Ordered)]
        public bool Ordered
        {
            get { return (bool)base[ConfigurationStrings.Ordered]; }
            set { base[ConfigurationStrings.Ordered] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReliableMessagingVersion, DefaultValue = ReliableSessionDefaults.ReliableMessagingVersionString)]
        [TypeConverter(typeof(ReliableMessagingVersionConverter))]
        public ReliableMessagingVersion ReliableMessagingVersion
        {
            get { return (ReliableMessagingVersion)base[ConfigurationStrings.ReliableMessagingVersion]; }
            set { base[ConfigurationStrings.ReliableMessagingVersion] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            ReliableSessionBindingElement binding = (ReliableSessionBindingElement)bindingElement;
            binding.AcknowledgementInterval = this.AcknowledgementInterval;
            binding.FlowControlEnabled = this.FlowControlEnabled;
            binding.InactivityTimeout = this.InactivityTimeout;
            binding.MaxPendingChannels = this.MaxPendingChannels;
            binding.MaxRetryCount = this.MaxRetryCount;
            binding.MaxTransferWindowSize = this.MaxTransferWindowSize;
            binding.Ordered = this.Ordered;
            binding.ReliableMessagingVersion = this.ReliableMessagingVersion;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ReliableSessionElement source = (ReliableSessionElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() checks for 'from' being null
            this.AcknowledgementInterval = source.AcknowledgementInterval;
            this.FlowControlEnabled = source.FlowControlEnabled;
            this.InactivityTimeout = source.InactivityTimeout;
            this.MaxPendingChannels = source.MaxPendingChannels;
            this.MaxRetryCount = source.MaxRetryCount;
            this.MaxTransferWindowSize = source.MaxTransferWindowSize;
            this.Ordered = source.Ordered;
            this.ReliableMessagingVersion = source.ReliableMessagingVersion;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            ReliableSessionBindingElement binding = new ReliableSessionBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            ReliableSessionBindingElement binding = (ReliableSessionBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AcknowledgementInterval, binding.AcknowledgementInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.FlowControlEnabled, binding.FlowControlEnabled);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.InactivityTimeout, binding.InactivityTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxPendingChannels, binding.MaxPendingChannels);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxRetryCount, binding.MaxRetryCount);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxTransferWindowSize, binding.MaxTransferWindowSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Ordered, binding.Ordered);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReliableMessagingVersion, binding.ReliableMessagingVersion);
        }
    }
}



