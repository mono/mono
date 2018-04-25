//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//--------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    abstract class ChannelReliableSession : ISession
    {
        IReliableChannelBinder binder;
        bool canSendFault = true;
        ChannelBase channel;
        SessionFaultState faulted = SessionFaultState.NotFaulted;
        FaultHelper faultHelper;
        SequenceRangeCollection finalRanges;
        Guard guard = new Guard(int.MaxValue);
        InterruptibleTimer inactivityTimer;
        TimeSpan initiationTime;
        UniqueId inputID;
        bool isSessionClosed = false;
        UniqueId outputID;
        RequestContext replyFaultContext;
        IReliableFactorySettings settings;
        Message terminatingFault;
        object thisLock = new object();
        UnblockChannelCloseHandler unblockChannelCloseCallback;

        protected ChannelReliableSession(ChannelBase channel, IReliableFactorySettings settings, IReliableChannelBinder binder, FaultHelper faultHelper)
        {
            this.channel = channel;
            this.settings = settings;
            this.binder = binder;
            this.faultHelper = faultHelper;
            this.inactivityTimer = new InterruptibleTimer(this.settings.InactivityTimeout, new WaitCallback(this.OnInactivityElapsed), null);
            this.initiationTime = ReliableMessagingConstants.UnknownInitiationTime;
        }

        protected ChannelBase Channel
        {
            get
            {
                return this.channel;
            }
        }

        protected Guard Guard
        {
            get
            {
                return this.guard;
            }
        }

        public string Id
        {
            get
            {
                UniqueId sequenceId = this.SequenceID;
                if (sequenceId == null)
                    return null;
                else
                    return sequenceId.ToString();
            }
        }

        public TimeSpan InitiationTime
        {
            get
            {
                return this.initiationTime;
            }
            protected set
            {
                this.initiationTime = value;
            }
        }

        public UniqueId InputID
        {
            get
            {
                return this.inputID;
            }
            protected set
            {
                this.inputID = value;
            }
        }

        protected FaultHelper FaultHelper
        {
            get
            {
                return this.faultHelper;
            }
        }

        public UniqueId OutputID
        {
            get
            {
                return this.outputID;
            }
            protected set
            {
                this.outputID = value;
            }
        }

        public abstract UniqueId SequenceID
        {
            get;
        }

        public IReliableFactorySettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public UnblockChannelCloseHandler UnblockChannelCloseCallback
        {
            set
            {
                this.unblockChannelCloseCallback = value;
            }
        }

        public virtual void Abort()
        {
            this.guard.Abort();
            this.inactivityTimer.Abort();

            // Try to send a fault.
            bool sendFault;
            lock (this.ThisLock)
            {
                // Faulted thread already cleaned up. No need to to anything more.
                if (this.faulted == SessionFaultState.CleanedUp)
                    return;

                // Can only send a fault if the other side did not send one already.
                sendFault = this.canSendFault && (this.faulted != SessionFaultState.RemotelyFaulted);    // NotFaulted || LocallyFaulted
                this.faulted = SessionFaultState.CleanedUp;
            }

            if (sendFault)
            {
                if ((this.binder.State == CommunicationState.Opened)
                    && this.binder.Connected
                    && (this.binder.CanSendAsynchronously || (this.replyFaultContext != null)))
                {
                    if (this.terminatingFault == null)
                    {
                        UniqueId sequenceId = this.InputID ?? this.OutputID;
                        if (sequenceId != null)
                        {
                            WsrmFault fault = SequenceTerminatedFault.CreateCommunicationFault(sequenceId, SR.GetString(SR.SequenceTerminatedOnAbort), null);
                            this.terminatingFault = fault.CreateMessage(this.settings.MessageVersion,
                                this.settings.ReliableMessagingVersion);
                        }
                    }

                    if (this.terminatingFault != null)
                    {
                        this.AddFinalRanges();
                        this.faultHelper.SendFaultAsync(this.binder, this.replyFaultContext, this.terminatingFault);
                        return;
                    }
                }
            }

            // Got here so the session did not actually send a fault, must clean up resources.
            if (this.terminatingFault != null)
                this.terminatingFault.Close();
            if (this.replyFaultContext != null)
                this.replyFaultContext.Abort();
            this.binder.Abort();
        }

        void AddFinalRanges()
        {
            // This relies on the assumption that acknowledgements can be piggybacked on sequence
            // faults for the converse sequence.
            if (this.finalRanges != null)
            {
                WsrmUtilities.AddAcknowledgementHeader(this.settings.ReliableMessagingVersion,
                    this.terminatingFault, this.InputID, this.finalRanges, true);
            }
        }

        public virtual IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.guard.BeginClose(timeout, callback, state);
        }

        public abstract IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);

        public virtual void Close(TimeSpan timeout)
        {
            this.guard.Close(timeout);
            this.inactivityTimer.Abort();
        }

        // Corresponds to the state where the other side could have gone away already.
        public void CloseSession()
        {
            this.isSessionClosed = true;
        }

        public virtual void EndClose(IAsyncResult result)
        {
            this.guard.EndClose(result);
            this.inactivityTimer.Abort();
        }

        public abstract void EndOpen(IAsyncResult result);

        protected virtual void FaultCore()
        {

            if (TD.ReliableSessionChannelFaultedIsEnabled())
            {
                TD.ReliableSessionChannelFaulted(this.Id);
            }

            this.inactivityTimer.Abort();
        }

        public void OnLocalFault(Exception e, WsrmFault fault, RequestContext context)
        {
            Message faultMessage = (fault == null) ? null : fault.CreateMessage(this.settings.MessageVersion,
                this.settings.ReliableMessagingVersion);
            this.OnLocalFault(e, faultMessage, context);
        }

        public void OnLocalFault(Exception e, Message faultMessage, RequestContext context)
        {
            if (this.channel.Aborted ||
                this.channel.State == CommunicationState.Faulted ||
                this.channel.State == CommunicationState.Closed)
            {
                if (faultMessage != null)
                    faultMessage.Close();
                if (context != null)
                    context.Abort();
                return;
            }

            lock (this.ThisLock)
            {
                if (this.faulted != SessionFaultState.NotFaulted)
                    return;
                this.faulted = SessionFaultState.LocallyFaulted;
                this.terminatingFault = faultMessage;
                this.replyFaultContext = context;
            }

            this.FaultCore();
            this.channel.Fault(e);
            this.UnblockChannelIfNecessary();
        }

        public void OnRemoteFault(WsrmFault fault)
        {
            this.OnRemoteFault(WsrmFault.CreateException(fault));
        }

        public void OnRemoteFault(Exception e)
        {
            if (this.channel.Aborted ||
                this.channel.State == CommunicationState.Faulted ||
                this.channel.State == CommunicationState.Closed)
            {
                return;
            }

            lock (this.ThisLock)
            {
                if (this.faulted != SessionFaultState.NotFaulted)
                    return;
                this.faulted = SessionFaultState.RemotelyFaulted;
            }

            this.FaultCore();
            this.channel.Fault(e);
            this.UnblockChannelIfNecessary();
        }

        public virtual void OnFaulted()
        {
            this.FaultCore();

            // Try to send a fault.
            bool sendFault;
            lock (this.ThisLock)
            {
                // Channel was faulted without the session being told first (e.g. open throws).
                // The session does not know what fault to send so let abort send it if it can.
                if (this.faulted == SessionFaultState.NotFaulted)
                    return;

                // Abort thread decided to clean up.
                if (this.faulted == SessionFaultState.CleanedUp)
                    return;

                // Can only send a fault if the other side did not send one already.
                sendFault = this.canSendFault && (this.faulted != SessionFaultState.RemotelyFaulted);  // LocallyFaulted
                this.faulted = SessionFaultState.CleanedUp;
            }

            if (sendFault)
            {
                if ((this.binder.State == CommunicationState.Opened)
                    && this.binder.Connected
                    && (this.binder.CanSendAsynchronously || (this.replyFaultContext != null))
                    && (this.terminatingFault != null))
                {
                    this.AddFinalRanges();
                    this.faultHelper.SendFaultAsync(this.binder, this.replyFaultContext, this.terminatingFault);
                    return;
                }
            }

            // Got here so the session did not actually send a fault, must clean up resources.
            if (this.terminatingFault != null)
                this.terminatingFault.Close();
            if (this.replyFaultContext != null)
                this.replyFaultContext.Abort();
            this.binder.Abort();
        }

        void OnInactivityElapsed(object state)
        {
            WsrmFault fault;
            Exception e;
            string exceptionMessage = SR.GetString(SR.SequenceTerminatedInactivityTimeoutExceeded, this.settings.InactivityTimeout);

            if (TD.InactivityTimeoutIsEnabled())
            {
                TD.InactivityTimeout(exceptionMessage);
            }

            if (this.SequenceID != null)
            {
                string faultReason = SR.GetString(SR.SequenceTerminatedInactivityTimeoutExceeded, this.settings.InactivityTimeout);
                fault = SequenceTerminatedFault.CreateCommunicationFault(this.SequenceID, faultReason, exceptionMessage);
                e = fault.CreateException();
            }
            else
            {
                fault = null;
                e = new CommunicationException(exceptionMessage);
            }

            OnLocalFault(e, fault, null);
        }

        public abstract void OnLocalActivity();

        public void OnUnknownException(Exception e)
        {
            this.canSendFault = false;
            this.OnLocalFault(e, (Message)null, null);
        }

        public abstract void Open(TimeSpan timeout);

        public virtual void OnRemoteActivity(bool fastPolling)
        {
            this.inactivityTimer.Set();
        }

        // returns true if the info does not fault the session.
        public bool ProcessInfo(WsrmMessageInfo info, RequestContext context)
        {
            return this.ProcessInfo(info, context, false);
        }

        public bool ProcessInfo(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            Exception e;
            if (info.ParsingException != null)
            {
                WsrmFault fault;

                if (this.SequenceID != null)
                {
                    string reason = SR.GetString(SR.CouldNotParseWithAction, info.Action);
                    fault = SequenceTerminatedFault.CreateProtocolFault(this.SequenceID, reason, null);
                }
                else
                {
                    fault = null;
                }

                e = new ProtocolException(SR.GetString(SR.MessageExceptionOccurred), info.ParsingException);
                this.OnLocalFault(throwException ? null : e, fault, context);
            }
            else if (info.FaultReply != null)
            {
                e = info.FaultException;
                this.OnLocalFault(throwException ? null : e, info.FaultReply, context);
            }
            else if ((info.WsrmHeaderFault != null) && (info.WsrmHeaderFault.SequenceID != this.InputID)
                && (info.WsrmHeaderFault.SequenceID != this.OutputID))
            {
                e = new ProtocolException(SR.GetString(SR.WrongIdentifierFault, FaultException.GetSafeReasonText(info.WsrmHeaderFault.Reason)));
                this.OnLocalFault(throwException ? null : e, (Message)null, context);
            }
            else if (info.FaultInfo != null)
            {
                if (this.isSessionClosed)
                {
                    UnknownSequenceFault unknownSequenceFault = info.FaultInfo as UnknownSequenceFault;

                    if (unknownSequenceFault != null)
                    {
                        UniqueId sequenceId = unknownSequenceFault.SequenceID;

                        if (((this.OutputID != null) && (this.OutputID == sequenceId))
                            || ((this.InputID != null) && (this.InputID == sequenceId)))
                        {
                            if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                            {
                                info.Message.Close();
                                return false;
                            }
                            else if (this.settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                            {
                                return true;
                            }
                            else
                            {
                                throw Fx.AssertAndThrow("Unknown version.");
                            }
                        }
                    }
                }

                e = info.FaultException;
                if (context != null)
                    context.Close();
                this.OnRemoteFault(throwException ? null : e);
            }
            else
            {
                return true;
            }

            info.Message.Close();
            if (throwException)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
            else
                return false;
        }

        public void SetFinalAck(SequenceRangeCollection finalRanges)
        {
            this.finalRanges = finalRanges;
        }

        public virtual void StartInactivityTimer()
        {
            this.inactivityTimer.Set();
        }

        // RM channels fault out of band. During the Closing and Closed states CommunicationObjects
        // do not fault. In all other states the RM channel can and must unblock various methods
        // from the OnFaulted method. This method will ensure that anything that needs to unblock
        // in the Closing state will unblock if a fault occurs.
        void UnblockChannelIfNecessary()
        {
            lock (this.ThisLock)
            {
                if (this.faulted == SessionFaultState.NotFaulted)
                {
                    throw Fx.AssertAndThrow("This method must be called from a fault thread.");
                }
                // Successfully faulted or aborted.
                else if (this.faulted == SessionFaultState.CleanedUp)
                {
                    return;
                }
            }

            // Make sure the fault is sent then unblock the channel.
            this.OnFaulted();
            this.unblockChannelCloseCallback();
        }

        public bool VerifyDuplexProtocolElements(WsrmMessageInfo info, RequestContext context)
        {
            return this.VerifyDuplexProtocolElements(info, context, false);
        }

        public bool VerifyDuplexProtocolElements(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            WsrmFault fault = this.VerifyDuplexProtocolElements(info);

            if (fault == null)
            {
                return true;
            }

            if (throwException)
            {
                Exception e = fault.CreateException();
                this.OnLocalFault(null, fault, context);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
            }
            else
            {
                this.OnLocalFault(fault.CreateException(), fault, context);
                return false;
            }
        }

        protected virtual WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null && info.AcknowledgementInfo.SequenceID != this.OutputID)
                return new UnknownSequenceFault(info.AcknowledgementInfo.SequenceID);
            else if (info.AckRequestedInfo != null && info.AckRequestedInfo.SequenceID != this.InputID)
                return new UnknownSequenceFault(info.AckRequestedInfo.SequenceID);
            else if (info.SequencedMessageInfo != null && info.SequencedMessageInfo.SequenceID != this.InputID)
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            else if (info.TerminateSequenceInfo != null && info.TerminateSequenceInfo.Identifier != this.InputID)
            {
                if (this.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedTerminateSequence), SR.GetString(SR.UnexpectedTerminateSequence));
                else if (info.TerminateSequenceInfo.Identifier == this.OutputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            }
            else if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);

                if (info.TerminateSequenceResponseInfo.Identifier == this.OutputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            else if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);

                if (info.CloseSequenceInfo.Identifier == this.InputID)
                    return null;
                else if (info.CloseSequenceInfo.Identifier == this.OutputID)
                    // Spec allows RM-Destination close, but we do not.
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnsupportedClose), SR.GetString(SR.UnsupportedCloseExceptionString));
                else
                    return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            else if (info.CloseSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.settings.ReliableMessagingVersion);

                if (info.CloseSequenceResponseInfo.Identifier == this.OutputID)
                    return null;
                else if (info.CloseSequenceResponseInfo.Identifier == this.InputID)
                    return SequenceTerminatedFault.CreateProtocolFault(this.InputID, SR.GetString(SR.SequenceTerminatedUnexpectedCloseSequenceResponse), SR.GetString(SR.UnexpectedCloseSequenceResponse));
                else
                    return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
            }
            else
                return null;
        }

        public bool VerifySimplexProtocolElements(WsrmMessageInfo info, RequestContext context)
        {
            return this.VerifySimplexProtocolElements(info, context, false);
        }

        public bool VerifySimplexProtocolElements(WsrmMessageInfo info, RequestContext context, bool throwException)
        {
            WsrmFault fault = this.VerifySimplexProtocolElements(info);

            if (fault == null)
            {
                return true;
            }

            info.Message.Close();

            if (throwException)
            {
                Exception e = fault.CreateException();
                this.OnLocalFault(null, fault, context);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
            }
            else
            {
                this.OnLocalFault(fault.CreateException(), fault, context);
                return false;
            }
        }

        protected abstract WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info);

        enum SessionFaultState
        {
            NotFaulted,
            LocallyFaulted,
            RemotelyFaulted,
            CleanedUp
        }

        public delegate void UnblockChannelCloseHandler();
    }

    class ClientReliableSession : ChannelReliableSession, IOutputSession
    {
        IClientReliableChannelBinder binder;
        PollingMode oldPollingMode;
        PollingHandler pollingHandler;
        PollingMode pollingMode;
        InterruptibleTimer pollingTimer;
        ReliableRequestor requestor;

        public delegate void PollingHandler();

        public ClientReliableSession(ChannelBase channel, IReliableFactorySettings factory, IClientReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID) :
            base(channel, factory, binder, faultHelper)
        {
            this.binder = binder;
            this.InputID = inputID;
            this.pollingTimer = new InterruptibleTimer(this.GetPollingInterval(), this.OnPollingTimerElapsed, null);

            if (this.binder.Channel is IRequestChannel)
            {
                this.requestor = new RequestReliableRequestor();
            }
            else if (this.binder.Channel is IDuplexChannel)
            {
                SendReceiveReliableRequestor sendReceiveRequestor = new SendReceiveReliableRequestor();
                sendReceiveRequestor.TimeoutIsSafe = !this.ChannelSupportsOneCreateSequenceAttempt();
                this.requestor = sendReceiveRequestor;
            }
            else
            {
                Fx.Assert("This channel type is not supported");
            }

            MessageVersion messageVersion = this.Settings.MessageVersion;
            ReliableMessagingVersion reliableMessagingVersion = this.Settings.ReliableMessagingVersion;
            this.requestor.MessageVersion = messageVersion;
            this.requestor.Binder = this.binder;
            this.requestor.IsCreateSequence = true;
            this.requestor.TimeoutString1Index = SR.TimeoutOnOpen;
            this.requestor.MessageAction = WsrmIndex.GetCreateSequenceActionHeader(messageVersion.Addressing,
                reliableMessagingVersion);
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                && (this.binder.GetInnerSession() is ISecureConversationSession))
            {
                this.requestor.MessageHeader = new WsrmUsesSequenceSTRHeader();
            }
            this.requestor.MessageBody = new CreateSequence(this.Settings.MessageVersion.Addressing,
                reliableMessagingVersion, this.Settings.Ordered, this.binder, this.InputID);
            this.requestor.SetRequestResponsePattern();
        }

        public PollingHandler PollingCallback
        {
            set
            {
                this.pollingHandler = value;
            }
        }

        public override UniqueId SequenceID
        {
            get
            {
                return this.OutputID;
            }
        }

        public override void Abort()
        {
            ReliableRequestor temp = this.requestor;

            if (temp != null)
                temp.Abort(this.Channel);
            pollingTimer.Abort();
            base.Abort();
        }

        public override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.pollingHandler == null)
            {
                throw Fx.AssertAndThrow("The client reliable channel must set the polling handler prior to opening the client reliable session.");
            }

            return new OpenAsyncResult(this, timeout, callback, state);
        }

        bool ChannelSupportsOneCreateSequenceAttempt()
        {
            IDuplexSessionChannel channel = this.binder.Channel as IDuplexSessionChannel;

            if (channel == null)
                return false;

            return (channel.Session is ISecuritySession && !(channel.Session is ISecureConversationSession));
        }

        public override void Close(TimeSpan timeout)
        {
            base.Close(timeout);
            pollingTimer.Abort();
        }

        public override void EndClose(IAsyncResult result)
        {
            base.EndClose(result);
            pollingTimer.Abort();
        }

        public override void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
            this.requestor = null;
        }

        protected override void FaultCore()
        {
            this.pollingTimer.Abort();
            base.FaultCore();
        }

        TimeSpan GetPollingInterval()
        {
            switch (this.pollingMode)
            {
                case PollingMode.Idle:
                    return Ticks.ToTimeSpan(Ticks.FromTimeSpan(this.Settings.InactivityTimeout) / 2);

                case PollingMode.KeepAlive:
                    return WsrmUtilities.CalculateKeepAliveInterval(this.Settings.InactivityTimeout, this.Settings.MaxRetryCount);

                case PollingMode.NotPolling:
                    return TimeSpan.MaxValue;

                case PollingMode.FastPolling:
                    TimeSpan keepAliveInterval = WsrmUtilities.CalculateKeepAliveInterval(this.Settings.InactivityTimeout, this.Settings.MaxRetryCount);
                    TimeSpan fastPollingInterval = Ticks.ToTimeSpan(Ticks.FromTimeSpan(this.binder.DefaultSendTimeout) / 2);

                    if (fastPollingInterval < keepAliveInterval)
                        return fastPollingInterval;
                    else
                        return keepAliveInterval;

                default:
                    throw Fx.AssertAndThrow("Unknown polling mode.");
            }
        }

        public override void OnFaulted()
        {
            base.OnFaulted();

            ReliableRequestor temp = this.requestor;

            if (temp != null)
                this.requestor.Fault(this.Channel);
        }

        void OnPollingTimerElapsed(object state)
        {
            if (this.Guard.Enter())
            {
                try
                {
                    lock (this.ThisLock)
                    {
                        if (this.pollingMode == PollingMode.NotPolling)
                            return;

                        if (this.pollingMode == PollingMode.Idle)
                            this.pollingMode = PollingMode.KeepAlive;
                    }

                    this.pollingHandler();
                    this.pollingTimer.Set(this.GetPollingInterval());
                }
                finally
                {
                    this.Guard.Exit();
                }
            }
        }

        public override void OnLocalActivity()
        {
            lock (this.ThisLock)
            {
                if (this.pollingMode == PollingMode.NotPolling)
                    return;

                this.pollingTimer.Set(this.GetPollingInterval());
            }
        }

        public override void Open(TimeSpan timeout)
        {
            if (this.pollingHandler == null)
            {
                throw Fx.AssertAndThrow("The client reliable channel must set the polling handler prior to opening the client reliable session.");
            }

            DateTime start = DateTime.UtcNow;
            Message response = this.requestor.Request(timeout);
            this.ProcessCreateSequenceResponse(response, start);
            this.requestor = null;
        }

        public override void OnRemoteActivity(bool fastPolling)
        {
            base.OnRemoteActivity(fastPolling);
            lock (this.ThisLock)
            {
                if (this.pollingMode == PollingMode.NotPolling)
                    return;

                if (fastPolling)
                    this.pollingMode = PollingMode.FastPolling;
                else
                    this.pollingMode = PollingMode.Idle;

                this.pollingTimer.Set(this.GetPollingInterval());
            }
        }

        void ProcessCreateSequenceResponse(Message response, DateTime start)
        {
            CreateSequenceResponseInfo createResponse = null;

            using (response)
            {
                if (response.IsFault)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmUtilities.CreateCSFaultException(
                        this.Settings.MessageVersion, this.Settings.ReliableMessagingVersion, response,
                        this.binder.Channel));
                }
                else
                {
                    WsrmMessageInfo info = WsrmMessageInfo.Get(this.Settings.MessageVersion,
                        this.Settings.ReliableMessagingVersion, this.binder.Channel, this.binder.GetInnerSession(),
                        response, true);

                    if (info.ParsingException != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.UnparsableCSResponse), info.ParsingException));

                    // this throws and sends a fault if something is wrong with the info
                    this.ProcessInfo(info, null, true);
                    createResponse = info.CreateSequenceResponseInfo;

                    string exceptionReason = null;
                    string faultReason = null;

                    if (createResponse == null)
                    {
                        exceptionReason = SR.GetString(SR.InvalidWsrmResponseChannelNotOpened,
                            WsrmFeb2005Strings.CreateSequence, info.Action,
                            WsrmIndex.GetCreateSequenceResponseActionString(this.Settings.ReliableMessagingVersion));
                    }
                    else if (!object.Equals(createResponse.RelatesTo, this.requestor.MessageId))
                    {
                        exceptionReason = SR.GetString(SR.WsrmMessageWithWrongRelatesToExceptionString, WsrmFeb2005Strings.CreateSequence);
                        faultReason = SR.GetString(SR.WsrmMessageWithWrongRelatesToFaultString, WsrmFeb2005Strings.CreateSequence);
                    }
                    else if ((createResponse.AcceptAcksTo == null) && (this.InputID != null))
                    {
                        if (this.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                        {
                            exceptionReason = SR.GetString(SR.CSResponseWithoutOffer);
                            faultReason = SR.GetString(SR.CSResponseWithoutOfferReason);
                        }
                        else if (this.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                        {
                            exceptionReason = SR.GetString(SR.CSResponseOfferRejected);
                            faultReason = SR.GetString(SR.CSResponseOfferRejectedReason);
                        }
                        else
                        {
                            throw Fx.AssertAndThrow("Reliable messaging version not supported.");
                        }
                    }
                    else if ((createResponse.AcceptAcksTo != null) && (this.InputID == null))
                    {
                        exceptionReason = SR.GetString(SR.CSResponseWithOffer);
                        faultReason = SR.GetString(SR.CSResponseWithOfferReason);
                    }
                    else if (createResponse.AcceptAcksTo != null && (createResponse.AcceptAcksTo.Uri != this.binder.RemoteAddress.Uri))
                    {
                        exceptionReason = SR.GetString(SR.AcksToMustBeSameAsRemoteAddress);
                        faultReason = SR.GetString(SR.AcksToMustBeSameAsRemoteAddressReason);
                    }

                    if ((faultReason != null) && (createResponse != null))
                    {
                        UniqueId sequenceId = createResponse.Identifier;
                        WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(sequenceId, faultReason, null);
                        this.OnLocalFault(null, fault, null);
                    }

                    if (exceptionReason != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(exceptionReason));
                }
            }

            this.InitiationTime = DateTime.UtcNow - start;
            this.OutputID = createResponse.Identifier;
            this.pollingTimer.Set(this.GetPollingInterval());
            base.StartInactivityTimer();
        }

        public void ResumePolling(bool fastPolling)
        {
            lock (this.ThisLock)
            {
                if (this.pollingMode != PollingMode.NotPolling)
                {
                    throw Fx.AssertAndThrow("Can't resume polling if pollingMode != PollingMode.NotPolling");
                }

                if (fastPolling)
                {
                    this.pollingMode = PollingMode.FastPolling;
                }
                else
                {
                    if (this.oldPollingMode == PollingMode.FastPolling)
                        this.pollingMode = PollingMode.Idle;
                    else
                        this.pollingMode = this.oldPollingMode;
                }

                this.Guard.Exit();
                this.pollingTimer.Set(this.GetPollingInterval());
            }
        }

        // Returns true if caller should resume polling
        public bool StopPolling()
        {
            lock (this.ThisLock)
            {
                if (this.pollingMode == PollingMode.NotPolling)
                    return false;

                this.oldPollingMode = pollingMode;
                this.pollingMode = PollingMode.NotPolling;
                this.pollingTimer.Cancel();
                return this.Guard.Enter();
            }
        }

        protected override WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            WsrmFault fault = base.VerifyDuplexProtocolElements(info);

            if (fault != null)
                return fault;
            else if (info.CreateSequenceInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedCS), SR.GetString(SR.UnexpectedCS));
            else if (info.CreateSequenceResponseInfo != null && info.CreateSequenceResponseInfo.Identifier != this.OutputID)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedCSROfferId), SR.GetString(SR.UnexpectedCSROfferId));
            else
                return null;
        }

        protected override WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null && info.AcknowledgementInfo.SequenceID != this.OutputID)
                return new UnknownSequenceFault(info.AcknowledgementInfo.SequenceID);
            else if (info.AckRequestedInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedAckRequested), SR.GetString(SR.UnexpectedAckRequested));
            else if (info.CreateSequenceInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedCS), SR.GetString(SR.UnexpectedCS));
            else if (info.SequencedMessageInfo != null)
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            else if (info.TerminateSequenceInfo != null)
            {
                if (this.Settings.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedTerminateSequence), SR.GetString(SR.UnexpectedTerminateSequence));
                else if (info.TerminateSequenceInfo.Identifier == this.OutputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            }
            else if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.TerminateSequenceResponseInfo.Identifier == this.OutputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            else if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.CloseSequenceInfo.Identifier == this.OutputID)
                    return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnsupportedClose), SR.GetString(SR.UnsupportedCloseExceptionString));
                else
                    return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            else if (info.CloseSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.CloseSequenceResponseInfo.Identifier == this.OutputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
            }
            else
                return null;
        }

        class OpenAsyncResult : AsyncResult
        {
            static AsyncCallback onRequestComplete = Fx.ThunkCallback(new AsyncCallback(OnRequestCompleteStatic));
            ClientReliableSession session;
            DateTime start;

            public OpenAsyncResult(ClientReliableSession session, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.session = session;
                this.start = DateTime.UtcNow;

                IAsyncResult result = this.session.requestor.BeginRequest(timeout, onRequestComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.CompleteRequest(result);
                    this.Complete(true);
                }
            }

            void CompleteRequest(IAsyncResult result)
            {
                Message response = this.session.requestor.EndRequest(result);
                this.session.ProcessCreateSequenceResponse(response, this.start);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            static void OnRequestCompleteStatic(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                OpenAsyncResult openResult = (OpenAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    openResult.CompleteRequest(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }

                openResult.Complete(false, exception);
            }
        }

        enum PollingMode
        {
            Idle,
            KeepAlive,
            FastPolling,
            NotPolling
        }
    }

    class ServerReliableSession : ChannelReliableSession, IInputSession
    {
        public ServerReliableSession(
            ChannelBase channel,
            IReliableFactorySettings listener,
            IServerReliableChannelBinder binder,
            FaultHelper faultHelper,
            UniqueId inputID,
            UniqueId outputID)
            : base(channel, listener, binder, faultHelper)
        {
            this.InputID = inputID;
            this.OutputID = outputID;
        }

        public override UniqueId SequenceID
        {
            get
            {
                return this.InputID;
            }
        }

        public override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        public override void EndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
            base.StartInactivityTimer();
        }

        public override void OnLocalActivity()
        {
        }

        public override void Open(TimeSpan timeout)
        {
            this.StartInactivityTimer();
        }

        protected override WsrmFault VerifyDuplexProtocolElements(WsrmMessageInfo info)
        {
            WsrmFault fault = base.VerifyDuplexProtocolElements(info);

            if (fault != null)
                return fault;
            else if (info.CreateSequenceInfo != null && info.CreateSequenceInfo.OfferIdentifier != this.OutputID)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedCSOfferId), SR.GetString(SR.UnexpectedCSOfferId));
            else if (info.CreateSequenceResponseInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.OutputID, SR.GetString(SR.SequenceTerminatedUnexpectedCSR), SR.GetString(SR.UnexpectedCSR));
            else
                return null;
        }

        protected override WsrmFault VerifySimplexProtocolElements(WsrmMessageInfo info)
        {
            if (info.AcknowledgementInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.InputID, SR.GetString(SR.SequenceTerminatedUnexpectedAcknowledgement), SR.GetString(SR.UnexpectedAcknowledgement));
            else if (info.AckRequestedInfo != null && info.AckRequestedInfo.SequenceID != this.InputID)
                return new UnknownSequenceFault(info.AckRequestedInfo.SequenceID);
            else if (info.CreateSequenceResponseInfo != null)
                return SequenceTerminatedFault.CreateProtocolFault(this.InputID, SR.GetString(SR.SequenceTerminatedUnexpectedCSR), SR.GetString(SR.UnexpectedCSR));
            else if (info.SequencedMessageInfo != null && info.SequencedMessageInfo.SequenceID != this.InputID)
                return new UnknownSequenceFault(info.SequencedMessageInfo.SequenceID);
            else if (info.TerminateSequenceInfo != null && info.TerminateSequenceInfo.Identifier != this.InputID)
                return new UnknownSequenceFault(info.TerminateSequenceInfo.Identifier);
            else if (info.TerminateSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.TerminateSequenceResponseInfo.Identifier == this.InputID)
                    return SequenceTerminatedFault.CreateProtocolFault(this.InputID, SR.GetString(SR.SequenceTerminatedUnexpectedTerminateSequenceResponse), SR.GetString(SR.UnexpectedTerminateSequenceResponse));
                else
                    return new UnknownSequenceFault(info.TerminateSequenceResponseInfo.Identifier);
            }
            else if (info.CloseSequenceInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.CloseSequenceInfo.Identifier == this.InputID)
                    return null;
                else
                    return new UnknownSequenceFault(info.CloseSequenceInfo.Identifier);
            }
            else if (info.CloseSequenceResponseInfo != null)
            {
                WsrmUtilities.AssertWsrm11(this.Settings.ReliableMessagingVersion);

                if (info.CloseSequenceResponseInfo.Identifier == this.InputID)
                    return SequenceTerminatedFault.CreateProtocolFault(this.InputID, SR.GetString(SR.SequenceTerminatedUnexpectedCloseSequenceResponse), SR.GetString(SR.UnexpectedCloseSequenceResponse));
                else
                    return new UnknownSequenceFault(info.CloseSequenceResponseInfo.Identifier);
            }
            else
                return null;
        }
    }
}
