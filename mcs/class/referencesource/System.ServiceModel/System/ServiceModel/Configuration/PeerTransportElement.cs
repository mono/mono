//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Net;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public partial class PeerTransportElement : BindingElementExtensionElement
    {
        public PeerTransportElement()
        {
        }

        public override Type BindingElementType
        {
            get { return typeof(PeerTransportBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.ListenIPAddress, DefaultValue = PeerTransportDefaults.ListenIPAddress)]
        [System.ComponentModel.TypeConverter(typeof(PeerTransportListenAddressConverter))]
        [PeerTransportListenAddressValidator()]
        public IPAddress ListenIPAddress
        {
            get { return (IPAddress)base[ConfigurationStrings.ListenIPAddress]; }
            set { base[ConfigurationStrings.ListenIPAddress] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferPoolSize, DefaultValue = TransportDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 1)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[ConfigurationStrings.MaxBufferPoolSize]; }
            set { base[ConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[ConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[ConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Port, DefaultValue = PeerTransportDefaults.Port)]
        [IntegerValidator(MinValue = PeerTransportConstants.MinPort, MaxValue = PeerTransportConstants.MaxPort)]
        public int Port
        {
            get { return (int)base[ConfigurationStrings.Port]; }
            set { base[ConfigurationStrings.Port] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public PeerSecurityElement Security
        {
            get { return (PeerSecurityElement)base[ConfigurationStrings.Security]; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            PeerTransportBindingElement binding = (PeerTransportBindingElement)bindingElement;
            binding.ListenIPAddress = this.ListenIPAddress;
            binding.Port = this.Port;
            binding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            binding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
#pragma warning suppress 56506 //[....]; base.ApplyConfiguration() checks for 'binding' being null
            this.Security.ApplyConfiguration(binding.Security);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            PeerTransportElement source = (PeerTransportElement)from;
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            this.ListenIPAddress = source.ListenIPAddress;
            this.Port = source.Port;
            this.MaxBufferPoolSize = source.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = source.MaxReceivedMessageSize;
            this.Security.CopyFrom(source.Security);
        }

        protected internal override BindingElement CreateBindingElement()
        {
            PeerTransportBindingElement binding = new PeerTransportBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            PeerTransportBindingElement binding = (PeerTransportBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ListenIPAddress, binding.ListenIPAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Port, binding.Port);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, binding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, binding.MaxReceivedMessageSize);
            this.Security.InitializeFrom(binding.Security);
        }
    }
}
