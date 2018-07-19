//------------------------------------------------------------------------------
// <copyright file="HtmlArea.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ControlBuilderAttribute(typeof(HtmlEmptyTagControlBuilder))]
    public class HtmlArea : HtmlControl {

        public HtmlArea() : base("area") {
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        UrlProperty()
        ]
        public string Href {
            get {
                string s = Attributes["href"];
                return s ?? String.Empty;
            }
            set {
                Attributes["href"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Override to process href attribute
         */
        protected override void RenderAttributes(HtmlTextWriter writer) {
            PreProcessRelativeReferenceAttribute(writer, "href");
            base.RenderAttributes(writer);
            writer.Write(" /");
        }

    }
}
