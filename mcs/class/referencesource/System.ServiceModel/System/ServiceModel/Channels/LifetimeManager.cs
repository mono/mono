//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;

    enum LifetimeState
    {
        Opened,
        Closing,
        Closed
    }

    class LifetimeManager
    {
#if DEBUG_EXPENSIVE
        StackTrace closeStack;
#endif
        bool aborted;
        int busyCount;
        ICommunicationWaiter busyWaiter;
        int busyWaiterCount;
        object mutex;
        LifetimeState state;

        public LifetimeManager(object mutex)
        {
            this.mutex = mutex;
            this.state = LifetimeState.Opened;
        }

        public int BusyCount
        {
            get { return this.busyCount; }
        }

        protected LifetimeState State
        {
            get { return this.state; }
        }

        protected object ThisLock
        {
            get { return this.mutex; }
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                if (this.State == LifetimeState.Closed || this.aborted)
                    return;
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
                this.aborted = true;
                this.state = LifetimeState.Closing;
            }

            this.OnAbort();
            this.state = LifetimeState.Closed;
        }

        void ThrowIfNotOpened()
        {
            if (!this.aborted && this.state != LifetimeState.Opened)
            {
#if DEBUG_EXPENSIVE
                String originalStack = closeStack.ToString().Replace("\r\n", "\r\n    ");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString() + ", Object already closed:\r\n    " + originalStack));
