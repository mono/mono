//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.Threading;
    using System.Collections.Generic;

    abstract class RandomDelayQueuedSendsAsyncResult<TItem> : 
        IteratorAsyncResult<RandomDelayQueuedSendsAsyncResult<TItem>>
        where TItem : class
    {
        readonly InputQueue<TItem> itemQueue;        
        readonly Random random;
        readonly double maxRandomDelayInMillis;
        readonly int[] preCalculatedDelays;
        readonly bool doDelay;

        static AsyncStep dequeueStep;
        static AsyncStep delayStep;
        static AsyncStep sendItemStep;

        TItem currentItem;
        int currentDelayIndex;

        public RandomDelayQueuedSendsAsyncResult(
            TimeSpan maxRandomDelay,
            InputQueue<TItem> itemQueue,            
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
            Fx.Assert(maxRandomDelay >= TimeSpan.Zero, "The maxRandomDelay parameter must be non negative.");
            Fx.Assert(itemQueue != null, "The itemQueue parameter must be non null.");            

            this.itemQueue = itemQueue;

            this.doDelay = maxRandomDelay > TimeSpan.Zero;
            if (this.doDelay)
            {
                this.random = new Random();
                this.maxRandomDelayInMillis = maxRandomDelay.TotalMilliseconds;

                if (this.itemQueue.PendingCount > 0)
                {
                    this.preCalculatedDelays = new int[this.itemQueue.PendingCount];
                    this.PreCalculateSendDelays();
                }
            }
        }

        public IAsyncResult BeginDelay(AsyncCallback callback, object state)
        {
            return new DelayAsyncResult(this, callback, state);
        }

        public void EndDelay(IAsyncResult result)
        {
            DelayAsyncResult.End(result);
        }

        protected override IEnumerator<AsyncStep> GetAsyncSteps()
        {
            while (true)
            {
                yield return RandomDelayQueuedSendsAsyncResult<TItem>.GetDequeueStep();

                if (this.currentItem == null)
                {
                    yield break;
                }

                if (this.doDelay)
                {
                    yield return RandomDelayQueuedSendsAsyncResult<TItem>.GetDelayStep();
                }

                yield return RandomDelayQueuedSendsAsyncResult<TItem>.GetSendItemStep();
            }
        }

        protected void Start(TimeSpan timeout)
        {
            this.Start(this, timeout);
        }

        protected abstract IAsyncResult OnBeginSendItem(
            TItem item,
            TimeSpan timeout,
            AsyncCallback callback,
            object state);

        protected abstract void OnEndSendItem(IAsyncResult result);

        static AsyncStep GetDequeueStep()
        {
            if (dequeueStep == null)
            {
                dequeueStep = RandomDelayQueuedSendsAsyncResult<TItem>.CallAsync(
                    (thisPtr, t, c, s) => thisPtr.itemQueue.BeginDequeue(TimeSpan.MaxValue, c, s),
                    (thisPtr, r) => thisPtr.currentItem = thisPtr.itemQueue.EndDequeue(r));
            }

            return dequeueStep;
        }

        static AsyncStep GetDelayStep()
        {
            if (delayStep == null)
            {
                delayStep = RandomDelayQueuedSendsAsyncResult<TItem>.CallAsync(
                    (thisPtr, t, c, s) => thisPtr.BeginDelay(c, s),
                    (thisPtr, r) => thisPtr.EndDelay(r));
            }

            return delayStep;
        }

        static AsyncStep GetSendItemStep()
        {
            if (sendItemStep == null)
            {
                sendItemStep = RandomDelayQueuedSendsAsyncResult<TItem>.CallParallel(
                    (thisPtr, t, c, s) => thisPtr.OnBeginSendItem(thisPtr.currentItem, t, c, s),
                    (thisPtr, r) => thisPtr.OnEndSendItem(r));
            }

            return sendItemStep;
        }

        void PreCalculateSendDelays()
        {
            this.currentDelayIndex = 0;
            for (int i = 0; i < this.preCalculatedDelays.Length; i++)
            {
                this.preCalculatedDelays[i] = (int)(this.random.NextDouble() * this.maxRandomDelayInMillis);
            }

            Array.Sort<int>(this.preCalculatedDelays);
        }

        int GetNextDelay()
        {
            int delay = 0;

            if ((this.preCalculatedDelays == null) || (this.preCalculatedDelays.Length == 0))
            {
                delay = (int)(this.maxRandomDelayInMillis * this.random.NextDouble());
            }
            else
            {
                if (this.preCalculatedDelays.Length == 1 || this.currentDelayIndex == 0)
                {
                    delay = this.preCalculatedDelays[0];
                }
                else
                {
                    this.currentDelayIndex++;
                    if (currentDelayIndex == this.preCalculatedDelays.Length)
                    {
                        this.currentDelayIndex = 1;
                    }

                    delay = this.preCalculatedDelays[this.currentDelayIndex] -
                        this.preCalculatedDelays[this.currentDelayIndex - 1];
                }
            }

            return delay;
        }

        class DelayAsyncResult : AsyncResult
        {
            readonly IOThreadTimer delayTimer;
            static Action<object> onDelayCompletedCallback = new Action<object>(OnDelayCompleted);

            public DelayAsyncResult(
                RandomDelayQueuedSendsAsyncResult<TItem> parent,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                int delay = parent.GetNextDelay();
                if (delay != 0)
                {
                    this.delayTimer = new IOThreadTimer(onDelayCompletedCallback, this, true);
                    this.delayTimer.Set(delay);
                }
                else
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DelayAsyncResult>(result);
            }

            static void OnDelayCompleted(object state)
            {
                DelayAsyncResult thisPtr = (DelayAsyncResult)state;
                thisPtr.delayTimer.Cancel();
                thisPtr.Complete(false);
            }
        }
    }
}
