namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Web.Mvc.Resources;
    using System.Web.UI;

    [FileLevelControlBuilder(typeof(ViewUserControlControlBuilder))]
    public class ViewUserControl : UserControl, IViewDataContainer {
        private AjaxHelper<object> _ajaxHelper;
        private DynamicViewDataDictionary _dynamicViewData;
        private HtmlHelper<object> _htmlHelper;
        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private string _viewDataKey;

        public AjaxHelper<object> Ajax {
            get {
                if (_ajaxHelper == null) {
                    _ajaxHelper = new AjaxHelper<object>(ViewContext, this);
                }
                return _ajaxHelper;
            }
        }

        public HtmlHelper<object> Html {
            get {
                if (_htmlHelper == null) {
                    _htmlHelper = new HtmlHelper<object>(ViewContext, this);
                }
                return _htmlHelper;
            }
        }

        public object Model {
            get {
                return ViewData.Model;
            }
        }

        public TempDataDictionary TempData {
            get {
                return ViewPage.TempData;
            }
        }

        public UrlHelper Url {
            get {
                return ViewPage.Url;
            }
        }

        public dynamic ViewBag {
            get {
                if (_dynamicViewData == null) {
                    _dynamicViewData = new DynamicViewDataDictionary(() => ViewData);
                }
                return _dynamicViewData;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ViewContext ViewContext {
            get {
                return _viewContext ?? ViewPage.ViewContext;
            }
            set {
                _viewContext = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is the mechanism by which the ViewUserControl gets its ViewDataDictionary object.")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ViewDataDictionary ViewData {
            get {
                EnsureViewData();
                return _viewData;
            }
            set {
                SetViewData(value);
            }
        }

        [DefaultValue("")]
        public string ViewDataKey {
            get {
                return _viewDataKey ?? String.Empty;
            }
            set {
                _viewDataKey = value;
            }
        }

        internal ViewPage ViewPage {
            get {
                ViewPage viewPage = Page as ViewPage;
                if (viewPage == null) {
                    throw new InvalidOperationException(MvcResources.ViewUserControl_RequiresViewPage);
                }
                return viewPage;
            }
        }

        public HtmlTextWriter Writer {
            get {
                return ViewPage.Writer;
            }
        }

        protected virtual void SetViewData(ViewDataDictionary viewData) {
            _viewData = viewData;
        }

        protected void EnsureViewData() {
            if (_viewData != null) {
                return;
            }

            // Get the ViewData for this ViewUserControl, optionally using the specified ViewDataKey
            IViewDataContainer vdc = GetViewDataContainer(this);
            if (vdc == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.ViewUserControl_RequiresViewDataProvider,
                        AppRelativeVirtualPath));
            }

            ViewDataDictionary myViewData = vdc.ViewData;

            // If we have a ViewDataKey, try to extract the ViewData from the dictionary, otherwise
            // return the container's ViewData.
            if (!String.IsNullOrEmpty(ViewDataKey)) {
                object target = myViewData.Eval(ViewDataKey);
                myViewData = target as ViewDataDictionary ?? new ViewDataDictionary(myViewData) { Model = target };
            }

            SetViewData(myViewData);
        }

        private static IViewDataContainer GetViewDataContainer(Control control) {
            // Walk up the control hierarchy until we find someone that implements IViewDataContainer
            while (control != null) {
                control = control.Parent;
                IViewDataContainer vdc = control as IViewDataContainer;
                if (vdc != null) {
                    return vdc;
                }
            }
            return null;
        }

        public virtual void RenderView(ViewContext viewContext) {
            using (ViewUserControlContainerPage containerPage = new ViewUserControlContainerPage(this)) {
                RenderViewAndRestoreContentType(containerPage, viewContext);
            }
        }

        internal static void RenderViewAndRestoreContentType(ViewPage containerPage, ViewContext viewContext) {
            // We need to restore the Content-Type since Page.SetIntrinsics() will reset it. It's not possible
            // to work around the call to SetIntrinsics() since the control's render method requires the
            // containing page's Response property to be non-null, and SetIntrinsics() is the only way to set
            // this.
            string savedContentType = viewContext.HttpContext.Response.ContentType;
            containerPage.RenderView(viewContext);
            viewContext.HttpContext.Response.ContentType = savedContentType;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "textWriter", Justification = "This method existed in MVC 1.0 and has been deprecated.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method existed in MVC 1.0 and has been deprecated.")]
        [Obsolete("The TextWriter is now provided by the ViewContext object passed to the RenderView method.", true /* error */)]
        public void SetTextWriter(TextWriter textWriter) {
            // this is now a no-op
        }

        private sealed class ViewUserControlContainerPage : ViewPage {
            private readonly ViewUserControl _userControl;

            public ViewUserControlContainerPage(ViewUserControl userControl) {
                _userControl = userControl;
            }

            public override void ProcessRequest(HttpContext context) {
                _userControl.ID = ViewPage.NextId();
                Controls.Add(_userControl);

                base.ProcessRequest(context);
            }
        }
    }
}
