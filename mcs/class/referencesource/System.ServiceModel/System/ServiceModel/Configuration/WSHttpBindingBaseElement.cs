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

    public abstract partial class WSHttpBindingBaseElement : StandardBindingElement
    {
        protected WSHttpBindingBaseElement(string name)
            : base(name)
        {
        }

        protected WSHttpBindingBaseElement()
            : this(null)
        {
        }

        [ConfigurationProperty(ConfigurationStrings.BypassProxyOnLocal, DefaultValue = HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return (bool)base[ConfigurationStrings.BypassProxyOnLocal]; }
            set { base[ConfigurationStrings.BypassProxyOnLocal] = value; }
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

        [ConfigurationProperty(ConfigurationStrings.MessageEncoding, DefaultValue = WSHttpBindingDefaults.MessageEncoding)]
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
        public StandardBindingOptionalReliableSessionElement ReliableSession
        {
            get { return (StandardBindingOptionalReliableSessionElement)base[ConfigurationStrings.ReliableSession]; }
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
            WSHttpBindingBase wspBinding = (WSHttpBindingBase)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BypassProxyOnLocal, wspBinding.BypassProxyOnLocal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransactionFlow, wspBinding.TransactionFlow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, wspBinding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, wspBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, wspBinding.MaxReceivedMessageSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MessageEncoding, wspBinding.MessageEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAddress, wspBinding.ProxyAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TextEncoding, wspBinding.TextEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseDefaultWebProxy, wspBinding.UseDefaultWebProxy);
            this.ReaderQuotas.InitializeFrom(wspBinding.ReaderQuotas);
            this.ReliableSession.InitializeFrom(wspBinding.ReliableSession);
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            WSHttpBindingBase wspBinding = (WSHttpBindingBase)binding;

            wspBinding.BypassProxyOnLocal = this.BypassProxyOnLocal;
            wspBinding.TransactionFlow = this.TransactionFlow;
            wspBinding.HostNameComparisonMode = this.HostNameComparisonMode;
            wspBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            wspBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            wspBinding.MessageEncoding = this.MessageEncoding;
            if (this.ProxyAddress != null)
                wspBinding.ProxyAddress = this.ProxyAddress;
            wspBinding.TextEncoding = this.TextEncoding;
            wspBinding.UseDefaultWebProxy = this.UseDefaultWebProxy;
            this.ReaderQuotas.ApplyConfiguration(wspBinding.ReaderQuotas);
            this.ReliableSession.ApplyConfiguration(wspBinding.ReliableSession);
        }
    }

}
