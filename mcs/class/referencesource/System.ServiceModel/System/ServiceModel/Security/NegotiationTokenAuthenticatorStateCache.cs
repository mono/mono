//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;

    sealed class NegotiationTokenAuthenticatorStateCache<T> : TimeBoundedCache
        where T : NegotiationTokenAuthenticatorState
    {
        static int lowWaterMark = 50;
        static TimeSpan purgingInterval = TimeSpan.FromMinutes(10);
        TimeSpan cachingSpan;

        public NegotiationTokenAuthenticatorStateCache(TimeSpan cachingSpan, int maximumCachedState)
            : base(lowWaterMark, maximumCachedState, null, PurgingMode.TimerBasedPurge, TimeSpan.FromTicks(cachingSpan.Ticks >> 2), true)
        {
            this.cachingSpan = cachingSpan;
        }

        public void AddState(string context, T state)
        {
            DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, this.cachingSpan);
            bool wasStateAdded = base.TryAddItem(context, state, expirationTime, false);
            if (!wasStateAdded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.NegotiationStateAlreadyPresent, context)));
            }
            if (TD.NegotiateTokenAuthenticatorStateCacheRatioIsEnabled())
            {
                TD.NegotiateTokenAuthenticatorStateCacheRatio(base.Count, base.Capacity);
            }
        }

        public T GetState(string context)
        {
            return (this.GetItem(context) as T);
        }

        public void RemoveState(string context)
        {
            this.TryRemoveItem(context);
            if (TD.NegotiateTokenAuthenticatorStateCacheRatioIsEnabled())
            {
                TD.NegotiateTokenAuthenticatorStateCacheRatio(base.Count, base.Capacity);
            }
        }


        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            if (TD.NegotiateTokenAuthenticatorStateCacheExceededIsEnabled())
            {
                TD.NegotiateTokenAuthenticatorStateCacheExceeded(SR.GetString(SR.CachedNegotiationStateQuotaReached, this.Capacity));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(SR.GetString(SR.CachedNegotiationStateQuotaReached, this.Capacity)));
        }

        protected override void OnRemove(object item)
        {
            ((IDisposable)item).Dispose();
            base.OnRemove(item);
        }
    }
}
