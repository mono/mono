namespace System.Web.Mvc {
    using System;
    using System.Web.Routing;

    internal static class AreaHelpers {

        public static string GetAreaName(RouteBase route) {
            IRouteWithArea routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null) {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;
            if (castRoute != null && castRoute.DataTokens != null) {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }

        public static string GetAreaName(RouteData routeData) {
            object area;
            if (routeData.DataTokens.TryGetValue("area", out area)) {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }

    }
}
