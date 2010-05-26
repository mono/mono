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

    public abstract class AsyncActionDescriptor : ActionDescriptor {

        public abstract IAsyncResult BeginExecute(ControllerContext controllerContext, IDictionary<string, object> parameters, AsyncCallback callback, object state);

        public abstract object EndExecute(IAsyncResult asyncResult);

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters) {
            // execute an asynchronous task synchronously
            IAsyncResult asyncResult = BeginExecute(controllerContext, parameters, null, null);
            AsyncUtil.WaitForAsyncResultCompletion(asyncResult, controllerContext.HttpContext.ApplicationInstance); // blocks
            return EndExecute(asyncResult);
        }

        internal static AsyncManager GetAsyncManager(ControllerBase controller) {
            IAsyncManagerContainer helperContainer = controller as IAsyncManagerContainer;
            if (helperContainer == null) {
                throw Error.AsyncCommon_ControllerMustImplementIAsyncManagerContainer(controller.GetType());
            }

            return helperContainer.AsyncManager;
        }

    }
}
