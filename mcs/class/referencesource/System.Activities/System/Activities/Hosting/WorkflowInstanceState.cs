//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System;

    public enum WorkflowInstanceState
    {
        Idle,
        Runnable,
        Complete,
        Aborted // only Abort is valid
    }
}
