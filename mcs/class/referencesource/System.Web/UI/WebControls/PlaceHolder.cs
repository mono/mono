//------------------------------------------------------------------------------
// <copyright file="PlaceHolder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

using System;
using System.ComponentModel;
using System.Web.UI;

    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.PlaceHolder'/> control.</para>
    /// </devdoc>
    public class PlaceHolderControlBuilder : ControlBuilder {
 

       /// <internalonly/>
       /// <devdoc>
       ///    <para>Specifies whether white space literals are allowed.</para>
       /// </devdoc>
       public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }

// The reason we define this empty override in the WebControls namespace is
// to expose it as a control that can be used on a page (ASURT 51116)
// E.g. <asp:placeholder runat=server id=ph1/>

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
    [
    ControlBuilderAttribute(typeof(PlaceHolderControlBuilder))
    ]
    public class PlaceHolder : Control {

        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }
    }
}
