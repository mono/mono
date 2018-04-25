//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;


    [Fx.Tag.XamlVisible(false)]
    sealed public class DiscoveryClientBindingElement : BindingElement
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes, Justification = "EndpointAddress is an immutable type.")]
        public static readonly EndpointAddress DiscoveryEndpointAddress = new EndpointAddress("http://schemas.microsoft.com/discovery/dynamic");

        DiscoveryEndpointProvider discoveryEndpointProvider;        
        FindCriteria findCriteria;        

        public DiscoveryClientBindingElement()            
        {
            this.FindCriteria = new FindCriteria();
            this.DiscoveryEndpointProvider = new UdpDiscoveryEndpointProvider();
        }

        public DiscoveryClientBindingElement(DiscoveryEndpointProvider discoveryEndpointProvider, FindCriteria findCriteria)
        {
            if (discoveryEndpointProvider == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryEndpointProvider");
            }            

            if (findCriteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("findCriteria");
            }

            this.findCriteria = findCriteria;
            this.discoveryEndpointProvider = discoveryEndpointProvider;
        }

        private DiscoveryClientBindingElement(DiscoveryClientBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.discoveryEndpointProvider = elementToBeCloned.DiscoveryEndpointProvider;
            this.findCriteria = elementToBeCloned.FindCriteria.Clone();;
        }

        public DiscoveryEndpointProvider DiscoveryEndpointProvider
        {
            get
            {
                return this.discoveryEndpointProvider;
            }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.discoveryEndpointProvider = value;
            }
        }

        public FindCriteria FindCriteria
        {
            get
            {
                return this.findCriteria;
            }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                this.findCriteria = value;
            }
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            if (typeof(TChannel) == typeof(IOutputChannel)
                || typeof(TChannel) == typeof(IDuplexChannel)
                || typeof(TChannel) == typeof(IRequestChannel)
                || typeof(TChannel) == typeof(IOutputSessionChannel)
                || typeof(TChannel) == typeof(IRequestSessionChannel)
                || typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return context.CanBuildInnerChannelFactory<TChannel>();
            }

            return false;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            if (context.Binding.Elements.IndexOf(this) != 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DiscoveryClientBindingElementNotFirst));
            }

            if (this.CanBuildChannelFactory<TChannel>(context))
            {
                return new DiscoveryClientChannelFactory<TChannel>(
                    context.BuildInnerChannelFactory<TChannel>(),
                    this.FindCriteria,
                    this.DiscoveryEndpointProvider);
            }
            else
            {
                throw FxTrace.Exception.Argument("TChannel", ServiceModel.SR.GetString(ServiceModel.SR.ChannelTypeNotSupported, typeof(TChannel)));
            }
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return false;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            throw FxTrace.Exception.Argument("TChannel", ServiceModel.SR.GetString(ServiceModel.SR.ChannelTypeNotSupported, typeof(TChannel)));
        }

        public override BindingElement Clone()
        {
            return new DiscoveryClientBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return context.GetInnerProperty<T>();
        }      
    }
}
