//------------------------------------------------------------------------------
// <copyright file="PageClientProxyGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System.Web;
    using System.Web.UI;

    internal class PageClientProxyGenerator : ClientProxyGenerator {
        private string _path;

        internal PageClientProxyGenerator(IPage page, bool debug)
            : this(VirtualPathUtility.MakeRelative(page.Request.Path, page.Request.FilePath), debug) {
            // Dev10 






        }

        internal PageClientProxyGenerator(string path, bool debug) {
            _path = path;
            _debugMode = debug;
        }

        internal static string GetClientProxyScript(HttpContext context, IPage page, bool debug) {
            // Do nothing during unit tests which have no context or page
            if (context == null || page == null) return null;

            WebServiceData webServiceData = WebServiceData.GetWebServiceData(context,
                page.AppRelativeVirtualPath,
                false /*failIfNoData*/,
                true /*pageMethods */);
            if (webServiceData == null)
                return null;

            PageClientProxyGenerator proxyGenerator = new PageClientProxyGenerator(page, debug);
            return proxyGenerator.GetClientProxyScript(webServiceData);
        }

        protected override void GenerateTypeDeclaration(WebServiceData webServiceData, bool genClass) {
            if (genClass) {
                _builder.Append("PageMethods.prototype = ");
            }
            else {
                _builder.Append("var PageMethods = ");
            }
        }

        protected override string GetProxyTypeName(WebServiceData data) {
            return "PageMethods";
        }

        protected override string GetProxyPath() {
            return _path;
        }
    }
}
