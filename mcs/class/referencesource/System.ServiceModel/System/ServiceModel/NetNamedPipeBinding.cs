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
    using System.Net.Sockets;
    using System.ComponentModel;

    public class NetNamedPipeBinding : Binding, IBindingRuntimePreferences
    {
        // private BindingElements
        TransactionFlowBindingElement context;
        BinaryMessageEncodingBindingElement encoding;
        NamedPipeTransportBindingElement namedPipe;
        NetNamedPipeSecurity security = new NetNamedPipeSecurity();

        public NetNamedPipeBinding()
            : base()
        {
            Initialize();
        }

        public NetNamedPipeBinding(NetNamedPipeSecurityMode securityMode)
            : this()
        {
            this.security.Mode = securityMode;
        }
        public NetNamedPipeBinding(string configurationName)
            : this()
        {
            ApplyConfiguration(configurationName);
        }
        NetNamedPipeBinding(NetNamedPipeSecurity security)
            : this()
        {
            this.security = security;
        }

        [DefaultValue(TransactionFlowDefaults.Transactions)]
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
            get { return namedPipe.TransferMode; }
            set { namedPipe.TransferMode = value; }
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return namedPipe.HostNameComparisonMode; }
            set { namedPipe.HostNameComparisonMode = value; }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get { return namedPipe.MaxBufferPoolSize; }
            set
            {
                namedPipe.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get { return namedPipe.MaxBufferSize; }
            set { namedPipe.MaxBufferSize = value; }
        }

        public int MaxConnections
        {
            get { return namedPipe.MaxPendingConnections; }
            set
            {
                namedPipe.MaxPendingConnections = value;
                namedPipe.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = value;
            }
        }

        internal bool IsMaxConnectionsSet
        {
            get { return namedPipe.IsMaxPendingConnectionsSet; }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return namedPipe.MaxReceivedMessageSize; }
            set { namedPipe.MaxReceivedMessageSize = value; }
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

        public override string Scheme { get { return namedPipe.Scheme; } }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.Soap12; }
        }

        public NetNamedPipeSecurity Security
        {
            get { return this.security; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.security = value;
            }
        }

        static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new TransactionFlowBindingElement(false);
        }

        void Initialize()
        {
            namedPipe = new NamedPipeTransportBindingElement();
            encoding = new BinaryMessageEncodingBindingElement();
            context = GetDefaultTransactionFlowBindingElement();
        }

        void InitializeFrom(NamedPipeTransportBindingElement namedPipe, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context)
        {
            Initialize();
            this.HostNameComparisonMode = namedPipe.HostNameComparisonMode;
            this.MaxBufferPoolSize = namedPipe.MaxBufferPoolSize;
            this.MaxBufferSize = namedPipe.MaxBufferSize;
            if (namedPipe.IsMaxPendingConnectionsSet)
            {
                this.MaxConnections = namedPipe.MaxPendingConnections;    
            }
            this.MaxReceivedMessageSize = namedPipe.MaxReceivedMessageSize;
            this.TransferMode = namedPipe.TransferMode;

            this.ReaderQuotas = encoding.ReaderQuotas;

            this.TransactionFlow = context.Transactions;
            this.TransactionProtocol = context.TransactionProtocol;
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on BasicHttpBinding 
        // match default values of the binding elements
        bool IsBindingElementsMatch(NamedPipeTransportBindingElement namedPipe, BinaryMessageEncodingBindingElement encoding, TransactionFlowBindingElement context)
        {
            if (!this.namedPipe.IsMatch(namedPipe))
                return false;
            if (!this.encoding.IsMatch(encoding))
                return false;
            if (!this.context.IsMatch(context))
                return false;
            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            NetNamedPipeBindingCollectionElement section = NetNamedPipeBindingCollectionElement.GetBindingCollectionElement();
            NetNamedPipeBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.NetNamedPipeBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {   // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add context
            bindingElements.Add(context);
            // add encoding
            bindingElements.Add(encoding);
            // add transport security
            WindowsStreamSecurityBindingElement transportSecurity = CreateTransportSecurity();
            if (transportSecurity != null)
            {
                bindingElements.Add(transportSecurity);
            }
            // add transport (named pipes)
            bindingElements.Add(this.namedPipe);

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 4)
                return false;

            TransactionFlowBindingElement context = null;
            BinaryMessageEncodingBindingElement encoding = null;
            WindowsStreamSecurityBindingElement security = null;
            NamedPipeTransportBindingElement namedPipe = null;

            foreach (BindingElement element in elements)
            {
                if (element is TransactionFlowBindingElement)
                    context = element as TransactionFlowBindingElement;
                else if (element is BinaryMessageEncodingBindingElement)
                    encoding = element as BinaryMessageEncodingBindingElement;
                else if (element is WindowsStreamSecurityBindingElement)
                    security = element as WindowsStreamSecurityBindingElement;
                else if (element is NamedPipeTransportBindingElement)
                    namedPipe = element as NamedPipeTransportBindingElement;
                else
                    return false;
            }

            if (namedPipe == null)
                return false;

            if (encoding == null)
                return false;

            if (context == null)
                context = GetDefaultTransactionFlowBindingElement();

            NetNamedPipeSecurity pipeSecurity;
            if (!TryCreateSecurity(security, out pipeSecurity))
                return false;

            NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding(pipeSecurity);
            netNamedPipeBinding.InitializeFrom(namedPipe, encoding, context);

            if (!netNamedPipeBinding.IsBindingElementsMatch(namedPipe, encoding, context))
                return false;

            binding = netNamedPipeBinding;
            return true;
        }

        WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            if (this.security.Mode == NetNamedPipeSecurityMode.Transport)
            {
                return this.security.CreateTransportSecurity();
            }
            else
            {
                return null;
            }
        }

        static bool TryCreateSecurity(WindowsStreamSecurityBindingElement wssbe, out NetNamedPipeSecurity security)
        {
            NetNamedPipeSecurityMode mode = wssbe == null ? NetNamedPipeSecurityMode.None : NetNamedPipeSecurityMode.Transport;
            return NetNamedPipeSecurity.TryCreate(wssbe, mode, out security);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            if (this.security.Mode != NetNamedPipeSecurity.DefaultMode)
            {
                return true;
            }
            if (this.security.Transport.ProtectionLevel != NamedPipeTransportSecurity.DefaultProtectionLevel)
            {
                return true;
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return (TransactionProtocol != NetTcpDefaults.TransactionProtocol);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMaxConnections()
        {
            return namedPipe.ShouldSerializeMaxPendingConnections();
        }
    }
}
