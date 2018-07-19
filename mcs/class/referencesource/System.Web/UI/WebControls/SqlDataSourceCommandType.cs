//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceCommandType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// Specifies the type of the command, either text or a stored procedure.
    /// </devdoc>
    public enum SqlDataSourceCommandType {
        /// <devdoc>
        /// The command is text (e.g. select * from authors).
        /// </devdoc>
        Text = 0,

        /// <devdoc>
        /// The command is a stored procedure (e.g. GetAuthorsByState).
        /// </devdoc>
        StoredProcedure = 1,
    }
}

