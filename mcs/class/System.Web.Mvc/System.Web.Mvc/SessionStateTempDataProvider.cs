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
    using System.Web.Mvc.Resources;

    public class SessionStateTempDataProvider : ITempDataProvider {
        internal const string TempDataSessionStateKey = "__ControllerTempData";

        public virtual IDictionary<string, object> LoadTempData(ControllerContext controllerContext) {
            HttpContextBase httpContext = controllerContext.HttpContext;
            
            if (httpContext.Session == null) {
                throw new InvalidOperationException(MvcResources.SessionStateTempDataProvider_SessionStateDisabled);
            }

            Dictionary<string, object> tempDataDictionary = httpContext.Session[TempDataSessionStateKey] as Dictionary<string, object>;

            if (tempDataDictionary != null) {
                // If we got it from Session, remove it so that no other request gets it
                httpContext.Session.Remove(TempDataSessionStateKey);
                return tempDataDictionary;
            }
            else {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public virtual void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values) {
            HttpContextBase httpContext = controllerContext.HttpContext;

            if (httpContext.Session == null) {
                throw new InvalidOperationException(MvcResources.SessionStateTempDataProvider_SessionStateDisabled);
            }

            httpContext.Session[TempDataSessionStateKey] = values;
        }        
    }
}
