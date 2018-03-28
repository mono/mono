// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public class NetHttpsBinding : HttpBindingBase
    {
        BinaryMessageEncodingBindingElement binaryMessageEncodingBindingElement;
        ReliableSessionBindingElement session;
        OptionalReliableSession reliableSession;
        NetHttpMessageEncoding messageEncoding;
        BasicHttpsSecurity basicHttpsSecurity;

        public NetHttpsBinding() 
            : this(BasicHttpsSecurity.DefaultMode) 
        { 
        }

        public NetHttpsBinding(BasicHttpsSecurityMode securityMode)
            : base()
        {
            this.Initialize();

            this.basicHttpsSecurity.Mode = securityMode;
        }

        public NetHttpsBinding(BasicHttpsSecurityMode securityMode, bool reliableSessionEnabled)
            : this(securityMode)
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        public NetHttpsBinding(string configurationName)
            : base()
        {
            this.Initialize();
            this.ApplyConfiguration(configurationName);
        }

        [DefaultValue(NetHttpMessageEncoding.Binary)]
        public NetHttpMessageEncoding MessageEncoding
        {
            get { return this.messageEncoding; }
            set { this.messageEncoding = value; }
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

        public OptionalReliableSession ReliableSession
        {
            get
            {
                return this.reliableSession;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.reliableSession.CopySettings(value);
            }
        }

        public WebSocketTransportSettings WebSocketSettings
        {
            get
            {
                return this.InternalWebSocketSettings;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            if ((this.BasicHttpSecurity.Mode == BasicHttpSecurityMode.Transport ||
                this.BasicHttpSecurity.Mode == BasicHttpSecurityMode.TransportCredentialOnly) &&
                this.BasicHttpSecurity.Transport.ClientCredentialType == HttpClientCredentialType.InheritedFromHost)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpClientCredentialTypeInvalid, this.BasicHttpSecurity.Transport.ClientCredentialType)));
            }

            return base.BuildChannelFactory<TChannel>(parameters);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            this.CheckSettings();

            // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();

            // order of BindingElements is important
            // add session
            if (this.reliableSession.Enabled)
            {
                bindingElements.Add(this.session);
            }

            // add security (*optional)
            SecurityBindingElement messageSecurity = this.BasicHttpSecurity.CreateMessageSecurity();
            if (messageSecurity != null)
            {
                bindingElements.Add(messageSecurity);
            }

            // add encoding
            switch (this.MessageEncoding)
            {
                case NetHttpMessageEncoding.Text:
                    bindingElements.Add(this.TextMessageEncodingBindingElement);
                    break;
                case NetHttpMessageEncoding.Mtom:
                    bindingElements.Add(this.MtomMessageEncodingBindingElement);
                    break;
                default:
                    bindingElements.Add(this.binaryMessageEncodingBindingElement);
                    break;
            }

            // add transport (http or https)
            bindingElements.Add(this.GetTransport());

            return bindingElements.Clone();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReliableSession()
        {
            return this.ReliableSession.Ordered != ReliableSessionDefaults.Ordered
                || this.ReliableSession.InactivityTimeout != ReliableSessionDefaults.InactivityTimeout
                || this.ReliableSession.Enabled != ReliableSessionDefaults.Enabled;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }

        internal override void SetReaderQuotas(XmlDictionaryReaderQuotas readerQuotas)
        {
            readerQuotas.CopyTo(this.binaryMessageEncodingBindingElement.ReaderQuotas);
        }

        internal override EnvelopeVersion GetEnvelopeVersion()
        {
            return EnvelopeVersion.Soap12;
        }

        internal override void CheckSettings()
        {
            base.CheckSettings();

            // In the Win8 profile, Mtom is not supported.
            if ((this.MessageEncoding == NetHttpMessageEncoding.Mtom) && UnsafeNativeMethods.IsTailoredApplication.Value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedBindingProperty, "MessageEncoding", this.MessageEncoding)));
            }
        }

        void Initialize()
        {
            this.messageEncoding = NetHttpBindingDefaults.MessageEncoding;            
            this.binaryMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement() { MessageVersion = MessageVersion.Soap12WSAddressing10 };
            this.TextMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.MtomMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.session = new ReliableSessionBindingElement();
            this.reliableSession = new OptionalReliableSession(this.session);
            this.InternalWebSocketSettings.TransportUsage = NetHttpBindingDefaults.TransportUsage;
            this.InternalWebSocketSettings.SubProtocol = WebSocketTransportSettings.SoapSubProtocol;
            this.basicHttpsSecurity = new BasicHttpsSecurity();
        }

        void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, ReliableSessionBindingElement session)
        {
            this.InitializeFrom(transport, encoding);
            if (encoding is BinaryMessageEncodingBindingElement)
            {
                this.messageEncoding = NetHttpMessageEncoding.Binary;
                BinaryMessageEncodingBindingElement binary = (BinaryMessageEncodingBindingElement)encoding;
                this.ReaderQuotas = binary.ReaderQuotas;
            }

            if (encoding is TextMessageEncodingBindingElement)
            {
                this.messageEncoding = NetHttpMessageEncoding.Text;
            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                this.messageEncoding = NetHttpMessageEncoding.Mtom;
            }

            if (session != null)
            {
                // only set properties that have standard binding manifestations
                this.session.InactivityTimeout = session.InactivityTimeout;
                this.session.Ordered = session.Ordered;
            }
        }

        bool IsBindingElementsMatch(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, ReliableSessionBindingElement session)
        {
            if (this.reliableSession.Enabled)
            {
                if (!this.session.IsMatch(session))
                {
                    return false;
                }
            }
            else if (session != null)
            {
                return false;
            }

            switch (this.MessageEncoding)
            {
                case NetHttpMessageEncoding.Text:
                    if (!this.TextMessageEncodingBindingElement.IsMatch(encoding))
                    {
                        return false;
                    }

                    break;
                case NetHttpMessageEncoding.Mtom:
                    if (!this.MtomMessageEncodingBindingElement.IsMatch(encoding))
                    {
                        return false;
                    }

                    break;
                default:    // NetHttpMessageEncoding.Binary
                    if (!this.binaryMessageEncodingBindingElement.IsMatch(encoding))
                    {
                        return false;
                    }

                    break;
            }

            if (!this.GetTransport().IsMatch(transport))
            {
                return false;
            }

            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            NetHttpsBindingCollectionElement section = NetHttpsBindingCollectionElement.GetBindingCollectionElement();
            NetHttpsBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.GetString(
                                                   SR.ConfigInvalidBindingConfigurationName,
                                                   configurationName,
                                                   ConfigurationStrings.NetHttpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }
    }
}
