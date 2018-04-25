//------------------------------------------------------------------------------
// <copyright file="PagerMode.cs" company="Microsoft">
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
    public enum PagerMode {


        /// <devdoc>
        ///    <para> Uses the Previous and Next buttons for
        ///       accessing the previous and next pages.</para>
        /// </devdoc>
        NextPrev = 0,


        /// <devdoc>
        ///    <para> Uses numbered buttons for accessing pages directly.</para>
        /// </devdoc>
        NumericPages = 1
    }
}

