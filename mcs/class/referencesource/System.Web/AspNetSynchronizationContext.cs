//------------------------------------------------------------------------------
// <copyright file="AspNetSynchronizationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Util;

    internal sealed class AspNetSynchronizationContext : AspNetSynchronizationContextBase {

        // we move all of the state to a separate field since our CreateCopy() method needs shallow copy semantics
        private readonly State _state;

        internal AspNetSynchronizationContext(ISyncContext syncContext)
            : this(new State(new SynchronizationHelper(syncContext))) {
        }

        private AspNetSynchronizationContext(State state) {
            _state = state;
        }

        internal override bool AllowAsyncDuringSyncStages {
            get {
                return _state.AllowAsyncDuringSyncStages;
            }
            set {
                _state.AllowAsyncDuringSyncStages = value;
            }
        }

        // We can't ever truly disable the AspNetSynchronizationContext, as the user and runtime can kick off asynchronous
        // operations whether we wanted them to or not. But this property can be used as a flag by Page and other types
        // to signal that asynchronous operations are not currently valid, so at least ASP.NET can avoid kicking them
        // off and can bubble an appropriate exception back to the developer.
        internal override bool Enabled {
            get { return _state.Enabled; }
        }

        internal override ExceptionDispatchInfo ExceptionDispatchInfo {
            get { return _state.Helper.Error; }
        }

        internal override int PendingOperationsCount {
            get { return _state.Helper.PendingCount; }
        }

        internal override void AllowVoidAsyncOperations() {
            _state.AllowVoidAsyncOperations = true;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used only during debug.")]
        internal override void AssociateWithCurrentThread() {
            IDisposable disassociationAction = _state.Helper.EnterSynchronousControl();

#if DBG
            IDisposable capturedDisassociationAction = disassociationAction;
            Thread capturedThread = Thread.CurrentThread;
            disassociationAction = new DisposableAction(() => {
                Debug.Assert(capturedThread == Thread.CurrentThread, String.Format("AssociateWithCurrentThread was called on thread ID '{0}', but DisassociateFromCurrentThread was called on thread ID '{1}'.", capturedThread.ManagedThreadId, Thread.CurrentThread.ManagedThreadId));
                capturedDisassociationAction.Dispose();
            });
#endif

            // Don't need to synchronize access to SyncControlDisassociationActions since only one thread can call
            // EnterSynchronousControl() at a time.
            _state.SyncControlDisassociationActions.Push(disassociationAction);
        }

        internal override void ClearError() {
            _state.Helper.Error = null;
        }

        // Called by the BCL when it needs a SynchronizationContext that is identical to the existing context
        // but does not have referential equality.
        public override SynchronizationContext CreateCopy() {
            return new AspNetSynchronizationContext(_state);
        }

        internal override void Disable() {
            _state.Enabled = false;
        }

        internal override void DisassociateFromCurrentThread() {
            // Don't need to synchronize access to SyncControlDisassociationActions since we assume that our callers are 
            // well-behaved and won't call DisassociateFromCurrentThread() on a thread other than the one which called
            // AssociateWithCurrentThread(), which itself serializes access.
            Debug.Assert(_state.SyncControlDisassociationActions.Count > 0, "DisassociateFromCurrentThread() was called on a thread which hadn't previously called AssociateWithCurrentThread().");
            IDisposable disassociationAction = _state.SyncControlDisassociationActions.Pop();
            disassociationAction.Dispose();
        }

        internal override void Enable() {
            _state.Enabled = true;
        }

        public override void OperationCompleted() {
            Interlocked.Decrement(ref _state.VoidAsyncOutstandingOperationCount); // this line goes first since ChangeOperationCount might invoke a callback which depends on this value
            _state.Helper.ChangeOperationCount(-1);
        }

        public override void OperationStarted() {
            // If the caller tries to kick off an asynchronous operation while we are not
            // processing an async module, handler, or Page, we should prohibit the operation.
            if (!AllowAsyncDuringSyncStages && !_state.AllowVoidAsyncOperations) {
                InvalidOperationException ex = new InvalidOperationException(SR.GetString(SR.Async_operation_cannot_be_started));
                throw ex;
            }

            _state.Helper.ChangeOperationCount(+1);
            Interlocked.Increment(ref _state.VoidAsyncOutstandingOperationCount);
        }

        // Dev11 




        internal override bool PendingCompletion(WaitCallback callback) {
            return _state.Helper.TrySetCompletionContinuation(() => callback(null));
        }

        public override void Post(SendOrPostCallback callback, Object state) {
            _state.Helper.QueueAsynchronous(() => callback(state));
        }

        // The method is used to post async func.
        internal void PostAsync(Func<object, Task> callback, Object state) {
            _state.Helper.QueueAsynchronousAsync(callback, state);
        }

        internal override void ProhibitVoidAsyncOperations() {
            _state.AllowVoidAsyncOperations = false;

            // If the caller tries to prohibit async operations while there are still some
            // outstanding, we should treat this as an error condition. We can't throw from
            // this method since (a) the caller generally isn't prepared for it and (b) we
            // need to wait for the outstanding operations to finish anyway, so we instead
            // need to mark the helper as faulted.
            // 
            // There is technically a race condition here: the caller isn't guaranteed to
            // observe the error if the operation counter hits zero at just the right time.
            // But it's actually not terrible if that happens, since the error is really
            // just meant to be used for diagnostic purposes.
            if (!AllowAsyncDuringSyncStages && Volatile.Read(ref _state.VoidAsyncOutstandingOperationCount) > 0) {
                InvalidOperationException ex = new InvalidOperationException(SR.GetString(SR.Async_operation_cannot_be_pending));
                _state.Helper.Error = ExceptionDispatchInfo.Capture(ex);
            }
        }

        internal override void ResetSyncCaller() {
            // no-op
            // this type doesn't special-case asynchronous work kicked off from a synchronous handler
        }

        internal override void SetSyncCaller() {
            // no-op
            // this type doesn't special-case asynchronous work kicked off from a synchronous handler
        }

        public override void Send(SendOrPostCallback callback, Object state) {
            _state.Helper.QueueSynchronous(() => callback(state));
        }

        private sealed class State {
            internal bool AllowAsyncDuringSyncStages = AppSettings.AllowAsyncDuringSyncStages;
            internal volatile bool AllowVoidAsyncOperations = false;
            internal bool Enabled = true;
            internal readonly SynchronizationHelper Helper; // handles scheduling of the asynchronous tasks
            internal Stack<IDisposable> SyncControlDisassociationActions = new Stack<IDisposable>(capacity: 1);
            internal int VoidAsyncOutstandingOperationCount = 0;

            internal State(SynchronizationHelper helper) {
                Helper = helper;
            }
        }

    }
}
