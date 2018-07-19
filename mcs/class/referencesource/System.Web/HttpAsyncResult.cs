//------------------------------------------------------------------------------
// <copyright file="HttpAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ASP.NET simple internal implementation of IAsyncResult
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Threading;
    using System.Web.Util;

    internal class HttpAsyncResult : IAsyncResult {
        private AsyncCallback _callback;
        private Object        _asyncState;

        private bool          _completed;
        private bool          _completedSynchronously;

        private Object        _result;
        private Exception     _error;
        private Thread        _threadWhichStartedOperation;

        // pipeline support
        private RequestNotificationStatus
                              _status;

        /*
         * Constructor with pending result
         */
        internal HttpAsyncResult(AsyncCallback cb, Object state) {
            _callback    = cb;
            _asyncState  = state;
            _status      = RequestNotificationStatus.Continue;
        }

        /*
         * Constructor with known result
         */
        internal HttpAsyncResult(AsyncCallback cb, Object state,
                                 bool completed, Object result, Exception error) {
            _callback    = cb;
            _asyncState  = state;

            _completed = completed;
            _completedSynchronously = completed;

            _result = result;
            _error = error;
            _status = RequestNotificationStatus.Continue;

            if (_completed && _callback != null)
                _callback(this);
        }

        internal void SetComplete() {
            _completed = true;
        }

        /*
         * Helper method to process completions
         */
        internal void Complete(bool synchronous, Object result, Exception error, RequestNotificationStatus status) {
            if (Volatile.Read(ref _threadWhichStartedOperation) == Thread.CurrentThread) {
                // If we're calling Complete on the same thread which kicked off the operation, then
                // we ignore the 'synchronous' value that the caller provided to us since we know
                // for a fact that this is really a synchronous completion. This is only checked if
                // the caller calls the MarkCallToBeginMethod* routines below.
                synchronous = true;
            }

            _completed              = true;
            _completedSynchronously = synchronous;
            _result                 = result;
            _error                  = error;
            _status                 = status;

            if (_callback != null)
                _callback(this);
        }

        internal void Complete(bool synchronous, Object result, Exception error) {
            Complete(synchronous, result, error, RequestNotificationStatus.Continue);
        }


        /*
         * Helper method to implement End call to async method
         */
        internal Object End() {
            if (_error != null)
                throw new HttpException(null, _error);

            return _result;
        }

        // If the caller needs to invoke an asynchronous method where the only way of knowing whether the
        // method actually completed synchronously is to inspect which thread the callback was invoked on,
        // then the caller should surround the asynchronous call with calls to the below Started / Completed
        // methods. The callback can compare the captured thread against the current thread to see if the
        // completion was synchronous. The caller calls the Completed method when unwinding so that the
        // captured thread can be cleared out, preventing an asynchronous invocation on the same thread
        // from being mistaken for a synchronous invocation.

        internal void MarkCallToBeginMethodStarted() {
            Thread originalThread = Interlocked.CompareExchange(ref _threadWhichStartedOperation, Thread.CurrentThread, null);
            Debug.Assert(originalThread == null, "Another thread already called MarkCallToBeginMethodStarted.");
        }

        internal void MarkCallToBeginMethodCompleted() {
            Thread originalThread = Interlocked.Exchange(ref _threadWhichStartedOperation, null);
            Debug.Assert(originalThread == Thread.CurrentThread, "This thread did not call MarkCallToBeginMethodStarted.");
        }

        //
        // Properties that are not part of IAsyncResult
        //

        internal Exception   Error { get { return _error;}}

        internal RequestNotificationStatus Status {
            get {
                return _status;
            }
        }

        //
        // IAsyncResult implementation
        //

        public bool         IsCompleted { get { return _completed;}}
        public bool         CompletedSynchronously { get { return _completedSynchronously;}}
        public Object       AsyncState { get { return _asyncState;}}
        public WaitHandle   AsyncWaitHandle { get { return null;}} // wait not supported
    }

}
