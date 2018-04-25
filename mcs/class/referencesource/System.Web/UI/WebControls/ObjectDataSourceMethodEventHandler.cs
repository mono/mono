//------------------------------------------------------------------------------
// <copyright file="ObjectDataSourceMethodEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    /// Represents a method that handles the Deleting, Inserting,
    /// Selecting, or Updating events of ObjectDataSource.
    /// </devdoc>
    public delegate void ObjectDataSourceMethodEventHandler(object sender, ObjectDataSourceMethodEventArgs e);
}

