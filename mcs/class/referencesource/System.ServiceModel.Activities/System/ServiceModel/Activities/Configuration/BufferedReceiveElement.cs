//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.ServiceModel.Activities.Configuration
{
    using System.Runtime;
    using System.Configuration;
    using System.ComponentModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Description;

    public sealed class BufferedReceiveElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;
        const string MaxPendingMessagesPerChannelString = "maxPendingMessagesPerChannel";

        public BufferedReceiveElement()
        {
        }

        [ConfigurationProperty(MaxPendingMessagesPerChannelString, DefaultValue = BufferedReceiveServiceBehavior.DefaultMaxPendingMessagesPerChannel)]
        [TypeConverter(typeof(Int32Converter))]
        [IntegerValidator(MinValue = 1, MaxValue = Int32.MaxValue)]
        public int MaxPendingMessagesPerChannel
        {
            get { return (int)base[MaxPendingMessagesPerChannelString]; }
            set { base[MaxPendingMessagesPerChannelString] = value; }
        }

        protected internal override object CreateBehavior()
        {
            return new BufferedReceiveServiceBehavior() { MaxPendingMessagesPerChannel = this.MaxPendingMessagesPerChannel };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Activities.Configuration.BufferedReceiveElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(BufferedReceiveServiceBehavior); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(MaxPendingMessagesPerChannelString, typeof(Int32), BufferedReceiveServiceBehavior.DefaultMaxPendingMessagesPerChannel, new Int32Converter(), new IntegerValidator(1, Int32.MaxValue), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}
