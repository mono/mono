namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public class HtmlHelper {
        public static readonly string ValidationInputCssClassName = "input-validation-error";
        public static readonly string ValidationInputValidCssClassName = "input-validation-valid";
        public static readonly string ValidationMessageCssClassName = "field-validation-error";
        public static readonly string ValidationMessageValidCssClassName = "field-validation-valid";
        public static readonly string ValidationSummaryCssClassName = "validation-summary-errors";
        public static readonly string ValidationSummaryValidCssClassName = "validation-summary-valid";

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
            ClientValidationRuleFactory = (name, metadata) => ModelValidatorProviders.Providers.GetValidators(metadata ?? ModelMetadata.FromStringExpression(name, ViewData), ViewContext).SelectMany(v => v.GetClientValidationRules());
        }

        public static bool ClientValidationEnabled {
            get {
                return ViewContext.GetClientValidationEnabled();
            }
            set {
                ViewContext.SetClientValidationEnabled(value);
            }
        }

        public static string IdAttributeDotReplacement {
            get {
                return System.Web.WebPages.Html.HtmlHelper.IdAttributeDotReplacement;
            }
            set {
                System.Web.WebPages.Html.HtmlHelper.IdAttributeDotReplacement = value;
            }
        }

        internal Func<string, ModelMetadata, IEnumerable<ModelClientValidationRule>> ClientValidationRuleFactory {
            get;
            set;
        }

        public RouteCollection RouteCollection {
            get;
            private set;
        }

        public static bool UnobtrusiveJavaScriptEnabled {
            get {
                return ViewContext.GetUnobtrusiveJavaScriptEnabled();
            }
            set {
                ViewContext.SetUnobtrusiveJavaScriptEnabled(value);
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

        public static RouteValueDictionary AnonymousObjectToHtmlAttributes(object htmlAttributes) {
            RouteValueDictionary result = new RouteValueDictionary();

            if (htmlAttributes != null) {
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(htmlAttributes)) {
                    result.Add(property.Name.Replace('_', '-'), property.GetValue(htmlAttributes));
                }
            }

            return result;
        }

        public MvcHtmlString AntiForgeryToken() {
            return AntiForgeryToken(salt: null);
        }

        public MvcHtmlString AntiForgeryToken(string salt) {
            return AntiForgeryToken(salt, domain: null, path: null);
        }

        public MvcHtmlString AntiForgeryToken(string salt, string domain, string path) {
            //Disabled to compile MVC3 with the newer System.Web.WebPages helpers
            //return new MvcHtmlString(AntiForgery.GetHtml(ViewContext.HttpContext, salt, domain, path).ToString());
            return new MvcHtmlString(AntiForgery.GetHtml().ToString());
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string AttributeEncode(string value) {
            return (!String.IsNullOrEmpty(value)) ? HttpUtility.HtmlAttributeEncode(value) : String.Empty;
        }

        public string AttributeEncode(object value) {
            return AttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public void EnableClientValidation() {
            EnableClientValidation(enabled: true);
        }

        public void EnableClientValidation(bool enabled) {
            ViewContext.ClientValidationEnabled = enabled;
        }

        public void EnableUnobtrusiveJavaScript() {
            EnableUnobtrusiveJavaScript(enabled: true);
        }

        public void EnableUnobtrusiveJavaScript(bool enabled) {
            ViewContext.UnobtrusiveJavaScriptEnabled = enabled;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value) {
            return (!String.IsNullOrEmpty(value)) ? HttpUtility.HtmlEncode(value) : String.Empty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(object value) {
            return value != null ? HttpUtility.HtmlEncode(value) : String.Empty;
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

            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
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

            // TagBuilder.CreateSanitizedId returns null for empty strings, return String.Empty instead to avoid breaking change
            if (name.Length == 0) {
                return String.Empty;
            }

            return TagBuilder.CreateSanitizedId(name, idAttributeDotReplacement);
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

        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name) {
            return GetUnobtrusiveValidationAttributes(name, metadata: null);
        }

        // Only render attributes if unobtrusive client-side validation is enabled, and then only if we've
        // never rendered validation for a field with this name in this form. Also, if there's no form context,
        // then we can't render the attributes (we'd have no <form> to attach them to).
        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name, ModelMetadata metadata) {
            Dictionary<string, object> results = new Dictionary<string, object>();

            // The ordering of these 3 checks (and the early exits) is for performance reasons.
            if (!ViewContext.UnobtrusiveJavaScriptEnabled) {
                return results;
            }

            FormContext formContext = ViewContext.GetFormContextForClientValidation();
            if (formContext == null) {
                return results;
            }

            string fullName = ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (formContext.RenderedField(fullName)) {
                return results;
            }

            formContext.RenderedField(fullName, true);

            IEnumerable<ModelClientValidationRule> clientRules = ClientValidationRuleFactory(name, metadata);
            bool renderedRules = false;

            foreach (ModelClientValidationRule rule in clientRules) {
                renderedRules = true;
                string ruleName = "data-val-" + rule.ValidationType;

                ValidateUnobtrusiveValidationRule(rule, results, ruleName);

                results.Add(ruleName, HttpUtility.HtmlEncode(rule.ErrorMessage ?? String.Empty));
                ruleName += "-";

                foreach (var kvp in rule.ValidationParameters) {
                    results.Add(ruleName + kvp.Key, kvp.Value ?? String.Empty);
                }
            }

            if (renderedRules) {
                results.Add("data-val", "true");
            }

            return results;
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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
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

        /// <summary>
        /// Wraps HTML markup in an IHtmlString, which will enable HTML markup to be
        /// rendered to the output without getting HTML encoded.
        /// </summary>
        /// <param name="value">HTML markup string.</param>
        /// <returns>An IHtmlString that represents HTML markup.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "For consistency, all helpers are instance methods.")]
        public IHtmlString Raw(string value) {
            return new HtmlString(value);
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

        private static void ValidateUnobtrusiveValidationRule(ModelClientValidationRule rule, Dictionary<string, object> resultsDictionary, string dictionaryKey) {
            if (String.IsNullOrWhiteSpace(rule.ValidationType)) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.HtmlHelper_ValidationTypeCannotBeEmpty,
                        rule.GetType().FullName
                    )
                );
            }

            if (resultsDictionary.ContainsKey(dictionaryKey)) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.HtmlHelper_ValidationTypeMustBeUnique,
                        rule.ValidationType
                    )
                );
            }

            if (rule.ValidationType.Any(c => !Char.IsLower(c))) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.HtmlHelper_ValidationTypeMustBeLegal,
                        rule.ValidationType,
                        rule.GetType().FullName
                    )
                );
            }

            foreach (var key in rule.ValidationParameters.Keys) {
                if (String.IsNullOrWhiteSpace(key)) {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            MvcResources.HtmlHelper_ValidationParameterCannotBeEmpty,
                            rule.GetType().FullName
                        )
                    );
                }

                if (!Char.IsLower(key.First()) || key.Any(c => !Char.IsLower(c) && !Char.IsDigit(c))) {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            MvcResources.HtmlHelper_ValidationParameterMustBeLegal,
                            key,
                            rule.GetType().FullName
                        )
                    );
                }
            }
        }
    }
}
