//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.Threading;

    abstract class RandomDelaySendsAsyncResult : AsyncResult
    {
        readonly ICommunicationObject channel;
        IOThreadTimer timer;
        TimeoutHelper timeoutHelper;
        TimeSpan maxDelay;
        long startTicks;
        long[] delaysInTicks;
        int numSends;
        Action<object> onTimerCallback;
        AsyncCallback onSendCompletedCallback;
        AsyncCallback onCloseCompletedCallback;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int currentSendIndex;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        long completesCounter;
        
        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        long sendCompletesCounter;

        bool cancelled;

        [Fx.Tag.SynchronizationObject()]
        object thisLock;

        protected RandomDelaySendsAsyncResult(int numSends, TimeSpan maxDelay, AsyncCallback callback, object state)
            : this(numSends, maxDelay, null, callback, state)
        {
        }

        protected RandomDelaySendsAsyncResult(int numSends, TimeSpan maxDelay, ICommunicationObject channel, AsyncCallback callback, object state)
            : this(numSends, maxDelay, channel, null, callback, state)
        {
        }

        protected RandomDelaySendsAsyncResult(int numSends, TimeSpan maxDelay, ICommunicationObject channel, Random random, AsyncCallback callback, object state)
            : base(callback, state)
        {
            Fx.Assert(numSends > 0, "The numSends must be positive.");
            Fx.Assert(maxDelay >= TimeSpan.Zero, "The maxDelay must be non negative.");

            this.onTimerCallback = new Action<object>(OnTimer);
            this.onSendCompletedCallback = Fx.ThunkCallback(new AsyncCallback(OnSendCompleted));
            this.channel = channel;
            if (this.channel != null)
            {
                this.onCloseCompletedCallback = Fx.ThunkCallback(new AsyncCallback(OnCloseCompleted));
            }
            this.numSends = numSends;
            this.maxDelay = maxDelay;
            this.completesCounter = 0;
            this.sendCompletesCounter = 0;
            this.cancelled = false;
            this.thisLock = new object();
            if (maxDelay != TimeSpan.Zero)
            {
                this.delaysInTicks = new long[numSends];
                Random innerRandom = (random != null) ? random : new Random();
                for (int i = 0; i < this.numSends; i++)
                {
                    this.delaysInTicks[i] = RandomDelay(innerRandom, maxDelay.Ticks);
                }
                Array.Sort<long>(this.delaysInTicks);
            }
        }

        public void Start(TimeSpan timeout)
        {
            if (this.cancelled)
            {
                return;
            }
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.timeoutHelper.RemainingTime();
            if (this.maxDelay == TimeSpan.Zero)
            {
                StartZeroDelay();
            }
            else
            {
                StartSchedule();
            }
        }

        void StartSchedule()
        {
            this.currentSendIndex = -1;
            this.timer = new IOThreadTimer(this.onTimerCallback, this, false);
            this.startTicks = Ticks.Now;
            Schedule(0);
        }

        void StartZeroDelay()
        {
            for (this.currentSendIndex = 0; this.currentSendIndex < this.numSends; this.currentSendIndex++)
            {
                IAsyncResult result = OnBeginSend(this.currentSendIndex, this.timeoutHelper.RemainingTime(), this.onSendCompletedCallback, null);
                if (result.CompletedSynchronously)
                {
                    OnEndSend(result);
                    if (Threading.Interlocked.Increment(ref this.sendCompletesCounter) == this.numSends)
                    {
                        CompleteSends(true);
                    }
                }
            }
        }

        void Schedule(int index)
        {
            if (index < this.numSends)
            {
                this.timer.SetAt(this.startTicks + this.delaysInTicks[index]);
            }
        }

        void StartSend(int index)
        {
            Exception error = null;
            IAsyncResult result;
            bool compeletedSynchronously = false;
            try
            {
                result = OnBeginSend(index, this.timeoutHelper.RemainingTime(), this.onSendCompletedCallback, null);
                if (result.CompletedSynchronously)
                {
                    compeletedSynchronously = true;
                    OnEndSend(result);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null)
            {
                CallCompleteOnce(false, error);
            }
            else
            {
                if (compeletedSynchronously)
                {
                    if (Threading.Interlocked.Increment(ref this.sendCompletesCounter) == this.numSends)
                    {
                        CompleteSends(false);
                    }
                }
            }
        }

        void OnTimer(object state)
        {
            int index = Threading.Interlocked.Increment(ref this.currentSendIndex);
            StartSend(index);
            Schedule(index + 1);
        }

        void OnSendCompleted(IAsyncResult result)
        {
            Exception error = null;
            if (!result.CompletedSynchronously)
            {
                try
                {
                    OnEndSend(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error = e;
                }
                if (error != null)
                {
                    CallCompleteOnce(false, error);
                }
                else
                {
                    if (Threading.Interlocked.Increment(ref this.sendCompletesCounter) == this.numSends)
                    {
                        CompleteSends(false);
                    }
                }
            }
        }

        void CompleteSends(bool sendsCompletedSynchronously)
        {
            Exception error = null;
            bool compeletedSynchronously = false;
            if (this.channel != null && !this.IsCompleted)
            {
                try
                {
                    IAsyncResult result = this.channel.BeginClose(this.timeoutHelper.RemainingTime(), onCloseCompletedCallback, null);
                    if (result.CompletedSynchronously)
                    {
                        this.channel.EndClose(result);
                        compeletedSynchronously = true;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error = e;
                }
                if (error != null)
                {
                    CallCompleteOnce(false, error);
                }
                if (compeletedSynchronously)
                {
                    CallCompleteOnce(sendsCompletedSynchronously, null);
                }
            }
            else
            {
                CallCompleteOnce(sendsCompletedSynchronously, null);
            }
        }

        void OnCloseCompleted(IAsyncResult result)
        {
            Exception error = null;
            if (!result.CompletedSynchronously)
            {
                try
                {
                    this.channel.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error = e;
                }
                if (error != null)
                {
                    CallCompleteOnce(false, error);
                }
                CallCompleteOnce(false, null);
            }
        }

        void CallCompleteOnce(bool completedSynchronously, Exception e)
        {
            if (Threading.Interlocked.Increment(ref this.completesCounter) == 1)
            {
                if (e != null)
                {
                    Cancel();
                }
                Complete(completedSynchronously, e);
            }
        }

        void CompleteOnCancel()
        {
            if (Threading.Interlocked.Increment(ref this.completesCounter) == 1)
            {
                Complete(false, new OperationCanceledException());
            }
        }

        public void Cancel()
        {
            if (!this.cancelled)
            {
                bool doCancel = false;
                lock (this.thisLock)
                {
                    if (!this.cancelled)
                    {
                        doCancel = true;
                        this.cancelled = true;
                    }
                }
                if (doCancel)
                {
                    if (this.timer != null)
                    {
                        this.timer.Cancel();
                    }
                    if (this.channel != null)
                    {
                        this.channel.Abort();
                    }
                    CompleteOnCancel();
                }
            }
        }

        // returns random in tick between 0 and maxTicks
        public static long RandomDelay(Random randomGenerator, long maxTicks)
        {
            double ticks = maxTicks;
            return (long)(ticks * randomGenerator.NextDouble());
        }

        protected abstract IAsyncResult OnBeginSend(int index, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnEndSend(IAsyncResult result);
    }
}
