//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml;
    using System.ServiceModel.Description;
    using System.Configuration;
    using System.ComponentModel;
    using System.Runtime;

    public class WebScriptEndpointElement : StandardEndpointElement
    {
        ConfigurationPropertyCollection properties;

        public WebScriptEndpointElement() : base() { }

        protected internal override Type EndpointType 
        {
            get { return typeof(WebScriptEndpoint); }
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
            set { base[WebConfigurationStrings.CrossDomainScriptAccessEnabled] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.HostNameComparisonMode, typeof(System.ServiceModel.HostNameComparisonMode), System.ServiceModel.HostNameComparisonMode.StrongWildcard, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.HostNameComparisonModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.MaxBufferSize, typeof(System.Int32), 65536, null, new System.Configuration.IntegerValidator(1, 2147483647, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.MaxBufferPoolSize, typeof(System.Int64), (long)524288, null, new System.Configuration.LongValidator(0, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.MaxReceivedMessageSize, typeof(System.Int64), (long)65536, null, new System.Configuration.LongValidator(1, 9223372036854775807, false), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.ReaderQuotas, typeof(System.ServiceModel.Configuration.XmlDictionaryReaderQuotasElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.Security, typeof(System.ServiceModel.Configuration.WebHttpSecurityElement), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.WriteEncoding, typeof(System.Text.Encoding), "utf-8", new System.ServiceModel.Configuration.EncodingConverter(), null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(ConfigurationStrings.TransferMode, typeof(System.ServiceModel.TransferMode), System.ServiceModel.TransferMode.Buffered, null, new System.ServiceModel.Configuration.ServiceModelEnumValidator(typeof(System.ServiceModel.TransferModeHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.ContentTypeMapper, typeof(string), string.Empty, null, new System.Configuration.StringValidator(0), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.CrossDomainScriptAccessEnabled, typeof(bool), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            return new WebScriptEndpoint(contractDescription);
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (string.IsNullOrEmpty(channelEndpointElement.Binding))
            {
                channelEndpointElement.Binding = WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName;
            }
            else if (!string.Equals(channelEndpointElement.Binding, WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName, StringComparison.Ordinal))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.WebEndpointRequiredBinding, typeof(WebScriptEndpoint).Name, WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName)));
            }    
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (string.IsNullOrEmpty(serviceEndpointElement.Binding))
            {
                serviceEndpointElement.Binding = WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName;
            }
            else if (!string.Equals(serviceEndpointElement.Binding, WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName, StringComparison.Ordinal))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.WebEndpointRequiredBinding, typeof(WebScriptEndpoint).Name, WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName)));
            }
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            InternalOnApplyConfiguration(endpoint);
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement serviceEndpointElement)
        {
            InternalOnApplyConfiguration(endpoint);
        }

        void InternalOnApplyConfiguration(ServiceEndpoint endpoint)
        {
            WebScriptEndpoint webScriptEndpoint = endpoint as WebScriptEndpoint;
            Fx.Assert(webScriptEndpoint != null, "The endpoint should be of type WebScriptEnablingServiceEndpoint since this is what was returned with CreateServiceEndpoint().");
            
            if (IsSet(ConfigurationStrings.HostNameComparisonMode))
            {
                webScriptEndpoint.HostNameComparisonMode = this.HostNameComparisonMode;
            }
            if (IsSet(ConfigurationStrings.MaxBufferPoolSize))
            {
                webScriptEndpoint.MaxBufferPoolSize = this.MaxBufferPoolSize;
            }
            if (IsSet(ConfigurationStrings.MaxReceivedMessageSize))
            {
                webScriptEndpoint.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            }
            if (IsSet(ConfigurationStrings.WriteEncoding))
            {
                webScriptEndpoint.WriteEncoding = this.WriteEncoding;
            }
            if (IsSet(ConfigurationStrings.TransferMode))
            {
                webScriptEndpoint.TransferMode = this.TransferMode;
            }
            if (IsSet(WebConfigurationStrings.CrossDomainScriptAccessEnabled))
            {
                webScriptEndpoint.CrossDomainScriptAccessEnabled = this.CrossDomainScriptAccessEnabled;
            }
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.MaxBufferSize].ValueOrigin != PropertyValueOrigin.Default)
            {
                webScriptEndpoint.MaxBufferSize = this.MaxBufferSize;
            }
            if (IsSet(WebConfigurationStrings.ContentTypeMapper))
            {
                webScriptEndpoint.ContentTypeMapper = WebHttpBindingElement.GetContentTypeMapper(this.ContentTypeMapper);
            }
            this.Security.ApplyConfiguration(webScriptEndpoint.Security);
            WebHttpBindingElement.ApplyReaderQuotasConfiguration(webScriptEndpoint.ReaderQuotas, this.ReaderQuotas);
        }

        bool IsSet(string propertyName)
        {
            return this.ElementInformation.Properties[propertyName].IsModified;
        }
    }
}
