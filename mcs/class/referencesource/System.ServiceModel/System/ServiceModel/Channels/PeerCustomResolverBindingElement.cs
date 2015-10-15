//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.PeerResolvers;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public sealed class PeerCustomResolverBindingElement : PeerResolverBindingElement
    {
        EndpointAddress address;
        Binding binding;
        string bindingSection, bindingConfiguration;
        //this should be PeerCustomResolver?
        PeerResolver resolver;
        ClientCredentials credentials;
        PeerReferralPolicy referralPolicy;

        public PeerCustomResolverBindingElement() { }
        public PeerCustomResolverBindingElement(PeerCustomResolverBindingElement other)
            : base(other)
        {
            this.address = other.address;
            this.bindingConfiguration = other.bindingConfiguration;
            this.bindingSection = other.bindingSection;
            this.binding = other.binding;
            this.resolver = other.resolver;
            this.credentials = other.credentials;
        }

        public PeerCustomResolverBindingElement(PeerCustomResolverSettings settings)
        {
            if (settings != null)
            {
                this.address = settings.Address;
                this.binding = settings.Binding;
                this.resolver = settings.Resolver;
                this.bindingConfiguration = settings.BindingConfiguration;
                this.bindingSection = settings.BindingSection;
            }
        }

        public PeerCustomResolverBindingElement(BindingContext context, PeerCustomResolverSettings settings)
            : this(settings)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

#pragma warning suppress 56506 // Microsoft, context.BindingParameters is never null
            credentials = context.BindingParameters.Find<ClientCredentials>();
        }
        public override T GetProperty<T>(System.ServiceModel.Channels.BindingContext context)
        {
#pragma warning suppress 56506 // context could be null. Pre-4.0 behaviour, won't fix in Dev10.
            return context.GetInnerProperty<T>();
        }
        public EndpointAddress Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }

        public Binding Binding
        {
            get
            {
                return binding;
            }
            set
            {
                binding = value;
            }
        }

        public override PeerReferralPolicy ReferralPolicy
        {
            get
            {
                return referralPolicy;
            }
            set
            {
                if (!PeerReferralPolicyHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(PeerReferralPolicy)));
                }
                referralPolicy = value;
            }
        }

        public override BindingElement Clone()
        {
            return new PeerCustomResolverBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

#pragma warning suppress 56506 // Microsoft, context.BindingParameters is never null
            context.BindingParameters.Add(this);
            credentials = context.BindingParameters.Find<ClientCredentials>();
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
#pragma warning suppress 56506 // Microsoft, context.BindingParameters is never null
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

#pragma warning suppress 56506 // Microsoft, context.BindingParameters is never null
            context.BindingParameters.Add(this);
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
#pragma warning suppress 56506 // Microsoft, context.BindingParameters is never null
            this.credentials = context.BindingParameters.Find<ClientCredentials>();
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override PeerResolver CreatePeerResolver()
        {
            if (resolver == null)
            {
                if (address == null || ((binding == null) && (String.IsNullOrEmpty(this.bindingSection) || String.IsNullOrEmpty(this.bindingConfiguration))))
                    PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();
                if (binding == null)
                {
                    this.binding = ConfigLoader.LookupBinding(this.bindingSection, this.bindingConfiguration);
                    if (binding == null)
                        PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();
                }
                resolver = new PeerDefaultCustomResolverClient();
            }
            if (resolver != null)
            {
                resolver.Initialize(address, binding, credentials, this.referralPolicy);
                if (resolver is PeerDefaultCustomResolverClient)
                {
                    (resolver as PeerDefaultCustomResolverClient).BindingName = this.bindingSection;
                    (resolver as PeerDefaultCustomResolverClient).BindingConfigurationName = this.bindingConfiguration;
                }
            }
            return resolver;
        }
    }
}

