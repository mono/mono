//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.

namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;

#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif

    internal abstract class BaseAsyncResult : IAsyncResult
    {
        internal readonly object Source;

        internal readonly string Method;

        private readonly AsyncCallback userCallback;

        private readonly object userState;

        private System.Threading.ManualResetEvent asyncWait;

        private Exception failure;

        private WebRequest abortable;

        private bool completedSynchronously = true;

        private bool userCompleted;

        private int completed;

        private int userNotified;

        private int done;

        private bool asyncWaitDisposed;

        private object asyncWaitDisposeLock;

        internal BaseAsyncResult(object source, string method, AsyncCallback callback, object state)
        {
            Debug.Assert(null != source, "null source");
            this.Source = source;
            this.Method = method;
            this.userCallback = callback;
            this.userState = state;
        }

        internal delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        #region IAsyncResult implmentation - AsyncState, AsyncWaitHandle, CompletedSynchronously, IsCompleted

        public object AsyncState
        {
            get { return this.userState; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get
            {
                if (null == this.asyncWait)
                {                    System.Threading.Interlocked.CompareExchange(ref this.asyncWait, new System.Threading.ManualResetEvent(this.IsCompleted), null);

                    if (this.IsCompleted)
                    {                        this.SetAsyncWaitHandle();
                    }
                }

                return this.asyncWait;
            }
        }

        public bool CompletedSynchronously
        {
            get { return this.completedSynchronously; }
            internal set { this.completedSynchronously = value; }
        }

        public bool IsCompleted
        {
            get { return this.userCompleted; }
        }

        internal bool IsCompletedInternally
        {
            get { return (0 != this.completed); }
        }

        internal bool IsAborted
        {
            get { return (2 == this.completed); }
        }

        #endregion

        internal WebRequest Abortable
        {
            get
            {
                return this.abortable;
            }

            set
            {
                this.abortable = value;
                if ((null != value) && this.IsAborted)
                {                    value.Abort();
                }
            }
        }

        internal Exception Failure
        {
            get { return this.failure; }
        }

        internal static T EndExecute<T>(object source, string method, IAsyncResult asyncResult) where T : BaseAsyncResult
        {
            Util.CheckArgumentNull(asyncResult, "asyncResult");

            T result = (asyncResult as T);
            if ((null == result) || (source != result.Source) || (result.Method != method))
            {
                throw Error.Argument(Strings.Context_DidNotOriginateAsync, "asyncResult");
            }

            Debug.Assert((result.CompletedSynchronously && result.IsCompleted) || !result.CompletedSynchronously, "CompletedSynchronously && !IsCompleted");

            if (!result.IsCompleted)
            {                result.AsyncWaitHandle.WaitOne();

                Debug.Assert(result.IsCompleted, "not completed after waiting");
            }

            if (System.Threading.Interlocked.Exchange(ref result.done, 1) != 0)
            {
                throw Error.Argument(Strings.Context_AsyncAlreadyDone, "asyncResult");
            }

            if (null != result.asyncWait)
            {
                System.Threading.Interlocked.CompareExchange(ref result.asyncWaitDisposeLock, new object(), null);
                lock (result.asyncWaitDisposeLock)
                {
                    result.asyncWaitDisposed = true;
                    Util.Dispose(result.asyncWait);
                }
            }

            if (result.IsAborted)
            {
                throw Error.InvalidOperation(Strings.Context_OperationCanceled);
            }

            if (null != result.Failure)
            {
                if (Util.IsKnownClientExcption(result.Failure))
                {
                    throw result.Failure;
                }

                throw Error.InvalidOperation(Strings.DataServiceException_GeneralError, result.Failure);
            }

            return result;
        }

        internal static IAsyncResult InvokeAsync(Func<AsyncCallback, object, IAsyncResult> asyncAction, AsyncCallback callback, object state)
        {
            IAsyncResult asyncResult = asyncAction(BaseAsyncResult.GetDataServiceAsyncCallback(callback), state);
            return PostInvokeAsync(asyncResult, callback);
        }

        internal static IAsyncResult InvokeAsync(Func<byte[], int, int, AsyncCallback, object, IAsyncResult> asyncAction, byte[] buffer, int offset, int length, AsyncCallback callback, object state)
        {
            IAsyncResult asyncResult = asyncAction(buffer, offset, length, BaseAsyncResult.GetDataServiceAsyncCallback(callback), state);
            return PostInvokeAsync(asyncResult, callback);
        }

        internal void HandleCompleted()
        {
            if (this.IsCompletedInternally && (System.Threading.Interlocked.Exchange(ref this.userNotified, 1) == 0))
            {
                this.abortable = null;                try
                {
                    if (!Util.DoNotHandleException(this.Failure))
                    {
                        this.CompletedRequest();
                    }
                }
                catch (Exception ex)
                {
                    if (this.HandleFailure(ex))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.userCompleted = true;

                    this.SetAsyncWaitHandle();

                    if ((null != this.userCallback) && !(this.Failure is System.Threading.ThreadAbortException) && !(this.Failure is System.StackOverflowException))
                    {                        this.userCallback(this);
                    }
                }
            }
        }

        internal bool HandleFailure(Exception e)
        {
            System.Threading.Interlocked.CompareExchange(ref this.failure, e, null);
            this.SetCompleted();
            return Util.DoNotHandleException(e);
        }

        internal void SetAborted()
        {
            System.Threading.Interlocked.Exchange(ref this.completed, 2);
        }

        internal void SetCompleted()
        {
            System.Threading.Interlocked.CompareExchange(ref this.completed, 1, 0);
        }

        protected abstract void CompletedRequest();

        private static IAsyncResult PostInvokeAsync(IAsyncResult asyncResult, AsyncCallback callback)
        {
            Debug.Assert(asyncResult != null, "asyncResult != null");
            if (asyncResult.CompletedSynchronously)
            {
                Debug.Assert(asyncResult.IsCompleted, "asyncResult.IsCompleted");
                callback(asyncResult);
            }

            return asyncResult;
        }

        private static AsyncCallback GetDataServiceAsyncCallback(AsyncCallback callback)
        {
            return (asyncResult) =>
            {
                Debug.Assert(asyncResult != null && asyncResult.IsCompleted, "asyncResult != null && asyncResult.IsCompleted");
                if (asyncResult.CompletedSynchronously)
                {
                    return;
                }

                callback(asyncResult);
            };
        }

        private void SetAsyncWaitHandle()
        {
            if (null != this.asyncWait)
            {
                System.Threading.Interlocked.CompareExchange(ref this.asyncWaitDisposeLock, new object(), null);
                lock (this.asyncWaitDisposeLock)
                {
                    if (!this.asyncWaitDisposed)
                    {
                        this.asyncWait.Set();
                    }
                }
            }
        }
    }
}
