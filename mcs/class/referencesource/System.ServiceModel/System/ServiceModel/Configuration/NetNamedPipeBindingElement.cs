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

    public partial class NetNamedPipeBindingElement : StandardBindingElement
    {
        public NetNamedPipeBindingElement(string name)
            : base(name)
        {
        }

        public NetNamedPipeBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetNamedPipeBinding); }
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

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public NetNamedPipeSecurityElement Security
        {
            get { return (NetNamedPipeSecurityElement)base[ConfigurationStrings.Security]; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            NetNamedPipeBinding npnpBinding = (NetNamedPipeBinding)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionFlow, npnpBinding.TransactionFlow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, npnpBinding.TransferMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionProtocol, npnpBinding.TransactionProtocol);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, npnpBinding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, npnpBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, npnpBinding.MaxBufferSize);
            if (npnpBinding.IsMaxConnectionsSet)
            {
                ConfigurationProperty maxConnectionsProperty = this.Properties[ConfigurationStrings.MaxConnections];
                SetPropertyValue(maxConnectionsProperty, npnpBinding.MaxConnections, false /*ignore locks*/);
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, npnpBinding.MaxReceivedMessageSize);
            this.Security.InitializeFrom(npnpBinding.Security);
            this.ReaderQuotas.InitializeFrom(npnpBinding.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            NetNamedPipeBinding npnpBinding = (NetNamedPipeBinding)binding;

            npnpBinding.TransactionFlow = this.TransactionFlow;
            npnpBinding.TransferMode = this.TransferMode;
            npnpBinding.TransactionProtocol = this.TransactionProtocol;
            npnpBinding.HostNameComparisonMode = this.HostNameComparisonMode;
            npnpBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                npnpBinding.MaxBufferSize = this.MaxBufferSize;
            }
            if (this.MaxConnections != 0)
            {
                npnpBinding.MaxConnections = this.MaxConnections;
            }
            npnpBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            this.Security.ApplyConfiguration(npnpBinding.Security);
            this.ReaderQuotas.ApplyConfiguration(npnpBinding.ReaderQuotas);
        }
    }
}
