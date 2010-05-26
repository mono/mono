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
    using System.Linq.Expressions;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class TextAreaExtensions {
        // These values are similar to the defaults used by WebForms
        // when using <asp:TextBox TextMode="MultiLine"> without specifying
        // the Rows and Columns attributes.
        private const int TextAreaRows = 2;
        private const int TextAreaColumns = 20;
        private static Dictionary<string, object> implicitRowsAndColumns = new Dictionary<string, object> {
            { "rows", TextAreaRows.ToString(CultureInfo.InvariantCulture) },
            { "cols", TextAreaColumns.ToString(CultureInfo.InvariantCulture) },
        };

        private static Dictionary<string, object> GetRowsAndColumnsDictionary(int rows, int columns) {
            if (rows < 0) {
                throw new ArgumentOutOfRangeException("rows", MvcResources.HtmlHelper_TextAreaParameterOutOfRange);
            }
            if (columns < 0) {
                throw new ArgumentOutOfRangeException("columns", MvcResources.HtmlHelper_TextAreaParameterOutOfRange);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            if (rows > 0) {
                result.Add("rows", rows.ToString(CultureInfo.InvariantCulture));
            }
            if (columns > 0) {
                result.Add("cols", columns.ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name) {
            return TextArea(htmlHelper, name, null /* value */, null /* htmlAttributes */);
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, object htmlAttributes) {
            return TextArea(htmlHelper, name, null /* value */, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes) {
            return TextArea(htmlHelper, name, null /* value */, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, string value) {
            return TextArea(htmlHelper, name, value, null /* htmlAttributes */);
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, string value, object htmlAttributes) {
            return TextArea(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, string value, IDictionary<string, object> htmlAttributes) {
            ModelMetadata metadata = ModelMetadata.FromStringExpression(name, htmlHelper.ViewContext.ViewData);
            if (value != null) {
                metadata.Model = value;
            }

            return TextAreaHelper(htmlHelper, metadata, name, implicitRowsAndColumns, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, string value, int rows, int columns, object htmlAttributes) {
            return TextArea(htmlHelper, name, value, rows, columns, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString TextArea(this HtmlHelper htmlHelper, string name, string value, int rows, int columns, IDictionary<string, object> htmlAttributes) {
            ModelMetadata metadata = ModelMetadata.FromStringExpression(name, htmlHelper.ViewContext.ViewData);
            if (value != null) {
                metadata.Model = value;
            }

            return TextAreaHelper(htmlHelper, metadata, name, GetRowsAndColumnsDictionary(rows, columns), htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression) {
            return TextAreaFor(htmlHelper, expression, (IDictionary<string, object>)null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes) {
            return TextAreaFor(htmlHelper, expression, new RouteValueDictionary(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }

            return TextAreaHelper(htmlHelper,
                                  ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData),
                                  ExpressionHelper.GetExpressionText(expression),
                                  implicitRowsAndColumns,
                                  htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int rows, int columns, object htmlAttributes) {
            return TextAreaFor(htmlHelper, expression, rows, columns, new RouteValueDictionary(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int rows, int columns, IDictionary<string, object> htmlAttributes) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }

            return TextAreaHelper(htmlHelper,
                                  ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData),
                                  ExpressionHelper.GetExpressionText(expression),
                                  GetRowsAndColumnsDictionary(rows, columns),
                                  htmlAttributes);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "If this fails, it is because the string-based version had an empty 'name' parameter")]
        private static MvcHtmlString TextAreaHelper(HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression, IDictionary<string, object> rowsAndColumns, IDictionary<string, object> htmlAttributes) {
            string name = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            TagBuilder tagBuilder = new TagBuilder("textarea");
            tagBuilder.GenerateId(name);
            tagBuilder.MergeAttributes(htmlAttributes, true);
            tagBuilder.MergeAttributes(rowsAndColumns, rowsAndColumns != implicitRowsAndColumns);  // Only force explicit rows/cols
            tagBuilder.MergeAttribute("name", name, true);

            // If there are any errors for a named field, we add the CSS attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState) && modelState.Errors.Count > 0) {
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            string value;
            if (modelState != null && modelState.Value != null) {
                value = modelState.Value.AttemptedValue;
            }
            else if (modelMetadata.Model != null) {
                value = modelMetadata.Model.ToString();
            }
            else {
                value = String.Empty;
            }

            // The first newline is always trimmed when a TextArea is rendered, so we add an extra one
            // in case the value being rendered is something like "\r\nHello".
            tagBuilder.SetInnerText(Environment.NewLine + value);

            return tagBuilder.ToMvcHtmlString(TagRenderMode.Normal);
        }
    }
}
