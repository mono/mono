//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Runtime;

    /// <summary>
    /// Base class for common AsyncResult programming scenarios.
    /// </summary>
    public abstract class AsyncResult : IAsyncResult, IDisposable
    {
        /// <summary>
        /// End should be called when the End function for the asynchronous operation is complete.  It
        /// ensures the asynchronous operation is complete, and does some common validation.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> representing the status of an asynchronous operation.</param>
        public static void End(IAsyncResult result)
        {
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            AsyncResult asyncResult = result as AsyncResult;

            if (asyncResult == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID4001), "result"));

            if (asyncResult.endCalled)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4002)));

            asyncResult.endCalled = true;

            if (!asyncResult.completed)
                asyncResult.AsyncWaitHandle.WaitOne();

            if (asyncResult.resetEvent != null)
                ((IDisposable)asyncResult.resetEvent).Dispose();

            if (asyncResult.exception != null)
                throw asyncResult.exception;
        }

        AsyncCallback callback;
        bool completed;
        bool completedSync;
        bool disposed;
        bool endCalled;
        Exception exception;
        ManualResetEvent resetEvent;
        object state;

        object thisLock;

        /// <summary>
        /// Constructor for async results that do not need a callback or state.
        /// </summary>
        protected AsyncResult()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructor for async results that do not need a callback.
        /// </summary>
        /// <param name="state">A user-defined object that qualifies or contains information about an asynchronous operation.</param>
        protected AsyncResult(object state)
            : this(null, state)
        {
        }

        /// <summary>
        /// Constructor for async results that need a callback and a state.
        /// </summary>
        /// <param name="callback">The method to be called when the async operation completes.</param>
        /// <param name="state">A user-defined object that qualifies or contains information about an asynchronous operation.</param>
        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.thisLock = new object();
            this.callback = callback;
            this.state = state;
        }

        /// <summary>
        /// Finalizer for AsyncResult.
        /// </summary>
        ~AsyncResult()
        {
            Dispose(false);
        }

        /// <summary>
        /// Call this version of complete when your asynchronous operation is complete.  This will update the state
        /// of the operation and notify the callback.
        /// </summary>
        /// <param name="completedSynchronously">True if the asynchronous operation completed synchronously.</param>
        protected void Complete(bool completedSynchronously)
        {
            Complete(completedSynchronously, null);
        }

        /// <summary>
        /// Call this version of complete if you raise an exception during processing.  In addition to notifying
        /// the callback, it will capture the exception and store it to be thrown during AsyncResult.End.
        /// </summary>
        /// <param name="completedSynchronously">True if the asynchronous operation completed synchronously.</param>
        /// <param name="exception">The exception during the processing of the asynchronous operation.</param>
        protected void Complete(bool completedSynchronously, Exception exception)
        {
            if (completed == true)
            {
                // it is a 
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AsynchronousOperationException(SR.GetString(SR.ID4005)));
            }

            completedSync = completedSynchronously;
            this.exception = exception;

            if (completedSynchronously)
            {
                //
                // No event should be set for synchronous completion
                //
                completed = true;
                Fx.Assert(resetEvent == null, SR.GetString(SR.ID8025));
            }
            else
            {
                //
                // Complete asynchronously
                //
                lock (thisLock)
                {
                    completed = true;
                    if (resetEvent != null)
                        resetEvent.Set();
                }
            }

            //
            // finally call the call back. Note, we are expecting user's callback to handle the exception
            // so, if the callback throw exception, all we can do is burn and crash.
            //
            try
            {
                if (callback != null)
                    callback(this);
            }
            catch (ThreadAbortException)
            {
                //
                // The thread running the callback is being aborted. We ignore this case.
                //
            }
            catch (AsynchronousOperationException)
            {
                throw;
            }
#pragma warning suppress 56500
            catch (Exception unhandledException)
            {
                //
                // The callback raising an exception is equivalent to Main raising an exception w/out a catch.
                // We should just throw it back. We should log the exception somewhere.
                //
                // Because the stack trace gets lost on a rethrow, we're wrapping it in a generic exception
                // so the stack trace is preserved.
                //
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AsynchronousOperationException(SR.GetString(SR.ID4003), unhandledException));
            }
        }

        /// <summary>
        /// Disposes of unmanaged resources held by the AsyncResult.
        /// </summary>
        /// <param name="isExplicitDispose">True if this is an explicit call to Dispose.</param>
        protected virtual void Dispose(bool isExplicitDispose)
        {
            if (!disposed)
            {
                if (isExplicitDispose)
                {
                    lock (thisLock)
                    {
                        if (!disposed)
                        {
                            //
                            // Mark disposed
                            //
                            disposed = true;

                            //
                            // Called explicitly to close the object
                            //
                            if (resetEvent != null)
                                resetEvent.Close();
                        }
                    }
                }
                else
                {
                    //
                    // Called for finalization
                    //
                }
            }
        }

        #region IAsyncResult implementation

        /// <summary>
        /// Gets the user-defined state information that was passed to the Begin method.
        /// </summary>
        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// Gets the wait handle of the async event.
        /// </summary>
        public virtual WaitHandle AsyncWaitHandle
        {
            get
            {
                if (resetEvent == null)
                {
                    bool savedCompleted = completed;

                    lock (thisLock)
                    {
                        if (resetEvent == null)
                            resetEvent = new ManualResetEvent(completed);
                    }

                    if (!savedCompleted && completed)
                        resetEvent.Set();
                }

                return resetEvent;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return completedSync;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return completed;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

