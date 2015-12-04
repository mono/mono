//------------------------------------------------------------------------------
// <copyright file="WinInetCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Win32 {
    using System;
    using System.Net;
    using System.Net.Cache;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Collections.Specialized;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ComponentModel;
    using System.Text;
    using System.Runtime.Versioning;
    using System.Diagnostics;

    // The class implements a RequestCache class contract on top of WinInet provider
    // Author: Alexei Vopilov    21-Dec-2002
    //
    // Revision History:
    //
    // Jan 25 2004  - Changed the visibility of the class from public to internal.

    internal class WinInetCache: RequestCache {
        private const int _MaximumResponseHeadersLength = Int32.MaxValue;
        private bool async;
        internal const string c_SPARSE_ENTRY_HACK = "~SPARSE_ENTRY:";

        private  readonly static DateTime   s_MinDateTimeUtcForFileTimeUtc = DateTime.FromFileTimeUtc(0L);
        internal readonly static TimeSpan   s_MaxTimeSpanForInt32 = TimeSpan.FromSeconds((double)int.MaxValue);

//        private  static readonly RequestCachePermission s_ReadPermission      = new RequestCachePermission(RequestCacheActions.CacheRead);
//        private  static readonly RequestCachePermission s_ReadWritePermission = new RequestCachePermission(RequestCacheActions.CacheReadWrite);

        /// <summary> A public constructor that demands CacheReadWrite flag for RequestCachePermission  </summary>
        internal WinInetCache(bool isPrivateCache, bool canWrite, bool async): base (isPrivateCache, canWrite)
        {
            /***********
            if (canWrite) {
                s_ReadWritePermission.Demand();
            }
            else
            {
                s_ReadPermission.Demand();
            }
            ***********/

            // Per VsWhidbey#88276 it was decided to not enforce any cache metadata limits for WinInet cache provider.
            //  ([....] 7/17 made this a const to avoid threading issues)
            //_MaximumResponseHeadersLength = Int32.MaxValue;
            this.async = async;

            /********
            if (_MaximumResponseHeadersLength == 0) {
                NetConfiguration config = (NetConfiguration)System.Configuration.ConfigurationManager.GetSection("system.net/settings");
                if (config != null) {
                    if (config.maximumResponseHeadersLength < 0 && config.maximumResponseHeadersLength != -1) {
                        throw new ArgumentOutOfRangeException(SR.GetString(SR.net_toosmall));
                    }
                    _MaximumResponseHeadersLength = config.maximumResponseHeadersLength * 1024;
                }
                else {
                    _MaximumResponseHeadersLength = 64 * 1024;
                }
            }
            ********/
        }

        /// <summary>
        /// <para>
        /// Gets the data stream and the metadata associated with a IE cache entry.
        /// Returns Stream.Null if there is no entry found.
        /// </para>
        /// </summary>
        internal override Stream Retrieve(string key, out RequestCacheEntry cacheEntry)
        {
            return Lookup(key, out cacheEntry, true);
        }
        //
        internal override bool TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream  readStream)
        {
            readStream = Lookup(key, out cacheEntry, false);
            if (readStream == null)
            {
                return false;
            }
            return true;
        }
        // Returns a write stream associated with the IE cache string Key.
        // Passed parameters allow cache to update an entry metadata accordingly.
        // <remarks>  The commit operation will happen upon the stream closure. </remarks>
        internal override Stream Store(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            return GetWriteStream(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, true);
        }
        // Does not throw on an error
        internal override bool TryStore(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, out Stream writeStream)
        {
            writeStream = GetWriteStream(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, false);
            if (writeStream == null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// <para>
        /// Removes an item from the IE cache. Throws Win32Excpetion if failed
        /// </para>
        /// </summary>
        internal override void Remove(string key) {

            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (!CanWrite)
            {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_operation_failed_with_error, "WinInetCache.Remove()", SR.GetString(SR.net_cache_access_denied, "Write")));
                return ;
            }

            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);

            if (_WinInetCache.Remove(entry) != _WinInetCache.Status.Success && entry.Error != _WinInetCache.Status.FileNotFound) {
                Win32Exception win32Exception = new Win32Exception((int)entry.Error);
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_cannot_remove, "WinInetCache.Remove()", key, win32Exception.Message));
                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
            }

            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_key_status, "WinInetCache.Remove(), ", key, entry.Error.ToString()));
        }
        //
        //  Tries to remove an item from the cache, possible by applying unsafe entry unlocking.
        //  Returns true if successful, false otherwise
        internal override bool TryRemove(string key)
        {
            return TryRemove(key, false);

        }
        //
        // Purges Wininet Cache Entry by Unlocking it's file until zero count (if forceRemove is set).
        //
        internal bool TryRemove(string key, bool forceRemove) {

            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (!CanWrite)
            {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_operation_failed_with_error, "WinInetCache.TryRemove()", SR.GetString(SR.net_cache_access_denied, "Write")));
                return false;
            }

            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);

            if (_WinInetCache.Remove(entry) == _WinInetCache.Status.Success || entry.Error == _WinInetCache.Status.FileNotFound) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_key_status, "WinInetCache.TryRemove()", key, entry.Error.ToString()));
                return true;
            }
            else if (!forceRemove) {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_key_remove_failed_status, "WinInetCache.TryRemove()", key, entry.Error.ToString()));
                return false;
            }

            _WinInetCache.Status status = _WinInetCache.LookupInfo(entry);
            if (status == _WinInetCache.Status.Success) {
                while (entry.Info.UseCount != 0) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_key_status, "WinInetCache.TryRemove()", key, entry.Error.ToString()));
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_usecount_file, "WinInetCache.TryRemove()", entry.Info.UseCount, entry.Filename));
                    if (!UnsafeNclNativeMethods.UnsafeWinInetCache.UnlockUrlCacheEntryFileW(key, 0)) {
                        break;
                    }
                    status = _WinInetCache.LookupInfo(entry);
                }
            }
            _WinInetCache.Remove(entry);
            if (entry.Error != _WinInetCache.Status.Success && _WinInetCache.LookupInfo(entry) == _WinInetCache.Status.FileNotFound) {
                entry.Error = _WinInetCache.Status.Success;
            }
            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_key_status, "WinInetCache.TryRemove()", key, entry.Error.ToString()));
            return entry.Error == _WinInetCache.Status.Success;
        }
        /// <summary>
        /// <para>
        /// Updates only the metadata associated with IE cached entry.
        /// </para>
        /// </summary>
        internal override void Update(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            UpdateInfo(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata, true);

        }
        // Does not throw on an error
        internal override bool TryUpdate(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            return UpdateInfo(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata, false);
        }
        //
        // Once the entry is unlocked it must not be updated
        // There is a design flaw in current RequestCache contract, it should allow detection of already replaced entry when updating one.
        //
        internal override void UnlockEntry(Stream stream)
        {
            ReadStream readStream = stream as ReadStream;

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_stream, "WinInetCache.UnlockEntry",  (stream == null ? "<null>" : stream.GetType().FullName)));

            // could be wrapped by some other stream, that's ok because the entry is unlocked on stream.Close anyway
            if (readStream == null)
                return;
            readStream.UnlockEntry();
        }
        //
        //
        //
        private Stream Lookup(string key, out RequestCacheEntry cacheEntry, bool isThrow)
        {
            if(Logging.On) Logging.Enter(Logging.RequestCache, "WinInetCache.Retrieve", "key = " + key);

            if (key == null) {
                throw new ArgumentNullException("key");
            }

            Stream result = Stream.Null;
            SafeUnlockUrlCacheEntryFile handle = null;
            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
            try {
                handle = _WinInetCache.LookupFile(entry);

                if (entry.Error == _WinInetCache.Status.Success) {
                    if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_filename, "WinInetCache.Retrieve()", entry.Filename, entry.Error));

                    cacheEntry = new RequestCacheEntry(entry, IsPrivateCache);

                    if (entry.MetaInfo != null && entry.MetaInfo.Length != 0)
                    {
                        // convert metadata to upto two string collections
                        unsafe
                        {
                            int start = 0;
                            int length = entry.MetaInfo.Length;
                            StringCollection sc = new StringCollection();
                            fixed (char * ch = entry.MetaInfo)
                            {
                                int i;
                                for (i = 0; i < length; ++i)
                                {
                                    // WinInet specific block!!
                                    // The point here is that wininet scans for ~U: throughly with no regard to \r\n so we mimic the same behavior
                                    if (i == start && i+2 < length)
                                    {
                                        if (ch[i] == '~' && (ch[i+1] == 'U' || ch[i+1] == 'u') && ch[i+2] == ':')
                                        {
                                            //Security: don't report what the username is
                                            while(i < length && ch[++i] != '\n') {;}
                                            start = i+1;
                                            continue;
                                        }

                                    }

                                    // note a metadata entry must terminate with \r\n

                                    if ((i+1 == length) || (ch[i] == '\n'))
                                    {
                                        string value = entry.MetaInfo.Substring(start, (ch[i-1] == '\r'? (i-1):(i+1)) - start);

                                        if (value.Length == 0 && cacheEntry.EntryMetadata == null)
                                        {
                                            // done with headers, prepare for system metadata
                                            cacheEntry.EntryMetadata = sc;
                                            sc = new StringCollection();
                                        }
                                        else
                                        {
                                            //WinInet specific block!!
                                            // HACK: if we are parsing system metadata and have found our hack,
                                            // then convert it to a sparse entry type (entry.Info.EntryType & _WinInetCache.EntryType.Sparse)
                                            if (cacheEntry.EntryMetadata != null && value.StartsWith(c_SPARSE_ENTRY_HACK, StringComparison.Ordinal))
                                                cacheEntry.IsPartialEntry = true;
                                            else
                                                sc.Add(value);
                                        }
                                        start = i+1;
                                    }
                                }
                            }
                            if (cacheEntry.EntryMetadata == null )
                                {cacheEntry.EntryMetadata = sc;}
                            else
                                {cacheEntry.SystemMetadata = sc;}
                        }
                    }

                    result = new ReadStream(entry, handle, async);

                }
                else {
                    if (handle != null) {
                        handle.Close();
                    }

                    cacheEntry = new RequestCacheEntry();
                    cacheEntry.IsPrivateEntry = IsPrivateCache;

                    if (entry.Error != _WinInetCache.Status.FileNotFound)
                    {
                        if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_lookup_failed, "WinInetCache.Retrieve()", new Win32Exception((int)entry.Error).Message));
                        if(Logging.On)Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()");
                        if(isThrow)
                        {
                            Win32Exception win32Exception = new Win32Exception((int)entry.Error);
                            throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
                        }
                        return null;
                    }
                }
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                    throw;
                }

                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_exception, "WinInetCache.Retrieve()", exception.ToString()));
                if(Logging.On)Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()");

                if (handle != null) {
                    handle.Close();
                }
                result.Close();
                result = Stream.Null;
                cacheEntry = new RequestCacheEntry();
                cacheEntry.IsPrivateEntry = IsPrivateCache;
                if (isThrow)
                {
                    throw;
                }
                return null;
            }
            if(Logging.On)Logging.Exit(Logging.RequestCache, "WinInetCache.Retrieve()", "Status = " + entry.Error.ToString());
            return result;
        }
        //
        //
        //
        private string CombineMetaInfo(StringCollection entryMetadata, StringCollection systemMetadata)
        {
            if ((entryMetadata == null || entryMetadata.Count == 0) && (systemMetadata == null || systemMetadata.Count == 0))
                return string.Empty;

            StringBuilder sb = new StringBuilder(100);
            int i;
            if (entryMetadata != null && entryMetadata.Count != 0)
                for (i = 0; i < entryMetadata.Count; ++i)
                {
                    if (entryMetadata[i] == null || entryMetadata[i].Length == 0)
                        continue;
                    sb.Append(entryMetadata[i]).Append("\r\n");
                }

            if (systemMetadata != null && systemMetadata.Count != 0)
            {
                // mark a start for system metadata
                sb.Append("\r\n");
                for (i = 0; i < systemMetadata.Count; ++i)
                {
                    if (systemMetadata[i] == null || systemMetadata[i].Length == 0)
                        continue;
                    sb.Append(systemMetadata[i]).Append("\r\n");}
            }

            return sb.ToString();
        }
        //
        //
        private Stream GetWriteStream(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isThrow)
        {
            if(Logging.On) Logging.Enter(Logging.RequestCache, "WinInetCache.Store()", "Key = " + key);

            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if (!CanWrite)
            {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_operation_failed_with_error, "WinInetCache.Store()", SR.GetString(SR.net_cache_access_denied, "Write")));
                if(Logging.On) Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                if(isThrow)
                {
                    throw new InvalidOperationException(SR.GetString(SR.net_cache_access_denied, "Write"));
                }
                return null;
            }


            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);

            entry.Key = key;
            entry.OptionalLength = (contentLength < 0L)? 0: contentLength > Int32.MaxValue? Int32.MaxValue: (int)(contentLength);

            entry.Info.ExpireTime = _WinInetCache.FILETIME.Zero;
            if (expiresUtc != DateTime.MinValue && expiresUtc > s_MinDateTimeUtcForFileTimeUtc) {
                entry.Info.ExpireTime  = new _WinInetCache.FILETIME(expiresUtc.ToFileTimeUtc());
            }

            entry.Info.LastModifiedTime = _WinInetCache.FILETIME.Zero;
            if (lastModifiedUtc != DateTime.MinValue && lastModifiedUtc > s_MinDateTimeUtcForFileTimeUtc) {
                entry.Info.LastModifiedTime = new _WinInetCache.FILETIME(lastModifiedUtc.ToFileTimeUtc());
            }

            entry.Info.EntryType = _WinInetCache.EntryType.NormalEntry;
            if (maxStale > TimeSpan.Zero) {
                if (maxStale >= s_MaxTimeSpanForInt32) {
                    maxStale = s_MaxTimeSpanForInt32;
                }
                entry.Info.U.ExemptDelta = (int)maxStale.TotalSeconds;
                entry.Info.EntryType = _WinInetCache.EntryType.StickyEntry;
            }


            entry.MetaInfo = CombineMetaInfo(entryMetadata, systemMetadata);

            entry.FileExt = "cache";
            if(Logging.On) {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_expected_length, entry.OptionalLength));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_last_modified, (entry.Info.LastModifiedTime.IsNull? "0": DateTime.FromFileTimeUtc(entry.Info.LastModifiedTime.ToLong()).ToString("r"))));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_expires, (entry.Info.ExpireTime.IsNull? "0": DateTime.FromFileTimeUtc(entry.Info.ExpireTime.ToLong()).ToString("r"))));
                Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_stale, (maxStale > TimeSpan.Zero? ((int)maxStale.TotalSeconds).ToString():"n/a")));
                if (Logging.IsVerbose(Logging.RequestCache)) {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_dumping_metadata));
                    if (entry.MetaInfo.Length == 0) {
                        Logging.PrintInfo(Logging.RequestCache, "<null>");
                    }
                    else {
                        if (entryMetadata != null) {
                            foreach (string s in entryMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, s.TrimEnd(LineSplits));
                            }
                        }
                        Logging.PrintInfo(Logging.RequestCache, "------");
                        if (systemMetadata != null) {
                            foreach (string s in systemMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, s.TrimEnd(LineSplits));
                            }
                        }
                    }
                }
            }

            _WinInetCache.CreateFileName(entry);

            Stream result = Stream.Null;
            if (entry.Error != _WinInetCache.Status.Success) {
                if(Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_create_failed, new Win32Exception((int)entry.Error).Message));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                }
                if (isThrow)
                {
                    Win32Exception win32Exception = new Win32Exception((int)entry.Error);
                    throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
                }
                return null;
            }


            try {
                result = new WriteStream(entry, isThrow, contentLength, async);
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                    throw;
                }

                if(Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_exception, "WinInetCache.Store()", exception));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Store");
                }
                if (isThrow)
                {
                    throw;
                }
                return null;
            }

            if(Logging.On) Logging.Exit(Logging.RequestCache, "WinInetCache.Store", "Filename = " + entry.Filename);
            return result;
        }
        //
        //
        private bool UpdateInfo(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isThrow)
        {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            if(Logging.On) Logging.Enter(Logging.RequestCache, "WinInetCache.Update", "Key = "+ key);

            if (!CanWrite)
            {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_operation_failed_with_error, "WinInetCache.Update()", SR.GetString(SR.net_cache_access_denied, "Write")));
                if(Logging.On) Logging.Exit(Logging.RequestCache, "WinInetCache.Update()");
                if(isThrow)
                {
                    throw new InvalidOperationException(SR.GetString(SR.net_cache_access_denied, "Write"));
                }
                return false;
            }

            _WinInetCache.Entry entry = new _WinInetCache.Entry(key, _MaximumResponseHeadersLength);
            _WinInetCache.Entry_FC attributes =  _WinInetCache.Entry_FC.None;

            if (expiresUtc != DateTime.MinValue && expiresUtc > s_MinDateTimeUtcForFileTimeUtc) {
                attributes |= _WinInetCache.Entry_FC.Exptime;
                entry.Info.ExpireTime = new _WinInetCache.FILETIME(expiresUtc.ToFileTimeUtc());
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_set_expires, expiresUtc.ToString("r")));
            }

            if (lastModifiedUtc != DateTime.MinValue && lastModifiedUtc > s_MinDateTimeUtcForFileTimeUtc) {
                attributes |= _WinInetCache.Entry_FC.Modtime;
                entry.Info.LastModifiedTime = new _WinInetCache.FILETIME(lastModifiedUtc.ToFileTimeUtc());
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_set_last_modified, lastModifiedUtc.ToString("r")));
            }

            if (lastSynchronizedUtc != DateTime.MinValue && lastSynchronizedUtc > s_MinDateTimeUtcForFileTimeUtc) {
                attributes |= _WinInetCache.Entry_FC.Synctime;
                entry.Info.LastSyncTime = new _WinInetCache.FILETIME(lastSynchronizedUtc.ToFileTimeUtc());
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_set_last_synchronized, lastSynchronizedUtc.ToString("r")));
            }

            if (maxStale != TimeSpan.MinValue) {
                attributes |= _WinInetCache.Entry_FC.ExemptDelta|_WinInetCache.Entry_FC.Attribute;
                entry.Info.EntryType = _WinInetCache.EntryType.NormalEntry;
                if (maxStale >= TimeSpan.Zero) {
                    if (maxStale >= s_MaxTimeSpanForInt32) {
                        maxStale = s_MaxTimeSpanForInt32;
                    }
                    entry.Info.EntryType = _WinInetCache.EntryType.StickyEntry;
                    entry.Info.U.ExemptDelta = (int)maxStale.TotalSeconds;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_enable_max_stale, ((int)maxStale.TotalSeconds).ToString()));
                }
                else {
                    entry.Info.U.ExemptDelta = 0;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_disable_max_stale));
                }
            }

            entry.MetaInfo = CombineMetaInfo(entryMetadata, systemMetadata);
            if (entry.MetaInfo.Length != 0) {
                attributes |= _WinInetCache.Entry_FC.Headerinfo;

                if(Logging.On) {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_dumping));
                    if (Logging.IsVerbose(Logging.RequestCache)) {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_dumping));
                        if (entryMetadata != null) {
                            foreach (string s in entryMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, s.TrimEnd(LineSplits));
                            }
                        }
                        Logging.PrintInfo(Logging.RequestCache, "------");
                        if (systemMetadata != null) {
                            foreach (string s in systemMetadata)
                            {
                                Logging.PrintInfo(Logging.RequestCache, s.TrimEnd(LineSplits));
                            }
                        }
                    }
                }
            }

            _WinInetCache.Update(entry, attributes) ;

            if (entry.Error != _WinInetCache.Status.Success) {
                if(Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_update_failed, "WinInetCache.Update()", entry.Key, new Win32Exception((int)entry.Error).Message));
                    Logging.Exit(Logging.RequestCache, "WinInetCache.Update()");
                }
                if (isThrow)
                {
                    Win32Exception win32Exception = new Win32Exception((int)entry.Error);
                    throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
                }
                return false;
            }

            if(Logging.On) Logging.Exit(Logging.RequestCache, "WinInetCache.Update()", "Status = " + entry.Error.ToString());
            return true;
        }

        /// <summary>
        ///    <para>
        ///         This is a FileStream wrapper on top of WinInet cache entry.
        //          The Close method will unlock the cached entry.
        ///    </para>
        ///</summary>
        private class ReadStream: FileStream, ICloseEx, IRequestLifetimeTracker {
            private string m_Key;
            private int m_ReadTimeout;
            private int m_WriteTimeout;
            private SafeUnlockUrlCacheEntryFile m_Handle;
            private int m_Disposed;
            private int m_CallNesting;
            private ManualResetEvent m_Event;
            private bool m_Aborted;
            private RequestLifetimeSetter m_RequestLifetimeSetter;

            //
            // Construct a read stream out of WinInet given handle
            //
            [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            internal ReadStream(_WinInetCache.Entry entry, SafeUnlockUrlCacheEntryFile handle, bool async)
                    : base(entry.Filename, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 4096, async)
            {
                m_Key = entry.Key;
                m_Handle = handle;
                m_ReadTimeout = m_WriteTimeout = System.Threading.Timeout.Infinite;
            }
            //
            // The stream will remain valid but after that call the entry can be replaced.
            // If the entry has been replaced then the physical file that this stream points to may be deleted on stream.Close()
            //
            internal void UnlockEntry()
            {
                m_Handle.Close();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (m_Handle)
                {
                    try {
                        if (m_CallNesting != 0)
                            throw new NotSupportedException(SR.GetString(SR.net_no_concurrent_io_allowed));
                        if (m_Aborted)
                            throw ExceptionHelper.RequestAbortedException;
                        if (m_Event != null)
                            throw new ObjectDisposedException(GetType().FullName);

                        m_CallNesting = 1;
                        return base.Read(buffer, offset, count);
                    }
                    finally {
                        m_CallNesting = 0;
                        if (m_Event != null)
                            m_Event.Set();
                    }
                }
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                lock (m_Handle)
                {
                    if (m_CallNesting != 0)
                        throw new NotSupportedException(SR.GetString(SR.net_no_concurrent_io_allowed));
                    if (m_Aborted)
                        throw ExceptionHelper.RequestAbortedException;
                    if (m_Event != null)
                        throw new ObjectDisposedException(GetType().FullName);

                    m_CallNesting = 1;
                    try {
                        return base.BeginRead(buffer, offset, count, callback, state);
                    }
                    catch {
                        m_CallNesting = 0;
                        throw;
                    }
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                lock (m_Handle)
                {
                    try {
                        return base.EndRead(asyncResult);
                    }
                    finally {
                        m_CallNesting = 0;
                        if (m_Event != null)
                            try {m_Event.Set();} catch {}   // the problem is he WaitHandle cannot tell if it is disposed or not
                    }
                }
            }

            public void CloseEx(CloseExState closeState)
            {
                if ((closeState & CloseExState.Abort) != 0)
                    m_Aborted = true;

                try {
                    Close();
                }
                catch {
                    if ((closeState & CloseExState.Silent) == 0)
                        throw;
                }
            }

            protected override void Dispose(bool disposing) 
            {                
                if (Interlocked.Exchange(ref m_Disposed, 1) == 0)
                {
                    if (!disposing)
                    {
                        base.Dispose(false);
                    }
                    else
                    {
                        // if m_key is null, it means that the base constructor failed
                        if (m_Key != null)
                        {
                            try
                            {
                                lock (m_Handle)
                                {
                                    if (m_CallNesting == 0)
                                        base.Dispose(true);
                                    else
                                        m_Event = new ManualResetEvent(false);
                                }

                                RequestLifetimeSetter.Report(m_RequestLifetimeSetter);

                                if (m_Event != null)
                                {
                                    using (m_Event)
                                    {
                                        // This assumes that FileStream will never hang on read
                                        m_Event.WaitOne();
                                        lock (m_Handle)
                                        {
                                            Debug.Assert(m_CallNesting == 0);
                                        }
                                    }
                                    base.Dispose(true);
                                }
                            }
                            finally
                            {
                                if (Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_key, "WinInetReadStream.Close()", m_Key));
                                // note, the handle may have been closed earlier if CacheProtocol knew that cache metadata update will not happen.
                                m_Handle.Close();
                            }
                        }
                    }
                }
            }

            public override bool CanTimeout {
                get {
                    return true;
                }
            }
            public override int ReadTimeout {
                get {
                    return m_ReadTimeout;
                }
                set {
                    m_ReadTimeout = value;
                }
            }
            public override int WriteTimeout {
                get {
                    return m_WriteTimeout;
                }
                set {
                    m_WriteTimeout = value;
                }
            }

            void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
            {
                Debug.Assert(m_RequestLifetimeSetter == null, "TrackRequestLifetime called more than once.");
                m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
            }
        }
        //
        //
        //
        private class WriteStream: FileStream, ICloseEx {
            private _WinInetCache.Entry m_Entry;
            private bool m_IsThrow;
            private long m_StreamSize;
            private bool m_Aborted;
            private int m_ReadTimeout;
            private int m_WriteTimeout;
            private int m_Disposed;
            private int m_CallNesting;
            private ManualResetEvent m_Event;
            private bool m_OneWriteSucceeded;


            [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            internal WriteStream(_WinInetCache.Entry entry, bool isThrow, long streamSize, bool async):
                    base(entry.Filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, async) {

                m_Entry = entry;
                m_IsThrow = isThrow;
                m_StreamSize = streamSize;
                m_OneWriteSucceeded = streamSize == 0; //if 0 is expected or the lenght is unknonw we will commit even an emtpy stream.
                m_ReadTimeout = m_WriteTimeout = System.Threading.Timeout.Infinite;
            }
            //
            public override bool CanTimeout {
                get {
                    return true;
                }
            }
            public override int ReadTimeout {
                get {
                    return m_ReadTimeout;
                }
                set {
                    m_ReadTimeout = value;
                }
            }
            public override int WriteTimeout {
                get {
                    return m_WriteTimeout;
                }
                set {
                    m_WriteTimeout = value;
                }
            }
            //
            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (m_Entry)
                {
                    if (m_Aborted)
                        throw ExceptionHelper.RequestAbortedException;
                    if (m_Event != null)
                        throw new ObjectDisposedException(GetType().FullName);

                    m_CallNesting = 1;
                    try {
                        base.Write(buffer, offset, count);
                        if (m_StreamSize > 0)
                            m_StreamSize -= count;
                        if (!m_OneWriteSucceeded && count != 0)
                            m_OneWriteSucceeded = true;
                    }
                    catch {
                        m_Aborted = true;
                        throw;
                    }
                    finally {
                        m_CallNesting = 0;
                        if (m_Event != null)
                            m_Event.Set();
                    }
                }
            }
            //
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                lock (m_Entry)
                {
                    if (m_CallNesting != 0)
                        throw new NotSupportedException(SR.GetString(SR.net_no_concurrent_io_allowed));
                    if (m_Aborted)
                        throw ExceptionHelper.RequestAbortedException;
                    if (m_Event != null)
                        throw new ObjectDisposedException(GetType().FullName);

                    m_CallNesting = 1;

                    try {
                        if (m_StreamSize > 0)
                            m_StreamSize -= count;
                        return base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch {
                        m_Aborted = true;
                        m_CallNesting = 0;
                        throw;
                    }
                }

            }
            //
            public override void EndWrite(IAsyncResult asyncResult)
            {
                lock (m_Entry)
                {
                    try {
                        base.EndWrite(asyncResult);
                        if (!m_OneWriteSucceeded)
                            m_OneWriteSucceeded = true;
                    }
                    catch {
                        m_Aborted = true;
                        throw;
                    }
                    finally {
                        m_CallNesting = 0;
                        if (m_Event != null)
                            try {m_Event.Set();} catch {}   // the problem is he WaitHandle cannot tell if it is disposed or not
                    }
                }
            }
            //
            public void CloseEx(CloseExState closeState)
            {
                // For abnormal stream termination we will commit a partial cache entry
                if ((closeState & CloseExState.Abort) != 0)
                    m_Aborted = true;

                try {
                    Close();
                }
                catch {
                    if ((closeState & CloseExState.Silent) == 0)
                        throw;
                }
            }
            //
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            protected override void Dispose(bool disposing)
            {
                //if m_Entry is null, it means that the base constructor failed
                if (Interlocked.Exchange(ref m_Disposed, 1) == 0 && m_Entry != null) {

                    lock (m_Entry)
                    {
                        if (m_CallNesting == 0)
                            base.Dispose(disposing);
                        else
                            m_Event = new ManualResetEvent(false);
                    }

                    //
                    // This assumes the FileStream will never hang on write
                    //
                    if (disposing && m_Event != null)
                    {
                        using (m_Event)
                        {
                            m_Event.WaitOne();
                            lock (m_Entry) 
                            {
                                Debug.Assert(m_CallNesting == 0);
                            }
                        }
                        base.Dispose(disposing);
                    }

                    // We use TriState to indicate:
                    //     False:   Delete
                    //     Unknown: Partial
                    //     True:    Full
                    TriState cacheCommitAction;
                    if (m_StreamSize < 0)
                    {
                        if (m_Aborted)
                        {
                            if (m_OneWriteSucceeded)
                                cacheCommitAction = TriState.Unspecified; // Partial
                            else
                                cacheCommitAction = TriState.False; // Delete
                        }
                        else
                        {
                            cacheCommitAction = TriState.True; // Full
                        }
                    }
                    else
                    {
                        if (!m_OneWriteSucceeded)
                        {
                            cacheCommitAction = TriState.False; // Delete
                        }
                        else
                        {
                            if (m_StreamSize > 0)
                                cacheCommitAction = TriState.Unspecified; // Partial
                            else
                                cacheCommitAction = TriState.True; // Full
                        }
                    }

                    if (cacheCommitAction == TriState.False)
                    {
                        try
                        {
                            if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_commit, "WinInetWriteStream.Close()"));
                            // Delete temp cache file
                            File.Delete(m_Entry.Filename);
                        }
                        catch (Exception exception)
                        {
                            if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                                throw;
                            }
                            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_error_deleting_filename, "WinInetWriteStream.Close()", m_Entry.Filename));
                        }
                        finally {
                            //Delete an old entry if there was one
                            _WinInetCache.Status errorStatus = _WinInetCache.Remove(m_Entry);
                            if (errorStatus != _WinInetCache.Status.Success && errorStatus != _WinInetCache.Status.FileNotFound)
                            {
                                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_delete_failed, "WinInetWriteStream.Close()", m_Entry.Key, new Win32Exception((int)m_Entry.Error).Message));
                            }

                            m_Entry = null;
                        }
                        return;
                    }

                    m_Entry.OriginalUrl = null;

                    //
                    // ATTN: WinIent currently does NOT support _WinInetCache.EntryType.Sparse
                    // USING a workaround
                    //
                    if (cacheCommitAction == TriState.Unspecified)
                    {
    // WinInet will not report this entry back we set this flag
    //                    m_Entry.Info.EntryType |= _WinInetCache.EntryType.Sparse;  // does not work for now

                        // HACK: WinInet does not support SPARSE_ENTRY bit
                        // We want to add c_SPARSE_ENTRY_HACK into the systemmetadata i.e. to the second block of strings separated by an empty line (\r\n).
                        if (m_Entry.MetaInfo == null || m_Entry.MetaInfo.Length == 0 ||
                            (m_Entry.MetaInfo != "\r\n" && m_Entry.MetaInfo.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1))
                        {
                            m_Entry.MetaInfo = "\r\n"+ WinInetCache.c_SPARSE_ENTRY_HACK+"\r\n";
                        }
                        else
                        {
                            m_Entry.MetaInfo += WinInetCache.c_SPARSE_ENTRY_HACK+"\r\n";
                        }
                    }

                    if (_WinInetCache.Commit(m_Entry) != _WinInetCache.Status.Success)
                    {
                        if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_commit_failed, "WinInetWriteStream.Close()", m_Entry.Key, new Win32Exception((int)m_Entry.Error).Message));
                        try
                        {
                            // Delete temp cache file
                            File.Delete(m_Entry.Filename);
                        }
                        catch (Exception exception)
                        {
                            if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                                throw;
                            }
                            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_error_deleting_filename, "WinInetWriteStream.Close()", m_Entry.Filename));
                        }

                        if (m_IsThrow)
                        {
                            Win32Exception win32Exception = new Win32Exception((int)m_Entry.Error);
                            throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
                        }
                        return;
                    }

                    if(Logging.On)
                    {
                        if (m_StreamSize > 0 || (m_StreamSize < 0 && m_Aborted))
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_committed_as_partial, "WinInetWriteStream.Close()", m_Entry.Key, (m_StreamSize > 0 ? m_StreamSize.ToString(CultureInfo.CurrentCulture) : SR.GetString(SR.net_log_unknown))));
                        Logging.PrintInfo(Logging.RequestCache, "WinInetWriteStream.Close(), Key = " + m_Entry.Key + ", Commit Status = " + m_Entry.Error.ToString());
                    }


                    if ((m_Entry.Info.EntryType & _WinInetCache.EntryType.StickyEntry) == _WinInetCache.EntryType.StickyEntry)
                    {
                        if (_WinInetCache.Update(m_Entry, _WinInetCache.Entry_FC.ExemptDelta) != _WinInetCache.Status.Success)
                        {
                            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_update_failed, "WinInetWriteStream.Close(), Key = " + m_Entry.Key, new Win32Exception((int)m_Entry.Error).Message));
                            if (m_IsThrow)
                            {
                                Win32Exception win32Exception = new Win32Exception((int)m_Entry.Error);
                                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, win32Exception.Message), win32Exception);
                            }
                            return;
                        }
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_stale_and_update_status, "WinInetWriteFile.Close()", m_Entry.Info.U.ExemptDelta, m_Entry.Error.ToString()));
                    }

                    base.Dispose(disposing);
                }
            }
        }
    }
}



