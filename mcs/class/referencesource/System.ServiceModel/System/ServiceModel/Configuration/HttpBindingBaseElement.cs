// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    /// <summary>
    /// HttpBindingBaseElement for HttpBindingBase
    /// </summary>
    public abstract partial class HttpBindingBaseElement : StandardBindingElement
    {
        protected HttpBindingBaseElement(string name)
            : base(name)
        {
        }

        [ConfigurationProperty(ConfigurationStrings.AllowCookies, DefaultValue = HttpTransportDefaults.AllowCookies)]
        public bool AllowCookies
        {
            get { return (bool)base[ConfigurationStrings.AllowCookies]; }
            set { base[ConfigurationStrings.AllowCookies] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.BypassProxyOnLocal, DefaultValue = HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return (bool)base[ConfigurationStrings.BypassProxyOnLocal]; }
            set { base[ConfigurationStrings.BypassProxyOnLocal] = value; }
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

        [ConfigurationProperty(ConfigurationStrings.MaxBufferSize, DefaultValue = TransportDefaults.MaxBufferSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxBufferSize
        {
            get { return (int)base[ConfigurationStrings.MaxBufferSize]; }
            set { base[ConfigurationStrings.MaxBufferSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, DefaultValue = TransportDefaults.MaxReceivedMessageSize)]
        [LongValidator(MinValue = 1)]
        public long MaxReceivedMessageSize
        {
            get { return (long)base[ConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[ConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Configuration, "Configuration104",
                            Justification = "This attribute comes from previous releases.")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Configuration, "Configuration104",
                            Justification = "This attribute comes from previous releases.")]
        [ConfigurationProperty(ConfigurationStrings.TextEncoding, DefaultValue = TextEncoderDefaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get { return (Encoding)base[ConfigurationStrings.TextEncoding]; }
            set { base[ConfigurationStrings.TextEncoding] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransferMode, DefaultValue = HttpTransportDefaults.TransferMode)]
        [ServiceModelEnumValidator(typeof(TransferModeHelper))]
        public TransferMode TransferMode
        {
            get { return (TransferMode)base[ConfigurationStrings.TransferMode]; }
            set { base[ConfigurationStrings.TransferMode] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.UseDefaultWebProxy, DefaultValue = HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get { return (bool)base[ConfigurationStrings.UseDefaultWebProxy]; }
            set { base[ConfigurationStrings.UseDefaultWebProxy] = value; }
        }

        // BasicHttpContextBinding uses this hook to not emit AllowCookies
        internal virtual void InitializeAllowCookies(HttpBindingBase binding)
        {
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AllowCookies, binding.AllowCookies);
        }

        protected internal override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            HttpBindingBase httpBindingBase = (HttpBindingBase)binding;
            this.InitializeAllowCookies(httpBindingBase);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BypassProxyOnLocal, httpBindingBase.BypassProxyOnLocal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, httpBindingBase.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, httpBindingBase.MaxBufferSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, httpBindingBase.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, httpBindingBase.MaxReceivedMessageSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAddress, httpBindingBase.ProxyAddress);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TextEncoding, httpBindingBase.TextEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, httpBindingBase.TransferMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseDefaultWebProxy, httpBindingBase.UseDefaultWebProxy);

            this.ReaderQuotas.InitializeFrom(httpBindingBase.ReaderQuotas);            
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            HttpBindingBase httpBindingBase = (HttpBindingBase)binding;

            httpBindingBase.BypassProxyOnLocal = this.BypassProxyOnLocal;
            httpBindingBase.HostNameComparisonMode = this.HostNameComparisonMode;
            httpBindingBase.MaxBufferPoolSize = this.MaxBufferPoolSize;
            httpBindingBase.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            httpBindingBase.TextEncoding = this.TextEncoding;
            httpBindingBase.TransferMode = this.TransferMode;
            httpBindingBase.UseDefaultWebProxy = this.UseDefaultWebProxy;
            httpBindingBase.AllowCookies = this.AllowCookies;
            if (this.ProxyAddress != null)
            {
                httpBindingBase.ProxyAddress = this.ProxyAddress;
            }

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                httpBindingBase.MaxBufferSize = this.MaxBufferSize;
            }

            this.ReaderQuotas.ApplyConfiguration(httpBindingBase.ReaderQuotas);
        }
    }
}
