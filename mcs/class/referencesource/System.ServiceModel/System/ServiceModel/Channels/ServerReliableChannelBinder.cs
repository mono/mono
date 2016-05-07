//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;

    abstract class ServerReliableChannelBinder<TChannel> : ReliableChannelBinder<TChannel>,
        IServerReliableChannelBinder
        where TChannel : class, IChannel
    {
        static string addressedPropertyName = "MessageAddressedByBinderProperty";
        IChannelListener<TChannel> listener;
        static AsyncCallback onAcceptChannelComplete = Fx.ThunkCallback(new AsyncCallback(OnAcceptChannelCompleteStatic));
        EndpointAddress cachedLocalAddress;
        TChannel pendingChannel;
        InterruptibleWaitObject pendingChannelEvent = new InterruptibleWaitObject(false, false);
        EndpointAddress remoteAddress;

        protected ServerReliableChannelBinder(ChannelBuilder builder,
            EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode,
            TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout,
            TimeSpan defaultSendTimeout)
            : base(null, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
        {
            this.listener = builder.BuildChannelListener<TChannel>(filter, priority);
            this.remoteAddress = remoteAddress;
        }

        protected ServerReliableChannelBinder(TChannel channel, EndpointAddress cachedLocalAddress,
            EndpointAddress remoteAddress, MaskingMode maskingMode,
            TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout,
            TimeSpan defaultSendTimeout)
            : base(channel, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
        {
            this.cachedLocalAddress = cachedLocalAddress;
            this.remoteAddress = remoteAddress;
        }

        protected override bool CanGetChannelForReceive
        {
            get
            {
                return true;
            }
        }

        public override EndpointAddress LocalAddress
        {
            get
            {
                if (this.cachedLocalAddress != null)
                {
                    return this.cachedLocalAddress;
                }
                else
                {
                    return this.GetInnerChannelLocalAddress();
                }
            }
        }

        protected override bool MustCloseChannel
        {
            get
            {
                return this.MustOpenChannel || this.HasSession;
            }
        }

        protected override bool MustOpenChannel
        {
            get
            {
                return this.listener != null;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        void AddAddressedProperty(Message message)
        {
            message.Properties.Add(addressedPropertyName, new object());
        }

        protected override void AddOutputHeaders(Message message)
        {
            if (this.GetAddressedProperty(message) == null)
            {
                this.RemoteAddress.ApplyTo(message);
                this.AddAddressedProperty(message);
            }
        }

        public bool AddressResponse(Message request, Message response)
        {
            if (this.GetAddressedProperty(response) != null)
            {
                throw Fx.AssertAndThrow("The binder can't address a response twice");
            }

            try
            {
                RequestReplyCorrelator.PrepareReply(response, request);
            }
            catch (MessageHeaderException exception)
            {
                // ---- it - we don't need to correlate the reply if the MessageId header is bad
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }

            bool sendResponse = true;
            try
            {
                sendResponse = RequestReplyCorrelator.AddressReply(response, request);
            }
            catch (MessageHeaderException exception)
            {
                // ---- it - we don't need to address the reply if the addressing headers are bad
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
            }

            if (sendResponse)
                this.AddAddressedProperty(response);

            return sendResponse;
        }

        protected override IAsyncResult BeginTryGetChannel(TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return this.pendingChannelEvent.BeginTryWait(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            if (this.DefaultMaskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }

            if (this.ValidateInputOperation(timeout))
            {
                return new WaitForRequestAsyncResult(this, timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        bool CompleteAcceptChannel(IAsyncResult result)
        {
            TChannel channel = this.listener.EndAcceptChannel(result);

            if (channel == null)
            {
                return false;
            }

            if (!this.UseNewChannel(channel))
            {
                channel.Abort();
            }

            return true;
        }

        public static IServerReliableChannelBinder CreateBinder(ChannelBuilder builder,
            EndpointAddress remoteAddress, MessageFilter filter, int priority,
            TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout,
            TimeSpan defaultSendTimeout)
        {
            Type type = typeof(TChannel);

            if (type == typeof(IDuplexChannel))
            {
                return new DuplexServerReliableChannelBinder(builder, remoteAddress, filter,
                    priority, MaskingMode.None, defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IDuplexSessionChannel))
            {
                return new DuplexSessionServerReliableChannelBinder(builder, remoteAddress, filter,
                    priority, MaskingMode.None, faultMode, defaultCloseTimeout,
                    defaultSendTimeout);
            }
            else if (type == typeof(IReplyChannel))
            {
                return new ReplyServerReliableChannelBinder(builder, remoteAddress, filter,
                    priority, MaskingMode.None, defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IReplySessionChannel))
            {
                return new ReplySessionServerReliableChannelBinder(builder, remoteAddress, filter,
                    priority, MaskingMode.None, faultMode, defaultCloseTimeout,
                    defaultSendTimeout);
            }
            else
            {
                throw Fx.AssertAndThrow("ServerReliableChannelBinder supports creation of IDuplexChannel, IDuplexSessionChannel, IReplyChannel, and IReplySessionChannel only.");
            }
        }

        public static IServerReliableChannelBinder CreateBinder(TChannel channel,
            EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
            TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout,
            TimeSpan defaultSendTimeout)
        {
            Type type = typeof(TChannel);

            if (type == typeof(IDuplexChannel))
            {
                return new DuplexServerReliableChannelBinder((IDuplexChannel)channel,
                    cachedLocalAddress, remoteAddress, MaskingMode.All, defaultCloseTimeout,
                    defaultSendTimeout);
            }
            else if (type == typeof(IDuplexSessionChannel))
            {
                return new DuplexSessionServerReliableChannelBinder((IDuplexSessionChannel)channel,
                    cachedLocalAddress, remoteAddress, MaskingMode.All, faultMode,
                    defaultCloseTimeout, defaultSendTimeout);
            }
            else if (type == typeof(IReplyChannel))
            {
                return new ReplyServerReliableChannelBinder((IReplyChannel)channel,
                    cachedLocalAddress, remoteAddress, MaskingMode.All, defaultCloseTimeout,
                    defaultSendTimeout);
            }
            else if (type == typeof(IReplySessionChannel))
            {
                return new ReplySessionServerReliableChannelBinder((IReplySessionChannel)channel,
                    cachedLocalAddress, remoteAddress, MaskingMode.All, faultMode,
                    defaultCloseTimeout, defaultSendTimeout);
            }
            else
            {
                throw Fx.AssertAndThrow("ServerReliableChannelBinder supports creation of IDuplexChannel, IDuplexSessionChannel, IReplyChannel, and IReplySessionChannel only.");
            }
        }

        protected override bool EndTryGetChannel(IAsyncResult result)
        {
            if (!this.pendingChannelEvent.EndTryWait(result))
                return false;

            TChannel abortChannel = null;

            lock (this.ThisLock)
            {
                if (this.State != CommunicationState.Faulted &&
                    this.State != CommunicationState.Closing &&
                    this.State != CommunicationState.Closed)
                {
                    if (!this.Synchronizer.SetChannel(this.pendingChannel))
                    {
                        abortChannel = this.pendingChannel;
                    }

                    this.pendingChannel = null;
                    this.pendingChannelEvent.Reset();
                }
            }

            if (abortChannel != null)
            {
                abortChannel.Abort();
            }

            return true;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            WaitForRequestAsyncResult waitForRequestResult = result as WaitForRequestAsyncResult;

            if (waitForRequestResult != null)
            {
                return waitForRequestResult.End();
            }
            else
            {
                CompletedAsyncResult.End(result);
                return true;
            }
        }

        object GetAddressedProperty(Message message)
        {
            object property;

            message.Properties.TryGetValue(addressedPropertyName, out property);
            return property;
        }

        protected abstract EndpointAddress GetInnerChannelLocalAddress();

        bool IsListenerExceptionNullOrHandleable(Exception e)
        {
            if (e == null)
            {
                return true;
            }

            if (this.listener.State == CommunicationState.Faulted)
            {
                return false;
            }

            return this.IsHandleable(e);
        }

        protected override void OnAbort()
        {
            if (this.listener != null)
            {
                this.listener.Abort();
            }
        }

        void OnAcceptChannelComplete(IAsyncResult result)
        {
            Exception expectedException = null;
            Exception unexpectedException = null;
            bool gotChannel = false;

            try
            {
                gotChannel = this.CompleteAcceptChannel(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (this.IsHandleable(e))
                {
                    expectedException = e;
                }
                else
                {
                    unexpectedException = e;
                }
            }

            if (gotChannel)
            {
                this.StartAccepting();
            }
            else if (unexpectedException != null)
            {
                this.Fault(unexpectedException);
            }
            else if ((expectedException != null)
                && (this.listener.State == CommunicationState.Opened))
            {
                this.StartAccepting();
            }
            else if (this.listener.State == CommunicationState.Faulted)
            {
                this.Fault(expectedException);
            }
        }

        static void OnAcceptChannelCompleteStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ServerReliableChannelBinder<TChannel> binder =
                    (ServerReliableChannelBinder<TChannel>)result.AsyncState;

                binder.OnAcceptChannelComplete(result);
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            if (this.listener != null)
            {
                return this.listener.BeginClose(timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            if (this.listener != null)
            {
                return this.listener.BeginOpen(timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected abstract IAsyncResult OnBeginWaitForRequest(TChannel channel, TimeSpan timeout,
            AsyncCallback callback, object state);

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.listener != null)
            {
                this.listener.Close(timeout);
            }
        }

        protected override void OnShutdown()
        {
            TChannel channel = null;

            lock (this.ThisLock)
            {
                channel = this.pendingChannel;
                this.pendingChannel = null;
                this.pendingChannelEvent.Set();
            }

            if (channel != null)
                channel.Abort();
        }

        protected abstract bool OnWaitForRequest(TChannel channel, TimeSpan timeout);

        protected override void OnEndClose(IAsyncResult result)
        {
            if (this.listener != null)
            {
                this.listener.EndClose(result);
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.listener != null)
            {
                this.listener.EndOpen(result);
                this.StartAccepting();
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected abstract bool OnEndWaitForRequest(TChannel channel, IAsyncResult result);

        protected override void OnOpen(TimeSpan timeout)
        {
            if (this.listener != null)
            {
                this.listener.Open(timeout);
                this.StartAccepting();
            }
        }

        void StartAccepting()
        {
            Exception expectedException = null;
            Exception unexpectedException = null;

            while (this.listener.State == CommunicationState.Opened)
            {
                expectedException = null;
                unexpectedException = null;

                try
                {
                    IAsyncResult result = this.listener.BeginAcceptChannel(TimeSpan.MaxValue,
                        onAcceptChannelComplete, this);

                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    else if (!this.CompleteAcceptChannel(result))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (this.IsHandleable(e))
                    {
                        expectedException = e;
                        continue;
                    }
                    else
                    {
                        unexpectedException = e;
                        break;
                    }
                }
            }

            if (unexpectedException != null)
            {
                this.Fault(unexpectedException);
            }
            else if (this.listener.State == CommunicationState.Faulted)
            {
                this.Fault(expectedException);
            }
        }

        protected override bool TryGetChannel(TimeSpan timeout)
        {
            if (!this.pendingChannelEvent.Wait(timeout))
                return false;

            TChannel abortChannel = null;

            lock (this.ThisLock)
            {
                if (this.State != CommunicationState.Faulted &&
                    this.State != CommunicationState.Closing &&
                    this.State != CommunicationState.Closed)
                {
                    if (!this.Synchronizer.SetChannel(this.pendingChannel))
                    {
                        abortChannel = this.pendingChannel;
                    }

                    this.pendingChannel = null;
                    this.pendingChannelEvent.Reset();
                }
            }

            if (abortChannel != null)
            {
                abortChannel.Abort();
            }

            return true;
        }

        public bool UseNewChannel(IChannel channel)
        {
            TChannel oldPendingChannel = null;
            TChannel oldBinderChannel = null;

            lock (this.ThisLock)
            {
                if (!this.Synchronizer.TolerateFaults ||
                    this.State == CommunicationState.Faulted ||
                    this.State == CommunicationState.Closing ||
                    this.State == CommunicationState.Closed)
                {
                    return false;
                }
                else
                {
                    oldPendingChannel = this.pendingChannel;
                    this.pendingChannel = (TChannel)channel;
                    oldBinderChannel = this.Synchronizer.AbortCurentChannel();
                }
            }

            if (oldPendingChannel != null)
            {
                oldPendingChannel.Abort();
            }

            this.pendingChannelEvent.Set();

            if (oldBinderChannel != null)
            {
                oldBinderChannel.Abort();
            }

            return true;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            if (this.DefaultMaskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }

            if (!this.ValidateInputOperation(timeout))
            {
                return true;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            while (true)
            {
                bool autoAborted = false;

                try
                {
                    TChannel channel;
                    bool success = !this.Synchronizer.TryGetChannelForInput(true, timeoutHelper.RemainingTime(),
                        out channel);

                    if (channel == null)
                    {
                        return success;
                    }

                    try
                    {
                        return this.OnWaitForRequest(channel, timeoutHelper.RemainingTime());
                    }
                    finally
                    {
                        autoAborted = this.Synchronizer.Aborting;
                        this.Synchronizer.ReturnChannel();
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (!this.HandleException(e, this.DefaultMaskingMode, autoAborted))
                    {
                        throw;
                    }
                    else
                    {
                        continue;
                    }
                }

            }
        }

        abstract class DuplexServerReliableChannelBinder<TDuplexChannel>
            : ServerReliableChannelBinder<TDuplexChannel>
            where TDuplexChannel : class, IDuplexChannel
        {
            protected DuplexServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected DuplexServerReliableChannelBinder(TDuplexChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool CanSendAsynchronously
            {
                get
                {
                    return true;
                }
            }

            protected override EndpointAddress GetInnerChannelLocalAddress()
            {
                IDuplexChannel channel = this.Synchronizer.CurrentChannel;
                EndpointAddress localAddress = (channel == null) ? null : channel.LocalAddress;
                return localAddress;
            }

            protected override IAsyncResult OnBeginSend(TDuplexChannel channel, Message message,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginSend(message, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginTryReceive(TDuplexChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginTryReceive(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginWaitForRequest(TDuplexChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginWaitForMessage(timeout, callback, state);
            }

            protected override void OnEndSend(TDuplexChannel channel, IAsyncResult result)
            {
                channel.EndSend(result);
            }

            protected override bool OnEndTryReceive(TDuplexChannel channel, IAsyncResult result,
                out RequestContext requestContext)
            {
                Message message;
                bool success = channel.EndTryReceive(result, out message);
                if (success)
                {
                    this.OnMessageReceived(message);
                }
                requestContext = this.WrapMessage(message);
                return success;
            }

            protected override bool OnEndWaitForRequest(TDuplexChannel channel,
                IAsyncResult result)
            {
                return channel.EndWaitForMessage(result);
            }

            protected abstract void OnMessageReceived(Message message);

            protected override void OnSend(TDuplexChannel channel, Message message,
                TimeSpan timeout)
            {
                channel.Send(message, timeout);
            }

            protected override bool OnTryReceive(TDuplexChannel channel, TimeSpan timeout,
                out RequestContext requestContext)
            {
                Message message;
                bool success = channel.TryReceive(timeout, out message);
                if (success)
                {
                    this.OnMessageReceived(message);
                }
                requestContext = this.WrapMessage(message);
                return success;
            }

            protected override bool OnWaitForRequest(TDuplexChannel channel, TimeSpan timeout)
            {
                return channel.WaitForMessage(timeout);
            }

        }

        sealed class DuplexServerReliableChannelBinder
            : DuplexServerReliableChannelBinder<IDuplexChannel>
        {
            public DuplexServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode,
                TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public DuplexServerReliableChannelBinder(IDuplexChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode, TolerateFaultsMode.Never,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IDuplexChannel channel)
            {
                return false;
            }

            protected override void OnMessageReceived(Message message)
            {
            }
        }

        sealed class DuplexSessionServerReliableChannelBinder
            : DuplexServerReliableChannelBinder<IDuplexSessionChannel>
        {
            public DuplexSessionServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public DuplexSessionServerReliableChannelBinder(IDuplexSessionChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }

            protected override IAsyncResult BeginCloseChannel(IDuplexSessionChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReliableChannelBinderHelper.BeginCloseDuplexSessionChannel(this, channel,
                    timeout, callback, state);
            }

            protected override void CloseChannel(IDuplexSessionChannel channel, TimeSpan timeout)
            {
                ReliableChannelBinderHelper.CloseDuplexSessionChannel(this, channel, timeout);
            }

            protected override void EndCloseChannel(IDuplexSessionChannel channel,
                IAsyncResult result)
            {
                ReliableChannelBinderHelper.EndCloseDuplexSessionChannel(channel, result);
            }

            public override ISession GetInnerSession()
            {
                return this.Synchronizer.CurrentChannel.Session;
            }

            protected override bool HasSecuritySession(IDuplexSessionChannel channel)
            {
                return channel.Session is ISecuritySession;
            }

            protected override void OnMessageReceived(Message message)
            {
                if (message == null)
                    this.Synchronizer.OnReadEof();
            }
        }

        abstract class ReplyServerReliableChannelBinder<TReplyChannel>
            : ServerReliableChannelBinder<TReplyChannel>
            where TReplyChannel : class, IReplyChannel
        {
            public ReplyServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplyServerReliableChannelBinder(TReplyChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool CanSendAsynchronously
            {
                get
                {
                    return false;
                }
            }

            protected override EndpointAddress GetInnerChannelLocalAddress()
            {
                IReplyChannel channel = this.Synchronizer.CurrentChannel;
                EndpointAddress localAddress = (channel == null) ? null : channel.LocalAddress;
                return localAddress;
            }

            protected override IAsyncResult OnBeginTryReceive(TReplyChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginWaitForRequest(TReplyChannel channel,
                TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginWaitForRequest(timeout, callback, state);
            }

            protected override bool OnEndTryReceive(TReplyChannel channel, IAsyncResult result,
                out RequestContext requestContext)
            {
                bool success = channel.EndTryReceiveRequest(result, out requestContext);
                if (success && (requestContext == null))
                {
                    this.OnReadNullMessage();
                }
                requestContext = this.WrapRequestContext(requestContext);
                return success;
            }

            protected override bool OnEndWaitForRequest(TReplyChannel channel, IAsyncResult result)
            {
                return channel.EndWaitForRequest(result);
            }

            protected virtual void OnReadNullMessage()
            {
            }

            protected override bool OnTryReceive(TReplyChannel channel, TimeSpan timeout,
                out RequestContext requestContext)
            {
                bool success = channel.TryReceiveRequest(timeout, out requestContext);
                if (success && (requestContext == null))
                {
                    this.OnReadNullMessage();
                }
                requestContext = this.WrapRequestContext(requestContext);
                return success;
            }

            protected override bool OnWaitForRequest(TReplyChannel channel, TimeSpan timeout)
            {
                return channel.WaitForRequest(timeout);
            }
        }

        sealed class ReplyServerReliableChannelBinder
            : ReplyServerReliableChannelBinder<IReplyChannel>
        {
            public ReplyServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode,
                TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplyServerReliableChannelBinder(IReplyChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode,
                TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IReplyChannel channel)
            {
                return false;
            }
        }

        sealed class ReplySessionServerReliableChannelBinder
            : ReplyServerReliableChannelBinder<IReplySessionChannel>
        {
            public ReplySessionServerReliableChannelBinder(ChannelBuilder builder,
                EndpointAddress remoteAddress, MessageFilter filter, int priority,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(builder, remoteAddress, filter, priority, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplySessionServerReliableChannelBinder(IReplySessionChannel channel,
                EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress,
                MaskingMode maskingMode, TolerateFaultsMode faultMode,
                TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
                : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode,
                defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }

            protected override IAsyncResult BeginCloseChannel(IReplySessionChannel channel,
               TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReliableChannelBinderHelper.BeginCloseReplySessionChannel(this, channel,
                    timeout, callback, state);
            }

            protected override void CloseChannel(IReplySessionChannel channel, TimeSpan timeout)
            {
                ReliableChannelBinderHelper.CloseReplySessionChannel(this, channel, timeout);
            }

            protected override void EndCloseChannel(IReplySessionChannel channel,
                IAsyncResult result)
            {
                ReliableChannelBinderHelper.EndCloseReplySessionChannel(channel, result);
            }

            public override ISession GetInnerSession()
            {
                return this.Synchronizer.CurrentChannel.Session;
            }

            protected override bool HasSecuritySession(IReplySessionChannel channel)
            {
                return channel.Session is ISecuritySession;
            }

            protected override void OnReadNullMessage()
            {
                this.Synchronizer.OnReadEof();
            }
        }

        sealed class WaitForRequestAsyncResult
            : InputAsyncResult<ServerReliableChannelBinder<TChannel>>
        {
            public WaitForRequestAsyncResult(ServerReliableChannelBinder<TChannel> binder,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(binder, true, timeout, binder.DefaultMaskingMode, callback, state)
            {
                if (this.Start())
                    this.Complete(true);
            }

            protected override IAsyncResult BeginInput(
                ServerReliableChannelBinder<TChannel> binder, TChannel channel, TimeSpan timeout,
                AsyncCallback callback, object state)
            {
                return binder.OnBeginWaitForRequest(channel, timeout, callback, state);
            }

            protected override bool EndInput(ServerReliableChannelBinder<TChannel> binder,
                TChannel channel, IAsyncResult result, out bool complete)
            {
                complete = true;
                return binder.OnEndWaitForRequest(channel, result);
            }
        }
    }
}
