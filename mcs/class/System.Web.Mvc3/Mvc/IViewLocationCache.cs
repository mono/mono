namespace System.Web.Mvc {
    using System.Web;

    public interface IViewLocationCache {
        string GetViewLocation(HttpContextBase httpContext, string key);
        void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath);
    }
}
