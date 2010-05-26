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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    [SuppressMessage("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields",
        Justification = "Public fields for CSS names do not contain secure information.")]
    public class HtmlHelper {

        private delegate string HtmlEncoder(object value);
        private static readonly HtmlEncoder _htmlEncoder = GetHtmlEncoder();

        private static string _idAttributeDotReplacement;

        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

        private AntiForgeryDataSerializer _serializer;

        public HtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes) {
        }

        public HtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection) {
            if (viewContext == null) {
                throw new ArgumentNullException("viewContext");
            }
            if (viewDataContainer == null) {
                throw new ArgumentNullException("viewDataContainer");
            }
            if (routeCollection == null) {
                throw new ArgumentNullException("routeCollection");
            }
            ViewContext = viewContext;
            ViewDataContainer = viewDataContainer;
            RouteCollection = routeCollection;
        }

        public static string IdAttributeDotReplacement {
            get {
                if (String.IsNullOrEmpty(_idAttributeDotReplacement)) {
                    _idAttributeDotReplacement = "_";
                }
                return _idAttributeDotReplacement;
            }
            set {
                _idAttributeDotReplacement = value;
            }
        }

        public RouteCollection RouteCollection {
            get;
            private set;
        }

        internal AntiForgeryDataSerializer Serializer {
            get {
                if (_serializer == null) {
                    _serializer = new AntiForgeryDataSerializer();
                }
                return _serializer;
            }
            set {
                _serializer = value;
            }
        }

        public ViewContext ViewContext {
            get;
            private set;
        }

        public ViewDataDictionary ViewData {
            get {
                return ViewDataContainer.ViewData;
            }
        }

        public IViewDataContainer ViewDataContainer {
            get;
            private set;
        }

        public MvcHtmlString AntiForgeryToken() {
            return AntiForgeryToken(null /* salt */);
        }

        public MvcHtmlString AntiForgeryToken(string salt) {
            return AntiForgeryToken(salt, null /* domain */, null /* path */);
        }

        public MvcHtmlString AntiForgeryToken(string salt, string domain, string path) {
            string formValue = GetAntiForgeryTokenAndSetCookie(salt, domain, path);
            string fieldName = AntiForgeryData.GetAntiForgeryTokenName(null);

            TagBuilder builder = new TagBuilder("input");
            builder.Attributes["type"] = "hidden";
            builder.Attributes["name"] = fieldName;
            builder.Attributes["value"] = formValue;
            return builder.ToMvcHtmlString(TagRenderMode.SelfClosing);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string AttributeEncode(string value) {
            return (!String.IsNullOrEmpty(value)) ? HttpUtility.HtmlAttributeEncode(value) : String.Empty;
        }

        public string AttributeEncode(object value) {
            return AttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public void EnableClientValidation() {
            ViewContext.ClientValidationEnabled = true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value) {
            return (!String.IsNullOrEmpty(value)) ? HttpUtility.HtmlEncode(value) : String.Empty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(object value) {
            return _htmlEncoder(value);
        }

        // method used if HttpUtility.HtmlEncode(object) method does not exist
        private static string EncodeLegacy(object value) {
            string stringVal = Convert.ToString(value, CultureInfo.CurrentCulture);
            return (!String.IsNullOrEmpty(stringVal)) ? HttpUtility.HtmlEncode(stringVal) : String.Empty;
        }

        internal string EvalString(string key) {
            return Convert.ToString(ViewData.Eval(key), CultureInfo.CurrentCulture);
        }

        internal bool EvalBoolean(string key) {
            return Convert.ToBoolean(ViewData.Eval(key), CultureInfo.InvariantCulture);
        }

        internal static IView FindPartialView(ViewContext viewContext, string partialViewName, ViewEngineCollection viewEngineCollection) {
            ViewEngineResult result = viewEngineCollection.FindPartialView(viewContext, partialViewName);
            if (result.View != null) {
                return result.View;
            }

            StringBuilder locationsText = new StringBuilder();
            foreach (string location in result.SearchedLocations) {
                locationsText.AppendLine();
                locationsText.Append(location);
            }

            throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture,
                MvcResources.Common_PartialViewNotFound, partialViewName, locationsText));
        }

        public static string GenerateIdFromName(string name) {
            return GenerateIdFromName(name, IdAttributeDotReplacement);
        }

        public static string GenerateIdFromName(string name, string idAttributeDotReplacement) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (idAttributeDotReplacement == null) {
                throw new ArgumentNullException("idAttributeDotReplacement");
            }

            return name.Replace(".", idAttributeDotReplacement);
        }

        public static string GenerateLink(RequestContext requestContext, RouteCollection routeCollection, string linkText, string routeName, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLink(requestContext, routeCollection, linkText, routeName, actionName, controllerName, null/* protocol */, null/* hostName */, null/* fragment */, routeValues, htmlAttributes);
        }

        public static string GenerateLink(RequestContext requestContext, RouteCollection routeCollection, string linkText, string routeName, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLinkInternal(requestContext, routeCollection, linkText, routeName, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes, true /* includeImplicitMvcValues */);
        }

        private static string GenerateLinkInternal(RequestContext requestContext, RouteCollection routeCollection, string linkText, string routeName, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool includeImplicitMvcValues) {
            string url = UrlHelper.GenerateUrl(routeName, actionName, controllerName, protocol, hostName, fragment, routeValues, routeCollection, requestContext, includeImplicitMvcValues);
            TagBuilder tagBuilder = new TagBuilder("a") {
                InnerHtml = (!String.IsNullOrEmpty(linkText)) ? HttpUtility.HtmlEncode(linkText) : String.Empty
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", url);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        public static string GenerateRouteLink(RequestContext requestContext, RouteCollection routeCollection, string linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateRouteLink(requestContext, routeCollection, linkText, routeName, null/* protocol */, null/* hostName */, null/* fragment */, routeValues, htmlAttributes);
        }

        public static string GenerateRouteLink(RequestContext requestContext, RouteCollection routeCollection, string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLinkInternal(requestContext, routeCollection, linkText, routeName, null /* actionName */, null /* controllerName */, protocol, hostName, fragment, routeValues, htmlAttributes, false /* includeImplicitMvcValues */);
        }

        private string GetAntiForgeryTokenAndSetCookie(string salt, string domain, string path) {
            string cookieName = AntiForgeryData.GetAntiForgeryTokenName(ViewContext.HttpContext.Request.ApplicationPath);

            AntiForgeryData cookieToken;
            HttpCookie cookie = ViewContext.HttpContext.Request.Cookies[cookieName];
            if (cookie != null) {
                cookieToken = Serializer.Deserialize(cookie.Value);
            }
            else {
                cookieToken = AntiForgeryData.NewToken();
                string cookieValue = Serializer.Serialize(cookieToken);

                HttpCookie newCookie = new HttpCookie(cookieName, cookieValue) { HttpOnly = true, Domain = domain };
                if (!String.IsNullOrEmpty(path)) {
                    newCookie.Path = path;
                }
                ViewContext.HttpContext.Response.Cookies.Set(newCookie);
            }

            AntiForgeryData formToken = new AntiForgeryData(cookieToken) {
                Salt = salt,
                Username = AntiForgeryData.GetUsername(ViewContext.HttpContext.User)
            };
            string formValue = Serializer.Serialize(formToken);
            return formValue;
        }

        public static string GetFormMethodString(FormMethod method) {
            switch (method) {
                case FormMethod.Get:
                    return "get";
                case FormMethod.Post:
                    return "post";
                default:
                    return "post";
            }
        }

        // selects the v3.5 (legacy) or v4 HTML encoder
        private static HtmlEncoder GetHtmlEncoder() {
            return TypeHelpers.CreateDelegate<HtmlEncoder>(TypeHelpers.SystemWebAssembly, "System.Web.HttpUtility", "HtmlEncode", null)
                ?? EncodeLegacy;
        }

        public static string GetInputTypeString(InputType inputType) {
            switch (inputType) {
                case InputType.CheckBox:
                    return "checkbox";
                case InputType.Hidden:
                    return "hidden";
                case InputType.Password:
                    return "password";
                case InputType.Radio:
                    return "radio";
                case InputType.Text:
                    return "text";
                default:
                    return "text";
            }
        }

        internal object GetModelStateValue(string key, Type destinationType) {
            ModelState modelState;
            if (ViewData.ModelState.TryGetValue(key, out modelState)) {
                if (modelState.Value != null) {
                    return modelState.Value.ConvertTo(destinationType, null /* culture */);
                }
            }
            return null;
        }

        public MvcHtmlString HttpMethodOverride(HttpVerbs httpVerb) {
            string httpMethod;
            switch (httpVerb) {
                case HttpVerbs.Delete:
                    httpMethod = "DELETE";
                    break;
                case HttpVerbs.Head:
                    httpMethod = "HEAD";
                    break;
                case HttpVerbs.Put:
                    httpMethod = "PUT";
                    break;
                default:
                    throw new ArgumentException(MvcResources.HtmlHelper_InvalidHttpVerb, "httpVerb");
            }

            return HttpMethodOverride(httpMethod);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public MvcHtmlString HttpMethodOverride(string httpMethod) {
            if (String.IsNullOrEmpty(httpMethod)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "httpMethod");
            }
            if (String.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException(MvcResources.HtmlHelper_InvalidHttpMethod, "httpMethod");
            }

            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.Attributes["type"] = "hidden";
            tagBuilder.Attributes["name"] = HttpRequestExtensions.XHttpMethodOverrideKey;
            tagBuilder.Attributes["value"] = httpMethod;

            return tagBuilder.ToMvcHtmlString(TagRenderMode.SelfClosing);
        }

        internal virtual void RenderPartialInternal(string partialViewName, ViewDataDictionary viewData, object model, TextWriter writer, ViewEngineCollection viewEngineCollection) {
            if (String.IsNullOrEmpty(partialViewName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "partialViewName");
            }

            ViewDataDictionary newViewData = null;

            if (model == null) {
                if (viewData == null) {
                    newViewData = new ViewDataDictionary(ViewData);
                }
                else {
                    newViewData = new ViewDataDictionary(viewData);
                }
            }
            else {
                if (viewData == null) {
                    newViewData = new ViewDataDictionary(model);
                }
                else {
                    newViewData = new ViewDataDictionary(viewData) { Model = model };
                }
            }

            ViewContext newViewContext = new ViewContext(ViewContext, ViewContext.View, newViewData, ViewContext.TempData, writer);
            IView view = FindPartialView(newViewContext, partialViewName, viewEngineCollection);
            view.Render(newViewContext, writer);
        }

    }
}
