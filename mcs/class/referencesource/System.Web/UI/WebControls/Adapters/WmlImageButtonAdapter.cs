//------------------------------------------------------------------------------
// <copyright file="WmlImageButtonAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;
    using System.Web.UI.WebControls;

    // REVIEW: Inheritance.  If this inherits from ImageButtonAdapter, there is no way to create a
    // WmlImageAdapter and set the Control property to delegate rendering (base.Render, below). Control is read-only. 
    // Maybe Control should be get/set for this situation.
    public class WmlImageButtonAdapter : WmlImageAdapter {

        protected new ImageButton Control {
            get {
                return (ImageButton)base.Control;
            }
        }

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter) markupWriter;

            string postUrl = Control.PostBackUrl;

            if (!String.IsNullOrEmpty(postUrl)) {
                postUrl = Control.ResolveClientUrl (Control.PostBackUrl);
            }

            // UNDONE: Replace hard coded string indexer with strongly typed capability.
            if (Page != null && Page.Request != null && (String)Page.Request.Browser["supportsImageSubmit"] == "false") {
                writer.EnterStyle(Control.ControlStyle);

                PageAdapter.RenderPostBackEvent(writer, Control.UniqueID /* target */, "EA" /* argument, placeholder only */, Control.SoftkeyLabel, Control.AlternateText, postUrl, null /* accesskey */);
                writer.ExitStyle(Control.ControlStyle);
                return;
            }
            writer.EnterStyle(Control.ControlStyle);
            ((WmlPageAdapter)PageAdapter).RenderBeginPostBack(writer, Control.SoftkeyLabel /* maps to title attribute, Whidbey 10732 */, Control.AccessKey);
            base.Render(writer);
            ((WmlPageAdapter)PageAdapter).RenderEndPostBack(writer, Control.UniqueID, "EA" /* argument, placeholder only */, postUrl);
            writer.ExitStyle(Control.ControlStyle);
        }
    }
}

#endif

