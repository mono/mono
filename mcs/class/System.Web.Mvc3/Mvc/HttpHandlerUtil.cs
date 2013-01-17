namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Mvc.Resources;
    using System.Web.UI;

    internal static class HttpHandlerUtil {

        // Since Server.Execute() doesn't propagate HttpExceptions where the status code is
        // anything other than 500, we need to wrap these exceptions ourselves.
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Dispose on Page doesn't do anything by default, and we control both of these internal types.")]
        public static IHttpHandler WrapForServerExecute(IHttpHandler httpHandler) {
            IHttpAsyncHandler asyncHandler = httpHandler as IHttpAsyncHandler;
            return (asyncHandler != null) ? new ServerExecuteHttpHandlerAsyncWrapper(asyncHandler) : new ServerExecuteHttpHandlerWrapper(httpHandler);
        }

        // Server.Execute() requires that the provided IHttpHandler subclass Page.
        internal class ServerExecuteHttpHandlerWrapper : Page {
            private readonly IHttpHandler _httpHandler;

            public ServerExecuteHttpHandlerWrapper(IHttpHandler httpHandler) {
                _httpHandler = httpHandler;
            }

            internal IHttpHandler InnerHandler {
                get {
                    return _httpHandler;
                }
            }

            public override void ProcessRequest(HttpContext context) {
                Wrap(() => _httpHandler.ProcessRequest(context));
            }

            protected static void Wrap(Action action) {
                Wrap(delegate {
                    action();
                    return (object)null;
                });
            }

            protected static TResult Wrap<TResult>(Func<TResult> func) {
                try {
                    return func();
                }
                catch (HttpException he) {
                    if (he.GetHttpCode() == 500) {
                        throw; // doesn't need to be wrapped
                    }
                    else {
                        HttpException newHe = new HttpException(500, MvcResources.ViewPageHttpHandlerWrapper_ExceptionOccurred, he);
                        throw newHe;
                    }
                }
            }
        }

        private sealed class ServerExecuteHttpHandlerAsyncWrapper : ServerExecuteHttpHandlerWrapper, IHttpAsyncHandler {
            private readonly IHttpAsyncHandler _httpHandler;

            public ServerExecuteHttpHandlerAsyncWrapper(IHttpAsyncHandler httpHandler)
                : base(httpHandler) {
                _httpHandler = httpHandler;
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
                return Wrap(() => _httpHandler.BeginProcessRequest(context, cb, extraData));
            }

            public void EndProcessRequest(IAsyncResult result) {
                Wrap(() => _httpHandler.EndProcessRequest(result));
            }
        }

    }
}
