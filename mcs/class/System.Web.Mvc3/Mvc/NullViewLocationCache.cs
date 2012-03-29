namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal sealed class NullViewLocationCache : IViewLocationCache {

        #region IViewLocationCache Members
        public string GetViewLocation(HttpContextBase httpContext, string key) {
            return null;
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath) {
        }
        #endregion
    }
}
