namespace System.Web.Mvc.Async {
    using System;
    using System.Threading;

    internal sealed class SimpleAsyncResult : IAsyncResult {

        private readonly object _asyncState;
        private bool _completedSynchronously;
        private volatile bool _isCompleted;

        public SimpleAsyncResult(object asyncState) {
            _asyncState = asyncState;
        }

        public object AsyncState {
            get {
                return _asyncState;
            }
        }

        // ASP.NET IAsyncResult objects should never expose a WaitHandle due to potential deadlocking
        public WaitHandle AsyncWaitHandle {
            get {
                return null;
            }
        }

        public bool CompletedSynchronously {
            get {
                return _completedSynchronously;
            }
        }

        public bool IsCompleted {
            get {
                return _isCompleted;
            }
        }

        // Proper order of execution:
        // 1. Set the CompletedSynchronously property to the correct value
        // 2. Set the IsCompleted flag
        // 3. Execute the callback
        // 4. Signal the WaitHandle (which we don't have)
        public void MarkCompleted(bool completedSynchronously, AsyncCallback callback) {
            _completedSynchronously = completedSynchronously;
            _isCompleted = true;

            if (callback != null) {
                callback(this);
            }
        }

    }
}
