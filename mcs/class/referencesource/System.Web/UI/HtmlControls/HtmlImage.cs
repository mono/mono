//------------------------------------------------------------------------------
// <copyright file="HtmlImage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlImage.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


/// <devdoc>
///    <para>
///       The <see langword='HtmlImage'/>
///       class defines the methods, properties, and events
///       for the HtmlImage server control.
///       This class provides programmatic access on the server to
///       the HTML &lt;img&gt; element.
///    </para>
/// </devdoc>
    [
    ControlBuilderAttribute(typeof(HtmlEmptyTagControlBuilder))
    ]
    public class HtmlImage : HtmlControl {

        /*
         *  Creates an intrinsic Html IMG control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlImage'/> class.</para>
        /// </devdoc>
        public HtmlImage() : base("img") {
        }

        /*
         * Alt property
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the alternative caption that the
        ///       browser displays if image is either unavailable or has not been downloaded yet.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Alt {
            get {
                string s = Attributes["alt"];
                return((s != null) ? s : String.Empty);
            }
            set {
                // Dev11 #382229: <img> elements must have 'alt' attributes to pass W3 validation.
                // An empty string is an acceptable default value for these attributes.
                if (RenderingCompatibility >= VersionUtil.Framework45) {
                    Attributes["alt"] = value;
                }
                else {
                    Attributes["alt"] = MapStringAttributeToString(value);
                }
            }
        }

        /*
         * Align property
         */

        /// <devdoc>
        ///    <para>Gets or sets the alignment of the image with
        ///       surrounding text.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Align {
            get {
                string s = Attributes["align"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["align"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Border property, size of border in pixels.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the width of image border, in pixels.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(0),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Border {
            get {
                string s = Attributes["border"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["border"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Height property
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the height of the image. By default, this is expressed in
        ///       pixels,
        ///       but can be a expressed as a percentage.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(100),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Height {
            get {
                string s = Attributes["height"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["height"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Src property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of and path to the
        ///       image file to be displayed. This can be an absolute or
        ///       relative path.
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
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["src"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Width property
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the width of the image. By default, this is
        ///       expressed in pixels,
        ///       but can be a expressed as a percentage.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(100),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Width {
            get {
                string s = Attributes["width"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["width"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Override to render unique name attribute.
         * The name attribute is owned by the framework.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            PreProcessRelativeReferenceAttribute(writer, "src");

            base.RenderAttributes(writer);
            writer.Write(" /");
        }

    }
}
