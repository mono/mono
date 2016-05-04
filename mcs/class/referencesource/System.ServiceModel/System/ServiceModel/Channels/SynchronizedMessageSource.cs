//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    class SynchronizedMessageSource
    {
        IMessageSource source;
        ThreadNeutralSemaphore sourceLock;

        public SynchronizedMessageSource(IMessageSource source)
        {
            this.source = source;
            this.sourceLock = new ThreadNeutralSemaphore(1);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForMessageAsyncResult(this, timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return WaitForMessageAsyncResult.End(result);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.sourceLock.TryEnter(timeoutHelper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TimeoutException(SR.GetString(SR.WaitForMessageTimedOut, timeout),
                    ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }

            try
            {
                return source.WaitForMessage(timeoutHelper.RemainingTime());
            }
            finally
            {
                this.sourceLock.Exit();
            }
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReceiveAsyncResult(this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return ReceiveAsyncResult.End(result);
        }

        public Message Receive(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.sourceLock.TryEnter(timeoutHelper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TimeoutException(SR.GetString(SR.ReceiveTimedOut2, timeout),
                    ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }

            try
            {
                return source.Receive(timeoutHelper.RemainingTime());
            }
            finally
            {
                this.sourceLock.Exit();
            }
        }

        abstract class SynchronizedAsyncResult<T> : AsyncResult
        {
            T returnValue;
            bool exitLock;
            SynchronizedMessageSource syncSource;
            static FastAsyncCallback onEnterComplete = new FastAsyncCallback(OnEnterComplete);
            TimeoutHelper timeoutHelper;

            public SynchronizedAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.syncSource = syncSource;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (!syncSource.sourceLock.EnterAsync(this.timeoutHelper.RemainingTime(), onEnterComplete, this))
                {
                    return;
                }

                exitLock = true;
                bool success = false;
                bool completeSelf;
                try
                {
                    completeSelf = PerformOperation(timeoutHelper.RemainingTime());
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        ExitLock();
                    }
                }
                if (completeSelf)
                {
                    CompleteWithUnlock(true);
                }
            }

            protected IMessageSource Source
            {
                get { return syncSource.source; }
            }

            protected void SetReturnValue(T returnValue)
            {
                this.returnValue = returnValue;
            }

            protected abstract bool PerformOperation(TimeSpan timeout);

            void ExitLock()
            {
                if (exitLock)
                {
                    syncSource.sourceLock.Exit();
                    exitLock = false;
                }
            }

            protected void CompleteWithUnlock(bool synchronous)
            {
                CompleteWithUnlock(synchronous, null);
            }

            protected void CompleteWithUnlock(bool synchronous, Exception exception)
            {
                ExitLock();
                base.Complete(synchronous, exception);
            }

            public static T End(IAsyncResult result)
            {
                SynchronizedAsyncResult<T> thisPtr = AsyncResult.End<SynchronizedAsyncResult<T>>(result);
                return thisPtr.returnValue;
            }

            static void OnEnterComplete(object state, Exception asyncException)
            {
                SynchronizedAsyncResult<T> thisPtr = (SynchronizedAsyncResult<T>)state;

                Exception completionException = asyncException;
                bool completeSelf;

                if (completionException != null)
                {
                    completeSelf = true;
                }
                else
                {
                    try
                    {
                        thisPtr.exitLock = true;
                        completeSelf = thisPtr.PerformOperation(thisPtr.timeoutHelper.RemainingTime());
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    thisPtr.CompleteWithUnlock(false, completionException);
                }
            }
        }

        class ReceiveAsyncResult : SynchronizedAsyncResult<Message>
        {
            static WaitCallback onReceiveComplete = new WaitCallback(OnReceiveComplete);

            public ReceiveAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(syncSource, timeout, callback, state)
            {
            }

            protected override bool PerformOperation(TimeSpan timeout)
            {
                if (Source.BeginReceive(timeout, onReceiveComplete, this) == AsyncReceiveResult.Completed)
                {
                    SetReturnValue(Source.EndReceive());
                    return true;
                }

                return false;
            }

            static void OnReceiveComplete(object state)
            {
                ReceiveAsyncResult thisPtr = ((ReceiveAsyncResult)state);
                Exception completionException = null;
                try
                {
                    thisPtr.SetReturnValue(thisPtr.Source.EndReceive());
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                thisPtr.CompleteWithUnlock(false, completionException);
            }
        }

        class WaitForMessageAsyncResult : SynchronizedAsyncResult<bool>
        {
            static WaitCallback onWaitForMessageComplete = new WaitCallback(OnWaitForMessageComplete);

            public WaitForMessageAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(syncSource, timeout, callback, state)
            {
            }

            protected override bool PerformOperation(TimeSpan timeout)
            {
                if (Source.BeginWaitForMessage(timeout, onWaitForMessageComplete, this) == AsyncReceiveResult.Completed)
                {
                    SetReturnValue(Source.EndWaitForMessage());
                    return true;
                }

                return false;
            }

            static void OnWaitForMessageComplete(object state)
            {
                WaitForMessageAsyncResult thisPtr = (WaitForMessageAsyncResult)state;
                Exception completionException = null;

                try
                {
                    thisPtr.SetReturnValue(thisPtr.Source.EndWaitForMessage());
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                thisPtr.CompleteWithUnlock(false, completionException);
            }
        }
    }
}
