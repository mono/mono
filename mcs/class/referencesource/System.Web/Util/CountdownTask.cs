//------------------------------------------------------------------------------
// <copyright file="CountdownTask.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // This class is similar to CountdownEvent, but it uses Task under the covers
    // instead of ManualResetEvent. When the internal counter hits zero, the Task
    // is marked as complete.
    //
    // This type is thread-safe. Error checking is not performed in RET builds.

    internal sealed class CountdownTask {

        private int _pendingCount;
        private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

        public CountdownTask(int initialCount) {
            AddCount(initialCount);
        }

        public int CurrentCount {
            get { return _pendingCount; }
        }

        public Task Task {
            get { return _tcs.Task; }
        }

        private void AddCount(int delta) {
            int newCount = Interlocked.Add(ref _pendingCount, delta);
            Debug.Assert(newCount >= 0, "The counter should never hit a negative value.");

            if (newCount == 0) {
                bool success = _tcs.TrySetResult(null);
                Debug.Assert(success, "TrySetResult shouldn't have been already called.");
            }
        }

        public void MarkOperationCompleted() {
            AddCount(-1);
        }

        public void MarkOperationPending() {
            AddCount(+1);
        }

    }
}
