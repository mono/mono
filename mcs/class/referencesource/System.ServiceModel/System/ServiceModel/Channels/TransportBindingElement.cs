//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;
    using System.ServiceModel.Security;
    using System.ServiceModel;
    using System.Xml;
    using WsdlNS = System.Web.Services.Description;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public abstract class TransportBindingElement
        : BindingElement
    {
        bool manualAddressing;
        long maxBufferPoolSize;
        long maxReceivedMessageSize;

        protected TransportBindingElement()
        {
            this.manualAddressing = TransportDefaults.ManualAddressing;
            this.maxBufferPoolSize = TransportDefaults.MaxBufferPoolSize;
            this.maxReceivedMessageSize = TransportDefaults.MaxReceivedMessageSize;
        }

        protected TransportBindingElement(TransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.manualAddressing = elementToBeCloned.manualAddressing;
            this.maxBufferPoolSize = elementToBeCloned.maxBufferPoolSize;
            this.maxReceivedMessageSize = elementToBeCloned.maxReceivedMessageSize;
        }

        [DefaultValue(TransportDefaults.ManualAddressing)]
        public virtual bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }

            set
            {
                this.manualAddressing = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public virtual long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.maxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public virtual long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));
                }
                this.maxReceivedMessageSize = value;
            }
        }

        public abstract string Scheme { get; }

        internal static IChannelFactory<TChannel> CreateChannelFactory<TChannel>(TransportBindingElement transport)
        {
            Binding binding = new CustomBinding(transport);
            return binding.BuildChannelFactory<TChannel>();
        }

        internal static IChannelListener CreateChannelListener<TChannel>(TransportBindingElement transport)
            where TChannel : class, IChannel
        {
            Binding binding = new CustomBinding(transport);
            return binding.BuildChannelListener<TChannel>();
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements myRequirements = this.GetProtectionRequirements(context);
                myRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T)(object)myRequirements;
            }

            // to cover all our bases, let's iterate through the BindingParameters to make sure
            // we haven't missed a query (since we're the Transport and we're at the bottom)
#pragma warning suppress 56506 // [....], BindingContext.BindingParameters cannot be null
            Collection<BindingElement> bindingElements = context.BindingParameters.FindAll<BindingElement>();

            T result = default(T);
            for (int i = 0; i < bindingElements.Count; i++)
            {
                result = bindingElements[i].GetIndividualProperty<T>();
                if (result != default(T))
                {
                    return result;
                }
            }

            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)TransportDefaults.GetDefaultMessageEncoderFactory().MessageVersion;
            }

            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T)(object)new XmlDictionaryReaderQuotas();
            }

            return null;
        }

        ChannelProtectionRequirements GetProtectionRequirements(AddressingVersion addressingVersion)
        {
            ChannelProtectionRequirements result = new ChannelProtectionRequirements();
            result.IncomingSignatureParts.AddParts(addressingVersion.SignedMessageParts);
            result.OutgoingSignatureParts.AddParts(addressingVersion.SignedMessageParts);
            return result;
        }

        internal ChannelProtectionRequirements GetProtectionRequirements(BindingContext context)
        {
            AddressingVersion addressingVersion = AddressingVersion.WSAddressing10;
#pragma warning suppress 56506 // [....], CustomBinding.Elements can never be null
            MessageEncodingBindingElement messageEncoderBindingElement = context.Binding.Elements.Find<MessageEncodingBindingElement>();
            if (messageEncoderBindingElement != null)
            {
                addressingVersion = messageEncoderBindingElement.MessageVersion.Addressing;
            }
            return GetProtectionRequirements(addressingVersion);
        }

        internal static void ExportWsdlEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext,
            string wsdlTransportUri, AddressingVersion addressingVersion)
        {
            ExportWsdlEndpoint(exporter, endpointContext, wsdlTransportUri, endpointContext.Endpoint.Address, addressingVersion);
        }

        internal static void ExportWsdlEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext,
            string wsdlTransportUri, EndpointAddress address, AddressingVersion addressingVersion)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (endpointContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");
            }

            // Set SoapBinding Transport URI
#pragma warning suppress 56506 // [....], these properties cannot be null in this context
            BindingElementCollection bindingElements = endpointContext.Endpoint.Binding.CreateBindingElements();
            if (wsdlTransportUri != null)
            {
                WsdlNS.SoapBinding soapBinding = SoapHelper.GetOrCreateSoapBinding(endpointContext, exporter);

                if (soapBinding != null)
                {
                    soapBinding.Transport = wsdlTransportUri;
                }
            }

            if (endpointContext.WsdlPort != null)
            {
                WsdlExporter.WSAddressingHelper.AddAddressToWsdlPort(endpointContext.WsdlPort, address, addressingVersion);
            }
        }
        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            TransportBindingElement transport = b as TransportBindingElement;
            if (transport == null)
            {
                return false;
            }
            if (this.maxBufferPoolSize != transport.MaxBufferPoolSize)
            {
                return false;
            }
            if (this.maxReceivedMessageSize != transport.MaxReceivedMessageSize)
            {
                return false;
            }
            return true;
        }
    }
}