#else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
#endif
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
                this.state = LifetimeState.Closing;
            }

            return this.OnBeginClose(timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
                this.state = LifetimeState.Closing;
            }

            this.OnClose(timeout);
            this.state = LifetimeState.Closed;
        }

        CommunicationWaitResult CloseCore(TimeSpan timeout, bool aborting)
        {
            ICommunicationWaiter busyWaiter = null;
            CommunicationWaitResult result = CommunicationWaitResult.Succeeded;

            lock (this.ThisLock)
            {
                if (this.busyCount > 0)
                {
                    if (this.busyWaiter != null)
                    {
                        if (!aborting && this.aborted)
                            return CommunicationWaitResult.Aborted;
                        busyWaiter = this.busyWaiter;
                    }
                    else
                    {
                        busyWaiter = new SyncCommunicationWaiter(this.ThisLock);
                        this.busyWaiter = busyWaiter;
                    }
                    Interlocked.Increment(ref busyWaiterCount);
                }
            }

            if (busyWaiter != null)
            {
                result = busyWaiter.Wait(timeout, aborting);
                if (Interlocked.Decrement(ref busyWaiterCount) == 0)
                {
                    busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }

            return result;
        }

        protected void DecrementBusyCount()
        {
            ICommunicationWaiter busyWaiter = null;
            bool empty = false;

            lock (this.ThisLock)
            {
                if (this.busyCount <= 0)
                {
                    throw Fx.AssertAndThrow("LifetimeManager.DecrementBusyCount: (this.busyCount > 0)");
                }
                if (--this.busyCount == 0)
                {
                    if (this.busyWaiter != null)
                    {
                        busyWaiter = this.busyWaiter;
                        Interlocked.Increment(ref this.busyWaiterCount);
                    }
                    empty = true;
                }
            }

            if (busyWaiter != null)
            {
                busyWaiter.Signal();
                if (Interlocked.Decrement(ref this.busyWaiterCount) == 0)
                {
                    busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }

            if (empty && this.State == LifetimeState.Opened)
                OnEmpty();
        }

        public void EndClose(IAsyncResult result)
        {
            this.OnEndClose(result);
            this.state = LifetimeState.Closed;
        }

        protected virtual void IncrementBusyCount()
        {
            lock (this.ThisLock)
            {
                Fx.Assert(this.State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCount: (this.State == LifetimeState.Opened)");
                this.busyCount++;
            }
        }

        protected virtual void IncrementBusyCountWithoutLock()
        {
            Fx.Assert(this.State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCountWithoutLock: (this.State == LifetimeState.Opened)");
            this.busyCount++;
        }

        protected virtual void OnAbort()
        {
            // We have decided not to make this configurable
            CloseCore(TimeSpan.FromSeconds(1), true);
        }

        protected virtual IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CloseCommunicationAsyncResult closeResult = null;

            lock (this.ThisLock)
            {
                if (this.busyCount > 0)
                {
                    if (this.busyWaiter != null)
                    {
                        Fx.Assert(this.aborted, "LifetimeManager.OnBeginClose: (this.aborted == true)");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
                    }
                    else
                    {
                        closeResult = new CloseCommunicationAsyncResult(timeout, callback, state, this.ThisLock);
                        Fx.Assert(this.busyWaiter == null, "LifetimeManager.OnBeginClose: (this.busyWaiter == null)");
                        this.busyWaiter = closeResult;
                        Interlocked.Increment(ref this.busyWaiterCount);
                    }
                }
            }

            if (closeResult != null)
            {
                return closeResult;
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected virtual void OnClose(TimeSpan timeout)
        {
            switch (CloseCore(timeout, false))
            {
                case CommunicationWaitResult.Expired:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.SFxCloseTimedOut1, timeout)));
                case CommunicationWaitResult.Aborted:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
            }
        }

        protected virtual void OnEmpty()
        {
        }

        protected virtual void OnEndClose(IAsyncResult result)
        {
            if (result is CloseCommunicationAsyncResult)
            {
                CloseCommunicationAsyncResult.End(result);
                if (Interlocked.Decrement(ref this.busyWaiterCount) == 0)
                {
                    this.busyWaiter.Dispose();
                    this.busyWaiter = null;
                }
            }
            else
                CompletedAsyncResult.End(result);
        }
    }

    enum CommunicationWaitResult
    {
        Waiting,
        Succeeded,
        Expired,
        Aborted
    }

    interface ICommunicationWaiter : IDisposable
    {
        void Signal();
        CommunicationWaitResult Wait(TimeSpan timeout, bool aborting);
    }

    class CloseCommunicationAsyncResult : AsyncResult, ICommunicationWaiter
    {
        object mutex;
        CommunicationWaitResult result;
        IOThreadTimer timer;
        TimeoutHelper timeoutHelper;
        TimeSpan timeout;

        public CloseCommunicationAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, object mutex)
            : base(callback, state)
        {
            this.timeout = timeout;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.mutex = mutex;

            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.SFxCloseTimedOut1, timeout)));

            this.timer = new IOThreadTimer(new Action<object>(TimeoutCallback), this, true);
            this.timer.Set(timeout);
        }

        object ThisLock
        {
            get { return mutex; }
        }

        public void Dispose()
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<CloseCommunicationAsyncResult>(result);
        }

        public void Signal()
        {
            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                    return;
                this.result = CommunicationWaitResult.Succeeded;
            }
            this.timer.Cancel();
            this.Complete(false);
        }

        void Timeout()
        {
            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                    return;
                this.result = CommunicationWaitResult.Expired;
            }
            this.Complete(false, new TimeoutException(SR.GetString(SR.SFxCloseTimedOut1, this.timeout)));
        }

        static void TimeoutCallback(object state)
        {
            CloseCommunicationAsyncResult closeResult = (CloseCommunicationAsyncResult)state;
            closeResult.Timeout();
        }

        public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
        {
            if (timeout < TimeSpan.Zero)
            {
                return CommunicationWaitResult.Expired;
            }

            // Synchronous Wait on AsyncResult should only be called in Abort code-path
            Fx.Assert(aborting, "CloseCommunicationAsyncResult.Wait: (aborting == true)");

            lock (this.ThisLock)
            {
                if (this.result != CommunicationWaitResult.Waiting)
                {
                    return this.result;
                }
                this.result = CommunicationWaitResult.Aborted;
            }
            this.timer.Cancel();

            TimeoutHelper.WaitOne(this.AsyncWaitHandle, timeout);

            this.Complete(false, new ObjectDisposedException(this.GetType().ToString()));
            return this.result;
        }
    }

    internal class SyncCommunicationWaiter : ICommunicationWaiter
    {
        bool closed;
        object mutex;
        CommunicationWaitResult result;
        ManualResetEvent waitHandle;

        public SyncCommunicationWaiter(object mutex)
        {
            this.mutex = mutex;
            this.waitHandle = new ManualResetEvent(false);
        }

        object ThisLock
        {
            get { return this.mutex; }
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                    return;
                this.closed = true;
                this.waitHandle.Close();
            }
        }

        public void Signal()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                    return;
                this.waitHandle.Set();
            }
        }

        public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
        {
            if (this.closed)
            {
                return CommunicationWaitResult.Aborted;
            }
            if (timeout < TimeSpan.Zero)
            {
                return CommunicationWaitResult.Expired;
            }

            if (aborting)
            {
                this.result = CommunicationWaitResult.Aborted;
            }

            bool expired = !TimeoutHelper.WaitOne(this.waitHandle, timeout);

            lock (this.ThisLock)
            {
                if (this.result == CommunicationWaitResult.Waiting)
                {
                    this.result = (expired ? CommunicationWaitResult.Expired : CommunicationWaitResult.Succeeded);
                }
            }

            lock (this.ThisLock)
            {
                if (!this.closed)
                    this.waitHandle.Set();  // unblock other waiters if there are any
            }

            return this.result;
        }
    }
}
