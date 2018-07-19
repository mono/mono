//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    public enum InstanceLockedExceptionAction
    {
        NoRetry = 0,
        BasicRetry,
        AggressiveRetry
    };
}
