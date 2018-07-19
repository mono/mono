//------------------------------------------------------------------------------
// <copyright file="PagerPosition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    ///    <para>Specifies the position of the Pager item
    ///       (for accessing various pages) within the <see cref='System.Web.UI.WebControls.DataGrid'/> control.</para>
    /// </devdoc>
    public enum PagerPosition {


        /// <devdoc>
        /// <para>Positioned at the bottom of the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
        /// </devdoc>
        Bottom = 0,


        /// <devdoc>
        /// <para>Positioned at the top of the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
        /// </devdoc>
        Top = 1,


        /// <devdoc>
        /// <para>Positioned at the top and the bottom of the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
        /// </devdoc>
        TopAndBottom = 2
    }
}

