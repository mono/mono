//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    static class LoadRetryStrategyFactory
    {
        public static ILoadRetryStrategy CreateRetryStrategy(InstanceLockedExceptionAction instanceLockedExceptionAction)
        {
            switch (instanceLockedExceptionAction)
            {
                case InstanceLockedExceptionAction.AggressiveRetry:
                {
                    return new LoadRetryExponentialBackoffStrategy();
                }
                case InstanceLockedExceptionAction.BasicRetry:
                {
                    return new LoadRetryConstantStrategy();
                }
                case InstanceLockedExceptionAction.NoRetry:
                default:
                {
                    return null;
                }
            }
        }
    }
}
