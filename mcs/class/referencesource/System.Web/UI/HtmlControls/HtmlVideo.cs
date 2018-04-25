//------------------------------------------------------------------------------
// <copyright file="HtmlVideo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlVideo'/>
    ///       class defines the methods, properties, and events
    ///       for the HtmlVideo server control.
    ///       This class provides programmatic access on the server to
    ///       the HTML 5 &lt;video&gt; element.
    ///    </para>
    /// </devdoc>
    public class HtmlVideo : HtmlContainerControl {

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlVideo'/> class.</para>
        /// </devdoc>
        public HtmlVideo() : base("video") {
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        UrlProperty()
        ]
        public string Poster {
            get {
                string s = Attributes["poster"];
                return s ?? String.Empty;
            }
            set {
                Attributes["poster"] = MapStringAttributeToString(value);
            }
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
            PreProcessRelativeReferenceAttribute(writer, "poster");
            base.RenderAttributes(writer);
        }

    }
}
