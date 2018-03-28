//------------------------------------------------------------------------------
// <copyright file="AsyncResultBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * IAsyncResult for asynchronous reads.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Web.Util;

    internal abstract class AsyncResultBase : IAsyncResult {
        private ManualResetEventSlim _mre; // unlike ManualResetEvent, this creates a kernel object lazily
        private AsyncCallback        _callback;
        private Object               _asyncState;
        private volatile bool        _completed;
        private volatile bool        _completedSynchronously;
        private volatile int         _hresult;
        private Thread               _threadWhichStartedOperation;
        private volatile ExceptionDispatchInfo _error;

        protected AsyncResultBase(AsyncCallback cb, Object state) {
            _callback    = cb;
            _asyncState  = state;
            _mre = new ManualResetEventSlim();
        }

        internal abstract void Complete(int bytesCompleted, int hresult, IntPtr pAsyncCompletionContext, bool synchronous);

        protected void Complete(int hresult, bool synchronous) {
            if (Volatile.Read(ref _threadWhichStartedOperation) == Thread.CurrentThread) {
                // If we're calling Complete on the same thread which kicked off the operation, then
                // we ignore the 'synchronous' value that the caller provided to us since we know
                // for a fact that this is really a synchronous completion. This is only checked if
                // the caller calls the MarkCallToBeginMethod* routines below, which only occurs in
                // the Classic Mode pipeline.
                synchronous = true;
            }

            _hresult = hresult;
            _completed = true;
            _completedSynchronously = synchronous;
            _mre.Set();

            if (_callback != null)
                _callback(this);
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

        internal void ReleaseWaitHandleWhenSignaled() {
            try {
                _mre.Wait();
            }
            finally {
                _mre.Dispose();
            }
        }

        internal void SetError(Exception error) {
            Debug.Assert(error != null);
            _error = ExceptionDispatchInfo.Capture(error);
        }

        // Properties that are not part of IAsyncResult
        //

        internal int HResult { get { return _hresult; } set { _hresult = value; } }
        internal ExceptionDispatchInfo Error { get { return _error; } }

        //
        // IAsyncResult implementation
        //

        public bool         IsCompleted { get { return _completed; } }
        public bool         CompletedSynchronously { get { return _completedSynchronously; } }
        public Object       AsyncState { get { return _asyncState; } }
        public WaitHandle   AsyncWaitHandle { get { return _mre.WaitHandle; } } // wait not supported
    }
}
