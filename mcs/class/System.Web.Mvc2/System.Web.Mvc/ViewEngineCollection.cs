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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Mvc.Resources;

    public class ViewEngineCollection : Collection<IViewEngine> {

        public ViewEngineCollection() {
        }

        public ViewEngineCollection(IList<IViewEngine> list)
            : base(list) {
        }

        protected override void InsertItem(int index, IViewEngine item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IViewEngine item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        private ViewEngineResult Find(Func<IViewEngine, ViewEngineResult> cacheLocator, Func<IViewEngine, ViewEngineResult> locator) {
            ViewEngineResult result;

            foreach (IViewEngine engine in Items) {
                if (engine != null) {
                    result = cacheLocator(engine);

                    if (result.View != null) {
                        return result;
                    }
                }
            }

            List<string> searched = new List<string>();

            foreach (IViewEngine engine in Items) {
                if (engine != null) {
                    result = locator(engine);

                    if (result.View != null) {
                        return result;
                    }

                    searched.AddRange(result.SearchedLocations);
                }
            }

            return new ViewEngineResult(searched);
        }

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }
            Func<IViewEngine, ViewEngineResult> cacheLocator = e => e.FindPartialView(controllerContext, partialViewName, true);
            Func<IViewEngine, ViewEngineResult> locator = e => e.FindPartialView(controllerContext, partialViewName, false);
            return Find(cacheLocator, locator);
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "viewName");
            }
            Func<IViewEngine, ViewEngineResult> cacheLocator = e => e.FindView(controllerContext, viewName, masterName, true);
            Func<IViewEngine, ViewEngineResult> locator = e => e.FindView(controllerContext, viewName, masterName, false);
            return Find(cacheLocator, locator);
        }
    }
}
