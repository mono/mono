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
