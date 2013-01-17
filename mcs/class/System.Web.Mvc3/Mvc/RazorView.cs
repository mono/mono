namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc.Razor;
    using System.Web.Mvc.Resources;
    using System.Web.WebPages;

    public class RazorView : BuildManagerCompiledView {

        public RazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions)
            : this(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions, null) {
        }

        public RazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions, IViewPageActivator viewPageActivator)
            : base(controllerContext, viewPath, viewPageActivator) {
            LayoutPath = layoutPath ?? String.Empty;
            RunViewStartPages = runViewStartPages;
            StartPageLookup = StartPage.GetStartPage;
            ViewStartFileExtensions = viewStartFileExtensions ?? Enumerable.Empty<string>();
        }

        public string LayoutPath {
            get;
            private set;
        }

        public bool RunViewStartPages {
            get;
            private set;
        }

        internal StartPageLookupDelegate StartPageLookup {
            get;
            set;
        }

        public IEnumerable<string> ViewStartFileExtensions {
            get;
            private set;
        }

        protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            WebViewPage webViewPage = instance as WebViewPage;
            if (webViewPage == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.CshtmlView_WrongViewBase,
                        ViewPath));
            }

            // An overriden master layout might have been specified when the ViewActionResult got returned.
            // We need to hold on to it so that we can set it on the inner page once it has executed.
            webViewPage.OverridenLayoutPath = LayoutPath;
            webViewPage.VirtualPath = ViewPath;
            webViewPage.ViewContext = viewContext;
            webViewPage.ViewData = viewContext.ViewData;

            webViewPage.InitHelpers();
            WebPageRenderingBase startPage = null;
            if (RunViewStartPages) {
                startPage = StartPageLookup(webViewPage, RazorViewEngine.ViewStartFileName, ViewStartFileExtensions);
            }
            webViewPage.ExecutePageHierarchy(new WebPageContext(context: viewContext.HttpContext, page: null, model: null), writer, startPage);
        }
    }
}
