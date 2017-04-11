//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Dispatcher;

    sealed class InternalDuplexBindingElement : BindingElement
    {
        InputChannelDemuxer clientChannelDemuxer;
        bool providesCorrelation;

        public InternalDuplexBindingElement()
            : this(false)
        {
        }

        // 


        internal InternalDuplexBindingElement(bool providesCorrelation)
            : base()
        {
            this.providesCorrelation = providesCorrelation;
        }

        InternalDuplexBindingElement(InternalDuplexBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.clientChannelDemuxer = elementToBeCloned.ClientChannelDemuxer;
            this.providesCorrelation = elementToBeCloned.ProvidesCorrelation;
        }

        internal InputChannelDemuxer ClientChannelDemuxer
        {
            get { return this.clientChannelDemuxer; }
        }

        internal bool ProvidesCorrelation
        {
            get { return this.providesCorrelation; }
        }

        public override BindingElement Clone()
        {
            return new InternalDuplexBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            IChannelFactory<IOutputChannel> innerChannelFactory = context.Clone().BuildInnerChannelFactory<IOutputChannel>();

            if (this.clientChannelDemuxer == null)
            {
                this.clientChannelDemuxer = new InputChannelDemuxer(context);
            }
            else
            {
#pragma warning suppress 56506 // Microsoft, context.RemainingBindingElements will never be null
                context.RemainingBindingElements.Clear();
            }
            LocalAddressProvider localAddressProvider = context.BindingParameters.Remove<LocalAddressProvider>();
            return (IChannelFactory<TChannel>)(object)
                new InternalDuplexChannelFactory(this, context, this.clientChannelDemuxer, innerChannelFactory, localAddressProvider);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IDuplexChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel",
                    SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return (IChannelListener<TChannel>)(object)new InternalDuplexChannelListener(this, context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            return typeof(TChannel) == typeof(IDuplexChannel)
                && context.CanBuildInnerChannelFactory<IOutputChannel>()
                && context.CanBuildInnerChannelListener<IInputChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            return typeof(TChannel) == typeof(IDuplexChannel)
                && context.CanBuildInnerChannelFactory<IOutputChannel>()
                && context.CanBuildInnerChannelListener<IInputChannel>();
        }

        internal static T GetSecurityCapabilities<T>(ISecurityCapabilities lowerCapabilities)
        {
            Fx.Assert(typeof(T) == typeof(ISecurityCapabilities), "Can only be used with ISecurityCapabilities");
            if (lowerCapabilities != null)
            {
                // composite duplex cannot ensure that messages it receives are from the part it sends
                // messages to. So it cannot offer server auth
                return (T)(object)(new SecurityCapabilities(lowerCapabilities.SupportsClientAuthentication,
                    false, lowerCapabilities.SupportsClientWindowsIdentity, lowerCapabilities.SupportedRequestProtectionLevel,
                    System.Net.Security.ProtectionLevel.None));
            }
            else
            {
                return (T)(object)null;
            }
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(ISecurityCapabilities) && !this.ProvidesCorrelation)
            {
                return InternalDuplexBindingElement.GetSecurityCapabilities<T>(context.GetInnerProperty<ISecurityCapabilities>());
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
                return false;
            return (b is InternalDuplexBindingElement);
        }

        public static void AddDuplexFactorySupport(BindingContext context, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if (context.CanBuildInnerChannelFactory<IDuplexChannel>())
                return;
            if (context.RemainingBindingElements.Find<CompositeDuplexBindingElement>() == null)
                return;

            if (context.CanBuildInnerChannelFactory<IOutputChannel>() &&
                context.CanBuildInnerChannelListener<IInputChannel>())
            {
                if (context.CanBuildInnerChannelFactory<IRequestChannel>())
                    return;
                if (context.CanBuildInnerChannelFactory<IRequestSessionChannel>())
                    return;
                if (context.CanBuildInnerChannelFactory<IOutputSessionChannel>())
                    return;
                if (context.CanBuildInnerChannelFactory<IDuplexSessionChannel>())
                    return;

                if (internalDuplexBindingElement == null)
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                context.RemainingBindingElements.Insert(0, internalDuplexBindingElement);
            }
        }

        public static void AddDuplexListenerSupport(BindingContext context, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if (context.CanBuildInnerChannelListener<IDuplexChannel>())
                return;
            if (context.RemainingBindingElements.Find<CompositeDuplexBindingElement>() == null)
                return;

            if (context.CanBuildInnerChannelFactory<IOutputChannel>() &&
                context.CanBuildInnerChannelListener<IInputChannel>())
            {
                if (context.CanBuildInnerChannelListener<IReplyChannel>())
                    return;
                if (context.CanBuildInnerChannelListener<IReplySessionChannel>())
                    return;
                if (context.CanBuildInnerChannelListener<IInputSessionChannel>())
                    return;
                if (context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
                    return;

                if (internalDuplexBindingElement == null)
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                context.RemainingBindingElements.Insert(0, internalDuplexBindingElement);
            }
        }

        public static void AddDuplexListenerSupport(CustomBinding binding, ref InternalDuplexBindingElement internalDuplexBindingElement)
        {
            if (binding.CanBuildChannelListener<IDuplexChannel>())
                return;
            if (binding.Elements.Find<CompositeDuplexBindingElement>() == null)
                return;

            if (binding.CanBuildChannelFactory<IOutputChannel>() &&
                binding.CanBuildChannelListener<IInputChannel>())
            {
                if (binding.CanBuildChannelListener<IReplyChannel>())
                    return;
                if (binding.CanBuildChannelListener<IReplySessionChannel>())
                    return;
                if (binding.CanBuildChannelListener<IInputSessionChannel>())
                    return;
                if (binding.CanBuildChannelListener<IDuplexSessionChannel>())
                    return;

                if (internalDuplexBindingElement == null)
                    internalDuplexBindingElement = new InternalDuplexBindingElement();
                binding.Elements.Insert(0, internalDuplexBindingElement);
            }
        }
    }

    class LocalAddressProvider
    {
        EndpointAddress localAddress;
        MessageFilter filter;
        int priority;

        public LocalAddressProvider(EndpointAddress localAddress, MessageFilter filter)
        {
            if (localAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localAddress");
            }
            if (filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter");
            }
            this.localAddress = localAddress;
            this.filter = filter;

            if (localAddress.Headers.FindHeader(XD.UtilityDictionary.UniqueEndpointHeaderName.Value,
                    XD.UtilityDictionary.UniqueEndpointHeaderNamespace.Value) == null)
            {
                this.priority = Int32.MaxValue - 1;
            }
            else
            {
                this.priority = Int32.MaxValue;
            }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        public MessageFilter Filter
        {
            get { return this.filter; }
        }

        public int Priority
        {
            get { return this.priority; }
        }
    }
}
