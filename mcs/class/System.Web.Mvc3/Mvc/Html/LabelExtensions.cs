namespace System.Web.Mvc.Html {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    public static class LabelExtensions {
        public static MvcHtmlString Label(this HtmlHelper html, string expression) {
            return Label(html,
                         expression,
                         null);
        }

        public static MvcHtmlString Label(this HtmlHelper html, string expression, string labelText) {
            return LabelHelper(html,
                               ModelMetadata.FromStringExpression(expression, html.ViewData),
                               expression,
                               labelText);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression) {
            return LabelFor<TModel, TValue>(html, expression, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText) {
            return LabelHelper(html,
                               ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                               ExpressionHelper.GetExpressionText(expression),
                               labelText);
        }

        public static MvcHtmlString LabelForModel(this HtmlHelper html) {
            return LabelForModel(html, null);
        }

        public static MvcHtmlString LabelForModel(this HtmlHelper html, string labelText) {
            return LabelHelper(html, html.ViewData.ModelMetadata, String.Empty, labelText);
        }

        internal static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string labelText = null) {
            string resolvedLabelText = labelText ?? metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(resolvedLabelText)) {
                return MvcHtmlString.Empty;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            tag.SetInnerText(resolvedLabelText);
            return tag.ToMvcHtmlString(TagRenderMode.Normal);
        }
    }
}
