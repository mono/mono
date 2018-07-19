//------------------------------------------------------------------------------
// <copyright file="PageAsyncTask.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Util;

    // Public class to serve as an interface between ASP.NET's synchronization systems and the user code to be executed asynchronously

    public sealed class PageAsyncTask {

        // APM
        public PageAsyncTask(BeginEventHandler beginHandler, EndEventHandler endHandler, EndEventHandler timeoutHandler, Object state)
            : this(beginHandler, endHandler, timeoutHandler, state, executeInParallel: false) {
        }

        // APM
        public PageAsyncTask(BeginEventHandler beginHandler, EndEventHandler endHandler, EndEventHandler timeoutHandler, Object state, bool executeInParallel)
            : this(beginHandler, endHandler, timeoutHandler, state, executeInParallel, currentMode: SynchronizationContextUtil.CurrentMode) {
        }

        // APM
        internal PageAsyncTask(BeginEventHandler beginHandler, EndEventHandler endHandler, EndEventHandler timeoutHandler, Object state, bool executeInParallel, SynchronizationContextMode currentMode) {
            if (beginHandler == null) {
                throw new ArgumentNullException("beginHandler");
            }
            if (endHandler == null) {
                throw new ArgumentNullException("endHandler");
            }

            // Only the legacy PageAsyncTaskManager supports timing out APM methods or executing them in parallel
            if (timeoutHandler != null || executeInParallel) {
                SynchronizationContextUtil.ValidateMode(currentMode, requiredMode: SynchronizationContextMode.Legacy, specificErrorMessage: SR.SynchronizationContextUtil_PageAsyncTaskTimeoutHandlerParallelNotCompatible);
            }

            BeginHandler = beginHandler;
            EndHandler = endHandler;
            TimeoutHandler = timeoutHandler;
            State = state;
            ExecuteInParallel = executeInParallel;
        }

        // TAP
        public PageAsyncTask(Func<Task> handler)
            : this(WrapParameterlessTaskHandler(handler)) {
        }

        public PageAsyncTask(Func<CancellationToken, Task> handler)
            : this(handler, currentMode: SynchronizationContextUtil.CurrentMode) {
        }

        // TAP
        internal PageAsyncTask(Func<CancellationToken, Task> handler, SynchronizationContextMode currentMode) {
            if (handler == null) {
                throw new ArgumentNullException("handler");
            }

            // The legacy PageAsyncTaskManager doesn't support TAP methods
            SynchronizationContextUtil.ValidateMode(currentMode, requiredMode: SynchronizationContextMode.Normal, specificErrorMessage: SR.SynchronizationContextUtil_TaskReturningPageAsyncMethodsNotCompatible);

            TaskHandler = handler;
        }

        #region Legacy properties
        // These properties were present in the original PageAsyncTask type, so they must remain in this new type, even if they're unused.

        public BeginEventHandler BeginHandler { get; private set; }
        public EndEventHandler EndHandler { get; private set; }
        public bool ExecuteInParallel { get; private set; }
        public object State { get; private set; }
        public EndEventHandler TimeoutHandler { get; private set; }
        #endregion

        // For TAP
        internal Func<CancellationToken, Task> TaskHandler { get; private set; }

        private static Func<CancellationToken, Task> WrapParameterlessTaskHandler(Func<Task> handler) {
            return (handler != null)
                ? (Func<CancellationToken, Task>)(_ => handler())
                : null;
        }

    }
}
