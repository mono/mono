namespace System.Web.Mvc {
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web.Mvc.Resources;

    public class WebFormView : BuildManagerCompiledView {

        public WebFormView(ControllerContext controllerContext, string viewPath)
            : this(controllerContext, viewPath, null, null) {
        }

        public WebFormView(ControllerContext controllerContext, string viewPath, string masterPath)
            : this(controllerContext, viewPath, masterPath, null) {
        }

        public WebFormView(ControllerContext controllerContext, string viewPath, string masterPath, IViewPageActivator viewPageActivator)
            : base(controllerContext, viewPath, viewPageActivator) {
            MasterPath = masterPath ?? String.Empty;
        }

        public string MasterPath {
            get;
            private set;
        }

        protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance) {

            ViewPage viewPage = instance as ViewPage;
            if (viewPage != null) {
                RenderViewPage(viewContext, viewPage);
                return;
            }

            ViewUserControl viewUserControl = instance as ViewUserControl;
            if (viewUserControl != null) {
                RenderViewUserControl(viewContext, viewUserControl);
                return;
            }

            throw new InvalidOperationException(
                String.Format(
                    CultureInfo.CurrentCulture,
                    MvcResources.WebFormViewEngine_WrongViewBase,
                    ViewPath));
        }

        private void RenderViewPage(ViewContext context, ViewPage page) {
            if (!String.IsNullOrEmpty(MasterPath)) {
                page.MasterLocation = MasterPath;
            }

            page.ViewData = context.ViewData;
            page.RenderView(context);
        }

        private void RenderViewUserControl(ViewContext context, ViewUserControl control) {
            if (!String.IsNullOrEmpty(MasterPath)) {
                throw new InvalidOperationException(MvcResources.WebFormViewEngine_UserControlCannotHaveMaster);
            }

            control.ViewData = context.ViewData;
            control.RenderView(context);
        }
    }
}
