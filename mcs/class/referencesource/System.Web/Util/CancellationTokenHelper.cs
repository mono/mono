//------------------------------------------------------------------------------
// <copyright file="CancellationTokenHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Threading;

    // Helper class for dealing with CancellationToken instances. Our Cancel and Dispose methods
    // are fully thread-safe and will never block or throw, while a normal CancellationTokenSource
    // doesn't make these guarantees.

    internal sealed class CancellationTokenHelper : IDisposable {

        private const int STATE_CREATED = 0;
        private const int STATE_CANCELING = 1;
        private const int STATE_CANCELED = 2;
        private const int STATE_DISPOSING = 3;
        private const int STATE_DISPOSED = 4; // terminal state

        // A CancellationTokenHelper which is already marked as disposed; useful for avoiding
        // allocations of CancellationTokenHelper instances which are never observed.
        internal static readonly CancellationTokenHelper StaticDisposed = GetStaticDisposedHelper();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _state;

        public CancellationTokenHelper(bool canceled) {
            if (canceled) {
                _cts.Cancel();
            }
            _state = (canceled) ? STATE_CANCELED : STATE_CREATED;
        }

        internal bool IsCancellationRequested {
            get { return _cts.IsCancellationRequested; }
        }

        internal CancellationToken Token {
            get { return _cts.Token; }
        }

        // Cancels the token.
        public void Cancel() {
            if (Interlocked.CompareExchange(ref _state, STATE_CANCELING, STATE_CREATED) == STATE_CREATED) {
                // Only allow cancellation if the token hasn't yet been canceled or disposed.
                // Cancel on a ThreadPool thread so that we can release the original thread back to IIS.
                // We can use UnsafeQUWI to avoid an extra ExecutionContext capture since CancellationToken already captures it.
                ThreadPool.UnsafeQueueUserWorkItem(_ => {
                    try {
                        _cts.Cancel();
                    }
                    catch {
                        // ---- all exceptions to avoid killing the worker process.
                    }
                    finally {
                        if (Interlocked.CompareExchange(ref _state, STATE_CANCELED, STATE_CANCELING) == STATE_DISPOSING) {
                            // A call to Dispose() came in on another thread while we were in the middle of a cancel
                            // operation. That thread will no-op, so we'll dispose of it here.
                            _cts.Dispose();
                            Interlocked.Exchange(ref _state, STATE_DISPOSED);
                        }
                    }
                }, null);
            }
        }

        // Disposes of the token.
        public void Dispose() {
            // Only allow a single call to Dispose.
            int originalState = Interlocked.Exchange(ref _state, STATE_DISPOSING);
            switch (originalState) {
                case STATE_CREATED:
                case STATE_CANCELED:
                    // If Cancel() hasn't yet been called or has already run to completion,
                    // the underlying CTS guarantees that the Dispose method won't block
                    // or throw, so we can just call it directly.
                    _cts.Dispose();
                    Interlocked.Exchange(ref _state, STATE_DISPOSED);
                    break;

                case STATE_DISPOSED:
                    // If the object was already disposed, we need to reset the flag here
                    // since we accidentally blew it away with the original Exchange.
                    Interlocked.Exchange(ref _state, STATE_DISPOSED);
                    break;

                // Otherwise, the object is already canceling or disposing, so the
                // other thread will handle the call to Dispose().
            }
        }

        private static CancellationTokenHelper GetStaticDisposedHelper() {
            CancellationTokenHelper helper = new CancellationTokenHelper(false);
            helper.Dispose();
            return helper;
        }

    }
}
