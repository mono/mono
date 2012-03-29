namespace System.Web.Mvc {
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Resources;
    using System.Web.WebPages;

    public abstract class WebViewPage : WebPageBase, IViewDataContainer, IViewStartPageChild {

        private ViewDataDictionary _viewData;
        private DynamicViewDataDictionary _dynamicViewData;
        private HttpContextBase _context;

        public AjaxHelper<object> Ajax {
            get;
            set;
        }

        public override HttpContextBase Context {
            // REVIEW why are we forced to override this?
            get {
                return _context ?? ViewContext.HttpContext;
            }
            set {
                _context = value;
            }
        }

        public HtmlHelper<object> Html {
            get;
            set;
        }

        public object Model {
            get {
                return ViewData.Model;
            }
        }

        internal string OverridenLayoutPath { get; set; }

        public TempDataDictionary TempData {
            get {
                return ViewContext.TempData;
            }
        }

        public UrlHelper Url {
            get;
            set;
        }

        public dynamic ViewBag {
            get {
                if (_dynamicViewData == null) {
                    _dynamicViewData = new DynamicViewDataDictionary(() => ViewData);
                }
                return _dynamicViewData;
            }
        }

        public ViewContext ViewContext {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is the mechanism by which the ViewPage gets its ViewDataDictionary object.")]
        public ViewDataDictionary ViewData {
            get {
                if (_viewData == null) {
                    SetViewData(new ViewDataDictionary());
                }
                return _viewData;
            }
            set {
                SetViewData(value);
            }
        }

        protected override void ConfigurePage(WebPageBase parentPage) {
            var baseViewPage = parentPage as WebViewPage;
            if (baseViewPage == null) {
                // TODO : review if this check is even necessary.
                // When this method is called by the framework parentPage should already be an instance of WebViewPage
                // Need to review what happens if this method gets called in Plan9 pointing at an MVC view
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MvcResources.CshtmlView_WrongViewBase, parentPage.VirtualPath));
            }

            // Set ViewContext and ViewData here so that the layout page inherits ViewData from the main page
            ViewContext = baseViewPage.ViewContext;
            ViewData = baseViewPage.ViewData;
            InitHelpers();
        }

        public override void ExecutePageHierarchy() {
            // Change the Writer so that things like Html.BeginForm work correctly
            ViewContext.Writer = Output;

            base.ExecutePageHierarchy();

            // Overwrite LayoutPage so that returning a view with a custom master page works.
            if (!String.IsNullOrEmpty(OverridenLayoutPath)) {
                Layout = OverridenLayoutPath;
            }
        }

        public virtual void InitHelpers() {
            Ajax = new AjaxHelper<object>(ViewContext, this);
            Html = new HtmlHelper<object>(ViewContext, this);
            Url = new UrlHelper(ViewContext.RequestContext);
        }

        protected virtual void SetViewData(ViewDataDictionary viewData) {
            _viewData = viewData;
        }
    }
}
