//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    // This is the in-memory cache used for caching SCTs
    sealed class SecurityContextTokenCache : TimeBoundedCache
    {
        // if there are less than lowWaterMark entries, no purging is done
        static int lowWaterMark = 50;
        // frequency of purging the cache of stale entries
        // this is set to 10 mins as SCTs are expected to have long lifetimes
        static TimeSpan purgingInterval = TimeSpan.FromMinutes(10);
        static double pruningFactor = 0.20;
        bool replaceOldestEntries = true;
        static SctEffectiveTimeComparer sctEffectiveTimeComparer = new SctEffectiveTimeComparer();
        TimeSpan clockSkew;

        public SecurityContextTokenCache( int capacity, bool replaceOldestEntries )
            : this( capacity, replaceOldestEntries, SecurityProtocolFactory.defaultMaxClockSkew )
        {
        }

        public SecurityContextTokenCache(int capacity, bool replaceOldestEntries, TimeSpan clockSkew)
            : base(lowWaterMark, capacity, null, PurgingMode.TimerBasedPurge, purgingInterval, true)

        {
            this.replaceOldestEntries = replaceOldestEntries;
            this.clockSkew = clockSkew;
        }

        public void AddContext(SecurityContextSecurityToken token)
        {
            TryAddContext(token, true);
        }
        
        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            return TryAddContext(token, false);
        }

        bool TryAddContext(SecurityContextSecurityToken token, bool throwOnFailure)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            if ( !SecurityUtils.IsCurrentlyTimeEffective( token.ValidFrom, token.ValidTo, this.clockSkew ) )
            {
                if (token.KeyGeneration == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SecurityContextExpiredNoKeyGeneration, token.ContextId));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SecurityContextExpired, token.ContextId, token.KeyGeneration.ToString()));
            }

            if ( !SecurityUtils.IsCurrentlyTimeEffective( token.KeyEffectiveTime, token.KeyExpirationTime, this.clockSkew ) )
            {
                if (token.KeyGeneration == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SecurityContextKeyExpiredNoKeyGeneration, token.ContextId));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SecurityContextKeyExpired, token.ContextId, token.KeyGeneration.ToString()));
            }

            object hashKey = GetHashKey(token.ContextId, token.KeyGeneration);
            bool wasTokenAdded = base.TryAddItem(hashKey, (SecurityContextSecurityToken)token.Clone(), false);
            if (!wasTokenAdded)
            {
                if (throwOnFailure)
                {
                    if (token.KeyGeneration == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ContextAlreadyRegisteredNoKeyGeneration, token.ContextId)));
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ContextAlreadyRegistered, token.ContextId, token.KeyGeneration.ToString())));
                }
            }
            return wasTokenAdded;
        }

        object GetHashKey(UniqueId contextId, UniqueId generation)
        {
            if (generation == null)
            {
                return contextId;
            }
            else
            {
                return new ContextAndGenerationKey(contextId, generation);
            }
        }

        public void ClearContexts()
        {
            base.ClearItems();
        }

        public SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            object hashKey = GetHashKey(contextId, generation);
            SecurityContextSecurityToken sct = (SecurityContextSecurityToken)base.GetItem(hashKey);
            return sct != null ? (SecurityContextSecurityToken)sct.Clone() : null;
        }

        public void RemoveContext(UniqueId contextId, UniqueId generation, bool throwIfNotPresent)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            object hashKey = GetHashKey(contextId, generation);
            if (!base.TryRemoveItem(hashKey) && throwIfNotPresent)
            {
                if (generation == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ContextNotPresentNoKeyGeneration, contextId)));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ContextNotPresent, contextId, generation.ToString())));
            }
        }

        ArrayList GetMatchingKeys(UniqueId contextId)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            ArrayList matchingKeys = new ArrayList(2);

            bool lockHeld = false;
            try
            {
                try { }
                finally
                {
                    base.CacheLock.AcquireReaderLock(-1);
                    lockHeld = true;
                }
                foreach (object key in this.Entries.Keys)
                {
                    bool isMatch = false;
                    if (key is UniqueId)
                    {
                        isMatch = (((UniqueId)key) == contextId);
                    }
                    else
                    {
                        isMatch = (((ContextAndGenerationKey)key).ContextId == contextId);
                    }
                    if (isMatch)
                    {
                        matchingKeys.Add(key);
                    }
                }
            }
            finally
            {
                if (lockHeld)
                {
                    base.CacheLock.ReleaseReaderLock();
                }
            }
            return matchingKeys;
        }

        public void RemoveAllContexts(UniqueId contextId)
        {
            ArrayList matchingKeys = GetMatchingKeys(contextId);
            for (int i = 0; i < matchingKeys.Count; ++i)
            {
                base.TryRemoveItem(matchingKeys[i]);
            }

        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken token, DateTime expirationTime)
        {
            if (token.ValidTo <= expirationTime.ToUniversalTime())
            {
                return;
            }
            base.TryReplaceItem(GetHashKey(token.ContextId, token.KeyGeneration), token, expirationTime);
        }

        public Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId)
        {
            ArrayList matchingKeys = GetMatchingKeys(contextId);

            Collection<SecurityContextSecurityToken> matchingContexts = new Collection<SecurityContextSecurityToken>();
            for (int i = 0; i < matchingKeys.Count; ++i)
            {
                SecurityContextSecurityToken token = base.GetItem(matchingKeys[i]) as SecurityContextSecurityToken;
                if (token != null)
                {
                    matchingContexts.Add(token);
                }
            }
            return matchingContexts;
        }

        protected override ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            if (!this.replaceOldestEntries)
            {
                SecurityTraceRecordHelper.TraceSecurityContextTokenCacheFull(this.Capacity, 0);
                return base.OnQuotaReached(cacheTable);
            }
            else
            {
                List<SecurityContextSecurityToken> tokens = new List<SecurityContextSecurityToken>(cacheTable.Count);
                foreach (IExpirableItem value in cacheTable.Values)
                {
                    SecurityContextSecurityToken token = (SecurityContextSecurityToken)ExtractItem(value);
                    tokens.Add(token);
                }
                tokens.Sort(sctEffectiveTimeComparer);
                int pruningAmount = (int)(((double)this.Capacity) * pruningFactor);
                pruningAmount = pruningAmount <= 0 ? this.Capacity : pruningAmount;
                ArrayList keys = new ArrayList(pruningAmount);
                for (int i = 0; i < pruningAmount; ++i)
                {
                    keys.Add(GetHashKey(tokens[i].ContextId, tokens[i].KeyGeneration));
                    OnRemove(tokens[i]);
                }
                SecurityTraceRecordHelper.TraceSecurityContextTokenCacheFull(this.Capacity, pruningAmount);
                return keys;
            }
        }

        sealed class SctEffectiveTimeComparer : IComparer<SecurityContextSecurityToken>
        {
            public int Compare(SecurityContextSecurityToken sct1, SecurityContextSecurityToken sct2)
            {
                if (sct1 == sct2)
                {
                    return 0;
                }
                if (sct1.ValidFrom.ToUniversalTime() < sct2.ValidFrom.ToUniversalTime())
                {
                    return -1;
                }
                else if (sct1.ValidFrom.ToUniversalTime() > sct2.ValidFrom.ToUniversalTime())
                {
                    return 1;
                }
                else
                {
                    // compare the key effective times
                    if (sct1.KeyEffectiveTime.ToUniversalTime() < sct2.KeyEffectiveTime.ToUniversalTime())
                    {
                        return -1;
                    }
                    else if (sct1.KeyEffectiveTime.ToUniversalTime() > sct2.KeyEffectiveTime.ToUniversalTime())
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        protected override void OnRemove(object item)
        {
            ((IDisposable)item).Dispose();
            base.OnRemove(item);
        }

        struct ContextAndGenerationKey
        {
            UniqueId contextId;
            UniqueId generation;

            public ContextAndGenerationKey(UniqueId contextId, UniqueId generation)
            {
                Fx.Assert(contextId != null && generation != null, "");
                this.contextId = contextId;
                this.generation = generation;
            }

            public UniqueId ContextId
            {
                get
                {
                    return this.contextId;
                }
            }

            public UniqueId Generation
            {
                get
                {
                    return this.generation;
                }
            }

            public override int GetHashCode()
            {
                return this.contextId.GetHashCode() ^ this.generation.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is ContextAndGenerationKey)
                {
                    ContextAndGenerationKey key2 = ((ContextAndGenerationKey)obj);
                    return (key2.ContextId == this.contextId && key2.Generation == this.generation);
                }
                else
                {
                    return false;
                }
            }

            public static bool operator ==(ContextAndGenerationKey a, ContextAndGenerationKey b)
            {
                if (object.ReferenceEquals(a, null))
                {
                    return object.ReferenceEquals(b, null);
                }

                return (a.Equals(b));
            }

            public static bool operator !=(ContextAndGenerationKey a, ContextAndGenerationKey b)
            {
                return !(a == b);
            }
        }
    }
}
