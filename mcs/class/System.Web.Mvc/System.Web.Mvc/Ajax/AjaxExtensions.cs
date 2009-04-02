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

namespace System.Web.Mvc.Ajax {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class AjaxExtensions {
        private const string LinkOnClickFormat = "Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), {0});";
        private const string FormOnSubmitFormat = "Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), {0});";

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, ajaxOptions);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, object routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            RouteValueDictionary newValues = new RouteValueDictionary(routeValues);
            Dictionary<string, object> newAttributes = ObjectToCaseSensitiveDictionary(htmlAttributes);
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, newValues, ajaxOptions, newAttributes);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(null, actionName, controllerName, routeValues, ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);

            return GenerateLink(linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            RouteValueDictionary newValues = new RouteValueDictionary(routeValues);
            Dictionary<string, object> newAttributes = ObjectToCaseSensitiveDictionary(htmlAttributes);
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, protocol, hostName, fragment, newValues, ajaxOptions, newAttributes);
        }

        public static string ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(null /* routeName */, actionName, controllerName, protocol, hostName, fragment, routeValues, ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);

            return GenerateLink(linkText, targetUrl, ajaxOptions, htmlAttributes);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, AjaxOptions ajaxOptions) {
            string formAction = ajaxHelper.ViewContext.HttpContext.Request.RawUrl;

            TagBuilder builder = new TagBuilder("form");

            builder.MergeAttribute("action", formAction);
            builder.MergeAttribute("method", "post");
            builder.MergeAttribute("onsubmit", GenerateAjaxScript(GetAjaxOptions(ajaxOptions), FormOnSubmitFormat));

            HttpResponseBase response = ajaxHelper.ViewContext.HttpContext.Response;
            response.Write(builder.ToString(TagRenderMode.StartTag));
            return new MvcForm(response);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, (string)null /* controllerName */, ajaxOptions);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, object routeValues, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return BeginForm(ajaxHelper, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return BeginForm(ajaxHelper, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, string controllerName, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            RouteValueDictionary newValues = new RouteValueDictionary(routeValues);
            Dictionary<string, object> newAttributes = ObjectToCaseSensitiveDictionary(htmlAttributes);
            return BeginForm(ajaxHelper, actionName, controllerName, newValues, ajaxOptions, newAttributes);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return BeginForm(ajaxHelper, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            // get target URL
            string formAction = UrlHelper.GenerateUrl(null, actionName, controllerName, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);
            return FormHelper(ajaxHelper, formAction, ajaxOptions, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, AjaxOptions ajaxOptions) {
            return BeginRouteForm(ajaxHelper, routeName, null /* routeValues */, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, object routeValues, AjaxOptions ajaxOptions) {
            return BeginRouteForm(ajaxHelper, routeName, (object)routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            Dictionary<string, object> newAttributes = ObjectToCaseSensitiveDictionary(htmlAttributes);
            return BeginRouteForm(ajaxHelper, routeName, new RouteValueDictionary(routeValues), ajaxOptions, newAttributes);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return BeginRouteForm(ajaxHelper, routeName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            string formAction = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);
            return FormHelper(ajaxHelper, formAction, ajaxOptions, htmlAttributes);
        }

        private static MvcForm FormHelper(this AjaxHelper ajaxHelper, string formAction, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            TagBuilder builder = new TagBuilder("form");
            builder.MergeAttributes(htmlAttributes);
            builder.MergeAttribute("action", formAction);
            builder.MergeAttribute("method", "post");
            builder.MergeAttribute("onsubmit", GenerateAjaxScript(GetAjaxOptions(ajaxOptions), FormOnSubmitFormat));

            HttpResponseBase response = ajaxHelper.ViewContext.HttpContext.Response;
            response.Write(builder.ToString(TagRenderMode.StartTag));
            return new MvcForm(response);
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, object routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, new RouteValueDictionary(routeValues), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, new RouteValueDictionary(routeValues), ajaxOptions,
                             ObjectToCaseSensitiveDictionary(htmlAttributes));
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions, ObjectToCaseSensitiveDictionary(htmlAttributes));
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions, htmlAttributes);
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, object routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(routeValues), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(routeValues), ajaxOptions,
                             ObjectToCaseSensitiveDictionary(htmlAttributes));
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, routeValues, ajaxOptions, new Dictionary<string, object>());
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);

            return GenerateLink(linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes);
        }

        public static string RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, protocol, hostName, fragment, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);

            return GenerateLink(linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes);
        }

        internal static string InsertionModeToString(InsertionMode insertionMode) {
            switch (insertionMode) {
                case InsertionMode.Replace:
                    return "Sys.Mvc.InsertionMode.replace";
                case InsertionMode.InsertBefore:
                    return "Sys.Mvc.InsertionMode.insertBefore";
                case InsertionMode.InsertAfter:
                    return "Sys.Mvc.InsertionMode.insertAfter";
                default:
                    return ((int)insertionMode).ToString(CultureInfo.InvariantCulture);
            }
        }

        private static Dictionary<string, object> ObjectToCaseSensitiveDictionary(object values) {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.Ordinal);
            if (values != null) {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(values)) {
                    object val = prop.GetValue(values);
                    dict[prop.Name] = val;
                }
            }
            return dict;
        }

        private static string GenerateLink(string linkText, string targetUrl, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            TagBuilder tag = new TagBuilder("a") {
                InnerHtml = HttpUtility.HtmlEncode(linkText)
            };

            tag.MergeAttributes(htmlAttributes);
            tag.MergeAttribute("href", targetUrl);
            tag.MergeAttribute("onclick", GenerateAjaxScript(ajaxOptions, LinkOnClickFormat));

            return tag.ToString(TagRenderMode.Normal);
        }

        private static string GenerateAjaxScript(AjaxOptions ajaxOptions, string scriptFormat) {
            string optionsString = ajaxOptions.ToJavascriptString();
            return String.Format(CultureInfo.InvariantCulture, scriptFormat, optionsString);
        }

        private static AjaxOptions GetAjaxOptions(AjaxOptions ajaxOptions) {
            return (ajaxOptions != null) ? ajaxOptions : new AjaxOptions();
        }
    }
}
