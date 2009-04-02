/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Resources;
    using System.Web.UI;

    [FileLevelControlBuilder(typeof(ViewUserControlControlBuilder))]
    public class ViewUserControl : UserControl, IViewDataContainer {
        private AjaxHelper _ajaxHelper;
        private HtmlHelper _htmlHelper;
        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private string _viewDataKey;

        public AjaxHelper Ajax {
            get {
                if (_ajaxHelper == null) {
                    _ajaxHelper = new AjaxHelper(ViewContext, this);
                }
                return _ajaxHelper;
            }
        }

        public HtmlHelper Html {
            get {
                if (_htmlHelper == null) {
                    _htmlHelper = new HtmlHelper(ViewContext, this);
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This is the mechanism by which the ViewUserControl gets its ViewDataDictionary object.")]
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
                        CultureInfo.CurrentUICulture,
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
            // TODO: Remove this hack. Without it, the browser appears to always load cached output
            viewContext.HttpContext.Response.Cache.SetExpires(DateTime.Now);
            ViewUserControlContainerPage containerPage = new ViewUserControlContainerPage(this);
            // Tracing requires Page IDs to be unique.
            ID = Guid.NewGuid().ToString();

            RenderViewAndRestoreContentType(containerPage, viewContext);
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

        private sealed class ViewUserControlContainerPage : ViewPage {
            public ViewUserControlContainerPage(ViewUserControl userControl) {
                Controls.Add(userControl);
            }
        }
    }
}
