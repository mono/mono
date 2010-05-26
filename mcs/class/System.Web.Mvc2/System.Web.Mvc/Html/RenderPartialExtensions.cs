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
    public static class RenderPartialExtensions {
        // Renders the partial view with the parent's view data and model
        public static void RenderPartial(this HtmlHelper htmlHelper, string partialViewName) {
            htmlHelper.RenderPartialInternal(partialViewName, htmlHelper.ViewData, null /* model */, htmlHelper.ViewContext.Writer, ViewEngines.Engines);
        }

        // Renders the partial view with the given view data and, implicitly, the given view data's model
        public static void RenderPartial(this HtmlHelper htmlHelper, string partialViewName, ViewDataDictionary viewData) {
            htmlHelper.RenderPartialInternal(partialViewName, viewData, null /* model */, htmlHelper.ViewContext.Writer, ViewEngines.Engines);
        }

        // Renders the partial view with an empty view data and the given model
        public static void RenderPartial(this HtmlHelper htmlHelper, string partialViewName, object model) {
            htmlHelper.RenderPartialInternal(partialViewName, htmlHelper.ViewData, model, htmlHelper.ViewContext.Writer, ViewEngines.Engines);
        }

        // Renders the partial view with a copy of the given view data plus the given model
        public static void RenderPartial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary viewData) {
            htmlHelper.RenderPartialInternal(partialViewName, viewData, model, htmlHelper.ViewContext.Writer, ViewEngines.Engines);
        }
    }
}