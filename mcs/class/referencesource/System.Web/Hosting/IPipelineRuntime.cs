//------------------------------------------------------------------------------
// <copyright file="IIS7Runtime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * The ASP.NET/IIS 7 integrated pipeline runtime service host
 *
 * Copyright (c) 2004 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Util;
    using System.Web;
    using System.Web.Management;
    using System.IO;

    using IIS = UnsafeIISMethods;

    delegate void AsyncCompletionDelegate(
        IntPtr rootedObjectsPointer, 
        int bytesRead, 
        int hresult,
        IntPtr pAsyncCompletionContext);

    delegate void AsyncDisconnectNotificationDelegate(
        IntPtr pManagedRootedObjects);

    // this delegate is called from native code
    // each time a native-managed
    // transition is made to process a request state
    delegate int ExecuteFunctionDelegate(
            IntPtr rootedObjectsPointer,
            IntPtr nativeRequestContext,
            IntPtr moduleData,
            int flags);

    delegate IntPtr PrincipalFunctionDelegate(
            IntPtr rootedObjectsPointer,
            int requestingAppDomainId);

    delegate int RoleFunctionDelegate(
            IntPtr pRootedObjects,
            IntPtr pszRole,
            int cchRole,
            out bool isInRole);

    // this delegate is called from native code when the request is complete
    // to free any managed resources associated with the request
    delegate void DisposeFunctionDelegate( [In] IntPtr rootedObjectsPointer );

    [ComImport, Guid("c96cb854-aec2-4208-9ada-a86a96860cb6"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPipelineRuntime {
        void StartProcessing();
        void StopProcessing();
        void InitializeApplication([In] IntPtr appContext);
        IntPtr GetAsyncCompletionDelegate();
        IntPtr GetAsyncDisconnectNotificationDelegate();
        IntPtr GetExecuteDelegate();
        IntPtr GetDisposeDelegate();
        IntPtr GetRoleDelegate();
        IntPtr GetPrincipalDelegate();
    }

    /// <include file='doc\ISAPIRuntime.uex' path='docs/doc[@for="ISAPIRuntime"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    /// <internalonly/>
    internal sealed class PipelineRuntime : MarshalByRefObject, IPipelineRuntime, IRegisteredObject {

        // initialization error handling
        internal const string InitExceptionModuleName = "AspNetInitializationExceptionModule";
        private const string s_InitExceptionModulePrecondition = "";

        // to control removal from unmanaged table (to it only once)
        private static int s_isThisAppDomainRemovedFromUnmanagedTable;
        private static IntPtr s_ApplicationContext;
        private static string s_thisAppDomainsIsapiAppId;

        // when GL_APPLICATION_STOP fires, this is set to true to indicate that we can unload the AppDomain
        private static bool s_StopProcessingCalled;
        private static bool s_InitializationCompleted;

        // keep rooted through the app domain lifetime
        private static object _delegatelock = new object();

        private static int _inIndicateCompletionCount;

        private static IntPtr _asyncCompletionDelegatePointer = IntPtr.Zero;
        private static AsyncCompletionDelegate _asyncCompletionDelegate = null;

        private static IntPtr _asyncDisconnectNotificationDelegatePointer = IntPtr.Zero;
        private static AsyncDisconnectNotificationDelegate _asyncDisconnectNotificationDelegate = null;

        private static IntPtr _executeDelegatePointer = IntPtr.Zero;
        private static ExecuteFunctionDelegate _executeDelegate = null;

        private static IntPtr _disposeDelegatePointer = IntPtr.Zero;
        private static DisposeFunctionDelegate _disposeDelegate = null;

        private static IntPtr _roleDelegatePointer = IntPtr.Zero;
        private static RoleFunctionDelegate _roleDelegate = null;

        private static IntPtr _principalDelegatePointer = IntPtr.Zero;
        private static PrincipalFunctionDelegate _principalDelegate = null;

        public IntPtr GetAsyncCompletionDelegate() {
            if (IntPtr.Zero == _asyncCompletionDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _asyncCompletionDelegatePointer) {
                        AsyncCompletionDelegate d = new AsyncCompletionDelegate(AsyncCompletionHandler);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                _asyncCompletionDelegate = d;
                                _asyncCompletionDelegatePointer = p;
                            }
                        }
                    }
                }
            }
            return _asyncCompletionDelegatePointer;
        }

        public IntPtr GetAsyncDisconnectNotificationDelegate() {
            if (IntPtr.Zero == _asyncDisconnectNotificationDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _asyncDisconnectNotificationDelegatePointer) {
                        AsyncDisconnectNotificationDelegate d = new AsyncDisconnectNotificationDelegate(AsyncDisconnectNotificationHandler);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                _asyncDisconnectNotificationDelegate = d;
                                _asyncDisconnectNotificationDelegatePointer = p;
                            }
                        }
                    }
                }
            }
            return _asyncDisconnectNotificationDelegatePointer;
        }

        public IntPtr GetExecuteDelegate() {
            if (IntPtr.Zero == _executeDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _executeDelegatePointer) {
                        ExecuteFunctionDelegate d = new ExecuteFunctionDelegate(ProcessRequestNotification);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                Thread.MemoryBarrier();
                                _executeDelegate = d;
                                _executeDelegatePointer = p;
                            }
                        }
                    }
                }
            }

            return _executeDelegatePointer;
        }

        public IntPtr GetDisposeDelegate() {
            if (IntPtr.Zero == _disposeDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _disposeDelegatePointer) {
                        DisposeFunctionDelegate d = new DisposeFunctionDelegate(DisposeHandler);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                Thread.MemoryBarrier();
                                _disposeDelegate = d;
                                _disposeDelegatePointer = p;
                            }
                        }
                    }
                }
            }

            return _disposeDelegatePointer;
        }

        public IntPtr GetRoleDelegate() {
            if (IntPtr.Zero == _roleDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _roleDelegatePointer) {
                        RoleFunctionDelegate d = new RoleFunctionDelegate(RoleHandler);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                Thread.MemoryBarrier();
                                _roleDelegate = d;
                                _roleDelegatePointer = p;
                            }
                        }
                    }
                }
            }

            return _roleDelegatePointer;
        }

        public IntPtr GetPrincipalDelegate() {
            if (IntPtr.Zero == _principalDelegatePointer) {
                lock (_delegatelock) {
                    if (IntPtr.Zero == _principalDelegatePointer) {
                        PrincipalFunctionDelegate d = new PrincipalFunctionDelegate(GetManagedPrincipalHandler);
                        if (null != d) {
                            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
                            if (IntPtr.Zero != p) {
                                Thread.MemoryBarrier();
                                _principalDelegate = d;
                                _principalDelegatePointer = p;
                            }
                        }
                    }
                }
            }

            return _principalDelegatePointer;
        }



        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public PipelineRuntime() {
            HostingEnvironment.RegisterObject(this);
            Debug.Trace("PipelineDomain", "RegisterObject(this) called");
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        public void StartProcessing() {
            Debug.Trace("PipelineDomain", "StartProcessing AppId = " + s_thisAppDomainsIsapiAppId);
            HostingEnvironment.SetupStopListeningHandler();
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted = true)]
        public void StopProcessing() {
            Debug.Trace("PipelineDomain", "StopProcessing with stack = " + Environment.StackTrace
                        + " for AppId= " +  s_thisAppDomainsIsapiAppId);

            if (!HostingEnvironment.StopListeningWasCalled && !HostingEnvironment.ShutdownInitiated) {
                // If GL_STOP_LISTENING wasn't triggered, the reset is likely due to a configuration change.
                HttpRuntime.SetShutdownReason(ApplicationShutdownReason.ConfigurationChange, "IIS configuration change");
            }

            s_StopProcessingCalled = true;
            // inititate shutdown and
            // require the native callback for Stop
            HostingEnvironment.InitiateShutdownWithoutDemand();
        }

        internal static void WaitForRequestsToDrain() {
            if (s_ApplicationContext == IntPtr.Zero) {
                // If InitializeApplication was never called, then no requests ever came in and StopProcessing will never be called.
                // We can just short-circuit this method.
                return;
            }

            while (!s_StopProcessingCalled || _inIndicateCompletionCount > 0) {
                Thread.Sleep(250);
            }
        }

        private StringBuilder FormatExceptionMessage(Exception e, string[] strings) {
            StringBuilder sb = new StringBuilder(4096);

            if (null != strings) {
                for (int i = 0; i < strings.Length; i++) {
                    sb.Append(strings[i]);
                }
            }
            for (Exception current = e; current != null; current = current.InnerException) {
                if (current == e)
                    sb.Append("\r\n\r\nException: ");
                else
                    sb.Append("\r\n\r\nInnerException: ");
                sb.Append(current.GetType().FullName);
                sb.Append("\r\nMessage: ");
                sb.Append(current.Message);
                sb.Append("\r\nStackTrace: ");
                sb.Append(current.StackTrace);
            }

            return sb;
        }

        public void InitializeApplication(IntPtr appContext)
        {
            s_ApplicationContext = appContext;

            // DevDiv #381425 - webengine4!RegisterModule runs *after* HostingEnvironment.Initialize (and thus the
            // HttpRuntime static ctor) when application preload is active. This means that any global state set
            // by RegisterModule (like the IIS version information, whether we're in integrated mode, misc server
            // info, etc.) will be unavailable to PreAppStart / preload code when the preload feature is active.
            // But since RegisterModule runs before InitializeApplication, we have one last chance here to collect
            // the information before the main part of the application starts, and the pipeline can depend on it
            // to be accurate.
            HttpRuntime.PopulateIISVersionInformation();

            HttpApplication app = null;

            try {
                // if HttpRuntime.HostingInit failed, do not attempt to create the application (WOS #1653963)
                if (!HttpRuntime.HostingInitFailed) {
                    //
                    //  On IIS7, application initialization does not provide an http context.  Theoretically,
                    //  no one should be using the context during application initialization, but people do.
                    //  Create a dummy context that is used during application initialization
                    //  to prevent breakage (ISAPI mode always provides a context)
                    //
                    HttpWorkerRequest initWorkerRequest = new SimpleWorkerRequest("" /*page*/,
                                                                                  "" /*query*/,
                                                                                  new StringWriter(CultureInfo.InvariantCulture));
                    MimeMapping.SetIntegratedApplicationContext(appContext);
                    HttpContext initHttpContext = new HttpContext(initWorkerRequest);
                    app = HttpApplicationFactory.GetPipelineApplicationInstance(appContext, initHttpContext);
                }
            }
            catch(Exception e)
            {
                if (HttpRuntime.InitializationException == null) {
                    HttpRuntime.InitializationException = e;
                }
            }
            finally {
                s_InitializationCompleted = true;

                if (HttpRuntime.InitializationException != null) {

                    // at least one module must be registered so that we
                    // call ProcessRequestNotification later and send the formatted
                    // InitializationException to the client.
                    int hresult = UnsafeIISMethods.MgdRegisterEventSubscription(
                        appContext,
                        InitExceptionModuleName,
                        RequestNotification.BeginRequest,
                        0 /*postRequestNotifications*/,
                        InitExceptionModuleName,
                        s_InitExceptionModulePrecondition,
                        new IntPtr(-1),
                        false /*useHighPriority*/);

                    if (hresult < 0) {
                        throw new COMException( SR.GetString(SR.Failed_Pipeline_Subscription, InitExceptionModuleName),
                                                hresult );
                    }

                    // Always register a managed handler:
                    // WOS 1990290: VS F5 Debugging: "AspNetInitializationExceptionModule" is registered for RQ_BEGIN_REQUEST, 
                    // but the DEBUG verb skips notifications until post RQ_AUTHENTICATE_REQUEST.  
                    hresult = UnsafeIISMethods.MgdRegisterEventSubscription(
                        appContext,
                        HttpApplication.IMPLICIT_HANDLER,
                        RequestNotification.ExecuteRequestHandler /*requestNotifications*/,
                        0 /*postRequestNotifications*/,
                        String.Empty /*type*/, 
                        HttpApplication.MANAGED_PRECONDITION /*precondition*/,
                        new IntPtr(-1),
                        false /*useHighPriority*/);

                    if (hresult < 0) {
                        throw new COMException( SR.GetString(SR.Failed_Pipeline_Subscription, HttpApplication.IMPLICIT_HANDLER),
                                                hresult );
                    }
                }

                if (app != null) {
                    HttpApplicationFactory.RecyclePipelineApplicationInstance(app);
                }
            }
        }

        private static HttpContext UnwrapContext(IntPtr rootedObjectsPointer) {
            RootedObjects objects = RootedObjects.FromPointer(rootedObjectsPointer);
            return objects.HttpContext;
        }

        internal bool HostingShutdownInitiated {
            get {
                return HostingEnvironment.ShutdownInitiated;
            }
        }

        // called from native code when the IHttpContext is disposed
        internal static void AsyncCompletionHandler(IntPtr rootedObjectsPointer, int bytesCompleted, int hresult, IntPtr pAsyncCompletionContext) {
            HttpContext context = UnwrapContext(rootedObjectsPointer);
            IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
            wr.OnAsyncCompletion(bytesCompleted, hresult, pAsyncCompletionContext);
        }

        // called from native code when the IHttpConnection is disconnected
        internal static void AsyncDisconnectNotificationHandler(IntPtr pManagedRootedObjects) {
            // Every object we're about to call into should be live / non-disposed,
            // but since we're paranoid we should put guard clauses everywhere.

            Debug.Assert(pManagedRootedObjects != IntPtr.Zero);
            if (pManagedRootedObjects != IntPtr.Zero) {
                RootedObjects rootObj = RootedObjects.FromPointer(pManagedRootedObjects);
                Debug.Assert(rootObj != null);
                if (rootObj != null) {
                    IIS7WorkerRequest workerRequest = rootObj.WorkerRequest;
                    Debug.Assert(workerRequest != null);
                    if (workerRequest != null) {
                        workerRequest.NotifyOfAsyncDisconnect();
                    }
                }
            }
        }

        // Called from native code to see if a principal is in a given role
        internal static int RoleHandler(IntPtr pRootedObjects, IntPtr pszRole, int cchRole, out bool isInRole) {
            isInRole = false;
            IPrincipal principal = RootedObjects.FromPointer(pRootedObjects).Principal;
            if (principal != null) {
                try {
                    isInRole = principal.IsInRole(StringUtil.StringFromWCharPtr(pszRole, cchRole));
                }
                catch (Exception e) {
                    return Marshal.GetHRForException(e);
                }
            }
            return HResults.S_OK;
        }

        // Called from native code to get the managed principal for a given request
        // If the return value is non-zero, the caller must free the returned GCHandle
        internal static IntPtr GetManagedPrincipalHandler(IntPtr pRootedObjects, int requestingAppDomainId) {
            // DevDiv 375079: Server.TransferRequest can be used to transfer requests to different applications,
            // which means that we might be trying to pass a GCHandle to the IPrincipal object to a different
            // AppDomain, which is disallowed. If this happens, we just tell our caller that we can't give him
            // a managed IPrincipal object.
            if (requestingAppDomainId != AppDomain.CurrentDomain.Id) {
                return IntPtr.Zero;
            }

            IPrincipal principal = RootedObjects.FromPointer(pRootedObjects).Principal;
            return GCUtil.RootObject(principal);
        }

        // called from native code when the IHttpContext is disposed
        internal static void DisposeHandler(IntPtr rootedObjectsPointer) {
            RootedObjects root = RootedObjects.FromPointer(rootedObjectsPointer);
            root.Destroy();
        }
        
        // called from managed code as a perf optimization to avoid calling back later
        internal static void DisposeHandler(HttpContext context, IntPtr nativeRequestContext, RequestNotificationStatus status) {
            if (IIS.MgdCanDisposeManagedContext(nativeRequestContext, status)) {
                context.RootedObjects.Destroy();
            }
        }

        //
        // This is the managed entry point for processing request notifications.
        // Although this method is wrapped in try/catch, it is not supposed to
        // cause an exception. If it does throw, the application, httpwriter, etc
        // may not be initialized, and it might not be possible to process the rest
        // of the request. I would prefer to let this method throw and crash the
        // process, but for now we will consume the exception, report the error to
        // IIS, and continue.
        //
        // Code that might throw belongs in HttpRuntime::ProcessRequestNotificationPrivate.
        //
        internal static int ProcessRequestNotification(
                IntPtr rootedObjectsPointer,
                IntPtr nativeRequestContext,
                IntPtr moduleData,
                int flags)
        {
            try {
                return ProcessRequestNotificationHelper(rootedObjectsPointer, nativeRequestContext, moduleData, flags);
            }
            catch(Exception e) {
                ApplicationManager.RecordFatalException(e);
                throw;
            }
        }

        internal static int ProcessRequestNotificationHelper(
                IntPtr rootedObjectsPointer,
                IntPtr nativeRequestContext,
                IntPtr moduleData,
                int flags)
        {
            IIS7WorkerRequest wr = null;
            HttpContext context = null;
            RequestNotificationStatus status = RequestNotificationStatus.Continue;
            RootedObjects root;
            bool workerRequestWasJustCreated = false;

            if (rootedObjectsPointer == IntPtr.Zero) {
                InitializeRequestContext(nativeRequestContext, flags, out wr, out context);
                workerRequestWasJustCreated = true;
                if (context == null) {
                    return (int)RequestNotificationStatus.FinishRequest;
                }

                root = RootedObjects.Create();
                root.HttpContext = context;
                root.WorkerRequest = wr;
                root.WriteTransferEventIfNecessary();
                context.RootedObjects = root;

                IIS.MgdSetManagedHttpContext(nativeRequestContext, root.Pointer);
            }
            else {
                root = RootedObjects.FromPointer(rootedObjectsPointer);
                context = root.HttpContext;
                wr = root.WorkerRequest as IIS7WorkerRequest;
            }

            Debug.Assert(root != null, "We should have a RootedObjects instance by this point.");
            Debug.Assert(wr != null, "We should have an IIS7WorkerRequest instance by this point.");

            using (root.WithinTraceBlock()) {
                if (workerRequestWasJustCreated) {
                    AspNetEventSource.Instance.RequestStarted(wr);
                }

                int currentModuleIndex;
                bool isPostNotification;
                int currentNotification;
                IIS.MgdGetCurrentNotificationInfo(nativeRequestContext, out currentModuleIndex, out isPostNotification, out currentNotification);

                // If the HttpContext is null at this point, then we've already transitioned this request to a WebSockets request.
                // The WebSockets module should already be running, and asynchronous module-level events (like SendResponse) are
                // ineligible to be hooked by managed code.
                if (context == null || context.HasWebSocketRequestTransitionStarted) {
                    return (int)RequestNotificationStatus.Continue;
                }

                // It is possible for a notification to complete asynchronously while we're in
                // a call to IndicateCompletion, in which case a new IIS thread might enter before 
                // the call to IndicateCompletion returns.  If this happens, block the thread until
                // IndicateCompletion returns.  But never block a SendResponse notification, because
                // that can cause the request to hang (DevDiv Bugs 187441).
                if (context.InIndicateCompletion
                    && context.ThreadInsideIndicateCompletion != Thread.CurrentThread 
                    && RequestNotification.SendResponse != (RequestNotification)currentNotification) {
                    while (context.InIndicateCompletion) {
                        Thread.Sleep(10);
                    }
                }
            
                // RQ_SEND_RESPONSE fires out of band and completes synchronously only.
                // The pipeline must be reentrant to support this, so the notification 
                // context for the previous notification must be saved and restored.
                NotificationContext savedNotificationContext = context.NotificationContext;
                bool cancellable = context.IsInCancellablePeriod;
                bool locked = false;
                try {
                    if (cancellable) {
                        context.EndCancellablePeriod();
                    }
                    bool isReEntry = (savedNotificationContext != null);
                    if (isReEntry) {
                        context.ApplicationInstance.AcquireNotifcationContextLock(ref locked);
                    }
                    context.NotificationContext = new NotificationContext(flags /*CurrentNotificationFlags*/, 
                                                                          isReEntry);

                    Action<RequestNotificationStatus> verifierCheck = null;
                    if (AppVerifier.IsAppVerifierEnabled) {
                        verifierCheck = AppVerifier.GetRequestNotificationStatusCheckDelegate(context, (RequestNotification)currentNotification, isPostNotification);
                    }

                    status = HttpRuntime.ProcessRequestNotification(wr, context);

                    if (verifierCheck != null) {
                        AppVerifier.InvokeVerifierCheck(verifierCheck, status);
                    }
                }
                finally {
                    if (status != RequestNotificationStatus.Pending) {
                        // if we completed the notification, pop the notification context stack
                        // if this is an asynchronous unwind, then the completion will clear the context
                        context.NotificationContext = savedNotificationContext;

                        // DevDiv 112755 restore cancellable state if its changed
                        if (cancellable && !context.IsInCancellablePeriod) {
                            context.BeginCancellablePeriod();
                        } else if (!cancellable && context.IsInCancellablePeriod) {
                            context.EndCancellablePeriod();
                        }
                    }
                    if (locked) {
                        context.ApplicationInstance.ReleaseNotifcationContextLock();
                    }
                }

                if (status != RequestNotificationStatus.Pending) {
                    // The current notification may have changed due to the HttpApplication progressing the IIS state machine, so retrieve the info again.
                    IIS.MgdGetCurrentNotificationInfo(nativeRequestContext, out currentModuleIndex, out isPostNotification, out currentNotification);

                    // WOS 1785741: (Perf) In profiles, 8% of HelloWorld is transitioning from native to managed.
                    // The fix is to keep managed code on the stack so that the AppDomain context remains on the
                    // thread, and we can re-enter managed code without setting up the AppDomain context.
                    // If this optimization is possible, MgdIndicateCompletion will execute one or more notifications
                    // and return PENDING as the status.
                    ThreadContext threadContext = context.IndicateCompletionContext;
                    // DevDiv 482614:
                    // Don't use local copy to detect if we can call MgdIndicateCompletion because another thread 
                    // unwinding from MgdIndicateCompletion may be changing context.IndicateCompletionContext at the same time.
                    if (!context.InIndicateCompletion && context.IndicateCompletionContext != null) {
                        if (status == RequestNotificationStatus.Continue) {
                            try {
                                context.InIndicateCompletion = true;
                                Interlocked.Increment(ref _inIndicateCompletionCount);
                                context.ThreadInsideIndicateCompletion = Thread.CurrentThread;
                                IIS.MgdIndicateCompletion(nativeRequestContext, ref status);
                            }
                            finally {
                                context.ThreadInsideIndicateCompletion = null;
                                Interlocked.Decrement(ref _inIndicateCompletionCount);

                                // Leave will have been called already if the last notification is returning pending
                                // DTS267762: Make sure InIndicateCompletion is released, not based on the thread context state
                                // Otherwise the next request notification may deadlock
                                if (!threadContext.HasBeenDisassociatedFromThread || context.InIndicateCompletion) {
                                    lock (threadContext) {
                                        if (!threadContext.HasBeenDisassociatedFromThread) {
                                            threadContext.DisassociateFromCurrentThread();
                                        }

                                        context.IndicateCompletionContext = null;
                                        context.InIndicateCompletion = false;
                                    }
                                }
                            }
                        }
                        else {
                            if (!threadContext.HasBeenDisassociatedFromThread || context.InIndicateCompletion) {
                                lock (threadContext) {
                                    if (!threadContext.HasBeenDisassociatedFromThread) {
                                        threadContext.DisassociateFromCurrentThread();
                                    }

                                    context.IndicateCompletionContext = null;
                                    context.InIndicateCompletion = false;
                                }
                            }
                        }
                    }
                }

                if (context.HasWebSocketRequestTransitionStarted && status == RequestNotificationStatus.Pending) {
                    // At this point, the WebSocket module event (PostEndRequest) has executed and set up the appropriate contexts for us.
                    // However, there is a race condition that we need to avoid. It is possible that one thread has kicked off some async
                    // work, e.g. via an IHttpAsyncHandler, and that thread is unwinding and has reached this line of execution.
                    // Meanwhile, the IHttpAsyncHandler completed quickly (but asynchronously) and invoked MgdPostCompletion, which
                    // resulted in a new thread calling ProcessRequestNotification. If this second thread starts the WebSocket transition,
                    // then there's the risk that *both* threads might attempt to call WebSocketPipeline.ProcessRequest, which could AV
                    // the process.
                    //
                    // We protect against this by allowing only the thread which started the transition to complete the transition, so in
                    // the above scenario the original thread (which invoked the IHttpAsyncHandler) no-ops at this point and just returns
                    // Pending to its caller.

                    if (context.DidCurrentThreadStartWebSocketTransition) {
                        // We'll mark the HttpContext as complete, call the continuation to kick off the socket send / receive loop, and return
                        // Pending to IIS so that it doesn't advance the state machine until the WebSocket loop completes.
                        root.ReleaseHttpContext();
                        root.WebSocketPipeline.ProcessRequest();
                    }
                }

                return (int)status;
            }
        }

        private static void InitializeRequestContext(IntPtr nativeRequestContext, int flags, out IIS7WorkerRequest wr, out HttpContext context) {
            wr = null;
            context = null;
            try {
                bool etwEnabled = ((flags & HttpContext.FLAG_ETW_PROVIDER_ENABLED) == HttpContext.FLAG_ETW_PROVIDER_ENABLED);

                // this may throw, e.g. if the request Content-Length header has a value greater than Int32.MaxValue
                wr = IIS7WorkerRequest.CreateWorkerRequest(nativeRequestContext, etwEnabled);

                // this may throw, e.g. see WOS 1724573: ASP.Net v2.0: wrong error code returned when ? is used in the URL
                context = new HttpContext(wr, false);
            }
            catch {
                // treat as "400 Bad Request" since that's the only reason the HttpContext.ctor should throw
                IIS.MgdSetBadRequestStatus(nativeRequestContext);
            }
        }

        /// <include file='doc\ISAPIRuntime.uex' path='docs/doc[@for="ISAPIRuntime.IRegisteredObject.Stop"]/*' />
        /// <internalonly/>
        void IRegisteredObject.Stop(bool immediate) {
            Debug.Trace("PipelineDomain", "IRegisteredObject.Stop appId = " +
                        s_thisAppDomainsIsapiAppId);

            while (!s_InitializationCompleted && !s_StopProcessingCalled) {
                // the native W3_MGD_APP_CONTEXT is not ready for us to unload
                Thread.Sleep(250);
            }

            RemoveThisAppDomainFromUnmanagedTable();
            HostingEnvironment.UnregisterObject(this);
        }

        internal void SetThisAppDomainsIsapiAppId(String appId) {
            Debug.Trace("PipelineDomain", "SetThisAppDomainsPipelineAppId appId=" + appId);
            s_thisAppDomainsIsapiAppId = appId;
        }

        internal static void RemoveThisAppDomainFromUnmanagedTable() {
            if (Interlocked.Exchange(ref s_isThisAppDomainRemovedFromUnmanagedTable, 1) != 0) {
                return;
            }

            //
            // only notify mgdeng of this shutdown if we went through
            // Initialize from the there
            // We can also have PipelineRuntime in app domains with only
            // other protocols
            //
            try {
                if (s_thisAppDomainsIsapiAppId != null  && s_ApplicationContext != IntPtr.Zero) {
                    Debug.Trace("PipelineDomain", "Calling MgdAppDomainShutdown appId=" +
                        s_thisAppDomainsIsapiAppId + " (AppDomainAppId=" + HttpRuntime.AppDomainAppId + ")");

                    UnsafeIISMethods.MgdAppDomainShutdown(s_ApplicationContext);
                }

                HttpRuntime.AddAppDomainTraceMessage(SR.GetString(SR.App_Domain_Restart));
            }
            catch(Exception e) {
                if (ShouldRethrowException(e)) {
                    throw;
                }
            }
        }

        internal static bool ShouldRethrowException(Exception ex) {
            return     ex is NullReferenceException
                    || ex is AccessViolationException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException;
        }

    }
}
