//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class DiscoveryClientChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        DiscoveryEndpointProvider discoveryEndpointProvider;
        FindCriteria findCriteria;

        IChannelFactory<TChannel> innerChannelFactory;

        public DiscoveryClientChannelFactory(IChannelFactory<TChannel> innerChannelFactory, FindCriteria findCriteria, DiscoveryEndpointProvider discoveryEndpointProvider)
        {
            Fx.Assert(findCriteria != null, "The findCriteria must be non null.");
            Fx.Assert(discoveryEndpointProvider != null, "The discoveryEndpointProvider must be non null.");
            Fx.Assert(innerChannelFactory != null, "The innerChannelFactory must be non null.");

            this.findCriteria = findCriteria;
            this.discoveryEndpointProvider = discoveryEndpointProvider;
            this.innerChannelFactory = innerChannelFactory;            
        }        

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            if (!address.Equals(DiscoveryClientBindingElement.DiscoveryEndpointAddress))
            {
                throw FxTrace.Exception.Argument(
                    "address",
                    Discovery.SR.DiscoveryEndpointAddressIncorrect("address", address.Uri, DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri));
            }

            if (!via.Equals(DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri))
            {
                throw FxTrace.Exception.Argument(
                    "via",
                    Discovery.SR.DiscoveryEndpointAddressIncorrect("via", via, DiscoveryClientBindingElement.DiscoveryEndpointAddress.Uri));
            }

            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel)(object)new DiscoveryClientOutputChannel<IOutputChannel>(
                    this,
                    (IChannelFactory<IOutputChannel>)this.innerChannelFactory, 
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }
            else if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel)(object)new DiscoveryClientRequestChannel<IRequestChannel>(
                    this,
                    (IChannelFactory<IRequestChannel>)this.innerChannelFactory,
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }
            else if (typeof(TChannel) == typeof(IDuplexChannel))            
            {
                return (TChannel)(object)new DiscoveryClientDuplexChannel<IDuplexChannel>(
                    this,
                    (IChannelFactory<IDuplexChannel>)this.innerChannelFactory,
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }
            else if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel)(object)new DiscoveryClientOutputSessionChannel(
                    this,
                    (IChannelFactory<IOutputSessionChannel>)this.innerChannelFactory,
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel)(object)new DiscoveryClientRequestSessionChannel(
                    this,
                    (IChannelFactory<IRequestSessionChannel>)this.innerChannelFactory,
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel)(object)new DiscoveryClientDuplexSessionChannel(
                    this,
                    (IChannelFactory<IDuplexSessionChannel>)this.innerChannelFactory,
                    this.findCriteria,
                    this.discoveryEndpointProvider);
            }

            throw FxTrace.Exception.Argument("TChannel", ServiceModel.SR.GetString(ServiceModel.SR.ChannelTypeNotSupported, typeof(TChannel)));
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelFactory<TChannel>))
            {
                return (T)(object)this;
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return this.innerChannelFactory.GetProperty<T>();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelFactory.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannelFactory.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannelFactory.Open(timeout);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // Must close higher channels before closing lower channels.
            return new ChainedAsyncResult(
                timeout, 
                callback, 
                state, 
                base.OnBeginClose, 
                base.OnEndClose, 
                this.innerChannelFactory.BeginClose,
                this.innerChannelFactory.EndClose);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            // Must close higher channels before closing lower channels.
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            this.innerChannelFactory.Close(timeoutHelper.RemainingTime());
        }

        protected override void OnAbort()
        {
            // Must abort higher channels before aborting lower channels.
            base.OnAbort();            
            this.innerChannelFactory.Abort();
        }
    }
}
