namespace System.Web.Mvc.Html {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    public static class DisplayTextExtensions {
        public static MvcHtmlString DisplayText(this HtmlHelper html, string name) {
            return DisplayTextHelper(ModelMetadata.FromStringExpression(name, html.ViewContext.ViewData));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString DisplayTextFor<TModel, TResult>(this HtmlHelper<TModel> html, Expression<Func<TModel, TResult>> expression) {
            return DisplayTextHelper(ModelMetadata.FromLambdaExpression(expression, html.ViewData));
        }

        private static MvcHtmlString DisplayTextHelper(ModelMetadata metadata) {
            return MvcHtmlString.Create(metadata.SimpleDisplayText);
        }
    }
}
