//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    sealed class InternalDuplexChannelFactory : LayeredChannelFactory<IDuplexChannel>
    {
        static long channelCount = 0;
        InputChannelDemuxer channelDemuxer;
        IChannelFactory<IOutputChannel> innerChannelFactory;
        IChannelListener<IInputChannel> innerChannelListener;
        LocalAddressProvider localAddressProvider;
        bool providesCorrelation;

        internal InternalDuplexChannelFactory(InternalDuplexBindingElement bindingElement, BindingContext context,
            InputChannelDemuxer channelDemuxer,
            IChannelFactory<IOutputChannel> innerChannelFactory, LocalAddressProvider localAddressProvider)
            : base(context.Binding, innerChannelFactory)
        {
            this.channelDemuxer = channelDemuxer;
            this.innerChannelFactory = innerChannelFactory;
            ChannelDemuxerFilter demuxFilter = new ChannelDemuxerFilter(new MatchNoneMessageFilter(), int.MinValue);
            this.innerChannelListener = this.channelDemuxer.BuildChannelListener<IInputChannel>(demuxFilter);
            this.localAddressProvider = localAddressProvider;
            this.providesCorrelation = bindingElement.ProvidesCorrelation;
        }

        bool CreateUniqueLocalAddress(out EndpointAddress address, out int priority)
        {
            long tempChannelCount = Interlocked.Increment(ref channelCount);
            if (tempChannelCount > 1)
            {
                AddressHeader uniqueEndpointHeader = AddressHeader.CreateAddressHeader(XD.UtilityDictionary.UniqueEndpointHeaderName,
                    XD.UtilityDictionary.UniqueEndpointHeaderNamespace, tempChannelCount);
                address = new EndpointAddress(this.innerChannelListener.Uri, uniqueEndpointHeader);
                priority = 1;
                return true;
            }
            else
            {
                address = new EndpointAddress(this.innerChannelListener.Uri);
                priority = 0;
                return false;
            }
        }

        protected override IDuplexChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            EndpointAddress localAddress;
            int priority;
            MessageFilter filter;
            // determines whether the CompositeDuplex channels created from this factory expect the UniqueEndpoint (ChannelInstance) header in its messages
            bool useUniqueHeader = false;

            if (localAddressProvider != null)
            {
                localAddress = localAddressProvider.LocalAddress;
                filter = localAddressProvider.Filter;
                priority = localAddressProvider.Priority;
            }
            else
            {
                useUniqueHeader = CreateUniqueLocalAddress(out localAddress, out priority);
                filter = new MatchAllMessageFilter();
            }

            return this.CreateChannel(address, via, localAddress, filter, priority, useUniqueHeader);

        }

        public IDuplexChannel CreateChannel(EndpointAddress address, Uri via, MessageFilter filter, int priority, bool usesUniqueHeader)
        {
            return this.CreateChannel(address, via, new EndpointAddress(this.innerChannelListener.Uri), filter, priority, usesUniqueHeader);
        }

        public IDuplexChannel CreateChannel(EndpointAddress remoteAddress, Uri via, EndpointAddress localAddress, MessageFilter filter, int priority, bool usesUniqueHeader)
        {
            ChannelDemuxerFilter demuxFilter = new ChannelDemuxerFilter(new AndMessageFilter(new EndpointAddressMessageFilter(localAddress, true), filter), priority);
            IDuplexChannel newChannel = null;
            IOutputChannel innerOutputChannel = null;
            IChannelListener<IInputChannel> innerInputListener = null;
            IInputChannel innerInputChannel = null;
            try
            {
                innerOutputChannel = this.innerChannelFactory.CreateChannel(remoteAddress, via);
                innerInputListener = this.channelDemuxer.BuildChannelListener<IInputChannel>(demuxFilter);
                innerInputListener.Open();
                innerInputChannel = innerInputListener.AcceptChannel();
                newChannel = new ClientCompositeDuplexChannel(this, innerInputChannel, innerInputListener, localAddress, innerOutputChannel, usesUniqueHeader);
            }
            finally
            {
                if (newChannel == null) // need to cleanup
                {
                    if (innerOutputChannel != null)
                    {
                        innerOutputChannel.Close();
                    }

                    if (innerInputListener != null)
                    {
                        innerInputListener.Close();
                    }

                    if (innerInputChannel != null)
                    {
                        innerInputChannel.Close();
                    }
                }
            }

            return newChannel;
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.innerChannelListener.Abort();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.innerChannelListener.Open(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.innerChannelListener);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            this.innerChannelListener.Close(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.innerChannelListener);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelListener))
            {
                return (T)(object)innerChannelListener;
            }

            if (typeof(T) == typeof(ISecurityCapabilities) && !this.providesCorrelation)
            {
                return InternalDuplexBindingElement.GetSecurityCapabilities<T>(base.GetProperty<ISecurityCapabilities>());
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            IChannelListener channelListener = innerChannelListener;
            if (channelListener != null)
            {
                return channelListener.GetProperty<T>();
            }
            else
            {
                return default(T);
            }
        }

        class ClientCompositeDuplexChannel : LayeredDuplexChannel
        {
            IChannelListener<IInputChannel> innerInputListener;
            bool usesUniqueHeader;  // Perf optimization - don't check message headers if we know there's only one CompositeDuplexChannel created

            public ClientCompositeDuplexChannel(ChannelManagerBase channelManager, IInputChannel innerInputChannel, IChannelListener<IInputChannel> innerInputListener, EndpointAddress localAddress, IOutputChannel innerOutputChannel, bool usesUniqueHeader)
                : base(channelManager, innerInputChannel, localAddress, innerOutputChannel)
            {
                this.innerInputListener = innerInputListener;
                this.usesUniqueHeader = usesUniqueHeader;
            }

            protected override void OnAbort()
            {
                base.OnAbort();
                this.innerInputListener.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.innerInputListener.BeginClose, this.innerInputListener.EndClose);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                this.innerInputListener.Close(timeoutHelper.RemainingTime());
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnReceive(Message message)
            {
                // Mark ChannelInstance header ref params as Understood on message
                // MessageFilters will take care of proper routing of the message, but we need to mark it as understood here. 

                if (usesUniqueHeader)
                {
                    // 3.0 allows for messages to be received with duplicate message headers; we cannot
                    // use MessageHeaders.FindHeader() to find the header because it will throw an exception
                    // if it encounters duplicate headers. We instead have to look through all headers. 

                    for (int i = 0; i < message.Headers.Count; i++)
                    {
                        if (message.Headers[i].Name == XD.UtilityDictionary.UniqueEndpointHeaderName.Value &&
                            message.Headers[i].Namespace == XD.UtilityDictionary.UniqueEndpointHeaderNamespace.Value)
                        {
                            message.Headers.AddUnderstood(i);
                        }
                    }
                }
            }
        }
    }
}
