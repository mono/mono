namespace System.Web.Mvc.Html {
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class ChildActionExtensions {

        // Action

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName) {
            return Action(htmlHelper, actionName, null /* controllerName */, null /* routeValues */);
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName, object routeValues) {
            return Action(htmlHelper, actionName, null /* controllerName */, new RouteValueDictionary(routeValues));
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName, RouteValueDictionary routeValues) {
            return Action(htmlHelper, actionName, null /* controllerName */, routeValues);
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName, string controllerName) {
            return Action(htmlHelper, actionName, controllerName, null /* routeValues */);
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues) {
            return Action(htmlHelper, actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public static MvcHtmlString Action(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues) {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture)) {
                ActionHelper(htmlHelper, actionName, controllerName, routeValues, writer);
                return MvcHtmlString.Create(writer.ToString());
            }
        }

        // RenderAction

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName) {
            RenderAction(htmlHelper, actionName, null /* controllerName */, null /* routeValues */);
        }

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName, object routeValues) {
            RenderAction(htmlHelper, actionName, null /* controllerName */, new RouteValueDictionary(routeValues));
        }

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName, RouteValueDictionary routeValues) {
            RenderAction(htmlHelper, actionName, null /* controllerName */, routeValues);
        }

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName, string controllerName) {
            RenderAction(htmlHelper, actionName, controllerName, null /* routeValues */);
        }

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues) {
            RenderAction(htmlHelper, actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public static void RenderAction(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues) {
            ActionHelper(htmlHelper, actionName, controllerName, routeValues, htmlHelper.ViewContext.Writer);
        }

        // Helpers

        internal static void ActionHelper(HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, TextWriter textWriter) {
            if (htmlHelper == null) {
                throw new ArgumentNullException("htmlHelper");
            }
            if (String.IsNullOrEmpty(actionName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }

            RouteValueDictionary additionalRouteValues = routeValues;
            routeValues = MergeDictionaries(routeValues, htmlHelper.ViewContext.RouteData.Values);
                     
            routeValues["action"] = actionName;
            if (!String.IsNullOrEmpty(controllerName)) {
                routeValues["controller"] = controllerName;
            }

            bool usingAreas;
            VirtualPathData vpd = htmlHelper.RouteCollection.GetVirtualPathForArea(htmlHelper.ViewContext.RequestContext, null /* name */, routeValues, out usingAreas);
            if (vpd == null) {
                throw new InvalidOperationException(MvcResources.Common_NoRouteMatched);
            }
   
            if (usingAreas) {
                routeValues.Remove("area");
                if (additionalRouteValues != null) {
                    additionalRouteValues.Remove("area");
                }
            }

            if (additionalRouteValues != null) {
                routeValues[ChildActionValueProvider.ChildActionValuesKey] = new DictionaryValueProvider<object>(additionalRouteValues, CultureInfo.InvariantCulture);
            }

            RouteData routeData = CreateRouteData(vpd.Route, routeValues, vpd.DataTokens, htmlHelper.ViewContext);
            HttpContextBase httpContext = htmlHelper.ViewContext.HttpContext;
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            ChildActionMvcHandler handler = new ChildActionMvcHandler(requestContext);
            httpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(handler), textWriter, true /* preserveForm */);
        }

        private static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, RouteValueDictionary dataTokens, ViewContext parentViewContext) {
            RouteData routeData = new RouteData();

            foreach (KeyValuePair<string, object> kvp in routeValues) {
                routeData.Values.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<string, object> kvp in dataTokens) {
                routeData.DataTokens.Add(kvp.Key, kvp.Value);
            }

            routeData.Route = route;
            routeData.DataTokens[ControllerContext.PARENT_ACTION_VIEWCONTEXT] = parentViewContext;
            return routeData;
        }

        private static RouteValueDictionary MergeDictionaries(params RouteValueDictionary[] dictionaries) {
            // Merge existing route values with the user provided values
            var result = new RouteValueDictionary();

            foreach (RouteValueDictionary dictionary in dictionaries.Where(d => d != null)) {
                foreach (KeyValuePair<string, object> kvp in dictionary) {
                    if (!result.ContainsKey(kvp.Key)) {
                        result.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return result;
        }

        internal class ChildActionMvcHandler : MvcHandler {
            public ChildActionMvcHandler(RequestContext context)
                : base(context) {
            }

            protected internal override void AddVersionHeader(HttpContextBase httpContext) {
                // No version header for child actions
            }
        }
    }
}
