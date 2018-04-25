namespace System.Web {
    using System;
    using System.Web;
    using System.Web.Util;
    using System.Web.Configuration;


    //
    // Module that implements the UrlMappings functionality
    // on IIS 7 in integrated mode, this takes the place of
    // the UrlMappings execution step and is listed in <modules/>
    sealed internal class UrlMappingsModule : IHttpModule {

        internal UrlMappingsModule() {}

        public void Init(HttpApplication application) {
                bool urlMappingsEnabled = false;
                UrlMappingsSection urlMappings = RuntimeConfig.GetConfig().UrlMappings;
                urlMappingsEnabled = urlMappings.IsEnabled && ( urlMappings.UrlMappings.Count > 0 );

                if (urlMappingsEnabled) {
                    application.BeginRequest += new EventHandler(OnEnter);
                }
        }

        public void Dispose() {}

        internal void OnEnter(Object source, EventArgs eventArgs) {
            HttpApplication app = (HttpApplication) source;
            UrlMappingRewritePath(app.Context);
        }

        internal static void UrlMappingRewritePath(HttpContext context) {
            HttpRequest request = context.Request;
            UrlMappingsSection urlMappings = RuntimeConfig.GetAppConfig().UrlMappings;
            string path = request.Path;
            string mappedUrl = null;

            // First check path with query string (for legacy reasons)
            string qs = request.QueryStringText;
            if (!String.IsNullOrEmpty(qs)) {
                mappedUrl = urlMappings.HttpResolveMapping(path + "?" + qs);
            }

            // Check Path if not found
            if (mappedUrl == null)
                mappedUrl = urlMappings.HttpResolveMapping(path);

            if (!String.IsNullOrEmpty(mappedUrl))
                context.RewritePath(mappedUrl, false);
        }
    }
}


