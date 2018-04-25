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

    public class WmlSubstitutionAdapter : SubstitutionAdapter {

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter) markupWriter;
            if(writer.AnalyzeMode) {
                return;
            }
            Control.RenderMarkup(writer);
        }
    }
}

#endif

