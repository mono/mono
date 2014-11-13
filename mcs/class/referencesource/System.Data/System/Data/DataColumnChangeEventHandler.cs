//------------------------------------------------------------------------------
// <copyright file="DataColumnChangeEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    // Represents the method that will handle the the <see cref='System.Data.DataTable.ColumnChanging'/> event.</para>
    public delegate void DataColumnChangeEventHandler(object sender, DataColumnChangeEventArgs e);
}
