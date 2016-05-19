//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;

    //Admin - End User/Admin/Support/Tools
    //Operational - Admin/Support/Tools
    //Analytic - Tools
    //Debug - Developers
    enum TraceChannel
    {
        Admin = 16,
        Operational = 17,
        Analytic = 18,
        Debug = 19,
        Perf = 20,
        Application = 9, //This is reserved for Windows Event Log
    }
}
