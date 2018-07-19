//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Xml;
    using Config = System.ServiceModel.Configuration;

    public class NetMsmqBinding : MsmqBindingBase
    {
        // private BindingElements
        BinaryMessageEncodingBindingElement encoding;
        NetMsmqSecurity security;

        public NetMsmqBinding()
        {
            Initialize();
            this.security = new NetMsmqSecurity();
        }

        public NetMsmqBinding(string configurationName)
        {
            Initialize();
            this.security = new NetMsmqSecurity();
            ApplyConfiguration(configurationName);
        }

        public NetMsmqBinding(NetMsmqSecurityMode securityMode)
        {
            if (!NetMsmqSecurityModeHelper.IsDefined(securityMode))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("mode", (int)securityMode, typeof(NetMsmqSecurityMode)));
            Initialize();
            this.security = new NetMsmqSecurity(securityMode);
        }

        NetMsmqBinding(NetMsmqSecurity security)
        {
            Initialize();
            Fx.Assert(security != null, "Invalid (null) NetMsmqSecurity value");
            this.security = security;
        }

        [DefaultValue(MsmqDefaults.QueueTransferProtocol)]
        public QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return (this.transport as MsmqTransportBindingElement).QueueTransferProtocol;
            }
            set
            {
                (this.transport as MsmqTransportBindingElement).QueueTransferProtocol = value;
            }
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

        public NetMsmqSecurity Security
        {
            get { return this.security; }
            set { this.security = value; }
        }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.Soap12; }
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

        internal int MaxPoolSize
        {
            get
            {
                return (transport as MsmqTransportBindingElement).MaxPoolSize;
            }
            set
            {
                (transport as MsmqTransportBindingElement).MaxPoolSize = value;
            }
        }

        [DefaultValue(MsmqDefaults.UseActiveDirectory)]
        public bool UseActiveDirectory
        {
            get
            {
                return (transport as MsmqTransportBindingElement).UseActiveDirectory;
            }
            set
            {
                (transport as MsmqTransportBindingElement).UseActiveDirectory = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            if (this.ReaderQuotas.MaxArrayLength != EncoderDefaults.MaxArrayLength)
            {
                return true;
            }
            if (this.ReaderQuotas.MaxBytesPerRead != EncoderDefaults.MaxBytesPerRead)
            {
                return true;
            }
            if (this.ReaderQuotas.MaxDepth != EncoderDefaults.MaxDepth)
            {
                return true;
            }
            if (this.ReaderQuotas.MaxNameTableCharCount != EncoderDefaults.MaxNameTableCharCount)
            {
                return true;
            }
            if (this.ReaderQuotas.MaxStringContentLength != EncoderDefaults.MaxStringContentLength)
            {
                return true;
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            if (this.security.Mode != NetMsmqSecurity.DefaultMode)
            {
                return true;
            }

            if (this.security.Transport.MsmqAuthenticationMode != MsmqDefaults.MsmqAuthenticationMode ||
            this.security.Transport.MsmqEncryptionAlgorithm != MsmqDefaults.MsmqEncryptionAlgorithm ||
            this.security.Transport.MsmqSecureHashAlgorithm != MsmqDefaults.MsmqSecureHashAlgorithm ||
            this.security.Transport.MsmqProtectionLevel != MsmqDefaults.MsmqProtectionLevel)
            {
                return true;
            }

            if (this.security.Message.AlgorithmSuite != MsmqDefaults.MessageSecurityAlgorithmSuite ||
            this.security.Message.ClientCredentialType != MsmqDefaults.DefaultClientCredentialType)
            {
                return true;
            }
            return false;
        }

        void Initialize()
        {
            transport = new MsmqTransportBindingElement();
            encoding = new BinaryMessageEncodingBindingElement();
        }

        void InitializeFrom(MsmqTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding)
        {
            // only set properties that have standard binding manifestations: MaxPoolSize *is not* one of them
            this.CustomDeadLetterQueue = transport.CustomDeadLetterQueue;
            this.DeadLetterQueue = transport.DeadLetterQueue;
            this.Durable = transport.Durable;
            this.ExactlyOnce = transport.ExactlyOnce;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ReceiveRetryCount = transport.ReceiveRetryCount;
            this.MaxRetryCycles = transport.MaxRetryCycles;
            this.ReceiveErrorHandling = transport.ReceiveErrorHandling;
            this.RetryCycleDelay = transport.RetryCycleDelay;
            this.TimeToLive = transport.TimeToLive;
            this.UseSourceJournal = transport.UseSourceJournal;
            this.UseMsmqTracing = transport.UseMsmqTracing;
            this.ReceiveContextEnabled = transport.ReceiveContextEnabled;
            this.QueueTransferProtocol = transport.QueueTransferProtocol;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.UseActiveDirectory = transport.UseActiveDirectory;
            this.ValidityDuration = transport.ValidityDuration;
            this.ReaderQuotas = encoding.ReaderQuotas;
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on NetMsmqBinding
        // match default values of the binding elements
        bool IsBindingElementsMatch(MsmqTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            // we do not have to check the transport match here: they always match
            if (!this.GetTransport().IsMatch(transport))
                return false;

            if (!this.encoding.IsMatch(encoding))
                return false;
            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            Config.NetMsmqBindingCollectionElement section = Config.NetMsmqBindingCollectionElement.GetBindingCollectionElement();
            Config.NetMsmqBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 Config.ConfigurationStrings.NetMsmqBindingCollectionElementName)));
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
            // add security
            SecurityBindingElement wsSecurity = CreateMessageSecurity();
            if (wsSecurity != null)
            {
                bindingElements.Add(wsSecurity);
            }
            // add encoding (text or mtom)
            bindingElements.Add(encoding);
            // add transport
            bindingElements.Add(GetTransport());

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count > 3)
                return false;

            SecurityBindingElement security = null;
            BinaryMessageEncodingBindingElement encoding = null;
            MsmqTransportBindingElement transport = null;

            foreach (BindingElement element in elements)
            {
                if (element is SecurityBindingElement)
                    security = element as SecurityBindingElement;
                else if (element is TransportBindingElement)
                    transport = element as MsmqTransportBindingElement;
                else if (element is MessageEncodingBindingElement)
                    encoding = element as BinaryMessageEncodingBindingElement;
                else
                    return false;
            }

            UnifiedSecurityMode mode;
            if (!IsValidTransport(transport, out mode))
                return false;

            if (encoding == null)
                return false;

            NetMsmqSecurity netMsmqSecurity;
            if (!TryCreateSecurity(security, mode, out netMsmqSecurity))
                return false;

            NetMsmqBinding netMsmqBinding = new NetMsmqBinding(netMsmqSecurity);
            netMsmqBinding.InitializeFrom(transport, encoding);
            if (!netMsmqBinding.IsBindingElementsMatch(transport, encoding))
                return false;

            binding = netMsmqBinding;
            return true;
        }

        SecurityBindingElement CreateMessageSecurity()
        {
            if (this.security.Mode == NetMsmqSecurityMode.Message || this.security.Mode == NetMsmqSecurityMode.Both)
            {
                return this.security.CreateMessageSecurity();
            }
            else
            {
                return null;
            }
        }

        static bool TryCreateSecurity(SecurityBindingElement sbe, UnifiedSecurityMode mode, out NetMsmqSecurity security)
        {
            if (sbe != null)
                mode &= UnifiedSecurityMode.Message | UnifiedSecurityMode.Both;
            else
                mode &= ~(UnifiedSecurityMode.Message | UnifiedSecurityMode.Both);

            NetMsmqSecurityMode netMsmqSecurityMode = NetMsmqSecurityModeHelper.ToSecurityMode(mode);
            Fx.Assert(NetMsmqSecurityModeHelper.IsDefined(netMsmqSecurityMode), string.Format("Invalid NetMsmqSecurityMode value: {0}.", netMsmqSecurityMode.ToString()));

            if (NetMsmqSecurity.TryCreate(sbe, netMsmqSecurityMode, out security))
            {
                return true;
            }
            return false;
        }

        MsmqBindingElementBase GetTransport()
        {
            this.security.ConfigureTransportSecurity(transport);
            return transport;
        }

        static bool IsValidTransport(MsmqTransportBindingElement msmq, out UnifiedSecurityMode mode)
        {
            mode = (UnifiedSecurityMode)0;
            if (msmq == null)
                return false;
            return NetMsmqSecurity.IsConfiguredTransportSecurity(msmq, out mode);
        }
    }
}
