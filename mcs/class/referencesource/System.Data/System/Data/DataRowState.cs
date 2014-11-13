//------------------------------------------------------------------------------
// <copyright file="DataRowState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    // Gets the state of a DataRow object.
    [ Flags ]
    public enum DataRowState {
        // DataViewRowState.None = 00000000;
        // The row has been created but is not part of any DataRowCollection.
        // A DataRow is in this state immediately after it has been created and 
        // before it is added to a collection, or if it has been removed from a collection.
        Detached  = 0x00000001,
        // The row has not changed since AcceptChanges was last called.
        Unchanged = 0x00000002,
        // The row was added to a DataRowCollection, and AcceptChanges has not been called.
        Added     = 0x00000004,
        // The row was deleted using the Delete method of the DataRow.
        Deleted   = 0x00000008,
        // The row was modified and AcceptChanges has not been called.
        Modified  = 0x000000010
    }
}
