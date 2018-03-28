//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;

    sealed class ReliableRequestSessionChannel : RequestChannel, IRequestSessionChannel
    {
        IClientReliableChannelBinder binder;
        ChannelParameterCollection channelParameters;
        ReliableRequestor closeRequestor;
        ReliableOutputConnection connection;
        bool isLastKnown = false;
        Exception maxRetryCountException = null;
        static AsyncCallback onPollingComplete = Fx.ThunkCallback(new AsyncCallback(OnPollingComplete));
        SequenceRangeCollection ranges = SequenceRangeCollection.Empty;
        Guard replyAckConsistencyGuard;
        ClientReliableSession session;
        IReliableFactorySettings settings;
        InterruptibleWaitObject shutdownHandle;
        ReliableRequestor terminateRequestor;

        public ReliableRequestSessionChannel(
            ChannelManagerBase factory,
            IReliableFactorySettings settings,
            IClientReliableChannelBinder binder,
            FaultHelper faultHelper,
            LateBoundChannelParameterCollection channelParameters,
            UniqueId inputID)
            : base(factory, binder.RemoteAddress, binder.Via, true)
        {
            this.settings = settings;
            this.binder = binder;
            this.session = new ClientReliableSession(this, settings, binder, faultHelper, inputID);
            this.session.PollingCallback = this.PollingCallback;
            this.session.UnblockChannelCloseCallback = this.UnblockClose;

            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle = new InterruptibleWaitObject(false);
            }
            else
            {
                this.replyAckConsistencyGuard = new Guard(Int32.MaxValue);
            }

            this.binder.Faulted += OnBinderFaulted;
            this.binder.OnException += OnBinderException;

            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
        }

        public IOutputSession Session
        {
            get
            {
                return this.session;
            }
        }

        void AddAcknowledgementHeader(Message message, bool force)
        {
            if (this.ranges.Count == 0)
            {
                return;
            }

            WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion, message,
                this.session.InputID, this.ranges, this.isLastKnown);
        }

        IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        IAsyncResult BeginTerminateSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateTerminateRequestor();
            return this.terminateRequestor.BeginRequest(timeout, callback, state);
        }

        void CloseSequence(TimeSpan timeout)
        {
            this.CreateCloseRequestor();
            Message closeReply = this.closeRequestor.Request(timeout);
            this.ProcessCloseOrTerminateReply(true, closeReply);
        }

        IAsyncResult BeginCloseSequence(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CreateCloseRequestor();
            return this.closeRequestor.BeginRequest(timeout, callback, state);
        }

        void EndCloseSequence(IAsyncResult result)
        {
            Message closeReply = this.closeRequestor.EndRequest(result);
            this.ProcessCloseOrTerminateReply(true, closeReply);
        }

        void ConfigureRequestor(ReliableRequestor requestor)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
            requestor.MessageHeader = new WsrmAcknowledgmentHeader(reliableMessagingVersion, this.session.InputID,
                this.ranges, true, -1);
        }

        Message CreateAckRequestedMessage()
        {
            Message request = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion, this.session.OutputID);
            this.AddAcknowledgementHeader(request, true);
            return request;
        }

        protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
        {
            return new AsyncRequest(this, callback, state);
        }

        void CreateCloseRequestor()
        {
            RequestReliableRequestor temp = new RequestReliableRequestor();

            this.ConfigureRequestor(temp);
            temp.TimeoutString1Index = SR.TimeoutOnClose;
            temp.MessageAction = WsrmIndex.GetCloseSequenceActionHeader(
                this.settings.MessageVersion.Addressing);
            temp.MessageBody = new CloseSequence(this.session.OutputID, this.connection.Last);

            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.closeRequestor = temp;
            }
        }

        protected override IRequest CreateRequest(Message message)
        {
            return new SyncRequest(this);
        }

        void CreateTerminateRequestor()
        {
            RequestReliableRequestor temp = new RequestReliableRequestor();

            this.ConfigureRequestor(temp);
            temp.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(
                this.settings.MessageVersion.Addressing, this.settings.ReliableMessagingVersion);
            temp.MessageBody = new TerminateSequence(this.settings.ReliableMessagingVersion,
                this.session.OutputID, this.connection.Last);

            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.terminateRequestor = temp;
                this.session.CloseSession();
            }
        }

        void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        void EndTerminateSequence(IAsyncResult result)
        {
            Message terminateReply = this.terminateRequestor.EndRequest(result);

            if (terminateReply != null)
            {
                this.ProcessCloseOrTerminateReply(false, terminateReply);
            }
        }

        Exception GetInvalidAddException()
        {
            if (this.State == CommunicationState.Faulted)
                return this.GetTerminalException();
            else
                return this.CreateClosedException();
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IRequestSessionChannel))
            {
                return (T)(object)this;
            }

            if (typeof(T) == typeof(ChannelParameterCollection))
            {
                return (T)(object)this.channelParameters;
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

        protected override void OnAbort()
        {
            if (this.connection != null)
            {
                this.connection.Abort(this);
            }

            if (this.shutdownHandle != null)
            {
                this.shutdownHandle.Abort(this);
            }

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
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            OperationWithTimeoutBeginCallback[] beginCallbacks = new OperationWithTimeoutBeginCallback[] {
                this.connection.BeginClose,
                this.BeginWaitForShutdown,
                wsrm11 ? this.BeginCloseSequence : default(OperationWithTimeoutBeginCallback),
                this.BeginTerminateSequence,
                this.session.BeginClose,
                this.BeginCloseBinder
            };

            OperationEndCallback[] endCallbacks = new OperationEndCallback[] {
                this.connection.EndClose,
                this.EndWaitForShutdown,
                wsrm11 ? this.EndCloseSequence : default(OperationEndCallback),
                this.EndTerminateSequence,
                this.session.EndClose,
                this.EndCloseBinder
            };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginCallbacks, endCallbacks, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(this.binder, this.session, timeout,
                callback, state);
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
                this.AddPendingException(exception);
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

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.connection.Close(timeoutHelper.RemainingTime());
            this.WaitForShutdown(timeoutHelper.RemainingTime());

            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CloseSequence(timeoutHelper.RemainingTime());
            }

            this.TerminateSequence(timeoutHelper.RemainingTime());
            this.session.Close(timeoutHelper.RemainingTime());
            this.binder.Close(timeoutHelper.RemainingTime(), MaskingMode.Handled);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.binder.Faulted -= this.OnBinderFaulted;
        }

        IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout,
            bool maskUnhandledException, AsyncCallback callback, object state)
        {
            if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
            {
                if (TD.MaxRetryCyclesExceededIsEnabled())
                {
                    TD.MaxRetryCyclesExceeded(SR.GetString(SR.MaximumRetryCountExceeded));
                }
                this.session.OnLocalFault(new CommunicationException(SR.GetString(SR.MaximumRetryCountExceeded), this.maxRetryCountException),
                       SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                this.session.OnLocalActivity();
                this.AddAcknowledgementHeader(attemptInfo.Message, false);

                ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state);
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

        void OnConnectionEndSend(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                Exception handledException;
                Message reply = ReliableBinderRequestAsyncResult.End(result, out handledException);
                ReliableBinderRequestAsyncResult requestResult = (ReliableBinderRequestAsyncResult)result;
                if (requestResult.MessageAttemptInfo.RetryCount == this.settings.MaxRetryCount)
                {
                    this.maxRetryCountException = handledException;
                }

                if (reply != null)
                {
                    this.ProcessReply(reply, (IReliableRequest)requestResult.MessageAttemptInfo.State,
                        requestResult.MessageAttemptInfo.GetSequenceNumber());
                }
            }
        }

        void OnConnectionSend(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
        {
            using (attemptInfo.Message)
            {
                if (attemptInfo.RetryCount > this.settings.MaxRetryCount)
                {
                    if (TD.MaxRetryCyclesExceededIsEnabled())
                    {
                        TD.MaxRetryCyclesExceeded(SR.GetString(SR.MaximumRetryCountExceeded));
                    }
                    this.session.OnLocalFault(new CommunicationException(SR.GetString(SR.MaximumRetryCountExceeded), this.maxRetryCountException),
                        SequenceTerminatedFault.CreateMaxRetryCountExceededFault(this.session.OutputID), null);
                    return;
                }

                this.AddAcknowledgementHeader(attemptInfo.Message, false);
                this.session.OnLocalActivity();

                Message reply = null;
                MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

                if (attemptInfo.RetryCount < this.settings.MaxRetryCount)
                {
                    maskingMode |= MaskingMode.Handled;
                    reply = this.binder.Request(attemptInfo.Message, timeout, maskingMode);
                }
                else
                {
                    try
                    {
                        reply = this.binder.Request(attemptInfo.Message, timeout, maskingMode);
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

                if (reply != null)
                    ProcessReply(reply, (IReliableRequest)attemptInfo.State, attemptInfo.GetSequenceNumber());
            }
        }

        void OnConnectionSendAckRequested(TimeSpan timeout)
        {
            // do nothing, only replies to sequence messages alter the state of the reliable output connection
        }

        IAsyncResult OnConnectionBeginSendAckRequested(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // do nothing, only replies to sequence messages alter the state of the reliable output connection
            return new CompletedAsyncResult(callback, state);
        }

        void OnConnectionEndSendAckRequested(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        void OnComponentFaulted(Exception faultException, WsrmFault fault)
        {
            this.session.OnLocalFault(faultException, fault, null);
        }

        void OnComponentException(Exception exception)
        {
            this.session.OnUnknownException(exception);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            bool throwing = true;

            try
            {
                this.binder.Open(timeoutHelper.RemainingTime());
                this.session.Open(timeoutHelper.RemainingTime());
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    this.binder.Close(timeoutHelper.RemainingTime());
                }
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.connection = new ReliableOutputConnection(this.session.OutputID, this.settings.MaxTransferWindowSize,
                this.settings.MessageVersion, this.settings.ReliableMessagingVersion, this.session.InitiationTime,
                false, this.DefaultSendTimeout);
            this.connection.Faulted += OnComponentFaulted;
            this.connection.OnException += OnComponentException;
            this.connection.BeginSendHandler = OnConnectionBeginSend;
            this.connection.EndSendHandler = OnConnectionEndSend;
            this.connection.SendHandler = OnConnectionSend;
            this.connection.BeginSendAckRequestedHandler = OnConnectionBeginSendAckRequested;
            this.connection.EndSendAckRequestedHandler = OnConnectionEndSendAckRequested;
            this.connection.SendAckRequestedHandler = OnConnectionSendAckRequested;
        }

        static void OnPollingComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableRequestSessionChannel channel = (ReliableRequestSessionChannel)result.AsyncState;
                channel.EndSendAckRequestedMessage(result);
            }
        }

        void PollingCallback()
        {
            IAsyncResult result = this.BeginSendAckRequestedMessage(this.DefaultSendTimeout, MaskingMode.All,
                onPollingComplete, this);

            if (result.CompletedSynchronously)
            {
                this.EndSendAckRequestedMessage(result);
            }
        }

        void ProcessCloseOrTerminateReply(bool close, Message reply)
        {
            if (reply == null)
            {
                // In the close case, the requestor is configured to throw TimeoutException instead of returning null.
                // In the terminate case, this value can be null, but the caller should not call this method.
                throw Fx.AssertAndThrow("Argument reply cannot be null.");
            }

            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (close)
                {
                    throw Fx.AssertAndThrow("Close does not exist in Feb2005.");
                }

                reply.Close();
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                WsrmMessageInfo info = this.closeRequestor.GetInfo();

                // Close - Final ack made it.
                // Terminate - UnknownSequence.
                // Either way, message has been verified and does not belong to this thread.
                if (info != null)
                {
                    return;
                }

                try
                {
                    info = WsrmMessageInfo.Get(this.settings.MessageVersion, reliableMessagingVersion,
                        this.binder.Channel, this.binder.GetInnerSession(), reply);
                    this.session.ProcessInfo(info, null, true);
                    this.session.VerifyDuplexProtocolElements(info, null, true);

                    WsrmFault fault = close
                        ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, this.closeRequestor.MessageId, info,
                        this.connection.Last)
                        : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, this.terminateRequestor.MessageId,
                        info, this.connection.Last);

                    if (fault != null)
                    {
                        this.session.OnLocalFault(null, fault, null);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                    }
                }
                finally
                {
                    reply.Close();
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Reliable messaging version not supported.");
            }
        }

        void ProcessReply(Message reply, IReliableRequest request, Int64 requestSequenceNumber)
        {
            WsrmMessageInfo messageInfo = WsrmMessageInfo.Get(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), reply);

            if (!this.session.ProcessInfo(messageInfo, null))
                return;

            if (!this.session.VerifyDuplexProtocolElements(messageInfo, null))
                return;

            bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            if (messageInfo.WsrmHeaderFault != null)
            {
                // Close message now, going to stop processing in all cases.
                messageInfo.Message.Close();

                if (!(messageInfo.WsrmHeaderFault is UnknownSequenceFault))
                {
                    throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                }

                if (this.terminateRequestor == null)
                {
                    throw Fx.AssertAndThrow("If we start getting UnknownSequence, terminateRequestor cannot be null.");
                }

                this.terminateRequestor.SetInfo(messageInfo);

                return;
            }

            if (messageInfo.AcknowledgementInfo == null)
            {
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID,
                    SR.GetString(SR.SequenceTerminatedReplyMissingAcknowledgement),
                    SR.GetString(SR.ReplyMissingAcknowledgement));
                messageInfo.Message.Close();
                this.session.OnLocalFault(fault.CreateException(), fault, null);
                return;
            }

            if (wsrm11 && (messageInfo.TerminateSequenceInfo != null))
            {
                UniqueId faultId = (messageInfo.TerminateSequenceInfo.Identifier == this.session.OutputID)
                    ? this.session.InputID
                    : this.session.OutputID;

                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(faultId,
                    SR.GetString(SR.SequenceTerminatedUnsupportedTerminateSequence),
                    SR.GetString(SR.UnsupportedTerminateSequenceExceptionString));
                messageInfo.Message.Close();
                this.session.OnLocalFault(fault.CreateException(), fault, null);
                return;
            }
            else if (wsrm11 && messageInfo.AcknowledgementInfo.Final)
            {
                // Close message now, going to stop processing in all cases.
                messageInfo.Message.Close();

                if (this.closeRequestor == null)
                {
                    // Remote endpoint signaled Close, this is not allowed so we fault.
                    string exceptionString = SR.GetString(SR.UnsupportedCloseExceptionString);
                    string faultString = SR.GetString(SR.SequenceTerminatedUnsupportedClose);

                    WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.OutputID, faultString,
                        exceptionString);
                    this.session.OnLocalFault(fault.CreateException(), fault, null);
                }
                else
                {
                    WsrmFault fault = WsrmUtilities.ValidateFinalAck(this.session, messageInfo, this.connection.Last);

                    if (fault == null)
                    {
                        // Received valid final ack after sending Close, inform the close thread.
                        this.closeRequestor.SetInfo(messageInfo);
                    }
                    else
                    {
                        // Received invalid final ack after sending Close, fault.
                        this.session.OnLocalFault(fault.CreateException(), fault, null);
                    }
                }

                return;
            }

            int bufferRemaining = -1;

            if (this.settings.FlowControlEnabled)
                bufferRemaining = messageInfo.AcknowledgementInfo.BufferRemaining;

            // We accept no more than MaxSequenceRanges ranges to limit the serialized ack size and
            // the amount of memory taken up by the ack ranges. Since request reply uses the presence of
            // a reply as an acknowledgement we cannot call ProcessTransferred (which stops retrying the
            // request) if we intend to drop the message. This means the limit is not strict since we do
            // not check for the limit and merge the ranges atomically. The limit + the number of
            // concurrent threads is a sufficient mitigation.
            if ((messageInfo.SequencedMessageInfo != null) &&
                !ReliableInputConnection.CanMerge(messageInfo.SequencedMessageInfo.SequenceNumber, this.ranges))
            {
                messageInfo.Message.Close();
                return;
            }

            bool exitGuard = this.replyAckConsistencyGuard != null ? this.replyAckConsistencyGuard.Enter() : false;

            try
            {
                this.connection.ProcessTransferred(requestSequenceNumber,
                    messageInfo.AcknowledgementInfo.Ranges, bufferRemaining);

                this.session.OnRemoteActivity(this.connection.Strategy.QuotaRemaining == 0);

                if (messageInfo.SequencedMessageInfo != null)
                {
                    lock (this.ThisLock)
                    {
                        this.ranges = this.ranges.MergeWith(messageInfo.SequencedMessageInfo.SequenceNumber);
                    }
                }
            }
            finally
            {
                if (exitGuard)
                {
                    this.replyAckConsistencyGuard.Exit();
                }
            }

            if (request != null)
            {
                if (WsrmUtilities.IsWsrmAction(this.settings.ReliableMessagingVersion, messageInfo.Action))
                {
                    messageInfo.Message.Close();
                    request.Set(null);
                }
                else
                {
                    request.Set(messageInfo.Message);
                }
            }

            // The termination mechanism in the TerminateSequence fails with RequestReply.
            // Since the ack ranges are updated after ProcessTransferred is called and
            // ProcessTransferred potentially signals the Termination process, this channel 
            // winds up sending a message with the ack for last message missing.
            // Thus we send the termination after we update the ranges.

            if ((this.shutdownHandle != null) && this.connection.CheckForTermination())
            {
                this.shutdownHandle.Set();
            }

            if (request != null)
                request.Complete();
        }

        IAsyncResult BeginSendAckRequestedMessage(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback,
            object state)
        {
            this.session.OnLocalActivity();
            ReliableBinderRequestAsyncResult requestResult = new ReliableBinderRequestAsyncResult(callback, state);
            requestResult.Binder = this.binder;
            requestResult.MaskingMode = maskingMode;
            requestResult.Message = this.CreateAckRequestedMessage();
            requestResult.Begin(timeout);

            return requestResult;
        }

        void EndSendAckRequestedMessage(IAsyncResult result)
        {
            Message reply = ReliableBinderRequestAsyncResult.End(result);

            if (reply != null)
            {
                this.ProcessReply(reply, null, 0);
            }
        }

        void TerminateSequence(TimeSpan timeout)
        {
            this.CreateTerminateRequestor();
            Message terminateReply = this.terminateRequestor.Request(timeout);

            if (terminateReply != null)
            {
                this.ProcessCloseOrTerminateReply(false, terminateReply);
            }
        }

        void UnblockClose()
        {
            FaultPendingRequests();

            if (this.connection != null)
            {
                this.connection.Fault(this);
            }

            if (this.shutdownHandle != null)
            {
                this.shutdownHandle.Fault(this);
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

        void WaitForShutdown(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle.Wait(timeoutHelper.RemainingTime());
            }
            else
            {
                this.isLastKnown = true;

                // We already closed the connection so we know everything was acknowledged.
                // Make sure the reply acknowledgement ranges are current.
                this.replyAckConsistencyGuard.Close(timeoutHelper.RemainingTime());
            }
        }

        IAsyncResult BeginWaitForShutdown(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return this.shutdownHandle.BeginWait(timeout, callback, state);
            }
            else
            {
                this.isLastKnown = true;

                // We already closed the connection so we know everything was acknowledged.
                // Make sure the reply acknowledgement ranges are current.
                return this.replyAckConsistencyGuard.BeginClose(timeout, callback, state);
            }
        }

        void EndWaitForShutdown(IAsyncResult result)
        {
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.shutdownHandle.EndWait(result);
            }
            else
            {
                this.replyAckConsistencyGuard.EndClose(result);
            }
        }

        interface IReliableRequest : IRequestBase
        {
            void Set(Message reply);
            void Complete();
        }

        class SyncRequest : IReliableRequest, IRequest
        {
            bool aborted = false;
            bool completed = false;
            ManualResetEvent completedHandle;
            bool faulted = false;
            TimeSpan originalTimeout;
            Message reply;
            ReliableRequestSessionChannel parent;
            object thisLock = new object();

            public SyncRequest(ReliableRequestSessionChannel parent)
            {
                this.parent = parent;
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            public void Abort(RequestChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.aborted = true;
                        this.completed = true;

                        if (this.completedHandle != null)
                            this.completedHandle.Set();
                    }
                }
            }

            public void Fault(RequestChannel channel)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.faulted = true;
                        this.completed = true;

                        if (this.completedHandle != null)
                            this.completedHandle.Set();
                    }
                }
            }

            public void Complete()
            {
            }

            public void SendRequest(Message message, TimeSpan timeout)
            {
                this.originalTimeout = timeout;
                if (!parent.connection.AddMessage(message, timeout, this))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetInvalidAddException());
            }

            public void Set(Message reply)
            {
                lock (this.ThisLock)
                {
                    if (!this.completed)
                    {
                        this.reply = reply;
                        this.completed = true;

                        if (this.completedHandle != null)
                            this.completedHandle.Set();

                        return;
                    }
                }
                if (reply != null)
                {
                    reply.Close();
                }
            }

            public Message WaitForReply(TimeSpan timeout)
            {
                bool throwing = true;

                try
                {
                    bool expired = false;

                    if (!this.completed)
                    {
                        bool wait = false;

                        lock (this.ThisLock)
                        {
                            if (!this.completed)
                            {
                                wait = true;
                                this.completedHandle = new ManualResetEvent(false);
                            }
                        }

                        if (wait)
                        {
                            expired = !TimeoutHelper.WaitOne(this.completedHandle, timeout);

                            lock (this.ThisLock)
                            {
                                if (!this.completed)
                                {
                                    this.completed = true;
                                }
                                else
                                {
                                    expired = false;
                                }
                            }

                            this.completedHandle.Close();
                        }
                    }

                    if (this.aborted)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.CreateClosedException());
                    }
                    else if (this.faulted)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetTerminalException());
                    }
                    else if (expired)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.TimeoutOnRequest, this.originalTimeout)));
                    }
                    else
                    {
                        throwing = false;
                        return this.reply;
                    }
                }
                finally
                {
                    if (throwing)
                    {
                        WsrmFault fault = SequenceTerminatedFault.CreateCommunicationFault(this.parent.session.InputID,
                            SR.GetString(SR.SequenceTerminatedReliableRequestThrew), null);
                        this.parent.session.OnLocalFault(null, fault, null);
                        if (this.completedHandle != null)
                            this.completedHandle.Close();
                    }
                }
            }

            public void OnReleaseRequest()
            {                
            }
        }

        class AsyncRequest : AsyncResult, IReliableRequest, IAsyncRequest
        {
            bool completed = false;
            ReliableRequestSessionChannel parent;
            Message reply;
            bool set = false;
            object thisLock = new object();

            public AsyncRequest(ReliableRequestSessionChannel parent, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            public void Abort(RequestChannel channel)
            {
                if (this.ShouldComplete())
                {
                    this.Complete(false, parent.CreateClosedException());
                }
            }

            public void Fault(RequestChannel channel)
            {
                if (this.ShouldComplete())
                {
                    this.Complete(false, parent.GetTerminalException());
                }
            }

            void AddCompleted(IAsyncResult result)
            {
                Exception completeException = null;

                try
                {
                    if (!parent.connection.EndAddMessage(result))
                        completeException = this.parent.GetInvalidAddException();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeException = e;
                }

                if (completeException != null && this.ShouldComplete())
                    this.Complete(result.CompletedSynchronously, completeException);
            }

            public void BeginSendRequest(Message message, TimeSpan timeout)
            {
                parent.connection.BeginAddMessage(message, timeout, this, Fx.ThunkCallback(new AsyncCallback(AddCompleted)), null);
            }

            public void Complete()
            {
                if (this.ShouldComplete())
                {
                    this.Complete(false, null);
                }
            }

            public Message End()
            {
                AsyncResult.End<AsyncRequest>(this);
                return this.reply;
            }

            public void Set(Message reply)
            {
                lock (this.ThisLock)
                {
                    if (!this.set)
                    {
                        this.reply = reply;
                        this.set = true;
                        return;
                    }
                }

                if (reply != null)
                {
                    reply.Close();
                }
            }

            bool ShouldComplete()
            {
                lock (this.ThisLock)
                {
                    if (this.completed)
                    {
                        return false;
                    }

                    this.completed = true;
                }

                return true;
            }

            public void OnReleaseRequest()
            {                
            }
        }
    }
}
