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
    using System.ComponentModel;
    using System.Xml;

    public abstract class WSHttpBindingBase : Binding, IBindingRuntimePreferences
    {
        WSMessageEncoding messageEncoding;
        OptionalReliableSession reliableSession;
        // private BindingElements
        HttpTransportBindingElement httpTransport;
        HttpsTransportBindingElement httpsTransport;
        TextMessageEncodingBindingElement textEncoding;
        MtomMessageEncodingBindingElement mtomEncoding;
        TransactionFlowBindingElement txFlow;
        ReliableSessionBindingElement session;

        protected WSHttpBindingBase()
            : base()
        {
            Initialize();
        }

        protected WSHttpBindingBase(bool reliableSessionEnabled)
            : this()
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        [DefaultValue(HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get { return httpTransport.BypassProxyOnLocal; }
            set
            {
                httpTransport.BypassProxyOnLocal = value;
                httpsTransport.BypassProxyOnLocal = value;
            }
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
            set
            {
                httpTransport.HostNameComparisonMode = value;
                httpsTransport.HostNameComparisonMode = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return httpTransport.MaxBufferPoolSize; }
            set
            {
                httpTransport.MaxBufferPoolSize = value;
                httpsTransport.MaxBufferPoolSize = value;
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
                httpsTransport.MaxReceivedMessageSize = value;
                mtomEncoding.MaxBufferSize = (int)value;
            }
        }

        [DefaultValue(WSHttpBindingDefaults.MessageEncoding)]
        public WSMessageEncoding MessageEncoding
        {
            get { return messageEncoding; }
            set { messageEncoding = value; }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAddress)]
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri ProxyAddress
        {
            get { return httpTransport.ProxyAddress; }
            set
            {
                httpTransport.ProxyAddress = value;
                httpsTransport.ProxyAddress = value;
            }
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

        public OptionalReliableSession ReliableSession
        {
            get { return reliableSession; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.reliableSession.CopySettings(value);
            }
        }

        public override string Scheme { get { return GetTransport().Scheme; } }

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
            set
            {
                httpTransport.UseDefaultWebProxy = value;
                httpsTransport.UseDefaultWebProxy = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        internal HttpTransportBindingElement HttpTransport
        {
            get { return httpTransport; }
        }

        internal HttpsTransportBindingElement HttpsTransport
        {
            get { return httpsTransport; }
        }

        internal ReliableSessionBindingElement ReliableSessionBindingElement
        {
            get { return session; }
        }

        internal TransactionFlowBindingElement TransactionFlowBindingElement
        {
            get { return txFlow; }
        }

        static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            TransactionFlowBindingElement tfbe = new TransactionFlowBindingElement(false);
            tfbe.TransactionProtocol = TransactionProtocol.WSAtomicTransactionOctober2004;
            return tfbe;
        }

        void Initialize()
        {
            httpTransport = new HttpTransportBindingElement();
            httpsTransport = new HttpsTransportBindingElement();
            messageEncoding = WSHttpBindingDefaults.MessageEncoding;
            txFlow = GetDefaultTransactionFlowBindingElement();
            session = new ReliableSessionBindingElement(true);
            textEncoding = new TextMessageEncodingBindingElement();
            textEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            mtomEncoding = new MtomMessageEncodingBindingElement();
            mtomEncoding.MessageVersion = MessageVersion.Soap12WSAddressing10;
            reliableSession = new OptionalReliableSession(session);
        }

        void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, TransactionFlowBindingElement txFlow, ReliableSessionBindingElement session)
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
            this.reliableSession.Enabled = session != null;

            //session
            if (session != null)
            {
                // only set properties that have standard binding manifestations
                this.session.InactivityTimeout = session.InactivityTimeout;
                this.session.Ordered = session.Ordered;
            }
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on BasicHttpBinding 
        // match default values of the binding elements
        bool IsBindingElementsMatch(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding, TransactionFlowBindingElement txFlow, ReliableSessionBindingElement session)
        {

            if (!this.GetTransport().IsMatch(transport))
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

            if (reliableSession.Enabled)
            {
                if (!this.session.IsMatch(session))
                    return false;
            }
            else if (session != null)
            {
                return false;
            }

            return true;
        }

        public override BindingElementCollection CreateBindingElements()
        {   // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // context

            bindingElements.Add(txFlow);
            // reliable
            if (reliableSession.Enabled)
            {
                bindingElements.Add(session);
            }

            // add security (*optional)
            SecurityBindingElement wsSecurity = this.CreateMessageSecurity();
            if (wsSecurity != null)
            {
                bindingElements.Add(wsSecurity);
            }

            // add encoding (text or mtom)
            WSMessageEncodingHelper.SyncUpEncodingBindingElementProperties(textEncoding, mtomEncoding);
            if (this.MessageEncoding == WSMessageEncoding.Text)
                bindingElements.Add(textEncoding);
            else if (this.MessageEncoding == WSMessageEncoding.Mtom)
                bindingElements.Add(mtomEncoding);

            // add transport (http or https)
            bindingElements.Add(GetTransport());

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 6)
                return false;

            // collect all binding elements
            PrivacyNoticeBindingElement privacy = null;
            TransactionFlowBindingElement txFlow = null;
            ReliableSessionBindingElement session = null;
            SecurityBindingElement security = null;
            MessageEncodingBindingElement encoding = null;
            HttpTransportBindingElement transport = null;

            foreach (BindingElement element in elements)
            {
                if (element is SecurityBindingElement)
                    security = element as SecurityBindingElement;
                else if (element is TransportBindingElement)
                    transport = element as HttpTransportBindingElement;
                else if (element is MessageEncodingBindingElement)
                    encoding = element as MessageEncodingBindingElement;
                else if (element is TransactionFlowBindingElement)
                    txFlow = element as TransactionFlowBindingElement;
                else if (element is ReliableSessionBindingElement)
                    session = element as ReliableSessionBindingElement;
                else if (element is PrivacyNoticeBindingElement)
                    privacy = element as PrivacyNoticeBindingElement;
                else
                    return false;
            }

            if (transport == null)
                return false;
            if (encoding == null)
                return false;

            if (!transport.AuthenticationScheme.IsSingleton())
            {
                //multiple authentication schemes selected -- not supported in StandardBindings
                return false;
            }

            HttpsTransportBindingElement httpsTransport = transport as HttpsTransportBindingElement;
            if ( ( security != null ) && ( httpsTransport != null ) && ( httpsTransport.RequireClientCertificate != TransportDefaults.RequireClientCertificate ) )
            {
                return false;
            }

            if (null != privacy || !WSHttpBinding.TryCreate(security, transport, session, txFlow, out binding))
                if (!WSFederationHttpBinding.TryCreate(security, transport, privacy, session, txFlow, out binding))
                    if (!WS2007HttpBinding.TryCreate(security, transport, session, txFlow, out binding))
                        if (!WS2007FederationHttpBinding.TryCreate(security, transport, privacy, session, txFlow, out binding))
                            return false;

            if (txFlow == null)
            {
                txFlow = GetDefaultTransactionFlowBindingElement();
                if ((binding is WS2007HttpBinding) || (binding is WS2007FederationHttpBinding))
                {
                    txFlow.TransactionProtocol = TransactionProtocol.WSAtomicTransaction11;
                }
            }

            WSHttpBindingBase wSHttpBindingBase = binding as WSHttpBindingBase;
            wSHttpBindingBase.InitializeFrom(transport, encoding, txFlow, session);
            if (!wSHttpBindingBase.IsBindingElementsMatch(transport, encoding, txFlow, session))
                return false;

            return true;
        }

        protected abstract TransportBindingElement GetTransport();
        protected abstract SecurityBindingElement CreateMessageSecurity();

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
                || this.ReliableSession.InactivityTimeout != ReliableSessionDefaults.InactivityTimeout
                || this.ReliableSession.Enabled != ReliableSessionDefaults.Enabled;
        }
    }
}
