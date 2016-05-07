//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;

    public partial class WebHttpBindingElement : StandardBindingElement
    {
        static readonly Type WebContentTypeMapperType = typeof(WebContentTypeMapper);

        ConfigurationPropertyCollection properties;

        public WebHttpBindingElement(string name)
            : base(name)
        {
        }

        public WebHttpBindingElement()
            : this(null)
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

        [SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule", MessageId = "System.ServiceModel.Configuration.WebHttpBindingElement.ProxyAddress", Justification = "The configuration system converts the config string to a Uri and vice versa")]
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

        [ConfigurationProperty(ConfigurationStrings.Security)]
        public WebHttpSecurityElement Security
        {
            get { return (WebHttpSecurityElement)base[ConfigurationStrings.Security]; }
        }

        [ConfigurationProperty(ConfigurationStrings.TransferMode, DefaultValue = WebHttpBindingDefaults.TransferMode)]
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

        [ConfigurationProperty(ConfigurationStrings.WriteEncoding, DefaultValue = TextEncoderDefaults.EncodingString)]
        [TypeConverter(typeof(EncodingConverter))]
        [WebEncodingValidator]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration104:ConfigurationValidatorAttributeRule", MessageId = "System.ServiceModel.Configuration.WebHttpBindingElement.WriteEncoding",
            Justification = "Bug with internal FxCop assembly flags this property as not having a validator.")]
        public Encoding WriteEncoding
        {
            get { return (Encoding)base[ConfigurationStrings.WriteEncoding]; }
            set { base[ConfigurationStrings.WriteEncoding] = value; }
        }

        [ConfigurationProperty(WebConfigurationStrings.ContentTypeMapper, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string ContentTypeMapper
        {
            get { return (string)base[WebConfigurationStrings.ContentTypeMapper]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[WebConfigurationStrings.ContentTypeMapper] = value;
            }
        }

        [ConfigurationProperty(WebConfigurationStrings.CrossDomainScriptAccessEnabled, DefaultValue = false)]
        public bool CrossDomainScriptAccessEnabled
        {
            get { return (bool)base[WebConfigurationStrings.CrossDomainScriptAccessEnabled]; }
            set
            {
                base[WebConfigurationStrings.CrossDomainScriptAccessEnabled] = value;
            }
        }

        protected override Type BindingElementType
        {
            get { return typeof(WebHttpBinding); }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("allowCookies", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bypassProxyOnLocal", typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("hostNameComparisonMode", typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferSize", typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxBufferPoolSize", typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("proxyAddress", typeof(System.Uri), HttpTransportDefaults.ProxyAddress, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("readerQuotas", typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("security", typeof(System.ServiceModel.Configuration.WebHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("writeEncoding", typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("transferMode", typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useDefaultWebProxy", typeof(System.Boolean), true, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("contentTypeMapper", typeof(string), string.Empty, null, new System.Configuration.StringValidator(0), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.CrossDomainScriptAccessEnabled, typeof(System.Boolean), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        internal protected override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            WebHttpBinding webBinding = (WebHttpBinding)binding;

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.BypassProxyOnLocal, webBinding.BypassProxyOnLocal);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.HostNameComparisonMode, webBinding.HostNameComparisonMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferSize, webBinding.MaxBufferSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxBufferPoolSize, webBinding.MaxBufferPoolSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxReceivedMessageSize, webBinding.MaxReceivedMessageSize);
            if (webBinding.ProxyAddress != null)
            {
                SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ProxyAddress, webBinding.ProxyAddress);
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.WriteEncoding, webBinding.WriteEncoding);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransferMode, webBinding.TransferMode);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.UseDefaultWebProxy, webBinding.UseDefaultWebProxy);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.AllowCookies, webBinding.AllowCookies);
            this.Security.InitializeFrom(webBinding.Security);
            this.InitializeReaderQuotas(webBinding.ReaderQuotas);
            SetPropertyValueIfNotDefaultValue(WebConfigurationStrings.CrossDomainScriptAccessEnabled, webBinding.CrossDomainScriptAccessEnabled);
        }

        internal void InitializeReaderQuotas(XmlDictionaryReaderQuotas readerQuotas)
        {
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("readerQuotas");
            }
            XmlDictionaryReaderQuotasElement thisQuotas = this.ReaderQuotas;

            // Can't call thisQuotas.InitializeFrom() because it's internal to System.ServiceModel.dll, so we duplicate the logic
            if (readerQuotas.MaxDepth != EncoderDefaults.MaxDepth && readerQuotas.MaxDepth != 0)
            {
                thisQuotas.MaxDepth = readerQuotas.MaxDepth;
            }
            if (readerQuotas.MaxStringContentLength != EncoderDefaults.MaxStringContentLength && readerQuotas.MaxStringContentLength != 0)
            {
                thisQuotas.MaxStringContentLength = readerQuotas.MaxStringContentLength;
            }
            if (readerQuotas.MaxArrayLength != EncoderDefaults.MaxArrayLength && readerQuotas.MaxArrayLength != 0)
            {
                thisQuotas.MaxArrayLength = readerQuotas.MaxArrayLength;
            }
            if (readerQuotas.MaxBytesPerRead != EncoderDefaults.MaxBytesPerRead && readerQuotas.MaxBytesPerRead != 0)
            {
                thisQuotas.MaxBytesPerRead = readerQuotas.MaxBytesPerRead;
            }
            if (readerQuotas.MaxNameTableCharCount != EncoderDefaults.MaxNameTableCharCount && readerQuotas.MaxNameTableCharCount != 0)
            {
                thisQuotas.MaxNameTableCharCount = readerQuotas.MaxNameTableCharCount;
            }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            WebHttpBinding webBinding = (WebHttpBinding)binding;

            webBinding.BypassProxyOnLocal = this.BypassProxyOnLocal;
            webBinding.HostNameComparisonMode = this.HostNameComparisonMode;
            webBinding.MaxBufferPoolSize = this.MaxBufferPoolSize;
            webBinding.MaxReceivedMessageSize = this.MaxReceivedMessageSize;

            webBinding.WriteEncoding = this.WriteEncoding;
            webBinding.TransferMode = this.TransferMode;
            webBinding.UseDefaultWebProxy = this.UseDefaultWebProxy;
            webBinding.AllowCookies = this.AllowCookies;
            if (this.ProxyAddress != null)
            {
                webBinding.ProxyAddress = this.ProxyAddress;
            }
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                webBinding.MaxBufferSize = this.MaxBufferSize;
            }
            webBinding.ContentTypeMapper = GetContentTypeMapper(this.ContentTypeMapper);
            webBinding.CrossDomainScriptAccessEnabled = this.CrossDomainScriptAccessEnabled;
            this.Security.ApplyConfiguration(webBinding.Security);
            ApplyReaderQuotasConfiguration(webBinding.ReaderQuotas, this.ReaderQuotas);
        }

        internal static WebContentTypeMapper GetContentTypeMapper(string contentTypeMapperType)
        {
            WebContentTypeMapper contentTypeMapper = null;
            if (!string.IsNullOrEmpty(contentTypeMapperType))
            {
                Type type = System.Type.GetType(contentTypeMapperType, true);
                if (!WebContentTypeMapperType.IsAssignableFrom(type))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR2.GetString(SR2.ConfigInvalidWebContentTypeMapperType, contentTypeMapperType, WebContentTypeMapperType.ToString())));
                }
                contentTypeMapper = (WebContentTypeMapper)Activator.CreateInstance(type);
            }
            return contentTypeMapper;
        }

        internal static void ApplyReaderQuotasConfiguration(XmlDictionaryReaderQuotas webBindingReaderQuotas, XmlDictionaryReaderQuotasElement elementReaderQuotas)
        {
            if (webBindingReaderQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("webBindingReaderQuotas");
            }
            if (elementReaderQuotas.MaxDepth != 0)
            {
                webBindingReaderQuotas.MaxDepth = elementReaderQuotas.MaxDepth;
            }
            if (elementReaderQuotas.MaxStringContentLength != 0)
            {
                webBindingReaderQuotas.MaxStringContentLength = elementReaderQuotas.MaxStringContentLength;
            }
            if (elementReaderQuotas.MaxArrayLength != 0)
            {
                webBindingReaderQuotas.MaxArrayLength = elementReaderQuotas.MaxArrayLength;
            }
            if (elementReaderQuotas.MaxBytesPerRead != 0)
            {
                webBindingReaderQuotas.MaxBytesPerRead = elementReaderQuotas.MaxBytesPerRead;
            }
            if (elementReaderQuotas.MaxNameTableCharCount != 0)
            {
                webBindingReaderQuotas.MaxNameTableCharCount = elementReaderQuotas.MaxNameTableCharCount;
            }
        }
    }
}
