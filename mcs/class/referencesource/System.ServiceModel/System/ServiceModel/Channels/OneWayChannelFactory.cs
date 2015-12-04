//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;

    class PacketRoutableHeader : DictionaryHeader
    {
        PacketRoutableHeader()
            : base()
        {
        }

        public static void AddHeadersTo(Message message, MessageHeader header)
        {
            int index = message.Headers.FindHeader(DotNetOneWayStrings.HeaderName, DotNetOneWayStrings.Namespace);
            if (index == -1)
            {
                if (header == null)
                {
                    header = PacketRoutableHeader.Create();
                }
                message.Headers.Add(header);
            }
        }

        public static void ValidateMessage(Message message)
        {
            if (!TryValidateMessage(message))
            {
                throw TraceUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.OneWayHeaderNotFound)), message);
            }
        }

        public static bool TryValidateMessage(Message message)
        {
            int index = message.Headers.FindHeader(
                DotNetOneWayStrings.HeaderName, DotNetOneWayStrings.Namespace);

            return (index != -1);
        }

        public static PacketRoutableHeader Create()
        {
            return new PacketRoutableHeader();
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.DotNetOneWayDictionary.HeaderName; }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return XD.DotNetOneWayDictionary.Namespace; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            // no contents necessary
        }
    }

    /// <summary>
    /// OneWayChannelFactory built on top of IRequestChannel
    /// </summary>
    class RequestOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        PacketRoutableHeader packetRoutableHeader;

        public RequestOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.BuildInnerChannelFactory<IRequestChannel>())
        {
            if (bindingElement.PacketRoutable)
            {
                this.packetRoutableHeader = PacketRoutableHeader.Create();
            }
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            IRequestChannel innerChannel =
                ((IChannelFactory<IRequestChannel>)this.InnerChannelFactory).CreateChannel(to, via);

            return new RequestOutputChannel(this, innerChannel, this.packetRoutableHeader);
        }

        class RequestOutputChannel : OutputChannel
        {
            IRequestChannel innerChannel;
            MessageHeader packetRoutableHeader;

            public RequestOutputChannel(ChannelManagerBase factory,
                IRequestChannel innerChannel, MessageHeader packetRoutableHeader)
                : base(factory)
            {
                this.innerChannel = innerChannel;
                this.packetRoutableHeader = packetRoutableHeader;
            }

            #region Inner Channel delegation
            public override EndpointAddress RemoteAddress
            {
                get { return this.innerChannel.RemoteAddress; }
            }

            public override Uri Via
            {
                get { return this.innerChannel.Via; }
            }

            protected override void OnAbort()
            {
                this.innerChannel.Abort();
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerChannel.Close(timeout);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginClose(timeout, callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerChannel.EndClose(result);
            }

            public override T GetProperty<T>()
            {
                T result = base.GetProperty<T>();

                if (result == null)
                {
                    result = this.innerChannel.GetProperty<T>();
                }

                return result;
            }
            #endregion

            // add our oneWay header to every message (if it's not already there)
            protected override void AddHeadersTo(Message message)
            {
                base.AddHeadersTo(message);

                if (this.packetRoutableHeader != null)
                {
                    PacketRoutableHeader.AddHeadersTo(message, this.packetRoutableHeader);
                }
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                Message response = this.innerChannel.Request(message, timeout);
                using (response)
                {
                    ValidateResponse(response);
                }
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginRequest(message, timeout, callback, state);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                Message response = this.innerChannel.EndRequest(result);
                using (response)
                {
                    ValidateResponse(response);
                }
            }

            void ValidateResponse(Message response)
            {
                if (response != null)
                {
                    if (response.Version == MessageVersion.None && response is NullMessage)
                    {
                        response.Close();
                        return;
                    }

                    Exception innerException = null;

                    if (response.IsFault)
                    {
                        try
                        {
                            MessageFault messageFault = MessageFault.CreateFault(response, TransportDefaults.MaxFaultSize);
                            innerException = new FaultException(messageFault);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            if (e is CommunicationException ||
                                e is TimeoutException ||
                                e is XmlException ||
                                e is IOException)
                            {
                                innerException = e; // expected exception generating fault
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    throw TraceUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.OneWayUnexpectedResponse), innerException),
                        response);
                }
            }
        }
    }

    // <summary>
    // OneWayChannelFactory built on top of IDuplexChannel
    // </summary>
    class DuplexOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        IChannelFactory<IDuplexChannel> innnerFactory;
        bool packetRoutable;

        public DuplexOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.BuildInnerChannelFactory<IDuplexChannel>())
        {
            this.innnerFactory = (IChannelFactory<IDuplexChannel>)this.InnerChannelFactory;
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            IDuplexChannel channel = this.innnerFactory.CreateChannel(address, via);
            return new DuplexOutputChannel(this, channel);
        }

        class DuplexOutputChannel : OutputChannel
        {
            IDuplexChannel innerChannel;
            bool packetRoutable;

            public DuplexOutputChannel(DuplexOneWayChannelFactory factory, IDuplexChannel innerChannel)
                : base(factory)
            {
                this.packetRoutable = factory.packetRoutable;
                this.innerChannel = innerChannel;
            }

            public override EndpointAddress RemoteAddress
            {
                get { return this.innerChannel.RemoteAddress; }
            }

            public override Uri Via
            {
                get { return this.innerChannel.Via; }
            }

            protected override void OnAbort()
            {
                this.innerChannel.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginClose(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                StampMessage(message);
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerChannel.Close(timeout);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerChannel.EndClose(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                StampMessage(message);
                this.innerChannel.Send(message, timeout);
            }

            void StampMessage(Message message)
            {
                if (this.packetRoutable)
                {
                    PacketRoutableHeader.AddHeadersTo(message, null);
                }
            }
        }
    }

    /// <summary>
    /// OneWayChannelFactory built on top of IDuplexSessionChannel
    /// </summary>
    class DuplexSessionOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        ChannelPool<IDuplexSessionChannel> channelPool;
        ChannelPoolSettings channelPoolSettings;
        bool packetRoutable;

        public DuplexSessionOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.BuildInnerChannelFactory<IDuplexSessionChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;

            ISecurityCapabilities innerSecurityCapabilities = this.InnerChannelFactory.GetProperty<ISecurityCapabilities>();

            // can't pool across outer channels if the inner channels support client auth
            if (innerSecurityCapabilities != null && innerSecurityCapabilities.SupportsClientAuthentication)
            {
                this.channelPoolSettings = bindingElement.ChannelPoolSettings.Clone();
            }
            else
            {
                this.channelPool = new ChannelPool<IDuplexSessionChannel>(bindingElement.ChannelPoolSettings);
            }
        }

        internal ChannelPool<IDuplexSessionChannel> GetChannelPool(out bool cleanupChannelPool)
        {
            if (this.channelPool != null)
            {
                cleanupChannelPool = false;
                return this.channelPool;
            }
            else
            {
                cleanupChannelPool = true;
                Fx.Assert(this.channelPoolSettings != null, "Need either settings or a pool");
                return new ChannelPool<IDuplexSessionChannel>(this.channelPoolSettings);
            }
        }

        protected override void OnAbort()
        {
            if (this.channelPool != null)
            {
                this.channelPool.Close(TimeSpan.Zero);
            }
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.channelPool != null)
            {
                this.channelPool.Close(timeoutHelper.RemainingTime());
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.channelPool != null)
            {
                this.channelPool.Close(timeoutHelper.RemainingTime());
            }
            return base.OnBeginClose(timeoutHelper.RemainingTime(), callback, state);
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new DuplexSessionOutputChannel(this, address, via);
        }

        class DuplexSessionOutputChannel : OutputChannel
        {
            ChannelPool<IDuplexSessionChannel> channelPool;
            EndpointAddress remoteAddress;
            IChannelFactory<IDuplexSessionChannel> innerFactory;
            AsyncCallback onReceive;
            bool packetRoutable;
            bool cleanupChannelPool;
            Uri via;

            public DuplexSessionOutputChannel(DuplexSessionOneWayChannelFactory factory,
                EndpointAddress remoteAddress, Uri via)
                : base(factory)
            {
                this.channelPool = factory.GetChannelPool(out cleanupChannelPool);
                this.packetRoutable = factory.packetRoutable;
                this.innerFactory = (IChannelFactory<IDuplexSessionChannel>)factory.InnerChannelFactory;
                this.remoteAddress = remoteAddress;
                this.via = via;
            }

            public override EndpointAddress RemoteAddress
            {
                get { return this.remoteAddress; }
            }

            public override Uri Via
            {
                get { return this.via; }
            }

            #region Channel Lifetime
            protected override void OnOpen(TimeSpan timeout)
            {
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnAbort()
            {
                if (cleanupChannelPool)
                {
                    this.channelPool.Close(TimeSpan.Zero);
                }
            }

            protected override void OnClose(TimeSpan timeout)
            {
                if (cleanupChannelPool)
                {
                    this.channelPool.Close(timeout);
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (cleanupChannelPool)
                {
                    this.channelPool.Close(timeout);
                }
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }
            #endregion

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                ChannelPoolKey key = null;
                bool isConnectionFromPool = true;
                IDuplexSessionChannel innerChannel =
                    GetChannelFromPool(ref timeoutHelper, out key, out isConnectionFromPool);

                bool success = false;
                try
                {
                    if (!isConnectionFromPool)
                    {
                        StampInitialMessage(message);
                        innerChannel.Open(timeoutHelper.RemainingTime());
                        StartBackgroundReceive(innerChannel);
                    }

                    innerChannel.Send(message, timeoutHelper.RemainingTime());
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        CleanupChannel(innerChannel, false, key, isConnectionFromPool, ref timeoutHelper);
                    }
                }

                CleanupChannel(innerChannel, true, key, isConnectionFromPool, ref timeoutHelper);
            }

            // kick off an async receive so that we notice when the server is trying to shutdown
            void StartBackgroundReceive(IDuplexSessionChannel channel)
            {
                if (this.onReceive == null)
                {
                    this.onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
                }

                channel.BeginReceive(TimeSpan.MaxValue, this.onReceive, channel);
            }

            void OnReceive(IAsyncResult result)
            {
                IDuplexSessionChannel channel = (IDuplexSessionChannel)result.AsyncState;
                bool success = false;
                try
                {
                    Message message = channel.EndReceive(result);
                    if (message == null)
                    {
                        channel.Close(this.channelPool.IdleTimeout);
                        success = true;
                    }
                    else
                    {
                        message.Close();
                    }
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (!success)
                    {
                        channel.Abort();
                    }
                }
            }

            void StampInitialMessage(Message message)
            {
                if (this.packetRoutable)
                {
                    PacketRoutableHeader.AddHeadersTo(message, null);
                }
            }

            void CleanupChannel(IDuplexSessionChannel channel, bool connectionStillGood, ChannelPoolKey key, bool isConnectionFromPool, ref TimeoutHelper timeoutHelper)
            {
                if (isConnectionFromPool)
                {
                    this.channelPool.ReturnConnection(key, channel, connectionStillGood, timeoutHelper.RemainingTime());
                }
                else
                {
                    if (connectionStillGood)
                    {
                        this.channelPool.AddConnection(key, channel, timeoutHelper.RemainingTime());
                    }
                    else
                    {
                        channel.Abort();
                    }
                }
            }

            IDuplexSessionChannel GetChannelFromPool(ref TimeoutHelper timeoutHelper, out ChannelPoolKey key,
                out bool isConnectionFromPool)
            {
                isConnectionFromPool = true;
                while (true)
                {
                    IDuplexSessionChannel pooledChannel
                        = this.channelPool.TakeConnection(this.RemoteAddress, this.Via, timeoutHelper.RemainingTime(), out key);

                    if (pooledChannel == null)
                    {
                        isConnectionFromPool = false;
                        return this.innerFactory.CreateChannel(RemoteAddress, Via);
                    }

                    // only return good connections
                    if (pooledChannel.State == CommunicationState.Opened)
                    {
                        return pooledChannel;
                    }

                    // Abort stale connections from the pool
                    this.channelPool.ReturnConnection(key, pooledChannel, false, timeoutHelper.RemainingTime());
                }
            }

            class SendAsyncResult : AsyncResult
            {
                DuplexSessionOutputChannel parent;
                IDuplexSessionChannel innerChannel;
                Message message;
                TimeoutHelper timeoutHelper;
                static AsyncCallback onOpen;
                static AsyncCallback onInnerSend = Fx.ThunkCallback(new AsyncCallback(OnInnerSend));
                ChannelPoolKey key;
                bool isConnectionFromPool;

                public SendAsyncResult(DuplexSessionOutputChannel parent, Message message, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.innerChannel =
                        parent.GetChannelFromPool(ref this.timeoutHelper, out this.key, out this.isConnectionFromPool);

                    bool success = false;
                    bool completeSelf = true;
                    try
                    {
                        if (!this.isConnectionFromPool)
                        {
                            completeSelf = OpenNewChannel();
                        }

                        if (completeSelf)
                        {
                            completeSelf = SendMessage();
                        }
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            Cleanup(false);
                        }
                    }

                    if (completeSelf)
                    {
                        Cleanup(true);
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SendAsyncResult>(result);
                }

                void Cleanup(bool connectionStillGood)
                {
                    parent.CleanupChannel(this.innerChannel, connectionStillGood, this.key,
                        this.isConnectionFromPool, ref this.timeoutHelper);
                }

                bool OpenNewChannel()
                {
                    if (onOpen == null)
                    {
                        onOpen = Fx.ThunkCallback(new AsyncCallback(OnOpen));
                    }

                    this.parent.StampInitialMessage(this.message);
                    IAsyncResult result = this.innerChannel.BeginOpen(timeoutHelper.RemainingTime(), onOpen, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }

                    this.CompleteOpen(result);
                    return true;
                }

                void CompleteOpen(IAsyncResult result)
                {
                    this.innerChannel.EndOpen(result);
                    this.parent.StartBackgroundReceive(this.innerChannel);
                }

                bool SendMessage()
                {
                    IAsyncResult result = innerChannel.BeginSend(this.message, onInnerSend, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }

                    innerChannel.EndSend(result);
                    return true;
                }

                static void OnOpen(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        thisPtr.CompleteOpen(result);
                        completeSelf = thisPtr.SendMessage();
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }

                    if (completeSelf)
                    {
                        thisPtr.Cleanup(completionException == null);
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnInnerSend(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.innerChannel.EndSend(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }

                    thisPtr.Cleanup(completionException == null);
                    thisPtr.Complete(false, completionException);
                }
            }
        }
    }
}
