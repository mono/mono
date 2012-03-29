namespace System.Web.Mvc.Ajax {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class AjaxExtensions {
        private const string LinkOnClickFormat = "Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), {0});";
        private const string FormOnClickValue = "Sys.Mvc.AsyncForm.handleClick(this, new Sys.UI.DomEvent(event));";
        private const string FormOnSubmitFormat = "Sys.Mvc.AsyncForm.handleSubmit(this, new Sys.UI.DomEvent(event), {0});";
        private const string _globalizationScript = @"<script type=""text/javascript"" src=""{0}""></script>";

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, ajaxOptions);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, object routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return ActionLink(ajaxHelper, linkText, actionName, (string)null /* controllerName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, null /* values */, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            RouteValueDictionary newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, newValues, ajaxOptions, newAttributes);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(null, actionName, controllerName, routeValues, ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);

            return MvcHtmlString.Create(GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            RouteValueDictionary newValues = new RouteValueDictionary(routeValues);
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return ActionLink(ajaxHelper, linkText, actionName, controllerName, protocol, hostName, fragment, newValues, ajaxOptions, newAttributes);
        }

        public static MvcHtmlString ActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(null /* routeName */, actionName, controllerName, protocol, hostName, fragment, routeValues, ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);

            return MvcHtmlString.Create(GenerateLink(ajaxHelper, linkText, targetUrl, ajaxOptions, htmlAttributes));
        }

        public static MvcForm BeginForm(this AjaxHelper ajaxHelper, AjaxOptions ajaxOptions) {
            string formAction = ajaxHelper.ViewContext.HttpContext.Request.RawUrl;
            return FormHelper(ajaxHelper, formAction, ajaxOptions, new RouteValueDictionary());
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
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
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
            RouteValueDictionary newAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return BeginRouteForm(ajaxHelper, routeName, new RouteValueDictionary(routeValues), ajaxOptions, newAttributes);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return BeginRouteForm(ajaxHelper, routeName, routeValues, ajaxOptions, null /* htmlAttributes */);
        }

        public static MvcForm BeginRouteForm(this AjaxHelper ajaxHelper, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            string formAction = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);
            return FormHelper(ajaxHelper, formAction, ajaxOptions, htmlAttributes);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "You don't want to dispose of this object unless you intend to write to the response")]
        private static MvcForm FormHelper(this AjaxHelper ajaxHelper, string formAction, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            TagBuilder builder = new TagBuilder("form");
            builder.MergeAttributes(htmlAttributes);
            builder.MergeAttribute("action", formAction);
            builder.MergeAttribute("method", "post");

            ajaxOptions = GetAjaxOptions(ajaxOptions);

            if (ajaxHelper.ViewContext.UnobtrusiveJavaScriptEnabled) {
                builder.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            }
            else {
                builder.MergeAttribute("onclick", FormOnClickValue);
                builder.MergeAttribute("onsubmit", GenerateAjaxScript(ajaxOptions, FormOnSubmitFormat));
            }

            if (ajaxHelper.ViewContext.ClientValidationEnabled) {
                // forms must have an ID for client validation
                builder.GenerateId(ajaxHelper.ViewContext.FormIdGenerator());
            }

            ajaxHelper.ViewContext.Writer.Write(builder.ToString(TagRenderMode.StartTag));
            MvcForm theForm = new MvcForm(ajaxHelper.ViewContext);

            if (ajaxHelper.ViewContext.ClientValidationEnabled) {
                ajaxHelper.ViewContext.FormContext.FormId = builder.Attributes["id"];
            }

            return theForm;
        }

        public static MvcHtmlString GlobalizationScript(this AjaxHelper ajaxHelper) {
            return GlobalizationScript(ajaxHelper, CultureInfo.CurrentCulture);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ajaxHelper", Justification = "This is an extension method")]
        public static MvcHtmlString GlobalizationScript(this AjaxHelper ajaxHelper, CultureInfo cultureInfo) {
            return GlobalizationScriptHelper(AjaxHelper.GlobalizationScriptPath, cultureInfo);
        }

        private static MvcHtmlString GlobalizationScriptHelper(string scriptPath, CultureInfo cultureInfo) {
            if (cultureInfo == null) {
                throw new ArgumentNullException("cultureInfo");
            }

            string src = VirtualPathUtility.AppendTrailingSlash(scriptPath) + cultureInfo.Name + ".js";
            string scriptWithCorrectNewLines = _globalizationScript.Replace("\r\n", Environment.NewLine);
            string formatted = String.Format(CultureInfo.InvariantCulture, scriptWithCorrectNewLines, src);

            return MvcHtmlString.Create(formatted);
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, object routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, new RouteValueDictionary(routeValues), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, new RouteValueDictionary(routeValues), ajaxOptions,
                             HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, null /* routeName */, routeValues, ajaxOptions, htmlAttributes);
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(), ajaxOptions, htmlAttributes);
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, object routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(routeValues), ajaxOptions,
                             new Dictionary<string, object>());
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes) {
            return RouteLink(ajaxHelper, linkText, routeName, new RouteValueDictionary(routeValues), ajaxOptions,
                             HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions) {
            return RouteLink(ajaxHelper, linkText, routeName, routeValues, ajaxOptions, new Dictionary<string, object>());
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);

            return MvcHtmlString.Create(GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this AjaxHelper ajaxHelper, string linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "linkText");
            }

            string targetUrl = UrlHelper.GenerateUrl(routeName, null /* actionName */, null /* controllerName */, protocol, hostName, fragment, routeValues ?? new RouteValueDictionary(), ajaxHelper.RouteCollection, ajaxHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);

            return MvcHtmlString.Create(GenerateLink(ajaxHelper, linkText, targetUrl, GetAjaxOptions(ajaxOptions), htmlAttributes));
        }

        private static string GenerateLink(AjaxHelper ajaxHelper, string linkText, string targetUrl, AjaxOptions ajaxOptions, IDictionary<string, object> htmlAttributes) {
            TagBuilder tag = new TagBuilder("a") {
                InnerHtml = HttpUtility.HtmlEncode(linkText)
            };

            tag.MergeAttributes(htmlAttributes);
            tag.MergeAttribute("href", targetUrl);

            if (ajaxHelper.ViewContext.UnobtrusiveJavaScriptEnabled) {
                tag.MergeAttributes(ajaxOptions.ToUnobtrusiveHtmlAttributes());
            }
            else {
                tag.MergeAttribute("onclick", GenerateAjaxScript(ajaxOptions, LinkOnClickFormat));
            }

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
