//------------------------------------------------------------------------------
// <copyright file="updatestatus.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum UpdateStatus {

        Continue = 0,

        ErrorsOccurred = 1,

        SkipCurrentRow = 2,

        SkipAllRemainingRows = 3,
    }
}
