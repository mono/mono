/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Web.Mvc.Async;
    using System.Web.Routing;

    public abstract class AsyncController : Controller, IAsyncManagerContainer, IAsyncController {

        private static readonly object _executeTag = new object();
        private static readonly object _executeCoreTag = new object();

        private readonly AsyncManager _asyncManager = new AsyncManager();

        public AsyncManager AsyncManager {
            get {
                return _asyncManager;
            }
        }

        protected virtual IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state) {
            if (requestContext == null) {
                throw new ArgumentNullException("requestContext");
            }

            VerifyExecuteCalledOnce();
            Initialize(requestContext);
            return AsyncResultWrapper.Begin(callback, state, BeginExecuteCore, EndExecuteCore, _executeTag);
        }

        protected virtual IAsyncResult BeginExecuteCore(AsyncCallback callback, object state) {
            // If code in this method needs to be updated, please also check the ExecuteCore() method
            // of Controller to see if that code also must be updated.

            PossiblyLoadTempData();
            try {
                string actionName = RouteData.GetRequiredString("action");
                IActionInvoker invoker = ActionInvoker;
                IAsyncActionInvoker asyncInvoker = invoker as IAsyncActionInvoker;
                if (asyncInvoker != null) {
                    // asynchronous invocation
                    BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                        return asyncInvoker.BeginInvokeAction(ControllerContext, actionName, asyncCallback, asyncState);
                    };

                    EndInvokeDelegate endDelegate = delegate(IAsyncResult asyncResult) {
                        if (!asyncInvoker.EndInvokeAction(asyncResult)) {
                            HandleUnknownAction(actionName);
                        }
                    };

                    return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _executeCoreTag);
                }
                else {
                    // synchronous invocation
                    Action action = () => {
                        if (!invoker.InvokeAction(ControllerContext, actionName)) {
                            HandleUnknownAction(actionName);
                        }
                    };
                    return AsyncResultWrapper.BeginSynchronous(callback, state, action, _executeCoreTag);
                }
            }
            catch {
                PossiblySaveTempData();
                throw;
            }
        }

        protected override IActionInvoker CreateActionInvoker() {
            return new AsyncControllerActionInvoker();
        }

        protected virtual void EndExecute(IAsyncResult asyncResult) {
            AsyncResultWrapper.End(asyncResult, _executeTag);
        }

        protected virtual void EndExecuteCore(IAsyncResult asyncResult) {
            // If code in this method needs to be updated, please also check the ExecuteCore() method
            // of Controller to see if that code also must be updated.

            try {
                AsyncResultWrapper.End(asyncResult, _executeCoreTag);
            }
            finally {
                PossiblySaveTempData();
            }
        }

        #region IAsyncController Members
        IAsyncResult IAsyncController.BeginExecute(RequestContext requestContext, AsyncCallback callback, object state) {
            return BeginExecute(requestContext, callback, state);
        }

        void IAsyncController.EndExecute(IAsyncResult asyncResult) {
            EndExecute(asyncResult);
        }
        #endregion

    }
}
