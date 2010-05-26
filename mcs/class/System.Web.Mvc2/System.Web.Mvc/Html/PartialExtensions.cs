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
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            htmlHelper.RenderPartialInternal(partialViewName, viewData, model, writer, ViewEngines.Engines);
            return MvcHtmlString.Create(writer.ToString());
        }
    }
}
