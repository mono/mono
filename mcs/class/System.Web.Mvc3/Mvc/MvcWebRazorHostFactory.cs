namespace System.Web.Mvc {
    using System.Web.Mvc.Razor;
    using System.Web.WebPages.Razor;

    public class MvcWebRazorHostFactory : WebRazorHostFactory {

        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath) {
            WebPageRazorHost host = base.CreateHost(virtualPath, physicalPath);

            if(!host.IsSpecialPage) {
                return new MvcWebPageRazorHost(virtualPath, physicalPath);
            }

            return host;
        }
    }
}
