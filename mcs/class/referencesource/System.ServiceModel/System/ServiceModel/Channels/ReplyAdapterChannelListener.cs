//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Threading;

    abstract class ReplyOverDuplexChannelListenerBase<TOuterChannel, TInnerChannel> : LayeredChannelListener<TOuterChannel>
        where TOuterChannel : class, IReplyChannel
        where TInnerChannel : class, IDuplexChannel
    {
        IChannelListener<TInnerChannel> innerChannelListener;

        public ReplyOverDuplexChannelListenerBase(BindingContext context)
            : base(context.Binding, context.BuildInnerChannelListener<TInnerChannel>())
        {
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<TInnerChannel>)this.InnerChannelListener;
            base.OnOpening();
        }

        protected override TOuterChannel OnAcceptChannel(TimeSpan timeout)
        {
            TInnerChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
            return WrapInnerChannel(innerChannel);
        }


        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override TOuterChannel OnEndAcceptChannel(IAsyncResult result)
        {
            TInnerChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
            return WrapInnerChannel(innerChannel);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected abstract TOuterChannel CreateWrappedChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel);

        TOuterChannel WrapInnerChannel(TInnerChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return null;
            }
            else
            {
                return CreateWrappedChannel(this, innerChannel);
            }
        }
    }

    class ReplyOverDuplexChannelListener : ReplyOverDuplexChannelListenerBase<IReplyChannel, IDuplexChannel>
    {
        public ReplyOverDuplexChannelListener(BindingContext context)
            : base(context)
        {
        }

        protected override IReplyChannel CreateWrappedChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
        {
            return new ReplyOverDuplexChannel(channelManager, innerChannel);
        }
    }

    class ReplySessionOverDuplexSessionChannelListener : ReplyOverDuplexChannelListenerBase<IReplySessionChannel, IDuplexSessionChannel>
    {
        public ReplySessionOverDuplexSessionChannelListener(BindingContext context)
            : base(context)
        {
        }

        protected override IReplySessionChannel CreateWrappedChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel)
        {
            return new ReplySessionOverDuplexSessionChannel(channelManager, innerChannel);
        }
    }


    abstract class ReplyOverDuplexChannelBase<TInnerChannel> : LayeredChannel<TInnerChannel>, IReplyChannel
        where TInnerChannel : class, IDuplexChannel
    {
        public ReplyOverDuplexChannelBase(ChannelManagerBase channelManager, TInnerChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.InnerChannel.LocalAddress;
            }
        }

        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(this.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return ReplyChannel.HelpReceiveRequest(this, timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ReplyChannel.HelpEndReceiveRequest(result);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            Message message;
            if (!this.InnerChannel.TryReceive(timeout, out message))
            {
                context = null;
                return false;
            }
            context = WrapInnerMessage(message);
            return true;
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            Message message;
            if (!this.InnerChannel.EndTryReceive(result, out message))
            {
                context = null;
                return false;
            }
            context = WrapInnerMessage(message);
            return true;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForMessage(result);
        }

        RequestContext WrapInnerMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }
            else
            {
                return new DuplexRequestContext(message, this.Manager, this.InnerChannel);
            }
        }

        class DuplexRequestContext : RequestContext
        {
            IDuplexChannel innerChannel;
            IDefaultCommunicationTimeouts defaultTimeouts;
            Message request;
            EndpointAddress replyTo;
            bool disposed;
            Object thisLock;

            public DuplexRequestContext(Message request, IDefaultCommunicationTimeouts defaultTimeouts, IDuplexChannel innerChannel)
            {
                this.request = request;
                this.defaultTimeouts = defaultTimeouts;
                this.innerChannel = innerChannel;
                if (request != null)
                {
                    replyTo = request.Headers.ReplyTo;
                }
                thisLock = new Object();
            }

            public override Message RequestMessage
            {
                get
                {
                    return this.request;
                }
            }

            public override void Abort()
            {
                Dispose(true);
            }

            public override void Close()
            {
                this.Close(this.defaultTimeouts.CloseTimeout);
            }

            public override void Close(TimeSpan timeout)
            {
                this.Dispose(true);
            }

            public override void Reply(Message message)
            {
                this.Reply(message, this.defaultTimeouts.SendTimeout);
            }

            public override void Reply(Message message, TimeSpan timeout)
            {
                PrepareReply(message);
                this.innerChannel.Send(message);
            }

            public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
            {
                return BeginReply(message, this.defaultTimeouts.SendTimeout, callback, state);
            }

            public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                PrepareReply(message);
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            public override void EndReply(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            void PrepareReply(Message message)
            {
                if (this.replyTo != null)
                {
                    this.replyTo.ApplyTo(message);
                }
            }

            protected override void Dispose(bool disposing)
            {
                bool cleanup = false;
                lock (thisLock)
                {
                    if (!disposed)
                    {
                        this.disposed = true;
                        cleanup = true;
                    }
                }
                if (cleanup && this.request != null)
                {
                    this.request.Close();
                }
            }
        }
    }

    class ReplyOverDuplexChannel : ReplyOverDuplexChannelBase<IDuplexChannel>
    {
        public ReplyOverDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }
    }

    class ReplySessionOverDuplexSessionChannel : ReplyOverDuplexChannelBase<IDuplexSessionChannel>, IReplySessionChannel
    {
        ReplySessionOverDuplexSession session;

        public ReplySessionOverDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel)
            : base(channelManager, innerChannel)
        {
            this.session = new ReplySessionOverDuplexSession(innerChannel.Session);
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        class ReplySessionOverDuplexSession : IInputSession
        {
            IDuplexSession innerSession;

            public ReplySessionOverDuplexSession(IDuplexSession innerSession)
            {
                if (innerSession == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerSession");
                }
                this.innerSession = innerSession;
            }

            public string Id
            {
                get
                {
                    return this.innerSession.Id;
                }
            }
        }
    }

    class ReplyAdapterBindingElement : BindingElement
    {
        public ReplyAdapterBindingElement()
        {
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            if (context.CanBuildInnerChannelListener<IReplySessionChannel>() ||
                context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                return context.BuildInnerChannelListener<TChannel>();
            }
            else if ((typeof(TChannel) == typeof(IReplySessionChannel)) && context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                return (IChannelListener<TChannel>)new ReplySessionOverDuplexSessionChannelListener(context);
            }
            else if ((typeof(TChannel) == typeof(IReplyChannel)) && context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                return (IChannelListener<TChannel>)new ReplyOverDuplexChannelListener(context);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                return (context.CanBuildInnerChannelListener<IReplySessionChannel>()
                        || context.CanBuildInnerChannelListener<IDuplexSessionChannel>());
            }
            else if (typeof(TChannel) == typeof(IReplyChannel))
            {
                return (context.CanBuildInnerChannelListener<IReplyChannel>()
                    || context.CanBuildInnerChannelListener<IDuplexChannel>());
            }
            else
            {
                return false;
            }
        }

        public override BindingElement Clone()
        {
            return new ReplyAdapterBindingElement();
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }
    }
}
