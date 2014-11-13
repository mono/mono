//------------------------------------------------------------------------------
// <copyright file="WebControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.Adapters {

    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;

    // Provides adaptive rendering for a web control.
    public class WebControlAdapter : ControlAdapter {
        // Returns a strongly typed control instance.
        protected new WebControl Control {
            get {
                return (WebControl)base.Control;
            }
        }

        /// Indicates whether the associated WebControl is enabled
        /// taking into account the cascading effect of the enabled property.
        protected bool IsEnabled {
            get {
                return Control.IsEnabled;
            }
        }

        protected virtual void RenderBeginTag(HtmlTextWriter writer) {
            Control.RenderBeginTag(writer); 
        }

        protected virtual void RenderEndTag(HtmlTextWriter writer) {
            Control.RenderEndTag(writer); 
        }

        protected virtual void RenderContents(HtmlTextWriter writer) {
            Control.RenderContents(writer); 
        }

        protected internal override void Render(HtmlTextWriter writer) {
            RenderBeginTag(writer);
            RenderContents(writer);
            RenderEndTag(writer);
        }
    }
}
