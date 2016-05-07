//------------------------------------------------------------------------------
// <copyright file="ValidatorDisplay.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    

    /// <devdoc>
    ///    <para>Specifies the display behavior of a validation control.</para>
    /// </devdoc>
    public enum ValidatorDisplay {

        /// <devdoc>
        ///    <para> The validation contents are never displayed
        ///       inline.</para>
        ///    <para> This is 
        ///       used so that the error message is only displayed in a <see cref='System.Web.UI.WebControls.ValidationSummary'/>
        ///       control.</para>
        /// </devdoc>
        None = 0,

        /// <devdoc>
        ///    <para>The validator contents are displayed inline if validation fails. In addition, the 
        ///       validator is part of a page layout even when it is hidden.</para>
        ///    <para>The layout of the page does not change when the validator becomes visible. 
        ///       However, multiple validators for the same input control must occupy different
        ///       physical locations on the page.</para>
        /// </devdoc>
        Static = 1,

        /// <devdoc>
        ///    <para>The validator contents are displayed inline if validation fails. In 
        ///       addition, the validator only takes up space on the page when it is
        ///       visible.</para>
        ///    <para>This allows multiple validators to occupy the same physical location on the 
        ///       page when they become visible. In order to avoid the page layout changing when a
        ///       validator becomes visible, the HTML element containing the validator must be
        ///       sized large enough to accommodate the maximum size of the validator.</para>
        /// </devdoc>
        Dynamic = 2
    }        
}


