//------------------------------------------------------------------------------
// <copyright file="WebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.WebSockets;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Util;
    using System.Web.WebSockets;

    // Responsible for kicking off the WebSocket pipeline at the end of an ASP.NET request

    internal sealed class WebSocketPipeline : IDisposable, ISyncContext {

        private readonly RootedObjects _root;
        private readonly HttpContext _httpContext;
        private volatile bool _isProcessingComplete;
        private Func<AspNetWebSocketContext, Task> _userFunc;
        private readonly string _subProtocol;

        public WebSocketPipeline(RootedObjects root, HttpContext httpContext, Func<AspNetWebSocketContext, Task> userFunc, string subProtocol) {
            _root = root;
            _httpContext = httpContext;
            _userFunc = userFunc;
            _subProtocol = subProtocol;
        }

        public void Dispose() {
            // disposal not currently needed
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Web.Hosting.UnsafeIISMethods.MgdPostCompletion(System.IntPtr,System.Web.RequestNotificationStatus)", Justification = @"This will never return an error HRESULT.")]
        public void ProcessRequest() {
            Task<AspNetWebSocket> webSocketTask = ProcessRequestImplAsync();

            // If 'webSocketTask' contains a non-null Result, this is our last chance to ensure that we
            // have completed all pending IO. Otherwise we run the risk of iiswsock.dll making a reverse
            // p/invoke call into our managed layer and touching objects that have already been GCed.
            Task abortTask = webSocketTask.ContinueWith(task => (task.Result != null) ? ((AspNetWebSocket)task.Result).AbortAsync() : (Task)null, TaskContinuationOptions.ExecuteSynchronously).Unwrap();

            // Once all pending IOs are complete, we can progress the IIS state machine and finish the request.
            // Execute synchronously since it's very short-running (posts to the native ThreadPool).
            abortTask.ContinueWith(_ => UnsafeIISMethods.MgdPostCompletion(_root.WorkerRequest.RequestContext, RequestNotificationStatus.Continue), TaskContinuationOptions.ExecuteSynchronously);
        }

        private ExceptionDispatchInfo DoFlush() {
            // See comments in ProcessRequestImplAsync() for why this method returns an ExceptionDispatchInfo
            // rather than allowing exceptions to propagate out.

            try {
                _root.WorkerRequest.FlushResponse(finalFlush: true); // pushes buffers to IIS; completes synchronously
                _root.WorkerRequest.ExplicitFlush(); // signals IIS to push its buffers to the network
                return null;
            }
            catch (Exception ex) {
                return ExceptionDispatchInfo.Capture(ex);
            }
        }

        private async Task<AspNetWebSocket> ProcessRequestImplAsync() {
            AspNetWebSocket webSocket = null;

            try {
                // SendResponse and other asynchronous notifications cannot be process by ASP.NET after this point.
                _root.WorkerRequest.SuppressSendResponseNotifications();

                // A flush is necessary to activate the WebSocket module so that we can get its pointer.
                //
                // DevDiv #401948: We can't allow a flush failure to propagate out, otherwise the rest of
                // this method doesn't run, which could leak resources (by not invoking the user callback)
                // or cause weird behavior (by not calling CompleteTransitionToWebSocket, which could corrupt
                // server state). If the flush fails, we'll wait to propagate the exception until a safe
                // point later in this method.
                ExceptionDispatchInfo flushExceptionDispatchInfo = DoFlush();

                // Create the AspNetWebSocket. There's a chance that the client disconnected before we
                // hit this code. If this is the case, we'll pass a null WebSocketPipe to the
                // AspNetWebSocket ctor, which immediately sets the socket into an aborted state.
                UnmanagedWebSocketContext unmanagedWebSocketContext = _root.WorkerRequest.GetWebSocketContext();
                WebSocketPipe pipe = (unmanagedWebSocketContext != null) ? new WebSocketPipe(unmanagedWebSocketContext, PerfCounters.Instance) : null;
                webSocket = new AspNetWebSocket(pipe, _subProtocol);

                // slim down the HttpContext as much as possible to allow the GC to reclaim memory
                _httpContext.CompleteTransitionToWebSocket();

                // always install a new SynchronizationContext, even if the user is running in legacy SynchronizationContext mode
                AspNetSynchronizationContext syncContext = new AspNetSynchronizationContext(this);
                _httpContext.SyncContext = syncContext;

                bool webSocketRequestSucceeded = false;
                try {
                    // need to keep track of this in the manager so that we can abort if it the AppDomain goes down
                    AspNetWebSocketManager.Current.Add(webSocket);

                    // bump up the total count (the currently-executing count is recorded separately)
                    PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL_WEBSOCKETS);

                    // Release the reference to the user delegate (which might just be a simple initialization routine) so that
                    // the GC can claim it. The only thing that needs to remain alive is the Task itself, which we're referencing.
                    Task task = null;
                    syncContext.Send(_ => {
                        task = _userFunc(new AspNetWebSocketContextImpl(new HttpContextWrapper(_httpContext), _root.WorkerRequest, webSocket));
                    }, null);

                    // Was there an exception from user code? If so, rethrow (which logs).
                    ExceptionDispatchInfo exception = syncContext.ExceptionDispatchInfo;
                    if (exception != null) {
                        exception.Throw();
                    }

                    _userFunc = null;
                    await task.ConfigureAwait(continueOnCapturedContext: false);

                    // Was there an exception from the earlier call to DoFlush? If so, rethrow (which logs).
                    // This needs to occur after the user's callback finishes, otherwise ASP.NET could try
                    // to complete the request while the callback is still accessing it.
                    if (flushExceptionDispatchInfo != null) {
                        flushExceptionDispatchInfo.Throw();
                    }

                    // Any final state except Aborted is marked as 'success'.
                    // It's possible execution never reaches this point, e.g. if the user's
                    // callback throws an exception. In that case, 'webSocketRequestSucceeded'
                    // will keep its default value of false, and the performance counter
                    // will mark this request as having failed.

                    if (webSocket.State != WebSocketState.Aborted) {
                        webSocketRequestSucceeded = true;
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_SUCCEEDED_WEBSOCKETS);
                    }
                }
                finally {
                    // we need to make sure the user can't call the WebSocket any more after this point
                    _isProcessingComplete = true;
                    webSocket.DisposeInternal();
                    AspNetWebSocketManager.Current.Remove(webSocket);

                    if (!webSocketRequestSucceeded) {
                        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_FAILED_WEBSOCKETS);
                    }
                }
            }
            catch (Exception ex) {
                // don't let the exception propagate upward; just log it instead
                WebBaseEvent.RaiseRuntimeError(ex, null);
            }

            return webSocket;
        }

        // consumed by AppVerifier when it is enabled
        HttpContext ISyncContext.HttpContext {
            get {
                // if processing is finished, this ISyncContext is no longer logically associated with an HttpContext, so return null
                return (_isProcessingComplete) ? null : _httpContext;
            }
        }

        ISyncContextLock ISyncContext.Enter() {
            // Restores impersonation, Culture, etc.
            ThreadContext threadContext = new ThreadContext(_httpContext);
            threadContext.AssociateWithCurrentThread(_httpContext.UsesImpersonation);
            return threadContext;
        }

    }
}
