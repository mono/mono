//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public class NetTcpBinding : Binding, IBindingRuntimePreferences
    {
        OptionalReliableSession reliableSession;
        // private BindingElements
        TcpTransportBindingElement transport;
        BinaryMessageEncodingBindingElement encoding;
        TransactionFlowBindingElement context;
        ReliableSessionBindingElement session;
        NetTcpSecurity security = new NetTcpSecurity();

        public NetTcpBinding() { Initialize(); }
        public NetTcpBinding(SecurityMode securityMode)
            : this()
        {
            this.security.Mode = securityMode;
        }

        public NetTcpBinding(SecurityMode securityMode, bool reliableSessionEnabled)
            : this(securityMode)
        {
            this.ReliableSession.Enabled = reliableSessionEnabled;
        }

        public NetTcpBinding(string configurationName)
            : this()
        {
            ApplyConfiguration(configurationName);
        }

        NetTcpBinding(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session, NetTcpSecurity security)
            : this()
        {
            this.security = security;
            this.ReliableSession.Enabled = session != null;
            InitializeFrom(transport, encoding, context, session);
        }

        [DefaultValue(NetTcpDefaults.TransactionsEnabled)]
        public bool TransactionFlow
        {
            get { return context.Transactions; }
            set { context.Transactions = value; }
        }

        public TransactionProtocol TransactionProtocol
        {
            get { return this.context.TransactionProtocol; }
            set { this.context.TransactionProtocol = value; }
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get { return this.transport.TransferMode; }
            set { this.transport.TransferMode = value; }
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return transport.HostNameComparisonMode; }
            set { transport.HostNameComparisonMode = value; }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return transport.MaxBufferPoolSize; }
            set
            {
                transport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get { return transport.MaxBufferSize; }
            set { transport.MaxBufferSize = value; }
        }

        public int MaxConnections
        {
            get { return transport.MaxPendingConnections; }
            set
            {
                transport.MaxPendingConnections = value;
                transport.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = value;
            }
        }

        internal bool IsMaxConnectionsSet
        {
            get { return transport.IsMaxPendingConnectionsSet; }
        }

        public int ListenBacklog
        {
            get { return transport.ListenBacklog; }
            set { transport.ListenBacklog = value; }
        }

        internal bool IsListenBacklogSet
        {
            get { return transport.IsListenBacklogSet; }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return transport.MaxReceivedMessageSize; }
            set { transport.MaxReceivedMessageSize = value; }
        }

        [DefaultValue(TcpTransportDefaults.PortSharingEnabled)]
        public bool PortSharingEnabled
        {
            get { return transport.PortSharingEnabled; }
            set { transport.PortSharingEnabled = value; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return encoding.ReaderQuotas; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                value.CopyTo(encoding.ReaderQuotas);
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        public OptionalReliableSession ReliableSession
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

        public override string Scheme { get { return transport.Scheme; } }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.Soap12; }
        }

        public NetTcpSecurity Security
        {
            get { return security; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                security = value;
            }
        }

        static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new TransactionFlowBindingElement(NetTcpDefaults.TransactionsEnabled);
        }

        void Initialize()
        {
            transport = new TcpTransportBindingElement();
            encoding = new BinaryMessageEncodingBindingElement();
            context = GetDefaultTransactionFlowBindingElement();
            session = new ReliableSessionBindingElement();
            this.reliableSession = new OptionalReliableSession(session);
        }

        void InitializeFrom(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session)
        {
            Fx.Assert(transport != null, "Invalid (null) transport value.");
            Fx.Assert(encoding != null, "Invalid (null) encoding value.");
            Fx.Assert(context != null, "Invalid (null) context value.");
            Fx.Assert(security != null, "Invalid (null) security value.");

            // transport
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxBufferSize = transport.MaxBufferSize;
            if (transport.IsMaxPendingConnectionsSet)
            {
                this.MaxConnections = transport.MaxPendingConnections;    
            }
            if (transport.IsListenBacklogSet)
            {
                this.ListenBacklog = transport.ListenBacklog;    
            }
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.PortSharingEnabled = transport.PortSharingEnabled;
            this.TransferMode = transport.TransferMode;

            // encoding
            this.ReaderQuotas = encoding.ReaderQuotas;

            // context
            this.TransactionFlow = context.Transactions;
            this.TransactionProtocol = context.TransactionProtocol;

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
        bool IsBindingElementsMatch(TcpTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context, ReliableSessionBindingElement session)
        {
            if (!this.transport.IsMatch(transport))
                return false;
            if (!this.encoding.IsMatch(encoding))
                return false;
            if (!this.context.IsMatch(context))
                return false;
            if (reliableSession.Enabled)
            {
                if (!this.session.IsMatch(session))
                    return false;
            }
            else if (session != null)
                return false;

            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            NetTcpBindingCollectionElement section = NetTcpBindingCollectionElement.GetBindingCollectionElement();
            NetTcpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.NetTcpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        // In the Win8 profile, some settings for the binding security are not supported.
        void CheckSettings()
        {
            if (!UnsafeNativeMethods.IsTailoredApplication.Value)
            {
                return;
            }

            NetTcpSecurity security = this.Security;
            if (security == null)
            {
                return;
            }

            SecurityMode mode = security.Mode;
            if (mode == SecurityMode.None)
            {
                return;
            }
            else if (mode == SecurityMode.Message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Mode", mode)));
            }

            // Message.ClientCredentialType = Certificate, IssuedToken or Windows are not supported.
            if (mode == SecurityMode.TransportWithMessageCredential)
            {
                MessageSecurityOverTcp message = security.Message;
                if (message != null)
                {
                    MessageCredentialType mct = message.ClientCredentialType;
                    if ((mct == MessageCredentialType.Certificate) || (mct == MessageCredentialType.IssuedToken) || (mct == MessageCredentialType.Windows))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Message.ClientCredentialType", mct)));
                    }
                }
            }

            // Transport.ClientCredentialType = Certificate is not supported.
            Fx.Assert((mode == SecurityMode.Transport) || (mode == SecurityMode.TransportWithMessageCredential), "Unexpected SecurityMode value: " + mode);
            TcpTransportSecurity transport = security.Transport;
            if ((transport != null) && (transport.ClientCredentialType == TcpClientCredentialType.Certificate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Transport.ClientCredentialType", transport.ClientCredentialType)));
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            this.CheckSettings();
            
            // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add context
            bindingElements.Add(context);
            // add session
            if (reliableSession.Enabled)
                bindingElements.Add(session);
            // add security (*optional)
            SecurityBindingElement wsSecurity = CreateMessageSecurity();
            if (wsSecurity != null)
                bindingElements.Add(wsSecurity);
            // add encoding
            bindingElements.Add(encoding);
            // add transport security
            BindingElement transportSecurity = CreateTransportSecurity();
            if (transportSecurity != null)
            {
                bindingElements.Add(transportSecurity);
            }
            transport.ExtendedProtectionPolicy = security.Transport.ExtendedProtectionPolicy;
            // add transport (tcp)
            bindingElements.Add(transport);

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 6)
                return false;

            // collect all binding elements
            TcpTransportBindingElement transport = null;
            BinaryMessageEncodingBindingElement encoding = null;
            TransactionFlowBindingElement context = null;
            ReliableSessionBindingElement session = null;
            SecurityBindingElement wsSecurity = null;
            BindingElement transportSecurity = null;

            foreach (BindingElement element in elements)
            {
                if (element is SecurityBindingElement)
                    wsSecurity = element as SecurityBindingElement;
                else if (element is TransportBindingElement)
                    transport = element as TcpTransportBindingElement;
                else if (element is MessageEncodingBindingElement)
                    encoding = element as BinaryMessageEncodingBindingElement;
                else if (element is TransactionFlowBindingElement)
                    context = element as TransactionFlowBindingElement;
                else if (element is ReliableSessionBindingElement)
                    session = element as ReliableSessionBindingElement;
                else
                {
                    if (transportSecurity != null)
                        return false;
                    transportSecurity = element;
                }
            }

            if (transport == null)
                return false;
            if (encoding == null)
                return false;
            if (context == null)
                context = GetDefaultTransactionFlowBindingElement();

            TcpTransportSecurity tcpTransportSecurity = new TcpTransportSecurity();
            UnifiedSecurityMode mode = GetModeFromTransportSecurity(transportSecurity);

            NetTcpSecurity security;
            if (!TryCreateSecurity(wsSecurity, mode, session != null, transportSecurity, tcpTransportSecurity, out security))
                return false;

            if (!SetTransportSecurity(transportSecurity, security.Mode, tcpTransportSecurity))
                return false;

            NetTcpBinding netTcpBinding = new NetTcpBinding(transport, encoding, context, session, security);
            if (!netTcpBinding.IsBindingElementsMatch(transport, encoding, context, session))
                return false;

            binding = netTcpBinding;
            return true;
        }

        BindingElement CreateTransportSecurity()
        {
            return this.security.CreateTransportSecurity();
        }

        static UnifiedSecurityMode GetModeFromTransportSecurity(BindingElement transport)
        {
            return NetTcpSecurity.GetModeFromTransportSecurity(transport);
        }

        static bool SetTransportSecurity(BindingElement transport, SecurityMode mode, TcpTransportSecurity transportSecurity)
        {
            return NetTcpSecurity.SetTransportSecurity(transport, mode, transportSecurity);
        }

        SecurityBindingElement CreateMessageSecurity()
        {
            if (this.security.Mode == SecurityMode.Message || this.security.Mode == SecurityMode.TransportWithMessageCredential)
            {
                return this.security.CreateMessageSecurity(this.ReliableSession.Enabled);
            }
            else
            {
                return null;
            }
        }

        static bool TryCreateSecurity(SecurityBindingElement sbe, UnifiedSecurityMode mode, bool isReliableSession, BindingElement transportSecurity, TcpTransportSecurity tcpTransportSecurity, out NetTcpSecurity security)
        {
            if (sbe != null)
                mode &= UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential;
            else
                mode &= ~(UnifiedSecurityMode.Message | UnifiedSecurityMode.TransportWithMessageCredential);

            SecurityMode securityMode = SecurityModeHelper.ToSecurityMode(mode);
            Fx.Assert(SecurityModeHelper.IsDefined(securityMode), string.Format("Invalid SecurityMode value: {0}.", securityMode.ToString()));

            if (NetTcpSecurity.TryCreate(sbe, securityMode, isReliableSession, transportSecurity, tcpTransportSecurity, out security))
                return true;

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.security.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return (TransactionProtocol != NetTcpDefaults.TransactionProtocol);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReliableSession()
        {
            return (this.ReliableSession.Ordered != ReliableSessionDefaults.Ordered
                || this.ReliableSession.InactivityTimeout != ReliableSessionDefaults.InactivityTimeout
                || this.ReliableSession.Enabled != ReliableSessionDefaults.Enabled);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeListenBacklog()
        {
            return transport.ShouldSerializeListenBacklog();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMaxConnections()
        {
            return transport.ShouldSerializeListenBacklog();
        }
    }
}
