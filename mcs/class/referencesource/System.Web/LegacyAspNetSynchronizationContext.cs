//------------------------------------------------------------------------------
// <copyright file="LegacyAspNetSynchronizationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Util;

namespace System.Web {

    // Represents a SynchronizationContext that has legacy behavior (<= FX 4.0) when it comes to asynchronous operations.
    // Characterized by locking on the HttpApplication to synchronize work, dispatching Posts as Sends.

    internal sealed class LegacyAspNetSynchronizationContext : AspNetSynchronizationContextBase {
        private HttpApplication _application;
        private Action<bool> _appVerifierCallback;
        private bool _disabled;
        private bool _syncCaller;
        private bool _invalidOperationEncountered;
        private int _pendingCount;
        private ExceptionDispatchInfo _error;
        private WaitCallback _lastCompletionCallback;
        private object _lastCompletionCallbackLock;

        internal LegacyAspNetSynchronizationContext(HttpApplication app) {
            _application = app;
            _appVerifierCallback = AppVerifier.GetSyncContextCheckDelegate(app);
            _lastCompletionCallbackLock = new object();
        }

        private void CheckForRequestStateIfRequired() {
            if (_appVerifierCallback != null) {
                _appVerifierCallback(false);
            }
        }

        private void CallCallback(SendOrPostCallback callback, Object state) {
            CheckForRequestStateIfRequired();

            // don't take app lock for sync caller to avoid deadlocks in case they poll for result
            if (_syncCaller) {
                CallCallbackPossiblyUnderLock(callback, state);
            }
            else {
                lock (_application) {
                    CallCallbackPossiblyUnderLock(callback, state);
                }
            }
        }

        private void CallCallbackPossiblyUnderLock(SendOrPostCallback callback, Object state) {
            ThreadContext threadContext = null;
            try {
                threadContext = _application.OnThreadEnter();
                try {
                    callback(state);
                }
                catch (Exception e) {
                    _error = ExceptionDispatchInfo.Capture(e);
                }
            }
            finally {
                if (threadContext != null) {
                    threadContext.DisassociateFromCurrentThread();
                }
            }
        }

        // this property no-ops using the legacy sync context
        internal override bool AllowAsyncDuringSyncStages {
            get;
            set;
        }

        internal override int PendingOperationsCount {
            get { return _pendingCount; }
        }

        internal override ExceptionDispatchInfo ExceptionDispatchInfo {
            get { return _error; }
        }

        internal override void ClearError() {
            _error = null;
        }

        // Dev11 Bug 70908: Race condition involving SynchronizationContext allows ASP.NET requests to be abandoned in the pipeline
        //  
        // When the last completion occurs, the _pendingCount is decremented and then the _lastCompletionCallbackLock is acquired to get
        // the _lastCompletionCallback.  If the _lastCompletionCallback is non-null, then the last completion will invoke the callback;
        // otherwise, the caller of PendingCompletion will handle the completion.
        internal override bool PendingCompletion(WaitCallback callback) {
            Debug.Assert(_lastCompletionCallback == null); // only one at a time
            bool pending = false;
            if (_pendingCount != 0) {
                lock (_lastCompletionCallbackLock) {
                    if (_pendingCount != 0) {
                        pending = true;
                        _lastCompletionCallback = callback;
                    }
                }
            }
            return pending;            
        }

        public override void Send(SendOrPostCallback callback, Object state) {
#if DBG
            Debug.Trace("Async", "Send");
            Debug.Trace("AsyncStack", "Send from:\r\n" + GetDebugStackTrace());
#endif
            CallCallback(callback, state);
        }

        public override void Post(SendOrPostCallback callback, Object state) {
#if DBG
            Debug.Trace("Async", "Post");
            Debug.Trace("AsyncStack", "Post from:\r\n" + GetDebugStackTrace());
#endif
            CallCallback(callback, state);
        }

#if DBG
        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void CreateCopyDumpStack() {
            Debug.Trace("Async", "CreateCopy");
            Debug.Trace("AsyncStack", "CreateCopy from:\r\n" + GetDebugStackTrace());
        }
#endif

        public override SynchronizationContext CreateCopy() {
#if DBG
            CreateCopyDumpStack();
#endif
            LegacyAspNetSynchronizationContext context = new LegacyAspNetSynchronizationContext(_application);
            context._disabled = _disabled;
            context._syncCaller = _syncCaller;
            context.AllowAsyncDuringSyncStages = AllowAsyncDuringSyncStages;
            return context;
        }

        public override void OperationStarted() {
            if (_invalidOperationEncountered || (_disabled && _pendingCount == 0)) {
                _invalidOperationEncountered = true;
                throw new InvalidOperationException(SR.GetString(SR.Async_operation_disabled));
            }

            Interlocked.Increment(ref _pendingCount);
#if DBG
            Debug.Trace("Async", "OperationStarted(count=" + _pendingCount + ")");
            Debug.Trace("AsyncStack", "OperationStarted(count=" + _pendingCount + ") from:\r\n" + GetDebugStackTrace());
#endif
        }

        public override void OperationCompleted() {
            if (_invalidOperationEncountered || (_disabled && _pendingCount == 0)) {
                // throw from operation started could cause extra operation completed
                return;
            }

            int pendingCount = Interlocked.Decrement(ref _pendingCount);

#if DBG
            Debug.Trace("Async", "OperationCompleted(pendingCount=" + pendingCount + ")");
            Debug.Trace("AsyncStack", "OperationCompleted(pendingCount=" + pendingCount + ") from:\r\n" + GetDebugStackTrace());
#endif

            // notify (once) about the last completion to resume the async work
            if (pendingCount == 0) {
                WaitCallback callback = null;
                lock (_lastCompletionCallbackLock) {
                    callback = _lastCompletionCallback;
                    _lastCompletionCallback = null;
                }

                if (callback != null) {
                    Debug.Trace("Async", "Queueing LastCompletionWorkItemCallback");
                    ThreadPool.QueueUserWorkItem(callback);
                }
            }
        }

        internal override bool Enabled {
            get { return !_disabled; }
        }

        internal override void Enable() {
            _disabled = false;
        }

        internal override void Disable() {
            _disabled = true;
        }

        internal override void SetSyncCaller() {
            _syncCaller = true;
        }

        internal override void ResetSyncCaller() {
            _syncCaller = false;
        }

        internal override void AssociateWithCurrentThread() {
            Monitor.Enter(_application);
        }

        internal override void DisassociateFromCurrentThread() {
            Monitor.Exit(_application);
        }

#if DBG
        [EnvironmentPermission(SecurityAction.Assert, Unrestricted = true)]
        private static string GetDebugStackTrace() {
            return Environment.StackTrace;
        }
#endif
    }
}
