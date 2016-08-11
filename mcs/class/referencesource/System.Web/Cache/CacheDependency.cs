//------------------------------------------------------------------------------
// <copyright file="CacheDependency.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * CacheDependency.cs
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */


namespace System.Web.Caching {
    using System.Collections;
    using System.Text;
    using System.IO;
    using System.Threading;
    using System.Web.Util;
    using System.Security.Permissions;
    using System.Globalization;
#if USE_MEMORY_CACHE
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Caching;
#endif

    internal interface ICacheDependencyChanged {
        void DependencyChanged(Object sender, EventArgs e);
    }


    /// <devdoc>
    /// <para>The <see langword='CacheDependency'/> class tracks cache dependencies, which can be files, 
    ///    directories, or keys to other objects in the System.Web.Cache.Cache. When an object of this class
    ///    is constructed, it immediately begins monitoring objects on which it is
    ///    dependent for changes. This avoids losing the changes made between the time the
    ///    object to cache is created and the time it is inserted into the
    /// <see langword='Cache'/>.</para>
    /// </devdoc>

    // Overhead is 24 bytes + object header
    public class CacheDependency : IDisposable {

#if DBG
        bool                       _isUniqueIDInitialized;
#endif

        string                     _uniqueID;              // used by HttpCachePolicy for the ETag
        object                     _depFileInfos;          // files to monitor for changes, either a DepFileInfo or array of DepFileInfos 
        object                     _entries;               // cache entries we are dependent on, either a string or array of strings 
        ICacheDependencyChanged    _objNotify;             // Associated object to notify when a change occurs 
        SafeBitVector32            _bits;                  // status bits for ready, used, changed, disposed  
        DateTime                   _utcLastModified;       // Time of last modified item
#if USE_MEMORY_CACHE
        HostFileChangeMonitor _fileChangeMonitor;
        CacheEntryChangeMonitor _entryChangeMonitor;
#endif

        static readonly string[]        s_stringsEmpty;
        static readonly CacheEntry[]    s_entriesEmpty;
        static readonly CacheDependency s_dependencyEmpty;
        static readonly DepFileInfo[]   s_depFileInfosEmpty;

        static readonly TimeSpan        FUTURE_FILETIME_BUFFER = new TimeSpan(0, 1, 0); // See VSWhidbey 400917

        const int BASE_INIT             = 0x01;
        const int USED                  = 0x02;
        const int CHANGED               = 0x04;
        const int BASE_DISPOSED         = 0x08;
        const int WANTS_DISPOSE         = 0x10;
        const int DERIVED_INIT          = 0x20;
        const int DERIVED_DISPOSED      = 0x40;

        internal class DepFileInfo {
            internal string             _filename;
            internal FileAttributesData _fad;
        }

        static CacheDependency() {
            s_stringsEmpty = new string[0];
            s_entriesEmpty = new CacheEntry[0];
            s_dependencyEmpty = new CacheDependency(0);
            s_depFileInfosEmpty = new DepFileInfo[0];
        }

        // creates an empty dependency which is used only by s_dependencyEmpty
        private CacheDependency(int bogus) {
            Debug.Assert(s_dependencyEmpty == null, "s_dependencyEmpty == null");
        }



