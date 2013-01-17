namespace System.Web.Mvc.Async {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    // This class is used for the following pattern:

    // public IAsyncResult BeginInner(..., callback, state);
    // public TInnerResult EndInner(asyncResult);
    // public IAsyncResult BeginOuter(..., callback, state);
    // public TOuterResult EndOuter(asyncResult);

    // That is, Begin/EndOuter() wrap Begin/EndInner(), potentially with pre- and post-processing.

    internal static class AsyncResultWrapper {

        // helper methods

        private static Func<AsyncVoid> MakeVoidDelegate(Action action) {
            return () =>
            {
                action();
                return default(AsyncVoid);
            };
        }

        private static EndInvokeDelegate<AsyncVoid> MakeVoidDelegate(EndInvokeDelegate endDelegate) {
            return ar =>
            {
                endDelegate(ar);
                return default(AsyncVoid);
            };
        }

        // kicks off an asynchronous operation

        public static IAsyncResult Begin<TResult>(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate) {
            return Begin<TResult>(callback, state, beginDelegate, endDelegate, null /* tag */);
        }

        public static IAsyncResult Begin<TResult>(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag) {
            return Begin<TResult>(callback, state, beginDelegate, endDelegate, tag, Timeout.Infinite);
        }

        public static IAsyncResult Begin<TResult>(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag, int timeout) {
            WrappedAsyncResult<TResult> asyncResult = new WrappedAsyncResult<TResult>(beginDelegate, endDelegate, tag);
            asyncResult.Begin(callback, state, timeout);
            return asyncResult;
        }

        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate endDelegate) {
            return Begin(callback, state, beginDelegate, endDelegate, null /* tag */);
        }

        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate endDelegate, object tag) {
            return Begin(callback, state, beginDelegate, endDelegate, tag, Timeout.Infinite);
        }

        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate endDelegate, object tag, int timeout) {
            return Begin<AsyncVoid>(callback, state, beginDelegate, MakeVoidDelegate(endDelegate), tag, timeout);
        }

        // wraps a synchronous operation in an asynchronous wrapper, but still completes synchronously

        public static IAsyncResult BeginSynchronous<TResult>(AsyncCallback callback, object state, Func<TResult> func) {
            return BeginSynchronous<TResult>(callback, state, func, null /* tag */);
        }

        public static IAsyncResult BeginSynchronous<TResult>(AsyncCallback callback, object state, Func<TResult> func, object tag) {
            // Begin() doesn't perform any work on its own and returns immediately.
            BeginInvokeDelegate beginDelegate = (asyncCallback, asyncState) =>
            {
                SimpleAsyncResult innerAsyncResult = new SimpleAsyncResult(asyncState);
                innerAsyncResult.MarkCompleted(true /* completedSynchronously */, asyncCallback);
                return innerAsyncResult;
            };

            // The End() method blocks.
            EndInvokeDelegate<TResult> endDelegate = _ =>
            {
                return func();
            };

            WrappedAsyncResult<TResult> asyncResult = new WrappedAsyncResult<TResult>(beginDelegate, endDelegate, tag);
            asyncResult.Begin(callback, state, Timeout.Infinite);
            return asyncResult;
        }

        public static IAsyncResult BeginSynchronous(AsyncCallback callback, object state, Action action) {
            return BeginSynchronous(callback, state, action, null /* tag */);
        }

        public static IAsyncResult BeginSynchronous(AsyncCallback callback, object state, Action action, object tag) {
            return BeginSynchronous<AsyncVoid>(callback, state, MakeVoidDelegate(action), tag);
        }

        // completes an asynchronous operation

        public static TResult End<TResult>(IAsyncResult asyncResult) {
            return End<TResult>(asyncResult, null /* tag */);
        }

        public static TResult End<TResult>(IAsyncResult asyncResult, object tag) {
            return WrappedAsyncResult<TResult>.Cast(asyncResult, tag).End();
        }

        public static void End(IAsyncResult asyncResult) {
            End(asyncResult, null /* tag */);
        }

        public static void End(IAsyncResult asyncResult, object tag) {
            End<AsyncVoid>(asyncResult, tag);
        }

        [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The Timer will be disposed of either when it fires or when the operation completes successfully.")]
        private sealed class WrappedAsyncResult<TResult> : IAsyncResult {

            private readonly BeginInvokeDelegate _beginDelegate;
            private readonly object _beginDelegateLockObj = new object();
            private readonly EndInvokeDelegate<TResult> _endDelegate;
            private readonly SingleEntryGate _endExecutedGate = new SingleEntryGate(); // prevent End() from being called twice
            private readonly SingleEntryGate _handleCallbackGate = new SingleEntryGate(); // prevent callback from being handled multiple times
            private IAsyncResult _innerAsyncResult;
            private AsyncCallback _originalCallback;
            private readonly object _tag; // prevent an instance of this type from being passed to the wrong End() method
            private volatile bool _timedOut;
            private Timer _timer;

            public WrappedAsyncResult(BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag) {
                _beginDelegate = beginDelegate;
                _endDelegate = endDelegate;
                _tag = tag;
            }

            public object AsyncState {
                get {
                    return _innerAsyncResult.AsyncState;
                }
            }

            public WaitHandle AsyncWaitHandle {
                get {
                    return _innerAsyncResult.AsyncWaitHandle;
                }
            }

            public bool CompletedSynchronously {
                get {
                    return _innerAsyncResult.CompletedSynchronously;
                }
            }

            public bool IsCompleted {
                get {
                    return _innerAsyncResult.IsCompleted;
                }
            }

            // kicks off the process, instantiates a timer if requested
            public void Begin(AsyncCallback callback, object state, int timeout) {
                _originalCallback = callback;
                bool completedSynchronously;

                // Force the target Begin() operation to complete before the callback can continue,
                // since the target operation might perform post-processing of the data.
                lock (_beginDelegateLockObj) {
                    _innerAsyncResult = _beginDelegate(HandleAsynchronousCompletion, state);

                    completedSynchronously = _innerAsyncResult.CompletedSynchronously;
                    if (!completedSynchronously) {
                        if (timeout > Timeout.Infinite) {
                            CreateTimer(timeout);
                        }
                    }
                }

                if (completedSynchronously) {
                    if (callback != null) {
                        callback(this);
                    }
                }
            }

            public static WrappedAsyncResult<TResult> Cast(IAsyncResult asyncResult, object tag) {
                if (asyncResult == null) {
                    throw new ArgumentNullException("asyncResult");
                }

                WrappedAsyncResult<TResult> castResult = asyncResult as WrappedAsyncResult<TResult>;
                if (castResult != null && Object.Equals(castResult._tag, tag)) {
                    return castResult;
                }
                else {
                    throw Error.AsyncCommon_InvalidAsyncResult("asyncResult");
                }
            }

            private void CreateTimer(int timeout) {
                // this method should be called within a lock(_beginDelegateLockObj)
                _timer = new Timer(HandleTimeout, null, timeout, Timeout.Infinite /* disable periodic signaling */);
            }

            public TResult End() {
                if (!_endExecutedGate.TryEnter()) {
                    throw Error.AsyncCommon_AsyncResultAlreadyConsumed();
                }

                if (_timedOut) {
                    throw new TimeoutException();
                }
                WaitForBeginToCompleteAndDestroyTimer();

                return _endDelegate(_innerAsyncResult);
            }

            private void ExecuteAsynchronousCallback(bool timedOut) {
                WaitForBeginToCompleteAndDestroyTimer();

                if (_handleCallbackGate.TryEnter()) {
                    _timedOut = timedOut;
                    if (_originalCallback != null) {
                        _originalCallback(this);
                    }
                }
            }

            private void HandleAsynchronousCompletion(IAsyncResult asyncResult) {
                if (asyncResult.CompletedSynchronously) {
                    // If the operation completed synchronously, the WrappedAsyncResult.Begin() method will handle it.
                    return;
                }

                ExecuteAsynchronousCallback(false /* timedOut */);
            }

            private void HandleTimeout(object state) {
                ExecuteAsynchronousCallback(true /* timedOut */);
            }

            private void WaitForBeginToCompleteAndDestroyTimer() {
                lock (_beginDelegateLockObj) {
                    // Wait for the target Begin() method to complete, as it might be performing
                    // post-processing. This also forces a memory barrier, so _innerAsyncResult
                    // is guaranteed to be non-null at this point.

                    if (_timer != null) {
                        _timer.Dispose();
                    }
                    _timer = null;
                }
            }

        }

    }
}
