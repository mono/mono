namespace System.Web {
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    // Contains information about any modifications ASP.NET has made to the current
    // thread and how to undo them. See also the comments on
    // HttpApplication.OnThreadEnterPrivate.

    internal sealed class ThreadContext : ISyncContextLock {

        // This is a marker holding the current ThreadContext for the current
        // thread. Uses TLS so that it's not wiped away by ExecutionContext.Run.
        [ThreadStatic]
        private static ThreadContext _currentThreadContext;

        private ImpersonationContext _newImpersonationContext;
        private HttpContext _originalHttpContext;
        private SynchronizationContext _originalSynchronizationContext;
        private ThreadContext _originalThreadContextCurrent;
        private CultureInfo _originalThreadCurrentCulture;
        private CultureInfo _originalThreadCurrentUICulture;
        private IPrincipal _originalThreadPrincipal;
        private bool _setCurrentThreadOnHttpContext;

        internal ThreadContext(HttpContext httpContext) {
            HttpContext = httpContext;
        }

        internal static ThreadContext Current {
            get { return _currentThreadContext; }
            private set { _currentThreadContext = value; }
        }

        internal bool HasBeenDisassociatedFromThread {
            get;
            private set;
        }

        internal HttpContext HttpContext {
            get;
            private set;
        }

        // Associates this ThreadContext with the current thread. This restores certain
        // ambient values associated with the current HttpContext, such as the current
        // user and cultures. It also sets HttpContext.Current.
        internal void AssociateWithCurrentThread(bool setImpersonationContext) {
            Debug.Assert(HttpContext != null); // only to be used when context is available
            Debug.Assert(Current != this, "This ThreadContext is already associated with this thread.");
            Debug.Assert(!HasBeenDisassociatedFromThread, "This ThreadContext has already been disassociated from a thread.");

            Debug.Trace("OnThread", GetTraceMessage("Enter1"));

            /*
             * !! IMPORTANT !!
             * Keep this logic in [....] with DisassociateFromCurrentThread and EnterExecutionContext.
             */

            // attach http context to the call context
            _originalHttpContext = DisposableHttpContextWrapper.SwitchContext(HttpContext);

            // set impersonation on the current thread
            if (setImpersonationContext) {
                SetImpersonationContext();
            }

            // set synchronization context for the current thread to support the async pattern
            _originalSynchronizationContext = AsyncOperationManager.SynchronizationContext;
            AspNetSynchronizationContextBase aspNetSynchronizationContext = HttpContext.SyncContext;
            AsyncOperationManager.SynchronizationContext = aspNetSynchronizationContext;

            // set ETW trace ID
            Guid g = HttpContext.WorkerRequest.RequestTraceIdentifier;
            if (g != Guid.Empty) {
                CallContext.LogicalSetData("E2ETrace.ActivityID", g);
            }

            // set SqlDependecyCookie
            HttpContext.ResetSqlDependencyCookie();

            // set principal on the current thread
            _originalThreadPrincipal = Thread.CurrentPrincipal;
            HttpApplication.SetCurrentPrincipalWithAssert(HttpContext.User);

            // only set culture on the current thread if it is not initialized
            SetRequestLevelCulture();

            // DevDivBugs 75042
            // set current thread in context if there is not there
            // the timeout manager  uses this to abort the correct thread
            if (HttpContext.CurrentThread == null) {
                _setCurrentThreadOnHttpContext = true;
                HttpContext.CurrentThread = Thread.CurrentThread;
            }

            // Store a reference to the original ThreadContext.Current. It is possible that a parent
            // ThreadContext might already be associated with the current thread, e.g. if the current
            // stack contains a call to MgdIndicateCompletion (via
            // PipelineRuntime.ProcessRequestNotificationHelper). If this is the case, the child
            // ThreadContext will temporarily take over.
            _originalThreadContextCurrent = Current;
            Current = this;

            Debug.Trace("OnThread", GetTraceMessage("Enter2"));
        }

        private ClientImpersonationContext CreateNewClientImpersonationContext() {
            // impersonation is set in the ClientImpersonationContext ctor
            return new ClientImpersonationContext(HttpContext);
        }

        // Disassociates this ThreadContext from the current thread. Any ambient values (e.g., culture)
        // associated with the current request are stored in the HttpContext object so that they
        // can be restored the next time a ThreadContext associated with this HttpContext is active.
        // Impersonation and other similar modifications to the current thread are undone.
        internal void DisassociateFromCurrentThread() {
            Debug.Trace("OnThread", GetTraceMessage("Leave1"));
            Debug.Assert(Current == this, "This ThreadContext isn't associated with current thread.");
            Debug.Assert(!HasBeenDisassociatedFromThread, "This ThreadContext has already been disassociated from a thread.");

            /*
             * !! IMPORTANT !!
             * Keep this logic in [....] with AssociateWithCurrentThread and EnterExecutionContext.
             */

            Current = _originalThreadContextCurrent;
            HasBeenDisassociatedFromThread = true;

            // remove thread if set
            if (_setCurrentThreadOnHttpContext) {
                HttpContext.CurrentThread = null;
            }

            // this thread should not be locking app state
            HttpApplicationFactory.ApplicationState.EnsureUnLock();

            // stop impersonation
            UndoImpersonationContext();

            // restore culture
            RestoreRequestLevelCulture();

            // restrore synchronization context
            AsyncOperationManager.SynchronizationContext = _originalSynchronizationContext;

            // restore thread principal
            HttpApplication.SetCurrentPrincipalWithAssert(_originalThreadPrincipal);

            // Remove SqlCacheDependency cookie from call context if necessary
            HttpContext.RemoveSqlDependencyCookie();

            // remove http context from the call context
            DisposableHttpContextWrapper.SwitchContext(_originalHttpContext);
            _originalHttpContext = null;

            Debug.Trace("OnThread", GetTraceMessage("Leave2"));
        }

        // Called by AspNetHostExecutionContextManager to signal that ExecutionContext.Run
        // is being called on a thread currently associated with our ThreadContext. Since
        // ExecutionContext.Run destroys some of our ambient state (HttpContext.Current, etc.),
        // we need to restore it. This method returns an Action which should be called when
        // the call to ExecutionContext.Run is concluding.
        internal Action EnterExecutionContext() {
            Debug.Trace("OnThread", GetTraceMessage("EnterExecutionContext1"));
            Debug.Assert(Current == this, "This ThreadContext isn't associated with current thread.");
            Debug.Assert(!HasBeenDisassociatedFromThread, "This ThreadContext has already been disassociated from a thread.");

            /*
             * !! IMPORTANT !!
             * Keep this logic in [....] with AssociateWithCurrentThread and DisassociateFromCurrentThread.
             */

            // ExecutionContext.Run replaces the current impersonation token, so we need to impersonate
            // if AssociateWithCurrentThread also did so.

            ClientImpersonationContext executionContextClientImpersonationContext = null;
            if (_newImpersonationContext != null) {
                executionContextClientImpersonationContext = CreateNewClientImpersonationContext();
            }

            // ExecutionContext.Run resets the LogicalCallContext / IllogicalCallContext (which contains HttpContext.Current),
            // so we need to restore both of them.

            DisposableHttpContextWrapper.SwitchContext(HttpContext);

            Guid g = HttpContext.WorkerRequest.RequestTraceIdentifier;
            if (g != Guid.Empty) {
                CallContext.LogicalSetData("E2ETrace.ActivityID", g);
            }

            HttpContext.ResetSqlDependencyCookie();

            // ExecutionContext.Run resets the thread's CurrentPrincipal, so we need to restore it.

            HttpApplication.SetCurrentPrincipalWithAssert(HttpContext.User);

            // Other items like [ThreadStatic] fields, culture, etc. are untouched by ExecutionContext.Run,
            // so we don't need to worry about them.

            Debug.Trace("OnThread", GetTraceMessage("EnterExecutionContext2"));

            // This delegate is the cleanup routine.
            return () => {
                Debug.Trace("OnThread", GetTraceMessage("LeaveExecutionContext1"));

                // Undo any impersonation that we performed.
                if (executionContextClientImpersonationContext != null) {
                    executionContextClientImpersonationContext.Undo();
                }

                // Other things, e.g. changes to the logical/illogical call contexts, changes
                // to CurrentPrincipal, etc., will automatically be reverted anyway when
                // the call to ExecutionContext.Run concludes, so we don't need to clean up
                // here.

                Debug.Trace("OnThread", GetTraceMessage("LeaveExecutionContext2"));
            };
        }

        private static string GetTraceMessage(string tag) {
#if DBG
            StringBuilder sb = new StringBuilder(256);
            sb.Append(tag);
            sb.AppendFormat(" Thread={0}", SafeNativeMethods.GetCurrentThreadId().ToString(CultureInfo.InvariantCulture));
            sb.AppendFormat(" Context={0}", (HttpContext.Current != null) ? HttpContext.Current.GetHashCode().ToString(CultureInfo.InvariantCulture) : "NULL_CTX");
            sb.AppendFormat(" Principal={0}", (Thread.CurrentPrincipal != null) ? Thread.CurrentPrincipal.GetHashCode().ToString(CultureInfo.InvariantCulture) : "NULL_PRIN");
            sb.AppendFormat(" Culture={0}", Thread.CurrentThread.CurrentCulture.LCID.ToString(CultureInfo.InvariantCulture));
            sb.AppendFormat(" UICulture={0}", Thread.CurrentThread.CurrentUICulture.LCID.ToString(CultureInfo.InvariantCulture));
            sb.AppendFormat(" ActivityID={0}", CallContext.LogicalGetData("E2ETrace.ActivityID"));
            return sb.ToString();
#else
            // This method should never be called in release mode.
            throw new NotImplementedException();
#endif
        }


        // Restores the thread's CurrentCulture and CurrentUICulture back to what
        // they were before this ThreadContext was associated with the thread. If
        // any culture has changed from its original value, we squirrel the new
        // culture away in HttpContext so that we can restore it the next time any
        // ThreadContext associated with this HttpContext is active.
        private void RestoreRequestLevelCulture() {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

            if (_originalThreadCurrentCulture != null) {
                // Avoid the cost of the Demand when setting the culture by comparing the cultures first
                if (currentCulture != _originalThreadCurrentCulture) {
                    HttpRuntime.SetCurrentThreadCultureWithAssert(_originalThreadCurrentCulture);
                    if (HttpContext != null) {
                        // remember changed culture for the rest of the request
                        HttpContext.DynamicCulture = currentCulture;
                    }
                }

                _originalThreadCurrentCulture = null;
            }

            if (_originalThreadCurrentUICulture != null) {
                // Avoid the cost of the Demand when setting the culture by comparing the cultures first
                if (currentUICulture != _originalThreadCurrentUICulture) {
                    Thread.CurrentThread.CurrentUICulture = _originalThreadCurrentUICulture;
                    if (HttpContext != null) {
                        // remember changed culture for the rest of the request
                        HttpContext.DynamicUICulture = currentUICulture;
                    }
                }

                _originalThreadCurrentUICulture = null;
            }
        }

        // Sets impersonation on the current thread.
        internal void SetImpersonationContext() {
            if (_newImpersonationContext == null) {
                _newImpersonationContext = CreateNewClientImpersonationContext();
            }
        }

        // Sets the thread's CurrentCulture and CurrentUICulture to those associated
        // with the current HttpContext. We do this since the culture of a request can
        // change over its lifetime and isn't necessarily the default for the AppDomain,
        // e.g. if the culture was read from the request headers.
        private void SetRequestLevelCulture() {
            CultureInfo culture = null;
            CultureInfo uiculture = null;

            GlobalizationSection globConfig = RuntimeConfig.GetConfig(HttpContext).Globalization;
            if (!String.IsNullOrEmpty(globConfig.Culture))
                culture = HttpContext.CultureFromConfig(globConfig.Culture, true);

            if (!String.IsNullOrEmpty(globConfig.UICulture))
                uiculture = HttpContext.CultureFromConfig(globConfig.UICulture, false);

            if (HttpContext.DynamicCulture != null)
                culture = HttpContext.DynamicCulture;

            if (HttpContext.DynamicUICulture != null)
                uiculture = HttpContext.DynamicUICulture;

            // Page also could have its own culture settings
            Page page = HttpContext.CurrentHandler as Page;

            if (page != null) {
                if (page.DynamicCulture != null)
                    culture = page.DynamicCulture;

                if (page.DynamicUICulture != null)
                    uiculture = page.DynamicUICulture;
            }

            _originalThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            _originalThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            if (culture != null && culture != Thread.CurrentThread.CurrentCulture) {
                HttpRuntime.SetCurrentThreadCultureWithAssert(culture);
            }

            if (uiculture != null && uiculture != Thread.CurrentThread.CurrentUICulture) {
                Thread.CurrentThread.CurrentUICulture = uiculture;
            }
        }

        // Use of IndicateCompletion requires that we synchronize the cultures
        // with what may have been set by user code during execution of the
        // notification.
        internal void Synchronize() {
            HttpContext.DynamicCulture = Thread.CurrentThread.CurrentCulture;
            HttpContext.DynamicUICulture = Thread.CurrentThread.CurrentUICulture;
        }

        // Undoes any impersonation that we did when associating this ThreadContext
        // with the current thread.
        internal void UndoImpersonationContext() {
            // remove impersonation on the current thread
            if (_newImpersonationContext != null) {
                _newImpersonationContext.Undo();
                _newImpersonationContext = null;
            }
        }

        // Called by AspNetSynchronizationContext to signal that it is finished
        // processing on the current thread.
        void ISyncContextLock.Leave() {
            DisassociateFromCurrentThread();
        }

    }
}
