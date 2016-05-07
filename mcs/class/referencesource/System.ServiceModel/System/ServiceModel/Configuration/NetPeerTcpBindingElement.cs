//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Net;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel.Channels;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public partial class NetPeerTcpBindingElement : StandardBindingElement
    {
        public NetPeerTcpBindingElement(string name)
            : base(name)
        {
        }

        public NetPeerTcpBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetPeerTcpBinding); }
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
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[ConfigurationStrings.MaxBufferPoolSize]; }
            set { base[ConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = PeerTransportConstants.MinMessageSize)]
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

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.PeerResolver, DefaultValue = null)]
        public PeerResolverElement Resolver
        {
            get { return (PeerResolverElement)base[ConfigurationStrings.PeerResolver]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public PeerSecurityElement Security
        {
            get { return (PeerSecurityElement)base[ConfigurationStrings.Security]; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetPeerTcpBinding peerBinding = (NetPeerTcpBinding)binding;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ListenIPAddress, peerBinding.ListenIPAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, peerBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, peerBinding.MaxReceivedMessageSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.Port, peerBinding.Port);
            this.Security.InitializeFrom(peerBinding.Security);
            this.Resolver.InitializeFrom(peerBinding.Resolver);

            this.ReaderQuotas.InitializeFrom(peerBinding.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            NetPeerTcpBinding peerBinding = (NetPeerTcpBinding)binding;
            peerBinding.ListenIPAddress = this.ListenIPAddress;
            peerBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            peerBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            peerBinding.Port = this.Port;
            peerBinding.Security = new PeerSecuritySettings();
            this.ReaderQuotas.ApplyConfiguration(peerBinding.ReaderQuotas);
            this.Resolver.ApplyConfiguration(peerBinding.Resolver);
            this.Security.ApplyConfiguration(peerBinding.Security);
        }
    }
}

