// <copyright file="ObjectCacheHost.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using System.Runtime.Caching.Hosting;
using System.Web.Util;

namespace System.Web.Hosting {
    [SuppressMessage("Microsoft.Usage", "CA2302:FlagServiceProviders", Justification = "Internal class")]
    internal sealed class ObjectCacheHost : IServiceProvider, IApplicationIdentifier, IFileChangeNotificationSystem, IMemoryCacheManager
    {

        private Object _lock = new Object();
        private Dictionary<MemoryCache, MemoryCacheInfo> _cacheInfos;

        internal sealed class FileChangeEventTarget {
            private OnChangedCallback _onChangedCallback;
            private FileChangeEventHandler _handler;

            private void OnChanged(Object sender, FileChangeEvent e) {
                _onChangedCallback(null);
            }
            
            internal FileChangeEventHandler Handler { get { return _handler; } }

            internal FileChangeEventTarget(OnChangedCallback onChangedCallback) {
                _onChangedCallback = onChangedCallback;
                _handler = new FileChangeEventHandler(this.OnChanged);
            }
        }
        
        internal sealed class MemoryCacheInfo {
            internal MemoryCache Cache;
            internal long Size;
        }

        Object IServiceProvider.GetService(Type service) {
            if (service == typeof(IFileChangeNotificationSystem)) {
                return this as IFileChangeNotificationSystem;
            }
            else if (service == typeof(IMemoryCacheManager)) {
                return this as IMemoryCacheManager;
            }
            else if (service == typeof(IApplicationIdentifier)) {
                return this as IApplicationIdentifier;
            }
            else {
                return null;
            }
        }

        String IApplicationIdentifier.GetApplicationId() {
            return HttpRuntime.AppDomainAppId;
        }

        void IFileChangeNotificationSystem.StartMonitoring(string filePath, OnChangedCallback onChangedCallback, out Object state, out DateTimeOffset lastWrite, out long fileSize) {
            if (filePath == null) {
                throw new ArgumentNullException("filePath");
            }
            if (onChangedCallback == null) {
                throw new ArgumentNullException("onChangedCallback");
            }
            FileChangeEventTarget target = new FileChangeEventTarget(onChangedCallback);
            FileAttributesData fad;
            HttpRuntime.FileChangesMonitor.StartMonitoringPath(filePath, target.Handler, out fad);
            if (fad == null) {
                fad = FileAttributesData.NonExistantAttributesData;
            }
            state = target;
#if DBG            
            Debug.Assert(fad.UtcLastWriteTime.Kind == DateTimeKind.Utc, "fad.UtcLastWriteTime.Kind == DateTimeKind.Utc");
#endif
            lastWrite = fad.UtcLastWriteTime;
            fileSize = fad.FileSize;
        }

        void IFileChangeNotificationSystem.StopMonitoring(string filePath, Object state) {
            if (filePath == null) {
                throw new ArgumentNullException("filePath");
            }
            if (state == null) {
                throw new ArgumentNullException("state");
            }
            HttpRuntime.FileChangesMonitor.StopMonitoringPath(filePath, state);
        }

        void IMemoryCacheManager.ReleaseCache(MemoryCache memoryCache) {
            if (memoryCache == null) {
                throw new ArgumentNullException("memoryCache");
            }
            lock (_lock) {
                if (_cacheInfos != null) {
                    MemoryCacheInfo info = null;
                    if (_cacheInfos.TryGetValue(memoryCache, out info)) {
                        _cacheInfos.Remove(memoryCache);
                    }
                }
            }
        }

        void IMemoryCacheManager.UpdateCacheSize(long size, MemoryCache memoryCache) {
            if (memoryCache == null) {
                throw new ArgumentNullException("memoryCache");
            }
            lock (_lock) {
                if (_cacheInfos == null) {
                    _cacheInfos = new Dictionary<MemoryCache, MemoryCacheInfo>();
                }
                MemoryCacheInfo info = null;
                if (!_cacheInfos.TryGetValue(memoryCache, out info)) {
                    info = new MemoryCacheInfo();
                    info.Cache = memoryCache;
                    _cacheInfos[memoryCache] = info;
                }
                info.Size = size;
            }
        }

        internal long TrimCache(int percent) {
            long trimmedOrExpired = 0;
            MemoryCache[] caches = null;
            lock (_lock) {
                if (_cacheInfos != null && _cacheInfos.Count > 0) {
                    caches = new MemoryCache[_cacheInfos.Keys.Count];
                    _cacheInfos.Keys.CopyTo(caches, 0);
                }
            }
            if (caches != null) {
                foreach (MemoryCache cache in caches) {
                    trimmedOrExpired += cache.Trim(percent);
                }
            }
            return trimmedOrExpired;
        }
    }
}
