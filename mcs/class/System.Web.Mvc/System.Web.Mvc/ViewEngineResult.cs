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
    using System.Collections.Generic;

    public class ViewEngineResult {

        public ViewEngineResult(IEnumerable<string> searchedLocations) {
            if (searchedLocations == null) {
                throw new ArgumentNullException("searchedLocations");
            }

            SearchedLocations = searchedLocations;
        }

        public ViewEngineResult(IView view, IViewEngine viewEngine) {
            if (view == null) {
                throw new ArgumentNullException("view");
            }
            if (viewEngine == null) {
                throw new ArgumentNullException("viewEngine");
            }

            View = view;
            ViewEngine = viewEngine;
        }

        public IEnumerable<string> SearchedLocations {
            get;
            private set;
        }

        public IView View {
            get;
            private set;
        }

        public IViewEngine ViewEngine {
            get;
            private set;
        }
    }
}
