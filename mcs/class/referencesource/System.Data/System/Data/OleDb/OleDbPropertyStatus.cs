//------------------------------------------------------------------------------
// <copyright file="OleDbPropertyStatus.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    internal enum OleDbPropertyStatus { // WebData 99005
        Ok = 0,
        NotSupported = 1,
        BadValue = 2,
        BadOption = 3,
        BadColumn = 4,
        NotAllSettable = 5,
        NotSettable = 6,
        NotSet = 7,
        Conflicting = 8,
        NotAvailable = 9,
    }
}
