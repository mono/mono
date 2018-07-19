//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Routing;
    using System.Collections.Generic;
    using System.Security.Principal;
    using SR2 = System.ServiceModel.SR;

    class SynchronousSendBindingElement : BindingElement
    {
        public override BindingElement Clone()
        {
            return new SynchronousSendBindingElement();
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return context.GetInnerProperty<T>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            
            Type typeofTChannel = typeof(TChannel);

            if (typeofTChannel == typeof(IDuplexChannel) ||
                typeofTChannel == typeof(IDuplexSessionChannel) ||
                typeofTChannel == typeof(IRequestChannel) ||
                typeofTChannel == typeof(IRequestSessionChannel) || 
                typeofTChannel == typeof(IOutputChannel) ||
                typeofTChannel == typeof(IOutputSessionChannel))
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

            if (!this.CanBuildChannelFactory<TChannel>(context.Clone()))
            {
                throw FxTrace.Exception.Argument("TChannel", SR2.GetString(SR2.ChannelTypeNotSupported, typeof(TChannel)));
            }

            IChannelFactory<TChannel> innerFactory = context.BuildInnerChannelFactory<TChannel>();
            if (innerFactory != null)
            {
                return new SynchronousChannelFactory<TChannel>(context.Binding, innerFactory);
            }
            return null;
        }
      
        class SynchronousChannelFactory<TChannel> : LayeredChannelFactory<TChannel>
        {
            public SynchronousChannelFactory(IDefaultCommunicationTimeouts timeouts, IChannelFactory<TChannel> innerFactory)
                : base(timeouts, innerFactory)
            {
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.InnerChannelFactory.Open(timeout);
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
            {
                IChannelFactory<TChannel> factory = (IChannelFactory<TChannel>)this.InnerChannelFactory;
                TChannel channel = factory.CreateChannel(address, via);
                if (channel != null)
                {
                    Type channelType = typeof(TChannel);
                    if (channelType == typeof(IDuplexSessionChannel))
                    {
                        channel = (TChannel)(object)new SynchronousDuplexSessionChannel(this, (IDuplexSessionChannel)channel);
                    }
                    else if (channelType == typeof(IRequestChannel))
                    {
                        channel = (TChannel)(object)new SynchronousRequestChannel(this, (IRequestChannel)channel);
                    }
                    else if (channelType == typeof(IRequestSessionChannel))
                    {
                        channel = (TChannel)(object)new SynchronousRequestSessionChannel(this, (IRequestSessionChannel)channel);
                    }
                    else if (channelType == typeof(IOutputChannel))
                    {
                        channel = (TChannel)(object)new SynchronousOutputChannel(this, (IOutputChannel)channel);
                    }
                    else if (channelType == typeof(IOutputSessionChannel))
                    {
                        channel = (TChannel)(object)new SynchronousOutputSessionChannel(this, (IOutputSessionChannel)channel);
                    }
                    else if (channelType == typeof(IDuplexChannel))
                    {
                        channel = (TChannel)(object)new SynchronousDuplexChannel(this, (IDuplexChannel)channel);
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(base.CreateChannelTypeNotSupportedException(typeof(TChannel)));
                    }
                }
                return channel;
            }
        }

        abstract class SynchronousChannelBase<TChannel> : LayeredChannel<TChannel>
            where TChannel : class, IChannel
        {
            protected SynchronousChannelBase(ChannelManagerBase manager, TChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.InnerChannel.Open(timeout);
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }
        }

        class SynchronousDuplexChannelBase<TChannel> : SynchronousOutputChannelBase<TChannel>
            where TChannel : class, IDuplexChannel
        {
            public SynchronousDuplexChannelBase(ChannelManagerBase manager, TChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginReceive(timeout, callback, state);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginReceive(callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginTryReceive(timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return this.InnerChannel.EndReceive(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return this.InnerChannel.EndTryReceive(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.InnerChannel.EndWaitForMessage(result);
            }

            public EndpointAddress LocalAddress
            {
                get { return this.InnerChannel.LocalAddress; }
            }

            public Message Receive(TimeSpan timeout)
            {
                return this.InnerChannel.Receive(timeout);
            }

            public Message Receive()
            {
                return this.InnerChannel.Receive();
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                return this.InnerChannel.TryReceive(timeout, out message);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.InnerChannel.WaitForMessage(timeout);
            }
        }

        class SynchronousDuplexChannel : SynchronousDuplexChannelBase<IDuplexChannel>, IDuplexChannel
        {
            public SynchronousDuplexChannel(ChannelManagerBase manager, IDuplexChannel innerChannel)
                : base(manager, innerChannel)
            {
            }
        }

        class SynchronousDuplexSessionChannel : SynchronousDuplexChannelBase<IDuplexSessionChannel>, IDuplexSessionChannel
        {
            public SynchronousDuplexSessionChannel(ChannelManagerBase manager, IDuplexSessionChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            IDuplexSession ISessionChannel<IDuplexSession>.Session
            {
                get { return this.InnerChannel.Session; }
            }
        }

        abstract class SynchronousOutputChannelBase<TChannel> : SynchronousChannelBase<TChannel>
            where TChannel : class, IOutputChannel
        {
            public SynchronousOutputChannelBase(ChannelManagerBase manager, TChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.InnerChannel.Send(message, timeout);
                return new CompletedAsyncResult(callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                this.InnerChannel.Send(message);
                return new CompletedAsyncResult(callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.InnerChannel.RemoteAddress; }
            }

            public void Send(Message message, TimeSpan timeout)
            {
                this.InnerChannel.Send(message, timeout);
            }

            public void Send(Message message)
            {
                this.InnerChannel.Send(message);
            }

            public Uri Via
            {
                get { return this.InnerChannel.Via; }
            }
        }

        class SynchronousOutputChannel : SynchronousOutputChannelBase<IOutputChannel>, IOutputChannel
        {
            public SynchronousOutputChannel(ChannelManagerBase manager, IOutputChannel innerChannel)
                : base(manager, innerChannel)
            {
            }
        }

        class SynchronousOutputSessionChannel : SynchronousOutputChannelBase<IOutputSessionChannel>, IOutputSessionChannel
        {
            public SynchronousOutputSessionChannel(ChannelManagerBase manager, IOutputSessionChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            IOutputSession ISessionChannel<IOutputSession>.Session
            {
                get { return this.InnerChannel.Session; }
            }
        }

        abstract class SynchronousRequestChannelBase<TChannel> : SynchronousChannelBase<TChannel>
            where TChannel : class, IRequestChannel
        {
            public SynchronousRequestChannelBase(ChannelManagerBase manager, TChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                Message reply = this.InnerChannel.Request(message, timeout);
                return new CompletedAsyncResult<Message>(reply, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                Message reply = this.InnerChannel.Request(message);
                return new CompletedAsyncResult<Message>(reply, callback, state);
            }

            public Message EndRequest(IAsyncResult result)
            {
                return CompletedAsyncResult<Message>.End(result);
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.InnerChannel.RemoteAddress; }
            }

            public Message Request(Message message, TimeSpan timeout)
            {
                return this.InnerChannel.Request(message, timeout);
            }

            public Message Request(Message message)
            {
                return this.InnerChannel.Request(message);
            }

            public Uri Via
            {
                get { return this.InnerChannel.Via; }
            }
        }

        class SynchronousRequestChannel : SynchronousRequestChannelBase<IRequestChannel>, IRequestChannel
        {
            public SynchronousRequestChannel(ChannelManagerBase manager, IRequestChannel innerChannel)
                : base(manager, innerChannel)
            {
            }
        }

        class SynchronousRequestSessionChannel : SynchronousRequestChannelBase<IRequestSessionChannel>, IRequestSessionChannel
        {
            public SynchronousRequestSessionChannel(ChannelManagerBase manager, IRequestSessionChannel innerChannel)
                : base(manager, innerChannel)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return this.InnerChannel.Session;
                }
            }
        }
    }
}
