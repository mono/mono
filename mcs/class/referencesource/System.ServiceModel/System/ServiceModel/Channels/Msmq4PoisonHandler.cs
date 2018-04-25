//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    sealed class Msmq4PoisonHandler : IPoisonHandlingStrategy
    {
        MsmqQueue mainQueue;
        MsmqQueue mainQueueForMove;
        MsmqQueue retryQueueForPeek;
        MsmqQueue retryQueueForMove;
        MsmqQueue poisonQueue;
        MsmqQueue lockQueueForReceive;

        IOThreadTimer timer;
        MsmqReceiveHelper receiver;

        bool disposed;

        string poisonQueueName;
        string retryQueueName;
        string mainQueueName;

        MsmqRetryQueueMessage retryQueueMessage;
        static Action<object> onStartPeek = new Action<object>(StartPeek);
        static AsyncCallback onPeekCompleted = Fx.ThunkCallback(OnPeekCompleted);

        public Msmq4PoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
            this.timer = new IOThreadTimer(new Action<object>(OnTimer), null, false);
            this.disposed = false;
            this.mainQueueName = this.ReceiveParameters.AddressTranslator.UriToFormatName(this.ListenUri);
            this.poisonQueueName = this.ReceiveParameters.AddressTranslator.UriToFormatName(new Uri(this.ListenUri.AbsoluteUri + ";poison"));
            this.retryQueueName = this.ReceiveParameters.AddressTranslator.UriToFormatName(new Uri(this.ListenUri.AbsoluteUri + ";retry"));
        }

        MsmqReceiveParameters ReceiveParameters
        {
            get { return this.receiver.MsmqReceiveParameters; }
        }

        Uri ListenUri
        {
            get { return this.receiver.ListenUri; }
        }

        public void Open()
        {
            if (this.ReceiveParameters.ReceiveContextSettings.Enabled)
            {
                Fx.Assert(this.receiver.Queue is MsmqSubqueueLockingQueue, "Queue must be MsmqSubqueueLockingQueue");
                this.lockQueueForReceive = ((MsmqSubqueueLockingQueue)this.receiver.Queue).LockQueueForReceive;
            }

            this.mainQueue = this.receiver.Queue;
            this.mainQueueForMove = new MsmqQueue(this.mainQueueName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
            // Open up the poison queue (for handling poison messages).
            this.poisonQueue = new MsmqQueue(this.poisonQueueName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
            this.retryQueueForMove = new MsmqQueue(this.retryQueueName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
            this.retryQueueForPeek = new MsmqQueue(this.retryQueueName, UnsafeNativeMethods.MQ_RECEIVE_ACCESS);
            this.retryQueueMessage = new MsmqRetryQueueMessage();

            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                StartPeek(this);
            }
            else
            {
                ActionItem.Schedule(Msmq4PoisonHandler.onStartPeek, this);
            }
        }

        static void StartPeek(object state)
        {
            Msmq4PoisonHandler handler = state as Msmq4PoisonHandler;
            lock (handler)
            {
                if (!handler.disposed)
                {
                    handler.retryQueueForPeek.BeginPeek(handler.retryQueueMessage, TimeSpan.MaxValue, onPeekCompleted, handler);
                }
            }
        }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            if (this.ReceiveParameters.ReceiveContextSettings.Enabled)
            {
                return ReceiveContextPoisonHandling(messageProperty);
            }
            else
            {
                return NonReceiveContextPoisonHandling(messageProperty);
            }
        }

        public bool ReceiveContextPoisonHandling(MsmqMessageProperty messageProperty)
        {
            // The basic idea is to use the message move count to get the number of retry attempts the message has been through
            // The computation of the retry count and retry cycle count is slightly involved due to fact that the message being processed
            // could have been recycled message. (Recycled message is the message that moves from lock queue to retry queue to main queue
            // and back to lock queue
            //

            // Count to tally message recycling (lock queue to retry queue to main queue adds move count of 2 to the message)
            const int retryMoveCount = 2;

            // Actual number of times message is received before recycling to retry queue
            int actualReceiveRetryCount = this.ReceiveParameters.ReceiveRetryCount + 1;

            // The message is recycled these many number of times
            int maxRetryCycles = this.ReceiveParameters.MaxRetryCycles;

            // Max change in message move count between recycling
            int maxMovePerCycle = (2 * actualReceiveRetryCount) + 1;

            // Number of recycles the message has been through
            int messageCyclesCompleted = messageProperty.MoveCount / (maxMovePerCycle + retryMoveCount);

            // Total number of moves on the message at the end of the last recycle
            int messageMoveCountForCyclesCompleted = messageCyclesCompleted * (maxMovePerCycle + retryMoveCount);

            // The differential move count for the current cycle
            int messageMoveCountForCurrentCycle = messageProperty.MoveCount - messageMoveCountForCyclesCompleted;

            lock (this)
            {
                if (this.disposed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));

                // Check if the message has already completed its max recycle count (MaxRetryCycles)
                // and the disposed the message first. Such a message was previously disposed using the ReceiveErrorHandling method
                // and the channel/listener would immediately fault 
                //
                if (messageCyclesCompleted > maxRetryCycles)
                {
                    FinalDisposition(messageProperty);
                    return true;
                }

                // Check if the message is eligible for recycling/disposition
                if (messageMoveCountForCurrentCycle >= maxMovePerCycle)
                {
                    if (TD.ReceiveRetryCountReachedIsEnabled())
                    {
                        TD.ReceiveRetryCountReached(messageProperty.MessageId);
                    }
                    if (messageCyclesCompleted < maxRetryCycles)
                    {
                        // The message is eligible for recycling, move the message the message to retry queue
                        MsmqReceiveHelper.MoveReceivedMessage(this.lockQueueForReceive, this.retryQueueForMove, messageProperty.LookupId);
                        MsmqDiagnostics.PoisonMessageMoved(messageProperty.MessageId, false, this.receiver.InstanceId);
                    }
                    else
                    {
                        if (TD.MaxRetryCyclesExceededMsmqIsEnabled())
                        {
                            TD.MaxRetryCyclesExceededMsmq(messageProperty.MessageId);
                        }
                        // Dispose the message using ReceiveErrorHandling
                        FinalDisposition(messageProperty);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool NonReceiveContextPoisonHandling(MsmqMessageProperty messageProperty)
        {
            if (messageProperty.AbortCount <= this.ReceiveParameters.ReceiveRetryCount)
                return false;
            int retryCycle = messageProperty.MoveCount / 2;

            lock (this)
            {
                if (this.disposed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));

                if (retryCycle >= this.ReceiveParameters.MaxRetryCycles)
                {
                    if (TD.MaxRetryCyclesExceededMsmqIsEnabled())
                    {
                        TD.MaxRetryCyclesExceededMsmq(messageProperty.MessageId);
                    }
                    FinalDisposition(messageProperty);
                }
                else
                {
                    MsmqReceiveHelper.MoveReceivedMessage(this.mainQueue, this.retryQueueForMove, messageProperty.LookupId);
                    MsmqDiagnostics.PoisonMessageMoved(messageProperty.MessageId, false, this.receiver.InstanceId);
                }
            }
            return true;
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            if (this.ReceiveParameters.ReceiveContextSettings.Enabled)
            {
                this.InternalFinalDisposition(this.lockQueueForReceive, messageProperty);
            }
            else
            {
                this.InternalFinalDisposition(this.mainQueue, messageProperty);
            }
        }


        private void InternalFinalDisposition(MsmqQueue disposeFromQueue, MsmqMessageProperty messageProperty)
        {
            switch (this.ReceiveParameters.ReceiveErrorHandling)
            {
                case ReceiveErrorHandling.Drop:
                    this.receiver.DropOrRejectReceivedMessage(disposeFromQueue, messageProperty, false);
                    break;

                case ReceiveErrorHandling.Fault:
                    MsmqReceiveHelper.TryAbortTransactionCurrent();
                    if (null != this.receiver.ChannelListener)
                        this.receiver.ChannelListener.FaultListener();
                    if (null != this.receiver.Channel)
                        this.receiver.Channel.FaultChannel();
                    break;

                case ReceiveErrorHandling.Reject:
                    this.receiver.DropOrRejectReceivedMessage(disposeFromQueue, messageProperty, true);
                    MsmqDiagnostics.PoisonMessageRejected(messageProperty.MessageId, this.receiver.InstanceId);
                    break;

                case ReceiveErrorHandling.Move:
                    MsmqReceiveHelper.MoveReceivedMessage(disposeFromQueue, this.poisonQueue, messageProperty.LookupId);
                    MsmqDiagnostics.PoisonMessageMoved(messageProperty.MessageId, true, this.receiver.InstanceId);
                    break;

                default:
                    Fx.Assert("System.ServiceModel.Channels.Msmq4PoisonHandler.FinalDisposition(): (unexpected ReceiveErrorHandling)");
                    break;
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.timer.Cancel();

                    if (null != this.retryQueueForPeek)
                        this.retryQueueForPeek.Dispose();
                    if (null != this.retryQueueForMove)
                        this.retryQueueForMove.Dispose();
                    if (null != this.poisonQueue)
                        this.poisonQueue.Dispose();
                    if (null != this.mainQueueForMove)
                        this.mainQueueForMove.Dispose();
                }
            }
        }

        static void OnPeekCompleted(IAsyncResult result)
        {
            Msmq4PoisonHandler handler = result.AsyncState as Msmq4PoisonHandler;
            MsmqQueue.ReceiveResult receiveResult = MsmqQueue.ReceiveResult.Unknown;
            try
            {
                receiveResult = handler.retryQueueForPeek.EndPeek(result);
            }
            catch (MsmqException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }

            if (MsmqQueue.ReceiveResult.MessageReceived == receiveResult)
            {
                lock (handler)
                {
                    if (!handler.disposed)
                    {
                        // Check the time - move it, and begin peeking again
                        // if necessary, or wait for the timeout.

                        DateTime lastMoveTime = MsmqDateTime.ToDateTime(handler.retryQueueMessage.LastMoveTime.Value);

                        TimeSpan waitTime = lastMoveTime + handler.ReceiveParameters.RetryCycleDelay - DateTime.UtcNow;
                        if (waitTime < TimeSpan.Zero)
                            handler.OnTimer(handler);
                        else
                            handler.timer.Set(waitTime);
                    }
                }
            }
        }

        void OnTimer(object state)
        {
            lock (this)
            {
                if (!this.disposed)
                {
                    try
                    {
                        this.retryQueueForPeek.TryMoveMessage(this.retryQueueMessage.LookupId.Value, this.mainQueueForMove, MsmqTransactionMode.Single);
                    }
                    catch (MsmqException ex)
                    {
                        MsmqDiagnostics.ExpectedException(ex);
                    }
                    this.retryQueueForPeek.BeginPeek(this.retryQueueMessage, TimeSpan.MaxValue, onPeekCompleted, this);
                }
            }
        }

        class MsmqRetryQueueMessage : NativeMsmqMessage
        {
            LongProperty lookupId;
            IntProperty lastMoveTime;

            public MsmqRetryQueueMessage()
                : base(2)
            {
                this.lookupId = new LongProperty(this, UnsafeNativeMethods.PROPID_M_LOOKUPID);
                this.lastMoveTime = new IntProperty(this, UnsafeNativeMethods.PROPID_M_LAST_MOVE_TIME);
            }

            public LongProperty LookupId
            {
                get { return this.lookupId; }
            }

            public IntProperty LastMoveTime
            {
                get { return this.lastMoveTime; }
            }
        }
    }
}
