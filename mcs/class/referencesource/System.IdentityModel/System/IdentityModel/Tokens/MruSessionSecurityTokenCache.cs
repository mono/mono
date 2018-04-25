//-----------------------------------------------------------------------
// <copyright file="MruSessionSecurityTokenCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Tokens;

    /// <summary>
    /// An MRU cache (Most Recently Used).
    /// </summary>
    /// <remarks>
    /// Thread safe. Critsec around each method.
    /// A LinkedList is used to track MRU for fast purge.
    /// A Dictionary is used for fast keyed lookup.
    /// Grows until it reaches this.maximumSize, then purges down to this.sizeAfterPurge.
    /// </remarks>
    internal class MruSessionSecurityTokenCache : SessionSecurityTokenCache
    {
#pragma warning disable 1591
        public const int DefaultTokenCacheSize = 20000; 
        public static readonly TimeSpan DefaultPurgeInterval = TimeSpan.FromMinutes(15);
#pragma warning restore 1591

        private DateTime nextPurgeTime = DateTime.UtcNow + DefaultPurgeInterval;        
        private Dictionary<SessionSecurityTokenCacheKey, CacheEntry> items;
        private int maximumSize;
        private CacheEntry mruEntry;
        private LinkedList<SessionSecurityTokenCacheKey> mruList;
        private int sizeAfterPurge;
        private object syncRoot = new object();
        private object purgeLock = new object();
        
        /// <summary>
        /// Constructor to create an instance of this class.
        /// </summary>
        /// <remarks>
        /// Uses the default maximum cache size.
        /// </remarks>
        public MruSessionSecurityTokenCache()
            : this(DefaultTokenCacheSize)
        {
        }

        /// <summary>
        /// Constructor to create an instance of this class.
        /// </summary>
        /// <param name="maximumSize">Defines the maximum size of the cache.</param>
        public MruSessionSecurityTokenCache(int maximumSize)
            : this(maximumSize, null)
        {
        }

        /// <summary>
        /// Constructor to create an instance of this class.
        /// </summary>
        /// <param name="maximumSize">Defines the maximum size of the cache.</param>
        /// <param name="comparer">The method used for comparing cache entries.</param>
        public MruSessionSecurityTokenCache(int maximumSize, IEqualityComparer<SessionSecurityTokenCacheKey> comparer)
            : this((maximumSize / 5) * 4, maximumSize, comparer)
        {
        }

        /// <summary>
        /// Constructor to create an instance of this class.
        /// </summary>
        /// <param name="sizeAfterPurge">
        /// If the cache size exceeds <paramref name="maximumSize"/>, 
        /// the cache will be resized to <paramref name="sizeAfterPurge"/> by removing least recently used items.
        /// </param>
        /// <param name="maximumSize">Defines the maximum size of the cache.</param>
        public MruSessionSecurityTokenCache(int sizeAfterPurge, int maximumSize)
            : this(sizeAfterPurge, maximumSize, null)
        {
        }

        /// <summary>
        /// Constructor to create an instance of this class.
        /// </summary>
        /// <param name="sizeAfterPurge">Specifies the size to which the cache is purged after it reaches <paramref name="maximumSize"/>.</param>
        /// <param name="maximumSize">Specifies the maximum size of the cache.</param>
        /// <param name="comparer">Specifies the method used for comparing cache entries.</param>
        public MruSessionSecurityTokenCache(int sizeAfterPurge, int maximumSize, IEqualityComparer<SessionSecurityTokenCacheKey> comparer)
        {
            if (sizeAfterPurge < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID0008), "sizeAfterPurge"));
            }

            if (sizeAfterPurge >= maximumSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID0009), "sizeAfterPurge"));
            }

            // null comparer is ok
            this.items = new Dictionary<SessionSecurityTokenCacheKey, CacheEntry>(maximumSize, comparer);
            this.maximumSize = maximumSize;
            this.mruList = new LinkedList<SessionSecurityTokenCacheKey>();
            this.sizeAfterPurge = sizeAfterPurge;
            this.mruEntry = new CacheEntry();
        }

        /// <summary>
        /// Gets the maximum size of the cache
        /// </summary>
        public int MaximumSize
        {
            get { return this.maximumSize; }
        }

        /// <summary>
        /// Deletes the specified cache entry from the MruCache.
        /// </summary>
        /// <param name="key">Specifies the key for the entry to be deleted.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> is null.</exception>
        public override void Remove(SessionSecurityTokenCacheKey key)
        {
            if (key == null)
            {
                return;
            }

            lock (this.syncRoot)
            {
                CacheEntry entry;
                if (this.items.TryGetValue(key, out entry))
                {
                    this.items.Remove(key);
                    this.mruList.Remove(entry.Node);
                    if (object.ReferenceEquals(this.mruEntry.Node, entry.Node))
                    {
                        this.mruEntry.Value = null;
                        this.mruEntry.Node = null;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to add an entry to the cache or update an existing one.
        /// </summary>
        /// <param name="key">The key for the entry to be added.</param>
        /// <param name="value">The security token to be added to the cache.</param>
        /// <param name="expirationTime">The expiration time for this entry.</param>
        public override void AddOrUpdate(SessionSecurityTokenCacheKey key, SessionSecurityToken value, DateTime expirationTime)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            lock (this.syncRoot)
            {
                this.Purge();
                this.Remove(key);

                // Add  the new entry to the cache and make it the MRU element
                CacheEntry entry = new CacheEntry();
                entry.Node = this.mruList.AddFirst(key);
                entry.Value = value;
                this.items.Add(key, entry);
                this.mruEntry = entry;
            }
        }

        /// <summary>
        /// Returns the Session Security Token corresponding to the specified key exists in the cache. Also if it exists, marks it as MRU. 
        /// </summary>
        /// <param name="key">Specifies the key for the entry to be retrieved.</param>
        /// <returns>Returns the Session Security Token from the cache if found, otherwise, null.</returns>
        public override SessionSecurityToken Get(SessionSecurityTokenCacheKey key)
        {
            if (key == null)
            {
                return null;
            }

            // If found, make the entry most recently used
            SessionSecurityToken sessionToken = null;
            CacheEntry entry;
            bool found;
            
            lock (this.syncRoot)
            {
                // first check our MRU item
                if (this.mruEntry.Node != null && key != null && key.Equals(this.mruEntry.Node.Value))
                {
                    return this.mruEntry.Value;                    
                }

                found = this.items.TryGetValue(key, out entry);
                if (found)
                {
                    sessionToken = entry.Value;

                    // Move the node to the head of the MRU list if it's not already there
                    if (this.mruList.Count > 1 && !object.ReferenceEquals(this.mruList.First, entry.Node))
                    {
                        this.mruList.Remove(entry.Node);
                        this.mruList.AddFirst(entry.Node);
                        this.mruEntry = entry;
                    }
                }
            }

            return sessionToken;
        }

        /// <summary>
        /// Deletes matching cache entries from the MruCache.
        /// </summary>
        /// <param name="endpointId">Specifies the endpointId for the entries to be deleted.</param>
        /// <param name="contextId">Specifies the contextId for the entries to be deleted.</param>
        public override void RemoveAll(string endpointId, System.Xml.UniqueId contextId)
        {
            if (null == contextId || string.IsNullOrEmpty(endpointId))
            {
                return;
            }

            Dictionary<SessionSecurityTokenCacheKey, CacheEntry> entriesToDelete = new Dictionary<SessionSecurityTokenCacheKey, CacheEntry>();
            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(endpointId, contextId, null);
            key.IgnoreKeyGeneration = true;
            lock (this.syncRoot)
            {
                foreach (SessionSecurityTokenCacheKey itemKey in this.items.Keys)
                {
                    if (itemKey.Equals(key))
                    {
                        entriesToDelete.Add(itemKey, this.items[itemKey]);
                    }
                }

                foreach (SessionSecurityTokenCacheKey itemKey in entriesToDelete.Keys)
                {
                    this.items.Remove(itemKey);
                    CacheEntry entry = entriesToDelete[itemKey];
                    this.mruList.Remove(entry.Node);
                    if (object.ReferenceEquals(this.mruEntry.Node, entry.Node))
                    {
                        this.mruEntry.Value = null;
                        this.mruEntry.Node = null;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to remove all entries with a matching endpoint Id from the cache.
        /// </summary>
        /// <param name="endpointId">The endpoint id for the entry to be removed.</param>
        public override void RemoveAll(string endpointId)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID4294)));
        }
        
        /// <summary>
        /// Returns all the entries that match the given key.
        /// </summary>
        /// <param name="endpointId">The endpoint id for the entries to be retrieved.</param>
        /// <param name="contextId">The context id for the entries to be retrieved.</param>
        /// <returns>A collection of all the matching entries, an empty collection of no match found.</returns>
        public override IEnumerable<SessionSecurityToken> GetAll(string endpointId, System.Xml.UniqueId contextId)
        {
            Collection<SessionSecurityToken> tokens = new Collection<SessionSecurityToken>();
            
            if (null == contextId || string.IsNullOrEmpty(endpointId))
            {
                return tokens;
            } 
            
            CacheEntry entry;
            SessionSecurityTokenCacheKey key = new SessionSecurityTokenCacheKey(endpointId, contextId, null);
            key.IgnoreKeyGeneration = true;

            lock (this.syncRoot)
            {
                foreach (SessionSecurityTokenCacheKey itemKey in this.items.Keys)
                {
                    if (itemKey.Equals(key))
                    {
                        entry = this.items[itemKey];

                        // Move the node to the head of the MRU list if it's not already there
                        if (this.mruList.Count > 1 && !object.ReferenceEquals(this.mruList.First, entry.Node))
                        {
                            this.mruList.Remove(entry.Node);
                            this.mruList.AddFirst(entry.Node);
                            this.mruEntry = entry;
                        }
                        
                        tokens.Add(entry.Value);
                    }
                }                
            }

            return tokens;
        }

        /// <summary>
        /// This method must not be called from within a read or writer lock as a deadlock will occur.
        /// Checks the time a decides if a cleanup needs to occur.
        /// </summary>
        private void Purge()
        {
            if (this.items.Count >= this.maximumSize)
            {
                // If the cache is full, purge enough LRU items to shrink the 
                // cache down to the low watermark
                int countToPurge = this.maximumSize - this.sizeAfterPurge;
                for (int i = 0; i < countToPurge; i++)
                {
                    SessionSecurityTokenCacheKey keyRemove = this.mruList.Last.Value;
                    this.mruList.RemoveLast();
                    this.items.Remove(keyRemove);
                }

                if (DiagnosticUtility.ShouldTrace(TraceEventType.Information))
                {
                    TraceUtility.TraceString(
                        TraceEventType.Information,
                        SR.GetString(
                        SR.ID8003,
                        this.maximumSize,
                        this.sizeAfterPurge));
                }
            }
        }
       
        public class CacheEntry
        {
            public SessionSecurityToken Value
            { 
                get; 
                set; 
            }

            public LinkedListNode<SessionSecurityTokenCacheKey> Node
            { 
                get; 
                set;
            }
        }
    }
}
