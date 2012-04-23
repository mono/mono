namespace System.Web.Mvc {
    using System.Collections.Specialized;

    internal class UrlRewriterHelper {
        private const string _urlWasRewrittenServerVar = "IIS_WasUrlRewritten";
        private const string _urlRewriterEnabledServerVar = "IIS_UrlRewriteModule";

        private object _lockObject = new object();
        private bool _urlRewriterIsTurnedOnValue;
        private bool _urlRewriterIsTurnedOnCalculated = false;

        private static bool WasThisRequestRewritten(HttpContextBase httpContext) {
            NameValueCollection serverVars = httpContext.Request.ServerVariables;
            bool requestWasRewritten = (serverVars != null && serverVars[_urlWasRewrittenServerVar] != null);
            return requestWasRewritten;
        }

        private bool IsUrlRewriterTurnedOn(HttpContextBase httpContext) {
            // Need to do double-check locking because a single instance of this class is shared in the entire app domain (see PathHelpers)
            if (!_urlRewriterIsTurnedOnCalculated) {
                lock (_lockObject) {
                    if (!_urlRewriterIsTurnedOnCalculated) {
                        NameValueCollection serverVars = httpContext.Request.ServerVariables;
                        bool urlRewriterIsEnabled = (serverVars != null && serverVars[_urlRewriterEnabledServerVar] != null);
                        _urlRewriterIsTurnedOnValue = urlRewriterIsEnabled;
                        _urlRewriterIsTurnedOnCalculated = true;
                    }
                }
            }
            return _urlRewriterIsTurnedOnValue;
        }

        public virtual bool WasRequestRewritten(HttpContextBase httpContext) {
            return IsUrlRewriterTurnedOn(httpContext) && WasThisRequestRewritten(httpContext);
        }
    }
}