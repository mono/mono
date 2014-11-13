//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public abstract class ConnectionOrientedTransportBindingElement
        : TransportBindingElement,
        IWsdlExportExtension,
        IPolicyExportExtension,
        ITransportPolicyImport
    {
        int connectionBufferSize;
        bool exposeConnectionProperty;
        HostNameComparisonMode hostNameComparisonMode;
        bool inheritBaseAddressSettings;
        TimeSpan channelInitializationTimeout;
        int maxBufferSize;
        bool maxBufferSizeInitialized;
        int maxPendingConnections;
        TimeSpan maxOutputDelay;
        int maxPendingAccepts;
        TransferMode transferMode;
        bool isMaxPendingConnectionsSet;
        bool isMaxPendingAcceptsSet;

        internal ConnectionOrientedTransportBindingElement()
            : base()
        {
            this.connectionBufferSize = ConnectionOrientedTransportDefaults.ConnectionBufferSize;
            this.hostNameComparisonMode = ConnectionOrientedTransportDefaults.HostNameComparisonMode;
            this.channelInitializationTimeout = ConnectionOrientedTransportDefaults.ChannelInitializationTimeout;
            this.maxBufferSize = TransportDefaults.MaxBufferSize;
            this.maxPendingConnections = ConnectionOrientedTransportDefaults.GetMaxPendingConnections();
            this.maxOutputDelay = ConnectionOrientedTransportDefaults.MaxOutputDelay;
            this.maxPendingAccepts = ConnectionOrientedTransportDefaults.GetMaxPendingAccepts();
            this.transferMode = ConnectionOrientedTransportDefaults.TransferMode;
        }

        internal ConnectionOrientedTransportBindingElement(ConnectionOrientedTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.connectionBufferSize = elementToBeCloned.connectionBufferSize;
            this.exposeConnectionProperty = elementToBeCloned.exposeConnectionProperty;
            this.hostNameComparisonMode = elementToBeCloned.hostNameComparisonMode;
            this.inheritBaseAddressSettings = elementToBeCloned.InheritBaseAddressSettings;
            this.channelInitializationTimeout = elementToBeCloned.ChannelInitializationTimeout;
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
            this.maxBufferSizeInitialized = elementToBeCloned.maxBufferSizeInitialized;
            this.maxPendingConnections = elementToBeCloned.maxPendingConnections;
            this.maxOutputDelay = elementToBeCloned.maxOutputDelay;
            this.maxPendingAccepts = elementToBeCloned.maxPendingAccepts;
            this.transferMode = elementToBeCloned.transferMode;
            this.isMaxPendingConnectionsSet = elementToBeCloned.isMaxPendingConnectionsSet;
            this.isMaxPendingAcceptsSet = elementToBeCloned.isMaxPendingAcceptsSet;
        }

        // client
        // server
        [DefaultValue(ConnectionOrientedTransportDefaults.ConnectionBufferSize)]
        public int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeNonNegative)));
                }

                this.connectionBufferSize = value;
            }
        }

        // client
        internal bool ExposeConnectionProperty
        {
            get
            {
                return this.exposeConnectionProperty;
            }
            set
            {
                this.exposeConnectionProperty = value;
            }
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }

            set
            {
                HostNameComparisonModeHelper.Validate(value);
                this.hostNameComparisonMode = value;
            }
        }

        // server
        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get
            {
                if (maxBufferSizeInitialized || TransferMode != TransferMode.Buffered)
                {
                    return maxBufferSize;
                }

                long maxReceivedMessageSize = MaxReceivedMessageSize;
                if (maxReceivedMessageSize > int.MaxValue)
                {
                    return int.MaxValue;
                }
                else
                {
                    return (int)maxReceivedMessageSize;
                }
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                maxBufferSizeInitialized = true;
                this.maxBufferSize = value;
            }
        }

        // server
        public int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }
            set
            {
                if (value <= 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));

                this.maxPendingConnections = value;
                this.isMaxPendingConnectionsSet = true;
            }
        }

        internal bool IsMaxPendingConnectionsSet
        {
            get { return this.isMaxPendingConnectionsSet; }
        }

        // MB#26970: used by MEX to ensure that we don't conflict on base-address scoped settings
        internal bool InheritBaseAddressSettings
        {
            get
            {
                return this.inheritBaseAddressSettings;
            }
            set
            {
                this.inheritBaseAddressSettings = value;
            }
        }

        // server
        [DefaultValue(typeof(TimeSpan), ConnectionOrientedTransportDefaults.ChannelInitializationTimeoutString)]
        public TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return this.channelInitializationTimeout;
            }

            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.channelInitializationTimeout = value;
            }
        }

        // server
        [DefaultValue(typeof(TimeSpan), ConnectionOrientedTransportDefaults.MaxOutputDelayString)]
        public TimeSpan MaxOutputDelay
        {
            get
            {
                return this.maxOutputDelay;
            }

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

                this.maxOutputDelay = value;
            }
        }

        // server
        public int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }

            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                this.maxPendingAccepts = value;
                this.isMaxPendingAcceptsSet = true;
            }
        }

        internal bool IsMaxPendingAcceptsSet
        {
            get { return this.isMaxPendingAcceptsSet; }
        }

        // client
        // server
        [DefaultValue(ConnectionOrientedTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
            set
            {
                TransferModeHelper.Validate(value);
                this.transferMode = value;
            }
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (TransferMode == TransferMode.Buffered)
            {
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
            else
            {
                return (typeof(TChannel) == typeof(IRequestChannel));
            }
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (TransferMode == TransferMode.Buffered)
            {
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
            else
            {
                return (typeof(TChannel) == typeof(IReplyChannel));
            }
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

            ICollection<XmlElement> policyAssertions = context.GetBindingAssertions();
            if (TransferModeHelper.IsRequestStreamed(this.TransferMode)
                || TransferModeHelper.IsResponseStreamed(this.TransferMode))
            {
                policyAssertions.Add(new XmlDocument().CreateElement(TransportPolicyConstants.DotNetFramingPrefix,
                    TransportPolicyConstants.StreamedName, TransportPolicyConstants.DotNetFramingNamespace));
            }

            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(context.BindingElements, out createdNew);
            if (createdNew && encodingBindingElement is IPolicyExportExtension)
            {
                encodingBindingElement = new BinaryMessageEncodingBindingElement();
                ((IPolicyExportExtension)encodingBindingElement).ExportPolicy(exporter, context);
            }

            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, encodingBindingElement.MessageVersion.Addressing);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }

        internal abstract string WsdlTransportUri { get; }
        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(endpointContext, out createdNew);
            TransportBindingElement.ExportWsdlEndpoint(exporter, endpointContext, this.WsdlTransportUri, encodingBindingElement.MessageVersion.Addressing);
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), TransportPolicyConstants.StreamedName, TransportPolicyConstants.DotNetFramingNamespace, true) != null)
            {
                this.TransferMode = TransferMode.Streamed;
            }

            WindowsStreamSecurityBindingElement.ImportPolicy(importer, policyContext);
            SslStreamSecurityBindingElement.ImportPolicy(importer, policyContext);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(T) == typeof(TransferMode))
            {
                return (T)(object)this.TransferMode;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
                return false;

            ConnectionOrientedTransportBindingElement connection = b as ConnectionOrientedTransportBindingElement;
            if (connection == null)
                return false;

            if (this.connectionBufferSize != connection.connectionBufferSize)
                return false;
            if (this.hostNameComparisonMode != connection.hostNameComparisonMode)
                return false;
            if (this.inheritBaseAddressSettings != connection.inheritBaseAddressSettings)
                return false;
            if (this.channelInitializationTimeout != connection.channelInitializationTimeout)
            {
                return false;
            }
            if (this.maxBufferSize != connection.maxBufferSize)
                return false;
            if (this.maxPendingConnections != connection.maxPendingConnections)
                return false;
            if (this.maxOutputDelay != connection.maxOutputDelay)
                return false;
            if (this.maxPendingAccepts != connection.maxPendingAccepts)
                return false;
            if (this.transferMode != connection.transferMode)
                return false;

            return true;
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
       
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMaxPendingAccepts()
        {
            return this.isMaxPendingAcceptsSet;
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMaxPendingConnections()
        {
            return this.isMaxPendingConnectionsSet;
        }
    }
}
