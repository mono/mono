//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;

    abstract class InputQueueChannel<TDisposable> : ChannelBase
        where TDisposable : class, IDisposable
    {
        InputQueue<TDisposable> inputQueue;

        protected InputQueueChannel(ChannelManagerBase channelManager)
            : base(channelManager)
        {
            this.inputQueue = TraceUtility.CreateInputQueue<TDisposable>();
        }

        public int InternalPendingItems
        {
            get
            {
                return this.inputQueue.PendingCount;
            }
        }

        public int PendingItems
        {
            get
            {
                ThrowIfDisposedOrNotOpen();
                return InternalPendingItems;
            }
        }

        public void EnqueueAndDispatch(TDisposable item)
        {
            EnqueueAndDispatch(item, null);
        }

        public void EnqueueAndDispatch(TDisposable item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            OnEnqueueItem(item);

            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            inputQueue.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
        }

        public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            inputQueue.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }

        public void EnqueueAndDispatch(TDisposable item, Action dequeuedCallback)
        {
            OnEnqueueItem(item);

            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            inputQueue.EnqueueAndDispatch(item, dequeuedCallback);
        }

        public bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
        {
            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            return inputQueue.EnqueueWithoutDispatch(exception, dequeuedCallback);
        }

        public bool EnqueueWithoutDispatch(TDisposable item, Action dequeuedCallback)
        {
            OnEnqueueItem(item);

            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            return inputQueue.EnqueueWithoutDispatch(item, dequeuedCallback);
        }

        public void Dispatch()
        {
            // NOTE: don't need to check IsDisposed here: InputQueue will handle dispose
            inputQueue.Dispatch();
        }

        public void Shutdown()
        {
            inputQueue.Shutdown();
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            inputQueue.Shutdown(() => this.GetPendingException());
        }

        protected virtual void OnEnqueueItem(TDisposable item)
        {
        }

        protected IAsyncResult BeginDequeue(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfNotOpened();
            return inputQueue.BeginDequeue(timeout, callback, state);
        }

        protected bool EndDequeue(IAsyncResult result, out TDisposable item)
        {
            bool dequeued = inputQueue.EndDequeue(result, out item);

            if (item == null)
            {
                this.ThrowIfFaulted();
                this.ThrowIfAborted();
            }

            return dequeued;
        }

        protected bool Dequeue(TimeSpan timeout, out TDisposable item)
        {
            this.ThrowIfNotOpened();
            bool dequeued = inputQueue.Dequeue(timeout, out item);

            if (item == null)
            {
                this.ThrowIfFaulted();
                this.ThrowIfAborted();
            }

            return dequeued;
        }

        protected bool WaitForItem(TimeSpan timeout)
        {
            this.ThrowIfNotOpened();
            bool dequeued = inputQueue.WaitForItem(timeout);

            this.ThrowIfFaulted();
            this.ThrowIfAborted();

            return dequeued;
        }

        protected IAsyncResult BeginWaitForItem(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfNotOpened();
            return inputQueue.BeginWaitForItem(timeout, callback, state);
        }

        protected bool EndWaitForItem(IAsyncResult result)
        {
            bool dequeued = inputQueue.EndWaitForItem(result);

            this.ThrowIfFaulted();
            this.ThrowIfAborted();

            return dequeued;
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            inputQueue.Shutdown(() => this.GetPendingException());
        }

        protected override void OnAbort()
        {
            inputQueue.Close();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            inputQueue.Close();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            inputQueue.Close();
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }
    }
}
