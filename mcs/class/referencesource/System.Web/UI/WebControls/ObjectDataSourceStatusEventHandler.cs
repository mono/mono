//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceStatusEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    /// Represents a method that handles the Deleted, Inserted,
    /// Selected, or Updated events of ObjectDataSource.
    /// </devdoc>
    public delegate void ObjectDataSourceStatusEventHandler(object sender, ObjectDataSourceStatusEventArgs e);
}

