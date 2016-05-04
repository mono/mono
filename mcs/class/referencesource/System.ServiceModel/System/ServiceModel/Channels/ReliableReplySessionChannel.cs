//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    // Note on locking:
    // The following rule must be followed in order to avoid deadlocks: ReliableRequestContext
    // locks MUST NOT be taken while under the ReliableReplySessionChannel lock.
    //
    // lock(context)-->lock(channel) ok.    
    // lock(channel)-->lock(context) BAD!
    //
    sealed class ReliableReplySessionChannel : ReplyChannel, IReplySessionChannel
    {
        List<Int64> acked = new List<Int64>();
        static Action<object> asyncReceiveComplete = new Action<object>(AsyncReceiveCompleteStatic);
        IServerReliableChannelBinder binder;
        ReplyHelper closeSequenceReplyHelper;
        ReliableInputConnection connection;
        bool contextAborted;
        DeliveryStrategy<RequestContext> deliveryStrategy;
        ReliableRequestContext lastReply;
        bool lastReplyAcked;
        Int64 lastReplySequenceNumber = Int64.MinValue;
        ReliableChannelListenerBase<IReplySessionChannel> listener;
        InterruptibleWaitObject messagingCompleteWaitObject;
        Int64 nextReplySequenceNumber;
        static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompletedStatic));
        string perfCounterId;
        Dictionary<Int64, ReliableRequestContext> requestsByRequestSequenceNumber = new Dictionary<Int64, ReliableRequestContext>();
        Dictionary<Int64, ReliableRequestContext> requestsByReplySequenceNumber = new Dictionary<Int64, ReliableRequestContext>();
        ServerReliableSession session;
        ReplyHelper terminateSequenceReplyHelper;

        public ReliableReplySessionChannel(
            ReliableChannelListenerBase<IReplySessionChannel> listener,
            IServerReliableChannelBinder binder,
            FaultHelper faultHelper,
            UniqueId inputID,
            UniqueId outputID)
            : base(listener, binder.LocalAddress)
        {
            this.listener = listener;
            this.connection = new ReliableInputConnection();
            this.connection.ReliableMessagingVersion = this.listener.ReliableMessagingVersion;
            this.binder = binder;
            this.session = new ServerReliableSession(this, listener, binder, faultHelper, inputID, outputID);
            this.session.UnblockChannelCloseCallback = this.UnblockClose;

            if (this.listener.Ordered)
                this.deliveryStrategy = new OrderedDeliveryStrategy<RequestContext>(this, this.listener.MaxTransferWindowSize, true);
            else
                this.deliveryStrategy = new UnorderedDeliveryStrategy<RequestContext>(this, this.listener.MaxTransferWindowSize);
            this.binder.Faulted += OnBinderFaulted;
            this.binder.OnException += OnBinderException;
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject = new InterruptibleWaitObject(false);
            }
            this.session.Open(TimeSpan.Zero);

            if (PerformanceCounters.PerformanceCountersEnabled)
                this.perfCounterId = this.listener.Uri.ToString().ToUpperInvariant();

            if (binder.HasSession)
            {
                try
                {
                    this.StartReceiving(false);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.session.OnUnknownException(e);
                }
            }
        }

        public IServerReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        bool IsMessagingCompleted
        {
            get
            {
                lock (this.ThisLock)
                {
                    return this.connection.AllAdded && (this.requestsByRequestSequenceNumber.Count == 0) && this.lastReplyAcked;
                }
            }
        }

        MessageVersion MessageVersion
        {
            get
            {
                return this.listener.MessageVersion;
            }
        }

        int PendingRequestContexts
        {
            get
            {
                lock (this.ThisLock)
                {
                    return (this.requestsByRequestSequenceNumber.Count - this.requestsByReplySequenceNumber.Count);
                }
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        void AbortContexts()
        {
            lock (this.ThisLock)
            {
                if (this.contextAborted)
                    return;
                this.contextAborted = true;
            }

            Dictionary<Int64, ReliableRequestContext>.ValueCollection contexts = this.requestsByRequestSequenceNumber.Values;

            foreach (ReliableRequestContext request in contexts)
            {
                request.Abort();
            }

            this.requestsByRequestSequenceNumber.Clear();
            this.requestsByReplySequenceNumber.Clear();


            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.lastReply != null)
                    this.lastReply.Abort();
            }
        }

        void AddAcknowledgementHeader(Message message)
        {
            WsrmUtilities.AddAcknowledgementHeader(
                this.listener.ReliableMessagingVersion,
                message,
                this.session.InputID,
                this.connection.Ranges,
                this.connection.IsLastKnown,
                this.listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount);
        }

        static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult)state;
            ReliableReplySessionChannel channel = (ReliableReplySessionChannel)(result.AsyncState);
            try
            {
                if (channel.HandleReceiveComplete(result))
                {
                    channel.StartReceiving(true);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                channel.session.OnUnknownException(e);
            }
        }

        IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        IAsyncResult BeginCloseOutput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                ReliableRequestContext reply = this.lastReply;

                if (reply == null)
                    return new CloseOutputCompletedAsyncResult(callback, state);
                else
                    return reply.BeginReplyInternal(null, timeout, callback, state);
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfClosed();
                    this.CreateCloseSequenceReplyHelper();
                }

                return this.closeSequenceReplyHelper.BeginWaitAndReply(timeout, callback, state);
            }
        }

        IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(this.session.InputID,
                this.session.OutputID, timeout, callback, state);
        }

        Message CreateAcknowledgement(SequenceRangeCollection ranges)
        {
            Message message = WsrmUtilities.CreateAcknowledgmentMessage(
                this.MessageVersion,
                this.listener.ReliableMessagingVersion,
                this.session.InputID,
                ranges,
                this.connection.IsLastKnown,
                this.listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount);

            return message;
        }

        Message CreateSequenceClosedFault()
        {
            Message message = new SequenceClosedFault(this.session.InputID).CreateMessage(
                this.listener.MessageVersion, this.listener.ReliableMessagingVersion);
            this.AddAcknowledgementHeader(message);
            return message;
        }

        bool CreateCloseSequenceReplyHelper()
        {
            if (this.State == CommunicationState.Faulted || this.Aborted)
            {
                return false;
            }

            if (this.closeSequenceReplyHelper == null)
            {
                this.closeSequenceReplyHelper = new ReplyHelper(this, CloseSequenceReplyProvider.Instance,
                    true);
            }

            return true;
        }

        bool CreateTerminateSequenceReplyHelper()
        {
            if (this.State == CommunicationState.Faulted || this.Aborted)
            {
                return false;
            }

            if (this.terminateSequenceReplyHelper == null)
            {
                this.terminateSequenceReplyHelper = new ReplyHelper(this,
                    TerminateSequenceReplyProvider.Instance, false);
            }

            return true;
        }

        void CloseOutput(TimeSpan timeout)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                ReliableRequestContext reply = this.lastReply;

                if (reply != null)
                    reply.ReplyInternal(null, timeout);
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfClosed();
                    this.CreateCloseSequenceReplyHelper();
                }

                this.closeSequenceReplyHelper.WaitAndReply(timeout);
            }
        }

        bool ContainsRequest(Int64 requestSeqNum)
        {
            lock (this.ThisLock)
            {
                bool haveRequestInDictionary = this.requestsByRequestSequenceNumber.ContainsKey(requestSeqNum);

                if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    return (haveRequestInDictionary
                        || ((this.lastReply != null) && (this.lastReply.RequestSequenceNumber == requestSeqNum) && (!this.lastReplyAcked)));
                }
                else
                {
                    return haveRequestInDictionary;
                }
            }
        }

        void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        void EndCloseOutput(IAsyncResult result)
        {
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (result is CloseOutputCompletedAsyncResult)
                    CloseOutputCompletedAsyncResult.End(result);
                else
                    this.lastReply.EndReplyInternal(result);
            }
            else
            {
                this.closeSequenceReplyHelper.EndWaitAndReply(result);
            }
        }

        void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IReplySessionChannel))
            {
                return (T)(object)this;
            }

            T baseProperty = base.GetProperty<T>();

            if (baseProperty != null)
            {
                return baseProperty;
            }

            T innerProperty = this.binder.Channel.GetProperty<T>();
            if ((innerProperty == null) && (typeof(T) == typeof(FaultConverter)))
            {
                return (T)(object)FaultConverter.GetDefaultFaultConverter(this.listener.MessageVersion);
            }
            else
            {
                return innerProperty;
            }
        }

        bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;

            if (this.Binder.EndTryReceive(result, out context))
            {
                if (context == null)
                {
                    bool terminated = false;

                    lock (this.ThisLock)
                    {
                        terminated = this.connection.Terminate();
                    }

                    if (!terminated && (this.Binder.State == CommunicationState.Opened))
                    {
                        Exception e = new CommunicationException(SR.GetString(SR.EarlySecurityClose));
                        this.session.OnLocalFault(e, (Message)null, null);
                    }

                    return false;
                }

                WsrmMessageInfo info = WsrmMessageInfo.Get(this.listener.MessageVersion,
                    this.listener.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(),
                    context.RequestMessage);

                this.StartReceiving(false);
                this.ProcessRequest(context, info);
                return false;
            }

            return true;
        }

        protected override void OnAbort()
        {
            if (this.closeSequenceReplyHelper != null)
            {
                this.closeSequenceReplyHelper.Abort();
            }

            this.connection.Abort(this);
            if (this.terminateSequenceReplyHelper != null)
            {
                this.terminateSequenceReplyHelper.Abort();
            }
            this.session.Abort();
            this.AbortContexts();
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject.Abort(this);
            }
            this.listener.OnReliableChannelAbort(this.session.InputID, this.session.OutputID);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfCloseInvalid();
            bool wsrmFeb2005 = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;

            OperationWithTimeoutBeginCallback[] beginOperations =
                new OperationWithTimeoutBeginCallback[] {
                    new OperationWithTimeoutBeginCallback (this.BeginCloseOutput),
                    wsrmFeb2005
                    ? new OperationWithTimeoutBeginCallback(this.connection.BeginClose)
                    : new OperationWithTimeoutBeginCallback(this.BeginTerminateSequence),
                    wsrmFeb2005
                    ? new OperationWithTimeoutBeginCallback(this.messagingCompleteWaitObject.BeginWait)
                    : new OperationWithTimeoutBeginCallback(this.connection.BeginClose),
                    new OperationWithTimeoutBeginCallback(this.session.BeginClose),
                    new OperationWithTimeoutBeginCallback(this.BeginCloseBinder),
                    new OperationWithTimeoutBeginCallback(this.BeginUnregisterChannel),
                    new OperationWithTimeoutBeginCallback(base.OnBeginClose)
                };

            OperationEndCallback[] endOperations =
                new OperationEndCallback[] {
                    new OperationEndCallback(this.EndCloseOutput),
                    wsrmFeb2005
                    ? new OperationEndCallback(this.connection.EndClose)
                    : new OperationEndCallback(this.EndTerminateSequence),
                    wsrmFeb2005
                    ? new OperationEndCallback(this.messagingCompleteWaitObject.EndWait)
                    : new OperationEndCallback(this.connection.EndClose),
                    new OperationEndCallback(this.session.EndClose),
                    new OperationEndCallback(this.EndCloseBinder),
                    new OperationEndCallback(this.EndUnregisterChannel),
                    new OperationEndCallback(base.OnEndClose)
                };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                beginOperations, endOperations, callback, state);
        }

        void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
                this.session.OnLocalFault(exception, (Message)null, null);
            else
                this.EnqueueAndDispatch(exception, null, false);
        }

        void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();

            exception = new CommunicationException(SR.GetString(SR.EarlySecurityFaulted), exception);
            this.session.OnLocalFault(exception, (Message)null, null);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.ThrowIfCloseInvalid();
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.CloseOutput(timeoutHelper.RemainingTime());
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.connection.Close(timeoutHelper.RemainingTime());
                this.messagingCompleteWaitObject.Wait(timeoutHelper.RemainingTime());
            }
            else
            {
                this.TerminateSequence(timeoutHelper.RemainingTime());
                this.connection.Close(timeoutHelper.RemainingTime());
            }
            this.session.Close(timeoutHelper.RemainingTime());
            this.binder.Close(timeoutHelper.RemainingTime(), MaskingMode.Handled);
            this.listener.OnReliableChannelClose(this.session.InputID, this.session.OutputID,
                timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnClosed()
        {
            this.deliveryStrategy.Dispose();
            this.binder.Faulted -= this.OnBinderFaulted;

            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.lastReply != null)
                {
                    this.lastReply.Abort();
                }
            }

            base.OnClosed();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
            if (PerformanceCounters.PerformanceCountersEnabled)
                PerformanceCounters.SessionFaulted(this.perfCounterId);
        }

        static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            ReliableReplySessionChannel channel = (ReliableReplySessionChannel)(result.AsyncState);

            try
            {
                if (channel.HandleReceiveComplete(result))
                {
                    channel.StartReceiving(true);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                channel.session.OnUnknownException(e);
            }
        }

        void OnTerminateSequenceCompleted()
        {
            if ((this.session.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                && this.connection.IsSequenceClosed)
            {
                lock (this.ThisLock)
                {
                    this.connection.Terminate();
                }
            }
        }

        bool PrepareReply(ReliableRequestContext context)
        {
            lock (this.ThisLock)
            {
                if (this.Aborted || this.State == CommunicationState.Faulted || this.State == CommunicationState.Closed)
                    return false;

                long requestSequenceNumber = context.RequestSequenceNumber;
                bool wsrmFeb2005 = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;

                if (wsrmFeb2005 && (this.connection.Last == requestSequenceNumber))
                {
                    if (this.lastReply == null)
                        this.lastReply = context;
                    this.requestsByRequestSequenceNumber.Remove(requestSequenceNumber);
                    bool canReply = this.connection.AllAdded && (this.State == CommunicationState.Closing);
                    if (!canReply)
                        return false;
                }
                else
                {
                    if (this.State == CommunicationState.Closing)
                        return false;

                    if (!context.HasReply)
                    {
                        this.requestsByRequestSequenceNumber.Remove(requestSequenceNumber);
                        return true;
                    }
                }

                // won't throw if you do not need next sequence number
                if (this.nextReplySequenceNumber == Int64.MaxValue)
                {
                    MessageNumberRolloverFault fault = new MessageNumberRolloverFault(this.session.OutputID);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                }
                context.SetReplySequenceNumber(++this.nextReplySequenceNumber);

                if (wsrmFeb2005 && (this.connection.Last == requestSequenceNumber))
                {
                    if (!context.HasReply)
                        this.lastReplyAcked = true;   //If Last Reply has no user data, it does not need to be acked. Here we just set it as its ack received.
                    this.lastReplySequenceNumber = this.nextReplySequenceNumber;
                    context.SetLastReply(this.lastReplySequenceNumber);
                }
                else if (context.HasReply)
                {
                    this.requestsByReplySequenceNumber.Add(this.nextReplySequenceNumber, context);
                }

                return true;
            }
        }

        Message PrepareReplyMessage(Int64 replySequenceNumber, bool isLast, SequenceRangeCollection ranges, Message reply)
        {
            this.AddAcknowledgementHeader(reply);

            WsrmUtilities.AddSequenceHeader(
                this.listener.ReliableMessagingVersion,
                reply,
                this.session.OutputID,
                replySequenceNumber,
                isLast);

            return reply;
        }

        void ProcessAcknowledgment(WsrmAcknowledgmentInfo info)
        {
            lock (this.ThisLock)
            {
                if (this.Aborted || this.State == CommunicationState.Faulted || this.State == CommunicationState.Closed)
                    return;

                if (this.requestsByReplySequenceNumber.Count > 0)
                {
                    Int64 reply;

                    this.acked.Clear();

                    foreach (KeyValuePair<Int64, ReliableRequestContext> pair in this.requestsByReplySequenceNumber)
                    {
                        reply = pair.Key;
                        if (info.Ranges.Contains(reply))
                        {
                            this.acked.Add(reply);
                        }
                    }

                    for (int i = 0; i < this.acked.Count; i++)
                    {
                        reply = this.acked[i];
                        this.requestsByRequestSequenceNumber.Remove(
                            this.requestsByReplySequenceNumber[reply].RequestSequenceNumber);
                        this.requestsByReplySequenceNumber.Remove(reply);
                    }

                    if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                    {
                        if (!this.lastReplyAcked && (this.lastReplySequenceNumber != Int64.MinValue))
                        {
                            this.lastReplyAcked = info.Ranges.Contains(this.lastReplySequenceNumber);
                        }
                    }
                }
            }
        }

        void ProcessAckRequested(RequestContext context)
        {
            try
            {
                using (Message reply = CreateAcknowledgement(this.connection.Ranges))
                {
                    context.Reply(reply);
                }
            }
            finally
            {
                context.RequestMessage.Close();
                context.Close();
            }
        }

        void ProcessShutdown11(RequestContext context, WsrmMessageInfo info)
        {
            bool cleanup = true;

            try
            {
                bool isTerminate = (info.TerminateSequenceInfo != null);
                WsrmRequestInfo requestInfo = isTerminate
                    ? (WsrmRequestInfo)info.TerminateSequenceInfo
                    : (WsrmRequestInfo)info.CloseSequenceInfo;
                Int64 last = isTerminate ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;

                if (!WsrmUtilities.ValidateWsrmRequest(this.session, requestInfo, this.binder, context))
                {
                    cleanup = false;
                    return;
                }

                bool scheduleShutdown = false;
                Exception remoteFaultException = null;
                ReplyHelper closeHelper = null;
                bool haveAllReplyAcks = true;
                bool isLastLargeEnough = true;
                bool isLastConsistent = true;

                lock (this.ThisLock)
                {
                    if (!this.connection.IsLastKnown)
                    {
                        // All requests and replies must be acknowledged.
                        if (this.requestsByRequestSequenceNumber.Count == 0)
                        {
                            if (isTerminate)
                            {
                                if (this.connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
                                {
                                    scheduleShutdown = true;
                                }
                                else if (isLastLargeEnough)
                                {
                                    remoteFaultException = new ProtocolException(SR.GetString(SR.EarlyTerminateSequence));
                                }
                            }
                            else
                            {
                                scheduleShutdown = this.connection.SetCloseSequenceLast(last);
                                isLastLargeEnough = scheduleShutdown;
                            }

                            if (scheduleShutdown)
                            {
                                // (1) !isTerminate && !IsLastKnown, CloseSequence received before TerminateSequence.
                                // - Need to ensure helper to delay the reply until Close.
                                // (2) isTerminate && !IsLastKnown, TerminateSequence received before CloseSequence.
                                // - Close not required, ensure it is created so we can bypass it.
                                if (!this.CreateCloseSequenceReplyHelper())
                                {
                                    return;
                                }

                                // Capture the helper in order to unblock it.
                                if (isTerminate)
                                {
                                    closeHelper = this.closeSequenceReplyHelper;
                                }

                                this.session.SetFinalAck(this.connection.Ranges);
                                this.deliveryStrategy.Dispose();
                            }
                        }
                        else
                        {
                            haveAllReplyAcks = false;
                        }
                    }
                    else
                    {
                        isLastConsistent = (last == this.connection.Last);
                    }
                }

                WsrmFault fault = null;

                if (!isLastLargeEnough)
                {
                    string faultString = SR.GetString(SR.SequenceTerminatedSmallLastMsgNumber);
                    string exceptionString = SR.GetString(SR.SmallLastMsgNumberExceptionString);
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, faultString,
                        exceptionString);
                }
                else if (!haveAllReplyAcks)
                {
                    string faultString = SR.GetString(SR.SequenceTerminatedNotAllRepliesAcknowledged);
                    string exceptionString = SR.GetString(SR.NotAllRepliesAcknowledgedExceptionString);
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultString,
                        exceptionString);
                }
                else if (!isLastConsistent)
                {
                    string faultString = SR.GetString(SR.SequenceTerminatedInconsistentLastMsgNumber);
                    string exceptionString = SR.GetString(SR.InconsistentLastMsgNumberExceptionString);
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                        faultString, exceptionString);
                }
                else if (remoteFaultException != null)
                {
                    Message message = WsrmUtilities.CreateTerminateMessage(this.MessageVersion,
                        this.listener.ReliableMessagingVersion, this.session.OutputID);
                    this.AddAcknowledgementHeader(message);

                    using (message)
                    {
                        context.Reply(message);
                    }

                    this.session.OnRemoteFault(remoteFaultException);
                    return;
                }

                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, context);
                    cleanup = false;
                    return;
                }

                if (isTerminate)
                {
                    if (closeHelper != null)
                    {
                        closeHelper.UnblockWaiter();
                    }

                    lock (this.ThisLock)
                    {
                        if (!this.CreateTerminateSequenceReplyHelper())
                        {
                            return;
                        }
                    }
                }

                ReplyHelper replyHelper = isTerminate ? this.terminateSequenceReplyHelper : this.closeSequenceReplyHelper;

                if (!replyHelper.TransferRequestContext(context, info))
                {
                    replyHelper.Reply(context, info, this.DefaultSendTimeout, MaskingMode.All);

                    if (isTerminate)
                    {
                        this.OnTerminateSequenceCompleted();
                    }
                }
                else
                {
                    cleanup = false;
                }

                if (scheduleShutdown)
                {
                    ActionItem.Schedule(this.ShutdownCallback, null);
                }
            }
            finally
            {
                if (cleanup)
                {
                    context.RequestMessage.Close();
                    context.Close();
                }
            }
        }

        public void ProcessDemuxedRequest(RequestContext context, WsrmMessageInfo info)
        {
            try
            {
                this.ProcessRequest(context, info);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.session.OnUnknownException(e);
            }
        }

        void ProcessRequest(RequestContext context, WsrmMessageInfo info)
        {
            bool closeMessage = true;
            bool closeContext = true;

            try
            {
                if (!this.session.ProcessInfo(info, context))
                {
                    closeMessage = false;
                    closeContext = false;
                    return;
                }

                if (!this.session.VerifyDuplexProtocolElements(info, context))
                {
                    closeMessage = false;
                    closeContext = false;
                    return;
                }

                this.session.OnRemoteActivity(false);

                if (info.CreateSequenceInfo != null)
                {
                    EndpointAddress acksTo;

                    if (WsrmUtilities.ValidateCreateSequence<IReplySessionChannel>(info, this.listener, this.binder.Channel, out acksTo))
                    {
                        Message response = WsrmUtilities.CreateCreateSequenceResponse(this.listener.MessageVersion,
                            this.listener.ReliableMessagingVersion, true, info.CreateSequenceInfo,
                            this.listener.Ordered, this.session.InputID, acksTo);

                        using (context)
                        {
                            using (response)
                            {
                                if (this.Binder.AddressResponse(info.Message, response))
                                    context.Reply(response, this.DefaultSendTimeout);
                            }
                        }
                    }
                    else
                    {
                        this.session.OnLocalFault(info.FaultException, info.FaultReply, context);
                    }

                    closeContext = false;
                    return;
                }

                closeContext = false;
                if (info.AcknowledgementInfo != null)
                {
                    ProcessAcknowledgment(info.AcknowledgementInfo);
                    closeContext = (info.Action == WsrmIndex.GetSequenceAcknowledgementActionString(this.listener.ReliableMessagingVersion));
                }

                if (!closeContext)
                {
                    closeMessage = false;
                    if (info.SequencedMessageInfo != null)
                    {
                        ProcessSequencedMessage(context, info.Action, info.SequencedMessageInfo);
                    }
                    else if (info.TerminateSequenceInfo != null)
                    {
                        if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                        {
                            ProcessTerminateSequenceFeb2005(context, info);
                        }
                        else if (info.TerminateSequenceInfo.Identifier == this.session.InputID)
                        {
                            ProcessShutdown11(context, info);
                        }
                        else    // Identifier == OutputID
                        {
                            WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                                SR.GetString(SR.SequenceTerminatedUnsupportedTerminateSequence),
                                SR.GetString(SR.UnsupportedTerminateSequenceExceptionString));

                            this.session.OnLocalFault(fault.CreateException(), fault, context);
                            closeMessage = false;
                            closeContext = false;
                            return;
                        }
                    }
                    else if (info.CloseSequenceInfo != null)
                    {
                        ProcessShutdown11(context, info);
                    }
                    else if (info.AckRequestedInfo != null)
                    {
                        ProcessAckRequested(context);
                    }
                }

                if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    if (this.IsMessagingCompleted)
                    {
                        this.messagingCompleteWaitObject.Set();
                    }
                }
            }
            finally
            {
                if (closeMessage)
                    info.Message.Close();

                if (closeContext)
                    context.Close();
            }
        }

        // A given reliable request can be in one of three states:
        // 1. Known and Processing: A ReliableRequestContext exists in requestTable but the outcome for
        //      for the request is unknown. Any transport request referencing this reliable request
        //      (by means of the sequence number) must be held until the outcome becomes known.
        // 2. Known and Processed: A ReliableRequestContext exists in the requestTable and the outcome for
        //      for the request is known. The ReliableRequestContext holds that outcome. Any transport requests
        //      referening this reliable request must send the response dictated by the outcome.
        // 3. Unknown: No ReliableRequestContext exists in the requestTable for the referenced reliable request.
        //      In this case a new ReliableRequestContext is added to the requestTable to await some outcome.
        //
        // There are 4 possible outcomes for a reliable request:
        //  a. It is captured and the user replies. Transport replies are then copies of the user's reply.
        //  b. It is captured and the user closes the context. Transport replies are then acknowledgments
        //      that include the sequence number of the reliable request.
        //  c. It is captured and and the user aborts the context. Transport contexts are then aborted.
        //  d. It is not captured. In this case an acknowledgment that includes all sequence numbers
        //      previously captured is sent. Note two sub-cases here:
        //          1. It is not captured because it is dropped (e.g. it doesn't fit in the buffer). In this
        //              case the reliable request's sequence number is not in the acknowledgment.
        //          2. It is not captured because it is a duplicate. In this case the reliable request's
        //              sequence number is included in the acknowledgment. 
        //
        // By following these rules it is possible to support one-way and two-operations without having
        // knowledge of them (the user drives using the request context we give them) and at the same time
        // it is possible to forget about past replies once acknowledgments for them are received.
        void ProcessSequencedMessage(RequestContext context, string action, WsrmSequencedMessageInfo info)
        {
            ReliableRequestContext reliableContext = null;
            WsrmFault fault = null;
            bool needDispatch = false;
            bool scheduleShutdown = false;
            bool wsrmFeb2005 = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            bool wsrm11 = this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            Int64 requestSequenceNumber = info.SequenceNumber;
            bool isLast = wsrmFeb2005 && info.LastMessage;
            bool isLastOnly = wsrmFeb2005 && (action == WsrmFeb2005Strings.LastMessageAction);
            bool isDupe;
            Message message = null;

            lock (this.ThisLock)
            {
                if (this.Aborted || this.State == CommunicationState.Faulted || this.State == CommunicationState.Closed)
                {
                    context.RequestMessage.Close();
                    context.Abort();
                    return;
                }

                isDupe = this.connection.Ranges.Contains(requestSequenceNumber);

                if (!this.connection.IsValid(requestSequenceNumber, isLast))
                {
                    if (wsrmFeb2005)
                    {
                        fault = new LastMessageNumberExceededFault(this.session.InputID);
                    }
                    else
                    {
                        message = this.CreateSequenceClosedFault();

                        if (PerformanceCounters.PerformanceCountersEnabled)
                            PerformanceCounters.MessageDropped(this.perfCounterId);
                    }
                }
                else if (isDupe)
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                        PerformanceCounters.MessageDropped(this.perfCounterId);

                    if (!this.requestsByRequestSequenceNumber.TryGetValue(info.SequenceNumber, out reliableContext))
                    {
                        if ((this.lastReply != null) && (this.lastReply.RequestSequenceNumber == info.SequenceNumber))
                            reliableContext = this.lastReply;
                        else
                            reliableContext = new ReliableRequestContext(context, info.SequenceNumber, this, true);
                    }

                    reliableContext.SetAckRanges(this.connection.Ranges);
                }
                else if ((this.State == CommunicationState.Closing) && !isLastOnly)
                {
                    if (wsrmFeb2005)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                            SR.GetString(SR.SequenceTerminatedSessionClosedBeforeDone),
                            SR.GetString(SR.SessionClosedBeforeDone));
                    }
                    else
                    {
                        message = this.CreateSequenceClosedFault();
                        if (PerformanceCounters.PerformanceCountersEnabled)
                            PerformanceCounters.MessageDropped(this.perfCounterId);
                    }
                }
                // In the unordered case we accept no more than MaxSequenceRanges ranges to limit the
                // serialized ack size and the amount of memory taken by the ack ranges. In the
                // ordered case, the delivery strategy MaxTransferWindowSize quota mitigates this
                // threat.
                else if (this.deliveryStrategy.CanEnqueue(requestSequenceNumber)
                    && (this.requestsByReplySequenceNumber.Count < this.listener.MaxTransferWindowSize)
                    && (this.listener.Ordered || this.connection.CanMerge(requestSequenceNumber)))
                {
                    this.connection.Merge(requestSequenceNumber, isLast);
                    reliableContext = new ReliableRequestContext(context, info.SequenceNumber, this, false);
                    reliableContext.SetAckRanges(this.connection.Ranges);

                    if (!isLastOnly)
                    {
                        needDispatch = this.deliveryStrategy.Enqueue(reliableContext, requestSequenceNumber);
                        this.requestsByRequestSequenceNumber.Add(info.SequenceNumber, reliableContext);
                    }
                    else
                    {
                        this.lastReply = reliableContext;
                    }

                    scheduleShutdown = this.connection.AllAdded;
                }
                else
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                        PerformanceCounters.MessageDropped(this.perfCounterId);
                }
            }

            if (fault != null)
            {
                this.session.OnLocalFault(fault.CreateException(), fault, context);
                return;
            }

            if (reliableContext == null)
            {
                if (message != null)
                {
                    using (message)
                    {
                        context.Reply(message);
                    }
                }

                context.RequestMessage.Close();
                context.Close();
                return;
            }

            if (isDupe && reliableContext.CheckForReplyOrAddInnerContext(context))
            {
                reliableContext.SendReply(context, MaskingMode.All);
                return;
            }

            if (!isDupe && isLastOnly)
            {
                reliableContext.Close();
            }

            if (needDispatch)
            {
                this.Dispatch();
            }

            if (scheduleShutdown)
            {
                ActionItem.Schedule(this.ShutdownCallback, null);
            }
        }

        void ProcessTerminateSequenceFeb2005(RequestContext context, WsrmMessageInfo info)
        {
            bool cleanup = true;

            try
            {
                Message message = null;
                bool isTerminateEarly;
                bool haveAllReplyAcks;

                lock (this.ThisLock)
                {
                    isTerminateEarly = !this.connection.Terminate();
                    haveAllReplyAcks = this.requestsByRequestSequenceNumber.Count == 0;
                }

                WsrmFault fault = null;

                if (isTerminateEarly)
                {
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                        SR.GetString(SR.SequenceTerminatedEarlyTerminateSequence),
                        SR.GetString(SR.EarlyTerminateSequence));
                }
                else if (!haveAllReplyAcks)
                {
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                        SR.GetString(SR.SequenceTerminatedBeforeReplySequenceAcked),
                        SR.GetString(SR.EarlyRequestTerminateSequence));
                }

                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, context);
                    cleanup = false;
                    return;
                }

                message = WsrmUtilities.CreateTerminateMessage(this.MessageVersion,
                    this.listener.ReliableMessagingVersion, this.session.OutputID);
                this.AddAcknowledgementHeader(message);

                using (message)
                {
                    context.Reply(message);
                }
            }
            finally
            {
                if (cleanup)
                {
                    context.RequestMessage.Close();
                    context.Close();
                }
            }
        }

        void StartReceiving(bool canBlock)
        {
            while (true)
            {
                IAsyncResult result = this.binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }
                if (!canBlock)
                {
                    ActionItem.Schedule(asyncReceiveComplete, result);
                    return;
                }
                if (!this.HandleReceiveComplete(result))
                    break;
            }
        }

        void ShutdownCallback(object state)
        {
            this.Shutdown();
        }

        void TerminateSequence(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.CreateTerminateSequenceReplyHelper();
            }

            this.terminateSequenceReplyHelper.WaitAndReply(timeout);
            this.OnTerminateSequenceCompleted();
        }

        IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.CreateTerminateSequenceReplyHelper();
            }

            return this.terminateSequenceReplyHelper.BeginWaitAndReply(timeout, callback, state);
        }

        void EndTerminateSequence(IAsyncResult result)
        {
            this.terminateSequenceReplyHelper.EndWaitAndReply(result);
            this.OnTerminateSequenceCompleted();
        }

        void ThrowIfCloseInvalid()
        {
            bool shouldFault = false;

            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.PendingRequestContexts != 0 || this.connection.Ranges.Count > 1)
                {
                    shouldFault = true;
                }
            }
            else if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.PendingRequestContexts != 0)
                {
                    shouldFault = true;
                }
            }

            if (shouldFault)
            {
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                    SR.GetString(SR.SequenceTerminatedSessionClosedBeforeDone), SR.GetString(SR.SessionClosedBeforeDone));
                this.session.OnLocalFault(null, fault, null);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
            }
        }

        void UnblockClose()
        {
            this.AbortContexts();

            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.messagingCompleteWaitObject.Fault(this);
            }
            else
            {
                if (this.closeSequenceReplyHelper != null)
                {
                    this.closeSequenceReplyHelper.Fault();
                }
                if (this.terminateSequenceReplyHelper != null)
                {
                    this.terminateSequenceReplyHelper.Fault();
                }
            }

            this.connection.Fault(this);
        }

        class CloseOutputCompletedAsyncResult : CompletedAsyncResult
        {
            public CloseOutputCompletedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }
        }

        class ReliableRequestContext : RequestContextBase
        {
            MessageBuffer bufferedReply;
            ReliableReplySessionChannel channel;
            List<RequestContext> innerContexts = new List<RequestContext>();
            bool isLastReply;
            bool outcomeKnown;
            SequenceRangeCollection ranges;
            Int64 requestSequenceNumber;
            Int64 replySequenceNumber;

            public ReliableRequestContext(RequestContext context, Int64 requestSequenceNumber, ReliableReplySessionChannel channel, bool outcome)
                : base(context.RequestMessage, channel.DefaultCloseTimeout, channel.DefaultSendTimeout)
            {
                this.channel = channel;
                this.requestSequenceNumber = requestSequenceNumber;
                this.outcomeKnown = outcome;
                if (!outcome)
                    this.innerContexts.Add(context);
            }

            public bool CheckForReplyOrAddInnerContext(RequestContext innerContext)
            {
                lock (this.ThisLock)
                {
                    if (this.outcomeKnown)
                        return true;
                    this.innerContexts.Add(innerContext);
                    return false;
                }
            }

            public bool HasReply
            {
                get
                {
                    return (this.bufferedReply != null);
                }
            }

            public Int64 RequestSequenceNumber
            {
                get
                {
                    return this.requestSequenceNumber;
                }
            }

            void AbortInnerContexts()
            {
                for (int i = 0; i < this.innerContexts.Count; i++)
                {
                    this.innerContexts[i].Abort();
                    this.innerContexts[i].RequestMessage.Close();
                }
                this.innerContexts.Clear();
            }

            internal IAsyncResult BeginReplyInternal(Message reply, TimeSpan timeout, AsyncCallback callback, object state)
            {
                bool needAbort = true;
                bool needReply = true;

                try
                {
                    lock (this.ThisLock)
                    {
                        if (this.ranges == null)
                        {
                            throw Fx.AssertAndThrow("this.ranges != null");
                        }

                        if (this.Aborted)
                        {
                            needAbort = false;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(SR.GetString(SR.RequestContextAborted)));
                        }

                        if (this.outcomeKnown)
                        {
                            needAbort = false;
                            needReply = false;
                        }
                        else
                        {
                            if ((reply != null) && (this.bufferedReply == null))
                                this.bufferedReply = reply.CreateBufferedCopy(int.MaxValue);

                            if (!this.channel.PrepareReply(this))
                            {
                                needAbort = false;
                                needReply = false;
                            }
                            else
                            {
                                this.outcomeKnown = true;
                            }
                        }
                    }

                    if (!needReply)
                        return new ReplyCompletedAsyncResult(callback, state);

                    IAsyncResult result = new ReplyAsyncResult(this, timeout, callback, state);
                    needAbort = false;
                    return result;
                }
                finally
                {
                    if (needAbort)
                    {
                        this.AbortInnerContexts();
                        this.Abort();
                    }
                }
            }

            internal void EndReplyInternal(IAsyncResult result)
            {
                if (result is ReplyCompletedAsyncResult)
                {
                    ReplyCompletedAsyncResult.End(result);
                    return;
                }

                bool throwing = true;

                try
                {
                    ReplyAsyncResult.End(result);
                    this.innerContexts.Clear();
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        this.AbortInnerContexts();
                        this.Abort();
                    }
                }
            }

            protected override void OnAbort()
            {
                bool outcome;
                lock (this.ThisLock)
                {
                    outcome = this.outcomeKnown;
                    this.outcomeKnown = true;
                }

                if (!outcome)
                {
                    this.AbortInnerContexts();
                }

                if (this.channel.ContainsRequest(this.requestSequenceNumber))
                {
                    Exception e = new ProtocolException(SR.GetString(SR.ReliableRequestContextAborted));
                    this.channel.session.OnLocalFault(e, (Message)null, null);
                }
            }

            protected override IAsyncResult OnBeginReply(Message reply, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginReplyInternal(reply, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                // ReliableRequestContext.Close() relies on base.Close() to call reply if reply is not initiated. 
                if (!this.ReplyInitiated)
                    this.OnReply(null, timeout);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                this.EndReplyInternal(result);
            }

            protected override void OnReply(Message reply, TimeSpan timeout)
            {
                this.ReplyInternal(reply, timeout);
            }

            internal void ReplyInternal(Message reply, TimeSpan timeout)
            {
                bool needAbort = true;

                try
                {
                    lock (this.ThisLock)
                    {
                        if (this.ranges == null)
                        {
                            throw Fx.AssertAndThrow("this.ranges != null");
                        }

                        if (this.Aborted)
                        {
                            needAbort = false;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(SR.GetString(SR.RequestContextAborted)));
                        }

                        if (this.outcomeKnown)
                        {
                            needAbort = false;
                            return;
                        }

                        if ((reply != null) && (this.bufferedReply == null))
                            this.bufferedReply = reply.CreateBufferedCopy(int.MaxValue);

                        if (!this.channel.PrepareReply(this))
                        {
                            needAbort = false;
                            return;
                        }

                        this.outcomeKnown = true;
                    }

                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    for (int i = 0; i < this.innerContexts.Count; i++)
                        SendReply(this.innerContexts[i], MaskingMode.Handled, ref timeoutHelper);
                    this.innerContexts.Clear();
                    needAbort = false;
                }
                finally
                {
                    if (needAbort)
                    {
                        this.AbortInnerContexts();
                        this.Abort();
                    }
                }
            }

            public void SetAckRanges(SequenceRangeCollection ranges)
            {
                if (this.ranges == null)
                    this.ranges = ranges;
            }

            public void SetLastReply(Int64 sequenceNumber)
            {
                this.replySequenceNumber = sequenceNumber;
                this.isLastReply = true;
                if (this.bufferedReply == null)
                    this.bufferedReply = Message.CreateMessage(this.channel.MessageVersion, WsrmFeb2005Strings.LastMessageAction).CreateBufferedCopy(int.MaxValue);
            }

            public void SendReply(RequestContext context, MaskingMode maskingMode)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(this.DefaultSendTimeout);
                SendReply(context, maskingMode, ref timeoutHelper);
            }

            void SendReply(RequestContext context, MaskingMode maskingMode, ref TimeoutHelper timeoutHelper)
            {
                Message reply;

                if (!this.outcomeKnown)
                {
                    throw Fx.AssertAndThrow("this.outcomeKnown");
                }

                if (this.bufferedReply != null)
                {
                    reply = this.bufferedReply.CreateMessage();
                    this.channel.PrepareReplyMessage(this.replySequenceNumber, this.isLastReply, this.ranges, reply);
                }
                else
                {
                    reply = this.channel.CreateAcknowledgement(this.ranges);
                }
                this.channel.binder.SetMaskingMode(context, maskingMode);

                using (reply)
                {
                    context.Reply(reply, timeoutHelper.RemainingTime());
                }
                context.Close(timeoutHelper.RemainingTime());
            }

            public void SetReplySequenceNumber(Int64 sequenceNumber)
            {
                this.replySequenceNumber = sequenceNumber;
            }

            class ReplyCompletedAsyncResult : CompletedAsyncResult
            {
                public ReplyCompletedAsyncResult(AsyncCallback callback, object state)
                    : base(callback, state)
                {
                }
            }

            class ReplyAsyncResult : AsyncResult
            {
                ReliableRequestContext context;
                int currentContext;
                Message reply;
                TimeoutHelper timeoutHelper;
                static AsyncCallback replyCompleteStatic = Fx.ThunkCallback(new AsyncCallback(ReplyCompleteStatic));

                public ReplyAsyncResult(ReliableRequestContext thisContext, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.context = thisContext;
                    if (this.SendReplies())
                    {
                        this.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ReplyAsyncResult>(result);
                }

                void HandleReplyComplete(IAsyncResult result)
                {
                    RequestContext thisInnerContext = this.context.innerContexts[this.currentContext];

                    try
                    {
                        thisInnerContext.EndReply(result);
                        thisInnerContext.Close(this.timeoutHelper.RemainingTime());
                        this.currentContext++;
                    }
                    finally
                    {
                        this.reply.Close();
                        this.reply = null;
                    }
                }

                static void ReplyCompleteStatic(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    Exception ex = null;
                    ReplyAsyncResult thisPtr = null;
                    bool complete = false;

                    try
                    {
                        thisPtr = (ReplyAsyncResult)result.AsyncState;
                        thisPtr.HandleReplyComplete(result);
                        complete = thisPtr.SendReplies();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        ex = e;
                        complete = true;
                    }

                    if (complete)
                        thisPtr.Complete(false, ex);
                }

                bool SendReplies()
                {
                    while (this.currentContext < this.context.innerContexts.Count)
                    {
                        if (this.context.bufferedReply != null)
                        {
                            this.reply = this.context.bufferedReply.CreateMessage();
                            this.context.channel.PrepareReplyMessage(
                                this.context.replySequenceNumber, this.context.isLastReply,
                                this.context.ranges, this.reply);
                        }
                        else
                        {
                            this.reply = this.context.channel.CreateAcknowledgement(this.context.ranges);
                        }

                        RequestContext thisInnerContext = this.context.innerContexts[this.currentContext];
                        this.context.channel.binder.SetMaskingMode(thisInnerContext, MaskingMode.Handled);

                        IAsyncResult result = thisInnerContext.BeginReply(this.reply, this.timeoutHelper.RemainingTime(), replyCompleteStatic, this);

                        if (!result.CompletedSynchronously)
                            return false;

                        this.HandleReplyComplete(result);
                    }
                    return true;
                }
            }
        }

        class ReplyHelper
        {
            Message asyncMessage;
            bool canTransfer = true;
            ReliableReplySessionChannel channel;
            WsrmMessageInfo info;
            ReplyProvider replyProvider;
            RequestContext requestContext;
            bool throwTimeoutOnWait;
            InterruptibleWaitObject waitHandle;

            internal ReplyHelper(ReliableReplySessionChannel channel, ReplyProvider replyProvider,
                bool throwTimeoutOnWait)
            {
                this.channel = channel;
                this.replyProvider = replyProvider;
                this.throwTimeoutOnWait = throwTimeoutOnWait;
                this.waitHandle = new InterruptibleWaitObject(false, this.throwTimeoutOnWait);
            }

            object ThisLock
            {
                get { return this.channel.ThisLock; }
            }

            internal void Abort()
            {
                this.Cleanup(true);
            }

            void Cleanup(bool abort)
            {
                lock (this.ThisLock)
                {
                    this.canTransfer = false;
                }

                if (abort)
                {
                    this.waitHandle.Abort(this.channel);
                }
                else
                {
                    this.waitHandle.Fault(this.channel);
                }
            }

            internal void Fault()
            {
                this.Cleanup(false);
            }

            internal void Reply(RequestContext context, WsrmMessageInfo info, TimeSpan timeout, MaskingMode maskingMode)
            {
                using (Message message = this.replyProvider.Provide(this.channel, info))
                {
                    this.channel.binder.SetMaskingMode(context, maskingMode);
                    context.Reply(message, timeout);
                }
            }

            IAsyncResult BeginReply(TimeSpan timeout, AsyncCallback callback, object state)
            {
                lock (this.ThisLock)
                {
                    this.canTransfer = false;
                }

                if (this.requestContext == null)
                {
                    return new ReplyCompletedAsyncResult(callback, state);
                }

                this.asyncMessage = this.replyProvider.Provide(this.channel, info);
                bool throwing = true;

                try
                {
                    this.channel.binder.SetMaskingMode(this.requestContext, MaskingMode.Handled);
                    IAsyncResult result = this.requestContext.BeginReply(this.asyncMessage, timeout,
                        callback, state);
                    throwing = false;
                    return result;
                }
                finally
                {
                    if (throwing)
                    {
                        this.asyncMessage.Close();
                        this.asyncMessage = null;
                    }
                }
            }

            void EndReply(IAsyncResult result)
            {
                ReplyCompletedAsyncResult completedResult = result as ReplyCompletedAsyncResult;
                if (completedResult != null)
                {
                    completedResult.End();
                    return;
                }

                try
                {
                    this.requestContext.EndReply(result);
                }
                finally
                {
                    if (this.asyncMessage != null)
                    {
                        this.asyncMessage.Close();
                    }
                }
            }

            internal bool TransferRequestContext(RequestContext requestContext, WsrmMessageInfo info)
            {
                RequestContext oldContext = null;
                WsrmMessageInfo oldInfo = null;

                lock (this.ThisLock)
                {
                    if (!this.canTransfer)
                    {
                        return false;
                    }
                    else
                    {
                        oldContext = this.requestContext;
                        oldInfo = this.info;
                        this.requestContext = requestContext;
                        this.info = info;
                    }
                }

                this.waitHandle.Set();

                if (oldContext != null)
                {
                    oldInfo.Message.Close();
                    oldContext.Close();
                }

                return true;
            }

            internal void UnblockWaiter()
            {
                this.TransferRequestContext(null, null);
            }

            internal void WaitAndReply(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.waitHandle.Wait(timeoutHelper.RemainingTime());

                lock (this.ThisLock)
                {
                    this.canTransfer = false;

                    if (this.requestContext == null)
                    {
                        return;
                    }
                }

                this.Reply(this.requestContext, this.info, timeoutHelper.RemainingTime(),
                    MaskingMode.Handled);
            }

            internal IAsyncResult BeginWaitAndReply(TimeSpan timeout, AsyncCallback callback, object state)
            {
                OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] {
                    new OperationWithTimeoutBeginCallback (this.waitHandle.BeginWait),
                    new OperationWithTimeoutBeginCallback (this.BeginReply),
                };

                OperationEndCallback[] endOperations = new OperationEndCallback[] {
                    new OperationEndCallback (this.waitHandle.EndWait),
                    new OperationEndCallback(this.EndReply),
                };

                return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations,
                    endOperations, callback, state);
            }

            internal void EndWaitAndReply(IAsyncResult result)
            {
                OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            }

            class ReplyCompletedAsyncResult : CompletedAsyncResult
            {
                internal ReplyCompletedAsyncResult(AsyncCallback callback, object state)
                    : base(callback, state)
                {
                }

                public void End()
                {
                    AsyncResult.End<ReplyCompletedAsyncResult>(this);
                }
            }
        }

        abstract class ReplyProvider
        {
            internal abstract Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo info);
        }

        class CloseSequenceReplyProvider : ReplyProvider
        {
            static CloseSequenceReplyProvider instance = new CloseSequenceReplyProvider();

            CloseSequenceReplyProvider()
            {
            }

            static internal ReplyProvider Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new CloseSequenceReplyProvider();
                    }

                    return instance;
                }
            }

            internal override Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo requestInfo)
            {
                Message message = WsrmUtilities.CreateCloseSequenceResponse(channel.MessageVersion,
                   requestInfo.CloseSequenceInfo.MessageId, channel.session.InputID);
                channel.AddAcknowledgementHeader(message);
                return message;
            }
        }

        class TerminateSequenceReplyProvider : ReplyProvider
        {
            static TerminateSequenceReplyProvider instance = new TerminateSequenceReplyProvider();

            TerminateSequenceReplyProvider()
            {
            }

            static internal ReplyProvider Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TerminateSequenceReplyProvider();
                    }

                    return instance;
                }
            }

            internal override Message Provide(ReliableReplySessionChannel channel, WsrmMessageInfo requestInfo)
            {
                Message message = WsrmUtilities.CreateTerminateResponseMessage(channel.MessageVersion,
                   requestInfo.TerminateSequenceInfo.MessageId, channel.session.InputID);
                channel.AddAcknowledgementHeader(message);
                return message;
            }
        }
    }
}
