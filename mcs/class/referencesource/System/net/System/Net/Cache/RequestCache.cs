/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCache.cs

Abstract:
    The file specifies interfaces used to communicate with Request Caching subsystem.


Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

    Jan 25 2004 - Changed the visibility of the class from public to internal.

--*/
namespace System.Net.Cache {
using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Threading;

    // The class specifies the contract for a caching storage to participate in the caching protocol.
    // The required functionality is to retrieve cached data and update cache based on a string as a Key.
    // It is also assumed that cache does storage quota management so it can remove expired cached entries under limited space conditions.
    // Note that no implementation methods should block as there is no Async API exposed by this type.
    internal abstract class RequestCache {
        internal static readonly char[] LineSplits = new char[] {'\r','\n'};

        private bool _IsPrivateCache;
        private bool _CanWrite;

        protected RequestCache( bool isPrivateCache, bool canWrite)
        {
            _IsPrivateCache = isPrivateCache;
            _CanWrite   = canWrite;
        }

        internal bool IsPrivateCache  {get {return _IsPrivateCache;}}
        internal bool CanWrite        {get {return _CanWrite;}}


        //  Returns a read data stream and metadata associated with a cached entry.
        //  Returns Stream.Null if there is no entry found.
        // <remarks> An opened cache entry be preserved until the stream is closed. </remarks>
        //
        internal abstract Stream  Retrieve(string key, out RequestCacheEntry cacheEntry);

        // Returns a write cache stream associated with the string Key.
        // Passed parameters allow cache to update an entry metadata accordingly.
        // <remarks>  The commit operation should happen on the stream closure. </remarks>
        //
        internal abstract Stream  Store(
                                        string           key,
                                        long             contentLength,
                                        DateTime         expiresUtc,
                                        DateTime         lastModifiedUtc,
                                        TimeSpan         maxStale,
                                        StringCollection entryMetadata,
                                        StringCollection systemMetadata
                                        );

        //
        // Removes an entry from the cache.
        //
        internal abstract void    Remove(string key);

        //
        // Updates only metadata associated with a cached entry.
        //
        internal abstract void    Update(
                                        string           key,
                                        DateTime         expiresUtc,
                                        DateTime         lastModifiedUtc,
                                        DateTime         lastSynchronizedUtc,
                                        TimeSpan         maxStale,
                                        StringCollection entryMetadata,
                                        StringCollection systemMetadata);


        //
        //  Does not throw on failure
        internal abstract bool    TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream  readStream);
        //
        //  Does not throw on failure
        internal abstract bool    TryStore(
                                        string           key,
                                        long             contentLength,
                                        DateTime         expiresUtc,
                                        DateTime         lastModifiedUtc,
                                        TimeSpan         maxStale,
                                        StringCollection entryMetadata,
                                        StringCollection systemMetadata,
                                        out Stream       writeStream);
        //
        //  Does not throw on failure
        internal abstract bool    TryRemove(string key);
        //
        //  Does not throw on failure
        internal abstract bool    TryUpdate(
                                        string           key,
                                        DateTime         expiresUtc,
                                        DateTime         lastModifiedUtc,
                                        DateTime         lastSynchronizedUtc,
                                        TimeSpan         maxStale,
                                        StringCollection entryMetadata,
                                        StringCollection systemMetadata);
        
        //
        // This can be looked as a design hole since we have to keep the entry
        // locked for the case when we want to update that previously retrieved entry.
        // I think RequestCache contract should allow to detect that a new physical cache entry
        // does not match to the "entry being updated" and so to should ignore updates on replaced entries.
        //
        internal abstract void UnlockEntry(Stream retrieveStream);

    }
}
