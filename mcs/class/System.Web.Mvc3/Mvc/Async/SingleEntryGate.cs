namespace System.Web.Mvc.Async {
    using System;
    using System.Threading;

    // used to synchronize access to a single-use consumable resource
    internal sealed class SingleEntryGate {

        private const int NOT_ENTERED = 0;
        private const int ENTERED = 1;

        private int _status;

        // returns true if this is the first call to TryEnter(), false otherwise
        public bool TryEnter() {
            int oldStatus = Interlocked.Exchange(ref _status, ENTERED);
            return (oldStatus == NOT_ENTERED);
        }

    }
}
