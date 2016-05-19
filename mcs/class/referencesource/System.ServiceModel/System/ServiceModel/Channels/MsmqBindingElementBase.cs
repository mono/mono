//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public abstract class MsmqBindingElementBase
        : TransportBindingElement, 
        ITransactedBindingElement,
        IWsdlExportExtension, IPolicyExportExtension, ITransportPolicyImport
    {
        Uri customDeadLetterQueue;
        DeadLetterQueue deadLetterQueue;
        bool durable;
        bool exactlyOnce;
        int maxRetryCycles;
        ReceiveErrorHandling receiveErrorHandling;
        int receiveRetryCount;
        TimeSpan retryCycleDelay;
        TimeSpan timeToLive;
        MsmqTransportSecurity msmqTransportSecurity;
        bool useMsmqTracing;
        bool useSourceJournal;
        bool receiveContextEnabled;
        
        internal MsmqBindingElementBase()
        {
            this.customDeadLetterQueue = MsmqDefaults.CustomDeadLetterQueue;
            this.deadLetterQueue = MsmqDefaults.DeadLetterQueue;
            this.durable = MsmqDefaults.Durable;
            this.exactlyOnce = MsmqDefaults.ExactlyOnce;
            this.maxRetryCycles = MsmqDefaults.MaxRetryCycles;
            this.receiveContextEnabled = MsmqDefaults.ReceiveContextEnabled;
            this.receiveErrorHandling = MsmqDefaults.ReceiveErrorHandling;
            this.receiveRetryCount = MsmqDefaults.ReceiveRetryCount;
            this.retryCycleDelay = MsmqDefaults.RetryCycleDelay;
            this.timeToLive = MsmqDefaults.TimeToLive;
            this.msmqTransportSecurity = new MsmqTransportSecurity();
            this.useMsmqTracing = MsmqDefaults.UseMsmqTracing;
            this.useSourceJournal = MsmqDefaults.UseSourceJournal;
            this.ReceiveContextSettings = new MsmqReceiveContextSettings();
        }

        internal MsmqBindingElementBase(MsmqBindingElementBase elementToBeCloned) : base(elementToBeCloned)
        {
            this.customDeadLetterQueue = elementToBeCloned.customDeadLetterQueue;
            this.deadLetterQueue = elementToBeCloned.deadLetterQueue;
            this.durable = elementToBeCloned.durable;
            this.exactlyOnce = elementToBeCloned.exactlyOnce;
            this.maxRetryCycles = elementToBeCloned.maxRetryCycles;
            this.msmqTransportSecurity = new MsmqTransportSecurity(elementToBeCloned.MsmqTransportSecurity);
            this.receiveContextEnabled = elementToBeCloned.ReceiveContextEnabled;
            this.receiveErrorHandling = elementToBeCloned.receiveErrorHandling;
            this.receiveRetryCount = elementToBeCloned.receiveRetryCount;
            this.retryCycleDelay = elementToBeCloned.retryCycleDelay;
            this.timeToLive = elementToBeCloned.timeToLive;
            this.useMsmqTracing = elementToBeCloned.useMsmqTracing;
            this.useSourceJournal = elementToBeCloned.useSourceJournal;
            // 


            this.ReceiveContextSettings = elementToBeCloned.ReceiveContextSettings;
        }

        internal IReceiveContextSettings ReceiveContextSettings
        {
            get; set;
        }

        internal abstract MsmqUri.IAddressTranslator AddressTranslator
        {
            get;
        }

        // applicable on: client
        public Uri CustomDeadLetterQueue
        {
            get { return this.customDeadLetterQueue; }
            set { this.customDeadLetterQueue = value; }
        }

        // applicable on: client
        public DeadLetterQueue DeadLetterQueue
        {
            get { return this.deadLetterQueue; }
            set 
            {
                if (! DeadLetterQueueHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.deadLetterQueue = value;
            }
        }

        // applicable on: client
        public bool Durable
        {
            get { return this.durable; }
            set { this.durable = value; }
        }

        public bool TransactedReceiveEnabled
        {
            get 
            {
                return this.exactlyOnce; 
            }
        }

        // applicable on: client, server
        public bool ExactlyOnce
        {
            get { return this.exactlyOnce; }
            set { this.exactlyOnce = value; }
        }

        // applicable on: server
        public int ReceiveRetryCount
        {
            get { return this.receiveRetryCount; }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("value", value, SR.GetString(SR.MsmqNonNegativeArgumentExpected)));
                }

                this.receiveRetryCount = value;
            }
        }

        // applicable on: server
        public int MaxRetryCycles
        {
            get { return this.maxRetryCycles; }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("value", value, SR.GetString(SR.MsmqNonNegativeArgumentExpected)));
                }

                this.maxRetryCycles = value;
            }
        }

        // applicable on: client, server
        public MsmqTransportSecurity MsmqTransportSecurity
        {
            get { return this.msmqTransportSecurity; }
            internal set { this.msmqTransportSecurity = value; }
        }

        // applicable on: server
        public bool ReceiveContextEnabled
        {
            get { return this.receiveContextEnabled; }
            set { this.receiveContextEnabled = value; }
        }
        
        // applicable on: server
        public ReceiveErrorHandling ReceiveErrorHandling
        {
            get { return this.receiveErrorHandling; }
            set 
            {
                if (! ReceiveErrorHandlingHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                this.receiveErrorHandling = value;
            }
        }

        // applicable on: server
        public TimeSpan RetryCycleDelay
        {
            get { return this.retryCycleDelay; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.retryCycleDelay = value;
            }
        }

        // applicable on: client
        public TimeSpan TimeToLive
        {
            get { return this.timeToLive; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.timeToLive = value;
            }
        }

        public bool UseMsmqTracing
        {
            get { return this.useMsmqTracing; }
            set { this.useMsmqTracing = value; }
        }

        public bool UseSourceJournal
        {
            get { return this.useSourceJournal; }
            set { this.useSourceJournal = value; }
        }
        
        public TimeSpan ValidityDuration 
        {
            get { return this.ReceiveContextSettings.ValidityDuration; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                ((MsmqReceiveContextSettings)this.ReceiveContextSettings).SetValidityDuration(value);
            }
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities)) 
            {
                return null;
            }
            else if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            }
            else if (typeof(T) == typeof(IReceiveContextSettings))
            {
                // receive context is not supported over a non-transactional queue
                if (this.ExactlyOnce && this.ReceiveContextEnabled)
                {
                    return (T)(object)this.ReceiveContextSettings;
                }
                else
                {
                    return null;
                }
            }
            else if (typeof(T) == typeof(ITransactedBindingElement))
            {
                return (T)(object)this;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        static bool FindAssertion(ICollection<XmlElement> assertions, string name)
        {
            return (PolicyConversionContext.FindAssertion(assertions, name, TransportPolicyConstants.MsmqTransportNamespace, true) != null);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            XmlDocument document = new XmlDocument();

            ICollection<XmlElement> policyAssertions = context.GetBindingAssertions();
            if (!this.Durable)
            {
                policyAssertions.Add(document.CreateElement(
                                         TransportPolicyConstants.MsmqTransportPrefix,
                                         TransportPolicyConstants.MsmqVolatile,
                                         TransportPolicyConstants.MsmqTransportNamespace));
            }

            if (!this.ExactlyOnce)
            {
                policyAssertions.Add(document.CreateElement(
                                         TransportPolicyConstants.MsmqTransportPrefix,
                                         TransportPolicyConstants.MsmqBestEffort,
                                         TransportPolicyConstants.MsmqTransportNamespace));
            }

            if (context.Contract.SessionMode == SessionMode.Required)
            {
                policyAssertions.Add(document.CreateElement(
                                         TransportPolicyConstants.MsmqTransportPrefix,
                                         TransportPolicyConstants.MsmqSession,
                                         TransportPolicyConstants.MsmqTransportNamespace));
            }

            if (this.MsmqTransportSecurity.MsmqProtectionLevel != ProtectionLevel.None)
            {
                policyAssertions.Add(document.CreateElement(
                                         TransportPolicyConstants.MsmqTransportPrefix,
                                         TransportPolicyConstants.MsmqAuthenticated,
                                         TransportPolicyConstants.MsmqTransportNamespace));
                if (this.MsmqTransportSecurity.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain)
                {
                    policyAssertions.Add(document.CreateElement(
                                             TransportPolicyConstants.MsmqTransportPrefix,
                                             TransportPolicyConstants.MsmqWindowsDomain,
                                             TransportPolicyConstants.MsmqTransportNamespace));
                }
            }

            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(context.BindingElements, out createdNew);
            if (createdNew && encodingBindingElement is IPolicyExportExtension)
            {
                ((IPolicyExportExtension)encodingBindingElement).ExportPolicy(exporter, context);
            }

            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, encodingBindingElement.MessageVersion.Addressing);
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            ICollection<XmlElement> policyAssertions = policyContext.GetBindingAssertions();
            if (FindAssertion(policyAssertions, TransportPolicyConstants.MsmqVolatile))
            {
                this.Durable = false;
            }

            if (FindAssertion(policyAssertions, TransportPolicyConstants.MsmqBestEffort))
            {
                this.ExactlyOnce = false;
            }

            if (FindAssertion(policyAssertions, TransportPolicyConstants.MsmqSession))
            {
                policyContext.Contract.SessionMode = SessionMode.Required;
            }

            if (FindAssertion(policyAssertions, TransportPolicyConstants.MsmqAuthenticated))
            {
                this.MsmqTransportSecurity.MsmqProtectionLevel = ProtectionLevel.Sign;
                if (FindAssertion(policyAssertions, TransportPolicyConstants.MsmqWindowsDomain))
                    this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.WindowsDomain;
                else
                    this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.Certificate;
            }
            else
            {
                this.MsmqTransportSecurity.MsmqProtectionLevel = ProtectionLevel.None;
                this.MsmqTransportSecurity.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            }
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }

        internal virtual string WsdlTransportUri
        {
            get
            {
                return null;
            }
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(endpointContext, out createdNew);
            TransportBindingElement.ExportWsdlEndpoint(
                exporter, endpointContext, this.WsdlTransportUri, 
                encodingBindingElement.MessageVersion.Addressing);
        }

        MessageEncodingBindingElement FindMessageEncodingBindingElement(BindingElementCollection bindingElements, out bool createdNew)
        {
            createdNew = false;
            MessageEncodingBindingElement encodingBindingElement = bindingElements.Find<MessageEncodingBindingElement>();
            if (encodingBindingElement == null)
            {
                createdNew = true;
                encodingBindingElement = new BinaryMessageEncodingBindingElement();
            }
            return encodingBindingElement;
        }

        MessageEncodingBindingElement FindMessageEncodingBindingElement(WsdlEndpointConversionContext endpointContext, out bool createdNew)
        {
            BindingElementCollection bindingElements = endpointContext.Endpoint.Binding.CreateBindingElements();
            return FindMessageEncodingBindingElement(bindingElements, out createdNew);
        }

        class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }
            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get { return false; }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get { return true; }
            }
        }
 
    }
}
    
