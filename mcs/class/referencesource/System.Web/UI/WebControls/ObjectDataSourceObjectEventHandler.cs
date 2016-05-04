//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceObjectEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    /// Represents a method that handles the ObjectCreating and ObjectCreated events of ObjectDataSource.
    /// </devdoc>
    public delegate void ObjectDataSourceObjectEventHandler(object sender, ObjectDataSourceEventArgs e);
}

