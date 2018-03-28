//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;

    class AsyncOperationLifetimeManager
    {
        [Fx.Tag.SynchronizationObject()]
        object thisLock;

        bool isAborted;
        AsyncWaitHandle closeHandle;
        Dictionary<UniqueId, AsyncOperationContext> activeOperations;

        public AsyncOperationLifetimeManager()
        {
            this.thisLock = new object();
            this.activeOperations = new Dictionary<UniqueId, AsyncOperationContext>();
        }

        public bool IsAborted
        {
            get
            {
                return this.isAborted;
            }
        }

        public bool IsClosed
        {
            get
            {
                return this.closeHandle != null;
            }
        }

        public bool TryAdd(AsyncOperationContext context)
        {
            Fx.Assert(context != null, "The context must be non null.");

            lock (this.thisLock)
            {
                if (this.IsAborted || this.IsClosed)
                {
                    return false;
                }
                if (this.activeOperations.ContainsKey(context.OperationId))
                {
                    return false;
                }
                this.activeOperations.Add(context.OperationId, context);
            }

            return true;
        }

        public AsyncOperationContext[] Abort()
        {
            AsyncOperationContext[] retValue = null;
            bool setCloseHandle = false;

            lock (this.thisLock)
            {
                if (this.IsAborted)
                {
                    return new AsyncOperationContext[] { };
                }
                else
                {
                    this.isAborted = true;
                }

                retValue = new AsyncOperationContext[this.activeOperations.Count];
                this.activeOperations.Values.CopyTo(retValue, 0);
                this.activeOperations.Clear();
                setCloseHandle = this.closeHandle != null;
            }

            if (setCloseHandle)
            {
                this.closeHandle.Set();
            }

            return retValue;
        }

        public bool TryLookup(UniqueId operationId, out AsyncOperationContext context)
        {
            bool success;

            lock (this.thisLock)
            {
                success = this.activeOperations.TryGetValue(operationId, out context);
            }

            return success;
        }

        public bool TryLookup<T>(UniqueId operationId, out T context) where T : AsyncOperationContext
        {
            AsyncOperationContext asyncContext = null;
            if (TryLookup(operationId, out asyncContext))
            {
                context = asyncContext as T;
                if (context != null)
                {
                    return true;
                }
            }

            context = null;
            return false;
        }

        public T Remove<T>(UniqueId operationId) where T : AsyncOperationContext
        {
            AsyncOperationContext context = null;
            bool setCloseHandle = false;

            lock (this.thisLock)
            {
                if ((this.activeOperations.TryGetValue(operationId, out context)) &&
                    (context is T))
                {
                    this.activeOperations.Remove(operationId);
                    setCloseHandle = (this.closeHandle != null) && (this.activeOperations.Count == 0);
                }
                else
                {
                    context = null;
                }
            }

            if (setCloseHandle)
            {
                this.closeHandle.Set();
            }

            return context as T;
        }

        public bool TryRemoveUnique(object userState, out AsyncOperationContext context)
        {
            bool success = false;
            bool setCloseHandle = false;
            context = null;

            lock (this.thisLock)
            {
                foreach (AsyncOperationContext value in this.activeOperations.Values)
                {
                    if (object.Equals(value.UserState, userState))
                    {
                        if (success)
                        {
                            success = false;
                            break;
                        }
                        else
                        {
                            context = value;
                            success = true;
                        }
                    }
                }

                if (success)
                {
                    this.activeOperations.Remove(context.OperationId);
                    setCloseHandle = (this.closeHandle != null) && (this.activeOperations.Count == 0);
                }
            }

            if (setCloseHandle)
            {
                this.closeHandle.Set();
            }

            return success;
        }

        public void Close(TimeSpan timeout)
        {
            InitializeCloseHandle();
            if (!this.closeHandle.Wait(timeout))
            {
                throw FxTrace.Exception.AsError(new TimeoutException(SR2.TimeoutOnOperation(timeout)));
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            InitializeCloseHandle();
            return new CloseAsyncResult(this.closeHandle, timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        void InitializeCloseHandle()
        {
            bool setCloseHandle = false;
            lock (this.thisLock)
            {
                this.closeHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                setCloseHandle = (this.activeOperations.Count == 0);

                if (this.IsAborted)
                {
                    setCloseHandle = true;
                }
            }
            if (setCloseHandle)
            {
                this.closeHandle.Set();
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> onWaitCompleted = new Action<object, TimeoutException>(OnWaitCompleted);
            AsyncWaitHandle asyncWaitHandle;

            internal CloseAsyncResult(AsyncWaitHandle asyncWaitHandle, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.asyncWaitHandle = asyncWaitHandle;
                if (this.asyncWaitHandle.WaitAsync(onWaitCompleted, this, timeout))
                {
                    Complete(true);
                }
            }

            static void OnWaitCompleted(object state, TimeoutException asyncException)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)state;
                thisPtr.Complete(false, asyncException);
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }
}
