//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    /// <summary>
    /// Wraps an IChannelListener<IReplyChannel> into an IChannelListener<IInputChannel>
    /// </summary>
    class ReplyOneWayChannelListener
        : LayeredChannelListener<IInputChannel>
    {
        IChannelListener<IReplyChannel> innerChannelListener;
        bool packetRoutable;

        public ReplyOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.BuildInnerChannelListener<IReplyChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IReplyChannel>)this.InnerChannelListener;
            base.OnOpening();
        }

        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            IReplyChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
            return WrapInnerChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            IReplyChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
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

        IInputChannel WrapInnerChannel(IReplyChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return null;
            }
            else
            {
                return new ReplyOneWayInputChannel(this, innerChannel);
            }
        }

        class ReplyOneWayInputChannel : LayeredChannel<IReplyChannel>, IInputChannel
        {
            bool validateHeader;

            public ReplyOneWayInputChannel(ReplyOneWayChannelListener listener, IReplyChannel innerChannel)
                : base(listener, innerChannel)
            {
                this.validateHeader = listener.packetRoutable;
            }

            public EndpointAddress LocalAddress
            {
                get { return this.InnerChannel.LocalAddress; }
            }

            Message ProcessContext(RequestContext context, TimeSpan timeout)
            {
                if (context == null)
                {
                    return null;
                }

                bool replySuccess = false;
                Message result = null;
                try
                {
                    // validate that the request message contains our expected header
                    result = context.RequestMessage;
                    result.Properties.Add(RequestContextMessageProperty.Name, new RequestContextMessageProperty(context));

                    if (this.validateHeader)
                    {
                        PacketRoutableHeader.ValidateMessage(result);
                    }

                    try
                    {
                        TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                        context.Reply(null, timeoutHelper.RemainingTime());
                        replySuccess = true;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.SendTimeoutIsEnabled())
                        {
                            TD.SendTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                }
                finally
                {
                    if (!replySuccess)
                    {
                        context.Abort();
                        if (result != null)
                        {
                            result.Close();
                            result = null;
                        }
                    }
                }

                return result;
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                RequestContext context = InnerChannel.ReceiveRequest(timeoutHelper.RemainingTime());
                return ProcessContext(context, timeoutHelper.RemainingTime());
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveAsyncResult(this.InnerChannel, timeout, this.validateHeader, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return ReceiveAsyncResult.End(result);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                RequestContext context;
                if (InnerChannel.TryReceiveRequest(timeoutHelper.RemainingTime(), out context))
                {
                    message = ProcessContext(context, timeoutHelper.RemainingTime());
                    return true;
                }
                else
                {
                    message = null;
                    return false;
                }
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new TryReceiveAsyncResult(this.InnerChannel, timeout, this.validateHeader, callback, state);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return TryReceiveAsyncResult.End(result, out message);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return InnerChannel.WaitForRequest(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InnerChannel.BeginWaitForRequest(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return InnerChannel.EndWaitForRequest(result);
            }

            class TryReceiveAsyncResult : ReceiveAsyncResultBase
            {
                bool tryResult;

                public TryReceiveAsyncResult(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader,
                    AsyncCallback callback, object state)
                    : base(innerChannel, timeout, validateHeader, callback, state)
                {
                }

                public static bool End(IAsyncResult result, out Message message)
                {
                    TryReceiveAsyncResult thisPtr = AsyncResult.End<TryReceiveAsyncResult>(result);
                    message = thisPtr.Message;
                    return thisPtr.tryResult;
                }

                protected override IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
                }

                protected override RequestContext OnEndReceiveRequest(IAsyncResult result)
                {
                    RequestContext context;
                    this.tryResult = InnerChannel.EndTryReceiveRequest(result, out context);
                    return context;
                }
            }

            class ReceiveAsyncResult : ReceiveAsyncResultBase
            {
                public ReceiveAsyncResult(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader,
                    AsyncCallback callback, object state)
                    : base(innerChannel, timeout, validateHeader, callback, state)
                {
                }

                public static Message End(IAsyncResult result)
                {
                    ReceiveAsyncResult thisPtr = AsyncResult.End<ReceiveAsyncResult>(result);
                    return thisPtr.Message;
                }

                protected override IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return InnerChannel.BeginReceiveRequest(timeout, callback, state);
                }

                protected override RequestContext OnEndReceiveRequest(IAsyncResult result)
                {
                    return InnerChannel.EndReceiveRequest(result);
                }
            }

            abstract class ReceiveAsyncResultBase : AsyncResult
            {
                IReplyChannel innerChannel;
                RequestContext context;
                Message message;
                TimeoutHelper timeoutHelper;
                bool validateHeader;
                static AsyncCallback onReceiveRequest = Fx.ThunkCallback(new AsyncCallback(OnReceiveRequest));
                static AsyncCallback onReply = Fx.ThunkCallback(new AsyncCallback(OnReply));

                protected ReceiveAsyncResultBase(IReplyChannel innerChannel, TimeSpan timeout, bool validateHeader,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.innerChannel = innerChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.validateHeader = validateHeader;
                    IAsyncResult result = this.OnBeginReceiveRequest(timeoutHelper.RemainingTime(), onReceiveRequest, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }

                    if (HandleReceiveRequestComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                protected IReplyChannel InnerChannel
                {
                    get
                    {
                        return this.innerChannel;
                    }
                }

                protected Message Message
                {
                    get
                    {
                        return this.message;
                    }
                }

                protected abstract IAsyncResult OnBeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
                protected abstract RequestContext OnEndReceiveRequest(IAsyncResult result);

                bool HandleReplyComplete(IAsyncResult result)
                {
                    bool abortContext = true;
                    try
                    {
                        context.EndReply(result);
                        abortContext = false;
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (TimeoutException e)
                    {
                        if (TD.SendTimeoutIsEnabled())
                        {
                            TD.SendTimeout(e.Message);
                        }
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    finally
                    {
                        if (abortContext)
                        {
                            context.Abort();
                        }
                    }

                    return true;
                }

                bool HandleReceiveRequestComplete(IAsyncResult result)
                {
                    this.context = this.OnEndReceiveRequest(result);
                    if (this.context == null)
                    {
                        return true;
                    }

                    bool replySuccess = false;
                    IAsyncResult replyResult = null;
                    try
                    {
                        this.message = context.RequestMessage;
                        this.message.Properties.Add(RequestContextMessageProperty.Name, new RequestContextMessageProperty(context));

                        if (validateHeader)
                        {
                            PacketRoutableHeader.ValidateMessage(this.message);
                        }
                        try
                        {
                            replyResult = context.BeginReply(null, timeoutHelper.RemainingTime(), onReply, this);
                            replySuccess = true;
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                        catch (TimeoutException e)
                        {
                            if (TD.SendTimeoutIsEnabled())
                            {
                                TD.SendTimeout(e.Message);
                            }
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                    }
                    finally
                    {
                        if (!replySuccess)
                        {
                            this.context.Abort();
                            if (this.message != null)
                            {
                                this.message.Close();
                                this.message = null;
                            }
                        }
                    }

                    if (replyResult == null)
                    {
                        return true;
                    }
                    else if (replyResult.CompletedSynchronously)
                    {
                        return HandleReplyComplete(replyResult);
                    }
                    else
                    {
                        return false;
                    }
                }

                static void OnReceiveRequest(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    ReceiveAsyncResultBase thisPtr = (ReceiveAsyncResultBase)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.HandleReceiveRequestComplete(result);
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
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnReply(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    ReceiveAsyncResultBase thisPtr = (ReceiveAsyncResultBase)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.HandleReplyComplete(result);
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
                        thisPtr.Complete(false, completionException);
                    }
                }
            }
        }
    }

    // <summary>
    // Wraps an IChannelListener<IDuplexChannel> into an IChannelListener<IInputChannel>
    // </summary>
    class DuplexOneWayChannelListener
        : LayeredChannelListener<IInputChannel>
    {
        IChannelListener<IDuplexChannel> innerChannelListener;
        bool packetRoutable;

        public DuplexOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.BuildInnerChannelListener<IDuplexChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IDuplexChannel>)this.InnerChannelListener;
            base.OnOpening();
        }

        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            IDuplexChannel channel = this.innerChannelListener.AcceptChannel(timeout);
            return WrapInnerChannel(channel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            IDuplexChannel channel = this.innerChannelListener.EndAcceptChannel(result);
            return WrapInnerChannel(channel);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        IInputChannel WrapInnerChannel(IDuplexChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return null;
            }
            else
            {
                return new DuplexOneWayInputChannel(this, innerChannel);
            }
        }

        class DuplexOneWayInputChannel : LayeredChannel<IDuplexChannel>, IInputChannel
        {
            bool validateHeader;

            public DuplexOneWayInputChannel(DuplexOneWayChannelListener listener, IDuplexChannel innerChannel)
                : base(listener, innerChannel)
            {
                this.validateHeader = listener.packetRoutable;
            }

            public EndpointAddress LocalAddress
            {
                get { return this.InnerChannel.LocalAddress; }
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerChannel.BeginReceive(timeout, callback, state);
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
                Message message = this.InnerChannel.EndReceive(result);
                return ValidateMessage(message);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                bool success = this.InnerChannel.EndTryReceive(result, out message);
                message = ValidateMessage(message);
                return success;
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.InnerChannel.EndWaitForMessage(result);
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                Message result = this.InnerChannel.Receive(timeout);
                return ValidateMessage(result);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                bool success = this.InnerChannel.TryReceive(timeout, out message);
                message = ValidateMessage(message);
                return success;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.InnerChannel.WaitForMessage(timeout);
            }

            Message ValidateMessage(Message message)
            {
                if (this.validateHeader && message != null)
                {
                    PacketRoutableHeader.ValidateMessage(message);
                }
                return message;
            }
        }
    }

    /// <summary>
    /// Wraps an IChannelListener<IDuplexSessionChannel> into an IChannelListener<IInputChannel>
    /// </summary>
    class DuplexSessionOneWayChannelListener
        : DelegatingChannelListener<IInputChannel>
    {
        IChannelListener<IDuplexSessionChannel> innerChannelListener;
        DuplexSessionOneWayInputChannelAcceptor inputChannelAcceptor;
        bool packetRoutable;
        int maxAcceptedChannels;
        bool acceptPending;
        int activeChannels;
        TimeSpan idleTimeout;
        static AsyncCallback onAcceptInnerChannel = Fx.ThunkCallback(new AsyncCallback(OnAcceptInnerChannel));
        AsyncCallback onOpenInnerChannel;
        EventHandler onInnerChannelClosed;
        Action onExceptionDequeued;
        Action<object> handleAcceptCallback;
        bool ownsInnerListener;
        object acceptLock;

        public DuplexSessionOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context)
            : base(true, context.Binding, context.BuildInnerChannelListener<IDuplexSessionChannel>())
        {
            this.acceptLock = new object();
            this.inputChannelAcceptor = new DuplexSessionOneWayInputChannelAcceptor(this);
            this.packetRoutable = bindingElement.PacketRoutable;
            this.maxAcceptedChannels = bindingElement.MaxAcceptedChannels;
            this.Acceptor = this.inputChannelAcceptor;
            this.idleTimeout = bindingElement.ChannelPoolSettings.IdleTimeout;
            this.onOpenInnerChannel = Fx.ThunkCallback(new AsyncCallback(OnOpenInnerChannel));
            this.ownsInnerListener = true;
            this.onInnerChannelClosed = new EventHandler(OnInnerChannelClosed);
        }

        bool IsAcceptNecessary
        {
            get
            {
                return !acceptPending
                    && (activeChannels < maxAcceptedChannels)
                    && (this.innerChannelListener.State == CommunicationState.Opened);
            }
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IDuplexSessionChannel>)this.InnerChannelListener;
            this.inputChannelAcceptor.TransferInnerChannelListener(this.innerChannelListener); // acceptor now owns the lifetime
            this.ownsInnerListener = false;
            base.OnOpening();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            ActionItem.Schedule(new Action<object>(AcceptLoop), null);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.ownsInnerListener && this.innerChannelListener != null) // Open didn't complete
            {
                this.innerChannelListener.Abort();
            }
        }

        void AcceptLoop(object state)
        {
            AcceptLoop(null);
        }

        // we need to kick off an accept (and possibly process a completion as well)
        void AcceptLoop(IAsyncResult pendingResult)
        {
            IDuplexSessionChannel pendingChannel = null;

            if (pendingResult != null)
            {
                if (!ProcessEndAccept(pendingResult, out pendingChannel))
                {
                    return;
                }
                pendingResult = null;
            }

            lock (acceptLock)
            {
                while (IsAcceptNecessary)
                {
                    Exception exceptionToEnqueue = null;
                    try
                    {
                        IAsyncResult result = null;

                        try
                        {
                            result = this.innerChannelListener.BeginAcceptChannel(TimeSpan.MaxValue, onAcceptInnerChannel, this);
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            continue;
                        }

                        acceptPending = true;
                        if (!result.CompletedSynchronously)
                        {
                            break;
                        }

                        if (this.handleAcceptCallback == null)
                        {
                            this.handleAcceptCallback = new Action<object>(HandleAcceptCallback);
                        }

                        if (pendingChannel != null)
                        {
                            // don't starve our completed Accept
                            ActionItem.Schedule(handleAcceptCallback, pendingChannel);
                            pendingChannel = null;
                        }

                        IDuplexSessionChannel channel = null;
                        if (ProcessEndAccept(result, out channel))
                        {
                            if (channel != null)
                            {
                                ActionItem.Schedule(handleAcceptCallback, channel);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        exceptionToEnqueue = e;
                    }

                    if (exceptionToEnqueue != null)
                    {
                        this.inputChannelAcceptor.Enqueue(exceptionToEnqueue, null, false);
                    }
                }
            }

            if (pendingChannel != null)
            {
                HandleAcceptComplete(pendingChannel);
            }
        }

        // return true if the loop should continue
        bool ProcessEndAccept(IAsyncResult result, out IDuplexSessionChannel channel)
        {
            channel = null;
            Exception exceptionToEnqueue = null;
            bool success = false;
            try
            {
                channel = innerChannelListener.EndAcceptChannel(result);
                success = true;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                exceptionToEnqueue = e;
            }

            if (success)
            {
                if (channel != null)
                {
                    channel.Closed += this.onInnerChannelClosed;
                    bool traceMaxInboundChannels = false;
                    lock (acceptLock)
                    {
                        this.acceptPending = false;
                        activeChannels++;
                        if (activeChannels >= maxAcceptedChannels)
                        {
                            traceMaxInboundChannels = true;
                        }
                    }

                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        if (traceMaxInboundChannels)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning,
                                TraceCode.MaxAcceptedChannelsReached,
                                SR.GetString(SR.TraceCodeMaxAcceptedChannelsReached),
                                new StringTraceRecord("MaxAcceptedChannels", maxAcceptedChannels.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                                this,
                                null);
                        }
                    }

                }
                else
                {
                    // we're at EOF. close up the Acceptor and break out of our loop
                    this.inputChannelAcceptor.Close();
                    return false;
                }
            }
            else if (exceptionToEnqueue != null)
            {
                // see what the state of the inner listener is. If it's still open, don't block the accept loop
                bool canDispatchOnThisThread = (innerChannelListener.State != CommunicationState.Opened);
                if (this.onExceptionDequeued == null)
                {
                    this.onExceptionDequeued = new Action(OnExceptionDequeued);
                }
                this.inputChannelAcceptor.Enqueue(exceptionToEnqueue, this.onExceptionDequeued, canDispatchOnThisThread);
            }
            else
            {
                lock (acceptLock)
                {
                    this.acceptPending = false;
                }
            }

            return true;
        }

        void OnExceptionDequeued()
        {
            lock (acceptLock)
            {
                this.acceptPending = false;
            }
            AcceptLoop(null);
        }

        static void OnAcceptInnerChannel(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            DuplexSessionOneWayChannelListener thisPtr = (DuplexSessionOneWayChannelListener)result.AsyncState;
            thisPtr.AcceptLoop(result);
        }

        void HandleAcceptCallback(object state)
        {
            this.HandleAcceptComplete((IDuplexSessionChannel)state);
        }

        void OnInnerChannelClosed(object sender, EventArgs e)
        {
            // Reduce our quota and kick off an accept
            IDuplexSessionChannel channel = (IDuplexSessionChannel)sender;
            channel.Closed -= this.onInnerChannelClosed;

            lock (acceptLock)
            {
                activeChannels--;
            }
            this.AcceptLoop(null);
        }

        void HandleAcceptComplete(IDuplexSessionChannel channel)
        {
            Exception exceptionToEnqueue = null;
            bool success = false;

            this.inputChannelAcceptor.PrepareChannel(channel);
            IAsyncResult openResult = null;
            try
            {
                openResult = channel.BeginOpen(this.idleTimeout, onOpenInnerChannel, channel);
                success = true;
            }
            catch (CommunicationException e) // ---- CommunicationException
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                exceptionToEnqueue = e;
            }
            finally
            {
                if (!success && channel != null)
                {
                    channel.Abort();
                }
            }

            if (success)
            {
                if (openResult.CompletedSynchronously)
                {
                    CompleteOpen(channel, openResult);
                }
            }
            else
            {
                if (exceptionToEnqueue != null)
                {
                    this.inputChannelAcceptor.Enqueue(exceptionToEnqueue, null);
                }
            }
        }

        void OnOpenInnerChannel(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IDuplexSessionChannel channel = (IDuplexSessionChannel)result.AsyncState;
            CompleteOpen(channel, result);
        }

        // open channel and start receiving messages
        void CompleteOpen(IDuplexSessionChannel channel, IAsyncResult result)
        {
            Exception exceptionToEnqueue = null;
            bool success = false;
            try
            {
                channel.EndOpen(result);
                success = true;
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (TimeoutException e)
            {
                if (TD.OpenTimeoutIsEnabled())
                {
                    TD.OpenTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                exceptionToEnqueue = e;
            }
            finally
            {
                if (!success)
                {
                    channel.Abort();
                }
            }

            if (success)
            {
                this.inputChannelAcceptor.AcceptInnerChannel(this, channel);
            }
            else if (exceptionToEnqueue != null)
            {
                this.inputChannelAcceptor.Enqueue(exceptionToEnqueue, null);
            }
        }

        class DuplexSessionOneWayInputChannelAcceptor : InputChannelAcceptor
        {
            ChannelTracker<IDuplexSessionChannel, ChannelReceiver> receivers;
            IChannelListener<IDuplexSessionChannel> innerChannelListener;

            public DuplexSessionOneWayInputChannelAcceptor(DuplexSessionOneWayChannelListener listener)
                : base(listener)
            {
                this.receivers = new ChannelTracker<IDuplexSessionChannel, ChannelReceiver>();
            }

            public void TransferInnerChannelListener(IChannelListener<IDuplexSessionChannel> innerChannelListener)
            {
                Fx.Assert(this.innerChannelListener == null, "innerChannelListener must be null prior to transfer");
                bool abortListener = false;
                lock (ThisLock)
                {
                    this.innerChannelListener = innerChannelListener;
                    if (this.State == CommunicationState.Closing || this.State == CommunicationState.Closed)
                    {
                        // abort happened before we completed the transfer
                        abortListener = true;
                    }
                }

                if (abortListener)
                {
                    innerChannelListener.Abort();
                }
            }

            public void AcceptInnerChannel(DuplexSessionOneWayChannelListener listener, IDuplexSessionChannel channel)
            {
                ChannelReceiver channelReceiver = new ChannelReceiver(listener, channel);
                this.receivers.Add(channel, channelReceiver);
                channelReceiver.StartReceiving();
            }

            public void PrepareChannel(IDuplexSessionChannel channel)
            {
                this.receivers.PrepareChannel(channel);
            }

            protected override InputChannel OnCreateChannel()
            {
                return new DuplexSessionOneWayInputChannel(this.ChannelManager, null);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnOpen(timeoutHelper.RemainingTime());
                this.receivers.Open(timeoutHelper.RemainingTime());
                this.innerChannelListener.Open(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.receivers, this.innerChannelListener);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                ChainedOpenAsyncResult.End(result);
            }

            protected override void OnAbort()
            {
                base.OnAbort();
                if (!TransferReceivers())
                {
                    this.receivers.Abort();
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Abort();
                    }
                }
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                if (!TransferReceivers())
                {
                    this.receivers.Close(timeoutHelper.RemainingTime());
                    this.innerChannelListener.Close(timeoutHelper.RemainingTime());
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                List<ICommunicationObject> objectsToClose = new List<ICommunicationObject>();
                if (!TransferReceivers())
                {
                    objectsToClose.Add(this.receivers);
                    objectsToClose.Add(this.innerChannelListener);
                }

                return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, objectsToClose);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedCloseAsyncResult.End(result);
            }

            // used to decouple our channel and listener lifetimes
            bool TransferReceivers()
            {
                DuplexSessionOneWayInputChannel singletonChannel = (DuplexSessionOneWayInputChannel)base.GetCurrentChannel();
                if (singletonChannel == null)
                {
                    return false;
                }
                else
                {
                    return singletonChannel.TransferReceivers(this.receivers, this.innerChannelListener);
                }
            }

            class DuplexSessionOneWayInputChannel : InputChannel
            {
                ChannelTracker<IDuplexSessionChannel, ChannelReceiver> receivers;
                IChannelListener<IDuplexSessionChannel> innerChannelListener;

                public DuplexSessionOneWayInputChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
                    : base(channelManager, localAddress)
                {
                }

                public bool TransferReceivers(ChannelTracker<IDuplexSessionChannel, ChannelReceiver> receivers,
                    IChannelListener<IDuplexSessionChannel> innerChannelListener)
                {
                    lock (ThisLock)
                    {
                        if (this.State != CommunicationState.Opened)
                        {
                            return false;
                        }

                        this.receivers = receivers;
                        this.innerChannelListener = innerChannelListener;
                        return true;
                    }
                }

                protected override void OnAbort()
                {
                    if (this.receivers != null)
                    {
                        Fx.Assert(this.innerChannelListener != null, "innerChannelListener and receiver should both be null or non-null");
                        this.receivers.Abort();
                        this.innerChannelListener.Abort();
                    }
                    base.OnAbort();
                }

                protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    List<ICommunicationObject> objectsToClose = new List<ICommunicationObject>();
                    if (this.receivers != null)
                    {
                        objectsToClose.Add(this.receivers);
                        objectsToClose.Add(this.innerChannelListener);
                    }

                    return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, objectsToClose);
                }

                protected override void OnEndClose(IAsyncResult result)
                {
                    ChainedCloseAsyncResult.End(result);
                }

                protected override void OnClose(TimeSpan timeout)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    if (this.receivers != null)
                    {
                        Fx.Assert(this.innerChannelListener != null, "innerChannelListener and receiver should both be null or non-null");
                        this.receivers.Close(timeoutHelper.RemainingTime());
                        this.innerChannelListener.Close(timeoutHelper.RemainingTime());
                    }
                    base.OnClose(timeoutHelper.RemainingTime());
                }

            }
        }


        // given an inner channel, pulls messages off of it and enqueues them into the upper channel
        class ChannelReceiver
        {
            Action onMessageDequeued;
            static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(OnReceive));
            DuplexSessionOneWayInputChannelAcceptor acceptor;
            IDuplexSessionChannel channel;
            TimeSpan idleTimeout;
            static Action<object> startReceivingCallback;
            Action<object> onStartReceiveLater;
            Action<object> onDispatchItemsLater;
            bool validateHeader;

            public ChannelReceiver(DuplexSessionOneWayChannelListener parent, IDuplexSessionChannel channel)
            {
                this.channel = channel;
                this.acceptor = parent.inputChannelAcceptor;
                this.idleTimeout = parent.idleTimeout;
                this.validateHeader = parent.packetRoutable;
                this.onMessageDequeued = new Action(OnMessageDequeued);
            }

            void StartReceivingCallback(object state)
            {
                ((ChannelReceiver)state).StartReceiving();
            }

            public void StartReceiving()
            {
                Exception exceptionToEnqueue = null;

                while (true)
                {
                    if (channel.State != CommunicationState.Opened)
                    {
                        channel.Abort();
                        break;
                    }

                    IAsyncResult result = null;
                    try
                    {
                        result = this.channel.BeginTryReceive(this.idleTimeout, onReceive, this);
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        exceptionToEnqueue = e;
                        break;
                    }

                    if (result != null)
                    {
                        if (!result.CompletedSynchronously)
                        {
                            break;
                        }

                        bool dispatch;
                        bool continueLoop = OnCompleteReceive(result, out dispatch);
                        if (dispatch)
                        {
                            Dispatch();
                        }
                        if (!continueLoop)
                        {
                            break;
                        }
                    }
                }

                if (exceptionToEnqueue != null)
                {
                    this.acceptor.Enqueue(exceptionToEnqueue, this.onMessageDequeued);
                }
            }

            bool EnqueueMessage(Message message)
            {
                if (this.validateHeader)
                {
                    if (!PacketRoutableHeader.TryValidateMessage(message))
                    {
                        this.channel.Abort();
                        message.Close();
                        return false;
                    }
                    else
                    {
                        this.validateHeader = false; // only validate the first message on a session
                    }
                }

                return this.acceptor.EnqueueWithoutDispatch(message, this.onMessageDequeued);
            }

            void OnStartReceiveLater(object state)
            {
                StartReceiving();
            }

            void OnDispatchItemsLater(object state)
            {
                Dispatch();
            }

            void Dispatch()
            {
                this.acceptor.DispatchItems();
            }

            // returns true if the Receive Loop should continue (or be started if it's not running)
            bool OnCompleteReceive(IAsyncResult result, out bool dispatchLater)
            {
                Exception exceptionToEnqueue = null;
                Message message = null;
                bool startLoop = false;
                dispatchLater = false;

                try
                {
                    if (!this.channel.EndTryReceive(result, out message))
                    {
                        this.channel.Abort(); // we've hit our IdleTimeout
                    }
                    else if (message == null)
                    {
                        this.channel.Close(); // read EOF, close our half of the session
                    }
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    startLoop = (this.channel.State == CommunicationState.Opened);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    startLoop = (this.channel.State == CommunicationState.Opened);
                }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exceptionToEnqueue = e;
                }

                if (message != null)
                {
                    dispatchLater = EnqueueMessage(message);
                }
                else if (exceptionToEnqueue != null)
                {
                    dispatchLater = this.acceptor.EnqueueWithoutDispatch(exceptionToEnqueue, this.onMessageDequeued);
                }

                return startLoop;
            }

            void OnMessageDequeued()
            {
                IAsyncResult result = null;
                Exception exceptionToEnqueue = null;

                try
                {
                    result = this.channel.BeginTryReceive(this.idleTimeout, onReceive, this);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
#pragma warning suppress 56500 // [....], transferring exception to input queue to be pulled off by user
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exceptionToEnqueue = e;
                }

                if (result != null)
                {
                    if (result.CompletedSynchronously)
                    {
                        bool dispatchLater;

                        if (OnCompleteReceive(result, out dispatchLater))
                        {
                            if (onStartReceiveLater == null)
                            {
                                onStartReceiveLater = new Action<object>(OnStartReceiveLater);
                            }
                            ActionItem.Schedule(onStartReceiveLater, null);
                        }

                        if (dispatchLater)
                        {
                            if (onDispatchItemsLater == null)
                            {
                                onDispatchItemsLater = new Action<object>(OnDispatchItemsLater);
                            }
                            ActionItem.Schedule(onDispatchItemsLater, null);
                        }
                    }
                }
                else if (exceptionToEnqueue != null)
                {
                    this.acceptor.Enqueue(exceptionToEnqueue, this.onMessageDequeued, false);
                }
                else // need to kickoff a new loop 
                {
                    if (this.channel.State == CommunicationState.Opened)
                    {
                        if (startReceivingCallback == null)
                        {
                            startReceivingCallback = new Action<object>(StartReceivingCallback);
                        }

                        ActionItem.Schedule(startReceivingCallback, this);
                    }
                }
            }

            static void OnReceive(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ChannelReceiver thisPtr = (ChannelReceiver)result.AsyncState;
                bool dispatch;
                if (thisPtr.OnCompleteReceive(result, out dispatch))
                {
                    thisPtr.StartReceiving();
                }

                if (dispatch)
                {
                    thisPtr.Dispatch();
                }
            }
        }
    }
}
