//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security;

    using System.Xml;
    using System.ComponentModel;

    public class WSDualHttpBinding : Binding, IBindingRuntimePreferences
    {
        WSMessageEncoding messageEncoding;
        ReliableSession reliableSession;
        // private BindingElements
        HttpTransportBindingElement httpTransport;
        TextMessageEncodingBindingElement textEncoding;
        MtomMessageEncodingBindingElement mtomEncoding;
        TransactionFlowBindingElement txFlow;
        ReliableSessionBindingElement session;
        CompositeDuplexBindingElement compositeDuplex;
        OneWayBindingElement oneWay;
        WSDualHttpSecurity security = new WSDualHttpSecurity();

        public WSDualHttpBinding(string configName)
            : this()
        {
            ApplyConfiguration(configName);
        }

        public WSDualHttpBinding(WSDualHttpSecurityMode securityMode)
            : this()
        {
            this.security.Mode = securityMode;
        }

        public WSDualHttpBinding()
        {
            Initialize();
        }

        WSDualHttpBinding(
            HttpTransportBindingElement transport,
            MessageEncodingBindingElement encoding,
            TransactionFlowBindingElement txFlow,
            ReliableSessionBindingElement session,
            CompositeDuplexBindingElement compositeDuplex,
            OneWayBindingElement oneWay,
            WSDualHttpSecurity security)
            : this()
        {
            this.security = security;
            InitializeFrom(transport, encoding, txFlow, session, compositeDuplex, oneWay);
        }

        [DefaultValue(HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return httpTransport.BypassProxyOnLocal; }
            set { httpTransport.BypassProxyOnLocal = value; }
        }

        [DefaultValue(null)]
        public Uri ClientBaseAddress
        {
            get { return this.compositeDuplex.ClientBaseAddress; }
            set { this.compositeDuplex.ClientBaseAddress = value; }
        }

        [DefaultValue(false)]
        public bool TransactionFlow
        {
            get { return this.txFlow.Transactions; }
            set { this.txFlow.Transactions = value; }
        }

        [DefaultValue(HttpTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return httpTransport.HostNameComparisonMode; }
            set { httpTransport.HostNameComparisonMode = value; }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return httpTransport.MaxBufferPoolSize; }
            set
            {
                httpTransport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return httpTransport.MaxReceivedMessageSize; }
            set
            {
                if (value > int.MaxValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("value.MaxReceivedMessageSize",
                        SR.GetString(SR.MaxReceivedMessageSizeMustBeInIntegerRange)));
                }
                httpTransport.MaxReceivedMessageSize = value;
                mtomEncoding.MaxBufferSize = (int)value;
            }
        }

        [DefaultValue(WSMessageEncoding.Text)]
        public WSMessageEncoding MessageEncoding
        {
            get { return messageEncoding; }
            set { messageEncoding = value; }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAddress)]
        public Uri ProxyAddress
        {
            get { return httpTransport.ProxyAddress; }
            set { httpTransport.ProxyAddress = value; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return textEncoding.ReaderQuotas; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                value.CopyTo(textEncoding.ReaderQuotas);
                value.CopyTo(mtomEncoding.ReaderQuotas);
            }
        }

        public ReliableSession ReliableSession
        {
            get
            {
                return reliableSession;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.reliableSession.CopySettings(value);
            }
        }

        public override string Scheme { get { return httpTransport.Scheme; } }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.Soap12; }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public System.Text.Encoding TextEncoding
        {
            get { return textEncoding.WriteEncoding; }
            set
            {
                textEncoding.WriteEncoding = value;
                mtomEncoding.WriteEncoding = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get { return httpTransport.UseDefaultWebProxy; }
            set { httpTransport.UseDefaultWebProxy = value; }
        }

        public WSDualHttpSecurity Security
        {
            get { return this.security; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.security = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            TransactionFlowBindingElement tfbe = new TransactionFlowBindingElement(false);
            tfbe.TransactionProtocol = TransactionProtocol.WSAtomicTransactionOctober2004;
            return tfbe;
        }

        void Initialize()
        {
            this.httpTransport = new HttpTransportBindingElement();
            this.messageEncoding = WSDualHttpBindingDefaults.MessageEncoding;
            this.txFlow = GetDefaultTransactionFlowBindingElement();
            this.session = new ReliableSessionBindingElement(true);
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            this.compositeDuplex = new CompositeDuplexBindingElement();
            this.reliableSession = new ReliableSession(session);
            this.oneWay = new OneWayBindingElement();
        }

        void InitializeFrom(
            HttpTransportBindingElement transport,
            MessageEncodingBindingElement encoding,
            TransactionFlowBindingElement txFlow,
            ReliableSessionBindingElement session,
            CompositeDuplexBindingElement compositeDuplex,
            OneWayBindingElement oneWay)
        {
            // transport
            this.BypassProxyOnLocal = transport.BypassProxyOnLocal;
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ProxyAddress = transport.ProxyAddress;
            this.UseDefaultWebProxy = transport.UseDefaultWebProxy;

            // this binding only supports Text and Mtom encoding
            if (encoding is TextMessageEncodingBindingElement)
            {
                this.MessageEncoding = WSMessageEncoding.Text;
                TextMessageEncodingBindingElement text = (TextMessageEncodingBindingElement)encoding;
                this.TextEncoding = text.WriteEncoding;
                this.ReaderQuotas = text.ReaderQuotas;

            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                messageEncoding = WSMessageEncoding.Mtom;
                MtomMessageEncodingBindingElement mtom = (MtomMessageEncodingBindingElement)encoding;
                this.TextEncoding = mtom.WriteEncoding;
                this.ReaderQuotas = mtom.ReaderQuotas;
            }
            this.TransactionFlow = txFlow.Transactions;
            this.ClientBaseAddress = compositeDuplex.ClientBaseAddress;

            //session
            if (session != null)
            {
                // only set properties that have standard binding manifestations
                this.session.InactivityTimeout = session.InactivityTimeout;
                this.session.Ordered = session.Ordered;
            }
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on WsHttpBinding 
        // match default values of the binding elements
        bool IsBindingElementsMatch(HttpTransportBindingElement transport,
            MessageEncodingBindingElement encoding,
            TransactionFlowBindingElement txFlow,
            ReliableSessionBindingElement session,
            CompositeDuplexBindingElement compositeDuplex,
            OneWayBindingElement oneWay)
        {
            if (!this.httpTransport.IsMatch(transport))
                return false;

            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                if (!this.textEncoding.IsMatch(encoding))
                    return false;
            }
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
            {
                if (!this.mtomEncoding.IsMatch(encoding))
                    return false;
            }
            if (!this.txFlow.IsMatch(txFlow))
                return false;
            if (!this.session.IsMatch(session))
                return false;
            if (!this.compositeDuplex.IsMatch(compositeDuplex))
                return false;

            if (!this.oneWay.IsMatch(oneWay))
            {
                return false;
            }

            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            WSDualHttpBindingCollectionElement section = WSDualHttpBindingCollectionElement.GetBindingCollectionElement();
            WSDualHttpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.WSDualHttpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        SecurityBindingElement CreateMessageSecurity()
        {
            return this.Security.CreateMessageSecurity();
        }

        static bool TryCreateSecurity(SecurityBindingElement securityElement, out WSDualHttpSecurity security)
        {
            return WSDualHttpSecurity.TryCreate(securityElement, out security);
        }

        public override BindingElementCollection CreateBindingElements()
        {   // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add context
            bindingElements.Add(txFlow);
            // add session
            bindingElements.Add(session);
            // add security (optional)
            SecurityBindingElement wsSecurity = CreateMessageSecurity();
            if (wsSecurity != null)
            {
                bindingElements.Add(wsSecurity);
            }

            // add duplex
            bindingElements.Add(compositeDuplex);

            // add oneWay adapter
            bindingElements.Add(oneWay);

            // add encoding (text or mtom)
            WSMessageEncodingHelper.SyncUpEncodingBindingElementProperties(textEncoding, mtomEncoding);
            if (this.MessageEncoding == WSMessageEncoding.Text)
            {
                bindingElements.Add(textEncoding);
            }
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
            {
                bindingElements.Add(mtomEncoding);
            }

            // add transport
            bindingElements.Add(httpTransport);

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 7)
            {
                return false;
            }

            SecurityBindingElement sbe = null;
            HttpTransportBindingElement transport = null;
            MessageEncodingBindingElement encoding = null;
            TransactionFlowBindingElement txFlow = null;
            ReliableSessionBindingElement session = null;
            CompositeDuplexBindingElement compositeDuplex = null;
            OneWayBindingElement oneWay = null;

            foreach (BindingElement element in elements)
            {
                if (element is SecurityBindingElement)
                {
                    sbe = element as SecurityBindingElement;
                }
                else if (element is TransportBindingElement)
                {
                    transport = element as HttpTransportBindingElement;
                }
                else if (element is MessageEncodingBindingElement)
                {
                    encoding = element as MessageEncodingBindingElement;
                }
                else if (element is TransactionFlowBindingElement)
                {
                    txFlow = element as TransactionFlowBindingElement;
                }
                else if (element is ReliableSessionBindingElement)
                {
                    session = element as ReliableSessionBindingElement;
                }
                else if (element is CompositeDuplexBindingElement)
                {
                    compositeDuplex = element as CompositeDuplexBindingElement;
                }
                else if (element is OneWayBindingElement)
                {
                    oneWay = element as OneWayBindingElement;
                }
                else
                {
                    return false;
                }
            }

            if (transport == null)
            {
                return false;
            }

            if (encoding == null)
            {
                return false;
            }

            // this binding only supports Soap12
            if (!encoding.CheckEncodingVersion(EnvelopeVersion.Soap12))
            {
                return false;
            }

            if (compositeDuplex == null)
            {
                return false;
            }

            if (oneWay == null)
            {
                return false;
            }

            if (session == null)
            {
                return false;
            }

            if (txFlow == null)
            {
                txFlow = GetDefaultTransactionFlowBindingElement();
            }

            WSDualHttpSecurity security;
            if (!TryCreateSecurity(sbe, out security))
                return false;

            WSDualHttpBinding wSDualHttpBinding =
                new WSDualHttpBinding(transport, encoding, txFlow, session, compositeDuplex, oneWay, security);

            if (!wSDualHttpBinding.IsBindingElementsMatch(transport, encoding, txFlow, session, compositeDuplex, oneWay))
            {
                return false;
            }

            binding = wSDualHttpBinding;
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTextEncoding()
        {
            return (!this.TextEncoding.Equals(TextEncoderDefaults.Encoding));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReliableSession()
        {
            return this.ReliableSession.Ordered != ReliableSessionDefaults.Ordered
                || this.ReliableSession.InactivityTimeout != ReliableSessionDefaults.InactivityTimeout;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }
    }
}
