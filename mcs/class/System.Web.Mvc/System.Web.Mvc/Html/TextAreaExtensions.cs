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
    using System.Globalization;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class TextAreaExtensions {
        // These values are similar to the defaults used by WebForms
        // when using <asp:TextBox TextMode="MultiLine"> without specifying
        // the Rows and Columns attributes.
        private const int TextAreaRows = 2;
        private const int TextAreaColumns = 20;

        public static string TextArea(this HtmlHelper htmlHelper, string name) {
            return TextArea(htmlHelper, name, (object)null /* htmlAttributes */);
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, object htmlAttributes) {
            return TextArea(htmlHelper, name, new RouteValueDictionary(htmlAttributes));
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes) {
            // Add implicit parameters
            Dictionary<string, object> implicitAttributes = new Dictionary<string, object>();
            implicitAttributes.Add("rows", TextAreaRows.ToString(CultureInfo.InvariantCulture));
            implicitAttributes.Add("cols", TextAreaColumns.ToString(CultureInfo.InvariantCulture));
            return TextAreaHelper(htmlHelper, name, true /* useViewData */, null /* value */, implicitAttributes, null /* explicitParameters */, htmlAttributes);
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, string value) {
            return TextArea(htmlHelper, name, value, (object) null /* htmlAttributes */);
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, string value, object htmlAttributes) {
            return TextArea(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, string value, IDictionary<string, object> htmlAttributes) {
            // Add implicit parameters
            Dictionary<string, object> implicitAttributes = new Dictionary<string, object>();
            implicitAttributes.Add("rows", TextAreaRows.ToString(CultureInfo.InvariantCulture));
            implicitAttributes.Add("cols", TextAreaColumns.ToString(CultureInfo.InvariantCulture));
            return TextAreaHelper(htmlHelper, name, (value == null) /* useViewData */, value, implicitAttributes, null /* explicitParameters */, htmlAttributes);
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, string value, int rows, int columns, object htmlAttributes) {
            return TextArea(htmlHelper, name, value, rows, columns, new RouteValueDictionary(htmlAttributes));
        }

        public static string TextArea(this HtmlHelper htmlHelper, string name, string value, int rows, int columns, IDictionary<string, object> htmlAttributes) {
            if (rows <= 0) {
                throw new ArgumentOutOfRangeException("rows", MvcResources.HtmlHelper_TextAreaParameterOutOfRange);
            }
            if (columns <= 0) {
                throw new ArgumentOutOfRangeException("columns", MvcResources.HtmlHelper_TextAreaParameterOutOfRange);
            }

            Dictionary<string, object> explicitParameters = new Dictionary<string, object>();
            explicitParameters.Add("rows", rows.ToString(CultureInfo.InvariantCulture));
            explicitParameters.Add("cols", columns.ToString(CultureInfo.InvariantCulture));
            return TextAreaHelper(htmlHelper, name, (value == null) /* useViewData */, value, null /* implictAttributes */, explicitParameters, htmlAttributes);
        }

        private static string TextAreaHelper(this HtmlHelper htmlHelper, string name, bool useViewData, string value, IDictionary<string, object> implicitAttributes, IDictionary<string, object> explicitParameters, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            TagBuilder tagBuilder = new TagBuilder("textarea");
            // Add implicit attributes.
            tagBuilder.MergeAttributes(implicitAttributes);
            tagBuilder.GenerateId(name);
            // Merge htmlAttributes.
            tagBuilder.MergeAttributes(htmlAttributes, true);
            // Override all the attributes with explicit parameters.
            tagBuilder.MergeAttributes(explicitParameters, true);
            tagBuilder.MergeAttribute("name", name, true);

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState)) {
                if (modelState.Errors.Count > 0) {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            // The first newline is always trimmed when a TextArea is rendered, so we add an extra one
            // in case the value being rendered is something like "\r\nHello".
            // The attempted value receives precedence over the explicitly supplied value parameter.
            string attemptedValue = (string)htmlHelper.GetModelStateValue(name, typeof(string));
            tagBuilder.SetInnerText(Environment.NewLine + (attemptedValue ?? ((useViewData) ? htmlHelper.EvalString(name) : value)));
            return tagBuilder.ToString(TagRenderMode.Normal);
        }
    }
}
