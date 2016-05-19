//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;
    using System.ComponentModel;

    public class WebHttpBinding : Binding, IBindingRuntimePreferences
    {
        HttpsTransportBindingElement httpsTransportBindingElement;
        // private BindingElements
        HttpTransportBindingElement httpTransportBindingElement;
        WebHttpSecurity security = new WebHttpSecurity();
        WebMessageEncodingBindingElement webMessageEncodingBindingElement;

        public WebHttpBinding() : base()
        {
            Initialize();
        }

        public WebHttpBinding(string configurationName) : this()
        {
            ApplyConfiguration(configurationName);
        }

        public WebHttpBinding(WebHttpSecurityMode securityMode) : base()
        {
            Initialize();
            this.security.Mode = securityMode;
        }

        [DefaultValue(HttpTransportDefaults.AllowCookies)]
        public bool AllowCookies
        {
            get { return httpTransportBindingElement.AllowCookies; }
            set
            {
                httpTransportBindingElement.AllowCookies = value;
                httpsTransportBindingElement.AllowCookies = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return httpTransportBindingElement.BypassProxyOnLocal; }
            set
            {
                httpTransportBindingElement.BypassProxyOnLocal = value;
                httpsTransportBindingElement.BypassProxyOnLocal = value;
            }
        }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.None; }
        }

        [DefaultValue(HttpTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return httpTransportBindingElement.HostNameComparisonMode; }
            set
            {
                httpTransportBindingElement.HostNameComparisonMode = value;
                httpsTransportBindingElement.HostNameComparisonMode = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return httpTransportBindingElement.MaxBufferPoolSize; }
            set
            {
                httpTransportBindingElement.MaxBufferPoolSize = value;
                httpsTransportBindingElement.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get { return httpTransportBindingElement.MaxBufferSize; }
            set
            {
                httpTransportBindingElement.MaxBufferSize = value;
                httpsTransportBindingElement.MaxBufferSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return httpTransportBindingElement.MaxReceivedMessageSize; }
            set
            {
                httpTransportBindingElement.MaxReceivedMessageSize = value;
                httpsTransportBindingElement.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAddress)]
        public Uri ProxyAddress
        {
            get { return httpTransportBindingElement.ProxyAddress; }
            set
            {
                httpTransportBindingElement.ProxyAddress = value;
                httpsTransportBindingElement.ProxyAddress = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return webMessageEncodingBindingElement.ReaderQuotas; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                value.CopyTo(webMessageEncodingBindingElement.ReaderQuotas);
            }
        }

        public override string Scheme
        { get { return GetTransport().Scheme; } }

        public WebHttpSecurity Security
        {
            get { return this.security; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.security = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get { return httpTransportBindingElement.TransferMode; }
            set
            {
                httpTransportBindingElement.TransferMode = value;
                httpsTransportBindingElement.TransferMode = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get { return httpTransportBindingElement.UseDefaultWebProxy; }
            set
            {
                httpTransportBindingElement.UseDefaultWebProxy = value;
                httpsTransportBindingElement.UseDefaultWebProxy = value;
            }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding WriteEncoding
        {
            get { return webMessageEncodingBindingElement.WriteEncoding; }
            set
            {
                webMessageEncodingBindingElement.WriteEncoding = value;
            }
        }

        public WebContentTypeMapper ContentTypeMapper
        {
            get { return webMessageEncodingBindingElement.ContentTypeMapper; }
            set
            {
                webMessageEncodingBindingElement.ContentTypeMapper = value;
            }
        }

        public bool CrossDomainScriptAccessEnabled
        {
            get { return webMessageEncodingBindingElement.CrossDomainScriptAccessEnabled; }
            set
            {
                webMessageEncodingBindingElement.CrossDomainScriptAccessEnabled = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")] // [....], This is the pattern we use on the standard bindings in Indigo V1
        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            if ((security.Mode == WebHttpSecurityMode.Transport ||
                security.Mode == WebHttpSecurityMode.TransportCredentialOnly) &&
                security.Transport.ClientCredentialType == HttpClientCredentialType.InheritedFromHost)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.HttpClientCredentialTypeInvalid, security.Transport.ClientCredentialType)));
            }

            return base.BuildChannelFactory<TChannel>(parameters);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add encoding 
            bindingElements.Add(webMessageEncodingBindingElement);
            // add transport (http or https)
            bindingElements.Add(GetTransport());

            return bindingElements.Clone();
        }

        void ApplyConfiguration(string configurationName)
        {
            WebHttpBindingCollectionElement section = WebHttpBindingCollectionElement.GetBindingCollectionElement();
            WebHttpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR2.GetString(SR2.ConfigInvalidBindingConfigurationName,
                    configurationName,
                    WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        TransportBindingElement GetTransport()
        {
            if (security.Mode == WebHttpSecurityMode.Transport)
            {
                security.EnableTransportSecurity(httpsTransportBindingElement);
                return httpsTransportBindingElement;
            }
            else if (security.Mode == WebHttpSecurityMode.TransportCredentialOnly)
            {
                security.EnableTransportAuthentication(httpTransportBindingElement);
                return httpTransportBindingElement;
            }
            else
            {
                // ensure that there is no transport security
                security.DisableTransportAuthentication(httpTransportBindingElement);
                return httpTransportBindingElement;
            }
        }

        void Initialize()
        {
            httpTransportBindingElement = new HttpTransportBindingElement();
            httpsTransportBindingElement = new HttpsTransportBindingElement();
            httpTransportBindingElement.ManualAddressing = true;
            httpsTransportBindingElement.ManualAddressing = true;
            webMessageEncodingBindingElement = new WebMessageEncodingBindingElement();
            webMessageEncodingBindingElement.MessageVersion = MessageVersion.None;
        }

        internal static class WebHttpBindingConfigurationStrings
        {
            internal const string WebHttpBindingCollectionElementName = "webHttpBinding";
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeWriteEncoding()
        {
            return (this.WriteEncoding != TextEncoderDefaults.Encoding);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return Security.InternalShouldSerialize();
        }
    }
}