        protected CacheDependency() {
            Init(true, null, null, null, DateTime.MaxValue);
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the System.Web.Cache.CacheDependency class. The new instance 
        ///    monitors a file or directory for changes.</para>
        /// </devdoc>
        public CacheDependency(string filename) :
            this (filename, DateTime.MaxValue) {
        }
            

        public CacheDependency(string filename, DateTime start) {
            if (filename == null) {
                return;
            }

            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            string[] filenames = new string[1] {filename};
            Init(true, filenames, null, null, utcStart);

        }


        /// <devdoc>
        /// <para>Initializes a new instance of the System.Web.Cache.CacheDependency class. The new instance monitors an array 
        ///    files or directories for changes.</para>
        /// </devdoc>
        public CacheDependency(string[] filenames) {
            Init(true, filenames, null, null, DateTime.MaxValue);
        }


        public CacheDependency(string[] filenames, DateTime start) {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            Init(true, filenames, null, null, utcStart);
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the System.Web.Cache.CacheDependency class. The new instance monitors an 
        ///    array files, directories, and cache keys for changes.</para>
        /// </devdoc>
        public CacheDependency(string[] filenames, string[] cachekeys) {
            Init(true, filenames, cachekeys, null, DateTime.MaxValue);
        }


        public CacheDependency(string[] filenames, string[] cachekeys, DateTime start) {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            Init(true, filenames, cachekeys, null, utcStart);
        }


        public CacheDependency(string[] filenames, string[] cachekeys, CacheDependency dependency) {
            Init(true, filenames, cachekeys, dependency, DateTime.MaxValue);
        }


        public CacheDependency(string[] filenames, string[] cachekeys, CacheDependency dependency, DateTime start) {
            DateTime utcStart = DateTimeUtil.ConvertToUniversalTime(start);
            Init(true, filenames, cachekeys, dependency, utcStart);
        }

        internal CacheDependency(int dummy, string filename) :
            this(dummy, filename, DateTime.MaxValue) {
        }
            
        internal CacheDependency(int dummy, string filename, DateTime utcStart) {
            if (filename == null) {
                return;
            }

            string[] filenames = new string[1] {filename};
            Init(false, filenames, null, null, utcStart);

        }

        internal CacheDependency(int dummy, string[] filenames) {
            Init(false, filenames, null, null, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string[] filenames, DateTime utcStart) {
            Init(false, filenames, null, null, utcStart);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys) {
            Init(false, filenames, cachekeys, null, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, DateTime utcStart) {
            Init(false, filenames, cachekeys, null, utcStart);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, CacheDependency dependency) {
            Init(false, filenames, cachekeys, dependency, DateTime.MaxValue);
        }

        internal CacheDependency(int dummy, string[] filenames, string[] cachekeys, CacheDependency dependency, DateTime utcStart) {
            Init(false, filenames, cachekeys, dependency, utcStart);
        }

#if USE_MEMORY_CACHE
        void OnChangedCallback(object state) {
            Debug.Trace("CacheDependencyFileChange", "OnChangedCallback fired");
            NotifyDependencyChanged(this, EventArgs.Empty);            
        }

        void InitForMemoryCache(bool isPublic, string[] filenamesArg, string[] cachekeysArg, CacheDependency dependency, DateTime utcStart) {
            bool dispose = true;
            try {
                MemCache memCache = HttpRuntime.CacheInternal as MemCache;
                _bits = new SafeBitVector32(0);
                _utcLastModified = DateTime.MinValue;
                IList<String> files = filenamesArg;
                IList<String> keys = cachekeysArg;
                if (dependency != null) {
                    ReadOnlyCollection<string> filePaths = (dependency._fileChangeMonitor != null) ? dependency._fileChangeMonitor.FilePaths : null;
                    ReadOnlyCollection<string> cacheKeys = (dependency._entryChangeMonitor != null) ? dependency._entryChangeMonitor.CacheKeys : null;
                    if (filePaths != null || filenamesArg != null) {
                        if (filePaths == null) {
                            files = filenamesArg;
                        }
                        else if (filenamesArg == null) {
                            files = filePaths;
                        }
                        else {
                            files = new List<String>(filenamesArg.Length + filePaths.Count);
                            foreach (string f in filenamesArg) {
                                files.Add(f);
                            }
                            foreach (string f in filePaths) {
                                files.Add(f);
                            }
                        }
                    }
                    if (cacheKeys != null || cachekeysArg != null) {
                        if (cacheKeys == null) {
                            keys = cachekeysArg;
                        }
                        else if (cachekeysArg == null) {
                            keys = cacheKeys;
                        }
                        else {
                            keys = new List<String>(cachekeysArg.Length + cacheKeys.Count);
                            foreach (string f in cachekeysArg) {
                                keys.Add(f);
                            }
                            foreach (string f in cacheKeys) {
                                keys.Add(f);
                            }
                        }
                    }
                }
                
                _fileChangeMonitor = (files != null) ? new HostFileChangeMonitor(files) : null;
                _entryChangeMonitor = (keys != null) ? memCache.CreateCacheEntryChangeMonitor(keys, isPublic) : null;
                
                string uniqueId = null;
                
                if (_fileChangeMonitor != null) {
                    _utcLastModified = _fileChangeMonitor.LastModified.UtcDateTime;
                    uniqueId = _fileChangeMonitor.UniqueId;
                    _fileChangeMonitor.NotifyOnChanged(new OnChangedCallback(OnChangedCallback));
                }
                if (_entryChangeMonitor != null) {
                    DateTime utcLastModified = _entryChangeMonitor.LastModified.UtcDateTime;
                    if (utcLastModified > _utcLastModified) {
                        _utcLastModified = utcLastModified;
                    }
                    uniqueId += _entryChangeMonitor.UniqueId;
                    _entryChangeMonitor.NotifyOnChanged(new OnChangedCallback(OnChangedCallback));
                }
                
                _uniqueID = uniqueId;
#if DBG
                _isUniqueIDInitialized = true;
#endif
                // check if file has changed since the start time
                if (utcStart < DateTime.MaxValue) {
                    if (_utcLastModified > utcStart
                        && !(_utcLastModified - DateTime.UtcNow > FUTURE_FILETIME_BUFFER)) {   // See VSWhidbey 400917
                        _bits[CHANGED] = true;
                    }
                }

                _bits[BASE_INIT] = true;
                if (dependency != null && dependency._bits[CHANGED]) {
                    _bits[CHANGED] = true;
                }
                if (_bits[WANTS_DISPOSE] || _bits[CHANGED]) {
                    Debug.Trace("CacheDependencyInit", "WANTS_DISPOSE or CHANGED.  InitForMemoryCache calling DisposeInternal");
                    DisposeInternal();
                }
                dispose = false;
            }
            finally {
                if (dispose) {
                    _bits[BASE_INIT] = true;
                    Debug.Trace("CacheDependencyInit", "\n\nERROR in CacheDependency.InitForMemoryCache, calling DisposeInternal");
                    DisposeInternal();
                }
            }
        }
#endif

        void Init(bool isPublic, string[] filenamesArg, string[] cachekeysArg, CacheDependency dependency, DateTime utcStart) {
#if USE_MEMORY_CACHE
            if (CacheInternal.UseMemoryCache) {
                InitForMemoryCache(isPublic, filenamesArg, cachekeysArg, dependency, utcStart);
                return;
            }
#endif
            DepFileInfo[]   depFileInfos = s_depFileInfosEmpty;
            CacheEntry[]    depEntries = s_entriesEmpty;
            string []       filenames, cachekeys;
            CacheInternal   cacheInternal;

            _bits = new SafeBitVector32(0);

            // copy array argument contents so they can't be changed beneath us
            if (filenamesArg != null) {
                filenames = (string []) filenamesArg.Clone();
            }
            else {
                filenames = null;
            }

            if (cachekeysArg != null) {
                cachekeys = (string []) cachekeysArg.Clone();
            }
            else {
                cachekeys = null;
            }

            _utcLastModified = DateTime.MinValue;

            try {
                // validate filenames array
                if (filenames == null) {
                    filenames = s_stringsEmpty;
                }
                else {
                    foreach (string f in filenames) {
                        if (f == null) {
                            throw new ArgumentNullException("filenamesArg");
                        }

                        // demand PathDiscovery if public
                        if (isPublic) {
                            InternalSecurityPermissions.PathDiscovery(f).Demand();
                        }
                    }
                }

                if (cachekeys == null) {
                    cachekeys = s_stringsEmpty;
                }
                else {
                    // validate cachekeys array
                    foreach (string k in cachekeys) {
                        if (k == null) {
                            throw new ArgumentNullException("cachekeysArg");
                        }
                    }
                }

                // copy all parts of another dependency if provided
                if (dependency == null) {
                    dependency = s_dependencyEmpty;
                }
                else {
                    if (dependency.GetType() != s_dependencyEmpty.GetType()) {
                        throw new ArgumentException(SR.GetString(SR.Invalid_Dependency_Type));
                    }

                    // Copy the parts of the dependency we need before
                    // we reference them, as the dependency can change
                    // underneath us.
                    object d_depFileInfos = dependency._depFileInfos;
                    object d_entries = dependency._entries;
                    DateTime d_lastModified = dependency._utcLastModified;

                    // if the dependency we're copying has changed, we're done
                    if (dependency._bits[CHANGED]) {
                        _bits[CHANGED] = true;
                        // There is nothing to dispose because we haven't started
                        // monitoring anything yet.  But we call DisposeInternal in
                        // order to set the WANTS_DISPOSE bit.
                        DisposeInternal();
                        return;
                    }

                    // copy depFileInfos
                    if (d_depFileInfos != null) {
                        if (d_depFileInfos is DepFileInfo) {
                            depFileInfos = new DepFileInfo[1] {(DepFileInfo) d_depFileInfos};
                        }
                        else {
                            depFileInfos = (DepFileInfo[]) (d_depFileInfos);

                        }

                        // verify that the object was fully constructed
                        // and that we have permission to discover the file
                        foreach (DepFileInfo depFileInfo in depFileInfos) {
                            string f = depFileInfo._filename;
                            
                            if (f == null) {
                                _bits[CHANGED] = true;
                                // There is nothing to dispose because we haven't started
                                // monitoring anything yet.  But we call DisposeInternal in
                                // order to set the WANTS_DISPOSE bit.
                                DisposeInternal();
                                return;
                            }

                            // demand PathDiscovery if public
                            if (isPublic) {
                                InternalSecurityPermissions.PathDiscovery(f).Demand();
                            }
                        }
                    }

                    // copy cache entries
                    if (d_entries != null) {
                        if (d_entries is CacheEntry) {
                            depEntries = new CacheEntry[1] {(CacheEntry) (d_entries)};
                        }
                        else {
                            depEntries = (CacheEntry[]) (d_entries);
                            // verify that the object was fully constructed
                            foreach (CacheEntry entry in depEntries) {
                                if (entry == null) {
                                    _bits[CHANGED] = true;
                                    // There is nothing to dispose because we haven't started
                                    // monitoring anything yet.  But we call DisposeInternal in
                                    // order to set the WANTS_DISPOSE bit.
                                    DisposeInternal();
                                    return;
                                }
                            }
                        }
                    }

                    _utcLastModified = d_lastModified;
                }

                // Monitor files for changes
                int lenMyDepFileInfos = depFileInfos.Length + filenames.Length;
                if (lenMyDepFileInfos > 0) {
                    DepFileInfo[] myDepFileInfos = new DepFileInfo[lenMyDepFileInfos];
                    FileChangeEventHandler handler = new FileChangeEventHandler(this.FileChange);
                    FileChangesMonitor fmon = HttpRuntime.FileChangesMonitor;

                    int i;
                    for (i = 0; i < lenMyDepFileInfos; i++) {
                        myDepFileInfos[i] = new DepFileInfo();
                    }

                    // monitor files from the existing dependency
                    // note that we don't check for start times in the existing dependency
                    i = 0;
                    foreach (DepFileInfo depFileInfo in depFileInfos) {
                        string  f = depFileInfo._filename;
                        fmon.StartMonitoringPath(f, handler, out myDepFileInfos[i]._fad);
                        myDepFileInfos[i]._filename = f;
                        i++;
                    }

                    // monitor new files
                    DateTime    utcNow = DateTime.MinValue;
                    foreach (string f in filenames) {
                        DateTime utcLastWrite = fmon.StartMonitoringPath(f, handler, out myDepFileInfos[i]._fad);
                        myDepFileInfos[i]._filename = f;
                        i++;

                        if (utcLastWrite > _utcLastModified) {
                            _utcLastModified = utcLastWrite;
                        }

                        // check if file has changed since the start time
                        if (utcStart < DateTime.MaxValue) {
                            if (utcNow == DateTime.MinValue) {
                                utcNow = DateTime.UtcNow;
                            }
                            
                            Debug.Trace("CacheDependencyInit", "file=" + f + "; utcStart=" + utcStart + "; utcLastWrite=" + utcLastWrite);
                            if (utcLastWrite >= utcStart &&
                                !(utcLastWrite - utcNow > FUTURE_FILETIME_BUFFER)) {   // See VSWhidbey 400917
                                Debug.Trace("CacheDependencyInit", "changes occurred since start time for file " + f);
                                _bits[CHANGED] = true;
                                break;
                            }
                        }
                    }

                    if (myDepFileInfos.Length == 1) {
                        _depFileInfos = myDepFileInfos[0];
                    }
                    else {
                        _depFileInfos = myDepFileInfos;
                    }
                }

                // Monitor other cache entries for changes
                int lenMyEntries = depEntries.Length + cachekeys.Length;
                if (lenMyEntries > 0 && !_bits[CHANGED]) {
                    CacheEntry[] myEntries = new CacheEntry[lenMyEntries];

                    // Monitor entries from the existing cache dependency
                    int i = 0;
                    foreach (CacheEntry entry in depEntries) {
                        entry.AddCacheDependencyNotify(this);
                        myEntries[i++] = entry;
                    }

                    // Monitor new entries specified for this depenedency
                    // Entries must be added to cache, and created before the startTime
                    cacheInternal = HttpRuntime.CacheInternal;
                    foreach (string k in cachekeys) {
                        CacheEntry entry = (CacheEntry) cacheInternal.DoGet(isPublic, k, CacheGetOptions.ReturnCacheEntry);
                        if (entry != null) {
                            entry.AddCacheDependencyNotify(this);
                            myEntries[i++] = entry;

                            if (entry.UtcCreated > _utcLastModified) {
                                _utcLastModified = entry.UtcCreated;
                            }

                            if (    entry.State != CacheEntry.EntryState.AddedToCache || 
                                    entry.UtcCreated > utcStart) {

#if DBG
                                if (entry.State != CacheEntry.EntryState.AddedToCache) {
                                    Debug.Trace("CacheDependencyInit", "Entry is not in cache, considered changed:" + k);
                                }
                                else {
                                    Debug.Trace("CacheDependencyInit", "Changes occurred to entry since start time:" + k);
                                }
#endif

                                _bits[CHANGED] = true;
                                break;
                            }
                        }
                        else {
                            Debug.Trace("CacheDependencyInit", "Cache item not found to create dependency on:" + k);
                            _bits[CHANGED] = true;
                            break;
                        }
                    }

                    if (myEntries.Length == 1) {
                        _entries = myEntries[0];
                    }
                    else {
                        _entries = myEntries;
                    }
                }

                _bits[BASE_INIT] = true;
                if (dependency._bits[CHANGED]) {
                    _bits[CHANGED] = true;
                }
 
                if (_bits[WANTS_DISPOSE] || _bits[CHANGED]) {
                    DisposeInternal();
                }

                Debug.Assert(_objNotify == null, "_objNotify == null");
            }
            catch {
                // derived constructor will not execute due to the throw,
                // so we just force a dispose on ourselves
                _bits[BASE_INIT] = true;
                DisposeInternal();
                throw;
            }
            finally {
                InitUniqueID();
            }
        }


        public void Dispose() {
            // Set this bit just in case our derived ctor forgot to call FinishInit()
            _bits[DERIVED_INIT] = true;
                
            if (Use()) {
                // Do the dispose only if the cache has not already used us
                DisposeInternal();
            }
        }

        protected internal void FinishInit() {
            _bits[DERIVED_INIT] = true;

            if (_bits[WANTS_DISPOSE]) {
                DisposeInternal();
            }
        }

        /*
         * Shutdown all dependency monitoring and firing of NotifyDependencyChanged notification.
         */
        internal void DisposeInternal() {
            _bits[WANTS_DISPOSE] = true;

            if (_bits[DERIVED_INIT]) {
                if (_bits.ChangeValue(DERIVED_DISPOSED, true)) {
                    // Dispose derived classes
                    DependencyDispose();
                }
            }

            if (_bits[BASE_INIT]) {
                if (_bits.ChangeValue(BASE_DISPOSED, true)) {
                    // Dispose ourself
                    DisposeOurself();
                }
            }
        }

        // Allow derived class to dispose itself

        protected virtual void DependencyDispose() {
            // We do our own dispose work in DisposeOurself, so that
            // we don't rely on derived classes calling their base
            // DependencyDispose for us to function correctly.
        }

        void DisposeOurself() {
            // guarantee that we execute only once if an exception
            // is thrown from this function by nulling fields before
            // we access them
            object l_depFileInfos = _depFileInfos;
            object l_entries = _entries;

            _objNotify = null;
            _depFileInfos = null;
            _entries = null;

            // stop monitoring files
            if (l_depFileInfos != null) {
                FileChangesMonitor fmon = HttpRuntime.FileChangesMonitor;

                DepFileInfo oneDepFileInfo = l_depFileInfos as DepFileInfo;
                if (oneDepFileInfo != null) {
                    fmon.StopMonitoringPath(oneDepFileInfo._filename, this);
                }
                else {
                    DepFileInfo[] depFileInfos = (DepFileInfo[]) l_depFileInfos;
                    foreach (DepFileInfo depFileInfo in depFileInfos) {
                        // ensure that we handle partially contructed
                        // objects by checking filename for null
                        string  filename = depFileInfo._filename;
                        if (filename != null) {
                            fmon.StopMonitoringPath(filename, this);
                        }
                    }
                }
            }

            // stop monitoring cache items
            if (l_entries != null) {
                CacheEntry oneEntry = l_entries as CacheEntry;
                if (oneEntry != null) {
                    oneEntry.RemoveCacheDependencyNotify(this);
                }
                else {
                    CacheEntry[] entries = (CacheEntry[]) l_entries;
                    foreach (CacheEntry entry in entries) {
                        // ensure that we handle partially contructed
                        // objects by checking entry for null
                        if (entry != null) {
                            entry.RemoveCacheDependencyNotify(this);
                        }
                    }
                }
            }

#if USE_MEMORY_CACHE
            if (_fileChangeMonitor != null) {
                _fileChangeMonitor.Dispose();
            }
            if (_entryChangeMonitor != null) {
                _entryChangeMonitor.Dispose();
            }
#endif
        }

        // allow the first user to declare ownership
        internal bool Use() {
            return _bits.ChangeValue(USED, true);
        }

        //
        // Has a dependency changed?
        //

        public bool HasChanged {
            get {return _bits[CHANGED];}
        }


        public DateTime UtcLastModified {
            get {
                return _utcLastModified;
            }
        }


        protected void SetUtcLastModified(DateTime utcLastModified) {
            _utcLastModified = utcLastModified;
        }

        //
        // Add/remove an NotifyDependencyChanged notification.
        //
        internal void SetCacheDependencyChanged(ICacheDependencyChanged objNotify) {
            Debug.Assert(_objNotify == null, "_objNotify == null");

            // Set this bit just in case our derived ctor forgot to call FinishInit()
            _bits[DERIVED_INIT] = true;
                
            if (!_bits[BASE_DISPOSED]) {
                _objNotify = objNotify;
            }
        }

        internal void AppendFileUniqueId(DepFileInfo depFileInfo, StringBuilder sb) {
            FileAttributesData fad = depFileInfo._fad;
                
            if (fad == null) {
                fad = FileAttributesData.NonExistantAttributesData;
            }

            sb.Append(depFileInfo._filename);
            sb.Append(fad.UtcLastWriteTime.Ticks.ToString("d", NumberFormatInfo.InvariantInfo));
            sb.Append(fad.FileSize.ToString(CultureInfo.InvariantCulture));
        }

        void InitUniqueID() {
            StringBuilder   sb = null;
            object          l_depFileInfos, l_entries;
                
#if !FEATURE_PAL // no File Change Monitoring
            // get unique id from files
            l_depFileInfos = _depFileInfos;
            if (l_depFileInfos != null) {
                DepFileInfo oneDepFileInfo = l_depFileInfos as DepFileInfo;
                if (oneDepFileInfo != null) {
                     sb = new StringBuilder();
                    AppendFileUniqueId(oneDepFileInfo, sb);
                }
                else {
                    DepFileInfo[] depFileInfos = (DepFileInfo[]) l_depFileInfos;
                    foreach (DepFileInfo depFileInfo in depFileInfos) {
                        // ensure that we handle partially contructed
                        // objects by checking filename for null
                        if (depFileInfo._filename != null) {
                            if (sb == null)
                                sb = new StringBuilder();
                            AppendFileUniqueId(depFileInfo, sb);
                        }
                    }
                }
            }
            
#endif // !FEATURE_PAL
            // get unique id from cache entries
            l_entries = _entries;
            if (l_entries != null) {
                CacheEntry oneEntry = l_entries as CacheEntry;
                if (oneEntry != null) {
                    if (sb == null)
                        sb = new StringBuilder();
                    sb.Append(oneEntry.Key);
                    sb.Append(oneEntry.UtcCreated.Ticks.ToString(CultureInfo.InvariantCulture));
                }
                else {
                    CacheEntry[] entries = (CacheEntry[]) l_entries;
                    foreach (CacheEntry entry in entries) {
                        // ensure that we handle partially contructed
                        // objects by checking entry for null
                        if (entry != null) {
                            if (sb == null)
                                sb = new StringBuilder();
                            sb.Append(entry.Key);
                            sb.Append(entry.UtcCreated.Ticks.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }

            if (sb != null)
                _uniqueID = sb.ToString();

#if DBG
            _isUniqueIDInitialized = true;
#endif
        }

        public virtual string GetUniqueID() {
#if DBG
            Debug.Assert(_isUniqueIDInitialized == true, "_isUniqueIDInitialized == true");
#endif
            return _uniqueID;
        }

        //
        // Return the cacheEntries monitored by this dependency
        // 
        internal CacheEntry[] CacheEntries {
            get {
                if (_entries == null) {
                    return null;
                }

                CacheEntry oneEntry = _entries as CacheEntry;
                if (oneEntry != null) {
                    return new CacheEntry[1] {oneEntry};
                }

                return (CacheEntry[]) _entries;
            }
        }

        //
        // This object has changed, so fire the NotifyDependencyChanged event.
        // We only allow this event to be fired once.
        //

        protected void NotifyDependencyChanged(Object sender, EventArgs e) {
            if (_bits.ChangeValue(CHANGED, true)) {
                _utcLastModified = DateTime.UtcNow;

                ICacheDependencyChanged objNotify = _objNotify;
                if (objNotify != null && !_bits[BASE_DISPOSED]) {
                    Debug.Trace("CacheDependencyNotifyDependencyChanged", "change occurred");
                    objNotify.DependencyChanged(sender, e);
                }

                DisposeInternal();
            }
        }

        //
        // ItemRemoved is called when a cache entry we are monitoring has been removed.
        //
        internal void ItemRemoved() {
            NotifyDependencyChanged(this, EventArgs.Empty);
        }

        //
        // FileChange is called when a file we are monitoring has changed.
        //
        void FileChange(Object sender, FileChangeEvent e) {
            Debug.Trace("CacheDependencyFileChange", "FileChange file=" + e.FileName + ";Action=" + e.Action);
            NotifyDependencyChanged(sender, e);
        }

        //
        //  This will examine the dependency and determine if it's ONLY a file dependency or not 
        //
        internal virtual bool IsFileDependency()
        {
#if USE_MEMORY_CACHE
            if (CacheInternal.UseMemoryCache) {
                if (_entryChangeMonitor != null) {
                    return false;
                }
                
                if (_fileChangeMonitor != null) {
                    return true;
                }
                return false;
            }
#endif

            object depInfos, l_entries;

            // Check and see if we are dependent on any cache entries
            l_entries = _entries;
            if (l_entries != null) {
                CacheEntry oneEntry = l_entries as CacheEntry;
                if (oneEntry != null) {
                    return false;
                }
                else {
                    CacheEntry[] entries = (CacheEntry[]) l_entries;
                    if (entries != null && entries.Length > 0) {
                        return false;
                    }
                }
            }

            depInfos = _depFileInfos;
            if (depInfos != null) {
                DepFileInfo oneDepFileInfo = depInfos as DepFileInfo;
                if (oneDepFileInfo != null) {
                    return true;
                }
                else {
                    DepFileInfo[] depFileInfos = (DepFileInfo[]) depInfos;
                    if (depFileInfos != null && depFileInfos.Length > 0) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method will return only the file dependencies from this dependency
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetFileDependencies()
        {
#if USE_MEMORY_CACHE
            if (CacheInternal.UseMemoryCache) {
                if (_fileChangeMonitor != null) {
                    ReadOnlyCollection<string> paths = _fileChangeMonitor.FilePaths;
                    if (paths != null && paths.Count > 0) {
                        string[] aryPaths = new string[paths.Count];
                        for (int i = 0; i < aryPaths.Length; i++) {
                            aryPaths[i] = paths[i];
                        }
                        return aryPaths;
                    }
                }
                return null;
            }
#endif
            object depInfos = _depFileInfos;

            if (depInfos != null) {
                DepFileInfo oneDepFileInfo = depInfos as DepFileInfo;
                if (oneDepFileInfo != null) {
                    return new string[] {oneDepFileInfo._filename};
                }
                else {
                    DepFileInfo[] depFileInfos = (DepFileInfo[]) depInfos;
                    string[] names = new string[depFileInfos.Length];
                    for (int i = 0; i < depFileInfos.Length; i++) {
                        names[i] = depFileInfos[i]._filename;
                    }

                    return names;
                }
            }

            return null;
        }
    }

    public sealed class AggregateCacheDependency : CacheDependency, ICacheDependencyChanged {
        ArrayList   _dependencies;
        bool        _disposed;


        public AggregateCacheDependency() {
            // The ctor of every class derived from CacheDependency must call this.
            FinishInit();
        }


        public void Add(params CacheDependency [] dependencies) {
            DateTime utcLastModified = DateTime.MinValue;

            if (dependencies == null) {
                throw new ArgumentNullException("dependencies");
            }

            // copy array argument contents so they can't be changed beneath us
            dependencies = (CacheDependency []) dependencies.Clone();

            // validate contents
            foreach (CacheDependency d in dependencies) {
                if (d == null) {
                    throw new ArgumentNullException("dependencies");
                }

                if (!d.Use()) {
                    throw new InvalidOperationException(SR.GetString(SR.Cache_dependency_used_more_that_once));
                }
            }

            // add dependencies, and check if any have changed
            bool hasChanged = false;
            lock (this) {
                if (!_disposed) {
                    if (_dependencies == null) {
                        _dependencies = new ArrayList();
                    }

                    _dependencies.AddRange(dependencies);

                    foreach (CacheDependency d in dependencies) {
                        d.SetCacheDependencyChanged(this);

                        if (d.UtcLastModified > utcLastModified) {
                            utcLastModified = d.UtcLastModified;
                        }

                        if (d.HasChanged) {
                            hasChanged = true;
                            break;
                        }
                    }
                }
            }

            SetUtcLastModified(utcLastModified);

            // if a dependency has changed, notify others that we have changed.
            if (hasChanged) {
                NotifyDependencyChanged(this, EventArgs.Empty);
            }
        }

        // Dispose our dependencies. Note that the call to this
        // function is thread safe.

        protected override void DependencyDispose() {
            CacheDependency[] dependencies = null;

            lock (this) {
                _disposed = true;
                if (_dependencies != null) {
                    dependencies = (CacheDependency[]) _dependencies.ToArray(typeof(CacheDependency));
                    _dependencies = null;
                }
            }

            if (dependencies != null) {
                foreach (CacheDependency d in dependencies) {
                    d.DisposeInternal();
                }
            }
        }

        // Forward call from the aggregate to the CacheEntry

        /// <internalonly/>
        void ICacheDependencyChanged.DependencyChanged(Object sender, EventArgs e) {
            NotifyDependencyChanged(sender, e);
        }


        public override string GetUniqueID() {
            StringBuilder sb = null;
            CacheDependency[] dependencies = null;

            //VSWhidbey 354570: return null if this AggregateCacheDependency cannot otherwise return a unique ID
            if (_dependencies == null) {
                return null;
            }

            lock (this) {
                if (_dependencies != null) {
                    dependencies = (CacheDependency[]) _dependencies.ToArray(typeof(CacheDependency));
                }
            }
            
            if (dependencies != null) {
                foreach (CacheDependency dependency in dependencies) {
                    string id = dependency.GetUniqueID();

                    if (id == null) {
                        // When AggregateCacheDependency contains a dependency for which GetUniqueID() returns null, 
                        // it should return null itself.  This is because it can no longer generate a UniqueID that 
                        // is guaranteed to be different when any of the dependencies change.
                        return null;
                    }

                    if (sb == null) {
                        sb = new StringBuilder();
                    }
                    sb.Append(id);
                }
            }

            return sb != null ? sb.ToString() : null;
        }

        internal CacheDependency[] GetDependencyArray()
        {
            CacheDependency[] dependencies = null;

            lock (this) {
                if (_dependencies != null) {
                    dependencies = (CacheDependency[]) _dependencies.ToArray(typeof(CacheDependency));
                }
            }

            return dependencies;
        }

        //
        //  This will examine the dependencies and only return true if ALL dependencies are file dependencies
        //
        internal override bool IsFileDependency()
        {
            CacheDependency[] dependencies = null;

            dependencies = GetDependencyArray();
            if (dependencies == null) {
                return false;
            }

            foreach (CacheDependency d in dependencies) {
                // We should only check if the type is either CacheDependency or the Aggregate.
                // Anything else, we can't guarantee that it's a file only dependency.
                if ( ! object.ReferenceEquals(d.GetType(), typeof(CacheDependency)) &&
                     ! object.ReferenceEquals(d.GetType(), typeof(AggregateCacheDependency)) ) {
                     return false;
                }

                if (! d.IsFileDependency()) {
                    return false;
                }
            }

            return true;
        }
 
        /// <summary>
        /// This method will return only the file dependencies from this dependency
        /// </summary>
        /// <returns></returns>
        public override string[] GetFileDependencies()
        {
            ArrayList fileNames = null;
            CacheDependency[] dependencies = null;

            dependencies = GetDependencyArray();
            if (dependencies == null) {
                return null;
            }

            foreach (CacheDependency d in dependencies) {
                // Check if the type is either CacheDependency or an Aggregate;
                // for anything else, we can't guarantee it's a file only dependency.
                if (object.ReferenceEquals(d.GetType(), typeof(CacheDependency))
                    || object.ReferenceEquals(d.GetType(), typeof(AggregateCacheDependency))) {

                    string[] tmpFileNames = d.GetFileDependencies();

                    if (tmpFileNames != null) {

                        if (fileNames == null) {
                            fileNames = new ArrayList();
                        }

                        fileNames.AddRange(tmpFileNames);
                    }
                }
            }

            if (fileNames != null) {
                return (string[])fileNames.ToArray(typeof(string));
            }
            else {
                return null;
            }
        }
    }
}

