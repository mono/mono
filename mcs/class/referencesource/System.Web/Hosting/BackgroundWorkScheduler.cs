//------------------------------------------------------------------------------
// <copyright file="BackgroundWorkScheduler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Util;

    internal sealed class BackgroundWorkScheduler : IRegisteredObject {

        private readonly CancellationTokenHelper _cancellationTokenHelper = new CancellationTokenHelper(canceled: false);
        private int _numExecutingWorkItems; // number of executing work items, not scheduled work items; a work item might never be scheduled
        private readonly Action<BackgroundWorkScheduler> _unregisterCallback;
        private readonly Action<AppDomain, Exception> _logCallback;
        private readonly Action _workItemCompleteCallback;

        internal BackgroundWorkScheduler(Action<BackgroundWorkScheduler> unregisterCallback, Action<AppDomain, Exception> logCallback, Action workItemCompleteCallback = null) {
            Debug.Assert(unregisterCallback != null);
            _unregisterCallback = unregisterCallback;
            _logCallback = logCallback;
            _workItemCompleteCallback = workItemCompleteCallback;
        }

        private void FinalShutdown() {
            _unregisterCallback(this);
        }

        // we can use 'async void' here since we're guaranteed to be off the AspNetSynchronizationContext
        private async void RunWorkItemImpl(Func<CancellationToken, Task> workItem) {
            Task returnedTask = null;
            try {
                returnedTask = workItem(_cancellationTokenHelper.Token);
                await returnedTask.ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex) {
                // ---- exceptions caused by the returned task being canceled
                if (returnedTask != null && returnedTask.IsCanceled) {
                    return;
                }

                // ---- exceptions caused by CancellationToken.ThrowIfCancellationRequested()
                OperationCanceledException operationCanceledException = ex as OperationCanceledException;
                if (operationCanceledException != null && operationCanceledException.CancellationToken == _cancellationTokenHelper.Token) {
                    return;
                }

                _logCallback(AppDomain.CurrentDomain, ex); // method shouldn't throw
            }
            finally {
                WorkItemComplete();
            }
        }

        public void ScheduleWorkItem(Func<CancellationToken, Task> workItem) {
            Debug.Assert(workItem != null);

            if (_cancellationTokenHelper.IsCancellationRequested) {
                return; // we're not going to run this work item
            }

            // Unsafe* since we want to get rid of Principal and other constructs specific to the current ExecutionContext
            ThreadPool.UnsafeQueueUserWorkItem(state => {
                lock (this) {
                    if (_cancellationTokenHelper.IsCancellationRequested) {
                        return; // we're not going to run this work item
                    }
                    else {
                        _numExecutingWorkItems++;
                    }
                }

                RunWorkItemImpl((Func<CancellationToken, Task>)state);
            }, workItem);
        }

        public void Stop(bool immediate) {
            // Hold the lock for as little time as possible
            int currentWorkItemCount;
            lock (this) {
                _cancellationTokenHelper.Cancel(); // dispatched onto a new thread
                currentWorkItemCount = _numExecutingWorkItems;
            }

            if (currentWorkItemCount == 0) {
                // There was no scheduled work, so we're done
                FinalShutdown();
            }
        }

        private void WorkItemComplete() {
            // Hold the lock for as little time as possible
            int newWorkItemCount;
            bool isCancellationRequested;
            lock (this) {
                newWorkItemCount = --_numExecutingWorkItems;
                isCancellationRequested = _cancellationTokenHelper.IsCancellationRequested;
            }

            // for debugging & unit tests
            if (_workItemCompleteCallback != null) {
                _workItemCompleteCallback();
            }

            if (newWorkItemCount == 0 && isCancellationRequested) {
                // Cancellation was requested, and we were the last work item to complete, so everybody is finished
                FinalShutdown();
            }
        }
    }
}
