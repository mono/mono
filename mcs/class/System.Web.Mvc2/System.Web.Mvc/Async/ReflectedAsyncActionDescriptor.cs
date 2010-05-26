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
    using System.Reflection;
    using System.Threading;

    public class ReflectedAsyncActionDescriptor : AsyncActionDescriptor {

        private readonly object _executeTag = new object();

        private readonly string _actionName;
        private readonly ControllerDescriptor _controllerDescriptor;
        private ParameterDescriptor[] _parametersCache;

        public ReflectedAsyncActionDescriptor(MethodInfo asyncMethodInfo, MethodInfo completedMethodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : this(asyncMethodInfo, completedMethodInfo, actionName, controllerDescriptor, true /* validateMethods */) {
        }

        internal ReflectedAsyncActionDescriptor(MethodInfo asyncMethodInfo, MethodInfo completedMethodInfo, string actionName, ControllerDescriptor controllerDescriptor, bool validateMethods) {
            if (asyncMethodInfo == null) {
                throw new ArgumentNullException("asyncMethodInfo");
            }
            if (completedMethodInfo == null) {
                throw new ArgumentNullException("completedMethodInfo");
            }
            if (String.IsNullOrEmpty(actionName)) {
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }
            if (controllerDescriptor == null) {
                throw new ArgumentNullException("controllerDescriptor");
            }

            if (validateMethods) {
                string asyncFailedMessage = VerifyActionMethodIsCallable(asyncMethodInfo);
                if (asyncFailedMessage != null) {
                    throw new ArgumentException(asyncFailedMessage, "asyncMethodInfo");
                }

                string completedFailedMessage = VerifyActionMethodIsCallable(completedMethodInfo);
                if (completedFailedMessage != null) {
                    throw new ArgumentException(completedFailedMessage, "completedMethodInfo");
                }
            }

            AsyncMethodInfo = asyncMethodInfo;
            CompletedMethodInfo = completedMethodInfo;
            _actionName = actionName;
            _controllerDescriptor = controllerDescriptor;
        }

        public override string ActionName {
            get {
                return _actionName;
            }
        }

        public MethodInfo AsyncMethodInfo {
            get;
            private set;
        }

        public MethodInfo CompletedMethodInfo {
            get;
            private set;
        }

        public override ControllerDescriptor ControllerDescriptor {
            get {
                return _controllerDescriptor;
            }
        }

        public override IAsyncResult BeginExecute(ControllerContext controllerContext, IDictionary<string, object> parameters, AsyncCallback callback, object state) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (parameters == null) {
                throw new ArgumentNullException("parameters");
            }

            AsyncManager asyncManager = GetAsyncManager(controllerContext.Controller);

            BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                // call the XxxAsync() method
                ParameterInfo[] parameterInfos = AsyncMethodInfo.GetParameters();
                var rawParameterValues = from parameterInfo in parameterInfos
                                         select ExtractParameterFromDictionary(parameterInfo, parameters, AsyncMethodInfo);
                object[] parametersArray = rawParameterValues.ToArray();

                TriggerListener listener = new TriggerListener();
                SimpleAsyncResult asyncResult = new SimpleAsyncResult(asyncState);

                // hook the Finished event to notify us upon completion
                Trigger finishTrigger = listener.CreateTrigger();
                asyncManager.Finished += delegate { finishTrigger.Fire(); };
                asyncManager.OutstandingOperations.Increment();

                // to simplify the logic, force the rest of the pipeline to execute in an asynchronous callback
                listener.SetContinuation(() => ThreadPool.QueueUserWorkItem(_ => asyncResult.MarkCompleted(false /* completedSynchronously */, asyncCallback)));

                // the inner operation might complete synchronously, so all setup work has to be done before this point
                ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(AsyncMethodInfo);
                dispatcher.Execute(controllerContext.Controller, parametersArray); // ignore return value from this method

                // now that the XxxAsync() method has completed, kick off any pending operations
                asyncManager.OutstandingOperations.Decrement();
                listener.Activate();
                return asyncResult;
            };

            EndInvokeDelegate<object> endDelegate = delegate(IAsyncResult asyncResult) {
                // call the XxxCompleted() method
                ParameterInfo[] completionParametersInfos = CompletedMethodInfo.GetParameters();
                var rawCompletionParameterValues = from parameterInfo in completionParametersInfos
                                                   select ExtractParameterOrDefaultFromDictionary(parameterInfo, asyncManager.Parameters);
                object[] completionParametersArray = rawCompletionParameterValues.ToArray();

                ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(CompletedMethodInfo);
                object actionReturnValue = dispatcher.Execute(controllerContext.Controller, completionParametersArray);
                return actionReturnValue;
            };

            return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _executeTag, asyncManager.Timeout);
        }

        public override object EndExecute(IAsyncResult asyncResult) {
            return AsyncResultWrapper.End<object>(asyncResult, _executeTag);
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return AsyncMethodInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return AsyncMethodInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override FilterInfo GetFilters() {
            // By default, we only look at filters on the XxxAsync() method.
            return GetFilters(AsyncMethodInfo);
        }

        public override ParameterDescriptor[] GetParameters() {
            ParameterDescriptor[] parameters = LazilyFetchParametersCollection();

            // need to clone array so that user modifications aren't accidentally stored
            return (ParameterDescriptor[])parameters.Clone();
        }

        public override ICollection<ActionSelector> GetSelectors() {
            // By default, we only look at filters on the XxxAsync() method.

            ActionMethodSelectorAttribute[] attrs = (ActionMethodSelectorAttribute[])AsyncMethodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), true /* inherit */);
            ActionSelector[] selectors = Array.ConvertAll(attrs, attr => (ActionSelector)(controllerContext => attr.IsValidForRequest(controllerContext, AsyncMethodInfo)));
            return selectors;
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return AsyncMethodInfo.IsDefined(attributeType, inherit);
        }

        private ParameterDescriptor[] LazilyFetchParametersCollection() {
            return DescriptorUtil.LazilyFetchOrCreateDescriptors<ParameterInfo, ParameterDescriptor>(
                ref _parametersCache /* cacheLocation */,
                AsyncMethodInfo.GetParameters /* initializer */,
                parameterInfo => new ReflectedParameterDescriptor(parameterInfo, this) /* converter */);
        }

    }
}
