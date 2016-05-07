//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    struct MessageAttemptInfo
    {
        readonly Message message;
        readonly int retryCount;
        readonly Int64 sequenceNumber;
        readonly object state;

        public MessageAttemptInfo(Message message, Int64 sequenceNumber, int retryCount, object state)
        {
            this.message = message;
            this.sequenceNumber = sequenceNumber;
            this.retryCount = retryCount;
            this.state = state;
        }
        public Message Message
        {
            get { return this.message; }
        }

        public int RetryCount
        {
            get { return this.retryCount; }
        }

        public object State
        {
            get { return this.state; }
        }

        public Int64 GetSequenceNumber()
        {
            if (this.sequenceNumber <= 0)
            {
                throw Fx.AssertAndThrow("The caller is not allowed to get an invalid SequenceNumber.");
            }

            return this.sequenceNumber;
        }
    }

    sealed class TransmissionStrategy
    {
        bool aborted;
        bool closed;
        int congestionControlModeAcks;
        UniqueId id;
        Int64 last = 0;
        int lossWindowSize;
        int maxWindowSize;
        Int64 meanRtt;
        ComponentExceptionHandler onException;
        Int32 quotaRemaining;
        ReliableMessagingVersion reliableMessagingVersion;
        List<Int64> retransmissionWindow = new List<Int64>();
        IOThreadTimer retryTimer;
        RetryHandler retryTimeoutElapsedHandler;
        bool requestAcks;
        Int64 serrRtt;
        int slowStartThreshold;
        bool startup = true;
        object thisLock = new object();
        Int64 timeout;
        Queue<IQueueAdder> waitQueue = new Queue<IQueueAdder>();
        SlidingWindow window;
        int windowSize = 1;
        Int64 windowStart = 1;

        public TransmissionStrategy(ReliableMessagingVersion reliableMessagingVersion, TimeSpan initRtt,
            int maxWindowSize, bool requestAcks, UniqueId id)
        {
            if (initRtt < TimeSpan.Zero)
            {
                if (DiagnosticUtility.ShouldTrace(TraceEventType.Warning))
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.WsrmNegativeElapsedTimeDetected,
                    SR.GetString(SR.TraceCodeWsrmNegativeElapsedTimeDetected), this);
                }

                initRtt = ReliableMessagingConstants.UnknownInitiationTime;
            }

            if (maxWindowSize <= 0)
            {
                throw Fx.AssertAndThrow("Argument maxWindow size must be positive.");
            }

            this.id = id;
            this.maxWindowSize = this.lossWindowSize = maxWindowSize;
            this.meanRtt = Math.Min((long)initRtt.TotalMilliseconds, Constants.MaxMeanRtt >> Constants.TimeMultiplier) << Constants.TimeMultiplier;
            this.serrRtt = this.meanRtt >> 1;
            this.window = new SlidingWindow(maxWindowSize);
            this.slowStartThreshold = maxWindowSize;
            this.timeout = Math.Max(((200 << Constants.TimeMultiplier) * 2) + this.meanRtt, this.meanRtt + (this.serrRtt << Constants.ChebychevFactor));
            this.quotaRemaining = Int32.MaxValue;
            this.retryTimer = new IOThreadTimer(new Action<object>(OnRetryElapsed), null, true);
            this.requestAcks = requestAcks;
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        public bool DoneTransmitting
        {
            get
            {
                return (this.last != 0 && this.windowStart == this.last + 1);
            }
        }

        public bool HasPending
        {
            get
            {
                return (this.window.Count > 0 || this.waitQueue.Count > 0);
            }
        }

        public Int64 Last
        {
            get
            {
                return this.last;
            }
        }

        // now in 128ths of a millisecond.
        static Int64 Now
        {
            get
            {
                return (Ticks.Now / TimeSpan.TicksPerMillisecond) << Constants.TimeMultiplier;
            }
        }

        public ComponentExceptionHandler OnException
        {
            set
            {
                this.onException = value;
            }
        }

        public RetryHandler RetryTimeoutElapsed
        {
            set
            {
                this.retryTimeoutElapsedHandler = value;
            }
        }

        public int QuotaRemaining
        {
            get
            {
                return this.quotaRemaining;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public int Timeout
        {
            get
            {
                return (int)(this.timeout >> Constants.TimeMultiplier);
            }
        }


        public void Abort(ChannelBase channel)
        {
            lock (this.ThisLock)
            {
                this.aborted = true;

                if (this.closed)
                    return;

                this.closed = true;

                this.retryTimer.Cancel();

                while (waitQueue.Count > 0)
                    waitQueue.Dequeue().Abort(channel);

                window.Close();
            }
        }

        public bool Add(Message message, TimeSpan timeout, object state, out MessageAttemptInfo attemptInfo)
        {
            return InternalAdd(message, false, timeout, state, out attemptInfo);
        }

        public MessageAttemptInfo AddLast(Message message, TimeSpan timeout, object state)
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                throw Fx.AssertAndThrow("Last message supported only in February 2005.");
            }

            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            InternalAdd(message, true, timeout, state, out attemptInfo);
            return attemptInfo;
        }

        // Must call in a lock(this.ThisLock).
        MessageAttemptInfo AddToWindow(Message message, bool isLast, object state)
        {
            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            Int64 sequenceNumber;

            sequenceNumber = this.windowStart + this.window.Count;
            WsrmUtilities.AddSequenceHeader(this.reliableMessagingVersion, message, this.id, sequenceNumber, isLast);

            if (this.requestAcks && (this.window.Count == this.windowSize - 1 || this.quotaRemaining == 1)) // can't add any more
            {
                message.Properties.AllowOutputBatching = false;
                WsrmUtilities.AddAckRequestedHeader(this.reliableMessagingVersion, message, this.id);
            }

            if (this.window.Count == 0)
            {
                this.retryTimer.Set(this.Timeout);
            }

            this.window.Add(message, Now, state);
            this.quotaRemaining--;
            if (isLast)
                this.last = sequenceNumber;

            int index = (int)(sequenceNumber - this.windowStart);
            attemptInfo = new MessageAttemptInfo(this.window.GetMessage(index), sequenceNumber, 0, state);

            return attemptInfo;
        }

        public IAsyncResult BeginAdd(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            return InternalBeginAdd(message, false, timeout, state, callback, asyncState);
        }

        public IAsyncResult BeginAddLast(Message message, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                throw Fx.AssertAndThrow("Last message supported only in February 2005.");
            }

            return InternalBeginAdd(message, true, timeout, state, callback, asyncState);
        }

        bool CanAdd()
        {
            return (this.window.Count < this.windowSize &&  // Does the message fit in the transmission window?
                this.quotaRemaining > 0 &&                  // Can the receiver handle another message?
                this.waitQueue.Count == 0);                 // Don't get ahead of anyone in the wait queue.
        }

        public void Close()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                    return;

                this.closed = true;

                this.retryTimer.Cancel();

                if (waitQueue.Count != 0)
                {
                    throw Fx.AssertAndThrow("The reliable channel must throw prior to the call to Close() if there are outstanding send or request operations.");
                }

                window.Close();
            }
        }

        public void DequeuePending()
        {
            Queue<IQueueAdder> adders = null;

            lock (this.ThisLock)
            {
                if (this.closed || this.waitQueue.Count == 0)
                    return;

                int count = Math.Min(this.windowSize, this.quotaRemaining) - this.window.Count;
                if (count <= 0)
                    return;

                count = Math.Min(count, this.waitQueue.Count);
                adders = new Queue<IQueueAdder>(count);

                while (count-- > 0)
                {
                    IQueueAdder adder = waitQueue.Dequeue();
                    adder.Complete0();
                    adders.Enqueue(adder);
                }
            }

            while (adders.Count > 0)
                adders.Dequeue().Complete1();
        }

        public bool EndAdd(IAsyncResult result, out MessageAttemptInfo attemptInfo)
        {
            return InternalEndAdd(result, out attemptInfo);
        }

        public MessageAttemptInfo EndAddLast(IAsyncResult result)
        {
            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            InternalEndAdd(result, out attemptInfo);
            return attemptInfo;
        }

        bool IsAddValid()
        {
            return (!this.aborted && !this.closed);
        }

        public void OnRetryElapsed(object state)
        {
            try
            {
                MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);

                lock (this.ThisLock)
                {
                    if (this.closed)
                        return;

                    if (this.window.Count == 0)
                        return;

                    this.window.RecordRetry(0, Now);
                    this.congestionControlModeAcks = 0;
                    this.slowStartThreshold = Math.Max(1, this.windowSize >> 1);
                    this.lossWindowSize = this.windowSize;
                    this.windowSize = 1;
                    this.timeout <<= 1;
                    this.startup = false;

                    attemptInfo = new MessageAttemptInfo(this.window.GetMessage(0), this.windowStart, this.window.GetRetryCount(0), this.window.GetState(0));
                }

                retryTimeoutElapsedHandler(attemptInfo);

                lock (this.ThisLock)
                {
                    if (!this.closed && (this.window.Count > 0))
                    {
                        this.retryTimer.Set(this.Timeout);
                    }
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                this.onException(e);
            }
        }

        public void Fault(ChannelBase channel)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                    return;

                this.closed = true;

                this.retryTimer.Cancel();

                while (waitQueue.Count > 0)
                    waitQueue.Dequeue().Fault(channel);

                window.Close();
            }
        }

        public MessageAttemptInfo GetMessageInfoForRetry(bool remove)
        {
            lock (this.ThisLock)
            {
                // Closed, no need to retry.
                if (this.closed)
                {
                    return default(MessageAttemptInfo);
                }

                if (remove)
                {
                    if (this.retransmissionWindow.Count == 0)
                    {
                        throw Fx.AssertAndThrow("The caller is not allowed to remove a message attempt when there are no message attempts.");
                    }

                    this.retransmissionWindow.RemoveAt(0);
                }

                while (this.retransmissionWindow.Count > 0)
                {
                    Int64 next = this.retransmissionWindow[0];
                    if (next < this.windowStart)
                    {
                        // Already removed from the window, no need to retry.
                        this.retransmissionWindow.RemoveAt(0);
                    }
                    else
                    {
                        int index = (int)(next - this.windowStart);
                        if (this.window.GetTransferred(index))
                            this.retransmissionWindow.RemoveAt(0);
                        else
                            return new MessageAttemptInfo(this.window.GetMessage(index), next, this.window.GetRetryCount(index), this.window.GetState(index));
                    }
                }

                // Nothing left to retry.
                return default(MessageAttemptInfo);
            }
        }

        public bool SetLast()
        {
            if (this.reliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11)
            {
                throw Fx.AssertAndThrow("SetLast supported only in 1.1.");
            }

            lock (this.ThisLock)
            {
                if (this.last != 0)
                {
                    throw Fx.AssertAndThrow("Cannot set last more than once.");
                }

                this.last = this.windowStart + this.window.Count - 1;
                return (this.last == 0) || this.DoneTransmitting;
            }
        }

        bool InternalAdd(Message message, bool isLast, TimeSpan timeout, object state, out MessageAttemptInfo attemptInfo)
        {
            attemptInfo = default(MessageAttemptInfo);

            WaitQueueAdder adder;

            lock (this.ThisLock)
            {
                if (isLast && this.last != 0)
                {
                    throw Fx.AssertAndThrow("Can't add more than one last message.");
                }

                if (!this.IsAddValid())
                    return false;

                ThrowIfRollover();

                if (CanAdd())
                {
                    attemptInfo = AddToWindow(message, isLast, state);
                    return true;
                }

                adder = new WaitQueueAdder(this, message, isLast, state);
                this.waitQueue.Enqueue(adder);
            }

            attemptInfo = adder.Wait(timeout);
            return true;
        }

        IAsyncResult InternalBeginAdd(Message message, bool isLast, TimeSpan timeout, object state, AsyncCallback callback, object asyncState)
        {
            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            bool isAddValid;

            lock (this.ThisLock)
            {
                if (isLast && this.last != 0)
                {
                    throw Fx.AssertAndThrow("Can't add more than one last message.");
                }

                isAddValid = this.IsAddValid();

                if (isAddValid)
                {
                    ThrowIfRollover();

                    if (CanAdd())
                    {
                        attemptInfo = AddToWindow(message, isLast, state);
                    }
                    else
                    {
                        AsyncQueueAdder adder = new AsyncQueueAdder(message, isLast, timeout, state, this, callback, asyncState);
                        this.waitQueue.Enqueue(adder);

                        return adder;
                    }
                }
            }

            return new CompletedAsyncResult<bool, MessageAttemptInfo>(isAddValid, attemptInfo, callback, asyncState);
        }

        bool InternalEndAdd(IAsyncResult result, out MessageAttemptInfo attemptInfo)
        {
            if (result is CompletedAsyncResult<bool, MessageAttemptInfo>)
            {
                return CompletedAsyncResult<bool, MessageAttemptInfo>.End(result, out attemptInfo);
            }
            else
            {
                attemptInfo = AsyncQueueAdder.End((AsyncQueueAdder)result);
                return true;
            }
        }

        public bool IsFinalAckConsistent(SequenceRangeCollection ranges)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return true;
                }

                // Nothing sent, ensure ack is empty.
                if ((this.windowStart == 1) && (this.window.Count == 0))
                {
                    return ranges.Count == 0;
                }

                // Ack is empty or first range is invalid.
                if (ranges.Count == 0 || ranges[0].Lower != 1)
                {
                    return false;
                }

                return ranges[0].Upper >= (this.windowStart - 1);
            }
        }

        public void ProcessAcknowledgement(SequenceRangeCollection ranges, out bool invalidAck, out bool inconsistentAck)
        {
            invalidAck = false;
            inconsistentAck = false;
            bool newAck = false;
            bool oldAck = false;

            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return;
                }

                Int64 lastMessageSent = this.windowStart + this.window.Count - 1;
                Int64 lastMessageAcked = this.windowStart - 1;
                int transferredInWindow = this.window.TransferredCount;

                for (int i = 0; i < ranges.Count; i++)
                {
                    SequenceRange range = ranges[i];

                    // Ack for a message not yet sent.
                    if (range.Upper > lastMessageSent)
                    {
                        invalidAck = true;
                        return;
                    }

                    if (((range.Lower > 1) && (range.Lower <= lastMessageAcked)) || (range.Upper < lastMessageAcked))
                    {
                        oldAck = true;
                    }

                    if (range.Upper >= this.windowStart)
                    {
                        if (range.Lower <= this.windowStart)
                        {
                            newAck = true;
                        }

                        if (!newAck)
                        {
                            int beginIndex = (int)(range.Lower - this.windowStart);
                            int endIndex = (int)((range.Upper > lastMessageSent) ? (this.window.Count - 1) : (range.Upper - this.windowStart));

                            newAck = this.window.GetTransferredInRangeCount(beginIndex, endIndex) < (endIndex - beginIndex + 1);
                        }

                        if (transferredInWindow > 0 && !oldAck)
                        {
                            int beginIndex = (int)((range.Lower < this.windowStart) ? 0 : (range.Lower - this.windowStart));
                            int endIndex = (int)((range.Upper > lastMessageSent) ? (this.window.Count - 1) : (range.Upper - this.windowStart));

                            transferredInWindow -= this.window.GetTransferredInRangeCount(beginIndex, endIndex);
                        }
                    }
                }

                if (transferredInWindow > 0)
                    oldAck = true;
            }

            inconsistentAck = oldAck && newAck;
        }

        // Called for RequestReply.
        // Argument transferred is the request sequence number and it is assumed to be positive.
        public bool ProcessTransferred(Int64 transferred, int quotaRemaining)
        {
            if (transferred <= 0)
            {
                throw Fx.AssertAndThrow("Argument transferred must be a valid sequence number.");
            }

            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return false;
                }

                return ProcessTransferred(new SequenceRange(transferred), quotaRemaining);
            }
        }

        // Called for Duplex and Output
        public bool ProcessTransferred(SequenceRangeCollection ranges, int quotaRemaining)
        {
            if (ranges.Count == 0)
            {
                return false;
            }

            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return false;
                }

                bool send = false;

                for (int rangeIndex = 0; rangeIndex < ranges.Count; rangeIndex++)
                {
                    if (this.ProcessTransferred(ranges[rangeIndex], quotaRemaining))
                    {
                        send = true;
                    }
                }

                return send;
            }
        }

        // It is necessary that ProcessAcknowledgement be called prior, as 
        // this method does not check for valid ack ranges.
        // This method returns true if the calling method should start sending retries 
        // obtained from GetMessageInfoForRetry.
        bool ProcessTransferred(SequenceRange range, int quotaRemaining)
        {
            if (range.Upper < this.windowStart)
            {
                if (range.Upper == this.windowStart - 1 && (quotaRemaining != -1) && quotaRemaining > this.quotaRemaining)
                    this.quotaRemaining = quotaRemaining - Math.Min(this.windowSize, this.window.Count);

                return false;
            }
            else if (range.Lower <= this.windowStart)
            {
                bool send = false;

                this.retryTimer.Cancel();

                Int64 slide = range.Upper - this.windowStart + 1;

                // For Request Reply: Requests are transferred 1 at a time, (i.e. when the reply comes back).
                // The TransmissionStrategy only removes messages if the window start is removed.
                // Because of this, RequestReply messages transferred out of order will cause many, many retries.
                // To avoid extraneous retries we mark each message transferred, and we remove our virtual slide.
                if (slide == 1)
                {
                    for (int i = 1; i < this.window.Count; i++)
                    {
                        if (this.window.GetTransferred(i))
                        {
                            slide++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Int64 now = Now;
                Int64 oldWindowEnd = this.windowStart + this.windowSize;

                for (int i = 0; i < (int)slide; i++)
                    UpdateStats(now, this.window.GetLastAttemptTime(i));

                if (quotaRemaining != -1)
                {
                    int inFlightAfterAck = Math.Min(this.windowSize, this.window.Count) - (int)slide;
                    this.quotaRemaining = quotaRemaining - Math.Max(0, inFlightAfterAck);
                }

                this.window.Remove((int)slide);

                this.windowStart += slide;

                int sendBeginIndex = 0;

                if (this.windowSize <= this.slowStartThreshold)
                {
                    this.windowSize = Math.Min(this.maxWindowSize, Math.Min(this.slowStartThreshold + 1, this.windowSize + (int)slide));

                    if (!startup)
                        sendBeginIndex = 0;
                    else
                        sendBeginIndex = Math.Max(0, (int)oldWindowEnd - (int)this.windowStart);
                }
                else
                {
                    this.congestionControlModeAcks += (int)slide;

                    // EXPERIMENTAL, needs optimizing ///
                    int segmentSize = Math.Max(1, (this.lossWindowSize - this.slowStartThreshold) / 8);
                    int windowGrowthAckThreshold = ((this.windowSize - this.slowStartThreshold) * this.windowSize) / segmentSize;

                    if (this.congestionControlModeAcks > windowGrowthAckThreshold)
                    {
                        this.congestionControlModeAcks = 0;
                        this.windowSize = Math.Min(this.maxWindowSize, this.windowSize + 1);
                    }

                    sendBeginIndex = Math.Max(0, (int)oldWindowEnd - (int)this.windowStart);
                }

                int sendEndIndex = Math.Min(this.windowSize, this.window.Count);

                if (sendBeginIndex < sendEndIndex)
                {
                    send = (this.retransmissionWindow.Count == 0);

                    for (int i = sendBeginIndex; i < this.windowSize && i < this.window.Count; i++)
                    {
                        Int64 sequenceNumber = this.windowStart + i;

                        if (!this.window.GetTransferred(i) && !this.retransmissionWindow.Contains(sequenceNumber))
                        {
                            this.window.RecordRetry(i, Now);
                            retransmissionWindow.Add(sequenceNumber);
                        }
                    }
                }

                if (window.Count > 0)
                {
                    this.retryTimer.Set(this.Timeout);
                }

                return send;
            }
            else
            {
                for (Int64 i = range.Lower; i <= range.Upper; i++)
                {
                    this.window.SetTransferred((int)(i - this.windowStart));
                }
            }

            return false;
        }

        bool RemoveAdder(IQueueAdder adder)
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                    return false;

                bool removed = false;
                for (int i = 0; i < this.waitQueue.Count; i++)
                {
                    IQueueAdder current = this.waitQueue.Dequeue();

                    if (Object.ReferenceEquals(adder, current))
                        removed = true;
                    else
                        this.waitQueue.Enqueue(current);
                }
                return removed;
            }
        }

        void ThrowIfRollover()
        {
            if (this.windowStart + this.window.Count + this.waitQueue.Count == Int64.MaxValue)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageNumberRolloverFault(this.id).CreateException());
        }

        void UpdateStats(Int64 now, Int64 lastAttemptTime)
        {
            now = Math.Max(now, lastAttemptTime);
            Int64 measuredRtt = now - lastAttemptTime;
            Int64 error = measuredRtt - this.meanRtt;
            this.serrRtt = Math.Min(this.serrRtt + ((Math.Abs(error) - this.serrRtt) >> Constants.Gain), Constants.MaxSerrRtt);
            this.meanRtt = Math.Min(this.meanRtt + (error >> Constants.Gain), Constants.MaxMeanRtt);
            this.timeout = Math.Max(((200 << Constants.TimeMultiplier) * 2) + this.meanRtt, this.meanRtt + (this.serrRtt << Constants.ChebychevFactor));
        }

        class AsyncQueueAdder : WaitAsyncResult, IQueueAdder
        {
            bool isLast;
            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            TransmissionStrategy strategy;

            public AsyncQueueAdder(Message message, bool isLast, TimeSpan timeout, object state, TransmissionStrategy strategy, AsyncCallback callback, object asyncState)
                : base(timeout, true, callback, asyncState)
            {
                // MessageAttemptInfo(Message message, Int64 sequenceNumber, int retryCount, object state)
                // this.attemptInfo is just a state bag, thus sequenceNumber can be 0 and should never be read.
                this.attemptInfo = new MessageAttemptInfo(message, 0, 0, state);
                this.isLast = isLast;
                this.strategy = strategy;
                base.Begin();
            }

            public void Abort(CommunicationObject communicationObject)
            {
                this.attemptInfo.Message.Close();
                OnAborted(communicationObject);
            }

            public void Complete0()
            {
                this.attemptInfo = strategy.AddToWindow(this.attemptInfo.Message, this.isLast, this.attemptInfo.State);
            }

            public void Complete1()
            {
                OnSignaled();
            }

            public static MessageAttemptInfo End(AsyncQueueAdder result)
            {
                AsyncResult.End<AsyncQueueAdder>(result);
                return result.attemptInfo;
            }

            public void Fault(CommunicationObject communicationObject)
            {
                this.attemptInfo.Message.Close();
                OnFaulted(communicationObject);
            }

            protected override string GetTimeoutString(TimeSpan timeout)
            {
                return SR.GetString(SR.TimeoutOnAddToWindow, timeout);
            }

            protected override void OnTimerElapsed(object state)
            {
                if (this.strategy.RemoveAdder(this))
                    base.OnTimerElapsed(state);
            }
        }

        static class Constants
        {
            // Used to adjust the timeout calculation, according to Chebychev's theorem,
            // to fit ~98% of actual rtt's within our timeout.
            public const int ChebychevFactor = 2;

            // Gain of 0.125 (1/8). Shift right by 3 to apply the gain to a term.
            public const int Gain = 3;

            // 1ms == 128 of our time units. Shift left by 7 to perform the multiplication.
            public const int TimeMultiplier = 7;

            // These guarantee no overflows when calculating timeout.
            public const long MaxMeanRtt = long.MaxValue / 3;
            public const long MaxSerrRtt = MaxMeanRtt / 2;
        }

        interface IQueueAdder
        {
            void Abort(CommunicationObject communicationObject);
            void Fault(CommunicationObject communicationObject);
            void Complete0();
            void Complete1();
        }

        class SlidingWindow
        {
            TransmissionInfo[] buffer;
            int head = 0;
            int tail = 0;
            int maxSize;

            public SlidingWindow(int maxSize)
            {
                this.maxSize = maxSize + 1;
                this.buffer = new TransmissionInfo[this.maxSize];
            }

            public int Count
            {
                get
                {
                    if (this.tail >= this.head)
                        return (this.tail - this.head);
                    else
                        return (this.tail - this.head + this.maxSize);
                }
            }

            public int TransferredCount
            {
                get
                {
                    if (this.Count == 0)
                        return 0;
                    else
                        return this.GetTransferredInRangeCount(0, this.Count - 1);
                }
            }

            public void Add(Message message, Int64 addTime, object state)
            {
                if (this.Count >= (this.maxSize - 1))
                {
                    throw Fx.AssertAndThrow("The caller is not allowed to add messages beyond the sliding window's maximum size.");
                }

                this.buffer[this.tail] = new TransmissionInfo(message, addTime, state);
                this.tail = (this.tail + 1) % this.maxSize;
            }

            void AssertIndex(int index)
            {
                if (index >= Count)
                {
                    throw Fx.AssertAndThrow("Argument index must be less than Count.");
                }

                if (index < 0)
                {
                    throw Fx.AssertAndThrow("Argument index must be positive.");
                }
            }

            public void Close()
            {
                this.Remove(Count);
            }

            public Int64 GetLastAttemptTime(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(head + index) % this.maxSize].LastAttemptTime;
            }

            public Message GetMessage(int index)
            {
                this.AssertIndex(index);
                if (!this.buffer[(head + index) % this.maxSize].Transferred)
                    return this.buffer[(head + index) % this.maxSize].Buffer.CreateMessage();
                else
                    return null;
            }

            public int GetRetryCount(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].RetryCount;
            }

            public object GetState(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].State;
            }

            public bool GetTransferred(int index)
            {
                this.AssertIndex(index);
                return this.buffer[(this.head + index) % this.maxSize].Transferred;
            }

            public int GetTransferredInRangeCount(int beginIndex, int endIndex)
            {
                if (beginIndex < 0)
                {
                    throw Fx.AssertAndThrow("Argument beginIndex cannot be negative.");
                }

                if (endIndex >= this.Count)
                {
                    throw Fx.AssertAndThrow("Argument endIndex cannot be greater than Count.");
                }

                if (endIndex < beginIndex)
                {
                    throw Fx.AssertAndThrow("Argument endIndex cannot be less than argument beginIndex.");
                }

                int result = 0;

                for (int index = beginIndex; index <= endIndex; index++)
                {
                    if (this.buffer[(head + index) % this.maxSize].Transferred)
                        result++;
                }

                return result;
            }

            public int RecordRetry(int index, Int64 retryTime)
            {
                this.AssertIndex(index);
                this.buffer[(head + index) % this.maxSize].LastAttemptTime = retryTime;

                return ++this.buffer[(head + index) % this.maxSize].RetryCount;
            }

            public void Remove(int count)
            {
                if (count > this.Count)
                {
                    Fx.Assert("Cannot remove more messages than the window's Count.");
                }

                while (count-- > 0)
                {
                    this.buffer[head].Buffer.Close();
                    this.buffer[head].Buffer = null;
                    this.head = (this.head + 1) % this.maxSize;
                }
            }

            public void SetTransferred(int index)
            {
                this.AssertIndex(index);
                this.buffer[(head + index) % this.maxSize].Transferred = true;
            }

            struct TransmissionInfo
            {
                internal MessageBuffer Buffer;
                internal Int64 LastAttemptTime;
                internal int RetryCount;
                internal object State;
                internal bool Transferred;

                public TransmissionInfo(Message message, Int64 lastAttemptTime, object state)
                {
                    this.Buffer = message.CreateBufferedCopy(int.MaxValue);
                    this.LastAttemptTime = lastAttemptTime;
                    this.RetryCount = 0;
                    this.State = state;
                    this.Transferred = false;
                }
            }
        }

        class WaitQueueAdder : IQueueAdder
        {
            ManualResetEvent completeEvent = new ManualResetEvent(false);
            Exception exception;
            bool isLast;
            MessageAttemptInfo attemptInfo = default(MessageAttemptInfo);
            TransmissionStrategy strategy;

            public WaitQueueAdder(TransmissionStrategy strategy, Message message, bool isLast, object state)
            {
                this.strategy = strategy;
                this.isLast = isLast;
                this.attemptInfo = new MessageAttemptInfo(message, 0, 0, state);
            }

            public void Abort(CommunicationObject communicationObject)
            {
                this.exception = communicationObject.CreateClosedException();
                completeEvent.Set();
            }

            public void Complete0()
            {
                attemptInfo = this.strategy.AddToWindow(this.attemptInfo.Message, this.isLast, this.attemptInfo.State);
                this.completeEvent.Set();
            }

            public void Complete1()
            {
            }

            public void Fault(CommunicationObject communicationObject)
            {
                this.exception = communicationObject.GetTerminalException();
                completeEvent.Set();
            }

            public MessageAttemptInfo Wait(TimeSpan timeout)
            {
                if (!TimeoutHelper.WaitOne(this.completeEvent, timeout))
                {
                    if (this.strategy.RemoveAdder(this) && this.exception == null)
                        this.exception = new TimeoutException(SR.GetString(SR.TimeoutOnAddToWindow, timeout));
                }

                if (this.exception != null)
                {
                    this.attemptInfo.Message.Close();
                    this.completeEvent.Close();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                }

                // This is safe because, Abort, Complete0, Fault, and RemoveAdder all occur under 
                // the TransmissionStrategy's lock and RemoveAdder ensures that the 
                // TransmissionStrategy will never call into this object again.
                this.completeEvent.Close();
                return this.attemptInfo;
            }
        }
    }
}
