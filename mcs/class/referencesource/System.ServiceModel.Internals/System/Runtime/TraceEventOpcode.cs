//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;

    enum TraceEventOpcode
    {
        Info = 0,
        Start = 1,
        Stop = 2,
        Reply = 6,
        Resume = 7,
        Suspend = 8,
        Send = 9,
        Receive = 240
    }
}
