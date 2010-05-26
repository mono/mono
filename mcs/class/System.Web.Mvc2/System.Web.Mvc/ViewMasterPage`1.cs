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

    public class ViewMasterPage<TModel> : ViewMasterPage {
        private AjaxHelper<TModel> _ajaxHelper;
        private HtmlHelper<TModel> _htmlHelper;
        private ViewDataDictionary<TModel> _viewData;

        public new AjaxHelper<TModel> Ajax {
            get {
                if (_ajaxHelper == null) {
                    _ajaxHelper = new AjaxHelper<TModel>(ViewContext, ViewPage);
                }
                return _ajaxHelper;
            }
        }

        public new HtmlHelper<TModel> Html {
            get {
                if (_htmlHelper == null) {
                    _htmlHelper = new HtmlHelper<TModel>(ViewContext, ViewPage);
                }
                return _htmlHelper;
            }
        }

        public new TModel Model {
            get {
                return ViewData.Model;
            }
        }

        public new ViewDataDictionary<TModel> ViewData {
            get {
                if (_viewData == null) {
                    _viewData = new ViewDataDictionary<TModel>(ViewPage.ViewData);
                }
                return _viewData;
            }
        }
    }
}
