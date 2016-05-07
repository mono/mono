//------------------------------------------------------------------------------
// <copyright file="HtmlTrack.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlTrack'/>
    ///       class defines the methods, properties, and events
    ///       for the HtmlTrack server control.
    ///       This class provides programmatic access on the server to
    ///       the HTML 5 &lt;Track&gt; element.
    ///    </para>
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(HtmlEmptyTagControlBuilder))
    ]
    public class HtmlTrack : HtmlControl {

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlTrack'/> class.</para>
        /// </devdoc>
        public HtmlTrack()
            : base("track") {
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of and path to the video file to be displayed. This can be an absolute or relative path.
        ///    </para>
        /// </devdoc>
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
            writer.Write(" /");
        }

    }
}
