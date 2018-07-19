//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// A default implementation of the Token replay cache that is backed by
    /// a bounded cache.
    /// </summary>
    internal class DefaultTokenReplayCache : TokenReplayCache
    {
        static readonly int DefaultTokenReplayCacheCapacity = 500000;
        static readonly TimeSpan DefaultTokenReplayCachePurgeInterval = TimeSpan.FromMinutes(1);

        BoundedCache<SecurityToken> _internalCache;

        /// <summary>
        /// Constructs the default token replay cache.
        /// </summary>
        public DefaultTokenReplayCache()
            : this(DefaultTokenReplayCache.DefaultTokenReplayCacheCapacity, DefaultTokenReplayCache.DefaultTokenReplayCachePurgeInterval)
        { }

        /// <summary>
        /// Constructs the default token replay cache with the specified 
        /// capacity and purge interval.
        /// </summary>
        /// <param name="capacity">The capacity of the token cache</param>
        /// <param name="purgeInterval">The time interval after which the cache must be purged</param>
        public DefaultTokenReplayCache(int capacity, TimeSpan purgeInterval)
            : base()
        {
            _internalCache = new BoundedCache<SecurityToken>(capacity, purgeInterval, StringComparer.Ordinal);
        }

        /// <summary>
        /// Gets or Sets the current Capacity of the cache in number of SecurityTokens.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If 'value' is less than or equal to zero.</exception>
        public int Capacity
        {
            get { return _internalCache.Capacity; }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID0002));
                }

                _internalCache.Capacity = value;
            }
        }

        /// <summary>
        /// Removes all SecurityTokens from the Cache.
        /// </summary>
        public void Clear()
        {
            _internalCache.Clear();
        }

        /// <summary>
        /// Increases the maximum number of SecurityTokens that this cache will hold. 
        /// </summary>
        /// <param name="size">The capacity to increase.</param>
        /// <exception cref="ArgumentOutOfRangeException">The input parameter 'size' is less than or equal to zero.</exception>
        /// <returns>Updated capacity</returns>
        /// <remarks>If size + current capacity >= int.MaxValue then capacity will be set to int.MaxValue and the cache capacity will be unbounded.</remarks>
        public int IncreaseCapacity(int size)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("size", size, SR.GetString(SR.ID0002));
            }

            return _internalCache.IncreaseCapacity(size);
        }

        /// <summary>
        /// Gets or Sets the time interval that will be used when checking for expired SecurityTokens.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If 'value' is less than or equal to TimeSpan.Zero.</exception>
        public TimeSpan PurgeInterval
        {
            get { return _internalCache.PurgeInterval; }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ThrowHelperArgumentOutOfRange("value", value, SR.GetString(SR.ID0016));
                }

                _internalCache.PurgeInterval = value;
            }
        }

        /// <summary>
        /// Attempt to add a new entry or update an existing entry.
        /// </summary>
        /// <param name="key">Key to use when adding item.</param>
        /// <param name="securityToken">SecurityToken to add to cache, can be null.</param>
        /// <param name="expirationTime">The expiration instant of the token being added.</param>
        /// <exception cref="System.IdentityModel.LimitExceededException">Thrown if an attempt is made to add a SecurityToken if cache is at capacity.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the expiration time is infinite.</exception>
        public override void AddOrUpdate(string key, SecurityToken securityToken, DateTime expirationTime)
        {
            if (DateTime.Equals(expirationTime, DateTime.MaxValue))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID1072));
            }

            _internalCache.TryRemove(key);
            _internalCache.TryAdd(key, securityToken, expirationTime);
        }

        /// <summary>
        /// Attempt to find if a matching entry exists in the cache.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>true if a matching entry is ifound in the cache, false otherwise</returns>
        public override bool Contains(string key)
        {
            return _internalCache.TryFind(key);
        }

        /// <summary>
        /// Attempt to get a SecurityToken
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The <see cref="SecurityToken"/> found, if any, null otherwise.</returns>
        public override SecurityToken Get(string key)
        {
            SecurityToken token;
            _internalCache.TryGet(key, out token);
            return token;
        }

        /// <summary>
        /// Attempt to remove an entry from the cache
        /// </summary>
        /// <param name="key">The key to the entry to remove</param>
        public override void Remove(string key)
        {
            _internalCache.TryRemove(key);
        }
    }
}
