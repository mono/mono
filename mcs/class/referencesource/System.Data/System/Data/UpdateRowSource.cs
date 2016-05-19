//------------------------------------------------------------------------------
// <copyright file="UpdateRowSource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum UpdateRowSource {

        None = 0,

        OutputParameters = 1,

        FirstReturnedRecord = 2,

        Both = 3,
    }
}
