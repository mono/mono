//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceSelectingEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;

    /// <devdoc>
    /// Represents a method that handles the Selecting events of ObjectDataSource.
    /// One of these events is for the SelectCount operation, the other is for
    /// the Select operation.
    /// </devdoc>
    public delegate void ObjectDataSourceSelectingEventHandler(object sender, ObjectDataSourceSelectingEventArgs e);
}

