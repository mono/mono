//------------------------------------------------------------------------------
// <copyright file="ConnectionState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    [Flags()]
    public enum ConnectionState {
        Closed     = 0,
        Open       = 1,
        Connecting = 2,
        Executing  = 4,
        Fetching   = 8,
        Broken     = 16,
    }
}
