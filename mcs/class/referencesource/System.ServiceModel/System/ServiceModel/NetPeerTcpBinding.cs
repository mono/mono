//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.PeerResolvers;
    using System.Xml;
    using System.ComponentModel;


    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public class NetPeerTcpBinding : Binding, IBindingRuntimePreferences
    {
        // private BindingElements
        PeerTransportBindingElement transport;
        PeerResolverSettings resolverSettings;
        BinaryMessageEncodingBindingElement encoding;
        PeerSecuritySettings peerSecurity;

        public NetPeerTcpBinding() { Initialize(); }
        public NetPeerTcpBinding(string configurationName) : this() { ApplyConfiguration(configurationName); }

        static public bool IsPnrpAvailable
        {
            get
            {
                return PnrpPeerResolver.IsPnrpAvailable;
            }
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

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get { return transport.MaxReceivedMessageSize; }
            set { transport.MaxReceivedMessageSize = value; }
        }

        [DefaultValue(PeerTransportDefaults.ListenIPAddress)]
        [TypeConverter(typeof(PeerTransportListenAddressConverter))]
        public IPAddress ListenIPAddress
        {
            get { return transport.ListenIPAddress; }
            set { transport.ListenIPAddress = value; }
        }

        public PeerSecuritySettings Security
        {
            get { return peerSecurity; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.peerSecurity = value;
            }
        }

        [DefaultValue(PeerTransportDefaults.Port)]
        public int Port
        {
            get { return transport.Port; }
            set { transport.Port = value; }
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

        public PeerResolverSettings Resolver
        {
            get { return this.resolverSettings; }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        public override string Scheme { get { return transport.Scheme; } }

        // Soap version supported by this binding
        public EnvelopeVersion EnvelopeVersion
        {
            get { return EnvelopeVersion.Soap12; }
        }

        void Initialize()
        {
            this.resolverSettings = new PeerResolverSettings();
            transport = new PeerTransportBindingElement();
            encoding = new BinaryMessageEncodingBindingElement();
            peerSecurity = new PeerSecuritySettings();
        }

        void InitializeFrom(PeerTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding)
        {
            Fx.Assert(transport != null, "Invalid null transport.");
            Fx.Assert(encoding != null, "Invalid null encoding.");

            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ListenIPAddress = transport.ListenIPAddress;
            this.Port = transport.Port;
            this.Security.Mode = transport.Security.Mode;
            this.ReaderQuotas = encoding.ReaderQuotas;
        }

        // check that properties of the HttpTransportBindingElement and 
        // MessageEncodingBindingElement not exposed as properties on BasicHttpBinding 
        // match default values of the binding elements
        bool IsBindingElementsMatch(PeerTransportBindingElement transport, BinaryMessageEncodingBindingElement encoding)
        {
            if (!this.transport.IsMatch(transport))
                return false;
            if (!this.encoding.IsMatch(encoding))
                return false;
            return true;
        }

        void ApplyConfiguration(string configurationName)
        {
            NetPeerTcpBindingCollectionElement section = NetPeerTcpBindingCollectionElement.GetBindingCollectionElement();
            NetPeerTcpBindingElement element = section.Bindings[configurationName];
            this.resolverSettings = new PeerResolverSettings();
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.NetPeerTcpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
            this.transport.CreateDefaultResolver(this.Resolver);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();
            switch (this.Resolver.Mode)
            {
                case PeerResolverMode.Auto:
                    {
                        if (CanUseCustomResolver())
                            bindingElements.Add(new PeerCustomResolverBindingElement(this.Resolver.Custom));
                        else if (PeerTransportDefaults.ResolverAvailable)
                            bindingElements.Add(new PnrpPeerResolverBindingElement(this.Resolver.ReferralPolicy));
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerResolverRequired)));
                    }
                    break;
                case PeerResolverMode.Custom:
                    {
                        if (CanUseCustomResolver())
                            bindingElements.Add(new PeerCustomResolverBindingElement(this.Resolver.Custom));
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerResolverSettingsInvalid)));
                    }
                    break;
                case PeerResolverMode.Pnrp:
                    {
                        if (PeerTransportDefaults.ResolverAvailable)
                            bindingElements.Add(new PnrpPeerResolverBindingElement(this.Resolver.ReferralPolicy));
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerResolverRequired)));
                    }
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerResolverRequired)));
            }

            bindingElements.Add(encoding);
            bindingElements.Add(transport);
            transport.Security.Mode = this.Security.Mode;
            transport.Security.Transport.CredentialType = this.Security.Transport.CredentialType;

            return bindingElements.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = null;
            if (elements.Count != 3)
                return false;

            PeerResolverBindingElement resolver = null;
            PeerTransportBindingElement transport = null;
            BinaryMessageEncodingBindingElement encoding = null;

            foreach (BindingElement element in elements)
            {
                if (element is TransportBindingElement)
                    transport = element as PeerTransportBindingElement;
                else if (element is BinaryMessageEncodingBindingElement)
                    encoding = element as BinaryMessageEncodingBindingElement;
                else if (element is PeerResolverBindingElement)
                    resolver = element as PeerResolverBindingElement;
                else
                    return false;
            }

            if (transport == null)
                return false;

            if (encoding == null)
                return false;

            if (resolver == null)
                return false;

            NetPeerTcpBinding netPeerTcpBinding = new NetPeerTcpBinding();
            netPeerTcpBinding.InitializeFrom(transport, encoding);
            if (!netPeerTcpBinding.IsBindingElementsMatch(transport, encoding))
                return false;

            PeerCustomResolverBindingElement customResolver = resolver as PeerCustomResolverBindingElement;
            if (customResolver != null)
            {
                netPeerTcpBinding.Resolver.Custom.Address = customResolver.Address;
                netPeerTcpBinding.Resolver.Custom.Binding = customResolver.Binding;
                netPeerTcpBinding.Resolver.Custom.Resolver = customResolver.CreatePeerResolver();
            }
            else if (resolver is PnrpPeerResolverBindingElement)
            {

                if (NetPeerTcpBinding.IsPnrpAvailable)
                    netPeerTcpBinding.Resolver.Mode = PeerResolverMode.Pnrp;
            }
            binding = netPeerTcpBinding;
            return true;
        }

        bool CanUseCustomResolver()
        {
            return (this.Resolver.Custom.Resolver != null || (this.Resolver.Custom.IsBindingSpecified && this.Resolver.Custom.Address != null));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }
    }
}
