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

namespace System.Web.Mvc.Async {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class AsyncControllerActionInvoker : ControllerActionInvoker, IAsyncActionInvoker {

        private static readonly object _invokeActionTag = new object();
        private static readonly object _invokeActionMethodTag = new object();
        private static readonly object _invokeActionMethodWithFiltersTag = new object();

        public virtual IAsyncResult BeginInvokeAction(ControllerContext controllerContext, string actionName, AsyncCallback callback, object state) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(actionName)) {
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }

            ControllerDescriptor controllerDescriptor = GetControllerDescriptor(controllerContext);
            ActionDescriptor actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);
            if (actionDescriptor != null) {
                FilterInfo filterInfo = GetFilters(controllerContext, actionDescriptor);
                Action continuation = null;

                BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                    try {
                        AuthorizationContext authContext = InvokeAuthorizationFilters(controllerContext, filterInfo.AuthorizationFilters, actionDescriptor);
                        if (authContext.Result != null) {
                            // the auth filter signaled that we should let it short-circuit the request
                            continuation = () => InvokeActionResult(controllerContext, authContext.Result);
                        }
                        else {
                            if (controllerContext.Controller.ValidateRequest) {
                                ValidateRequest(controllerContext);
                            }

                            IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
                            IAsyncResult asyncResult = BeginInvokeActionMethodWithFilters(controllerContext, filterInfo.ActionFilters, actionDescriptor, parameters, asyncCallback, asyncState);
                            continuation = () => {
                                ActionExecutedContext postActionContext = EndInvokeActionMethodWithFilters(asyncResult);
                                InvokeActionResultWithFilters(controllerContext, filterInfo.ResultFilters, postActionContext.Result);
                            };
                            return asyncResult;
                        }
                    }
                    catch (ThreadAbortException) {
                        // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                        // the filters don't see this as an error.
                        throw;
                    }
                    catch (Exception ex) {
                        // something blew up, so execute the exception filters
                        ExceptionContext exceptionContext = InvokeExceptionFilters(controllerContext, filterInfo.ExceptionFilters, ex);
                        if (!exceptionContext.ExceptionHandled) {
                            throw;
                        }

                        continuation = () => InvokeActionResult(controllerContext, exceptionContext.Result);
                    }

                    return BeginInvokeAction_MakeSynchronousAsyncResult(asyncCallback, asyncState);
                };

                EndInvokeDelegate<bool> endDelegate = delegate(IAsyncResult asyncResult) {
                    try {
                        continuation();
                    }
                    catch (ThreadAbortException) {
                        // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                        // the filters don't see this as an error.
                        throw;
                    }
                    catch (Exception ex) {
                        // something blew up, so execute the exception filters
                        ExceptionContext exceptionContext = InvokeExceptionFilters(controllerContext, filterInfo.ExceptionFilters, ex);
                        if (!exceptionContext.ExceptionHandled) {
                            throw;
                        }
                        InvokeActionResult(controllerContext, exceptionContext.Result);
                    }

                    return true;
                };

                return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _invokeActionTag);
            }
            else {
                // Notify the controller that no action was found.
                return BeginInvokeAction_ActionNotFound(callback, state);
            }
        }

        private static IAsyncResult BeginInvokeAction_ActionNotFound(AsyncCallback callback, object state) {
            BeginInvokeDelegate beginDelegate = BeginInvokeAction_MakeSynchronousAsyncResult;

            EndInvokeDelegate<bool> endDelegate = delegate(IAsyncResult asyncResult) {
                return false;
            };

            return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _invokeActionTag);
        }

        private static IAsyncResult BeginInvokeAction_MakeSynchronousAsyncResult(AsyncCallback callback, object state) {
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(state);
            asyncResult.MarkCompleted(true /* completedSynchronously */, callback);
            return asyncResult;
        }

        protected internal virtual IAsyncResult BeginInvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters, AsyncCallback callback, object state) {
            AsyncActionDescriptor asyncActionDescriptor = actionDescriptor as AsyncActionDescriptor;
            if (asyncActionDescriptor != null) {
                return BeginInvokeAsynchronousActionMethod(controllerContext, asyncActionDescriptor, parameters, callback, state);
            }
            else {
                return BeginInvokeSynchronousActionMethod(controllerContext, actionDescriptor, parameters, callback, state);
            }
        }

        protected internal virtual IAsyncResult BeginInvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters, AsyncCallback callback, object state) {
            Func<ActionExecutedContext> endContinuation = null;

            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                ActionExecutingContext preContext = new ActionExecutingContext(controllerContext, actionDescriptor, parameters);
                IAsyncResult innerAsyncResult = null;

                Func<Func<ActionExecutedContext>> beginContinuation = () => {
                    innerAsyncResult = BeginInvokeActionMethod(controllerContext, actionDescriptor, parameters, asyncCallback, asyncState);
                    return () =>
                        new ActionExecutedContext(controllerContext, actionDescriptor, false /* canceled */, null /* exception */) {
                            Result = EndInvokeActionMethod(innerAsyncResult)
                        };
                };

                // need to reverse the filter list because the continuations are built up backward
                Func<Func<ActionExecutedContext>> thunk = filters.Reverse().Aggregate(beginContinuation,
                    (next, filter) => () => InvokeActionMethodFilterAsynchronously(filter, preContext, next));
                endContinuation = thunk();

                if (innerAsyncResult != null) {
                    // we're just waiting for the inner result to complete
                    return innerAsyncResult;
                }
                else {
                    // something was short-circuited and the action was not called, so this was a synchronous operation
                    SimpleAsyncResult newAsyncResult = new SimpleAsyncResult(asyncState);
                    newAsyncResult.MarkCompleted(true /* completedSynchronously */, asyncCallback);
                    return newAsyncResult;
                }
            };

            EndInvokeDelegate<ActionExecutedContext> endDelegate = delegate(IAsyncResult asyncResult) {
                return endContinuation();
            };

            return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _invokeActionMethodWithFiltersTag);
        }

        private IAsyncResult BeginInvokeAsynchronousActionMethod(ControllerContext controllerContext, AsyncActionDescriptor actionDescriptor, IDictionary<string, object> parameters, AsyncCallback callback, object state) {
            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                return actionDescriptor.BeginExecute(controllerContext, parameters, asyncCallback, asyncState);
            };

            EndInvokeDelegate<ActionResult> endDelegate = delegate(IAsyncResult asyncResult) {
                object returnValue = actionDescriptor.EndExecute(asyncResult);
                ActionResult result = CreateActionResult(controllerContext, actionDescriptor, returnValue);
                return result;
            };

            return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _invokeActionMethodTag);
        }

        private IAsyncResult BeginInvokeSynchronousActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters, AsyncCallback callback, object state) {
            return AsyncResultWrapper.BeginSynchronous(callback, state,
                () => InvokeSynchronousActionMethod(controllerContext, actionDescriptor, parameters),
                _invokeActionMethodTag);
        }

        public virtual bool EndInvokeAction(IAsyncResult asyncResult) {
            return AsyncResultWrapper.End<bool>(asyncResult, _invokeActionTag);
        }

        protected internal virtual ActionResult EndInvokeActionMethod(IAsyncResult asyncResult) {
            return AsyncResultWrapper.End<ActionResult>(asyncResult, _invokeActionMethodTag);
        }

        protected internal virtual ActionExecutedContext EndInvokeActionMethodWithFilters(IAsyncResult asyncResult) {
            return AsyncResultWrapper.End<ActionExecutedContext>(asyncResult, _invokeActionMethodWithFiltersTag);
        }

        protected override ControllerDescriptor GetControllerDescriptor(ControllerContext controllerContext) {
            Type controllerType = controllerContext.Controller.GetType();
            ControllerDescriptor controllerDescriptor = DescriptorCache.GetDescriptor(controllerType, () => new ReflectedAsyncControllerDescriptor(controllerType));
            return controllerDescriptor;
        }

        internal static Func<ActionExecutedContext> InvokeActionMethodFilterAsynchronously(IActionFilter filter, ActionExecutingContext preContext, Func<Func<ActionExecutedContext>> nextInChain) {
            filter.OnActionExecuting(preContext);
            if (preContext.Result != null) {
                ActionExecutedContext shortCircuitedPostContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, true /* canceled */, null /* exception */) {
                    Result = preContext.Result
                };
                return () => shortCircuitedPostContext;
            }

            // There is a nested try / catch block here that contains much the same logic as the outer block.
            // Since an exception can occur on either side of the asynchronous invocation, we need guards on
            // on both sides. In the code below, the second side is represented by the nested delegate. This
            // is really just a parallel of the synchronous ControllerActionInvoker.InvokeActionMethodFilter()
            // method.

            try {
                Func<ActionExecutedContext> continuation = nextInChain();

                // add our own continuation, then return the new function
                return () => {
                    ActionExecutedContext postContext;
                    bool wasError = true;

                    try {
                        postContext = continuation();
                        wasError = false;
                    }
                    catch (ThreadAbortException) {
                        // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                        // the filters don't see this as an error.
                        postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, null /* exception */);
                        filter.OnActionExecuted(postContext);
                        throw;
                    }
                    catch (Exception ex) {
                        postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, ex);
                        filter.OnActionExecuted(postContext);
                        if (!postContext.ExceptionHandled) {
                            throw;
                        }
                    }
                    if (!wasError) {
                        filter.OnActionExecuted(postContext);
                    }

                    return postContext;
                };
            }
            catch (ThreadAbortException) {
                // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                // the filters don't see this as an error.
                ActionExecutedContext postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, null /* exception */);
                filter.OnActionExecuted(postContext);
                throw;
            }
            catch (Exception ex) {
                ActionExecutedContext postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, ex);
                filter.OnActionExecuted(postContext);
                if (postContext.ExceptionHandled) {
                    return () => postContext;
                }
                else {
                    throw;
                }
            }
        }

        private ActionResult InvokeSynchronousActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters) {
            return base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
        }

    }
}
