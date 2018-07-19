//------------------------------------------------------------------------------
// <copyright file="ListSelectionMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    ///    <para>Specifies the selection behavior of list controls.</para>
    /// </devdoc>
    public enum ListSelectionMode {
    

        /// <devdoc>
        ///    <para>The list control allows only one item to be selected at one time.</para>
        /// </devdoc>
        Single = 0,
    

        /// <devdoc>
        ///    <para> The list control allows multiple items to be selected at one time.</para>
        /// </devdoc>
        Multiple = 1
    }
}

