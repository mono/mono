namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    public abstract class ViewResultBase : ActionResult {
        private DynamicViewDataDictionary _dynamicViewData;
        private TempDataDictionary _tempData;
        private ViewDataDictionary _viewData;
        private ViewEngineCollection _viewEngineCollection;
        private string _viewName;

        public object Model {
            get {
                return ViewData.Model;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This entire type is meant to be mutable.")]
        public TempDataDictionary TempData {
            get {
                if (_tempData == null) {
                    _tempData = new TempDataDictionary();
                }
                return _tempData;
            }
            set {
                _tempData = value;
            }
        }

        public IView View {
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This entire type is meant to be mutable.")]
        public ViewDataDictionary ViewData {
            get {
                if (_viewData == null) {
                    _viewData = new ViewDataDictionary();
                }
                return _viewData;
            }
            set {
                _viewData = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This entire type is meant to be mutable.")]
        public ViewEngineCollection ViewEngineCollection {
            get {
                return _viewEngineCollection ?? ViewEngines.Engines;
            }
            set {
                _viewEngineCollection = value;
            }
        }

        public string ViewName {
            get {
                return _viewName ?? String.Empty;
            }
            set {
                _viewName = value;
            }
        }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            if (String.IsNullOrEmpty(ViewName)) {
                ViewName = context.RouteData.GetRequiredString("action");
            }

            ViewEngineResult result = null;

            if (View == null) {
                result = FindView(context);
                View = result.View;
            }

            TextWriter writer = context.HttpContext.Response.Output;
            ViewContext viewContext = new ViewContext(context, View, ViewData, TempData, writer);
            View.Render(viewContext, writer);

            if (result != null) {
                result.ViewEngine.ReleaseView(context, View);
            }
        }

        protected abstract ViewEngineResult FindView(ControllerContext context);
    }
}
