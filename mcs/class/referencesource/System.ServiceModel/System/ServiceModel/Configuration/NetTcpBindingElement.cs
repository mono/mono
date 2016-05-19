//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.Net.Security;
    using System.Net.Sockets;

    public partial class NetTcpBindingElement : StandardBindingElement
    {
        public NetTcpBindingElement(string name)
            : base(name)
        {
        }

        public NetTcpBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetTcpBinding); }
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionFlow, DefaultValue = TransactionFlowDefaults.Transactions)]
        public bool TransactionFlow
        {
            get { return (bool)base[ConfigurationStrings.TransactionFlow]; }
            set { base[ConfigurationStrings.TransactionFlow] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransferMode, DefaultValue = ConnectionOrientedTransportDefaults.TransferMode)]
        [ServiceModelEnumValidator(typeof(TransferModeHelper))]
        public TransferMode TransferMode
        {
            get { return (TransferMode)base[ConfigurationStrings.TransferMode]; }
            set { base[ConfigurationStrings.TransferMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionProtocol, DefaultValue = TransactionFlowDefaults.TransactionProtocolString)]
        [TypeConverter(typeof(TransactionProtocolConverter))]
        public TransactionProtocol TransactionProtocol
        {
            get { return (TransactionProtocol)base[ConfigurationStrings.TransactionProtocol]; }
            set { base[ConfigurationStrings.TransactionProtocol] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HostNameComparisonMode, DefaultValue = ConnectionOrientedTransportDefaults.HostNameComparisonMode)]
        [ServiceModelEnumValidator(typeof(HostNameComparisonModeHelper))]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return (HostNameComparisonMode)base[ConfigurationStrings.HostNameComparisonMode]; }
            set { base[ConfigurationStrings.HostNameComparisonMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ListenBacklog, DefaultValue = TcpTransportDefaults.ListenBacklogConst)]
        [IntegerValidator(MinValue = 0)]
        public int ListenBacklog
        {
            get { return (int)base[ConfigurationStrings.ListenBacklog]; }
            set { base[ConfigurationStrings.ListenBacklog] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferPoolSize, DefaultValue = TransportDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[ConfigurationStrings.MaxBufferPoolSize]; }
            set { base[ConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxBufferSize, DefaultValue = TransportDefaults.MaxBufferSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferSize
        {
            get { return (int)base[ConfigurationStrings.MaxBufferSize]; }
            set { base[ConfigurationStrings.MaxBufferSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxConnections, DefaultValue = ConnectionOrientedTransportDefaults.MaxPendingConnectionsConst)]
        [IntegerValidator(MinValue = 0)]
        public int MaxConnections
        {
            get { return (int)base[ConfigurationStrings.MaxConnections]; }
            set { base[ConfigurationStrings.MaxConnections] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[ConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[ConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PortSharingEnabled, DefaultValue = TcpTransportDefaults.PortSharingEnabled)]
        public bool PortSharingEnabled
        {
            get { return (bool)base[ConfigurationStrings.PortSharingEnabled]; }
            set { base[ConfigurationStrings.PortSharingEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReliableSession)]
        public StandardBindingOptionalReliableSessionElement ReliableSession
        {
            get { return (StandardBindingOptionalReliableSessionElement)base[ConfigurationStrings.ReliableSession]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public NetTcpSecurityElement Security
        {
            get { return (NetTcpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetTcpBinding nptBinding = (NetTcpBinding)binding;
            
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionFlow, nptBinding.TransactionFlow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, nptBinding.TransferMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionProtocol, nptBinding.TransactionProtocol);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, nptBinding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, nptBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, nptBinding.MaxBufferSize);
            if (nptBinding.IsMaxConnectionsSet)
            {
                ConfigurationProperty maxConnectionsProperty = this.Properties[ConfigurationStrings.MaxConnections];
                SetPropertyValue(maxConnectionsProperty, nptBinding.MaxConnections, false /*ignore locks*/);
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, nptBinding.MaxReceivedMessageSize);
            if (nptBinding.IsListenBacklogSet)
            {
                ConfigurationProperty listenBacklogProperty = this.Properties[ConfigurationStrings.ListenBacklog];
                SetPropertyValue(listenBacklogProperty, nptBinding.ListenBacklog, false /*ignore locks*/);
            }
            this.ReliableSession.InitializeFrom(nptBinding.ReliableSession);
            this.Security.InitializeFrom(nptBinding.Security);
            this.ReaderQuotas.InitializeFrom(nptBinding.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            NetTcpBinding nptBinding = (NetTcpBinding)binding;
            
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            nptBinding.TransactionFlow = this.TransactionFlow;
            nptBinding.TransferMode = this.TransferMode;
            nptBinding.TransactionProtocol = this.TransactionProtocol;
            nptBinding.HostNameComparisonMode = this.HostNameComparisonMode;
            if (this.ListenBacklog != TcpTransportDefaults.ListenBacklogConst)
            {
                nptBinding.ListenBacklog = this.ListenBacklog;                
            }
            nptBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                nptBinding.MaxBufferSize = this.MaxBufferSize;
            }
            if (this.MaxConnections != 0)
            {
                nptBinding.MaxConnections = this.MaxConnections;                
            }
            nptBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            nptBinding.PortSharingEnabled = this.PortSharingEnabled;
            this.ReliableSession.ApplyConfiguration(nptBinding.ReliableSession);
            this.Security.ApplyConfiguration(nptBinding.Security);
            this.ReaderQuotas.ApplyConfiguration(nptBinding.ReaderQuotas);
        }
    }
}
