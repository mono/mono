//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    abstract class ReliableOutputSessionChannel : OutputChannel, IOutputSessionChannel
    {
        IClientReliableChannelBinder binder;
        ChannelParameterCollection channelParameters;
        ReliableRequestor closeRequestor;
        ReliableOutputConnection connection;
        Exception maxRetryCountException = null;
        ClientReliableSession session;
        IReliableFactorySettings settings;
        ReliableRequestor terminateRequestor;

        protected ReliableOutputSessionChannel(
            ChannelManagerBase factory,
            IReliableFactorySettings settings,
            IClientReliableChannelBinder binder,
            FaultHelper faultHelper,
            LateBoundChannelParameterCollection channelParameters)
            : base(factory)
        {
            this.settings = settings;
            this.binder = binder;
            this.session = new ClientReliableSession(this, settings, binder, faultHelper, null);
            this.session.PollingCallback = this.PollingCallback;
            this.session.UnblockChannelCloseCallback = this.UnblockClose;
            this.binder.Faulted += OnBinderFaulted;
            this.binder.OnException += OnBinderException;

            this.channelParameters = channelParameters;
            channelParameters.SetChannel(this);
        }

        protected IReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        protected ReliableOutputConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected Exception MaxRetryCountException
        {
            set
            {
                this.maxRetryCountException = value;
            }
        }

        protected ChannelReliableSession ReliableSession
        {
            get
            {
                return this.session;
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.binder.RemoteAddress;
            }
        }

        protected abstract bool RequestAcks
        {
            get;
        }

        public IOutputSession Session
        {
            get
            {
                return this.session;
            }
        }

        public override Uri Via
        {
            get
            {
                return this.binder.Via;
            }
        }

        protected IReliableFactorySettings Settings
        {
            get
            {
                return this.settings;
            }
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
            requestor.MessageVersion = this.settings.MessageVersion;
            requestor.Binder = this.binder;
            requestor.SetRequestResponsePattern();
        }

        void CreateCloseRequestor()
        {
            ReliableRequestor temp = this.CreateRequestor();
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

        protected abstract ReliableRequestor CreateRequestor();

        void CreateTerminateRequestor()
        {
            ReliableRequestor temp = this.CreateRequestor();
            this.ConfigureRequestor(temp);
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;
            temp.MessageAction = WsrmIndex.GetTerminateSequenceActionHeader(
                this.settings.MessageVersion.Addressing, reliableMessagingVersion);
            temp.MessageBody = new TerminateSequence(reliableMessagingVersion, this.session.OutputID,
                this.connection.Last);

            lock (this.ThisLock)
            {
                this.ThrowIfClosed();
                this.terminateRequestor = temp;
                this.session.CloseSession();
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IOutputSessionChannel))
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

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            OperationWithTimeoutBeginCallback[] beginCallbacks = new OperationWithTimeoutBeginCallback[] 
            {
                this.connection.BeginClose,
                wsrm11 ? this.BeginCloseSequence : default(OperationWithTimeoutBeginCallback),
                this.BeginTerminateSequence,
                this.session.BeginClose
            };

            OperationEndCallback[] endCallbacks = new OperationEndCallback[] 
            {
                this.connection.EndClose,
                wsrm11 ? this.EndCloseSequence : default(OperationEndCallback),
                this.EndTerminateSequence,
                this.session.EndClose
            };

            return new ReliableChannelCloseAsyncResult(beginCallbacks, endCallbacks, this.binder,
                timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReliableChannelOpenAsyncResult(this.binder, this.session, timeout,
                callback, state);
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.connection.BeginAddMessage(message, timeout, null, callback, state);
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

        protected abstract void OnConnectionSend(Message message, TimeSpan timeout, bool saveHandledException,
            bool maskUnhandledException);
        protected abstract IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo, TimeSpan timeout,
            bool maskUnhandledException, AsyncCallback callback, object state);
        protected abstract void OnConnectionEndSend(IAsyncResult result);

        void OnConnectionSendAckRequestedHandler(TimeSpan timeout)
        {
            this.session.OnLocalActivity();
            using (Message message = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.OnConnectionSend(message, timeout, false, true);
            }
        }

        IAsyncResult OnConnectionBeginSendAckRequestedHandler(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.session.OnLocalActivity();
            Message request = WsrmUtilities.CreateAckRequestedMessage(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion, this.ReliableSession.OutputID);

            return this.OnConnectionBeginSendMessage(request, timeout, callback, state);
        }

        void OnConnectionEndSendAckRequestedHandler(IAsyncResult result)
        {
            this.OnConnectionEndSendMessage(result);
        }

        void OnConnectionSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException)
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
                }
                else
                {
                    this.session.OnLocalActivity();
                    OnConnectionSend(attemptInfo.Message, timeout,
                        (attemptInfo.RetryCount == this.settings.MaxRetryCount), maskUnhandledException);
                }
            }
        }

        IAsyncResult OnConnectionBeginSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
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
                return this.OnConnectionBeginSend(attemptInfo, timeout, maskUnhandledException, callback, state);
            }
        }

        void OnConnectionEndSendHandler(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
                CompletedAsyncResult.End(result);
            else
                OnConnectionEndSend(result);
        }

        protected abstract void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode);
        protected abstract IAsyncResult OnConnectionBeginSendMessage(Message message, TimeSpan timeout,
            AsyncCallback callback, object state);
        protected abstract void OnConnectionEndSendMessage(IAsyncResult result);

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
            ReliableChannelCloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ReliableChannelOpenAsyncResult.End(result);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            if (!this.connection.EndAddMessage(result))
                this.ThrowInvalidAddException();
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
                    this.Binder.Close(timeoutHelper.RemainingTime());
                }
            }
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            if (!this.connection.AddMessage(message, timeout, null))
                this.ThrowInvalidAddException();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.connection = new ReliableOutputConnection(this.session.OutputID, this.Settings.MaxTransferWindowSize,
                this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, this.session.InitiationTime,
                this.RequestAcks, this.DefaultSendTimeout);
            this.connection.Faulted += OnComponentFaulted;
            this.connection.OnException += OnComponentException;
            this.connection.BeginSendHandler = OnConnectionBeginSendHandler;
            this.connection.EndSendHandler = OnConnectionEndSendHandler;
            this.connection.SendHandler = OnConnectionSendHandler;
            this.connection.BeginSendAckRequestedHandler = OnConnectionBeginSendAckRequestedHandler;
            this.connection.EndSendAckRequestedHandler = OnConnectionEndSendAckRequestedHandler;
            this.connection.SendAckRequestedHandler = OnConnectionSendAckRequestedHandler;
        }

        void PollingCallback()
        {
            using (Message request = WsrmUtilities.CreateAckRequestedMessage(this.Settings.MessageVersion,
                this.Settings.ReliableMessagingVersion, this.ReliableSession.OutputID))
            {
                this.OnConnectionSendMessage(request, this.DefaultSendTimeout, MaskingMode.All);
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

            ReliableRequestor requestor = close ? this.closeRequestor : this.terminateRequestor;
            WsrmMessageInfo info = requestor.GetInfo();

            // Some other thread has verified and cleaned up the reply, no more work to do.
            if (info != null)
            {
                return;
            }

            try
            {
                info = WsrmMessageInfo.Get(this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion,
                    this.binder.Channel, this.binder.GetInnerSession(), reply);
                this.ReliableSession.ProcessInfo(info, null, true);
                this.ReliableSession.VerifyDuplexProtocolElements(info, null, true);

                WsrmFault fault = close
                    ? WsrmUtilities.ValidateCloseSequenceResponse(this.session, requestor.MessageId, info,
                    this.connection.Last)
                    : WsrmUtilities.ValidateTerminateSequenceResponse(this.session, requestor.MessageId, info,
                    this.connection.Last);

                if (fault != null)
                {
                    this.ReliableSession.OnLocalFault(null, fault, null);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
                }
            }
            finally
            {
                reply.Close();
            }
        }

        protected void ProcessMessage(Message message)
        {
            bool closeMessage = true;
            WsrmMessageInfo messageInfo = WsrmMessageInfo.Get(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(), message);
            bool wsrm11 = this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            try
            {
                if (!this.session.ProcessInfo(messageInfo, null))
                {
                    closeMessage = false;
                    return;
                }

                if (!this.ReliableSession.VerifySimplexProtocolElements(messageInfo, null))
                {
                    closeMessage = false;
                    return;
                }

                bool final = false;

                if (messageInfo.AcknowledgementInfo != null)
                {
                    final = wsrm11 && messageInfo.AcknowledgementInfo.Final;
                    int bufferRemaining = -1;

                    if (this.settings.FlowControlEnabled)
                        bufferRemaining = messageInfo.AcknowledgementInfo.BufferRemaining;

                    this.connection.ProcessTransferred(messageInfo.AcknowledgementInfo.Ranges, bufferRemaining);
                }

                if (wsrm11)
                {
                    WsrmFault fault = null;

                    if (messageInfo.TerminateSequenceResponseInfo != null)
                    {
                        fault = WsrmUtilities.ValidateTerminateSequenceResponse(this.session,
                            this.terminateRequestor.MessageId, messageInfo, this.connection.Last);

                        if (fault == null)
                        {
                            fault = this.ProcessRequestorResponse(this.terminateRequestor, WsrmFeb2005Strings.TerminateSequence, messageInfo);
                        }
                    }
                    else if (messageInfo.CloseSequenceResponseInfo != null)
                    {
                        fault = WsrmUtilities.ValidateCloseSequenceResponse(this.session,
                            this.closeRequestor.MessageId, messageInfo, this.connection.Last);

                        if (fault == null)
                        {
                            fault = this.ProcessRequestorResponse(this.closeRequestor, Wsrm11Strings.CloseSequence, messageInfo);
                        }
                    }
                    else if (messageInfo.TerminateSequenceInfo != null)
                    {
                        if (!WsrmUtilities.ValidateWsrmRequest(this.session, messageInfo.TerminateSequenceInfo, this.binder, null))
                        {
                            return;
                        }

                        WsrmAcknowledgmentInfo ackInfo = messageInfo.AcknowledgementInfo;
                        fault = WsrmUtilities.ValidateFinalAckExists(this.session, ackInfo);

                        if ((fault == null) && !this.connection.IsFinalAckConsistent(ackInfo.Ranges))
                        {
                            fault = new InvalidAcknowledgementFault(this.session.OutputID, ackInfo.Ranges);
                        }

                        if (fault == null)
                        {
                            Message response = WsrmUtilities.CreateTerminateResponseMessage(
                                this.settings.MessageVersion,
                                messageInfo.TerminateSequenceInfo.MessageId,
                                this.session.OutputID);

                            try
                            {
                                this.OnConnectionSend(response, this.DefaultSendTimeout, false, true);
                            }
                            finally
                            {
                                response.Close();
                            }

                            this.session.OnRemoteFault(new ProtocolException(SR.GetString(SR.UnsupportedTerminateSequenceExceptionString)));
                            return;
                        }
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
                            fault = WsrmUtilities.ValidateFinalAck(this.session, messageInfo, this.connection.Last);

                            if (fault == null)
                            {
                                this.closeRequestor.SetInfo(messageInfo);
                            }
                        }
                    }
                    else if (messageInfo.WsrmHeaderFault != null)
                    {
                        if (!(messageInfo.WsrmHeaderFault is UnknownSequenceFault))
                        {
                            throw Fx.AssertAndThrow("Fault must be UnknownSequence fault.");
                        }

                        if (this.terminateRequestor == null)
                        {
                            throw Fx.AssertAndThrow("In wsrm11, if we start getting UnknownSequence, terminateRequestor cannot be null.");
                        }

                        this.terminateRequestor.SetInfo(messageInfo);
                    }

                    if (fault != null)
                    {
                        this.session.OnLocalFault(fault.CreateException(), fault, null);
                        return;
                    }
                }

                this.session.OnRemoteActivity(this.connection.Strategy.QuotaRemaining == 0);
            }
            finally
            {
                if (closeMessage)
                    messageInfo.Message.Close();
            }
        }

        protected abstract WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info);

        void TerminateSequence(TimeSpan timeout)
        {
            ReliableMessagingVersion reliableMessagingVersion = this.settings.ReliableMessagingVersion;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.session.CloseSession();
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion,
                    reliableMessagingVersion, this.session.OutputID);
                this.OnConnectionSendMessage(message, timeout, MaskingMode.Handled);
            }
            else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                this.CreateTerminateRequestor();
                Message terminateReply = this.terminateRequestor.Request(timeout);

                if (terminateReply != null)
                {
                    this.ProcessCloseOrTerminateReply(false, terminateReply);
                }
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
                this.session.CloseSession();
                Message message = WsrmUtilities.CreateTerminateMessage(this.settings.MessageVersion,
                    reliableMessagingVersion, this.session.OutputID);
                return this.OnConnectionBeginSendMessage(message, timeout, callback, state);
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
            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                this.OnConnectionEndSendMessage(result);
            }
            else
            {
                Message terminateReply = this.terminateRequestor.EndRequest(result);

                if (terminateReply != null)
                {
                    this.ProcessCloseOrTerminateReply(false, terminateReply);
                }
            }
        }

        void ThrowInvalidAddException()
        {
            if (this.State == CommunicationState.Faulted)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.GetTerminalException());
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateClosedException());
        }

        void UnblockClose()
        {
            if (this.connection != null)
            {
                this.connection.Fault(this);
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

    sealed class ReliableOutputSessionChannelOverRequest : ReliableOutputSessionChannel
    {
        IClientReliableChannelBinder binder;

        public ReliableOutputSessionChannelOverRequest(ChannelManagerBase factory, IReliableFactorySettings settings,
            IClientReliableChannelBinder binder, FaultHelper faultHelper,
            LateBoundChannelParameterCollection channelParameters)
            : base(factory, settings, binder, faultHelper, channelParameters)
        {
            this.binder = binder;
        }

        protected override bool RequestAcks
        {
            get
            {
                return false;
            }
        }

        protected override ReliableRequestor CreateRequestor()
        {
            return new RequestReliableRequestor();
        }

        protected override void OnConnectionSend(Message message, TimeSpan timeout,
            bool saveHandledException, bool maskUnhandledException)
        {
            MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;
            Message reply = null;

            if (saveHandledException)
            {
                try
                {
                    reply = this.binder.Request(message, timeout, maskingMode);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (this.Binder.IsHandleable(e))
                    {
                        this.MaxRetryCountException = e;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                maskingMode |= MaskingMode.Handled;
                reply = this.binder.Request(message, timeout, maskingMode);

                if (reply != null)
                    ProcessMessage(reply);
            }
        }

        protected override IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo,
            TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            ReliableBinderRequestAsyncResult result = new ReliableBinderRequestAsyncResult(callback, state);
            result.Binder = this.binder;
            result.MessageAttemptInfo = attemptInfo;
            result.MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

            if (attemptInfo.RetryCount < this.Settings.MaxRetryCount)
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

        protected override void OnConnectionEndSend(IAsyncResult result)
        {
            Exception handledException;
            Message reply = ReliableBinderRequestAsyncResult.End(result, out handledException);
            ReliableBinderRequestAsyncResult requestResult = (ReliableBinderRequestAsyncResult)result;
            if (requestResult.MessageAttemptInfo.RetryCount == this.Settings.MaxRetryCount)
            {
                this.MaxRetryCountException = handledException;
            }

            if (reply != null)
                ProcessMessage(reply);
        }

        protected override void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            Message reply = this.binder.Request(message, timeout, maskingMode);

            if (reply != null)
            {
                ProcessMessage(reply);
            }
        }

        protected override IAsyncResult OnConnectionBeginSendMessage(Message message, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            ReliableBinderRequestAsyncResult requestResult = new ReliableBinderRequestAsyncResult(callback, state);
            requestResult.Binder = this.binder;
            requestResult.MaskingMode = MaskingMode.Handled;
            requestResult.Message = message;

            requestResult.Begin(timeout);
            return requestResult;
        }

        protected override void OnConnectionEndSendMessage(IAsyncResult result)
        {
            Message reply = ReliableBinderRequestAsyncResult.End(result);

            if (reply != null)
            {
                this.ProcessMessage(reply);
            }
        }

        protected override WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info)
        {
            string faultString = SR.GetString(SR.ReceivedResponseBeforeRequestFaultString, requestName);
            string exceptionString = SR.GetString(SR.ReceivedResponseBeforeRequestExceptionString, requestName);
            return SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.OutputID, faultString, exceptionString);
        }
    }

    sealed class ReliableOutputSessionChannelOverDuplex : ReliableOutputSessionChannel
    {
        static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompletedStatic));

        public ReliableOutputSessionChannelOverDuplex(ChannelManagerBase factory, IReliableFactorySettings settings,
            IClientReliableChannelBinder binder, FaultHelper faultHelper,
            LateBoundChannelParameterCollection channelParameters)
            : base(factory, settings, binder, faultHelper, channelParameters)
        {
        }

        protected override bool RequestAcks
        {
            get
            {
                return true;
            }
        }

        protected override ReliableRequestor CreateRequestor()
        {
            return new SendWaitReliableRequestor();
        }

        protected override void OnConnectionSend(Message message, TimeSpan timeout, bool saveHandledException, bool maskUnhandledException)
        {
            MaskingMode maskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

            if (saveHandledException)
            {
                try
                {
                    this.Binder.Send(message, timeout, maskingMode);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (this.Binder.IsHandleable(e))
                    {
                        this.MaxRetryCountException = e;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                maskingMode |= MaskingMode.Handled;
                this.Binder.Send(message, timeout, maskingMode);
            }
        }

        protected override IAsyncResult OnConnectionBeginSend(MessageAttemptInfo attemptInfo,
            TimeSpan timeout, bool maskUnhandledException, AsyncCallback callback, object state)
        {
            ReliableBinderSendAsyncResult result = new ReliableBinderSendAsyncResult(callback, state);
            result.Binder = this.Binder;
            result.MessageAttemptInfo = attemptInfo;
            result.MaskingMode = maskUnhandledException ? MaskingMode.Unhandled : MaskingMode.None;

            if (attemptInfo.RetryCount < this.Settings.MaxRetryCount)
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

        protected override void OnConnectionEndSend(IAsyncResult result)
        {
            Exception handledException;
            ReliableBinderSendAsyncResult.End(result, out handledException);
            ReliableBinderSendAsyncResult sendResult = (ReliableBinderSendAsyncResult)result;
            if (sendResult.MessageAttemptInfo.RetryCount == this.Settings.MaxRetryCount)
            {
                this.MaxRetryCountException = handledException;
            }
        }

        protected override void OnConnectionSendMessage(Message message, TimeSpan timeout, MaskingMode maskingMode)
        {
            this.Binder.Send(message, timeout, maskingMode);
        }

        protected override IAsyncResult OnConnectionBeginSendMessage(Message message, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            ReliableBinderSendAsyncResult sendResult = new ReliableBinderSendAsyncResult(callback, state);
            sendResult.Binder = this.Binder;
            sendResult.MaskingMode = MaskingMode.Unhandled;
            sendResult.Message = message;
            sendResult.Begin(timeout);

            return sendResult;
        }

        protected override void OnConnectionEndSendMessage(IAsyncResult result)
        {
            ReliableBinderSendAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                try
                {
                    this.StartReceiving();
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
                ActionItem.Schedule(new Action<object>(StartReceiving), this);
            }
        }

        static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            ReliableOutputSessionChannelOverDuplex channel = (ReliableOutputSessionChannelOverDuplex)result.AsyncState;

            try
            {
                channel.OnReceiveCompleted(result);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                channel.ReliableSession.OnUnknownException(e);
            }
        }

        void OnReceiveCompleted(IAsyncResult result)
        {
            RequestContext context;
            if (this.Binder.EndTryReceive(result, out context))
            {
                if (context != null)
                {
                    using (context)
                    {
                        Message requestMessage = context.RequestMessage;
                        ProcessMessage(requestMessage);
                        context.Close(this.DefaultCloseTimeout);
                    }
                    this.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
                }
                else
                {
                    if (!this.Connection.Closed && (this.Binder.State == CommunicationState.Opened))
                    {
                        Exception e = new CommunicationException(SR.GetString(SR.EarlySecurityClose));
                        this.ReliableSession.OnLocalFault(e, (Message)null, null);
                    }
                }
            }
            else
            {
                this.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
            }
        }

        protected override WsrmFault ProcessRequestorResponse(ReliableRequestor requestor, string requestName, WsrmMessageInfo info)
        {
            if (requestor != null)
            {
                requestor.SetInfo(info);
                return null;
            }
            else
            {
                string faultString = SR.GetString(SR.ReceivedResponseBeforeRequestFaultString, requestName);
                string exceptionString = SR.GetString(SR.ReceivedResponseBeforeRequestExceptionString, requestName);
                return SequenceTerminatedFault.CreateProtocolFault(this.ReliableSession.OutputID, faultString, exceptionString);
            }
        }

        void StartReceiving()
        {
            this.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
        }

        static void StartReceiving(object state)
        {
            ReliableOutputSessionChannelOverDuplex channel =
                (ReliableOutputSessionChannelOverDuplex)state;

            try
            {
                channel.StartReceiving();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                channel.ReliableSession.OnUnknownException(e);
            }
        }
    }
}
