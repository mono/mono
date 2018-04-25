//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;
    using System.Xml;

    delegate IAsyncResult BeginSendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException, AsyncCallback asyncCallback, object state);
    delegate void EndSendHandler(IAsyncResult result);
    delegate void SendHandler(MessageAttemptInfo attemptInfo, TimeSpan timeout, bool maskUnhandledException);
    delegate void ComponentFaultedHandler(Exception faultException, WsrmFault fault);
    delegate void ComponentExceptionHandler(Exception exception);
    delegate void RetryHandler(MessageAttemptInfo attemptInfo);

    sealed class ReliableOutputConnection
    {
        BeginSendHandler beginSendHandler;
        OperationWithTimeoutBeginCallback beginSendAckRequestedHandler;
        bool closed = false;
        EndSendHandler endSendHandler;
        OperationEndCallback endSendAckRequestedHandler;
        UniqueId id;
        MessageVersion messageVersion;
        object mutex = new Object();
        static AsyncCallback onSendRetriesComplete = Fx.ThunkCallback(new AsyncCallback(OnSendRetriesComplete));
        static AsyncCallback onSendRetryComplete = Fx.ThunkCallback(new AsyncCallback(OnSendRetryComplete));
        ReliableMessagingVersion reliableMessagingVersion;
        Guard sendGuard = new Guard(Int32.MaxValue);
        SendHandler sendHandler;
        OperationWithTimeoutCallback sendAckRequestedHandler;
        static Action<object> sendRetries = new Action<object>(SendRetries);
        TimeSpan sendTimeout;
        InterruptibleWaitObject shutdownHandle = new InterruptibleWaitObject(false);
        TransmissionStrategy strategy;
        bool terminated = false;

        public ReliableOutputConnection(UniqueId id,
            int maxTransferWindowSize,
            MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion,
            TimeSpan initialRtt,
            bool requestAcks,
            TimeSpan sendTimeout)
        {
            this.id = id;
            this.messageVersion = messageVersion;
            this.reliableMessagingVersion = reliableMessagingVersion;
            this.sendTimeout = sendTimeout;
            this.strategy = new TransmissionStrategy(reliableMessagingVersion, initialRtt, maxTransferWindowSize,
                requestAcks, id);
            this.strategy.RetryTimeoutElapsed = OnRetryTimeoutElapsed;
            this.strategy.OnException = RaiseOnException;
        }

        public ComponentFaultedHandler Faulted;
        public ComponentExceptionHandler OnException;

        MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public BeginSendHandler BeginSendHandler
        {
            set
            {
                this.beginSendHandler = value;
            }
        }

        public OperationWithTimeoutBeginCallback BeginSendAckRequestedHandler
        {
            set
            {
                this.beginSendAckRequestedHandler = value;
            }
        }

        public bool Closed
        {
            get
            {
                return this.closed;
            }
        }

        public EndSendHandler EndSendHandler
        {
            set
            {
                this.endSendHandler = value;
            }
        }

        public OperationEndCallback EndSendAckRequestedHandler
        {
            set
            {
                this.endSendAckRequestedHandler = value;
            }
        }

        public Int64 Last
        {
            get
            {
                return this.strategy.Last;
            }
        }

        public SendHandler SendHandler
        {
            set
            {
                this.sendHandler = value;
            }
        }

        public OperationWithTimeoutCallback SendAckRequestedHandler
        {
            set
            {
                this.sendAckRequestedHandler = value;
            }
        }

        public TransmissionStrategy Strategy
        {
            get
            {
                return this.strategy;
            }
        }

        object ThisLock
        {
            get { return this.mutex; }
        }

        public void Abort(ChannelBase channel)
        {
            this.sendGuard.Abort();
            this.shutdownHandle.Abort(channel);
            this.strategy.Abort(channel);
        }

        void CompleteTransfer(TimeSpan timeout)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                Message message = Message.CreateMessage(this.MessageVersion, WsrmFeb2005Strings.LastMessageAction);
                message.Properties.AllowOutputBatching = false;

                // Return value ignored.
                this.InternalAddMessage(message, timeout, null, true);
            }
            else if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.strategy.SetLast())
                {
                    this.shutdownHandle.Set();
                }
                else
                {
                    this.sendAckRequestedHandler(timeout);
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }
        }

        public bool AddMessage(Message message, TimeSpan timeout, object state)
        {
            return this.InternalAddMessage(message, timeout, state, false);
        }

        public IAsyncResult BeginAddMessage(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            return new AddAsyncResult(message, false, timeout, state, this, callback, asyncState);
        }

        public bool EndAddMessage(IAsyncResult result)
        {
            return AddAsyncResult.End(result);
        }

        IAsyncResult BeginCompleteTransfer(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                Message message = Message.CreateMessage(this.MessageVersion, WsrmFeb2005Strings.LastMessageAction);
                message.Properties.AllowOutputBatching = false;
                return new AddAsyncResult(message, true, timeout, null, this, callback, state);
            }
            else if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (this.strategy.SetLast())
                {
                    this.shutdownHandle.Set();
                    return new AlreadyCompletedTransferAsyncResult(callback, state);
                }
                else
                {
                    return this.beginSendAckRequestedHandler(timeout, callback, state);
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }
        }

        void EndCompleteTransfer(IAsyncResult result)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                AddAsyncResult.End(result);
            }
            else if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                AlreadyCompletedTransferAsyncResult completedResult = result as AlreadyCompletedTransferAsyncResult;
                if (completedResult != null)
                {
                    completedResult.End();
                }
                else
                {
                    this.endSendAckRequestedHandler(result);
                }
            }
            else
            {
                throw Fx.AssertAndThrow("Unsupported version.");
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool completeTransfer = false;

            lock (this.ThisLock)
            {
                completeTransfer = !this.closed;
                this.closed = true;
            }

            OperationWithTimeoutBeginCallback[] beginCallbacks;
            OperationEndCallback[] endCallbacks;

            beginCallbacks = new OperationWithTimeoutBeginCallback[] {
                completeTransfer ? this.BeginCompleteTransfer : default(OperationWithTimeoutBeginCallback),
                this.shutdownHandle.BeginWait,
                this.sendGuard.BeginClose,
                this.beginSendAckRequestedHandler };

            endCallbacks = new OperationEndCallback[] {
                completeTransfer ? this.EndCompleteTransfer : default(OperationEndCallback),
                this.shutdownHandle.EndWait,
                this.sendGuard.EndClose,
                this.endSendAckRequestedHandler };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginCallbacks, endCallbacks, callback, state);
        }

        public bool CheckForTermination()
        {
            return this.strategy.DoneTransmitting;
        }

        public void Close(TimeSpan timeout)
        {
            bool completeTransfer = false;

            lock (this.ThisLock)
            {
                completeTransfer = !this.closed;
                this.closed = true;
            }

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (completeTransfer)
            {
                this.CompleteTransfer(timeoutHelper.RemainingTime());
            }

            this.shutdownHandle.Wait(timeoutHelper.RemainingTime());
            this.sendGuard.Close(timeoutHelper.RemainingTime());
            this.strategy.Close();
        }

        void CompleteSendRetries(IAsyncResult result)
        {
            while (true)
            {
                this.endSendHandler(result);
                this.sendGuard.Exit();
                this.strategy.DequeuePending();

                if (this.sendGuard.Enter())
                {
                    MessageAttemptInfo attemptInfo = this.strategy.GetMessageInfoForRetry(true);

                    if (attemptInfo.Message == null)
                    {
                        break;
                    }
                    else
                    {
                        result = this.beginSendHandler(attemptInfo, this.sendTimeout, true, onSendRetriesComplete, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            // We are here if there are no more messages to retry.            
            this.sendGuard.Exit();
            this.OnTransferComplete();
        }

        void CompleteSendRetry(IAsyncResult result)
        {
            try
            {
                this.endSendHandler(result);
            }
            finally
            {
                this.sendGuard.Exit();
            }
        }

        public void EndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            this.strategy.Close();
        }

        public void Fault(ChannelBase channel)
        {
            this.sendGuard.Abort();
            this.shutdownHandle.Fault(channel);
            this.strategy.Fault(channel);
        }

        bool InternalAddMessage(Message message, TimeSpan timeout, object state, bool isLast)
        {
            MessageAttemptInfo attemptInfo;
            TimeoutHelper helper = new TimeoutHelper(timeout);

            try
            {
                if (isLast)
                {
                    if (state != null)
                    {
                        throw Fx.AssertAndThrow("The isLast overload does not take a state.");
                    }

                    attemptInfo = this.strategy.AddLast(message, helper.RemainingTime(), null);
                }
                else if (!this.strategy.Add(message, helper.RemainingTime(), state, out attemptInfo))
                {
                    return false;
                }
            }
            catch (TimeoutException)
            {
                if (isLast)
                    this.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.id, SR.GetString(SR.SequenceTerminatedAddLastToWindowTimedOut), null));
                // else - RM does not fault the channel based on a timeout exception trying to add a sequenced message to the window.

                throw;
            }
            catch (Exception e)
            {
                if (!Fx.IsFatal(e))
                    this.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.id, SR.GetString(SR.SequenceTerminatedUnknownAddToWindowError), null));

                throw;
            }

            if (sendGuard.Enter())
            {
                try
                {
                    this.sendHandler(attemptInfo, helper.RemainingTime(), false);
                }
                catch (QuotaExceededException)
                {
                    this.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.id));
                    throw;
                }
                finally
                {
                    this.sendGuard.Exit();
                }
            }

            return true;
        }

        public bool IsFinalAckConsistent(SequenceRangeCollection ranges)
        {
            return this.strategy.IsFinalAckConsistent(ranges);
        }

        void OnRetryTimeoutElapsed(MessageAttemptInfo attemptInfo)
        {
            if (this.sendGuard.Enter())
            {
                IAsyncResult result = this.beginSendHandler(attemptInfo, this.sendTimeout, true, onSendRetryComplete, this);

                if (result.CompletedSynchronously)
                {
                    this.CompleteSendRetry(result);
                }
            }
        }

        static void OnSendRetryComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputConnection outputConnection = (ReliableOutputConnection)result.AsyncState;

                try
                {
                    outputConnection.CompleteSendRetry(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    outputConnection.RaiseOnException(e);
                }
            }
        }

        static void OnSendRetriesComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputConnection outputConnection = (ReliableOutputConnection)result.AsyncState;

                try
                {
                    outputConnection.CompleteSendRetries(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    outputConnection.RaiseOnException(e);
                }
            }
        }

        void OnTransferComplete()
        {
            this.strategy.DequeuePending();

            if (this.strategy.DoneTransmitting)
                Terminate();
        }

        public void ProcessTransferred(Int64 transferred, SequenceRangeCollection ranges, int quotaRemaining)
        {
            if (transferred < 0)
            {
                throw Fx.AssertAndThrow("Argument transferred must be a valid sequence number or 0 for protocol messages.");
            }

            bool invalidAck;

            // ignored, TransmissionStrategy is being used to keep track of what must be re-sent.
            // In the Request-Reply case this state may not align with acks.
            bool inconsistentAck;

            this.strategy.ProcessAcknowledgement(ranges, out invalidAck, out inconsistentAck);
            invalidAck = (invalidAck || ((transferred != 0) && !ranges.Contains(transferred)));

            if (!invalidAck)
            {
                if ((transferred > 0) && this.strategy.ProcessTransferred(transferred, quotaRemaining))
                {
                    ActionItem.Schedule(sendRetries, this);
                }
                else
                {
                    this.OnTransferComplete();
                }
            }
            else
            {
                WsrmFault fault = new InvalidAcknowledgementFault(this.id, ranges);
                RaiseFault(fault.CreateException(), fault);
            }
        }

        public void ProcessTransferred(SequenceRangeCollection ranges, int quotaRemaining)
        {
            bool invalidAck;
            bool inconsistentAck;

            this.strategy.ProcessAcknowledgement(ranges, out invalidAck, out inconsistentAck);

            if (!invalidAck && !inconsistentAck)
            {
                if (this.strategy.ProcessTransferred(ranges, quotaRemaining))
                {
                    ActionItem.Schedule(sendRetries, this);
                }
                else
                {
                    this.OnTransferComplete();
                }
            }
            else
            {
                WsrmFault fault = new InvalidAcknowledgementFault(this.id, ranges);
                RaiseFault(fault.CreateException(), fault);
            }
        }

        void RaiseFault(Exception faultException, WsrmFault fault)
        {
            ComponentFaultedHandler handler = this.Faulted;

            if (handler != null)
                handler(faultException, fault);
        }

        void RaiseOnException(Exception exception)
        {
            ComponentExceptionHandler handler = this.OnException;

            if (handler != null)
                handler(exception);
        }

        void SendRetries()
        {
            IAsyncResult result = null;

            if (this.sendGuard.Enter())
            {
                MessageAttemptInfo attemptInfo = this.strategy.GetMessageInfoForRetry(false);

                if (attemptInfo.Message != null)
                {
                    result = this.beginSendHandler(attemptInfo, this.sendTimeout, true, onSendRetriesComplete, this);
                }

                if (result != null)
                {
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteSendRetries(result);
                    }
                }
                else
                {
                    this.sendGuard.Exit();
                    this.OnTransferComplete();
                }
            }
        }

        static void SendRetries(object state)
        {
            ReliableOutputConnection outputConnection = (ReliableOutputConnection)state;

            try
            {
                outputConnection.SendRetries();
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                outputConnection.RaiseOnException(e);
            }
        }

        public void Terminate()
        {
            lock (this.ThisLock)
            {
                if (this.terminated)
                    return;

                this.terminated = true;
            }

            this.shutdownHandle.Set();
        }

        sealed class AddAsyncResult : AsyncResult
        {
            static AsyncCallback addCompleteStatic = Fx.ThunkCallback(new AsyncCallback(AddComplete));
            ReliableOutputConnection connection;
            bool isLast;
            static AsyncCallback sendCompleteStatic = Fx.ThunkCallback(new AsyncCallback(SendComplete));
            TimeoutHelper timeoutHelper;
            bool validAdd;

            public AddAsyncResult(Message message, bool isLast, TimeSpan timeout, object state,
                ReliableOutputConnection connection, AsyncCallback callback, object asyncState)
                : base(callback, asyncState)
            {
                this.connection = connection;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.isLast = isLast;

                bool complete = false;
                IAsyncResult result;

                try
                {
                    if (isLast)
                    {
                        if (state != null)
                        {
                            throw Fx.AssertAndThrow("The isLast overload does not take a state.");
                        }

                        result = this.connection.strategy.BeginAddLast(message, this.timeoutHelper.RemainingTime(), state, addCompleteStatic, this);
                    }
                    else
                    {
                        result = this.connection.strategy.BeginAdd(message, this.timeoutHelper.RemainingTime(), state, addCompleteStatic, this);
                    }
                }
                catch (TimeoutException)
                {
                    if (isLast)
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, SR.GetString(SR.SequenceTerminatedAddLastToWindowTimedOut), null));
                    // else - RM does not fault the channel based on a timeout exception trying to add a sequenced message to the window.

                    throw;
                }
                catch (Exception e)
                {
                    if (!Fx.IsFatal(e))
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, SR.GetString(SR.SequenceTerminatedUnknownAddToWindowError), null));

                    throw;
                }

                if (result.CompletedSynchronously)
                    complete = this.CompleteAdd(result);

                if (complete)
                    this.Complete(true);
            }

            static void AddComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    AddAsyncResult addResult = (AddAsyncResult)result.AsyncState;
                    bool complete = false;
                    Exception completeException = null;

                    try
                    {
                        complete = addResult.CompleteAdd(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        completeException = e;
                    }

                    if (complete || completeException != null)
                        addResult.Complete(false, completeException);
                }
            }

            bool CompleteAdd(IAsyncResult result)
            {
                MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
                this.validAdd = true;

                try
                {
                    if (this.isLast)
                    {
                        attemptInfo = this.connection.strategy.EndAddLast(result);
                    }
                    else if (!this.connection.strategy.EndAdd(result, out attemptInfo))
                    {
                        this.validAdd = false;
                        return true;
                    }
                }
                catch (TimeoutException)
                {
                    if (this.isLast)
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, SR.GetString(SR.SequenceTerminatedAddLastToWindowTimedOut), null));
                    // else - RM does not fault the channel based on a timeout exception trying to add a sequenced message to the window.

                    throw;
                }
                catch (Exception e)
                {
                    if (!Fx.IsFatal(e))
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateCommunicationFault(this.connection.id, SR.GetString(SR.SequenceTerminatedUnknownAddToWindowError), null));

                    throw;
                }

                if (this.connection.sendGuard.Enter())
                {
                    bool throwing = true;

                    try
                    {
                        result = this.connection.beginSendHandler(attemptInfo, this.timeoutHelper.RemainingTime(), false, sendCompleteStatic, this);
                        throwing = false;
                    }
                    catch (QuotaExceededException)
                    {
                        this.connection.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.connection.id));
                        throw;
                    }
                    finally
                    {
                        if (throwing)
                            this.connection.sendGuard.Exit();
                    }
                }
                else
                {
                    return true;
                }

                if (result.CompletedSynchronously)
                {
                    this.CompleteSend(result);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            void CompleteSend(IAsyncResult result)
            {
                try
                {
                    this.connection.endSendHandler(result);
                }
                catch (QuotaExceededException)
                {
                    this.connection.RaiseFault(null, SequenceTerminatedFault.CreateQuotaExceededFault(this.connection.id));
                    throw;
                }
                finally
                {
                    this.connection.sendGuard.Exit();
                }
            }

            static void SendComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    AddAsyncResult addResult = (AddAsyncResult)result.AsyncState;
                    Exception completeException = null;

                    try
                    {
                        addResult.CompleteSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        completeException = e;
                    }

                    addResult.Complete(false, completeException);
                }
            }

            public static bool End(IAsyncResult result)
            {
                AsyncResult.End<AddAsyncResult>(result);
                return ((AddAsyncResult)result).validAdd;
            }
        }

        class AlreadyCompletedTransferAsyncResult : CompletedAsyncResult
        {
            public AlreadyCompletedTransferAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public void End()
            {
                AsyncResult.End<AlreadyCompletedTransferAsyncResult>(this);
            }
        }
    }
}
