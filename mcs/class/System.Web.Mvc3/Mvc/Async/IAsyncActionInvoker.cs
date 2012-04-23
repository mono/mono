namespace System.Web.Mvc.Async {
    using System;

    public interface IAsyncActionInvoker : IActionInvoker {
        IAsyncResult BeginInvokeAction(ControllerContext controllerContext, string actionName, AsyncCallback callback, object state);
        bool EndInvokeAction(IAsyncResult asyncResult);
    }
}
