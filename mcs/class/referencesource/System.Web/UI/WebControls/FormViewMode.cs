//------------------------------------------------------------------------------
// <copyright file="FormViewMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    

    /// <devdoc>
    /// <para>Specifies the FormView edit/view mode.</para>
    /// </devdoc>
    public enum FormViewMode {
        

        /// <devdoc>
        /// <para> 
        /// The control is in read-only mode.</para>
        /// </devdoc>
        ReadOnly = 0,


        /// <devdoc>
        /// <para> 
        /// The control is editing an existing record for update.</para>
        /// </devdoc>
        Edit = 1,


        /// <devdoc>
        /// <para> 
        /// The control is editing a new record for insert.</para>
        /// </devdoc>
        Insert = 2
    }
}

