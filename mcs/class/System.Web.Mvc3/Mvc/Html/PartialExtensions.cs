namespace System.Web.Mvc.Html {
    using System.Globalization;
    using System.IO;

    public static class PartialExtensions {
        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName) {
            return Partial(htmlHelper, partialViewName, null /* model */, htmlHelper.ViewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData) {
            return Partial(htmlHelper, partialViewName, null /* model */, viewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model) {
            return Partial(htmlHelper, partialViewName, model, htmlHelper.ViewData);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData) {
            using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture)) {
                htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines);
                return MvcHtmlString.Create(writer.ToString());
            }
        }
    }
}
