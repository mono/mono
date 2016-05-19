//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;

    class LoadRetryExponentialBackoffStrategy : ILoadRetryStrategy
    {
        readonly TimeSpan DefaultBackoffLimit = TimeSpan.FromSeconds(10);
        readonly TimeSpan DefaultBackoffMultiplier = TimeSpan.FromMilliseconds(100);
        readonly int expLimit = (int)(Math.Log(Int32.MaxValue, 2)) - 1;
        readonly TimeSpan multiplier;
        readonly TimeSpan maxDelay;

        Random random = new Random(DateTime.Now.Millisecond);

        public LoadRetryExponentialBackoffStrategy()
        {
            this.multiplier = DefaultBackoffMultiplier;
            this.maxDelay = DefaultBackoffLimit;   
        }

        public TimeSpan RetryDelay(int retryAttempt)
        {
            int power = Math.Min(retryAttempt, this.expLimit);

            return TimeSpan.FromMilliseconds
            (
                Math.Min
                (
                    this.maxDelay.TotalMilliseconds,
                    this.multiplier.TotalMilliseconds * random.Next(1, ((2 << power) - 1))
                )
            );
        }

    }
}
