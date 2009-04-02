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

namespace System.Web.Mvc.Html {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class ValidationExtensions {

        private static string _resourceClassKey;

        public static string ResourceClassKey {
            get {
                return _resourceClassKey ?? String.Empty;
            }
            set {
                _resourceClassKey = value;
            }
        }

        private static string GetInvalidPropertyValueResource(HttpContextBase httpContext) {
            string resourceValue = null;
            if (!String.IsNullOrEmpty(ResourceClassKey) && (httpContext != null)) {
                // If the user specified a ResourceClassKey try to load the resource they specified.
                // If the class key is invalid, an exception will be thrown.
                // If the class key is valid but the resource is not found, it returns null, in which
                // case it will fall back to the MVC default error message.
                resourceValue = httpContext.GetGlobalResourceObject(ResourceClassKey, "InvalidPropertyValue", CultureInfo.CurrentUICulture) as string;
            }
            return resourceValue ?? MvcResources.Common_ValueNotValidForProperty;
        }

        private static string GetUserErrorMessageOrDefault(HttpContextBase httpContext, ModelError error, ModelState modelState) {
            if (!String.IsNullOrEmpty(error.ErrorMessage)) {
                return error.ErrorMessage;
            }
            if (modelState == null) {
                return null;
            }

            string attemptedValue = (modelState.Value != null) ? modelState.Value.AttemptedValue : null;
            return String.Format(CultureInfo.CurrentCulture, GetInvalidPropertyValueResource(httpContext), attemptedValue);
        }

        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName) {
            return ValidationMessage(htmlHelper, modelName, (object)null /* htmlAttributes */);
        }

        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName, object htmlAttributes) {
            return ValidationMessage(htmlHelper, modelName, new RouteValueDictionary(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            Justification = "'validationMessage' refers to the message that will be rendered by the ValidationMessage helper.")]
        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName, string validationMessage) {
            return ValidationMessage(htmlHelper, modelName, validationMessage, (object)null /* htmlAttributes */);
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            Justification = "'validationMessage' refers to the message that will be rendered by the ValidationMessage helper.")]
        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName, string validationMessage, object htmlAttributes) {
            return ValidationMessage(htmlHelper, modelName, validationMessage, new RouteValueDictionary(htmlAttributes));
        }

        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName, IDictionary<string, object> htmlAttributes) {
            return ValidationMessage(htmlHelper, modelName, null /* validationMessage */, htmlAttributes);
        }

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            Justification = "'validationMessage' refers to the message that will be rendered by the ValidationMessage helper.")]
        public static string ValidationMessage(this HtmlHelper htmlHelper, string modelName, string validationMessage, IDictionary<string, object> htmlAttributes) {
            if (modelName == null) {
                throw new ArgumentNullException("modelName");
            }

            if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName)) {
                return null;
            }

            ModelState modelState = htmlHelper.ViewData.ModelState[modelName];
            ModelErrorCollection modelErrors = (modelState == null) ? null : modelState.Errors;
            ModelError modelError = ((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors[0];

            if (modelError == null) {
                return null;
            }

            TagBuilder builder = new TagBuilder("span");
            builder.MergeAttributes(htmlAttributes);
            builder.MergeAttribute("class", HtmlHelper.ValidationMessageCssClassName);
            builder.SetInnerText(String.IsNullOrEmpty(validationMessage) ? GetUserErrorMessageOrDefault(htmlHelper.ViewContext.HttpContext, modelError, modelState) : validationMessage);

            return builder.ToString(TagRenderMode.Normal);
        }

        public static string ValidationSummary(this HtmlHelper htmlHelper) {
            return ValidationSummary(htmlHelper, null /* message */);
        }

        public static string ValidationSummary(this HtmlHelper htmlHelper, string message) {
            return ValidationSummary(htmlHelper, message, (object)null /* htmlAttributes */);
        }

        public static string ValidationSummary(this HtmlHelper htmlHelper, string message, object htmlAttributes) {
            return ValidationSummary(htmlHelper, message, new RouteValueDictionary(htmlAttributes));
        }

        public static string ValidationSummary(this HtmlHelper htmlHelper, string message, IDictionary<string, object> htmlAttributes) {
            // Nothing to do if there aren't any errors
            if (htmlHelper.ViewData.ModelState.IsValid) {
                return null;
            }

            string messageSpan;
            if (!String.IsNullOrEmpty(message)) {
                TagBuilder spanTag = new TagBuilder("span");
                spanTag.MergeAttributes(htmlAttributes);
                spanTag.MergeAttribute("class", HtmlHelper.ValidationSummaryCssClassName);
                spanTag.SetInnerText(message);
                messageSpan = spanTag.ToString(TagRenderMode.Normal) + Environment.NewLine;
            }
            else {
                messageSpan = null;
            }

            StringBuilder htmlSummary = new StringBuilder();
            TagBuilder unorderedList = new TagBuilder("ul");
            unorderedList.MergeAttributes(htmlAttributes);
            unorderedList.MergeAttribute("class", HtmlHelper.ValidationSummaryCssClassName);

            foreach (ModelState modelState in htmlHelper.ViewData.ModelState.Values) {
                foreach (ModelError modelError in modelState.Errors) {
                    string errorText = GetUserErrorMessageOrDefault(htmlHelper.ViewContext.HttpContext, modelError, null /* modelState */);
                    if (!String.IsNullOrEmpty(errorText)) {
                        TagBuilder listItem = new TagBuilder("li");
                        listItem.SetInnerText(errorText);
                        htmlSummary.AppendLine(listItem.ToString(TagRenderMode.Normal));
                    }
                }
            }

            unorderedList.InnerHtml = htmlSummary.ToString();

            return messageSpan + unorderedList.ToString(TagRenderMode.Normal);
        }

    }
}
