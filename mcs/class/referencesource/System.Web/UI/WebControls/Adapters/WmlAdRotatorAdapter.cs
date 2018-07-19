//------------------------------------------------------------------------------
// <copyright file="WmlListControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;

    public class WmlAdRotatorAdapter : AdRotatorAdapter {
        private bool _firstTimeRender = true;
        private bool _wmlTopOfForm;

        protected internal override void Render(HtmlTextWriter writer) {
            WmlTextWriter wmlWriter = (WmlTextWriter) writer;
            if (wmlWriter.AnalyzeMode) {
                return;
            }

            if (Control.DoPostCacheSubstitutionAsNeeded(writer)) {
                return;
            }

            // This is to work around the issue that WmlTextWriter has its
            // state info for rendering block level elements, such as <p> tag.
            // We keep the state info for subsequent PostCache Render call below.
            //
            // e.g. If the ad is at the beginning of the form, we need to
            // call the method below to explicitly write out a <p> tag for
            // a valid WML output.
            if (_wmlTopOfForm) {
                wmlWriter.BeginRender();
            }
            if (_firstTimeRender) {
                _wmlTopOfForm = wmlWriter.TopOfForm;
                _firstTimeRender = false;
            }

            RenderHyperLinkAsAd(writer);
        }
    }
}

#endif 

