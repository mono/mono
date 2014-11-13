//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public class BasicHttpBinding : HttpBindingBase
    {
        WSMessageEncoding messageEncoding = BasicHttpBindingDefaults.MessageEncoding;
        BasicHttpSecurity basicHttpSecurity;

        public BasicHttpBinding() : this(BasicHttpSecurityMode.None) { }

        public BasicHttpBinding(string configurationName) : base() 
        {
            this.Initialize();
            this.ApplyConfiguration(configurationName);
        }

        public BasicHttpBinding(BasicHttpSecurityMode securityMode)
            : base()
        {
            this.Initialize();
            this.basicHttpSecurity.Mode = securityMode;
        }

        BasicHttpBinding(BasicHttpSecurity security)
            : base()
        {
            this.Initialize();
            this.basicHttpSecurity = security;
        }

        [DefaultValue(WSMessageEncoding.Text)]
        public WSMessageEncoding MessageEncoding
        {
            get { return messageEncoding; }
            set { messageEncoding = value; }
        }

        public BasicHttpSecurity Security
        {
            get { return this.basicHttpSecurity; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.basicHttpSecurity = value;
            }
        }

        [Obsolete(HttpChannelUtilities.ObsoleteDescriptionStrings.PropertyObsoleteUseAllowCookies, false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool EnableHttpCookieContainer 
        {
            get
            {
                return this.AllowCookies;
            }
            set
            {
                this.AllowCookies = value;
            }
        }

        internal override BasicHttpSecurity BasicHttpSecurity
        {
            get 
            {
                return this.basicHttpSecurity;
            }
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on BasicHttpBinding 
        // match default values of the binding elements
        bool IsBindingElementsMatch(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                if (!this.TextMessageEncodingBindingElement.IsMatch(encoding))
                    return false;
            }
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
            {
                if (!this.MtomMessageEncodingBindingElement.IsMatch(encoding))
                    return false;
            }
            if (!this.GetTransport().IsMatch(transport))
                return false;

            return true;
        }

        internal override EnvelopeVersion GetEnvelopeVersion()
        {
            return EnvelopeVersion.Soap11;
        }

        internal override void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            base.InitializeFrom(transport, encoding);
            // BasicHttpBinding only supports Text and Mtom encoding
            if (encoding is TextMessageEncodingBindingElement)
            {
                this.MessageEncoding = WSMessageEncoding.Text;
            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                messageEncoding = WSMessageEncoding.Mtom;
            }
        }

        void ApplyConfiguration(string configurationName)
        {
            BasicHttpBindingCollectionElement section = BasicHttpBindingCollectionElement.GetBindingCollectionElement();
            BasicHttpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.BasicHttpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            if ((this.BasicHttpSecurity.Mode == BasicHttpSecurityMode.Transport ||
                this.BasicHttpSecurity.Mode == BasicHttpSecurityMode.TransportCredentialOnly) &&
                this.BasicHttpSecurity.Transport.ClientCredentialType == HttpClientCredentialType.InheritedFromHost)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.HttpClientCredentialTypeInvalid, this.BasicHttpSecurity.Transport.ClientCredentialType)));
            }

            return base.BuildChannelFactory<TChannel>(parameters);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            this.CheckSettings();

            // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add security (*optional)
            SecurityBindingElement wsSecurity = this.BasicHttpSecurity.CreateMessageSecurity();
            if (wsSecurity != null)
            {
                bindingElements.Add(wsSecurity);
            }
            // add encoding (text or mtom)
            WSMessageEncodingHelper.SyncUpEncodingBindingElementProperties(this.TextMessageEncodingBindingElement, this.MtomMessageEncodingBindingElement);
            if (this.MessageEncoding == WSMessageEncoding.Text)
                bindingElements.Add(this.TextMessageEncodingBindingElement);
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
                bindingElements.Add(this.MtomMessageEncodingBindingElement);
            // add transport (http or https)
            bindingElements.Add(this.GetTransport());

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 3)
                return false;

            SecurityBindingElement securityElement = null;
            MessageEncodingBindingElement encoding = null;
            HttpTransportBindingElement transport = null;

            foreach (BindingElement element in elements)
            {
                if (element is SecurityBindingElement)
                    securityElement = element as SecurityBindingElement;
                else if (element is TransportBindingElement)
                    transport = element as HttpTransportBindingElement;
                else if (element is MessageEncodingBindingElement)
                    encoding = element as MessageEncodingBindingElement;
                else
                    return false;
            }

            HttpsTransportBindingElement httpsTransport = transport as HttpsTransportBindingElement;
            if ( ( securityElement != null ) && ( httpsTransport != null ) && ( httpsTransport.RequireClientCertificate != TransportDefaults.RequireClientCertificate ) )
            {
                return false;
            }

            // process transport binding element
            UnifiedSecurityMode mode;
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            if (!GetSecurityModeFromTransport(transport, transportSecurity, out mode))
                return false;
            if (encoding == null)
                return false;
            // BasicHttpBinding only supports Soap11
            if (!encoding.CheckEncodingVersion(EnvelopeVersion.Soap11))
                return false;

            BasicHttpSecurity security;
            if (!TryCreateSecurity(securityElement, mode, transportSecurity, out security))
                return false;

            BasicHttpBinding basicHttpBinding = new BasicHttpBinding(security);
            basicHttpBinding.InitializeFrom(transport, encoding);

            // make sure all our defaults match
            if (!basicHttpBinding.IsBindingElementsMatch(transport, encoding))
                return false;

            binding = basicHttpBinding;
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeEnableHttpCookieContainer()
        {
            return false;
        }

        void Initialize()
        {
            this.basicHttpSecurity = new BasicHttpSecurity();
        }
    }
}
