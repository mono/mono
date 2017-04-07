//------------------------------------------------------------------------------
// <copyright file="SuspendManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Web.Util;

    // Handles calling suspend and resume methods, including issues around
    // synchronization and timeout handling.

    internal sealed class SuspendManager {

        private static readonly TimeSpan _suspendMethodTimeout = TimeSpan.FromSeconds(5);

        private readonly ConcurrentDictionary<ISuspendibleRegisteredObject, object> _registeredObjects = new ConcurrentDictionary<ISuspendibleRegisteredObject, object>();

        public void RegisterObject(ISuspendibleRegisteredObject o) {
            Debug.Assert(o != null);
            _registeredObjects[o] = null;
        }

        public void UnregisterObject(ISuspendibleRegisteredObject o) {
            Debug.Assert(o != null);
            ((IDictionary<ISuspendibleRegisteredObject, object>)_registeredObjects).Remove(o);
        }

        // Returns a state object that will be passed to the Resume method.
        public object Suspend() {
            // ConcurrentDictionary.Count / IsEmpty are basically as expensive as getting
            // the entire collection of keys, so we may as well just read the keys anyway.
            var allRegisteredObjects = _registeredObjects.Keys;
            return SuspendImpl(allRegisteredObjects);
        }

        private static SuspendState SuspendImpl(ICollection<ISuspendibleRegisteredObject> allRegisteredObjects) {
            // Our behavior is:
            // - We'll call each registered object's suspend method serially.
            // - All methods have a combined 5 seconds to respond, at which
            //   point we'll forcibly return to our caller.
            // - If a Resume call comes in, we'll not call any Suspend methods
            //   we haven't yet gotten around to, and we'll execute each
            //   resume callback we got.
            // - Resume callbacks may fire in parallel, even if Suspend methods
            //   fire sequentially.
            // - Resume methods fire asynchronously, so other events (such as
            //   Stop or a new Suspend call) could happen while a Resume callback
            //   is in progress.

            CountdownEvent countdownEvent = new CountdownEvent(2);
            SuspendState suspendState = new SuspendState(allRegisteredObjects);

            // Unsafe QUWI since occurs outside the context of a request.
            // We are not concerned about impersonation, identity, etc.

            // Invoke any registered subscribers to let them know that we're about
            // to suspend. This is done in parallel with ASP.NET's own cleanup below.
            if (allRegisteredObjects.Count > 0) {
                ThreadPool.UnsafeQueueUserWorkItem(_ => {
                    suspendState.Suspend();
                    countdownEvent.Signal();
                }, null);
            }
            else {
                countdownEvent.Signal(); // nobody is subscribed
            }

            // Release any unnecessary memory that we're holding on to. The GC will
            // be able to reclaim these, which means that we'll have to page in less
            // memory when the next request comes in.
            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                // Release any char[] buffers we're keeping around
                HttpWriter.ReleaseAllPooledBuffers();

                // Trim expired entries from the runtime cache
                var iCache = HttpRuntime.Cache.GetInternalCache(createIfDoesNotExist: false);
                var oCache = HttpRuntime.Cache.GetObjectCache(createIfDoesNotExist: false);
                if (iCache != null) {
                    iCache.Trim(0);
                }
                if (oCache != null && !oCache.Equals(iCache)) {
                    oCache.Trim(0);
                }

                // Trim all pooled HttpApplication instances
                HttpApplicationFactory.TrimApplicationInstances(removeAll: true);

                countdownEvent.Signal();
            }, null);

            if (Debug.IsDebuggerPresent()) {
                countdownEvent.Wait(); // to assist with debugging, don't time out if a debugger is attached
            }
            else {
                countdownEvent.Wait(_suspendMethodTimeout); // blocking call, ok for our needs since has finite wait time
            }
            return suspendState;
        }

        public void Resume(object state) {
            ((SuspendState)state).Resume();
        }

        internal sealed class SuspendState {
            private static readonly WaitCallback _quwiThunk = (state) => ((Action)state)();

            private readonly ICollection<ISuspendibleRegisteredObject> _suspendibleObjects;

            // these two fields should only ever be accessed under lock
            private readonly List<Action> _resumeCallbacks;
            private bool _resumeWasCalled;

            public SuspendState(ICollection<ISuspendibleRegisteredObject> suspendibleObjects) {
                _suspendibleObjects = suspendibleObjects;
                _resumeCallbacks = new List<Action>(suspendibleObjects.Count);
            }

            public void Suspend() {
                foreach (ISuspendibleRegisteredObject suspendibleObject in _suspendibleObjects) {
                    Action callback = suspendibleObject.Suspend();

                    lock (this) {
                        // If Resume was called while the Suspend method was still executing,
                        // this callback won't be invoked, so we need to invoke it manually.
                        if (_resumeWasCalled) {
                            if (callback != null) {
                                InvokeResumeCallbackAsync(callback);
                            }
                            return; // don't run any other Suspend methods
                        }

                        if (callback != null) {
                            _resumeCallbacks.Add(callback);
                        }
                    }
                }
            }

            public void Resume() {
                lock (this) {
                    Debug.Assert(!_resumeWasCalled, "Resume was called too many times!");
                    _resumeWasCalled = true;

                    foreach (Action callback in _resumeCallbacks) {
                        InvokeResumeCallbackAsync(callback);
                    }
                }
            }

            private static void InvokeResumeCallbackAsync(Action callback) {
                // Unsafe QUWI since occurs outside the context of a request.
                // We are not concerned about impersonation, identity, etc.
                ThreadPool.UnsafeQueueUserWorkItem(_quwiThunk, callback);
            }
        }
    }
}
