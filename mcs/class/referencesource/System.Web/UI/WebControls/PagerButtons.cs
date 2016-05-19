//------------------------------------------------------------------------------
// <copyright file="PagerButtons.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    ///    <para> Specifies the behavior mode of the Pager item (for accessing various
    ///       pages) within the <see cref='System.Web.UI.WebControls.DataGrid'/> control.</para>
    /// </devdoc>
    public enum PagerButtons {


        /// <devdoc>
        ///    <para> Uses the Previous and Next buttons for
        ///       accessing the previous and next pages.</para>
        /// </devdoc>
        NextPrevious = 0,


        /// <devdoc>
        ///    <para> Uses numbered buttons for accessing pages directly.</para>
        /// </devdoc>
        Numeric = 1,


        /// <devdoc>
        ///    <para> Uses the Previous and Next buttons for
        ///       accessing the previous and next pages, plus first and last page buttons.</para>
        /// </devdoc>
        NextPreviousFirstLast = 2,


        /// <devdoc>
        ///    <para> Uses numbered buttons for accessing pages directly, plus first and last page buttons.</para>
        /// </devdoc>
        NumericFirstLast = 3,

    }
}

