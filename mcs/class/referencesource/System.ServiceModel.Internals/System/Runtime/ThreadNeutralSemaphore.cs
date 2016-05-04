//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.PrivatePrimitive,
        SupportsAsync = true, ReleaseMethod = "Exit")]
    class ThreadNeutralSemaphore
    {
#if DEBUG
        StackTrace exitStack;
#endif

        static Action<object, TimeoutException> enteredAsyncCallback;

        bool aborted;
        Func<Exception> abortedExceptionGenerator;
        int count;
        int maxCount;

        [Fx.Tag.SynchronizationObject(Blocking = false)]
        object ThisLock = new object();

        [Fx.Tag.SynchronizationObject]
        Queue<AsyncWaitHandle> waiters;

        public ThreadNeutralSemaphore(int maxCount)
            : this(maxCount, null)
        {
        }

        public ThreadNeutralSemaphore(int maxCount, Func<Exception> abortedExceptionGenerator)
        {
            Fx.Assert(maxCount > 0, "maxCount must be positive");
            this.maxCount = maxCount;
            this.abortedExceptionGenerator = abortedExceptionGenerator;
        }

        static Action<object, TimeoutException> EnteredAsyncCallback
        {
            get
            {
                if (enteredAsyncCallback == null)
                {
                    enteredAsyncCallback = new Action<object, TimeoutException>(OnEnteredAsync);
                }

                return enteredAsyncCallback;
            }
        }

        Queue<AsyncWaitHandle> Waiters
        {
            get
            {
                if (this.waiters == null)
                {
                    this.waiters = new Queue<AsyncWaitHandle>();
                }

                return this.waiters;
            }
        }

        public bool EnterAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
        {
            Fx.Assert(callback != null, "must have a non-null call back for async purposes");

            AsyncWaitHandle waiter = null;

            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    throw Fx.Exception.AsError(CreateObjectAbortedException());
                }

                if (this.count < this.maxCount)
                {
                    this.count++;
                    return true;
                }

                waiter = new AsyncWaitHandle();
                this.Waiters.Enqueue(waiter);
            }

            return waiter.WaitAsync(EnteredAsyncCallback, new EnterAsyncData(this, waiter, callback, state), timeout);
        }

        static void OnEnteredAsync(object state, TimeoutException exception)
        {
            EnterAsyncData data = (EnterAsyncData)state;
            ThreadNeutralSemaphore thisPtr = data.Semaphore;
            Exception exceptionToPropagate = exception;

            if (exception != null)
            {
                if (!thisPtr.RemoveWaiter(data.Waiter))
                {
                    // The timeout ----d with Exit and exit won.
                    // We've successfully entered.
                    exceptionToPropagate = null;
                }
            }

            Fx.Assert(!thisPtr.waiters.Contains(data.Waiter), "The waiter should have been removed already.");

            if (thisPtr.aborted)
            {
                exceptionToPropagate = thisPtr.CreateObjectAbortedException();
            }

            data.Callback(data.State, exceptionToPropagate);
        }

        public bool TryEnter()
        {
            lock (this.ThisLock)
            {
                if (this.count < this.maxCount)
                {
                    this.count++;
                    return true;
                }

                return false;
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public void Enter(TimeSpan timeout)
        {
            if (!TryEnter(timeout))
            {
                throw Fx.Exception.AsError(CreateEnterTimedOutException(timeout));
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public bool TryEnter(TimeSpan timeout)
        {
            AsyncWaitHandle waiter = EnterCore();

            if (waiter != null)
            {
                bool timedOut = !waiter.Wait(timeout);

                if (this.aborted)
                {
                    throw Fx.Exception.AsError(CreateObjectAbortedException());
                }

                if (timedOut && !RemoveWaiter(waiter))
                {
                    // The timeout ----d with Exit and exit won.
                    // We've successfully entered.

                    timedOut = false;
                }


                return !timedOut;
            }

            return true;
        }

        internal static TimeoutException CreateEnterTimedOutException(TimeSpan timeout)
        {
            return new TimeoutException(InternalSR.LockTimeoutExceptionMessage(timeout));
        }

        Exception CreateObjectAbortedException()
        {
            if (this.abortedExceptionGenerator != null)
            {
                return this.abortedExceptionGenerator();
            }
            else
            {
                return new OperationCanceledException(InternalSR.ThreadNeutralSemaphoreAborted);
            }
        }

        // remove a waiter from our queue. Returns true if successful. Used to implement timeouts.
        bool RemoveWaiter(AsyncWaitHandle waiter)
        {
            bool removed = false;

            lock (this.ThisLock)
            {
                for (int i = this.Waiters.Count; i > 0; i--)
                {
                    AsyncWaitHandle temp = this.Waiters.Dequeue();
                    if (object.ReferenceEquals(temp, waiter))
                    {
                        removed = true;
                    }
                    else
                    {
                        this.Waiters.Enqueue(temp);
                    }
                }
            }

            return removed;
        }

        AsyncWaitHandle EnterCore()
        {
            AsyncWaitHandle waiter;

            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    throw Fx.Exception.AsError(CreateObjectAbortedException());
                }

                if (this.count < this.maxCount)
                {
                    this.count++;
                    return null;
                }

                waiter = new AsyncWaitHandle();
                this.Waiters.Enqueue(waiter);
            }

            return waiter;
        }

        public int Exit()
        {
            AsyncWaitHandle waiter;

            int remainingCount = -1;
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    return remainingCount;
                }

                if (this.count == 0)
                {
                    string message = InternalSR.InvalidSemaphoreExit;

#if DEBUG
                    if (!Fx.FastDebug && exitStack != null)
                    {
                        string originalStack = exitStack.ToString().Replace("\r\n", "\r\n    ");
                        message = string.Format(CultureInfo.InvariantCulture,
                            "Object synchronization method was called from an unsynchronized block of code. Previous Exit(): {0}", originalStack);
                    }
#endif

                    throw Fx.Exception.AsError(new SynchronizationLockException(message));
                }

                if (this.waiters == null || this.waiters.Count == 0)
                {
                    this.count--;

#if DEBUG
                    if (!Fx.FastDebug && this.count == 0)
                    {
                        exitStack = new StackTrace();
                    }
#endif

                    return this.count;
                }

                waiter = this.waiters.Dequeue();
                remainingCount = this.count;
            }

            waiter.Set();
            return remainingCount;
        }

        // Abort the ThreadNeutralSemaphore object.
        public void Abort()
        {
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    return;
                }

                this.aborted = true;

                if (this.waiters != null)
                {
                    while (this.waiters.Count > 0)
                    {
                        AsyncWaitHandle waiter = this.waiters.Dequeue();
                        waiter.Set();
                    }
                }
            }
        }

        class EnterAsyncData
        {
            public EnterAsyncData(ThreadNeutralSemaphore semaphore, AsyncWaitHandle waiter, FastAsyncCallback callback, object state)
            {
                this.Waiter = waiter;
                this.Semaphore = semaphore;
                this.Callback = callback;
                this.State = state;
            }

            public ThreadNeutralSemaphore Semaphore
            {
                get;
                set;
            }

            public AsyncWaitHandle Waiter
            {
                get;
                set;
            }

            public FastAsyncCallback Callback
            {
                get;
                set;
            }

            public object State
            {
                get;
                set;
            }
        }
    }
}
