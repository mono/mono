//------------------------------------------------------------------------------
// <copyright file="RootedObjects.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.Util;

    // Used by the IIS integrated pipeline to reference managed objects so that they're not claimed by the GC while unmanaged code is executing.

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class RootedObjects : IPrincipalContainer {

        // These two fields are for ETW scenarios. In .NET 4.5.1, the API SetCurrentThreadActivityId
        // can be used to correlate operations back to a given activity ID, where [in our case] an
        // activity ID corresponds to a single request. We need to ref count since we have multiple
        // threads operating on (or destroying) the request at once, and we need to know when the
        // managed request has actually finished. For example, we can't just release an activity ID
        // inside of our Destroy method since Destroy might be called on a thread pool thread while
        // some other managed thread is still unwinding inside of PipelineRuntime. When this counter
        // hits zero, we can fully release the activity ID.
        private readonly bool _activityIdTracingIsEnabled;
        private readonly Guid _requestActivityId;
        private int _requestActivityIdRefCount = 1;

        private SubscriptionQueue<IDisposable> _pipelineCompletedQueue;
        private GCHandle _handle;

        private RootedObjects() {
            _handle = GCHandle.Alloc(this);
            Pointer = (IntPtr)_handle;

            // Increment active request count as soon as possible to prevent
            // shutdown of the appdomain while requests are in flight.  It
            // is decremented in Destroy().
            HttpRuntime.IncrementActivePipelineCount();

            // this is an instance field instead of a static field since ETW can be enabled at any time
            _activityIdTracingIsEnabled = ActivityIdHelper.Instance != null && AspNetEventSource.Instance.IsEnabled();
            if (_activityIdTracingIsEnabled) {
                _requestActivityId = ActivityIdHelper.UnsafeCreateNewActivityId();
            }
        }

        // The HttpContext associated with this request.
        // May be null if this is a WebSocket request or if the request has completed.
        public HttpContext HttpContext {
            get;
            set;
        }

        // The principal associated with this request.
        // May be null if there is no associated principal or if the request has completed.
        public IPrincipal Principal {
            get;
            set;
        }

        // A pointer that can be used (via FromPointer) to reference this RootedObjects instance.
        public IntPtr Pointer {
            get;
            private set;
        }

        // The WebSocketPipeline that's associated with this request.
        // May be null if this request won't be transitioned to a WebSocket request or if it has completed.
        public WebSocketPipeline WebSocketPipeline {
            get;
            set;
        }

        // The worker request (IIS7+) associated with this request.
        // May be null if the request has completed.
        public IIS7WorkerRequest WorkerRequest {
            get;
            set;
        }

        // Using a static factory instead of a public constructor since there's a side effect.
        // Namely, the new object will never be garbage collected unless Destroy() is called.
        public static RootedObjects Create() {
            return new RootedObjects();
        }

        // Fully releases all managed resources associated with this request, including
        // the HttpContext, WebSocketContext, principal, worker request, etc.
        public void Destroy() {
            Debug.Trace("RootedObjects", "Destroy");

            // 'isDestroying = true' means that we'll release the implicit 'this' ref in _requestActivityIdRefCount
            using (WithinTraceBlock(isDestroying: true)) {
                try {
                    ReleaseHttpContext();
                    ReleaseWebSocketPipeline();
                    ReleaseWorkerRequest();
                    ReleasePrincipal();

                    // need to raise OnPipelineCompleted outside of the ThreadContext so that HttpContext.Current, User, etc. are unavailable
                    RaiseOnPipelineCompleted();

                    PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
                }
                finally {
                    if (_handle.IsAllocated) {
                        _handle.Free();
                    }
                    Pointer = IntPtr.Zero;
                    HttpRuntime.DecrementActivePipelineCount();

                    AspNetEventSource.Instance.RequestCompleted();
                }
            }
        }

        // Analog of HttpContext.DisposeOnPipelineCompleted for the integrated pipeline
        internal ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target) {
            return _pipelineCompletedQueue.Enqueue(target);
        }

        public static RootedObjects FromPointer(IntPtr pointer) {
            GCHandle handle = (GCHandle)pointer;
            return (RootedObjects)handle.Target;
        }

        internal void RaiseOnPipelineCompleted() {
            // The callbacks really shouldn't throw exceptions, but we have a catch block just in case.
            // Since there's nobody else that can listen for these errors (the request is unwinding and
            // user code will no longer run), we'll just log the error.
            try {
                _pipelineCompletedQueue.FireAndComplete(disposable => disposable.Dispose());
            }
            catch (Exception e) {
                WebBaseEvent.RaiseRuntimeError(e, null);
            }
        }

        // Fully releases the HttpContext instance associated with this request.
        public void ReleaseHttpContext() {
            Debug.Trace("RootedObjects", "ReleaseHttpContext");
            if (HttpContext != null) {
                HttpContext.FinishPipelineRequest();
            }

            HttpContext = null;
        }

        // Disposes of the principal associated with this request.
        public void ReleasePrincipal() {
            Debug.Trace("RootedObjects", "ReleasePrincipal");
            if (Principal != null && Principal != WindowsAuthenticationModule.AnonymousPrincipal) {
                WindowsIdentity identity = Principal.Identity as WindowsIdentity; // original code only disposed of WindowsIdentity, not arbitrary IDisposable types
                if (identity != null) {
                    Principal = null;
                    identity.Dispose();
                }
            }

            // Fix Bug 640366: Setting the Principal to null (irrespective of Identity) 
            // only if framework version is above .NetFramework 4.5 as this change is new and 
            // we want to keep the functionality same for previous versions.
            if (BinaryCompatibility.Current.TargetsAtLeastFramework45) {
                Principal = null;
            }
        }

        // Disposes of the WebSocketPipeline instance associated with this request.
        public void ReleaseWebSocketPipeline() {
            Debug.Trace("RootedObjects", "ReleaseWebSocketContext");
            if (WebSocketPipeline != null) {
                WebSocketPipeline.Dispose();
            }

            WebSocketPipeline = null;
        }

        // Disposes of the worker request associated with this request.
        public void ReleaseWorkerRequest() {
            Debug.Trace("RootedObjects", "ReleaseWorkerRequest");
            if (WorkerRequest != null) {
                WorkerRequest.Dispose();
            }

            WorkerRequest = null;
        }

        // Sets up the ETW Activity ID on the current thread; caller should be:
        // using (rootedObjects.WithinTraceBlock()) {
        //   .. something that might require tracing ..
        // }
        //
        // This is designed to be *very* cheap if tracing isn't enabled.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActivityIdToken WithinTraceBlock() {
            return WithinTraceBlock(isDestroying: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ActivityIdToken WithinTraceBlock(bool isDestroying) {
            if (_activityIdTracingIsEnabled) {
                return new ActivityIdToken(this, isDestroying);
            }
            else {
                return default(ActivityIdToken);
            }
        }

        // Called once per request; emits an ETW event saying that we transitioned from IIS -> ASP.NET
        public void WriteTransferEventIfNecessary() {
            Debug.Assert(WorkerRequest != null);
            if (_activityIdTracingIsEnabled) {
                Debug.Assert(_requestActivityId != Guid.Empty);
                AspNetEventSource.Instance.RequestEnteredAspNetPipeline(WorkerRequest, _requestActivityId);
            }
        }

        internal struct ActivityIdToken : IDisposable {
            private readonly bool _isDestroying;
            private readonly Guid _originalActivityId;
            private readonly RootedObjects _rootedObjects; // might be null if this is a dummy token

            internal ActivityIdToken(RootedObjects rootedObjects, bool isDestroying) {
                Debug.Assert(ActivityIdHelper.Instance != null);
                ActivityIdHelper.Instance.SetCurrentThreadActivityId(rootedObjects._requestActivityId, out _originalActivityId);

                lock (rootedObjects) {
                    rootedObjects._requestActivityIdRefCount++;
                    Debug.Assert(rootedObjects._requestActivityIdRefCount >= 2, "The original ref count should have been 1 or higher, else the activity ID could already have been released.");
                }

                _rootedObjects = rootedObjects;
                _isDestroying = isDestroying;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() {
                if (_rootedObjects == null) {
                    return; // this was a dummy token; no-op
                }

                DisposeImpl();
            }

            private void DisposeImpl() {
                Debug.Assert(ActivityIdHelper.Instance != null);
                Debug.Assert(ActivityIdHelper.Instance.CurrentThreadActivityId == _rootedObjects._requestActivityId, "Unexpected activity ID on current thread.");

                // We use a lock instead of Interlocked.Decrement so that we can guarantee that no thread
                // ever invokes the 'if' code path below before some other thread invokes the 'else' code
                // path.
                lock (_rootedObjects) {
                    _rootedObjects._requestActivityIdRefCount -= (_isDestroying) ? 2 : 1;
                    Debug.Assert(_rootedObjects._requestActivityIdRefCount >= 0, "Somebody called Dispose() too many times.");

                    if (_rootedObjects._requestActivityIdRefCount == 0) {
                        // this overload restores the original activity ID and releases the current activity ID
                        ActivityIdHelper.Instance.SetCurrentThreadActivityId(_originalActivityId);
                    }
                    else {
                        // this overload restores the original activity ID but preserves the current activity ID
                        Guid unused;
                        ActivityIdHelper.Instance.SetCurrentThreadActivityId(_originalActivityId, out unused);
                    }
                }
            }
        }
    }
}
