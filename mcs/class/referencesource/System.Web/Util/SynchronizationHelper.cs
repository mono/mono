//------------------------------------------------------------------------------
// <copyright file="SynchronizationHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    // This class is used by the AspNetSynchronizationContext to assist with scheduling tasks in a non-blocking fashion.
    // Asynchronous work will be queued and will execute sequentially, never consuming more than a single thread at a time.
    // Synchronous work will block and will execute on the current thread.

    internal sealed class SynchronizationHelper {

        private Task _completionTask; // the Task that will run when all in-flight operations have completed
        private Thread _currentThread; // the Thread that's running the current Task; all threads must see the same value for this field
        private Task _lastScheduledTask = CreateInitialTask(); // the last Task that was queued to this helper, used to hook future Tasks (not volatile since always accessed under lock)
        private readonly object _lockObj = new object(); // synchronizes access to _lastScheduledTask
        private int _operationsInFlight; // operation counter
        private readonly ISyncContext _syncContext; // a context that wraps an operation with pre- and post-execution phases
        private readonly Action _appVerifierCallback; // for making sure that developers don't try calling us after the request has completed

        public SynchronizationHelper(ISyncContext syncContext) {
            _syncContext = syncContext;
            _appVerifierCallback = AppVerifier.GetSyncContextCheckDelegate(syncContext);
        }

        // If an operation results in an exception, this property will provide access to it.
        public ExceptionDispatchInfo Error { get; set; }

        // Helper to access the _currentThread field in a thread-safe fashion.
        // It is not enough to mark the _currentThread field volatile, since that only guarantees
        // read / write ordering and doesn't ensure that each thread sees the same value.
        private Thread CurrentThread {
            get { return Interlocked.CompareExchange(ref _currentThread, null, null); }
            set { Interlocked.Exchange(ref _currentThread, value); }
        }

        // Returns the number of pending operations
        public int PendingCount { get { return ChangeOperationCount(0); } }

        public int ChangeOperationCount(int addend) {
            int newOperationCount = Interlocked.Add(ref _operationsInFlight, addend);
            if (newOperationCount == 0) {
                // if an asynchronous completion operation is queued, run it
                Task completionTask = Interlocked.Exchange(ref _completionTask, null);
                if (completionTask != null) {
                    completionTask.Start();
                }
            }

            return newOperationCount;
        }

        private void CheckForRequestCompletionIfRequired() {
            if (_appVerifierCallback != null) {
                _appVerifierCallback();
            }
        }

        // Creates the initial hook that future operations can ride off of
        private static Task CreateInitialTask() {
            return Task.FromResult<object>(null);
        }

        // Takes control of this SynchronizationHelper instance synchronously. Asynchronous operations
        // will be queued but will not be dispatched until control is released (by disposing of the
        // returned object). This operation might block if a different thread is currently in
        // control of the context.
        public IDisposable EnterSynchronousControl() {
            if (CurrentThread == Thread.CurrentThread) {
                // If the current thread already has control of this context, there's nothing extra to do.
                return DisposableAction.Empty;
            }

            // used to mark the end of the synchronous task
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task lastTask;
            lock (_lockObj) {
                lastTask = _lastScheduledTask;
                _lastScheduledTask = tcs.Task; // future work can be scheduled off this Task
            }

            // The original task may end up Faulted, which would make its Wait() method throw an exception.
            // To avoid this, we instead wait on a continuation which is always guaranteed to complete successfully.
            if (!lastTask.IsCompleted) { lastTask.ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously).Wait(); }
            CurrentThread = Thread.CurrentThread;

            // synchronous control is released by marking the Task as complete
            return new DisposableAction(() => {
                CurrentThread = null;
                tcs.TrySetResult(null);
            });
        }

        public void QueueAsynchronous(Action action) {
            CheckForRequestCompletionIfRequired();
            ChangeOperationCount(+1);

            // This method only schedules work; it doesn't itself do any work. The lock is held for a very
            // short period of time.
            lock (_lockObj) {
                Task newTask = _lastScheduledTask.ContinueWith(_ => SafeWrapCallback(action));
                _lastScheduledTask = newTask; // the newly-created task is now the last one
            }
        }

        public void QueueSynchronous(Action action) {
            CheckForRequestCompletionIfRequired();
            if (CurrentThread == Thread.CurrentThread) {
                // current thread already owns the context, so just execute inline to prevent deadlocks
                action();
                return;
            }

            ChangeOperationCount(+1);
            using (EnterSynchronousControl()) {
                SafeWrapCallback(action);
            }
        }

        private void SafeWrapCallback(Action action) {
            // This method will try to catch exceptions so that they don't bubble up to our
            // callers. However, ThreadAbortExceptions will continue to bubble up.
            try {
                CurrentThread = Thread.CurrentThread;
                ISyncContextLock syncContextLock = null;
                try {
                    syncContextLock = (_syncContext != null) ? _syncContext.Enter() : null;
                    try {
                        action();
                    }
                    catch (Exception ex) {
                        Error = ExceptionDispatchInfo.Capture(ex);
                    }
                }
                finally {
                    if (syncContextLock != null) {
                        syncContextLock.Leave();
                    }
                }
            }
            finally {
                CurrentThread = null;
                ChangeOperationCount(-1);
            }
        }

        // Sets the continuation that will asynchronously execute when the pending operation counter
        // hits zero. Returns true if asynchronous execution is expected, false if the operation
        // counter is already at zero and the caller should run the continuation inline.
        public bool TrySetCompletionContinuation(Action continuation) {
            int newOperationCount = ChangeOperationCount(+1); // prevent the operation counter from hitting zero while we're setting the field
            bool scheduledAsynchronously = (newOperationCount > 1);
            if (scheduledAsynchronously) {
                Interlocked.Exchange(ref _completionTask, new Task(continuation));
            }

            ChangeOperationCount(-1);
            return scheduledAsynchronously;
        }

    }
}
