//------------------------------------------------------------------------------
// <copyright file="HtmlEmptyTagControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Web.UI;


    /// <devdoc>
    /// Used as ControlBuilder for controls that do not have a body or end
    /// tag, for example, INPUT and IMG.
    /// </devdoc>
    public sealed class HtmlEmptyTagControlBuilder : ControlBuilder {


        // <devdoc>
        // Indicate that the control does not have a body or end tag.
        // </devdoc>
        public override bool HasBody() {
            return false;
        }
    }
}
