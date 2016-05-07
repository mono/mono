//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.MonitorWait, SupportsAsync = true, ReleaseMethod = "Set")]
    class AsyncWaitHandle
    {
        static Action<object> timerCompleteCallback;

        List<AsyncWaiter> asyncWaiters;
        bool isSignaled;
        EventResetMode resetMode;

        [Fx.Tag.SynchronizationObject(Kind = Fx.Tag.SynchronizationKind.MonitorWait)]
        object syncObject;

        int syncWaiterCount;

        public AsyncWaitHandle()
            : this(EventResetMode.AutoReset)
        {
        }

        public AsyncWaitHandle(EventResetMode resetMode)
        {
            this.resetMode = resetMode;
            this.syncObject = new object();
        }

        public bool WaitAsync(Action<object, TimeoutException> callback, object state, TimeSpan timeout)
        {
            if (!this.isSignaled || (this.isSignaled && this.resetMode == EventResetMode.AutoReset))
            {
                lock (syncObject)
                {
                    if (this.isSignaled && this.resetMode == EventResetMode.AutoReset)
                    {
                        this.isSignaled = false;
                    }
                    else if (!this.isSignaled)
                    {
                        AsyncWaiter waiter = new AsyncWaiter(this, callback, state);

                        if (this.asyncWaiters == null)
                        {
                            this.asyncWaiters = new List<AsyncWaiter>();
                        }

                        this.asyncWaiters.Add(waiter);

                        if (timeout != TimeSpan.MaxValue)
                        {
                            if (timerCompleteCallback == null)
                            {
                                timerCompleteCallback = new Action<object>(OnTimerComplete);
                            }
                            waiter.SetTimer(timerCompleteCallback, waiter, timeout);
                        }
                        return false;
                    }
                }
            }

            return true;
        }

        static void OnTimerComplete(object state)
        {
            AsyncWaiter waiter = (AsyncWaiter)state;
            AsyncWaitHandle thisPtr = waiter.Parent;
            bool callWaiter = false;

            lock (thisPtr.syncObject)
            {
                // If still in the waiting list (that means it hasn't been signaled)
                if (thisPtr.asyncWaiters != null && thisPtr.asyncWaiters.Remove(waiter))
                {
                    waiter.TimedOut = true;
                    callWaiter = true;
                }
            }

            waiter.CancelTimer();

            if (callWaiter)
            {
                waiter.Call();
            }
        }

        [Fx.Tag.Blocking]
        public bool Wait(TimeSpan timeout)
        {
            if (!this.isSignaled || (this.isSignaled && this.resetMode == EventResetMode.AutoReset))
            {
                lock (syncObject)
                {
                    if (this.isSignaled && this.resetMode == EventResetMode.AutoReset)
                    {
                        this.isSignaled = false;
                    }
                    else if (!this.isSignaled)
                    {
                        bool decrementRequired = false;

                        try
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.syncWaiterCount++;
                                decrementRequired = true;
                            }

                            if (timeout == TimeSpan.MaxValue)
                            {
                                if (!Monitor.Wait(syncObject, Timeout.Infinite))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (!Monitor.Wait(syncObject, timeout))
                                {
                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            if (decrementRequired)
                            {
                                this.syncWaiterCount--;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void Set()
        {
            List<AsyncWaiter> toCallList = null;
            AsyncWaiter toCall = null;

            if (!this.isSignaled)
            {
                lock (syncObject)
                {
                    if (!this.isSignaled)
                    {
                        if (this.resetMode == EventResetMode.ManualReset)
                        {
                            this.isSignaled = true;
                            Monitor.PulseAll(syncObject);
                            toCallList = this.asyncWaiters;
                            this.asyncWaiters = null;
                        }
                        else
                        {
                            if (this.syncWaiterCount > 0)
                            {
                                Monitor.Pulse(syncObject);
                            }
                            else if (this.asyncWaiters != null && this.asyncWaiters.Count > 0)
                            {
                                toCall = this.asyncWaiters[0];
                                this.asyncWaiters.RemoveAt(0);
                            }
                            else
                            {
                                this.isSignaled = true;
                            }
                        }
                    }
                }
            }

            if (toCallList != null)
            {
                foreach (AsyncWaiter waiter in toCallList)
                {
                    waiter.CancelTimer();
                    waiter.Call();
                }
            }

            if (toCall != null)
            {
                toCall.CancelTimer();
                toCall.Call();
            }
        }

        public void Reset()
        {
            // Doesn't matter if this changes during processing of another method
            this.isSignaled = false;
        }

        class AsyncWaiter : ActionItem
        {
            [Fx.Tag.SecurityNote(Critical = "Store the delegate to be invoked")]
            [SecurityCritical]
            Action<object, TimeoutException> callback;
            [Fx.Tag.SecurityNote(Critical = "Stores the state object to be passed to the callback")]
            [SecurityCritical]
            object state;
            IOThreadTimer timer;
            TimeSpan originalTimeout;

            [Fx.Tag.SecurityNote(Critical = "Access critical members", Safe = "Doesn't leak information")]
            [SecuritySafeCritical]
            public AsyncWaiter(AsyncWaitHandle parent, Action<object, TimeoutException> callback, object state)
            {
                this.Parent = parent;
                this.callback = callback;
                this.state = state;
            }

            public AsyncWaitHandle Parent
            {
                get;
                private set;
            }

            public bool TimedOut
            {
                get;
                set;
            }

            [Fx.Tag.SecurityNote(Critical = "Calls into critical method Schedule", Safe = "Invokes the given delegate under the current context")]
            [SecuritySafeCritical]
            public void Call()
            {
                Schedule();
            }

            [Fx.Tag.SecurityNote(Critical = "Overriding an inherited critical method, access critical members")]
            [SecurityCritical]
            protected override void Invoke()
            {
                this.callback(this.state,
                    this.TimedOut ? new TimeoutException(InternalSR.TimeoutOnOperation(this.originalTimeout)) : null);
            }

            public void SetTimer(Action<object> callback, object state, TimeSpan timeout)
            {
                if (this.timer != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.MustCancelOldTimer));
                }

                this.originalTimeout = timeout;
                this.timer = new IOThreadTimer(callback, state, false);

                this.timer.Set(timeout);
            }

            public void CancelTimer()
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                    this.timer = null;
                }
            }
        }
    }

}
