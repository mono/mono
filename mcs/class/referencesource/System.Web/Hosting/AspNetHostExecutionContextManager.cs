namespace System.Web.Hosting {
    using System;
    using System.Security.Permissions;
    using System.Threading;

    // This HostExecutionContextManager can provide both setup and cleanup logic that
    // is invoked during a call to ExecutionContext.Run. This may be necessary when
    // using the Task-based APIs, as the 'await' language feature generally causes
    // this stack to be generated:
    //
    // { state machine callback }
    // ExecutionContext.Run
    // Task.SomeInternalCallback
    // AspNetSynchronizationContext.PostCallbackLogic
    //
    // The callback logic invoked by our AspNetSynchronizationContext.Post method puts
    // HttpContext-related information in the current ExecutionContext, but the
    // subsequent call to ExecutionContext.Run overwrites that information. So we have
    // logic in AspNetHostExecutionContextManager that can detect if a ThreadContext is
    // associated with the current thread (it will have been set by the Post callback),
    // and if so it should restore HttpContext.Current and other ExecutionContext-related
    // items on the current thread.

    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    internal sealed class AspNetHostExecutionContextManager : HostExecutionContextManager {

        // Used as the return value from SetHostExecutionContext when our logic is active.
        // We use RevertAction instead of Action since it's unambiguous in the event that
        // base.SetHostExecutionContext itself ever changes to return Action.
        private delegate void RevertAction();

        public override HostExecutionContext Capture() {
            ThreadContext currentContext = ThreadContext.Current;
            if (currentContext != null) {
                // We need to capture a reference to the current HttpContext's ThreadContextId
                // so that we can properly restore this instance on the call to SetHostExecutionContext.
                // See comment on HttpContext.ThreadContextId for more information.
                return new AspNetHostExecutionContext(
                    baseContext: base.Capture(),
                    httpContextThreadContextId: currentContext.HttpContext.ThreadContextId);
            }
            else {
                // There is no ThreadContext associated with this thread, hence there is no special
                // setup we need to do to restore things like HttpContext.Current. We can just
                // delegate to the base implementation.
                return base.Capture();
            }
        }

        public override void Revert(object previousState) {
            RevertAction revertAction = previousState as RevertAction;
            if (revertAction != null) {
                // Our revert logic should run. It will eventually call base.Revert.
                revertAction();
            }
            else {
                // We have no revert logic, so just call the base implementation.
                base.Revert(previousState);
            }
        }

        public override object SetHostExecutionContext(HostExecutionContext hostExecutionContext) {
            AspNetHostExecutionContext castHostExecutionContext = hostExecutionContext as AspNetHostExecutionContext;
            if (castHostExecutionContext != null) {
                // Call base.SetHostExecutionContext before calling our own logic.
                object baseRevertParameter = null;
                if (castHostExecutionContext.BaseContext != null) {
                    baseRevertParameter = base.SetHostExecutionContext(castHostExecutionContext.BaseContext);
                }

                ThreadContext currentContext = ThreadContext.Current;
                if (currentContext != null && currentContext.HttpContext.ThreadContextId == castHostExecutionContext.HttpContextThreadContextId) {
                    // If we reached this point, then 'castHostExecutionContext' was captured for the HttpContext
                    // that is associated with the ThreadContext that is assigned to the current thread. We can
                    // safely restore it.
                    Action threadContextCleanupAction = currentContext.EnterExecutionContext();

                    // Perform cleanup in the opposite order from initialization.
                    return (RevertAction)(() => {
                        threadContextCleanupAction();
                        if (baseRevertParameter != null) {
                            base.Revert(baseRevertParameter);
                        }
                    });
                }
                else {
                    // If we reached this point, then 'castHostExecutionContext' was captured by us
                    // but is not applicable to the current thread. This can happen if the developer
                    // called ThreadPool.QueueUserWorkItem, for example. We don't restore HttpContext
                    // on such threads since they're not under the control of ASP.NET. In this case,
                    // we have already called base.SetHostExecutionContext, so we just need to return
                    // the result of that function directly to our caller.
                    return baseRevertParameter;
                }
            }
            else {
                // If we reached this point, then 'hostExecutionContext' was generated by our
                // base class instead of by us, so just delegate to the base implementation.
                return base.SetHostExecutionContext(hostExecutionContext);
            }
        }

        private sealed class AspNetHostExecutionContext : HostExecutionContext {
            public readonly HostExecutionContext BaseContext;
            public readonly object HttpContextThreadContextId;

            internal AspNetHostExecutionContext(HostExecutionContext baseContext, object httpContextThreadContextId) {
                BaseContext = baseContext;
                HttpContextThreadContextId = httpContextThreadContextId;
            }

            // copy ctor
            private AspNetHostExecutionContext(AspNetHostExecutionContext original)
                : this(CreateCopyHelper(original.BaseContext), original.HttpContextThreadContextId) {
            }

            public override HostExecutionContext CreateCopy() {
                return new AspNetHostExecutionContext(this);
            }

            private static HostExecutionContext CreateCopyHelper(HostExecutionContext hostExecutionContext) {
                // creating a copy of a null context should just itself return null
                return (hostExecutionContext != null) ? hostExecutionContext.CreateCopy() : null;
            }

            public override void Dispose(bool disposing) {
                if (disposing && BaseContext != null) {
                    BaseContext.Dispose();
                }
            }
        }

    }
}
