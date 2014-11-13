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

    public class BasicHttpsBinding : HttpBindingBase
    {
        WSMessageEncoding messageEncoding = BasicHttpBindingDefaults.MessageEncoding;
        BasicHttpsSecurity basicHttpsSecurity;

        public BasicHttpsBinding() : this(BasicHttpsSecurity.DefaultMode) { }

        public BasicHttpsBinding(string configurationName) : this() 
        { 
            this.ApplyConfiguration(configurationName); 
        }

        public BasicHttpsBinding(BasicHttpsSecurityMode securityMode)
            : base()
        {
            this.basicHttpsSecurity = new BasicHttpsSecurity();
            this.basicHttpsSecurity.Mode = securityMode;
        }

        [DefaultValue(WSMessageEncoding.Text)]
        public WSMessageEncoding MessageEncoding
        {
            get 
            {
                return this.messageEncoding;
            }

            set 
            {
                this.messageEncoding = value;
            }
        }

        public BasicHttpsSecurity Security
        {
            get
            {
                return this.basicHttpsSecurity;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.basicHttpsSecurity = value;
            }
        }

        internal override BasicHttpSecurity BasicHttpSecurity
        {
            get 
            {
                return this.basicHttpsSecurity.BasicHttpSecurity;
            }
        }

        internal override EnvelopeVersion GetEnvelopeVersion()
        {
            return EnvelopeVersion.Soap11;
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            // Default Security mode here is different from that of BasicHttpBinding. Therefore, we call into the BasicHttpsSecurity.InternalShouldSerialize() here.
            return this.Security.InternalShouldSerialize();
        }

        void ApplyConfiguration(string configurationName)
        {
            BasicHttpsBindingCollectionElement section = BasicHttpsBindingCollectionElement.GetBindingCollectionElement();
            BasicHttpsBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ConfigurationErrorsException(
                        SR.GetString(
                                SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.BasicHttpsBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }
    }
}
