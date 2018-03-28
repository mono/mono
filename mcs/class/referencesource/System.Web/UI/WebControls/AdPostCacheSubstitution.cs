//------------------------------------------------------------------------------
// <copyright file="AdPostCacheSubstitution.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * The class is used internally to handle post-cache substitution mechanism in
 * AdRotator.
 *
 * Copyright (c) 2002 Microsoft Corporation
 */
namespace System.Web.UI.WebControls {
    using System.Globalization;
    using System.IO;
    using System.Web.Util;

    internal class AdPostCacheSubstitution {
        private AdRotator _adRotatorHelper;

        private AdPostCacheSubstitution() {}

        internal AdPostCacheSubstitution(AdRotator adRotator) {
            _adRotatorHelper = new AdRotator();
            _adRotatorHelper.CopyFrom(adRotator);
            _adRotatorHelper.IsPostCacheAdHelper = true;
            _adRotatorHelper.Page = new Page();
        }

        internal void RegisterPostCacheCallBack(HttpContext context,
                                                Page page,
                                                HtmlTextWriter writer) {
            // Assumption: called from AdRotator's Render phase

            HttpResponseSubstitutionCallback callback = new HttpResponseSubstitutionCallback(Render);
            context.Response.WriteSubstitution(callback);
        }

        internal string Render(HttpContext context) {
            // 


            Debug.Assert(_adRotatorHelper != null && _adRotatorHelper.Page != null);

            // In PostCache Substitution, we use a string writer to return the markup.
            StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture);
            HtmlTextWriter htmlWriter = _adRotatorHelper.Page.CreateHtmlTextWriter(stringWriter);
            Debug.Assert(htmlWriter != null);
            _adRotatorHelper.RenderControl(htmlWriter);

            // Dump the content out as needed for post-cache substitution.
            return stringWriter.ToString();
        }
    }
}
