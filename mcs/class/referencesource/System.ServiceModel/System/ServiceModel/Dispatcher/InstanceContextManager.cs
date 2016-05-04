//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    interface IInstanceContextManager
    {
        void Abort();
        void Add(InstanceContext instanceContext);
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state);
        void Close(TimeSpan timeout);
        void CloseInput(TimeSpan timeout);
        void EndClose(IAsyncResult result);
        void EndCloseInput(IAsyncResult result);
        bool Remove(InstanceContext instanceContext);
        InstanceContext[] ToArray();
    }

    class InstanceContextManager : LifetimeManager, IInstanceContextManager
    {
        int firstFreeIndex;
        Item[] items;

        public InstanceContextManager(object mutex)
            : base(mutex)
        {
        }

        public void Add(InstanceContext instanceContext)
        {
            bool added = false;

            lock (this.ThisLock)
            {
                if (this.State == LifetimeState.Opened)
                {
                    if (instanceContext.InstanceContextManagerIndex != 0)
                        return;
                    if (this.firstFreeIndex == 0)
                        this.GrowItems();
                    this.AddItem(instanceContext);
                    base.IncrementBusyCountWithoutLock();
                    added = true;
                }
            }

            if (!added)
            {
                instanceContext.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
            }
        }

        void AddItem(InstanceContext instanceContext)
        {
            int index = this.firstFreeIndex;
            this.firstFreeIndex = this.items[index].nextFreeIndex;
            this.items[index].instanceContext = instanceContext;
            instanceContext.InstanceContextManagerIndex = index;
        }

        public IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseInputAsyncResult(timeout, callback, state, this.ToArray());
        }

        void CloseInitiate(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            InstanceContext[] instances = this.ToArray();
            for (int index = 0; index < instances.Length; index++)
            {
                InstanceContext instance = instances[index];
                try
                {
                    if (instance.State == CommunicationState.Opened)
                    {
                        IAsyncResult result = instance.BeginClose(timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(CloseInstanceContextCallback)), instance);
                        if (!result.CompletedSynchronously)
                            continue;
                        instance.EndClose(result);
                    }
                    else
                    {
                        instance.Abort();
                    }
                }
                catch (ObjectDisposedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (InvalidOperationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
        }

        public void CloseInput(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            InstanceContext[] instances = this.ToArray();
            for (int index = 0; index < instances.Length; index++)
                instances[index].CloseInput(timeoutHelper.RemainingTime());
        }

        static void CloseInstanceContextCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            InstanceContext instanceContext = (InstanceContext)result.AsyncState;
            try
            {
                instanceContext.EndClose(result);
            }
            catch (ObjectDisposedException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
             }
             catch (InvalidOperationException e)
             {
                 DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
             }
             catch (CommunicationException e)
             {
                 DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
             }
             catch (TimeoutException e)
             {
                 if (TD.CloseTimeoutIsEnabled())
                 {
                     TD.CloseTimeout(e.Message);
                 }
                 DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
             }
        }

        public void EndCloseInput(IAsyncResult result)
        {
            CloseInputAsyncResult.End(result);
        }

        void GrowItems()
        {
            Item[] existingItems = this.items;
            if (existingItems != null)
            {
                this.InitItems(existingItems.Length * 2);
                for (int i = 1; i < existingItems.Length; i++)
                    this.AddItem(existingItems[i].instanceContext);
            }
            else
            {
                this.InitItems(4);
            }
        }

        void InitItems(int count)
        {
            this.items = new Item[count];
            for (int i = count - 2; i > 0; i--)
            {
                this.items[i].nextFreeIndex = i + 1;
            }
            this.firstFreeIndex = 1;
        }

        protected override void OnAbort()
        {
            InstanceContext[] instances = this.ToArray();
            for (int index = 0; index < instances.Length; index++)
            {
                instances[index].Abort();
            }

            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            CloseInitiate(timeoutHelper.RemainingTime());
            return base.OnBeginClose(timeoutHelper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            CloseInitiate(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        public bool Remove(InstanceContext instanceContext)
        {
            if (instanceContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("instanceContext"));

            lock (this.ThisLock)
            {
                int index = instanceContext.InstanceContextManagerIndex;
                if (index == 0)
                    return false;
                instanceContext.InstanceContextManagerIndex = 0;
                this.items[index].nextFreeIndex = this.firstFreeIndex;
                this.items[index].instanceContext = null;
                this.firstFreeIndex = index;
            }

            base.DecrementBusyCount();
            return true;
        }

        public InstanceContext[] ToArray()
        {
            if (this.items == null)
            {
                return EmptyArray<InstanceContext>.Instance;
            }

            lock (this.ThisLock)
            {
                int count = 0;
                for (int i = 1; i < this.items.Length; i++)
                    if (this.items[i].instanceContext != null)
                        count++;

                if (count == 0)
                    return EmptyArray<InstanceContext>.Instance;

                InstanceContext[] array = new InstanceContext[count];
                count = 0;
                for (int i = 1; i < this.items.Length; i++)
                {
                    InstanceContext instanceContext = this.items[i].instanceContext;
                    if (instanceContext != null)
                    {
                        array[count++] = instanceContext;
                    }
                }

                return array;
            }
        }

        struct Item
        {
            public int nextFreeIndex;
            public InstanceContext instanceContext;
        }
    }

    class CloseInputAsyncResult : AsyncResult
    {
        bool completedSynchronously;
        Exception exception;
        static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(Callback));
        int count;
        TimeoutHelper timeoutHelper;

        public CloseInputAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, InstanceContext[] instances)
            : base(otherCallback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            completedSynchronously = true;

            count = instances.Length;
            if (count == 0)
            {
                Complete(true);
                return;
            }

            for (int index = 0; index < instances.Length; index++)
            {
                CallbackState callbackState = new CallbackState(this, instances[index]);
                IAsyncResult result;
                try
                {
                    result = instances[index].BeginCloseInput(this.timeoutHelper.RemainingTime(), nestedCallback, callbackState);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    Decrement(true, e);
                    continue;
                }
                if (result.CompletedSynchronously)
                {
                    instances[index].EndCloseInput(result);
                    Decrement(true);
                }
            }
        }

        static void Callback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
                return;
            CallbackState callbackState = (CallbackState)result.AsyncState;
            try
            {
                callbackState.Instance.EndCloseInput(result);
                callbackState.Result.Decrement(false);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                callbackState.Result.Decrement(false, e);
            }
        }

        void Decrement(bool completedSynchronously)
        {
            if (completedSynchronously == false)
                this.completedSynchronously = false;
            if (Interlocked.Decrement(ref count) == 0)
            {
                if (this.exception != null)
                    Complete(this.completedSynchronously, this.exception);
                else
                    Complete(this.completedSynchronously);
            }
        }

        void Decrement(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Decrement(completedSynchronously);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<CloseInputAsyncResult>(result);
        }

        class CallbackState
        {
            InstanceContext instance;
            CloseInputAsyncResult result;

            public CallbackState(CloseInputAsyncResult result, InstanceContext instance)
            {
                this.result = result;
                this.instance = instance;
            }

            public InstanceContext Instance
            {
                get { return instance; }
            }

            public CloseInputAsyncResult Result
            {
                get { return result; }
            }
        }
    }
}
