//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    class LoadRetryConstantStrategy : ILoadRetryStrategy
    {
        static readonly TimeSpan defaultRetryDelay = TimeSpan.FromSeconds(5);        

        public TimeSpan RetryDelay(int retryAttempt)
        {
            return defaultRetryDelay;
        }
    }
}
