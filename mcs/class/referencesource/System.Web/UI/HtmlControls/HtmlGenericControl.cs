//------------------------------------------------------------------------------
// <copyright file="HtmlGenericControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlGenericControl.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Web.UI;

/*
 *  A control representing an unknown Html tag.
 */

/// <devdoc>
///    <para>
///       The <see langword='HtmlGenericControl'/> class defines the methods,
///       properties, and events for all HTML Server control tags not represented by a
///       specific class.
///    </para>
/// </devdoc>
    [ConstructorNeedsTag(true)]
    public class HtmlGenericControl : HtmlContainerControl {
        /*
         * Creates a new WebControl
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlGenericControl'/> class with default 
        ///    values.</para>
        /// </devdoc>
        public HtmlGenericControl() : this("span") {
        }

        /*
         *  Creates a new HtmlGenericControl
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlGenericControl'/> class using the specified 
        ///    string.</para>
        /// </devdoc>
        public HtmlGenericControl(string tag) {
            if (tag == null)
                tag = String.Empty;
 
            _tagName = tag;
        }

        /*
        * Property to get name of tag.
        */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the element name of a tag that contains a
        ///       runat="server" attribute/value pair.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        new public string TagName {
            get { return _tagName;}

            set {_tagName = value;}
        }

    }
}
