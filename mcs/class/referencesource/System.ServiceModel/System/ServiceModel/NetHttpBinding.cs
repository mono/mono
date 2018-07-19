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
    using System.Text;
    using System.Xml;

    public class NetHttpBinding : HttpBindingBase
    {
        BinaryMessageEncodingBindingElement binaryMessageEncodingBindingElement;
        ReliableSessionBindingElement session;
        OptionalReliableSession reliableSession;
        NetHttpMessageEncoding messageEncoding;
        BasicHttpSecurity basicHttpSecurity;

        public NetHttpBinding()
            : this(BasicHttpSecurityMode.None)
        {
        }

        public NetHttpBinding(BasicHttpSecurityMode securityMode)
            : base()
        {
            this.Initialize();
            this.basicHttpSecurity.Mode = securityMode;
        }

        public NetHttpBinding(BasicHttpSecurityMode securityMode, bool reliableSessionEnabled)
            : this(securityMode)
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        public NetHttpBinding(string configurationName)
            : base()
        {
            this.Initialize();
            this.ApplyConfiguration(configurationName);
        }

        NetHttpBinding(BasicHttpSecurity security)
            : base()
        {
            this.Initialize();
            this.basicHttpSecurity = security;
        }

        [DefaultValue(NetHttpMessageEncoding.Binary)]
        public NetHttpMessageEncoding MessageEncoding
        {
            get { return this.messageEncoding; }
            set { this.messageEncoding = value; }
        }        

        public BasicHttpSecurity Security
        {
            get
            {
                return this.basicHttpSecurity;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.basicHttpSecurity = value;
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

        internal override BasicHttpSecurity BasicHttpSecurity
        {
            get
            {
                return this.basicHttpSecurity;
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

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 4)
            {
                return false;
            }

            ReliableSessionBindingElement session = null;
            SecurityBindingElement securityElement = null;
            MessageEncodingBindingElement encoding = null;
            HttpTransportBindingElement transport = null;

            foreach (BindingElement element in elements)
            {
                if (element is ReliableSessionBindingElement)
                {
                    session = element as ReliableSessionBindingElement;
                }

                if (element is SecurityBindingElement)
                {
                    securityElement = element as SecurityBindingElement;
                }
                else if (element is TransportBindingElement)
                {
                    transport = element as HttpTransportBindingElement;
                }
                else if (element is MessageEncodingBindingElement)
                {
                    encoding = element as MessageEncodingBindingElement;
                }
                else
                {
                    return false;
                }
            }

            if (transport == null || transport.WebSocketSettings.TransportUsage != WebSocketTransportUsage.Always)
            {
                return false;
            }

            HttpsTransportBindingElement httpsTransport = transport as HttpsTransportBindingElement;
            if ((securityElement != null) && (httpsTransport != null) && (httpsTransport.RequireClientCertificate != TransportDefaults.RequireClientCertificate))
            {
                return false;
            }

            // process transport binding element
            UnifiedSecurityMode mode;
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            if (!GetSecurityModeFromTransport(transport, transportSecurity, out mode))
            {
                return false;
            }

            if (encoding == null)
            {
                return false;
            }

            if (!(encoding is TextMessageEncodingBindingElement || encoding is MtomMessageEncodingBindingElement || encoding is BinaryMessageEncodingBindingElement))
            {
                return false;
            }

            if (encoding.MessageVersion != MessageVersion.Soap12WSAddressing10)
            {
                return false;
            }

            BasicHttpSecurity security;
            if (!TryCreateSecurity(securityElement, mode, transportSecurity, out security))
            {
                return false;
            }

            NetHttpBinding netHttpBinding = new NetHttpBinding(security);
            netHttpBinding.InitializeFrom(transport, encoding, session);

            // make sure all our defaults match
            if (!netHttpBinding.IsBindingElementsMatch(transport, encoding, session))
            {
                return false;
            }

            binding = netHttpBinding;
            return true;
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
            this.WebSocketSettings.TransportUsage = NetHttpBindingDefaults.TransportUsage;
            this.WebSocketSettings.SubProtocol = WebSocketTransportSettings.SoapSubProtocol;
            this.basicHttpSecurity = new BasicHttpSecurity();
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

        void ApplyConfiguration(string configurationName)
        {
            NetHttpBindingCollectionElement section = NetHttpBindingCollectionElement.GetBindingCollectionElement();
            NetHttpBindingElement element = section.Bindings[configurationName];
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
    }
}
