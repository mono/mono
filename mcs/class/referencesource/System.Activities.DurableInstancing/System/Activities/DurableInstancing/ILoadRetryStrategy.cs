//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    interface ILoadRetryStrategy
    {
        TimeSpan RetryDelay(int retryAttempt);
    }
}
