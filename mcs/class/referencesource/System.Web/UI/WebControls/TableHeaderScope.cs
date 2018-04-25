//------------------------------------------------------------------------------
// <copyright file="TableHeaderScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    
    using System;
    

    /// <devdoc>
    ///    <para>
    ///        Used for table header cell scope attribute and property
    ///    </para>
    /// </devdoc>
    public enum TableHeaderScope {


        /// <devdoc>
        ///    <para>
        ///       Property is not set.
        ///    </para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        ///    <para>
        ///        Row scope   
        ///    </para>
        /// </devdoc>
        Row = 1,


        /// <devdoc>
        ///    <para>
        ///        Column scope
        ///    </para>
        /// </devdoc>
        Column = 2
    }
}
