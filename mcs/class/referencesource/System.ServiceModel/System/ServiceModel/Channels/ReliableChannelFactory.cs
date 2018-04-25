//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;

    class ReliableChannelFactory<TChannel, InnerChannel> : ChannelFactoryBase<TChannel>, IReliableFactorySettings
        where InnerChannel : class, IChannel
    {
        TimeSpan acknowledgementInterval;
        FaultHelper faultHelper;
        bool flowControlEnabled;
        TimeSpan inactivityTimeout;
        int maxPendingChannels;
        int maxRetryCount;
        int maxTransferWindowSize;
        MessageVersion messageVersion;
        bool ordered;
        ReliableMessagingVersion reliableMessagingVersion;

        IChannelFactory<InnerChannel> innerChannelFactory;

        public ReliableChannelFactory(ReliableSessionBindingElement settings, IChannelFactory<InnerChannel> innerChannelFactory, Binding binding)
            : base(binding)
        {
            this.acknowledgementInterval = settings.AcknowledgementInterval;
            this.flowControlEnabled = settings.FlowControlEnabled;
            this.inactivityTimeout = settings.InactivityTimeout;
            this.maxPendingChannels = settings.MaxPendingChannels;
            this.maxRetryCount = settings.MaxRetryCount;
            this.maxTransferWindowSize = settings.MaxTransferWindowSize;
            this.messageVersion = binding.MessageVersion;
            this.ordered = settings.Ordered;
            this.reliableMessagingVersion = settings.ReliableMessagingVersion;

            this.innerChannelFactory = innerChannelFactory;
            this.faultHelper = new SendFaultHelper(binding.SendTimeout, binding.CloseTimeout);
        }

        public TimeSpan AcknowledgementInterval
        {
            get { return this.acknowledgementInterval; }
        }

        public FaultHelper FaultHelper
        {
            get { return this.faultHelper; }
        }

        public bool FlowControlEnabled
        {
            get { return this.flowControlEnabled; }
        }

        public TimeSpan InactivityTimeout
        {
            get { return this.inactivityTimeout; }
        }

        protected IChannelFactory<InnerChannel> InnerChannelFactory
        {
            get { return this.innerChannelFactory; }
        }

        public int MaxPendingChannels
        {
            get { return this.maxPendingChannels; }
        }

        public int MaxRetryCount
        {
            get { return this.maxRetryCount; }
        }

        public MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
        }

        public int MaxTransferWindowSize
        {
            get { return this.maxTransferWindowSize; }
        }

        public bool Ordered
        {
            get { return this.ordered; }
        }

        public ReliableMessagingVersion ReliableMessagingVersion
        {
            get { return this.reliableMessagingVersion; }
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

        public TimeSpan SendTimeout
        {
            get { return this.InternalSendTimeout; }
        }

        protected override void OnAbort()
        {
            // Aborting base first to abort channels. Must abort higher channels before aborting lower channels.
            base.OnAbort();
            this.faultHelper.Abort();
            this.innerChannelFactory.Abort();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannelFactory.Open(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelFactory.BeginOpen(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannelFactory.EndOpen(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            // Closing base first to close channels.  Must close higher channels before closing lower channels.
            base.OnClose(timeoutHelper.RemainingTime());
            this.faultHelper.Close(timeoutHelper.RemainingTime());
            this.innerChannelFactory.Close(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // Closing base first to close channels.  Must close higher channels before closing lower channels.
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(
                timeout,
                new OperationWithTimeoutBeginCallback[] 
                {
                    new OperationWithTimeoutBeginCallback(base.OnBeginClose),
                    new OperationWithTimeoutBeginCallback(this.faultHelper.BeginClose),
                    new OperationWithTimeoutBeginCallback(this.innerChannelFactory.BeginClose) 
                },
                new OperationEndCallback[] 
                {
                    new OperationEndCallback(base.OnEndClose),
                    new OperationEndCallback(this.faultHelper.EndClose),
                    new OperationEndCallback(this.innerChannelFactory.EndClose)
                },
                callback, 
                state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            LateBoundChannelParameterCollection channelParameters = new LateBoundChannelParameterCollection();

            IClientReliableChannelBinder binder = ClientReliableChannelBinder<InnerChannel>.CreateBinder(address, via,
                this.InnerChannelFactory, MaskingMode.All,
                TolerateFaultsMode.IfNotSecuritySession, channelParameters, this.DefaultCloseTimeout,
                this.DefaultSendTimeout);

            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                if (typeof(InnerChannel) == typeof(IDuplexChannel) || typeof(InnerChannel) == typeof(IDuplexSessionChannel))
                    return (TChannel)(object)new ReliableOutputSessionChannelOverDuplex(this, this, binder, this.faultHelper, channelParameters);

                // typeof(InnerChannel) == typeof(IRequestChannel) || typeof(InnerChannel) == typeof(IRequestSessionChannel))
                return (TChannel)(object)new ReliableOutputSessionChannelOverRequest(this, this, binder, this.faultHelper, channelParameters);
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel)(object)new ClientReliableDuplexSessionChannel(this, this, binder, this.faultHelper, channelParameters, WsrmUtilities.NextSequenceId());
            }

            // (typeof(TChannel) == typeof(IRequestSessionChannel)
            return (TChannel)(object)new ReliableRequestSessionChannel(this, this, binder, this.faultHelper, channelParameters, WsrmUtilities.NextSequenceId());
        }
    }
}
