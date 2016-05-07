//------------------------------------------------------------------------------
// <copyright file="StatementType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum StatementType {

        Select = 0,

        Insert = 1,

        Update = 2,

        Delete = 3,

        Batch = 4,
    }
}
