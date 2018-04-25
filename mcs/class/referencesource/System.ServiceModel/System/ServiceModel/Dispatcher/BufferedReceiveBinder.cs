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

    class BufferedReceiveBinder : IChannelBinder
    {
        static Action<object> tryReceive = new Action<object>(BufferedReceiveBinder.TryReceive);
        static AsyncCallback tryReceiveCallback = Fx.ThunkCallback(new AsyncCallback(TryReceiveCallback));

        IChannelBinder channelBinder;
        InputQueue<RequestContextWrapper> inputQueue;

        [Fx.Tag.SynchronizationObject(Blocking = true, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int pendingOperationSemaphore;

        public BufferedReceiveBinder(IChannelBinder channelBinder)
        {
            this.channelBinder = channelBinder;
            this.inputQueue = new InputQueue<RequestContextWrapper>();
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
            this.inputQueue.Close();
            this.channelBinder.Abort();
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.inputQueue.Close();
            this.channelBinder.CloseAfterFault(timeout);
        }

        // Locking:
        // Only 1 channelBinder operation call should be active at any given time. All future calls
        // will wait on the inputQueue. The semaphore is always released right before the Dispatch on the inputQueue.
        // This protects a new call racing with an existing operation that is just about to fully complete.

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            if (Interlocked.CompareExchange(ref this.pendingOperationSemaphore, 1, 0) == 0)
            {
                ActionItem.Schedule(tryReceive, this);
            }

            RequestContextWrapper wrapper;
            bool success = this.inputQueue.Dequeue(timeout, out wrapper);

            if (success && wrapper != null)
            {
                requestContext = wrapper.RequestContext;
            }
            else
            {
                requestContext = null;
            }

            return success;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (Interlocked.CompareExchange(ref this.pendingOperationSemaphore, 1, 0) == 0)
            {
                IAsyncResult result = this.channelBinder.BeginTryReceive(timeout, tryReceiveCallback, this);
                if (result.CompletedSynchronously)
                {
                    HandleEndTryReceive(result);
                }
            }

            return this.inputQueue.BeginDequeue(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            RequestContextWrapper wrapper;
            bool success = this.inputQueue.EndDequeue(result, out wrapper);

            if (success && wrapper != null)
            {
                requestContext = wrapper.RequestContext;
            }
            else
            {
                requestContext = null;
            }
            return success;
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

        internal void InjectRequest(RequestContext requestContext)
        {
            // Reuse the existing requestContext
            this.inputQueue.EnqueueAndDispatch(new RequestContextWrapper(requestContext));
        }

        //
        // TryReceive threads
        //

        static void TryReceive(object state)
        {
            BufferedReceiveBinder binder = (BufferedReceiveBinder)state;

            RequestContext requestContext;
            bool requiresDispatch = false;
            try
            {
                if (binder.channelBinder.TryReceive(TimeSpan.MaxValue, out requestContext))
                {
                    requiresDispatch = binder.inputQueue.EnqueueWithoutDispatch(new RequestContextWrapper(requestContext), null);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                requiresDispatch = binder.inputQueue.EnqueueWithoutDispatch(exception, null);
            }
            finally
            {
                Interlocked.Exchange(ref binder.pendingOperationSemaphore, 0);
                if (requiresDispatch)
                {
                    binder.inputQueue.Dispatch();
                }
            }
        }

        static void TryReceiveCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            HandleEndTryReceive(result);
        }

        static void HandleEndTryReceive(IAsyncResult result)
        {
            BufferedReceiveBinder binder = (BufferedReceiveBinder)result.AsyncState;

            RequestContext requestContext;
            bool requiresDispatch = false;
            try
            {
                if (binder.channelBinder.EndTryReceive(result, out requestContext))
                {
                    requiresDispatch = binder.inputQueue.EnqueueWithoutDispatch(new RequestContextWrapper(requestContext), null);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                requiresDispatch = binder.inputQueue.EnqueueWithoutDispatch(exception, null);
            }
            finally
            {
                Interlocked.Exchange(ref binder.pendingOperationSemaphore, 0);
                if (requiresDispatch)
                {
                    binder.inputQueue.Dispatch();
                }
            }
        }

        // A RequestContext may be 'null' (some pieces of ChannelHandler depend on this) but the InputQueue
        // will not allow null items to be enqueued. Wrap the RequestContexts in another object to
        // facilitate this semantic
        class RequestContextWrapper
        {
            public RequestContextWrapper(RequestContext requestContext)
            {
                this.RequestContext = requestContext;
            }

            public RequestContext RequestContext
            {
                get;
                private set;
            }
        }
    }
}
