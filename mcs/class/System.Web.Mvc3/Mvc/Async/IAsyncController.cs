namespace System.Web.Mvc.Async {
    using System.Web.Routing;

    public interface IAsyncController : IController {
        IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state);
        void EndExecute(IAsyncResult asyncResult);
    }
}
