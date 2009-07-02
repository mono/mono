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
    using System.Globalization;
    using System.IO;
    using System.Web.Mvc.Resources;

    public class WebFormView : IView {

        private IBuildManager _buildManager;

        public WebFormView(string viewPath)
            : this(viewPath, null) {
        }

        public WebFormView(string viewPath, string masterPath) {
            if (String.IsNullOrEmpty(viewPath)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewPath");
            }

            ViewPath = viewPath;
            MasterPath = masterPath ?? String.Empty;
        }

        internal IBuildManager BuildManager {
            get {
                if (_buildManager == null) {
                    _buildManager = new BuildManagerWrapper();
                }
                return _buildManager;
            }
            set {
                _buildManager = value;
            }
        }

        public string MasterPath {
            get;
            private set;
        }

        public string ViewPath {
            get;
            private set;
        }

        public virtual void Render(ViewContext viewContext, TextWriter writer) {
            if (viewContext == null) {
                throw new ArgumentNullException("viewContext");
            }

            object viewInstance = BuildManager.CreateInstanceFromVirtualPath(ViewPath, typeof(object));
            if (viewInstance == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        MvcResources.WebFormViewEngine_ViewCouldNotBeCreated,
                        ViewPath));
            }

            ViewPage viewPage = viewInstance as ViewPage;
            if (viewPage != null) {
                RenderViewPage(viewContext, viewPage);
                return;
            }

            ViewUserControl viewUserControl = viewInstance as ViewUserControl;
            if (viewUserControl != null) {
                RenderViewUserControl(viewContext, viewUserControl);
                return;
            }

            throw new InvalidOperationException(
                String.Format(
                    CultureInfo.CurrentUICulture,
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
