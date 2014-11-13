//------------------------------------------------------------------------------
// <copyright file="HtmlIframe.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    public class HtmlIframe : HtmlContainerControl {

        public HtmlIframe() : base("iframe") {
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        UrlProperty()
        ]
        public string Src {
            get {
                string s = Attributes["src"];
                return s ?? String.Empty;
            }
            set {
                Attributes["src"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Override to process src attribute
         */
        protected override void RenderAttributes(HtmlTextWriter writer) {
            PreProcessRelativeReferenceAttribute(writer, "src");
            base.RenderAttributes(writer);
        }

    }
}
