//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel;
    using System.ServiceModel.PeerResolvers;
    using System.Xml;



    static class PeerTransportPolicyConstants
    {
        public const string PeerTransportSecurityMode = "PeerTransportSecurityMode";
        public const string PeerTransportCredentialType = "PeerTransportCredentialType";
        public const string PeerTransportCredentialTypePassword = "PeerTransportCredentialTypePassword";
        public const string PeerTransportCredentialTypeCertificate = "PeerTransportCredentialTypeCertificate";
        public const string PeerTransportSecurityModeNone = "PeerTransportSecurityModeNone";
        public const string PeerTransportSecurityModeTransport = "PeerTransportSecurityModeTransport";
        public const string PeerTransportSecurityModeMessage = "PeerTransportSecurityModeMessage";
        public const string PeerTransportSecurityModeTransportWithMessageCredential = "PeerTransportSecurityModeTransportWithMessageCredential";
        public const string PeerTransportPrefix = "pc";
    }

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public sealed class PeerTransportBindingElement
        : TransportBindingElement, IWsdlExportExtension, ITransportPolicyImport, IPolicyExportExtension
    {
        IPAddress listenIPAddress;
        int port;
        PeerResolver resolver;
        bool resolverSet;
        PeerSecuritySettings peerSecurity;

        public PeerTransportBindingElement()
            : base()
        {
            this.listenIPAddress = PeerTransportDefaults.ListenIPAddress;
            this.port = PeerTransportDefaults.Port;
            if (PeerTransportDefaults.ResolverAvailable)
            {
                this.resolver = PeerTransportDefaults.CreateResolver();
            }
            peerSecurity = new PeerSecuritySettings();
        }

        PeerTransportBindingElement(PeerTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.listenIPAddress = elementToBeCloned.listenIPAddress;
            this.port = elementToBeCloned.port;
            this.resolverSet = elementToBeCloned.resolverSet;
            this.resolver = elementToBeCloned.resolver;
            peerSecurity = new PeerSecuritySettings(elementToBeCloned.Security);
        }

        public IPAddress ListenIPAddress
        {
            get
            {
                return this.listenIPAddress;
            }

            set
            {
                PeerValidateHelper.ValidateListenIPAddress(value);
                this.listenIPAddress = value;
            }
        }

        public override long MaxReceivedMessageSize
        {
            get
            {
                return base.MaxReceivedMessageSize;
            }

            set
            {
                PeerValidateHelper.ValidateMaxMessageSize(value);
                base.MaxReceivedMessageSize = value;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }

            set
            {
                PeerValidateHelper.ValidatePort(value);
                this.port = value;
            }
        }

        internal PeerResolver Resolver
        {
            get
            {
                return this.resolver;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.GetType() == PeerTransportDefaults.ResolverType)
                {
                    if (!PeerTransportDefaults.ResolverInstalled)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.PeerPnrpNotInstalled));
                    }
                    else if (!PeerTransportDefaults.ResolverAvailable)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.PeerPnrpNotAvailable));
                    }
                }

                this.resolver = value;
                this.resolverSet = true;
            }
        }

        public override string Scheme { get { return PeerStrings.Scheme; } }

        public PeerSecuritySettings Security
        {
            get { return peerSecurity; }
        }


        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");

            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            peerSecurity.OnImportPolicy(importer, context);
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

            peerSecurity.OnExportPolicy(exporter, context);

            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(context.BindingElements, out createdNew);
            if (createdNew && encodingBindingElement is IPolicyExportExtension)
            {
                ((IPolicyExportExtension)encodingBindingElement).ExportPolicy(exporter, context);
            }

            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, encodingBindingElement.MessageVersion.Addressing);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(endpointContext, out createdNew);
            TransportBindingElement.ExportWsdlEndpoint(
                exporter, endpointContext, TransportPolicyConstants.PeerTransportUri,
                encodingBindingElement.MessageVersion.Addressing);
        }

        internal void CreateDefaultResolver(PeerResolverSettings settings)
        {
            if (PeerTransportDefaults.ResolverAvailable)
            {
                this.resolver = new PnrpPeerResolver(settings.ReferralPolicy);
            }
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

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            if (this.ManualAddressing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ManualAddressingNotSupported)));
            }

            PeerResolver peerResolver = GetResolver(context);
            return new PeerChannelFactory<TChannel>(this, context, peerResolver);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            PeerChannelListenerBase peerListener = null;
            PeerResolver peerResolver = GetResolver(context);
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                peerListener = new PeerInputChannelListener(this, context, peerResolver);
            }
            else if (typeof(TChannel) == typeof(IDuplexChannel))
            {
                peerListener = new PeerDuplexChannelListener(this, context, peerResolver);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return (IChannelListener<TChannel>)peerListener;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return (typeof(TChannel) == typeof(IOutputChannel)
                || typeof(TChannel) == typeof(IDuplexChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return (typeof(TChannel) == typeof(IInputChannel)
                || typeof(TChannel) == typeof(IDuplexChannel));
        }

        public override BindingElement Clone()
        {
            return new PeerTransportBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(IBindingMulticastCapabilities))
            {
                return (T)(object)new BindingMulticastCapabilities();
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)new SecurityCapabilities(Security.SupportsAuthentication, Security.SupportsAuthentication,
                    false, Security.SupportedProtectionLevel, Security.SupportedProtectionLevel);
            }
            else if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            }

            return base.GetProperty<T>(context);
        }

        // Return the resolver member (if set) or create one from the resolver binding element in the context
        PeerResolver GetResolver(BindingContext context)
        {
            if (this.resolverSet)
            {
                return this.resolver;
            }

            Collection<PeerCustomResolverBindingElement> customResolverElements
                = context.BindingParameters.FindAll<PeerCustomResolverBindingElement>();

            if (customResolverElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultiplePeerCustomResolverBindingElementsInParameters)));
            }
            else if (customResolverElements.Count == 1)
            {
                context.BindingParameters.Remove<PeerCustomResolverBindingElement>();
                return customResolverElements[0].CreatePeerResolver();
            }


            // If resolver binding element is included in the context, use it to create the resolver. elementToBeClonedwise,
            // if default resolver is available, use it.
            Collection<PeerResolverBindingElement> resolverBindingElements
                = context.BindingParameters.FindAll<PeerResolverBindingElement>();

            if (resolverBindingElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultiplePeerResolverBindingElementsinParameters)));
            }
            else if (resolverBindingElements.Count == 0)
            {
                if (this.resolver != null)  // default resolver available?
                {
                    return this.resolver;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.PeerResolverBindingElementRequired, context.Binding.Name)));
                }
            }
            else if (resolverBindingElements[0].GetType() == PeerTransportDefaults.ResolverBindingElementType)
            {
                if (!PeerTransportDefaults.ResolverInstalled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.PeerPnrpNotInstalled)));
                }
                else if (!PeerTransportDefaults.ResolverAvailable)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.PeerPnrpNotAvailable)));
                }
            }

            context.BindingParameters.Remove<PeerResolverBindingElement>();
            return resolverBindingElements[0].CreatePeerResolver();
        }

        class BindingMulticastCapabilities : IBindingMulticastCapabilities
        {
            public bool IsMulticast { get { return true; } }
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
                get { return false; }
            }
        }


    }
}
