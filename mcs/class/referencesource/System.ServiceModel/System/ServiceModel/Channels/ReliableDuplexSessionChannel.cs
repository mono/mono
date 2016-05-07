//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Xml;

    abstract class ReliableDuplexSessionChannel : DuplexChannel, IDuplexSessionChannel
    {
        bool acknowledgementScheduled = false;
        IOThreadTimer acknowledgementTimer;
        UInt64 ackVersion = 1;
        bool advertisedZero = false;
        IReliableChannelBinder binder;
        InterruptibleWaitObject closeOutputWaitObject;
        SendWaitReliableRequestor closeRequestor;
        DeliveryStrategy<Message> deliveryStrategy;
        Guard guard = new Guard(Int32.MaxValue);
        ReliableInputConnection inputConnection;
        Exception maxRetryCountException = null;
        static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompletedStatic));
        ReliableOutputConnection outputConnection;
        int pendingAcknowledgements = 0;
        ChannelReliableSession session;
        IReliableFactorySettings settings;
        SendWaitReliableRequestor terminateRequestor;
        static Action<object> asyncReceiveComplete = new Action<object>(AsyncReceiveCompleteStatic);

        protected ReliableDuplexSessionChannel(ChannelManagerBase manager, IReliableFactorySettings settings, IReliableChannelBinder binder)
            : base(manager, binder.LocalAddress)
        {
            this.binder = binder;
            this.settings = settings;
            this.acknowledgementTimer = new IOThreadTimer(new Action<object>(this.OnAcknowledgementTimeoutElapsed), null, true);
            this.binder.Faulted += OnBinderFaulted;
            this.binder.OnException += OnBinderException;
        }

        public IReliableChannelBinder Binder
        {
            get { return this.binder; }
        }

        public override EndpointAddress LocalAddress
        {
            get { return this.binder.LocalAddress; }
        }

        protected ReliableOutputConnection OutputConnection
        {
            get { return this.outputConnection; }
        }

        protected UniqueId OutputID
        {
            get { return this.session.OutputID; }
        }

        protected ChannelReliableSession ReliableSession
        {
            get { return this.session; }
        }

        public override EndpointAddress RemoteAddress
        {
            get { return this.binder.RemoteAddress; }
        }

        protected IReliableFactorySettings Settings
        {
            get { return this.settings; }
        }

        public override Uri Via
        {
            get { return this.RemoteAddress.Uri; }
        }

        public IDuplexSession Session
        {
            get { return (IDuplexSession)this.session; }
        }

        void AddPendingAcknowledgements(Message message)
        {
            lock (this.ThisLock)
            {
                if (this.pendingAcknowledgements > 0)
                {
                    this.acknowledgementTimer.Cancel();
                    this.acknowledgementScheduled = false;
                    this.pendingAcknowledgements = 0;
                    this.ackVersion++;

                    int bufferRemaining = this.GetBufferRemaining();

                    WsrmUtilities.AddAcknowledgementHeader(
                        this.settings.ReliableMessagingVersion,
                        message,
                        this.session.InputID,
                        this.inputConnection.Ranges,
                        this.inputConnection.IsLastKnown,
                        bufferRemaining);
                }
            }
        }

        IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        void CloseSequence(TimeSpan timeout)
        {
            this.CreateCloseRequestor();
            this.closeRequestor.Request(timeout);
            // reply came from receive loop, receive loop owns verified message so nothing more to do.
        }

        IAsyncResult BeginCloseSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateCloseRequestor();
            return this.closeRequestor.BeginRequest(timeout, callback, state);
        }

        void EndCloseSequence(IAsyncResult result)
        {
            this.closeRequestor.EndRequest(result);
            // reply came from receive loop, receive loop owns verified message so nothing more to do.
        }

        void ConfigureRequestor(ReliableRequestor requestor)
        {
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
        }

        Message CreateAcknowledgmentMessage()
        {
            lock (this.ThisLock)
                this.ackVersion++;

            int bufferRemaining = this.GetBufferRemaining();

            Message message = WsrmUtilities.CreateAcknowledgmentMessage(this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.session.InputID, this.inputConnection.Ranges,
                this.inputConnection.IsLastKnown, bufferRemaining);

            if (TD.SequenceAcknowledgementSentIsEnabled())
            {
                TD.SequenceAcknowledgementSent(this.session.Id);
            }

            return message;
        }

        void CreateCloseRequestor()
        {
            SendWaitReliableRequestor temp = new SendWaitReliableRequestor();

            this.ConfigureRequestor(temp);
            temp.TimeoutString1Index = SR.TimeoutOnClose;
            temp.MessageAction = WsrmIndex.GetCloseSequenceActionHeader(
                this.settings.MessageVersion.Addressing);
            temp.MessageBody = new CloseSequence(this.session.OutputID, this.outputConnection.Last);

            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.closeRequestor = temp;
            }
        }

        void CreateTerminateRequestor()
        {
            SendWaitReliableRequestor temp = new SendWaitReliableRequestor();

            this.ConfigureRequestor(temp);
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            temp.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(
                this.settings.MessageVersion.Addressing, reliableMessagingVersion);
            temp.MessageBody = new TerminateSequence(reliableMessagingVersion, this.session.OutputID,
                this.outputConnection.Last);

            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.terminateRequestor = temp;

                if (this.inputConnection.IsLastKnown)
                {
                    this.session.CloseSession();
                }
            }
        }

        void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        int GetBufferRemaining()
        {
            int bufferRemaining = -1;

            if (this.settings.FlowControlEnabled)
            {
                bufferRemaining = this.settings.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.advertisedZero = (bufferRemaining == 0);
            }

            return bufferRemaining;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IDuplexSessionChannel))
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
                return (T)(object)FaultConverter.GetDefaultFaultConverter(this.settings.MessageVersion);
            }
            else
            {
                return innerProperty;
            }
        }

        void InternalCloseOutputSession(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.outputConnection.Close(timeoutHelper.RemainingTime());

            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CloseSequence(timeoutHelper.RemainingTime());
            }

            this.TerminateSequence(timeoutHelper.RemainingTime());
        }

        IAsyncResult BeginInternalCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] {
                this.outputConnection.BeginClose,
                wsrm11 ? this.BeginCloseSequence : default(OperationWithTimeoutBeginCallback),
                this.BeginTerminateSequence };

            OperationEndCallback[] endOperations = new OperationEndCallback[] {
                this.outputConnection.EndClose,
                wsrm11 ? this.EndCloseSequence : default(OperationEndCallback),
                this.EndTerminateSequence };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                beginOperations, endOperations, callback, state);
        }

        void EndInternalCloseOutputSession(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected virtual void OnRemoteActivity()
        {
            this.session.OnRemoteActivity(false);
        }

        WsrmFault ProcessCloseOrTerminateSequenceResponse(bool close, WsrmMessageInfo info)
        {
            SendWaitReliableRequestor requestor = close ? this.closeRequestor : this.terminateRequestor;

            if (requestor != null)
            {
                WsrmFault fault = close
                    ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, this.closeRequestor.MessageId, info,
                    this.outputConnection.Last)
                    : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, this.terminateRequestor.MessageId,
                    info, this.outputConnection.Last);

                if (fault != null)
                {
                    return fault;
                }

                requestor.SetInfo(info);
                return null;
            }

            string request = close ? Wsrm11Strings.CloseSequence : WsrmFeb2005Strings.TerminateSequence;
            string faultString = SR.GetString(SR.ReceivedResponseBeforeRequestFaultString, request);
            string exceptionString = SR.GetString(SR.ReceivedResponseBeforeRequestExceptionString, request);
            return SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultString, exceptionString);
        }

        protected void ProcessDuplexMessage(WsrmMessageInfo info)
        {
            bool closeMessage = true;

            try
            {
                bool wsrmFeb2005 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool final = false;

                if (this.outputConnection != null && info.AcknowledgementInfo != null)
                {
                    final = wsrm11 && info.AcknowledgementInfo.Final;

                    int bufferRemaining = -1;

                    if (this.settings.FlowControlEnabled)
                        bufferRemaining = info.AcknowledgementInfo.BufferRemaining;

                    this.outputConnection.ProcessTransferred(info.AcknowledgementInfo.Ranges, bufferRemaining);
                }

                this.OnRemoteActivity();

                bool tryAckNow = (info.AckRequestedInfo != null);
                bool forceAck = false;
                bool terminate = false;
                bool scheduleShutdown = false;
                UInt64 oldAckVersion = 0;
                WsrmFault fault = null;
                Message message = null;
                Exception remoteFaultException = null;

                if (info.SequencedMessageInfo != null)
                {
                    bool needDispatch = false;

                    lock (this.ThisLock)
                    {
                        if (this.Aborted || this.State == CommunicationState.Faulted)
                        {
                            return;
                        }

                        Int64 sequenceNumber = info.SequencedMessageInfo.SequenceNumber;
                        bool isLast = wsrmFeb2005 && info.SequencedMessageInfo.LastMessage;

                        if (!this.inputConnection.IsValid(sequenceNumber, isLast))
                        {
                            if (wsrmFeb2005)
                            {
                                fault = new LastMessageNumberExceededFault(this.ReliableSession.InputID);
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.session.InputID).CreateMessage(
                                    this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
                                forceAck = true;

                                this.OnMessageDropped();
                            }
                        }
                        else if (this.inputConnection.Ranges.Contains(sequenceNumber))
                        {
                            this.OnMessageDropped();
                            tryAckNow = true;
                        }
                        else if (wsrmFeb2005 && info.Action == WsrmFeb2005Strings.LastMessageAction)
                        {
                            this.inputConnection.Merge(sequenceNumber, isLast);

                            if (this.inputConnection.AllAdded)
                            {
                                scheduleShutdown = true;

                                if (this.outputConnection.CheckForTermination())
                                {
                                    this.session.CloseSession();
                                }
                            }
                        }
                        else if (this.State == CommunicationState.Closing)
                        {
                            if (wsrmFeb2005)
                            {
                                fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                                    SR.GetString(SR.SequenceTerminatedSessionClosedBeforeDone),
                                    SR.GetString(SR.SessionClosedBeforeDone));
                            }
                            else
                            {
                                message = new SequenceClosedFault(this.session.InputID).CreateMessage(
                                    this.settings.MessageVersion, this.settings.ReliableMessagingVersion);
                                forceAck = true;

                                this.OnMessageDropped();
                            }
                        }
                        // In the unordered case we accept no more than MaxSequenceRanges ranges to limit the
                        // serialized ack size and the amount of memory taken by the ack ranges. In the
                        // ordered case, the delivery strategy MaxTransferWindowSize quota mitigates this
                        // threat.
                        else if (this.deliveryStrategy.CanEnqueue(sequenceNumber)
                            && (this.Settings.Ordered || this.inputConnection.CanMerge(sequenceNumber)))
                        {
                            this.inputConnection.Merge(sequenceNumber, isLast);
                            needDispatch = this.deliveryStrategy.Enqueue(info.Message, sequenceNumber);
                            closeMessage = false;
                            oldAckVersion = this.ackVersion;
                            this.pendingAcknowledgements++;

                            if (this.inputConnection.AllAdded)
                            {
                                scheduleShutdown = true;

                                if (this.outputConnection.CheckForTermination())
                                {
                                    this.session.CloseSession();
                                }
                            }
                        }
                        else
                        {
                            this.OnMessageDropped();
                        }

                        // if (ack now && we enqueued && an ack has been sent since we enqueued (and thus 
                        // carries the sequence number of the message we just processed)) then we don't
                        // need to ack again.
                        if (this.inputConnection.IsLastKnown || this.pendingAcknowledgements == this.settings.MaxTransferWindowSize)
                            tryAckNow = true;

                        bool startTimer = tryAckNow || (this.pendingAcknowledgements > 0 && fault == null);
                        if (startTimer && !this.acknowledgementScheduled)
                        {
                            this.acknowledgementScheduled = true;
                            this.acknowledgementTimer.Set(this.settings.AcknowledgementInterval);
                        }
                    }

                    if (needDispatch)
                    {
                        this.Dispatch();
                    }
                }
                else if (wsrmFeb2005 && info.TerminateSequenceInfo != null)
                {
                    bool isTerminateEarly;

                    lock (this.ThisLock)
                    {
                        isTerminateEarly = !this.inputConnection.Terminate();
                    }

                    if (isTerminateEarly)
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                            SR.GetString(SR.SequenceTerminatedEarlyTerminateSequence),
                            SR.GetString(SR.EarlyTerminateSequence));
                    }
                }
                else if (wsrm11)
                {
                    if (((info.TerminateSequenceInfo != null) && (info.TerminateSequenceInfo.Identifier == this.session.InputID))
                        || (info.CloseSequenceInfo != null))
                    {
                        bool isTerminate = info.TerminateSequenceInfo != null;
                        WsrmRequestInfo requestInfo = isTerminate
                            ? (WsrmRequestInfo)info.TerminateSequenceInfo
                            : (WsrmRequestInfo)info.CloseSequenceInfo;
                        Int64 last = isTerminate ? info.TerminateSequenceInfo.LastMsgNumber : info.CloseSequenceInfo.LastMsgNumber;

                        if (!WsrmUtilities.ValidateWsrmRequest(this.session, requestInfo, this.binder, null))
                        {
                            return;
                        }

                        bool isLastLargeEnough = true;
                        bool isLastConsistent = true;

                        lock (this.ThisLock)
                        {
                            if (!this.inputConnection.IsLastKnown)
                            {
                                if (isTerminate)
                                {
                                    if (this.inputConnection.SetTerminateSequenceLast(last, out isLastLargeEnough))
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
                                    scheduleShutdown = this.inputConnection.SetCloseSequenceLast(last);
                                    isLastLargeEnough = scheduleShutdown;
                                }

                                if (scheduleShutdown)
                                {
                                    this.session.SetFinalAck(this.inputConnection.Ranges);
                                    if (this.terminateRequestor != null)
                                    {
                                        this.session.CloseSession();
                                    }

                                    this.deliveryStrategy.Dispose();
                                }
                            }
                            else
                            {
                                isLastConsistent = (last == this.inputConnection.Last);

                                // Have seen CloseSequence already, TerminateSequence means cleanup.
                                if (isTerminate && isLastConsistent && this.inputConnection.IsSequenceClosed)
                                {
                                    terminate = true;
                                }
                            }
                        }

                        if (!isLastLargeEnough)
                        {
                            string faultString = SR.GetString(SR.SequenceTerminatedSmallLastMsgNumber);
                            string exceptionString = SR.GetString(SR.SmallLastMsgNumberExceptionString);
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, faultString, exceptionString);
                        }
                        else if (!isLastConsistent)
                        {
                            string faultString = SR.GetString(SR.SequenceTerminatedInconsistentLastMsgNumber);
                            string exceptionString = SR.GetString(SR.InconsistentLastMsgNumberExceptionString);
                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, faultString, exceptionString);
                        }
                        else
                        {
                            message = isTerminate
                                ? WsrmUtilities.CreateTerminateResponseMessage(this.settings.MessageVersion,
                                requestInfo.MessageId, this.session.InputID)
                                : WsrmUtilities.CreateCloseSequenceResponse(this.settings.MessageVersion,
                                requestInfo.MessageId, this.session.InputID);
                            forceAck = true;
                        }
                    }
                    else if (info.TerminateSequenceInfo != null)    // Identifier == OutputID
                    {
                        fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                            SR.GetString(SR.SequenceTerminatedUnsupportedTerminateSequence),
                            SR.GetString(SR.UnsupportedTerminateSequenceExceptionString));
                    }
                    else if (info.TerminateSequenceResponseInfo != null)
                    {
                        fault = this.ProcessCloseOrTerminateSequenceResponse(false, info);
                    }
                    else if (info.CloseSequenceResponseInfo != null)
                    {
                        fault = this.ProcessCloseOrTerminateSequenceResponse(true, info);
                    }
                    else if (final)
                    {
                        if (this.closeRequestor == null)
                        {
                            string exceptionString = SR.GetString(SR.UnsupportedCloseExceptionString);
                            string faultString = SR.GetString(SR.SequenceTerminatedUnsupportedClose);

                            fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultString,
                                exceptionString);
                        }
                        else
                        {
                            fault = WsrmUtilities.ValidateFinalAck(this.session, info, this.outputConnection.Last);

                            if (fault == null)
                            {
                                this.closeRequestor.SetInfo(info);
                            }
                        }
                    }
                    else if (info.WsrmHeaderFault != null)
                    {
                        if (!(info.WsrmHeaderFault is UnknownSequenceFault))
                        {
                            throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                        }

                        if (this.terminateRequestor == null)
                        {
                            throw Fx.AssertAndThrow("In wsrm11, if we start getting UnknownSequence, terminateRequestor cannot be null.");
                        }

                        this.terminateRequestor.SetInfo(info);
                    }
                }

                if (fault != null)
                {
                    this.session.OnLocalFault(fault.CreateException(), fault, null);
                    return;
                }

                if (scheduleShutdown)
                {
                    ActionItem.Schedule(this.ShutdownCallback, null);
                }

                if (message != null)
                {
                    if (forceAck)
                    {
                        WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, message,
                            this.session.InputID, this.inputConnection.Ranges, true, this.GetBufferRemaining());
                    }
                    else if (tryAckNow)
                    {
                        this.AddPendingAcknowledgements(message);
                    }
                }
                else if (tryAckNow)
                {
                    lock (this.ThisLock)
                    {
                        if (oldAckVersion != 0 && oldAckVersion != this.ackVersion)
                            return;

                        if (this.acknowledgementScheduled)
                        {
                            this.acknowledgementTimer.Cancel();
                            this.acknowledgementScheduled = false;
                        }
                        this.pendingAcknowledgements = 0;
                    }

                    message = this.CreateAcknowledgmentMessage();
                }

                if (message != null)
                {
                    using (message)
                    {
                        if (this.guard.Enter())
                        {
                            try
                            {
                                this.binder.Send(message, this.DefaultSendTimeout);
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
                        this.inputConnection.Terminate();
                    }
                }

                if (remoteFaultException != null)
                {
                    this.ReliableSession.OnRemoteFault(remoteFaultException);
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

        protected abstract void ProcessMessage(WsrmMessageInfo info);

        protected override void OnAbort()
        {
            if (this.outputConnection != null)
                this.outputConnection.Abort(this);

            if (this.inputConnection != null)
                this.inputConnection.Abort(this);

            this.guard.Abort();

            ReliableRequestor tempRequestor = this.closeRequestor;
            if (tempRequestor != null)
            {
                tempRequestor.Abort(this);
            }

            tempRequestor = this.terminateRequestor;
            if (tempRequestor != null)
            {
                tempRequestor.Abort(this);
            }

            this.session.Abort();
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
                        this.binder.Send(message, this.DefaultSendTimeout);
                    }
                }
                finally
                {
                    this.guard.Exit();
                }
            }
        }

        protected IAsyncResult OnBeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool complete = false;

            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
                this.ThrowIfFaulted();

                if ((this.State != CommunicationState.Opened)
                    || (this.closeOutputWaitObject != null))
                {
                    complete = true;
                }
                else
                {
                    this.closeOutputWaitObject = new InterruptibleWaitObject(false, true);
                }
            }

            if (complete)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                bool throwing = true;

                try
                {
                    IAsyncResult result = this.BeginInternalCloseOutputSession(timeout, callback,
                        state);

                    throwing = false;
                    return result;
                }
                finally
                {
                    if (throwing)
                    {
                        this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, SR.GetString(SR.CloseOutputSessionErrorReason), null), null);
                        this.closeOutputWaitObject.Fault(this);
                    }
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfCloseInvalid();

            OperationWithTimeoutBeginCallback closeOutputConnectionBeginCallback;
            OperationEndCallback closeOutputConnectionEndCallback;

            if (this.outputConnection == null)
            {
                closeOutputConnectionBeginCallback = default(OperationWithTimeoutBeginCallback);
                closeOutputConnectionEndCallback = default(OperationEndCallback);
            }
            else if (this.closeOutputWaitObject == null)
            {
                closeOutputConnectionBeginCallback = new OperationWithTimeoutBeginCallback(
                    this.BeginInternalCloseOutputSession);
                closeOutputConnectionEndCallback = new OperationEndCallback(
                    this.EndInternalCloseOutputSession);
            }
            else
            {
                closeOutputConnectionBeginCallback = new OperationWithTimeoutBeginCallback(
                    this.closeOutputWaitObject.BeginWait);
                closeOutputConnectionEndCallback = new OperationEndCallback(
                    this.closeOutputWaitObject.EndWait);
            }

            OperationWithTimeoutBeginCallback closeInputConnectionBeginCallback;
            OperationEndCallback closeInputConnectionEndCallback;

            if (this.inputConnection == null)
            {
                closeInputConnectionBeginCallback = default(OperationWithTimeoutBeginCallback);
                closeInputConnectionEndCallback = default(OperationEndCallback);
            }
            else
            {
                closeInputConnectionBeginCallback = new OperationWithTimeoutBeginCallback(
                    this.inputConnection.BeginClose);
                closeInputConnectionEndCallback = new OperationEndCallback(
                    this.inputConnection.EndClose);
            }

            OperationWithTimeoutBeginCallback[] beginOperations;
            OperationEndCallback[] endOperations;

            beginOperations = new OperationWithTimeoutBeginCallback[] 
            {
                closeOutputConnectionBeginCallback,
                closeInputConnectionBeginCallback,
                this.guard.BeginClose,
                this.session.BeginClose,
                this.BeginCloseBinder,
                base.OnBeginClose
            };

            endOperations = new OperationEndCallback[] 
            {
                closeOutputConnectionEndCallback,
                closeInputConnectionEndCallback,
                this.guard.EndClose,
                this.session.EndClose,
                this.EndCloseBinder,
                base.OnEndClose
            };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                beginOperations, endOperations, callback, state);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.outputConnection.BeginAddMessage(message, timeout, null, callback, state);
        }

        IAsyncResult OnBeginSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
            {
                this.session.OnLocalFault(new CommunicationException(SR.GetString(SR.MaximumRetryCountExceeded), this.maxRetryCountException),
                    SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                this.session.OnLocalActivity();
                this.AddPendingAcknowledgements(attemptInfo.Message);

                ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state);
                result.Binder = this.binder;
                result.MessageAttemptInfo = attemptInfo;
                result.MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

                if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
                {
                    result.MaskingMode |= MaskingMode.Handled;
                    result.SaveHandledException = false;
                }
                else
                {
                    result.SaveHandledException = true;
                }

                result.Begin(timeout);
                return result;
            }
        }

        IAsyncResult OnBeginSendAckRequestedHandler(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.session.OnLocalActivity();

            ReliableBinderSendAsyncResult sendResult = new ReliableBinderSendAsyncResult(callback, state);
            sendResult.Binder = this.binder;
            sendResult.MaskingMode = MaskingMode.Handled;
            sendResult.Message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID);
            sendResult.Begin(timeout);

            return sendResult;
        }

        void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
            {
                if (this.State == CommunicationState.Opening ||
                    this.State == CommunicationState.Opened ||
                    this.State == CommunicationState.Closing)
                {
                    this.session.OnLocalFault(exception, SequenceTerminatedFault.CreateQuotaExceededFault(this.session.OutputID), null);
                }
            }
            else
            {
                this.EnqueueAndDispatch(exception, null, false);
            }
        }

        void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();

            if (this.State == CommunicationState.Opening ||
                this.State == CommunicationState.Opened ||
                this.State == CommunicationState.Closing)
            {
                exception = new CommunicationException(SR.GetString(SR.EarlySecurityFaulted), exception);
                this.session.OnLocalFault(exception, (Message)null, null);
            }
        }

        // CloseOutputSession && Close: CloseOutputSession only closes the ReliableOutputConnection
        // from the Opened state, if it does, it must create the closeOutputWaitObject so that
        // close may properly synchronize. If no closeOutputWaitObject is present, Close may close
        // the ---- safely since it is in the Closing state.
        protected override void OnClose(TimeSpan timeout)
        {
            this.ThrowIfCloseInvalid();
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (this.outputConnection != null)
            {
                if (this.closeOutputWaitObject != null)
                {
                    this.closeOutputWaitObject.Wait(timeoutHelper.RemainingTime());
                }
                else
                {
                    this.InternalCloseOutputSession(timeoutHelper.RemainingTime());
                }

                this.inputConnection.Close(timeoutHelper.RemainingTime());
            }

            this.guard.Close(timeoutHelper.RemainingTime());
            this.session.Close(timeoutHelper.RemainingTime());
            this.binder.Close(timeoutHelper.RemainingTime(), MaskingMode.Handled);
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected void OnCloseOutputSession(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
                this.ThrowIfFaulted();

                if ((this.State != CommunicationState.Opened)
                    || (this.closeOutputWaitObject != null))
                {
                    return;
                }

                this.closeOutputWaitObject = new InterruptibleWaitObject(false, true);
            }

            bool throwing = true;

            try
            {
                this.InternalCloseOutputSession(timeout);
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, SR.GetString(SR.CloseOutputSessionErrorReason), null), null);
                    this.closeOutputWaitObject.Fault(this);
                }
                else
                {
                    this.closeOutputWaitObject.Set();
                }
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            this.binder.Faulted -= this.OnBinderFaulted;
            if (this.deliveryStrategy != null)
                this.deliveryStrategy.Dispose();
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.acknowledgementTimer.Cancel();
        }

        void OnComponentFaulted(Exception faultException, WsrmFault fault)
        {
            this.session.OnLocalFault(faultException, fault, null);
        }

        void OnComponentException(Exception exception)
        {
            this.ReliableSession.OnUnknownException(exception);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected void OnEndCloseOutputSession(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                bool throwing = true;
                try
                {
                    this.EndInternalCloseOutputSession(result);
                    throwing = false;
                }
                finally
                {
                    if (throwing)
                    {
                        this.session.OnLocalFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.session.OutputID, SR.GetString(SR.CloseOutputSessionErrorReason), null), null);
                        this.closeOutputWaitObject.Fault(this);
                    }
                    else
                    {
                        this.closeOutputWaitObject.Set();
                    }
                }
            }
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (!this.outputConnection.EndAddMessage(result))
                this.ThrowInvalidAddException();
        }

        void OnEndSendHandler(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                Exception handledException;

                ReliableBinderSendAsyncResult.End(result, out handledException);
                ReliableBinderSendAsyncResult sendResult = (ReliableBinderSendAsyncResult)result;
                if (sendResult.MessageAttemptInfo.RetryCount == this.settings.MaxRetryCount)
                {
                    this.maxRetryCountException = handledException;
                }
            }
        }

        void OnEndSendAckRequestedHandler(IAsyncResult result)
        {
            ReliableBinderSendAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            if (!this.outputConnection.AddMessage(message, timeout, null))
                this.ThrowInvalidAddException();
        }

        void OnSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
        {
            using (attemptInfo.Message)
            {
                if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
                {
                    this.session.OnLocalFault(new CommunicationException(SR.GetString(SR.MaximumRetryCountExceeded), this.maxRetryCountException),
                        SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                }
                else
                {
                    this.session.OnLocalActivity();
                    this.AddPendingAcknowledgements(attemptInfo.Message);

                    MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

                    if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
                    {
                        maskingMode |= MaskingMode.Handled;
                        this.binder.Send(attemptInfo.Message, timeout, maskingMode);
                    }
                    else
                    {
                        try
                        {
                            this.binder.Send(attemptInfo.Message, timeout, maskingMode);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                                throw;

                            if (this.binder.IsHandleable(e))
                            {
                                this.maxRetryCountException = e;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        void OnSendAckRequestedHandler(TimeSpan timeout)
        {
            this.session.OnLocalActivity();
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.binder.Send(message, timeout, MaskingMode.Handled);
            }
        }

        static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            ReliableDuplexSessionChannel channel = (ReliableDuplexSessionChannel)(result.AsyncState);

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

        static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult)state;
            ReliableDuplexSessionChannel channel = (ReliableDuplexSessionChannel)(result.AsyncState);
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
                        terminated = this.inputConnection.Terminate();
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

                WsrmMessageInfo info = WsrmMessageInfo.Get(this.settings.MessageVersion,
                    this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(),
                    message);

                this.StartReceiving(false);
                this.ProcessMessage(info);
                return false;
            }
            return true;
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        protected virtual void OnMessageDropped()
        {
        }

        protected void SetConnections()
        {
            this.outputConnection = new ReliableOutputConnection(this.session.OutputID,
                this.settings.MaxTransferWindowSize, this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.session.InitiationTime, true, this.DefaultSendTimeout);
            this.outputConnection.Faulted += OnComponentFaulted;
            this.outputConnection.OnException += OnComponentException;
            this.outputConnection.BeginSendHandler = OnBeginSendHandler;
            this.outputConnection.EndSendHandler = OnEndSendHandler;
            this.outputConnection.SendHandler = OnSendHandler;
            this.outputConnection.BeginSendAckRequestedHandler = OnBeginSendAckRequestedHandler;
            this.outputConnection.EndSendAckRequestedHandler = OnEndSendAckRequestedHandler;
            this.outputConnection.SendAckRequestedHandler = OnSendAckRequestedHandler;

            this.inputConnection = new ReliableInputConnection();
            this.inputConnection.ReliableMessagingVersion = this.Settings.ReliableMessagingVersion;

            if (this.settings.Ordered)
                this.deliveryStrategy = new OrderedDeliveryStrategy<Message>(this, this.settings.MaxTransferWindowSize, false);
            else
                this.deliveryStrategy = new UnorderedDeliveryStrategy<Message>(this, this.settings.MaxTransferWindowSize);

            this.deliveryStrategy.DequeueCallback = this.OnDeliveryStrategyItemDequeued;
        }

        protected void SetSession(ChannelReliableSession session)
        {
            session.UnblockChannelCloseCallback = this.UnblockClose;
            this.session = session;
        }

        void OnDeliveryStrategyItemDequeued()
        {
            if (this.advertisedZero)
                this.OnAcknowledgementTimeoutElapsed(null);
        }

        protected void StartReceiving(bool canBlock)
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
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.outputConnection.CheckForTermination())
                {
                    this.session.CloseSession();
                }

                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion,
                    reliableMessagingVersion, this.session.OutputID);
                this.binder.Send(message, timeout, MaskingMode.Handled);
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CreateTerminateRequestor();
                this.terminateRequestor.Request(timeout);
                // reply came from receive loop, receive loop owns verified message so nothing more to do.
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.outputConnection.CheckForTermination())
                {
                    this.session.CloseSession();
                }

                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion,
                    reliableMessagingVersion, this.session.OutputID);
                return this.binder.BeginSend(message, timeout, MaskingMode.Handled, callback, state);
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CreateTerminateRequestor();
                return this.terminateRequestor.BeginRequest(timeout, callback, state);
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        void EndTerminateSequence(IAsyncResult result)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.binder.EndSend(result);
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.terminateRequestor.EndRequest(result);
                // reply came from receive loop, receive loop owns verified message so nothing more to do.
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        void ThrowIfCloseInvalid()
        {
            bool shouldFault = false;

            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (this.deliveryStrategy.EnqueuedCount > 0 || this.inputConnection.Ranges.Count > 1)
                {
                    shouldFault = true;
                }
            }
            else if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.deliveryStrategy.EnqueuedCount > 0)
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

        void ThrowInvalidAddException()
        {
            if (this.State == CommunicationState.Opened)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SendCannotBeCalledAfterCloseOutputSession)));
            else if (this.State == CommunicationState.Faulted)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetTerminalException());
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateClosedException());
        }

        void UnblockClose()
        {
            if (this.outputConnection != null)
            {
                this.outputConnection.Fault(this);
            }

            if (this.inputConnection != null)
            {
                this.inputConnection.Fault(this);
            }

            ReliableRequestor tempRequestor = this.closeRequestor;
            if (tempRequestor != null)
            {
                tempRequestor.Fault(this);
            }

            tempRequestor = this.terminateRequestor;
            if (tempRequestor != null)
            {
                tempRequestor.Fault(this);
            }
        }
    }

    class ClientReliableDuplexSessionChannel : ReliableDuplexSessionChannel
    {
        ChannelParameterCollection channelParameters;
        DuplexClientReliableSession clientSession;
        TimeoutHelper closeTimeoutHelper;
        bool closing;
        static AsyncCallback onReconnectComplete = Fx.ThunkCallback(new AsyncCallback(OnReconnectComplete));
        static Action<object> onReconnectTimerElapsed = new Action<object>(OnReconnectTimerElapsed);

        public ClientReliableDuplexSessionChannel(ChannelManagerBase factory, IReliableFactorySettings settings,
            IReliableChannelBinder binder, FaultHelper faultHelper,
            LateBoundChannelParameterCollection channelParameters, UniqueId inputID)
            : base(factory, settings, binder)
        {
            this.clientSession = new DuplexClientReliableSession(this, settings, faultHelper, inputID);
            this.clientSession.PollingCallback = this.PollingCallback;
            this.SetSession(this.clientSession);

            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
            ((IClientReliableChannelBinder)binder).ConnectionLost += this.OnConnectionLost;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(ChannelParameterCollection))
            {
                return (T)(object)this.channelParameters;
            }

            return base.GetProperty<T>();
        }

        void HandleReconnectComplete(IAsyncResult result)
        {
            bool handleException = true;

            try
            {
                this.Binder.EndSend(result);
                handleException = false;

                lock (this.ThisLock)
                {
                    if (this.Binder.Connected)
                        this.clientSession.ResumePolling(this.OutputConnection.Strategy.QuotaRemaining == 0);
                    else
                        this.WaitForReconnect();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (handleException)
                    this.WaitForReconnect();
                else
                    throw;
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.closeTimeoutHelper = new TimeoutHelper(timeout);
            this.closing = true;
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(this.Binder, this.ReliableSession,
                timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.closeTimeoutHelper = new TimeoutHelper(timeout);
            this.closing = true;
            base.OnClose(timeout);
        }

        void OnConnectionLost(object sender, EventArgs args)
        {
            lock (this.ThisLock)
            {
                if ((this.State == CommunicationState.Opened || this.State == CommunicationState.Closing) &&
                    !this.Binder.Connected && this.clientSession.StopPolling())
                {

                    if (TD.ClientReliableSessionReconnectIsEnabled())
                    {
                        TD.ClientReliableSessionReconnect(this.clientSession.Id);
                    }

                    this.Reconnect();
                }
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            bool throwing = true;

            try
            {
                this.Binder.Open(timeoutHelper.RemainingTime());
                this.ReliableSession.Open(timeoutHelper.RemainingTime());
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    this.Binder.Close(timeoutHelper.RemainingTime());
                }
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.SetConnections();

            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                try
                {
                    this.StartReceiving(false);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    this.ReliableSession.OnUnknownException(e);
                }
            }
            else
            {
                ActionItem.Schedule(new Action<object>(StartReceivingStatic), this);
            }
        }

        static void OnReconnectComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;

            ClientReliableDuplexSessionChannel channel = (ClientReliableDuplexSessionChannel)result.AsyncState;
            channel.HandleReconnectComplete(result);
        }

        static void OnReconnectTimerElapsed(object state)
        {
            ClientReliableDuplexSessionChannel channel = (ClientReliableDuplexSessionChannel)state;

            lock (channel.ThisLock)
            {
                if ((channel.State == CommunicationState.Opened || channel.State == CommunicationState.Closing) &&
                    !channel.Binder.Connected)
                {
                    channel.Reconnect();
                }
                else
                {
                    channel.clientSession.ResumePolling(channel.OutputConnection.Strategy.QuotaRemaining == 0);
                }
            }
        }

        protected override void OnRemoteActivity()
        {
            this.ReliableSession.OnRemoteActivity(this.OutputConnection.Strategy.QuotaRemaining == 0);
        }

        void PollingCallback()
        {
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.Binder.Send(message, this.DefaultSendTimeout);
            }
        }

        protected override void ProcessMessage(WsrmMessageInfo info)
        {
            if (!this.ReliableSession.ProcessInfo(info, null))
                return;

            if (!this.ReliableSession.VerifyDuplexProtocolElements(info, null))
                return;

            this.ProcessDuplexMessage(info);
        }

        static void StartReceivingStatic(object state)
        {
            ClientReliableDuplexSessionChannel channel = (ClientReliableDuplexSessionChannel)state;

            try
            {
                channel.StartReceiving(true);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                channel.ReliableSession.OnUnknownException(e);
            }
        }

        // It is safe to call this in a lock.
        void Reconnect()
        {
            bool handleException = true;

            try
            {
                Message message = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion,
                    this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID);
                TimeSpan timeout = this.closing ? this.closeTimeoutHelper.RemainingTime() : this.DefaultCloseTimeout;
                IAsyncResult result = this.Binder.BeginSend(message, timeout, onReconnectComplete, this);

                handleException = false;
                if (result.CompletedSynchronously)
                    this.HandleReconnectComplete(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (handleException)
                    this.WaitForReconnect();
                else
                    throw;
            }
        }

        // If anything throws out of this method, we'll consider it fatal.
        void WaitForReconnect()
        {
            TimeSpan timeout;

            if (this.closing)
                timeout = TimeoutHelper.Divide(this.closeTimeoutHelper.RemainingTime(), 2);
            else
                timeout = TimeoutHelper.Divide(this.DefaultSendTimeout, 2);

            IOThreadTimer timer = new IOThreadTimer(onReconnectTimerElapsed, this, false);
            timer.Set(timeout);
        }

        class DuplexClientReliableSession : ClientReliableSession, IDuplexSession
        {
            ClientReliableDuplexSessionChannel channel;

            public DuplexClientReliableSession(ClientReliableDuplexSessionChannel channel,
                IReliableFactorySettings settings, FaultHelper helper, UniqueId inputID)
                : base(channel, settings, (IClientReliableChannelBinder)channel.Binder, helper, inputID)
            {
                this.channel = channel;
            }

            public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
            {
                return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.OnBeginCloseOutputSession(timeout, callback, state);
            }

            public void EndCloseOutputSession(IAsyncResult result)
            {
                channel.OnEndCloseOutputSession(result);
            }

            public void CloseOutputSession()
            {
                this.CloseOutputSession(this.channel.DefaultCloseTimeout);
            }

            public void CloseOutputSession(TimeSpan timeout)
            {
                channel.OnCloseOutputSession(timeout);
            }
        }
    }

    sealed class ServerReliableDuplexSessionChannel : ReliableDuplexSessionChannel
    {
        ReliableChannelListenerBase<IDuplexSessionChannel> listener;
        string perfCounterId;

        public ServerReliableDuplexSessionChannel(
            ReliableChannelListenerBase<IDuplexSessionChannel> listener,
            IReliableChannelBinder binder, FaultHelper faultHelper,
            UniqueId inputID,
            UniqueId outputID)
            : base(listener, listener, binder)
        {
            this.listener = listener;
            DuplexServerReliableSession session = new DuplexServerReliableSession(this, listener, faultHelper, inputID, outputID);
            this.SetSession(session);
            session.Open(TimeSpan.Zero);
            this.SetConnections();

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
                        throw;

                    this.ReliableSession.OnUnknownException(e);
                }
            }
        }

        IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(this.ReliableSession.InputID,
                this.ReliableSession.OutputID, timeout, callback, state);
        }

        void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        // Close/Abort: The base Close/Abort is called first because it is shutting down the
        // channel. Shutting down the server state should be done after shutting down the channel.
        protected override void OnAbort()
        {
            base.OnAbort();
            this.listener.OnReliableChannelAbort(this.ReliableSession.InputID,
                this.ReliableSession.OutputID);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback,
            object state)
        {
            OperationWithTimeoutBeginCallback[] beginOperations =
                new OperationWithTimeoutBeginCallback[] {
                    new OperationWithTimeoutBeginCallback(base.OnBeginClose),
                    new OperationWithTimeoutBeginCallback(this.BeginUnregisterChannel) };

            OperationEndCallback[] endOperations =
                new OperationEndCallback[] {
                    new OperationEndCallback(base.OnEndClose),
                    new OperationEndCallback(this.EndUnregisterChannel) };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout,
                beginOperations, endOperations, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            this.listener.OnReliableChannelClose(this.ReliableSession.InputID,
                this.ReliableSession.OutputID, timeoutHelper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

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

        protected override void OnFaulted()
        {
            base.OnFaulted();
            if (PerformanceCounters.PerformanceCountersEnabled)
                PerformanceCounters.SessionFaulted(this.perfCounterId);
        }

        protected override void OnMessageDropped()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
                PerformanceCounters.MessageDropped(this.perfCounterId);
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

        protected override void ProcessMessage(WsrmMessageInfo info)
        {
            if (!this.ReliableSession.ProcessInfo(info, null))
                return;

            if (!this.ReliableSession.VerifyDuplexProtocolElements(info, null))
                return;

            if (info.CreateSequenceInfo != null)
            {
                EndpointAddress acksTo;

                if (WsrmUtilities.ValidateCreateSequence<IDuplexSessionChannel>(info, this.listener, this.Binder.Channel, out acksTo))
                {
                    Message response = WsrmUtilities.CreateCreateSequenceResponse(this.Settings.MessageVersion,
                        this.Settings.ReliableMessagingVersion, true, info.CreateSequenceInfo, this.Settings.Ordered,
                        this.ReliableSession.InputID, acksTo);
                    using (info.Message)
                    {
                        using (response)
                        {
                            if (((IServerReliableChannelBinder)this.Binder).AddressResponse(info.Message, response))
                                this.Binder.Send(response, this.DefaultSendTimeout);
                        }
                    }
                }
                else
                {
                    this.ReliableSession.OnLocalFault(info.FaultException, info.FaultReply, null);
                }

                return;
            }

            this.ProcessDuplexMessage(info);
        }

        class DuplexServerReliableSession : ServerReliableSession, IDuplexSession
        {
            ServerReliableDuplexSessionChannel channel;

            public DuplexServerReliableSession(ServerReliableDuplexSessionChannel channel,
                ReliableChannelListenerBase<IDuplexSessionChannel> listener, FaultHelper faultHelper, UniqueId inputID,
                UniqueId outputID)
                : base(channel, listener, (IServerReliableChannelBinder)channel.Binder, faultHelper, inputID, outputID)
            {
                this.channel = channel;
            }

            public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
            {
                return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.OnBeginCloseOutputSession(timeout, callback, state);
            }

            public void EndCloseOutputSession(IAsyncResult result)
            {
                channel.OnEndCloseOutputSession(result);
            }

            public void CloseOutputSession()
            {
                this.CloseOutputSession(this.channel.DefaultCloseTimeout);
            }

            public void CloseOutputSession(TimeSpan timeout)
            {
                channel.OnCloseOutputSession(timeout);
            }
        }
    }
}
