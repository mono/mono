//------------------------------------------------------------------------------
// <copyright file="WmlButtonAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.WebControls;

    public class WmlButtonAdapter : ButtonAdapter {

        protected internal override void Render(HtmlTextWriter writer) {
            RenderAsPostBackLink(writer);
        }
        // renders the button as a postback link
        protected override void RenderAsPostBackLink(HtmlTextWriter writer) {
            String text = Control.Text;
            String softkeyLabel = Control.SoftkeyLabel;

            string postUrl = Control.PostBackUrl;
            if (!String.IsNullOrEmpty(postUrl)) {
                postUrl = ((WebControl)Control).ResolveClientUrl(Control.PostBackUrl);
            }

            writer.EnterStyle(((WebControl)Control).ControlStyle);
            // Do not encode LinkButton Text for V1 compatibility.
            if (!(Control is LinkButton) ){
                text = text.Replace("$", "$$");
                text = HttpUtility.HtmlEncode(text);
                softkeyLabel = softkeyLabel.Replace("$", "$$");
                softkeyLabel = HttpUtility.HtmlEncode(softkeyLabel);
            }
            PageAdapter.RenderPostBackEvent(writer, ((Control)base.Control).UniqueID, null /* argument */, softkeyLabel, text, postUrl, null /* accesskey */);
            writer.ExitStyle(((WebControl)Control).ControlStyle);
        }
    }
}

#endif

