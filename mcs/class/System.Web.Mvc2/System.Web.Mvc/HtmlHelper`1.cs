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
    using System.Web.Routing;

    public class HtmlHelper<TModel> : HtmlHelper {
        private ViewDataDictionary<TModel> _viewData;

        public HtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes) {
        }

        public HtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(viewContext, viewDataContainer, routeCollection) {

            _viewData = new ViewDataDictionary<TModel>(viewDataContainer.ViewData);
        }

        public new ViewDataDictionary<TModel> ViewData {
            get {
                return _viewData;
            }
        }
    }
}
