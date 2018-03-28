//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{

    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Threading;

    class MultipleReceiveBinder : IChannelBinder
    {
        internal static class MultipleReceiveDefaults
        {
            internal const int MaxPendingReceives = 1;
        }

        static AsyncCallback onInnerReceiveCompleted = Fx.ThunkCallback(OnInnerReceiveCompleted);

        MultipleReceiveAsyncResult outstanding;
        IChannelBinder channelBinder;
        ReceiveScopeQueue pendingResults;
        bool ordered;

        public MultipleReceiveBinder(IChannelBinder channelBinder, int size, bool ordered)
        {
            this.ordered = ordered;
            this.channelBinder = channelBinder;
            this.pendingResults = new ReceiveScopeQueue(size);
        }

        public IChannel Channel
        {
            get { return this.channelBinder.Channel; }
        }

        public bool HasSession
        {
            get { return this.channelBinder.HasSession; }
        }

        public Uri ListenUri
        {
            get { return this.channelBinder.ListenUri; }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.channelBinder.LocalAddress; }
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.channelBinder.RemoteAddress; }
        }

        public void Abort()
        {
            this.channelBinder.Abort();
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.channelBinder.CloseAfterFault(timeout);
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.channelBinder.TryReceive(timeout, out requestContext);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // At anytime there can be only one thread in BeginTryReceive and the 
            // outstanding AsyncResult should have completed before the next one.
            // There should be no pending oustanding result here.
            Fx.AssertAndThrow(this.outstanding == null, "BeginTryReceive should not have a pending result.");

            MultipleReceiveAsyncResult multipleReceiveResult = new MultipleReceiveAsyncResult(callback, state);
            this.outstanding = multipleReceiveResult;
            EnsurePump(timeout);
            IAsyncResult innerResult;
            if (this.pendingResults.TryDequeueHead(out innerResult))
            {
                HandleReceiveRequestComplete(innerResult, true);
            }

            return multipleReceiveResult;
        }

        void EnsurePump(TimeSpan timeout)
        {
            // ensure we're running at full throttle, the BeginTryReceive calls we make below on the
            // IChannelBinder will typically complete future calls to BeginTryReceive made by CannelHandler
            // corollary to that is that most times these calls will be completed sycnhronously
            while (!this.pendingResults.IsFull)
            {
                ReceiveScopeSignalGate receiveScope = new ReceiveScopeSignalGate(this);

                // Enqueue the result without locks since this is the pump. 
                // BeginTryReceive can be called only from one thread and 
                // the head is not yet unlocked so no items can proceed.
                this.pendingResults.Enqueue(receiveScope);
                IAsyncResult result = this.channelBinder.BeginTryReceive(timeout, onInnerReceiveCompleted, receiveScope);
                if (result.CompletedSynchronously)
                {
                    this.SignalReceiveCompleted(result);
                }
            }
        }

        static void OnInnerReceiveCompleted(IAsyncResult nestedResult)
        {
            if (nestedResult.CompletedSynchronously)
            {
                return;
            }

            ReceiveScopeSignalGate thisPtr = nestedResult.AsyncState as ReceiveScopeSignalGate;
            thisPtr.Binder.HandleReceiveAndSignalCompletion(nestedResult, false);
        }

        void HandleReceiveAndSignalCompletion(IAsyncResult nestedResult, bool completedSynchronosly)
        {
            if (SignalReceiveCompleted(nestedResult))
            {
                HandleReceiveRequestComplete(nestedResult, completedSynchronosly);
            }
        }

        private bool SignalReceiveCompleted(IAsyncResult nestedResult)
        {
            if (this.ordered)
            {
                // Ordered recevies can proceed only if its own gate has 
                // been unlocked. Head is the only gate unlocked and only the 
                // result that owns the is the gate at the head can proceed.
                return this.pendingResults.TrySignal((ReceiveScopeSignalGate)nestedResult.AsyncState, nestedResult);
            }
            else
            {
                // Unordered receives can proceed with any gate. If the is head 
                // is not unlocked by BeginTryReceive then the result will 
                // be put on the last pending gate.
                return this.pendingResults.TrySignalPending(nestedResult);
            }
        }

        void HandleReceiveRequestComplete(IAsyncResult innerResult, bool completedSynchronously)
        {
            MultipleReceiveAsyncResult receiveResult = this.outstanding;
            Exception completionException = null;

            try
            {
                Fx.AssertAndThrow(receiveResult != null, "HandleReceive invoked without an outstanding result");
                // Cleanup states
                this.outstanding = null;

                // set the context on the outer result for the ChannelHandler.
                RequestContext context;
                receiveResult.Valid = this.channelBinder.EndTryReceive(innerResult, out context);
                receiveResult.RequestContext = context;
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                completionException = ex;
            }

            receiveResult.Complete(completedSynchronously, completionException);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            return MultipleReceiveAsyncResult.End(result, out requestContext);
        }

        public RequestContext CreateRequestContext(Message message)
        {
            return this.channelBinder.CreateRequestContext(message);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channelBinder.Send(message, timeout);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.channelBinder.EndSend(result);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.channelBinder.Request(message, timeout);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginRequest(message, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return this.channelBinder.EndRequest(result);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.channelBinder.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channelBinder.EndWaitForMessage(result);
        }

        class MultipleReceiveAsyncResult : AsyncResult
        {
            public MultipleReceiveAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public bool Valid
            {
                get;
                set;
            }

            public RequestContext RequestContext
            {
                get;
                set;
            }

            public new void Complete(bool completedSynchronously, Exception completionException)
            {
                base.Complete(completedSynchronously, completionException);
            }

            public static bool End(IAsyncResult result, out RequestContext context)
            {
                MultipleReceiveAsyncResult thisPtr = AsyncResult.End<MultipleReceiveAsyncResult>(result);
                context = thisPtr.RequestContext;
                return thisPtr.Valid;
            }
        }

        class ReceiveScopeSignalGate : SignalGate<IAsyncResult>
        {
            public ReceiveScopeSignalGate(MultipleReceiveBinder binder)
            {
                this.Binder = binder;
            }

            public MultipleReceiveBinder Binder
            {
                get;
                private set;
            }
        }

        class ReceiveScopeQueue
        {
            // This class is a circular queue with 2 pointers for pending items and head.
            // Ordered Receives : The head is unlocked by BeginTryReceive. The ReceiveGate can signal only the 
            // the gate that it owns. If the gate is the head then it will proceed.
            // Unordered Receives:  Any pending item can be signalled. The pending index keeps track 
            // of results that haven't  been completed. If the head is unlocked then it will proceed.

            int pending;
            int head;
            int count;
            readonly int size;
            ReceiveScopeSignalGate[] items;

            public ReceiveScopeQueue(int size)
            {
                this.size = size;
                this.head = 0;
                this.count = 0;
                this.pending = 0;
                items = new ReceiveScopeSignalGate[size];
            }

            internal bool IsFull
            {
                get { return this.count == this.size; }
            }

            internal void Enqueue(ReceiveScopeSignalGate receiveScope)
            {
                // This should only be called from EnsurePump which itself should only be 
                // BeginTryReceive. This makes sure that we don't need locks to enqueue an item.
                Fx.AssertAndThrow(this.count < this.size, "Cannot Enqueue into a full queue.");
                this.items[(this.head + this.count) % this.size] = receiveScope;
                count++;
            }

            void Dequeue()
            {
                // Dequeue should not be called outside a signal/unlock boundary.
                // There are no locks as this boundary ensures that only one thread 
                // Tries to dequeu an item either in the unlock or Signal thread.
                Fx.AssertAndThrow(this.count > 0, "Cannot Dequeue and empty queue.");
                this.items[head] = null;
                this.head = (head + 1) % this.size;
                this.count--;
            }

            internal bool TryDequeueHead(out IAsyncResult result)
            {
                // Invoked only from BeginTryReceive as only the main thread can 
                // dequeue the head and is  Successful only if it's already been signaled and completed.
                Fx.AssertAndThrow(this.count > 0, "Cannot unlock item when queue is empty");
                if (this.items[head].Unlock(out result))
                {
                    this.Dequeue();
                    return true;
                }

                return false;
            }

            public bool TrySignal(ReceiveScopeSignalGate scope, IAsyncResult nestedResult)
            {
                // Ordered receives can only signal the gate that the AsyncResult owns.
                // If the head has already been unlocked then it can proceed.
                if (scope.Signal(nestedResult))
                {
                    Dequeue();
                    return true;
                }

                return false;
            }

            public bool TrySignalPending(IAsyncResult result)
            {
                // free index will wrap around and always return the next free index;
                // Only the head of the queue can proceed as the head would be unlocked by
                // BeginTryReceive. All other requests will just submit their completed result.
                int nextPending = GetNextPending();
                if (this.items[nextPending].Signal(result))
                {
                    Dequeue();
                    return true;
                }

                return false;
            }

            int GetNextPending()
            {
                int slot = this.pending;
                while (true)
                {
                    if (slot == (slot = Interlocked.CompareExchange(ref this.pending, (slot + 1) % this.size, slot)))
                    {
                        return slot;
                    }
                }
            }
        }
    }
}
