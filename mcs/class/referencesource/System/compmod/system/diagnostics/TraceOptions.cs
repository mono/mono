//------------------------------------------------------------------------------
// <copyright file="TraceOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    [Flags]
    public enum TraceOptions {
        None =             0,
        LogicalOperationStack = 0x01,
        DateTime=       0x02,
        Timestamp=      0x04,
        ProcessId=      0x08,
        ThreadId=       0x10,
        Callstack=      0x20,
    }
}
