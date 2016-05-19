//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    abstract class NamedPipeChannelListener<TChannel, TChannelAcceptor>
        : NamedPipeChannelListener, IChannelListener<TChannel>
        where TChannel : class, IChannel
        where TChannelAcceptor : ChannelAcceptor<TChannel>
    {
        protected NamedPipeChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
        }

        protected abstract TChannelAcceptor ChannelAcceptor { get; }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            ChannelAcceptor.Open(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, ChannelAcceptor);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            ChannelAcceptor.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnAbort()
        {
            this.ChannelAcceptor.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, ChannelAcceptor);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            return ChannelAcceptor.AcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return ChannelAcceptor.BeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            base.ThrowPending();
            return ChannelAcceptor.EndAcceptChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return ChannelAcceptor.WaitForChannel(timeout);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ChannelAcceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return ChannelAcceptor.EndWaitForChannel(result);
        }
    }

    class NamedPipeReplyChannelListener
        : NamedPipeChannelListener<IReplyChannel, ReplyChannelAcceptor>, ISingletonChannelListener
    {
        ReplyChannelAcceptor replyAcceptor;

        public NamedPipeReplyChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.replyAcceptor = new ConnectionOrientedTransportReplyChannelAcceptor(this);
        }

        protected override ReplyChannelAcceptor ChannelAcceptor
        {
            get { return this.replyAcceptor; }
        }

        TimeSpan ISingletonChannelListener.ReceiveTimeout
        {
            get { return this.InternalReceiveTimeout; }
        }

        void ISingletonChannelListener.ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.NamedPipeChannelMessageReceived,
                    SR.GetString(SR.TraceCodeNamedPipeChannelMessageReceived), requestContext.RequestMessage);
            }
            replyAcceptor.Enqueue(requestContext, callback, canDispatchOnThisThread);
        }
    }

    class NamedPipeDuplexChannelListener
        : NamedPipeChannelListener<IDuplexSessionChannel, InputQueueChannelAcceptor<IDuplexSessionChannel>>, ISessionPreambleHandler
    {
        InputQueueChannelAcceptor<IDuplexSessionChannel> duplexAcceptor;

        public NamedPipeDuplexChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.duplexAcceptor = new InputQueueChannelAcceptor<IDuplexSessionChannel>(this);
        }

        protected override InputQueueChannelAcceptor<IDuplexSessionChannel> ChannelAcceptor
        {
            get { return this.duplexAcceptor; }
        }

        void ISessionPreambleHandler.HandleServerSessionPreamble(ServerSessionPreambleConnectionReader preambleReader,
            ConnectionDemuxer connectionDemuxer)
        {
            IDuplexSessionChannel channel = preambleReader.CreateDuplexSessionChannel(
                this, new EndpointAddress(this.Uri), ExposeConnectionProperty, connectionDemuxer);

            duplexAcceptor.EnqueueAndDispatch(channel, preambleReader.ConnectionDequeuedCallback);
        }
    }

    abstract class NamedPipeChannelListener : ConnectionOrientedTransportChannelListener
    {
        List<SecurityIdentifier> allowedUsers;

        protected NamedPipeChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            SetIdleTimeout(bindingElement.ConnectionPoolSettings.IdleTimeout);
            InitializeMaxPooledConnections(bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);
        }

        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable =
            new UriPrefixTable<ITransportManagerRegistration>();

        public override string Scheme
        {
            get { return Uri.UriSchemeNetPipe; }
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return allowedUsers;
            }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfDisposedOrImmutable();
                    this.allowedUsers = value;
                }
            }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new ExclusiveNamedPipeTransportManager(listenUri, this);
        }

        protected override bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return !(upgradeBindingElement is SslStreamSecurityBindingElement);
        }
    }
}
