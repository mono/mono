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
    using System.Web.Routing;

    internal static class RouteValuesHelpers {
        public static RouteValueDictionary GetRouteValues(RouteValueDictionary routeValues) {
            return (routeValues != null) ? new RouteValueDictionary(routeValues) : new RouteValueDictionary();
        }

        public static RouteValueDictionary MergeRouteValues(string actionName, string controllerName, RouteValueDictionary implicitRouteValues, RouteValueDictionary routeValues, bool includeImplicitMvcValues) {
            // Create a new dictionary containing implicit and auto-generated values
            RouteValueDictionary mergedRouteValues = new RouteValueDictionary();

            if (includeImplicitMvcValues) {
                // We only include MVC-specific values like 'controller' and 'action' if we are generating an action link.
                // If we are generating a route link [as to MapRoute("Foo", "any/url", new { controller = ... })], including
                // the current controller name will cause the route match to fail if the current controller is not the same
                // as the destination controller.

                object implicitValue;
                if (implicitRouteValues != null && implicitRouteValues.TryGetValue("action", out implicitValue)) {
                    mergedRouteValues["action"] = implicitValue;
                }

                if (implicitRouteValues != null && implicitRouteValues.TryGetValue("controller", out implicitValue)) {
                    mergedRouteValues["controller"] = implicitValue;
                }
            }

            // Merge values from the user's dictionary/object
            if (routeValues != null) {
                foreach (KeyValuePair<string, object> routeElement in GetRouteValues(routeValues)) {
                    mergedRouteValues[routeElement.Key] = routeElement.Value;
                }
            }

            // Merge explicit parameters when not null
            if (actionName != null) {
                mergedRouteValues["action"] = actionName;
            }

            if (controllerName != null) {
                mergedRouteValues["controller"] = controllerName;
            }

            return mergedRouteValues;
        }
    }
}
