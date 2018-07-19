//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    abstract class ReliableInputSessionChannel : InputChannel, IInputSessionChannel
    {
        bool advertisedZero = false;
        IServerReliableChannelBinder binder;
        ReliableInputConnection connection;
        DeliveryStrategy<Message> deliveryStrategy;
        ReliableChannelListenerBase<IInputSessionChannel> listener;
        ServerReliableSession session;
        protected string perfCounterId;
        static Action<object> asyncReceiveComplete = new Action<object>(AsyncReceiveCompleteStatic);
        static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompletedStatic));

        protected ReliableInputSessionChannel(
            ReliableChannelListenerBase<IInputSessionChannel> listener,
            IServerReliableChannelBinder binder,
            FaultHelper faultHelper,
            UniqueId inputID)
            : base(listener, binder.LocalAddress)
        {
            this.binder = binder;
            this.listener = listener;
            this.connection = new ReliableInputConnection();
            this.connection.ReliableMessagingVersion = listener.ReliableMessagingVersion;
            this.session = new ServerReliableSession(this, listener, binder, faultHelper, inputID, null);
            this.session.UnblockChannelCloseCallback = this.UnblockClose;

            if (listener.Ordered)
                this.deliveryStrategy = new OrderedDeliveryStrategy<Message>(this, listener.MaxTransferWindowSize, false);
            else
                this.deliveryStrategy = new UnorderedDeliveryStrategy<Message>(this, listener.MaxTransferWindowSize);

            this.binder.Faulted += OnBinderFaulted;
            this.binder.OnException += OnBinderException;
            this.session.Open(TimeSpan.Zero);

            if (PerformanceCounters.PerformanceCountersEnabled)
                this.perfCounterId = this.listener.Uri.ToString().ToUpperInvariant();
        }

        protected bool AdvertisedZero
        {
            get
            {
                return this.advertisedZero;
            }
            set
            {
                this.advertisedZero = value;
            }
        }

        public IServerReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        protected ReliableInputConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected DeliveryStrategy<Message> DeliveryStrategy
        {
            get
            {
                return this.deliveryStrategy;
            }
        }

        protected ReliableChannelListenerBase<IInputSessionChannel> Listener
        {
            get
            {
                return this.listener;
            }
        }

        protected ChannelReliableSession ReliableSession
        {
            get
            {
                return this.session;
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }

        protected virtual void AggregateAsyncCloseOperations(List<OperationWithTimeoutBeginCallback> beginOperations, List<OperationEndCallback> endOperations)
        {
            beginOperations.Add(new OperationWithTimeoutBeginCallback(this.session.BeginClose));
            endOperations.Add(new OperationEndCallback(this.session.EndClose));
        }

        static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult)state;
            ReliableInputSessionChannel channel = (ReliableInputSessionChannel)(result.AsyncState);

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
                    throw;

                channel.ReliableSession.OnUnknownException(e);
            }
        }

        static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;

            ReliableInputSessionChannel channel = (ReliableInputSessionChannel)(result.AsyncState);
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
                    throw;

                channel.ReliableSession.OnUnknownException(e);
            }
        }

        protected abstract bool HandleReceiveComplete(IAsyncResult result);

        protected virtual void AbortGuards()
        {
        }

        protected void AddAcknowledgementHeader(Message message)
        {
            int bufferRemaining = -1;

            if (this.Listener.FlowControlEnabled)
            {
                bufferRemaining = this.Listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.AdvertisedZero = (bufferRemaining == 0);
            }

            WsrmUtilities.AddAcknowledgementHeader(this.listener.ReliableMessagingVersion, message,
                this.session.InputID, this.connection.Ranges, this.connection.IsLastKnown, bufferRemaining);
        }

        IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        protected virtual IAsyncResult BeginCloseGuards(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(this.ReliableSession.InputID, null,
                timeout, callback, state);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.binder.Faulted -= this.OnBinderFaulted;
            this.deliveryStrategy.Dispose();
        }

        protected virtual void CloseGuards(TimeSpan timeout)
        {
        }

        protected Message CreateAcknowledgmentMessage()
        {
            int bufferRemaining = -1;

            if (this.Listener.FlowControlEnabled)
            {
                bufferRemaining = this.Listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.AdvertisedZero = (bufferRemaining == 0);
            }

            Message message = WsrmUtilities.CreateAcknowledgmentMessage(
                this.listener.MessageVersion,
                this.listener.ReliableMessagingVersion,
                this.session.InputID,
                this.connection.Ranges,
                this.connection.IsLastKnown,
                bufferRemaining);

            return message;
        }

        void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        protected virtual void EndCloseGuards(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IInputSessionChannel))
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

        protected override void OnAbort()
        {
            this.connection.Abort(this);
            this.AbortGuards();
            this.session.Abort();
            this.listener.OnReliableChannelAbort(this.ReliableSession.InputID, null);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfCloseInvalid();

            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] 
                { 
                    this.connection.BeginClose,
                    this.session.BeginClose, 
                    this.BeginCloseGuards, 
                    this.BeginCloseBinder,
                    this.BeginUnregisterChannel, 
                    base.OnBeginClose
                };

            OperationEndCallback[] endOperations = new OperationEndCallback[] 
                { 
                    this.connection.EndClose,
                    this.session.EndClose, 
                    this.EndCloseGuards, 
                    this.EndCloseBinder,
                    this.EndUnregisterChannel, 
                    base.OnEndClose 
                };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                beginOperations, endOperations, callback, state);
        }

        void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
                this.session.OnLocalFault(exception, SequenceTerminatedFault.CreateQuotaExceededFault(this.session.OutputID), null);
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

            this.connection.Close(timeoutHelper.RemainingTime());
            this.session.Close(timeoutHelper.RemainingTime());
            this.CloseGuards(timeoutHelper.RemainingTime());
            this.binder.Close(timeoutHelper.RemainingTime(), MaskingMode.Handled);
            this.listener.OnReliableChannelClose(this.ReliableSession.InputID, null,
                timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
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

        protected virtual void OnQuotaAvailable()
        {
        }

        protected void ShutdownCallback(object state)
        {
            this.Shutdown();
        }

        protected void StartReceiving(bool canBlock)
        {
            while (true)
            {
                IAsyncResult result = this.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
                if (!result.CompletedSynchronously)
                    return;

                if (!canBlock)
                {
                    ActionItem.Schedule(asyncReceiveComplete, result);
                    return;
                }

                if (!this.HandleReceiveComplete(result))
                    break;
            }
        }

        void ThrowIfCloseInvalid()
        {
            bool shouldFault = false;

            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.DeliveryStrategy.EnqueuedCount > 0 || this.Connection.Ranges.Count > 1)
                {
                    shouldFault = true;
                }
            }
            else if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.DeliveryStrategy.EnqueuedCount > 0)
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
            this.connection.Fault(this);
        }
    }

    sealed class ReliableInputSessionChannelOverDuplex : ReliableInputSessionChannel
    {
        TimeSpan acknowledgementInterval;
        bool acknowledgementScheduled = false;
        IOThreadTimer acknowledgementTimer;
        Guard guard = new Guard(Int32.MaxValue);
        int pendingAcknowledgements = 0;

        public ReliableInputSessionChannelOverDuplex(
            ReliableChannelListenerBase<IInputSessionChannel> listener,
            IServerReliableChannelBinder binder, FaultHelper faultHelper,
            UniqueId inputID)
            : base(listener, binder, faultHelper, inputID)
        {
            this.acknowledgementInterval = listener.AcknowledgementInterval;
            this.acknowledgementTimer = new IOThreadTimer(new Action<object>(this.OnAcknowledgementTimeoutElapsed), null, true);
            this.DeliveryStrategy.DequeueCallback = this.OnDeliveryStrategyItemDequeued;

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

                    this.ReliableSession.OnUnknownException(e);
                }
            }
        }

        protected override void AbortGuards()
        {
            this.guard.Abort();
        }

        protected override IAsyncResult BeginCloseGuards(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.guard.BeginClose(timeout, callback, state);
        }

        protected override void CloseGuards(TimeSpan timeout)
        {
            this.guard.Close(timeout);
        }

        protected override void EndCloseGuards(IAsyncResult result)
        {
            this.guard.EndClose(result);
        }

        protected override bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;
            if (this.Binder.EndTryReceive(result, out context))
            {
                if (context == null)
                {
                    bool terminated = false;

                    lock (this.ThisLock)
                    {
                        terminated = this.Connection.Terminate();
                    }

                    if (!terminated && (this.Binder.State == CommunicationState.Opened))
                    {
                        Exception e = new CommunicationException(SR.GetString(SR.EarlySecurityClose));
                        this.ReliableSession.OnLocalFault(e, (Message)null, null);
                    }
                    return false;
                }

                Message message = context.RequestMessage;
                context.Close();

                WsrmMessageInfo info = WsrmMessageInfo.Get(this.Listener.MessageVersion,
                    this.Listener.ReliableMessagingVersion, this.Binder.Channel, this.Binder.GetInnerSession(),
                    message);

                this.StartReceiving(false);
                this.ProcessMessage(info);
                return false;
            }
            return true;
        }

        void OnAcknowledgementTimeoutElapsed(object state)
        {
            lock (this.ThisLock)
            {
                this.acknowledgementScheduled = false;
                this.pendingAcknowledgements = 0;

                if (this.State == CommunicationState.Closing
                    || this.State == CommunicationState.Closed
                    || this.State == CommunicationState.Faulted)
                    return;
            }

            if (this.guard.Enter())
            {
                try
                {
                    using (Message message = CreateAcknowledgmentMessage())
                    {
                        this.Binder.Send(message, this.DefaultSendTimeout);
                    }
                }

                finally
                {
                    this.guard.Exit();
                }
            }
        }

        void OnDeliveryStrategyItemDequeued()
        {
            if (this.AdvertisedZero)
                this.OnAcknowledgementTimeoutElapsed(null);
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.acknowledgementTimer.Cancel();
        }

        protected override void OnQuotaAvailable()
        {
            this.OnAcknowledgementTimeoutElapsed(null);
        }

        public void ProcessDemuxedMessage(WsrmMessageInfo info)
        {
            try
            {
                this.ProcessMessage(info);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.ReliableSession.OnUnknownException(e);
            }
        }

        void ProcessMessage(WsrmMessageInfo info)
        {
            bool closeMessage = true;

            try
            {
                if (!this.ReliableSession.ProcessInfo(info, null))
                {
                    closeMessage = false;
                    return;
                }

                if (!this.ReliableSession.VerifySimplexProtocolElements(info, null))
                {
                    closeMessage = false;
                    return;
                }

                this.ReliableSession.OnRemoteActivity(false);

                if (info.CreateSequenceInfo != null)
                {
                    EndpointAddress acksTo;

                    if (WsrmUtilities.ValidateCreateSequence<IInputSessionChannel>(info, this.Listener, this.Binder.Channel, out acksTo))
                    {
                        Message response = WsrmUtilities.CreateCreateSequenceResponse(this.Listener.MessageVersion,
                            this.Listener.ReliableMessagingVersion, false, info.CreateSequenceInfo,
                            this.Listener.Ordered, this.ReliableSession.InputID, acksTo);

                        using (response)
                        {
                            if (this.Binder.AddressResponse(info.Message, response))
                                this.Binder.Send(response, this.DefaultSendTimeout);
                        }
                    }
                    else
                    {
                        this.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, null);
                    }

                    return;
                }

                bool needDispatch = false;
                bool scheduleShutdown = false;
                bool tryAckNow = (info.AckRequestedInfo != null);
                bool terminate = false;
                Message message = null;
                WsrmFault fault = null;
                Exception remoteFaultException = null;
                bool wsrmFeb2005 = this.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool wsrm11 = this.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

                if (info.SequencedMessageInfo != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.Aborted || this.State == CommunicationState.Faulted)
                        {
                            return;
                        }

                        Int64 sequenceNumber = info.SequencedMessageInfo.SequenceNumber;
                        bool isLast = wsrmFeb2005 && info.SequencedMessageInfo.LastMessage;

                        if (!this.Connection.IsValid(sequenceNumber, isLast))
                        {
                            if (wsrmFeb2005)
                            {
                                fault = new LastMessageNumberExceededFault(this.ReliableSession.InputID);
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.ReliableSession.InputID).CreateMessage(
                                    this.Listener.MessageVersion, this.Listener.ReliableMessagingVersion);
                                tryAckNow = true;

                                if (PerformanceCounters.PerformanceCountersEnabled)
                                    PerformanceCounters.MessageDropped(this.perfCounterId);
                            }
                        }
                        else if (this.Connection.Ranges.Contains(sequenceNumber))
                        {
                            if (PerformanceCounters.PerformanceCountersEnabled)
                                PerformanceCounters.MessageDropped(this.perfCounterId);

                            tryAckNow = true;
                        }
                        else if (wsrmFeb2005 && info.Action == WsrmFeb2005Strings.LastMessageAction)
                        {
                            this.Connection.Merge(sequenceNumber, isLast);

                            if (this.Connection.AllAdded)
                            {
                                scheduleShutdown = true;
                                this.ReliableSession.CloseSession();
                            }
                        }
                        else if (this.State == CommunicationState.Closing)
                        {
                            if (wsrmFeb2005)
                            {
                                fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                                    SR.GetString(SR.SequenceTerminatedSessionClosedBeforeDone),
                                    SR.GetString(SR.SessionClosedBeforeDone));
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.ReliableSession.InputID).CreateMessage(
                                    this.Listener.MessageVersion, this.Listener.ReliableMessagingVersion);
                                tryAckNow = true;

                                if (PerformanceCounters.PerformanceCountersEnabled)
                                    PerformanceCounters.MessageDropped(this.perfCounterId);
                            }
                        }
                        // In the unordered case we accept no more than MaxSequenceRanges ranges to limit the
                        // serialized ack size and the amount of memory taken by the ack ranges. In the
                        // ordered case, the delivery strategy MaxTransferWindowSize quota mitigates this
                        // threat.
                        else if (this.DeliveryStrategy.CanEnqueue(sequenceNumber)
                            && (this.Listener.Ordered || this.Connection.CanMerge(sequenceNumber)))
                        {
                            this.Connection.Merge(sequenceNumber, isLast);
                            needDispatch = this.DeliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            closeMessage = false;

                            this.pendingAcknowledgements++;
                            if (this.pendingAcknowledgements == this.Listener.MaxTransferWindowSize)
                                tryAckNow = true;

                            if (this.Connection.AllAdded)
                            {
                                scheduleShutdown = true;
                                this.ReliableSession.CloseSession();
                            }
                        }
                        else
                        {
                            if (PerformanceCounters.PerformanceCountersEnabled)
                                PerformanceCounters.MessageDropped(this.perfCounterId);
                        }

                        if (this.Connection.IsLastKnown)
                            tryAckNow = true;

                        if (!tryAckNow && this.pendingAcknowledgements > 0 && !this.acknowledgementScheduled && fault == null)
                        {
                            this.acknowledgementScheduled = true;
                            this.acknowledgementTimer.Set(this.acknowledgementInterval);
                        }
                    }
                }
                else if (wsrmFeb2005 && info.TerminateSequenceInfo != null)
                {
                    bool isTerminateEarly;

                    lock (this.ThisLock)
                    {
                        isTerminateEarly = !this.Connection.Terminate();
                    }

                    if (isTerminateEarly)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedEarlyTerminateSequence),
                            SR.GetString(SR.EarlyTerminateSequence));
                    }
                }
                else if (wsrm11 && ((info.TerminateSequenceInfo != null) || info.CloseSequenceInfo != null))
                {
                    bool isTerminate = info.TerminateSequenceInfo != null;
                    WsrmRequestInfo requestInfo = isTerminate
                        ? (WsrmRequestInfo)info.TerminateSequenceInfo
                        : (WsrmRequestInfo)info.CloseSequenceInfo;
                    Int64 last = isTerminate ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;

                    if (!WsrmUtilities.ValidateWsrmRequest(this.ReliableSession, requestInfo, this.Binder, null))
                    {
                        return;
                    }

                    bool isLastLargeEnough = true;
                    bool isLastConsistent = true;

                    lock (this.ThisLock)
                    {
                        if (!this.Connection.IsLastKnown)
                        {
                            if (isTerminate)
                            {
                                if (this.Connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
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
                                scheduleShutdown = this.Connection.SetCloseSequenceLast(last);
                                isLastLargeEnough = scheduleShutdown;
                            }

                            if (scheduleShutdown)
                            {
                                this.ReliableSession.SetFinalAck(this.Connection.Ranges);
                                this.DeliveryStrategy.Dispose();
                            }
                        }
                        else
                        {
                            isLastConsistent = (last == this.Connection.Last);

                            // Have seen CloseSequence already, TerminateSequence means cleanup.
                            if (isTerminate && isLastConsistent && this.Connection.IsSequenceClosed)
                            {
                                terminate = true;
                            }
                        }
                    }

                    if (!isLastLargeEnough)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedSmallLastMsgNumber),
                            SR.GetString(SR.SmallLastMsgNumberExceptionString));
                    }
                    else if (!isLastConsistent)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedInconsistentLastMsgNumber),
                            SR.GetString(SR.InconsistentLastMsgNumberExceptionString));
                    }
                    else
                    {
                        message = isTerminate
                            ? WsrmUtilities.CreateTerminateResponseMessage(this.Listener.MessageVersion,
                            requestInfo.MessageId, this.ReliableSession.InputID)
                            : WsrmUtilities.CreateCloseSequenceResponse(this.Listener.MessageVersion,
                            requestInfo.MessageId, this.ReliableSession.InputID);

                        tryAckNow = true;
                    }
                }

                if (fault != null)
                {
                    this.ReliableSession.OnLocalFault(fault.CreateException(), fault, null);
                }
                else
                {
                    if (tryAckNow)
                    {
                        lock (this.ThisLock)
                        {
                            if (this.acknowledgementScheduled)
                            {
                                this.acknowledgementTimer.Cancel();
                                this.acknowledgementScheduled = false;
                            }

                            this.pendingAcknowledgements = 0;
                        }

                        if (message != null)
                        {
                            this.AddAcknowledgementHeader(message);
                        }
                        else
                        {
                            message = this.CreateAcknowledgmentMessage();
                        }
                    }

                    if (message != null)
                    {
                        using (message)
                        {
                            if (this.guard.Enter())
                            {
                                try
                                {
                                    this.Binder.Send(message, this.DefaultSendTimeout);
                                }
                                finally
                                {
                                    this.guard.Exit();
                                }
                            }
                        }
                    }

                    if (terminate)
                    {
                        lock (this.ThisLock)
                        {
                            this.Connection.Terminate();
                        }
                    }

                    if (remoteFaultException != null)
                    {
                        this.ReliableSession.OnRemoteFault(remoteFaultException);
                        return;
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
            }
            finally
            {
                if (closeMessage)
                {
                    info.Message.Close();
                }
            }
        }
    }

    sealed class ReliableInputSessionChannelOverReply : ReliableInputSessionChannel
    {
        public ReliableInputSessionChannelOverReply(
            ReliableChannelListenerBase<IInputSessionChannel> listener,
            IServerReliableChannelBinder binder, FaultHelper faultHelper,
            UniqueId inputID)
            : base(listener, binder, faultHelper, inputID)
        {
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

                    this.ReliableSession.OnUnknownException(e);
                }
            }
        }

        protected override bool HandleReceiveComplete(IAsyncResult result)
        {
            RequestContext context;
            bool timeoutOkay = this.Binder.EndTryReceive(result, out context);

            if (timeoutOkay)
            {
                if (context == null)
                {
                    bool terminated = false;

                    lock (this.ThisLock)
                    {
                        terminated = this.Connection.Terminate();
                    }

                    if (!terminated && (this.Binder.State == CommunicationState.Opened))
                    {
                        Exception e = new CommunicationException(SR.GetString(SR.EarlySecurityClose));
                        this.ReliableSession.OnLocalFault(e, (Message)null, null);
                    }
                    return false;
                }

                WsrmMessageInfo info = WsrmMessageInfo.Get(this.Listener.MessageVersion,
                    this.Listener.ReliableMessagingVersion, this.Binder.Channel, this.Binder.GetInnerSession(),
                    context.RequestMessage);

                this.StartReceiving(false);
                this.ProcessRequest(context, info);
                return false;
            }
            return true;
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

                this.ReliableSession.OnUnknownException(e);
            }
        }

        void ProcessRequest(RequestContext context, WsrmMessageInfo info)
        {
            bool closeContext = true;
            bool closeMessage = true;

            try
            {
                if (!this.ReliableSession.ProcessInfo(info, context))
                {
                    closeContext = false;
                    closeMessage = false;
                    return;
                }

                if (!this.ReliableSession.VerifySimplexProtocolElements(info, context))
                {
                    closeContext = false;
                    closeMessage = false;
                    return;
                }

                this.ReliableSession.OnRemoteActivity(false);

                if (info.CreateSequenceInfo != null)
                {
                    EndpointAddress acksTo;

                    if (WsrmUtilities.ValidateCreateSequence<IInputSessionChannel>(info, this.Listener, this.Binder.Channel, out acksTo))
                    {
                        Message response = WsrmUtilities.CreateCreateSequenceResponse(this.Listener.MessageVersion,
                            this.Listener.ReliableMessagingVersion, false, info.CreateSequenceInfo,
                            this.Listener.Ordered, this.ReliableSession.InputID, acksTo);

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
                        this.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, context);
                    }

                    closeContext = false;
                    closeMessage = false;
                    return;
                }

                bool needDispatch = false;
                bool scheduleShutdown = false;
                bool terminate = false;
                WsrmFault fault = null;
                Message message = null;
                Exception remoteFaultException = null;
                bool wsrmFeb2005 = this.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool wsrm11 = this.Listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool addAck = info.AckRequestedInfo != null;

                if (info.SequencedMessageInfo != null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.Aborted || (this.State == CommunicationState.Faulted))
                        {
                            return;
                        }
                        Int64 sequenceNumber = info.SequencedMessageInfo.SequenceNumber;
                        bool isLast = wsrmFeb2005 && info.SequencedMessageInfo.LastMessage;

                        if (!this.Connection.IsValid(sequenceNumber, isLast))
                        {
                            if (wsrmFeb2005)
                            {
                                fault = new LastMessageNumberExceededFault(this.ReliableSession.InputID);
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.ReliableSession.InputID).CreateMessage(
                                    this.Listener.MessageVersion, this.Listener.ReliableMessagingVersion);

                                if (PerformanceCounters.PerformanceCountersEnabled)
                                    PerformanceCounters.MessageDropped(this.perfCounterId);
                            }
                        }
                        else if (this.Connection.Ranges.Contains(sequenceNumber))
                        {
                            if (PerformanceCounters.PerformanceCountersEnabled)
                                PerformanceCounters.MessageDropped(this.perfCounterId);
                        }
                        else if (wsrmFeb2005 && info.Action == WsrmFeb2005Strings.LastMessageAction)
                        {
                            this.Connection.Merge(sequenceNumber, isLast);
                            scheduleShutdown = this.Connection.AllAdded;
                        }
                        else if (this.State == CommunicationState.Closing)
                        {
                            if (wsrmFeb2005)
                            {
                                fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                                    SR.GetString(SR.SequenceTerminatedSessionClosedBeforeDone),
                                    SR.GetString(SR.SessionClosedBeforeDone));
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.ReliableSession.InputID).CreateMessage(
                                    this.Listener.MessageVersion, this.Listener.ReliableMessagingVersion);

                                if (PerformanceCounters.PerformanceCountersEnabled)
                                    PerformanceCounters.MessageDropped(this.perfCounterId);
                            }
                        }
                        // In the unordered case we accept no more than MaxSequenceRanges ranges to limit the
                        // serialized ack size and the amount of memory taken by the ack ranges. In the
                        // ordered case, the delivery strategy MaxTransferWindowSize quota mitigates this
                        // threat.
                        else if (this.DeliveryStrategy.CanEnqueue(sequenceNumber)
                            && (this.Listener.Ordered || this.Connection.CanMerge(sequenceNumber)))
                        {
                            this.Connection.Merge(sequenceNumber, isLast);
                            needDispatch = this.DeliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            scheduleShutdown = this.Connection.AllAdded;
                            closeMessage = false;
                        }
                        else
                        {
                            if (PerformanceCounters.PerformanceCountersEnabled)
                                PerformanceCounters.MessageDropped(this.perfCounterId);
                        }
                    }
                }
                else if (wsrmFeb2005 && info.TerminateSequenceInfo != null)
                {
                    bool isTerminateEarly;

                    lock (this.ThisLock)
                    {
                        isTerminateEarly = !this.Connection.Terminate();
                    }

                    if (isTerminateEarly)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedEarlyTerminateSequence),
                            SR.GetString(SR.EarlyTerminateSequence));
                    }
                    else
                    {
                        // In the normal case, TerminateSequence is a one-way operation, returning (the finally
                        // block will close the context).
                        return;
                    }
                }
                else if (wsrm11 && ((info.TerminateSequenceInfo != null) || (info.CloseSequenceInfo != null)))
                {
                    bool isTerminate = (info.TerminateSequenceInfo != null);
                    WsrmRequestInfo requestInfo = isTerminate
                        ? (WsrmRequestInfo)info.TerminateSequenceInfo
                        : (WsrmRequestInfo)info.CloseSequenceInfo;
                    Int64 last = isTerminate ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;

                    if (!WsrmUtilities.ValidateWsrmRequest(this.ReliableSession, requestInfo, this.Binder, context))
                    {
                        closeMessage = false;
                        closeContext = false;
                        return;
                    }

                    bool isLastLargeEnough = true;
                    bool isLastConsistent = true;

                    lock (this.ThisLock)
                    {
                        if (!this.Connection.IsLastKnown)
                        {
                            if (isTerminate)
                            {
                                if (this.Connection.SetTerminateSequenceLast(last, out isLastLargeEnough))
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
                                scheduleShutdown = this.Connection.SetCloseSequenceLast(last);
                                isLastLargeEnough = scheduleShutdown;
                            }

                            if (scheduleShutdown)
                            {
                                this.ReliableSession.SetFinalAck(this.Connection.Ranges);
                                this.DeliveryStrategy.Dispose();
                            }
                        }
                        else
                        {
                            isLastConsistent = (last == this.Connection.Last);

                            // Have seen CloseSequence already, TerminateSequence means cleanup.
                            if (isTerminate && isLastConsistent && this.Connection.IsSequenceClosed)
                            {
                                terminate = true;
                            }
                        }
                    }

                    if (!isLastLargeEnough)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedSmallLastMsgNumber),
                            SR.GetString(SR.SmallLastMsgNumberExceptionString));
                    }
                    else if (!isLastConsistent)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.InputID,
                            SR.GetString(SR.SequenceTerminatedInconsistentLastMsgNumber),
                            SR.GetString(SR.InconsistentLastMsgNumberExceptionString));
                    }
                    else
                    {
                        message = isTerminate
                            ? WsrmUtilities.CreateTerminateResponseMessage(this.Listener.MessageVersion,
                            requestInfo.MessageId, this.ReliableSession.InputID)
                            : WsrmUtilities.CreateCloseSequenceResponse(this.Listener.MessageVersion,
                            requestInfo.MessageId, this.ReliableSession.InputID);
                        addAck = true;
                    }
                }

                if (fault != null)
                {
                    this.ReliableSession.OnLocalFault(fault.CreateException(), fault, context);
                    closeMessage = false;
                    closeContext = false;
                    return;
                }

                if (message != null && addAck)
                {
                    this.AddAcknowledgementHeader(message);
                }
                else if (message == null)
                {
                    message = this.CreateAcknowledgmentMessage();
                }

                using (message)
                {
                    context.Reply(message);
                }

                if (terminate)
                {
                    lock (this.ThisLock)
                    {
                        this.Connection.Terminate();
                    }
                }

                if (remoteFaultException != null)
                {
                    this.ReliableSession.OnRemoteFault(remoteFaultException);
                    return;
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
            finally
            {
                if (closeMessage)
                    info.Message.Close();

                if (closeContext)
                    context.Close();
            }
        }
    }
}
