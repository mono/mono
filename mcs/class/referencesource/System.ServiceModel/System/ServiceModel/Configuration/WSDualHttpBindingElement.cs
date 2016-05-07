//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.ServiceModel.Security;
    using System.ComponentModel;
    using System.Text;
    using System.ServiceModel.Channels;

    public partial class WSDualHttpBindingElement : StandardBindingElement
    {
        public WSDualHttpBindingElement(string name)
            : base(name)
        {
        }

        public WSDualHttpBindingElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(WSDualHttpBinding); }
        }

        [ConfigurationProperty(ConfigurationStrings.BypassProxyOnLocal, DefaultValue = HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return (bool)base[ConfigurationStrings.BypassProxyOnLocal]; }
            set { base[ConfigurationStrings.BypassProxyOnLocal] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ClientBaseAddress, DefaultValue = null)]
        public Uri ClientBaseAddress
        {
            get { return (Uri)base[ConfigurationStrings.ClientBaseAddress]; }
            set { base[ConfigurationStrings.ClientBaseAddress] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransactionFlow, DefaultValue = TransactionFlowDefaults.Transactions)]
        public bool TransactionFlow
        {
            get { return (bool)base[ConfigurationStrings.TransactionFlow]; }
            set { base[ConfigurationStrings.TransactionFlow] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.HostNameComparisonMode, DefaultValue = HttpTransportDefaults.HostNameComparisonMode)]
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

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[ConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[ConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MessageEncoding, DefaultValue = WSDualHttpBindingDefaults.MessageEncoding)]
        [ServiceModelEnumValidator(typeof(WSMessageEncodingHelper))]
        public WSMessageEncoding MessageEncoding
        {
            get { return (WSMessageEncoding)base[ConfigurationStrings.MessageEncoding]; }
            set { base[ConfigurationStrings.MessageEncoding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ProxyAddress, DefaultValue = HttpTransportDefaults.ProxyAddress)]
        public Uri ProxyAddress
        {
            get { return (Uri)base[ConfigurationStrings.ProxyAddress]; }
            set { base[ConfigurationStrings.ProxyAddress] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReaderQuotas)]
        public XmlDictionaryReaderQuotasElement ReaderQuotas
        {
            get { return (XmlDictionaryReaderQuotasElement)base[ConfigurationStrings.ReaderQuotas]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReliableSession)]
        public StandardBindingReliableSessionElement ReliableSession
        {
            get { return (StandardBindingReliableSessionElement)base[ConfigurationStrings.ReliableSession]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public WSDualHttpSecurityElement Security
        {
            get { return (WSDualHttpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        [ConfigurationProperty(ConfigurationStrings.TextEncoding, DefaultValue = TextEncoderDefaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get { return (Encoding)base[ConfigurationStrings.TextEncoding]; }
            set { base[ConfigurationStrings.TextEncoding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseDefaultWebProxy, DefaultValue = HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get { return (bool)base[ConfigurationStrings.UseDefaultWebProxy]; }
            set { base[ConfigurationStrings.UseDefaultWebProxy] = value; }
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WSDualHttpBinding wspBinding = (WSDualHttpBinding)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BypassProxyOnLocal, wspBinding.BypassProxyOnLocal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientBaseAddress, wspBinding.ClientBaseAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionFlow, wspBinding.TransactionFlow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, wspBinding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, wspBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, wspBinding.MaxReceivedMessageSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageEncoding, wspBinding.MessageEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAddress, wspBinding.ProxyAddress);
            this.ReliableSession.InitializeFrom(wspBinding.ReliableSession);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TextEncoding, wspBinding.TextEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseDefaultWebProxy, wspBinding.UseDefaultWebProxy);
            this.Security.InitializeFrom(wspBinding.Security);
            this.ReaderQuotas.InitializeFrom(wspBinding.ReaderQuotas);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            WSDualHttpBinding wspBinding = (WSDualHttpBinding)binding;

            wspBinding.BypassProxyOnLocal = this.BypassProxyOnLocal;
            if (this.ClientBaseAddress != null)
                wspBinding.ClientBaseAddress = this.ClientBaseAddress;
            wspBinding.TransactionFlow = this.TransactionFlow;
            wspBinding.HostNameComparisonMode = this.HostNameComparisonMode;
            wspBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            wspBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            wspBinding.MessageEncoding = this.MessageEncoding;
            if (this.ProxyAddress != null)
                wspBinding.ProxyAddress = this.ProxyAddress;
            this.ReliableSession.ApplyConfiguration(wspBinding.ReliableSession);
            wspBinding.TextEncoding = this.TextEncoding;
            wspBinding.UseDefaultWebProxy = this.UseDefaultWebProxy;
            this.Security.ApplyConfiguration(wspBinding.Security);
            this.ReaderQuotas.ApplyConfiguration(wspBinding.ReaderQuotas);
        }
    }
}
