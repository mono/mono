namespace System.Web.Routing {
    // Token class used to signal Auth failures, not meant to be used as a handler    
    internal sealed class UrlAuthFailureHandler : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            throw new NotImplementedException();
        }

        public bool IsReusable { 
            get {
                return true;
            }
        }
    }
}
