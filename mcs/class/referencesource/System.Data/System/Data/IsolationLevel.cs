//------------------------------------------------------------------------------
// <copyright file="IsolationLevel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum IsolationLevel { // MDAC 74269

        Unspecified     = unchecked((int)0xffffffff),
        Chaos           = 0x10,
        ReadUncommitted = 0x100,
        ReadCommitted   = 0x1000,
        RepeatableRead  = 0x10000,
        Serializable    = 0x100000,
        Snapshot        = 0x1000000,
    }
}
