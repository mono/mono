//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    class ConcurrencyBehavior
    {
        ConcurrencyMode concurrencyMode;
        bool enforceOrderedReceive;
        bool supportsTransactedBatch;

        internal ConcurrencyBehavior(DispatchRuntime runtime)
        {
            this.concurrencyMode = runtime.ConcurrencyMode;
            this.enforceOrderedReceive = runtime.EnsureOrderedDispatch;
            this.supportsTransactedBatch = ConcurrencyBehavior.SupportsTransactedBatch(runtime.ChannelDispatcher);
        }

        static bool SupportsTransactedBatch(ChannelDispatcher channelDispatcher)
        {
            return channelDispatcher.IsTransactedReceive && (channelDispatcher.MaxTransactedBatchSize > 0);
        }

        internal bool IsConcurrent(ref MessageRpc rpc)
        {
            return IsConcurrent(this.concurrencyMode, this.enforceOrderedReceive, rpc.Channel.HasSession, this.supportsTransactedBatch);
        }

        internal static bool IsConcurrent(ConcurrencyMode concurrencyMode, bool ensureOrderedDispatch, bool hasSession, bool supportsTransactedBatch)
        {
            if (supportsTransactedBatch)
            {
                return false;
            }

            if (concurrencyMode != ConcurrencyMode.Single)
            {
                return true;
            }

            if (hasSession)
            {
                return false;
            }

            if (ensureOrderedDispatch)
            {
                return false;
            }

            return true;
        }

        internal static bool IsConcurrent(ChannelDispatcher runtime, bool hasSession)
        {
            bool isConcurrencyModeSingle = true;

            if (ConcurrencyBehavior.SupportsTransactedBatch(runtime))
            {
                return false;
            }

            foreach (EndpointDispatcher endpointDispatcher in runtime.Endpoints)
            {
                if (endpointDispatcher.DispatchRuntime.EnsureOrderedDispatch)
                {
                    return false;
                }

                if (endpointDispatcher.DispatchRuntime.ConcurrencyMode != ConcurrencyMode.Single)
                {
                    isConcurrencyModeSingle = false;
                }
            }

            if (!isConcurrencyModeSingle)
            {
                return true;
            }

            if (!hasSession)
            {
                return true;
            }

            return false;
        }

        internal void LockInstance(ref MessageRpc rpc)
        {
            if (this.concurrencyMode != ConcurrencyMode.Multiple)
            {
                ConcurrencyInstanceContextFacet resource = rpc.InstanceContext.Concurrency;
                lock (rpc.InstanceContext.ThisLock)
                {
                    if (!resource.Locked)
                    {
                        resource.Locked = true;
                    }
                    else
                    {
                        MessageRpcWaiter waiter = new MessageRpcWaiter(rpc.Pause());
                        resource.EnqueueNewMessage(waiter);
                    }
                }

                if (this.concurrencyMode == ConcurrencyMode.Reentrant)
                {
                    rpc.OperationContext.IsServiceReentrant = true;
                }
            }
        }

        internal void UnlockInstance(ref MessageRpc rpc)
        {
            if (this.concurrencyMode != ConcurrencyMode.Multiple)
            {
                ConcurrencyBehavior.UnlockInstance(rpc.InstanceContext);
            }
        }

        internal static void UnlockInstanceBeforeCallout(OperationContext operationContext)
        {
            if (operationContext != null && operationContext.IsServiceReentrant)
            {
                ConcurrencyBehavior.UnlockInstance(operationContext.InstanceContext);
            }
        }

        static void UnlockInstance(InstanceContext instanceContext)
        {
            ConcurrencyInstanceContextFacet resource = instanceContext.Concurrency;

            lock (instanceContext.ThisLock)
            {
                if (resource.HasWaiters)
                {
                    IWaiter nextWaiter = resource.DequeueWaiter();
                    nextWaiter.Signal();
                }
                else
                {
                    //We have no pending Callouts and no new Messages to process
                    resource.Locked = false;
                }
            }
        }

        internal static void LockInstanceAfterCallout(OperationContext operationContext)
        {
            if (operationContext != null)
            {
                InstanceContext instanceContext = operationContext.InstanceContext;
                
                if (operationContext.IsServiceReentrant)
                {
                    ConcurrencyInstanceContextFacet resource = instanceContext.Concurrency;
                    ThreadWaiter waiter = null;

                    lock (instanceContext.ThisLock)
                    {
                        if (!resource.Locked)
                        {
                            resource.Locked = true;
                        }
                        else
                        {
                            waiter = new ThreadWaiter();
                            resource.EnqueueCalloutMessage(waiter);
                        }
                    }

                    if (waiter != null)
                    {
                        waiter.Wait();
                    }
                }
            }
        }

        internal interface IWaiter
        {
            void Signal();
        }

        class MessageRpcWaiter : IWaiter
        {
            IResumeMessageRpc resume;

            internal MessageRpcWaiter(IResumeMessageRpc resume)
            {
                this.resume = resume;
            }

            void IWaiter.Signal()
            {
                try
                {
                    bool alreadyResumedNoLock;
                    this.resume.Resume(out alreadyResumedNoLock);

                    if (alreadyResumedNoLock)
                    {
                        Fx.Assert("ConcurrencyBehavior resumed more than once for same call.");
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                }
            }
        }

        class ThreadWaiter : IWaiter
        {
            ManualResetEvent wait = new ManualResetEvent(false);

            void IWaiter.Signal()
            {
                this.wait.Set();
            }

            internal void Wait()
            {
                this.wait.WaitOne();
                this.wait.Close();
            }
        }
    }

    internal class ConcurrencyInstanceContextFacet
    {
        internal bool Locked;
        Queue<ConcurrencyBehavior.IWaiter> calloutMessageQueue;
        Queue<ConcurrencyBehavior.IWaiter> newMessageQueue;

        internal bool HasWaiters
        {
            get
            {
                return (((this.calloutMessageQueue != null) && (this.calloutMessageQueue.Count > 0)) ||
                        ((this.newMessageQueue != null) && (this.newMessageQueue.Count > 0)));
            }
        }

        ConcurrencyBehavior.IWaiter DequeueFrom(Queue<ConcurrencyBehavior.IWaiter> queue)
        {
            ConcurrencyBehavior.IWaiter waiter = queue.Dequeue();

            if (queue.Count == 0)
            {
                queue.TrimExcess();
            }

            return waiter;
        }

        internal ConcurrencyBehavior.IWaiter DequeueWaiter()
        {
            // Finishing old work takes precedence over new work.
            if ((this.calloutMessageQueue != null) && (this.calloutMessageQueue.Count > 0))
            {
                return this.DequeueFrom(this.calloutMessageQueue);
            }
            else
            {
                return this.DequeueFrom(this.newMessageQueue);
            }
        }

        internal void EnqueueNewMessage(ConcurrencyBehavior.IWaiter waiter)
        {
            if (this.newMessageQueue == null)
                this.newMessageQueue = new Queue<ConcurrencyBehavior.IWaiter>();
            this.newMessageQueue.Enqueue(waiter);
        }

        internal void EnqueueCalloutMessage(ConcurrencyBehavior.IWaiter waiter)
        {
            if (this.calloutMessageQueue == null)
                this.calloutMessageQueue = new Queue<ConcurrencyBehavior.IWaiter>();
            this.calloutMessageQueue.Enqueue(waiter);
        }
    }
}
