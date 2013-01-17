namespace System.Web.Mvc.Async {
    using System;

    internal delegate TResult EndInvokeDelegate<TResult>(IAsyncResult asyncResult);
}
