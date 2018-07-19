//------------------------------------------------------------------------------
// <copyright file="ValidationSummaryDisplayMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    

    /// <devdoc>
    ///    <para>Specifies the validation summary display mode to be 
    ///       used by the <see cref='System.Web.UI.WebControls.ValidationSummary'/> control.</para>
    /// </devdoc>
    public enum ValidationSummaryDisplayMode {

        /// <devdoc>
        ///    Specifies that each error message is
        ///    displayed on its own line.
        /// </devdoc>
        List = 0,

        /// <devdoc>
        ///    <para>Specifies that each error message is
        ///       displayed on its own bulleted line.</para>
        /// </devdoc>
        BulletList = 1,

        /// <devdoc>
        ///    Specifies that all error messages are
        ///    displayed together in a single paragraph, separated from each other by two
        ///    spaces.
        /// </devdoc>
        SingleParagraph = 2
    }
}

