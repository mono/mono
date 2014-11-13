//------------------------------------------------------------------------------
// <copyright file="SqlBulkCopyOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    [Flags]
    public enum SqlBulkCopyOptions {
        Default             = 0,
        KeepIdentity        = 1 << 0,
        CheckConstraints    = 1 << 1,
        TableLock           = 1 << 2,
        KeepNulls           = 1 << 3,
        FireTriggers        = 1 << 4,
        UseInternalTransaction = 1 << 5,
    }
}




