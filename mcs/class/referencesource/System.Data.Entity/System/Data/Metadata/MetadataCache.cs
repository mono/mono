//---------------------------------------------------------------------
// <copyright file="MetadataCache.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Diagnostics;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Runtime Metadata Cache - this class contains the metadata cache entry for edm and store collections.
    /// </summary>
    internal static class MetadataCache
    {
        #region Fields

        private const string s_dataDirectory = "|datadirectory|";
        private const string s_metadataPathSeparator = "|";

        // This is the period in the periodic cleanup measured in milliseconds
        private const int cleanupPeriod = 5 * 60 * 1000;

        // This dictionary contains the cache entry for the edm item collection. The reason why we need to keep a seperate dictionary 
        // for CSpace item collection is that the same model can be used for different providers. We don't want to load the model
        // again and again
        private static readonly Dictionary<string, EdmMetadataEntry> _edmLevelCache = new Dictionary<string, EdmMetadataEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// This dictionary contains the store cache entry - this entry will only keep track of StorageMappingItemCollection, since internally
        /// storage mapping item collection keeps strong references to both edm item collection and store item collection.
        /// </summary>
        private static readonly Dictionary<string, StoreMetadataEntry> _storeLevelCache = new Dictionary<string, StoreMetadataEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The list maintains the store metadata entries that are still in use, maybe because someone is still holding a strong reference
        /// to it. We need to scan this list everytime the clean up thread wakes up and make sure if the item collection is no longer in use,
        /// call clear on query cache
        /// </summary>
        private static readonly List<StoreMetadataEntry> _metadataEntriesRemovedFromCache = new List<StoreMetadataEntry>();

        private static Memoizer<string, List<MetadataArtifactLoader>> _artifactLoaderCache = new Memoizer<string, List<MetadataArtifactLoader>>(MetadataCache.SplitPaths, null);

        /// <summary>
        /// Read/Write lock for edm cache
        /// </summary>
        private static readonly object _edmLevelLock = new object();

        /// <summary>
        /// Read/Write lock for the store cache
        /// </summary>
        private static readonly object _storeLevelLock = new object();

        // Periodic thread which runs every n mins (look up the cleanupPeriod variable to see the exact time), walks through
        // every item in other store and edm cache and tries to do some cleanup
        private static Timer timer = new Timer(PeriodicCleanupCallback, null, cleanupPeriod, cleanupPeriod);

        #endregion

        #region Methods

        /// <summary>
        /// The purpose of the thread is to do cleanup. It marks the object in various stages before it actually cleans up the object
        /// Here's what this does for each entry in the cache:
        ///     1> First checks if the entry is marked for cleanup.
        ///     2> If the entry is marked for cleanup, that means its in one of the following 3 states
        ///         a) If the strong reference to item collection is not null, it means that this item was marked for cleanup in 
        ///            the last cleanup cycle and we must make the strong reference set to null so that it can be garbage collected.
        ///         b) Otherwise, we are waiting for GC to collect the item collection so that we can remove this entry from the cache
        ///            If the weak reference to item collection is still alive, we don't do anything
        ///         c) If the weak reference to item collection is not alive, we need to remove this entry from the cache
        ///     3> If the entry is not marked for cleanup, then check whether the weak reference to entry token is alive
        ///         a) if it is alive, then this entry is in use and we must do nothing
        ///         b) Otherwise, we can mark this entry for cleanup
        /// </summary>
        /// <param name="state"></param>
        private static void PeriodicCleanupCallback(object state)
        {
            // Perform clean up on edm cache
            DoCacheClean<EdmMetadataEntry>(_edmLevelCache, _edmLevelLock);

            // Perform clean up on store cache
            DoCacheClean<StoreMetadataEntry>(_storeLevelCache, _storeLevelLock);
        }

        /// <summary>
        /// A helper function for splitting up a string that is a concatenation of strings delimited by the metadata
        /// path separator into a string list. The resulting list is NOT sorted.
        /// </summary>
        /// <param name="paths">The paths to split</param>
        /// <returns>An array of strings</returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file name which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataArtifactLoader.Create method call. But the path is not created in this method.
        internal static List<MetadataArtifactLoader> SplitPaths(string paths)
        {
            Debug.Assert(!string.IsNullOrEmpty(paths), "paths cannot be empty or null");
            
            string[] results;

            // This is the registry of all URIs in the global collection.
            HashSet<string> uriRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();

            // If the argument contains one or more occurrences of the macro '|DataDirectory|', we
            // pull those paths out so that we don't lose them in the string-splitting logic below.
            // Note that the macro '|DataDirectory|' cannot have any whitespace between the pipe 
            // symbols and the macro name. Also note that the macro must appear at the beginning of 
            // a path (else we will eventually fail with an invalid path exception, because in that
            // case the macro is not expanded). If a real/physical folder named 'DataDirectory' needs
            // to be included in the metadata path, whitespace should be used on either or both sides
            // of the name.
            //
            List<string> dataDirPaths = new List<string>();

            int indexStart = paths.IndexOf(MetadataCache.s_dataDirectory, StringComparison.OrdinalIgnoreCase);
            while (indexStart != -1)
            {
                int prevSeparatorIndex = indexStart == 0 ? -1 : paths.LastIndexOf(
                                                                MetadataCache.s_metadataPathSeparator,
                                                                indexStart - 1, // start looking here
                                                                StringComparison.Ordinal
                                                            );

                int macroPathBeginIndex = prevSeparatorIndex + 1;

                // The '|DataDirectory|' macro is composable, so identify the complete path, like
                // '|DataDirectory|\item1\item2'. If the macro appears anywhere other than at the
                // beginning, splice out the entire path, e.g. 'C:\item1\|DataDirectory|\item2'. In this
                // latter case the macro will not be expanded, and downstream code will throw an exception.
                //
                int indexEnd = paths.IndexOf(MetadataCache.s_metadataPathSeparator,
                                             indexStart + MetadataCache.s_dataDirectory.Length,
                                             StringComparison.Ordinal);
                if (indexEnd == -1)
                {
                    dataDirPaths.Add(paths.Substring(macroPathBeginIndex));
                    paths = paths.Remove(macroPathBeginIndex);   // update the concatenated list of paths
                    break;
                }

                dataDirPaths.Add(paths.Substring(macroPathBeginIndex, indexEnd - macroPathBeginIndex));

                // Update the concatenated list of paths by removing the one containing the macro.
                //
                paths = paths.Remove(macroPathBeginIndex, indexEnd - macroPathBeginIndex);
                indexStart = paths.IndexOf(MetadataCache.s_dataDirectory, StringComparison.OrdinalIgnoreCase);
            }

            // Split the string on the separator and remove all spaces around each parameter value
            results = paths.Split(new string[] { MetadataCache.s_metadataPathSeparator }, StringSplitOptions.RemoveEmptyEntries);

            // Now that the non-macro paths have been identified, merge the paths containing the macro
            // into the complete list.
            //
            if (dataDirPaths.Count > 0)
            {
                dataDirPaths.AddRange(results);
                results = dataDirPaths.ToArray();
            }

            for (int i = 0; i < results.Length; i++)
            {
                // Trim out all the spaces for this parameter and add it only if it's not blank
                results[i] = results[i].Trim();
                if (results[i].Length > 0)
                {
                    loaders.Add(MetadataArtifactLoader.Create(
                                    results[i],
                                    MetadataArtifactLoader.ExtensionCheck.All,  // validate the extension against all acceptable values
                                    null,
                                    uriRegistry
                                ));
                }
            }

            return loaders;
        }


        /// <summary>
        /// Walks through the given cache and calls cleanup on each entry in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="objectToLock"></param>
        private static void DoCacheClean<T>(Dictionary<string, T> cache, object objectToLock) where T: MetadataEntry
        {
            // Sometime, for some reason, timer can be initialized and the cache is still not initialized.
            if (cache != null)
            {
                List<KeyValuePair<string, T>> keysForRemoval = null;

                lock (objectToLock)
                {
                    // we should check for type of the lock object first, since otherwise we might be reading the count of the list
                    // while some other thread might be modifying it. For e.g. when this function is called for edmcache,
                    // we will be acquiring edmlock and trying to get the count for the list, while some other thread
                    // might be calling ClearCache and we might be adding entries to the list
                    if (objectToLock == _storeLevelLock && _metadataEntriesRemovedFromCache.Count != 0)
                    {
                        // First check the list of entries and remove things which are no longer in use
                        for (int i = _metadataEntriesRemovedFromCache.Count - 1; 0 <= i; i--)
                        {
                            if (!_metadataEntriesRemovedFromCache[i].IsEntryStillValid())
                            {
                                // Clear the query cache
                                _metadataEntriesRemovedFromCache[i].CleanupQueryCache();
                                // Remove the entry at the current index. This is the reason why we
                                // go backwards.
                                _metadataEntriesRemovedFromCache.RemoveAt(i);
                            }
                        }
                    }

                    // We have to use a list to keep track of the keys to remove because we can't remove while enumerating
                    foreach (KeyValuePair<string, T> pair in cache)
                    {
                        if (pair.Value.PeriodicCleanUpThread())
                        {
                            if (keysForRemoval == null)
                            {
                                keysForRemoval = new List<KeyValuePair<string, T>>();
                            }
                            keysForRemoval.Add(pair);
                        }
                    }

                    // Remove all the entries from the cache
                    if (keysForRemoval != null)
                    {
                        for (int i = 0; i < keysForRemoval.Count; i++)
                        {
                            keysForRemoval[i].Value.Clear();
                            cache.Remove(keysForRemoval[i].Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves an cache entry holding to edm metadata for a given cache key
        /// </summary>
        /// <param name="cacheKey">string containing all the files from which edm metadata is to be retrieved</param>
        /// <param name="composite">An instance of the composite MetadataArtifactLoader</param>
        /// <param name="entryToken">The metadata entry token for the returned entry</param>
        /// <returns>Returns the entry containing the edm metadata</returns>
        internal static EdmItemCollection GetOrCreateEdmItemCollection(string cacheKey, 
                                                             MetadataArtifactLoader loader,
                                                             out object entryToken)
        {
            EdmMetadataEntry entry = GetCacheEntry<EdmMetadataEntry>(_edmLevelCache, cacheKey, _edmLevelLock,
                new EdmMetadataEntryConstructor(), out entryToken);

            // Load the edm item collection or if the collection is already loaded, check for security permission
            LoadItemCollection(new EdmItemCollectionLoader(loader), entry);

            return entry.EdmItemCollection;
        }

        /// <summary>
        /// Retrieves an entry holding store metadata for a given cache key
        /// </summary>
        /// <param name="cacheKey">The connection string whose store metadata is to be retrieved</param>
        /// <param name="composite">An instance of the composite MetadataArtifactLoader</param>
        /// <param name="entryToken">The metadata entry token for the returned entry</param>
        /// <returns>the entry containing the information on how to load store metadata</returns>
        internal static StorageMappingItemCollection GetOrCreateStoreAndMappingItemCollections(
                                                                 string cacheKey,
                                                                 MetadataArtifactLoader loader,
                                                                 EdmItemCollection edmItemCollection,
                                                                 out object entryToken)
        {
            StoreMetadataEntry entry = GetCacheEntry<StoreMetadataEntry>(_storeLevelCache, cacheKey, _storeLevelLock,
                new StoreMetadataEntryConstructor(), out entryToken);

            // Load the store item collection or if the collection is already loaded, check for security permission
            LoadItemCollection(new StoreItemCollectionLoader(edmItemCollection, loader), entry);

            return entry.StorageMappingItemCollection;
        }

        internal static List<MetadataArtifactLoader> GetOrCreateMetdataArtifactLoader(string paths)
        {
            return _artifactLoaderCache.Evaluate(paths);
        }

        /// <summary>
        /// Get the entry from the cache given the cache key. If the entry is not present, it creates a new entry and
        /// adds it to the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="cacheKey"></param>
        /// <param name="entryToken"></param>
        /// <param name="metadataEntry"></param>
        /// <param name="objectToLock"></param>
        /// <returns></returns>
        private static T GetCacheEntry<T>(Dictionary<string, T> cache, string cacheKey, object objectToLock, 
            IMetadataEntryConstructor<T> metadataEntry, out object entryToken) where T: MetadataEntry
        {
            T entry;

            // In the critical section, we need to do the minimal thing to ensure correctness
            // Within the lock, we will see if an entry is present. If it is not, we will create a new entry and
            // add it to the cache. In either case, we need to ensure the token to make sure so that any other
            // thread that comes looking for the same entry does nothing in this critical section
            // Also the cleanup thread doesn't do anything since the token is alive
            lock (objectToLock)
            {
                if (cache.TryGetValue(cacheKey, out entry))
                {
                    entryToken = entry.EnsureToken();
                }
                else
                {
                    entry = metadataEntry.GetMetadataEntry();
                    entryToken = entry.EnsureToken();
                    cache.Add(cacheKey, entry);
                }
            }

            return entry;
        }

        /// <summary>
        /// Loads the item collection for the entry
        /// </summary>
        /// <param name="itemCollectionLoader">struct which loads an item collection</param>
        /// <param name="entry">entry whose item collection needs to be loaded</param>
        private static void LoadItemCollection<T>(IItemCollectionLoader<T> itemCollectionLoader, T entry) where T : MetadataEntry
        {
            // At this point, you have made sure that there is an entry with an alive token in the cache so that
            // other threads can find it if they come querying for it, and cleanup thread won't clean the entry
            // If two or more threads come one after the other, we don't won't both of them to load the metadata.
            // So if one of them is loading the metadata, the other should wait and then use the same metadata.
            // For that reason, we have this lock on the entry itself to make sure that this happens. Its okay to
            // update the item collection outside the lock, since assignment are guarantees to be atomic and no two
            // thread are updating this at the same time
            bool isItemCollectionAlreadyLoaded = true;

            if (!entry.IsLoaded)
            {
                lock (entry)
                {
                    if (!entry.IsLoaded)
                    {
                        itemCollectionLoader.LoadItemCollection(entry);
                        isItemCollectionAlreadyLoaded = false;
                    }
                }
            }

            Debug.Assert(entry.IsLoaded, "The entry must be loaded at this point");

            // Making sure that the thread which loaded the item collection is not checking for file permisssions
            // again
            if (isItemCollectionAlreadyLoaded)
            {
                entry.CheckFilePermission();
            }
        }
                
        /// <summary>
        /// Remove all the entries from the cache
        /// </summary>
        internal static void Clear()
        {
            lock (_edmLevelLock)
            {
                _edmLevelCache.Clear();
            }

            lock (_storeLevelLock)
            {
                // Call clear on each of the metadata entries. This is to make sure we clear all the performance
                // counters associated with the query cache
                foreach (StoreMetadataEntry entry in _storeLevelCache.Values)
                {
                    // Check if the weak reference to item collection is still alive
                    if (entry.IsEntryStillValid())
                    {
                        _metadataEntriesRemovedFromCache.Add(entry);
                    }
                    else
                    {
                        entry.Clear();
                    }
                }
                _storeLevelCache.Clear();
            }

            Memoizer<string, List<MetadataArtifactLoader>> artifactLoaderCacheTemp =
                new Memoizer<string, List<MetadataArtifactLoader>>(MetadataCache.SplitPaths, null);

            Interlocked.CompareExchange(ref _artifactLoaderCache, artifactLoaderCacheTemp, _artifactLoaderCache);
        }

        #endregion

        #region InlineClasses

        /// <summary>
        /// The base class having common implementation for all metadata entry classes
        /// </summary>
        private abstract class MetadataEntry
        {
            private WeakReference _entryTokenReference;
            private ItemCollection _itemCollection;
            private WeakReference _weakReferenceItemCollection;
            private bool _markEntryForCleanup;
            private FileIOPermission _filePermissions;

            /// <summary>
            /// The constructor for constructing this MetadataEntry
            /// </summary>
            internal MetadataEntry()
            {
                // Create this once per life time of the object. Creating extra weak references causing unnecessary GC pressure
                _entryTokenReference = new WeakReference(null);
                _weakReferenceItemCollection = new WeakReference(null);
            }

            /// <summary>
            /// returns the item collection inside this metadata entry
            /// </summary>
            protected ItemCollection ItemCollection { get { return _itemCollection; } }

            /// <summary>
            /// Update the entry with the given item collection
            /// </summary>
            /// <param name="itemCollection"></param>
            protected void UpdateMetadataEntry(ItemCollection itemCollection, FileIOPermission filePermissions)
            {
                Debug.Assert(_entryTokenReference.IsAlive, "You must call Ensure token before you call this method");
                Debug.Assert(_markEntryForCleanup == false, "The entry must not be marked for cleanup");
                Debug.Assert(_itemCollection == null, "Item collection must be null");
                Debug.Assert(_filePermissions == null, "filePermissions must be null");

                // Update strong and weak reference for item collection
                _weakReferenceItemCollection.Target = itemCollection;
                _filePermissions = filePermissions;
                
                // do this last, because it signals that we are loaded
                _itemCollection = itemCollection;
            }

            internal bool IsLoaded { get { return _itemCollection != null; } }

            /// <summary>
            /// This method is called periodically by the cleanup thread to make the unused entries
            /// go through various stages, before it is ready for cleanup. If it is ready, this method
            /// returns true and then the entry is completely removed from the cache
            /// </summary>
            /// <returns></returns>
            internal bool PeriodicCleanUpThread()
            {
                // Here's what this does for each entry in the cache:
                //     1> First checks if the entry is marked for cleanup.
                //     2> If the entry is marked for cleanup, that means its in one of the following 3 states
                //         a) If the strong reference to item collection is not null, it means that this item was marked for cleanup in 
                //            the last cleanup cycle and we must make the strong reference set to null so that it can be garbage collected. (GEN 2)
                //         b) Otherwise, we are waiting for GC to collect the item collection so that we can remove this entry from the cache
                //            If the weak reference to item collection is still alive, we don't do anything
                //         c) If the weak reference to item collection is not alive, we need to remove this entry from the cache (GEN 3)
                //     3> If the entry is not marked for cleanup, then check whether the weak reference to entry token is alive
                //         a) if it is alive, then this entry is in use and we must do nothing
                //         b) Otherwise, we can mark this entry for cleanup (GEN 1)
                if (_markEntryForCleanup)
                {
                    Debug.Assert(_entryTokenReference.IsAlive == false, "Entry Token must never be alive if the entry is marked for cleanup");

                    if (_itemCollection != null)
                    {
                        // GEN 2
                        _itemCollection = null;
                    }
                    else if (!_weakReferenceItemCollection.IsAlive)
                    {
                        // GEN 3
                        _filePermissions = null;
                        // this entry must be removed from the cache
                        return true;
                    }
                }
                else if (!_entryTokenReference.IsAlive)
                {
                    // GEN 1

                    // If someone creates a entity connection, and calls GetMetadataWorkspace. This creates an cache entry,
                    // but the item collection is not initialized yet (since store item collection are initialized only 
                    // when one calls connection.Open()). Suppose now the connection is no longer used - in other words,
                    // open was never called and it goes out of scope. After some time when the connection gets GC'ed,
                    // entry token won't be alive any longer, but item collection inside it will be null, since it was never initialized.
                    // So we can't assert that item collection must be always initialized here
                    _markEntryForCleanup = true;
                }

                return false;
            }

            /// <summary>
            /// Make sure that the entry has a alive token and returns that token - it can be new token or an existing
            /// one, depending on the state of the entry
            /// </summary>
            /// <returns></returns>
            internal object EnsureToken()
            {
                object entryToken = _entryTokenReference.Target;
                ItemCollection itemCollection = (ItemCollection)_weakReferenceItemCollection.Target;

                // When ensure token is called, the entry can be in different stages
                // 1> Its a newly created entry - no token, no item collection, etc. Just create a new token and 
                //    return back
                // 2> An entry already in use - the weak reference to token must be alive. We just need to grab the token
                //    and return it
                // 3> No one is using this entry and hence the token is no longer alive. If we have strong reference to item
                //    collection, then create a new token and return it
                // 4> No one has used this token for one cleanup cycle and hence strong reference is null. But the weak reference
                //    is still alive. We need to make the initialize the strong reference again, create a new token and return it
                // 5> This entry has not been used for long enough that even the weak reference is no longer alive. This entry is
                //    now exactly like a new entry, except that it is still marked for cleanup. Create a new token, set mark for
                //    cleanup to false and return the token
                if (_entryTokenReference.IsAlive)
                {
                    Debug.Assert(_markEntryForCleanup == false, "An entry with alive token cannot be marked for cleanup");
                    // ItemCollection strong pointer can be null or not null. If the entry has been created, and loadItemCollection
                    // hasn't been called yet, the token will be alive, but item collection will be null. If someone called
                    // load item collection, then item collection will not be non-null
                    return entryToken;
                }
                // If the entry token is not alive, then it can be either a new created entry with everything set
                // to null or it must be one of the entries which is no longer in use
                else if (_itemCollection != null)
                {
                    Debug.Assert(_weakReferenceItemCollection.IsAlive, "Since the strong reference is still there, weak reference must also be alive");
                    // This means that no one is using the item collection, and its waiting to be cleanuped
                }
                else 
                {
                    if (_weakReferenceItemCollection.IsAlive)
                    {
                        Debug.Assert(_markEntryForCleanup, "Since the strong reference is null, this entry must be marked for cleanup");
                        // Initialize the strong reference to item collection
                        _itemCollection = itemCollection;
                    }
                    else
                    {
                        // no more references to the collection
                        // are available, so get rid of the permissions
                        // object.  We will get a new one when we get a new collection
                        _filePermissions = null;
                    }
                }
                // Even if the _weakReferenceItemCollection is no longer alive, we will reuse this entry. Assign a new entry token and set mark for cleanup to false
                // so that this entry is not cleared by the cleanup thread

                entryToken = new object();
                _entryTokenReference.Target = entryToken;
                _markEntryForCleanup = false;
                return entryToken;
            }

            /// <summary>
            /// Check if the thread has appropriate permissions to use the already loaded metadata
            /// </summary>
            internal void CheckFilePermission()
            {
                Debug.Assert(_itemCollection != null, "Item collection must be present since we want to reuse the metadata");
                Debug.Assert(_entryTokenReference.IsAlive, "This entry must be in use");
                Debug.Assert(_markEntryForCleanup == false, "The entry must not marked for cleanup");
                Debug.Assert(_weakReferenceItemCollection.IsAlive, "Weak reference to item collection must be alive");

                // we will have an empty ItemCollection (no files were used to load it)
                if (_filePermissions != null)
                {
                    _filePermissions.Demand();
                }
            }

            /// <summary>
            /// Dispose the composite loader that encapsulates all artifacts
            /// </summary>
            internal virtual void Clear()
            {
            }

            /// <summary>
            /// This returns true if the entry is still in use - the entry can be use if the entry token is 
            /// still alive.If the entry token is still not alive, it means that no one is using this entry
            /// and its okay to remove it. Today there is no
            /// </summary>
            /// <returns></returns>
            internal bool IsEntryStillValid()
            {
                return _entryTokenReference.IsAlive;
            }
        }

        /// <summary>
        /// A metadata entry holding EdmItemCollection object for the cache
        /// </summary>
        private class EdmMetadataEntry : MetadataEntry
        {
            /// <summary>
            /// Gets the EdmItemCollection for this entry
            /// </summary>
            internal EdmItemCollection EdmItemCollection
            {
                get
                {
                    return (EdmItemCollection)this.ItemCollection;
                }
            }

            /// <summary>
            /// Just loads the edm item collection
            /// </summary>
            /// <returns></returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity")]
            internal void LoadEdmItemCollection(MetadataArtifactLoader loader)
            {
                Debug.Assert(loader != null, "loader is null");

                List<XmlReader> readers = loader.CreateReaders(DataSpace.CSpace);
                try
                {
                    EdmItemCollection itemCollection = new EdmItemCollection(
                                                           readers,
                                                           loader.GetPaths(DataSpace.CSpace)
                                                            );

                    List<string> permissionPaths = new List<string>();
                    loader.CollectFilePermissionPaths(permissionPaths, DataSpace.CSpace);
                    FileIOPermission filePermissions = null;
                    if (permissionPaths.Count > 0)
                    {
                        filePermissions = new FileIOPermission(FileIOPermissionAccess.Read, permissionPaths.ToArray());
                    }

                    UpdateMetadataEntry(itemCollection, filePermissions);
                }
                finally
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }

        /// <summary>
        /// A metadata entry holding a StoreItemCollection and a StorageMappingItemCollection objects for the cache
        /// </summary>
        private class StoreMetadataEntry : MetadataEntry
        {
            private System.Data.Common.QueryCache.QueryCacheManager _queryCacheManager;

            /// <summary>
            /// The constructor for constructing this entry with an StoreItemCollection and a StorageMappingItemCollection
            /// </summary>
            /// <param name="compositeLoader">An instance of the composite MetadataArtifactLoader</param>
            internal StoreMetadataEntry()
            {
            }

            /// <summary>
            /// Gets the StorageMappingItemCollection for this entry
            /// </summary>
            internal StorageMappingItemCollection StorageMappingItemCollection
            {
                get
                {
                    return (StorageMappingItemCollection)this.ItemCollection;
                }
            }

            /// <summary>
            /// Load store specific metadata into the StoreItemCollection for this entry
            /// </summary>
            /// <param name="factory">The store-specific provider factory</param>
            /// <param name="edmItemCollection">edmItemCollection</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity")]
            internal void LoadStoreCollection(EdmItemCollection edmItemCollection, MetadataArtifactLoader loader)
            {
                StoreItemCollection storeItemCollection = null;
                IEnumerable<XmlReader> sSpaceXmlReaders = loader.CreateReaders(DataSpace.SSpace);
                try
                {
                    // Load the store side, however, only do so if we don't already have one
                    storeItemCollection = new StoreItemCollection(
                                    sSpaceXmlReaders,
                                    loader.GetPaths(DataSpace.SSpace));

                }
                finally
                {
                    Helper.DisposeXmlReaders(sSpaceXmlReaders);
                }

                // If this entry is getting re-used, make sure that the previous query cache manager gets
                // cleared up
                if (_queryCacheManager != null)
                {
                    _queryCacheManager.Clear();
                }

                // Update the query cache manager reference
                _queryCacheManager = storeItemCollection.QueryCacheManager;

                // With the store metadata in place, we can then load the mappings, however, only use it 
                // if we don't already have one
                //
                StorageMappingItemCollection storageMappingItemCollection = null;
                IEnumerable<XmlReader> csSpaceXmlReaders = loader.CreateReaders(DataSpace.CSSpace);
                try
                {
                    storageMappingItemCollection = new StorageMappingItemCollection(
                                                                        edmItemCollection,
                                                                        storeItemCollection,
                                                                        csSpaceXmlReaders,
                                                                        loader.GetPaths(DataSpace.CSSpace));
                }
                finally
                {
                    Helper.DisposeXmlReaders(csSpaceXmlReaders);
                }

                List<string> permissionPaths = new List<string>();
                loader.CollectFilePermissionPaths(permissionPaths, DataSpace.SSpace);
                loader.CollectFilePermissionPaths(permissionPaths, DataSpace.CSSpace);
                FileIOPermission filePermissions = null;
                if (permissionPaths.Count > 0)
                {
                    filePermissions = new FileIOPermission(FileIOPermissionAccess.Read, permissionPaths.ToArray());
                }
                this.UpdateMetadataEntry(storageMappingItemCollection, filePermissions);

            }

            /// <summary>
            /// Calls clear on query cache manager to make sure all the performance counters associated with the query
            /// cache are gone
            /// </summary>
            internal override void Clear()
            {
                // there can be entries in cache for which the store item collection was never created. For e.g.
                // if you create a new entity connection, but never call open on it
                CleanupQueryCache();
                base.Clear();
            }

            /// <summary>
            /// Cleans and Dispose query cache manager
            /// </summary>
            internal void CleanupQueryCache()
            {
                if (null != _queryCacheManager)
                {
                    _queryCacheManager.Dispose();
                    _queryCacheManager = null;
                }
            }

        }

        /// <summary>
        /// Interface to construct the metadata entry so that code can be reused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        interface IMetadataEntryConstructor<T>
        {
            T GetMetadataEntry();
        }

        /// <summary>
        /// Struct for creating EdmMetadataEntry
        /// </summary>
        private struct EdmMetadataEntryConstructor : IMetadataEntryConstructor<EdmMetadataEntry>
        {
            public EdmMetadataEntry GetMetadataEntry()
            {
                return new EdmMetadataEntry();
            }
        }

        /// <summary>
        /// Struct for creating StoreMetadataEntry
        /// </summary>
        private struct StoreMetadataEntryConstructor : IMetadataEntryConstructor<StoreMetadataEntry>
        {
            public StoreMetadataEntry GetMetadataEntry()
            {
                return new StoreMetadataEntry();
            }
        }

        /// <summary>
        /// Interface which constructs a new Item collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        interface IItemCollectionLoader<T> where T : MetadataEntry
        {
            void LoadItemCollection(T entry);
        }

        private struct EdmItemCollectionLoader : IItemCollectionLoader<EdmMetadataEntry>
        {

            private MetadataArtifactLoader _loader;

            public EdmItemCollectionLoader(MetadataArtifactLoader loader)
            {
                Debug.Assert(loader != null, "loader must never be null");
                _loader = loader;
            }
            
            /// <summary>
            /// Creates a new item collection and updates the entry with the item collection
            /// </summary>
            /// <param name="entry"></param>
            /// <returns></returns>
            public void LoadItemCollection(EdmMetadataEntry entry)
            {
                entry.LoadEdmItemCollection(_loader);
            }
        }

        private struct StoreItemCollectionLoader : IItemCollectionLoader<StoreMetadataEntry>
        {
            private EdmItemCollection _edmItemCollection;
            private MetadataArtifactLoader _loader;

            /// <summary>
            /// Constructs a struct from which you can load edm item collection
            /// </summary>
            /// <param name="factory"></param>
            /// <param name="edmItemCollection"></param>
            internal StoreItemCollectionLoader(EdmItemCollection edmItemCollection, MetadataArtifactLoader loader)
            {
                Debug.Assert(edmItemCollection != null, "EdmItemCollection must never be null");
                Debug.Assert(loader != null, "loader must never be null");
                //StoreItemCollection requires atleast one SSDL path.
                if ((loader.GetPaths(DataSpace.SSpace) == null) || (loader.GetPaths(DataSpace.SSpace).Count == 0))
                {
                    throw EntityUtil.Metadata(Strings.AtleastOneSSDLNeeded);
                }

                _edmItemCollection = edmItemCollection;
                _loader = loader;
            }

            public void LoadItemCollection(StoreMetadataEntry entry)
            {
                entry.LoadStoreCollection(_edmItemCollection, _loader);
            }
        }

        #endregion
    }
}
