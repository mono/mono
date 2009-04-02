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
    using System.Diagnostics.CodeAnalysis;

    public abstract class ViewResultBase : ActionResult {
        private TempDataDictionary _tempData;
        private ViewDataDictionary _viewData;
        private ViewEngineCollection _viewEngineCollection;
        private string _viewName;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This entire type is meant to be mutable.")]
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This entire type is meant to be mutable.")]
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This entire type is meant to be mutable.")]
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

            ViewContext viewContext = new ViewContext(context, View, ViewData, TempData);
            View.Render(viewContext, context.HttpContext.Response.Output);

            if (result != null) {
                result.ViewEngine.ReleaseView(context, View);
            }
        }

        protected abstract ViewEngineResult FindView(ControllerContext context);
    }
}
