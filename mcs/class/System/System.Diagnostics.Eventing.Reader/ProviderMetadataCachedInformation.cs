namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Security;

    internal class ProviderMetadataCachedInformation
    {
        private Dictionary<ProviderMetadataId, CacheItem> cache;
        private string logfile;
        private int maximumCacheSize;
        private EventLogSession session;

        public ProviderMetadataCachedInformation(EventLogSession session, string logfile, int maximumCacheSize)
        {
            this.session = session;
            this.logfile = logfile;
            this.cache = new Dictionary<ProviderMetadataId, CacheItem>();
            this.maximumCacheSize = maximumCacheSize;
        }

        private void AddCacheEntry(ProviderMetadataId key, ProviderMetadata pm)
        {
            if (this.IsCacheFull())
            {
                this.FlushOldestEntry();
            }
            CacheItem item = new CacheItem(pm);
            this.cache.Add(key, item);
        }

        private void DeleteCacheEntry(ProviderMetadataId key)
        {
            if (this.IsProviderinCache(key))
            {
                CacheItem item = this.cache[key];
                this.cache.Remove(key);
                item.ProviderMetadata.Dispose();
            }
        }

        private void FlushOldestEntry()
        {
            double totalMilliseconds = -10.0;
            DateTime now = DateTime.Now;
            ProviderMetadataId key = null;
            foreach (KeyValuePair<ProviderMetadataId, CacheItem> pair in this.cache)
            {
                TimeSpan span = now.Subtract(pair.Value.TheTime);
                if (span.TotalMilliseconds >= totalMilliseconds)
                {
                    totalMilliseconds = span.TotalMilliseconds;
                    key = pair.Key;
                }
            }
            if (key != null)
            {
                this.DeleteCacheEntry(key);
            }
        }

        [SecuritySafeCritical]
        public string GetFormatDescription(string ProviderName, EventLogHandle eventHandle)
        {
            string str;
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                try
                {
                    str = NativeWrapper.EvtFormatMessageRenderName(this.GetProviderMetadata(key).Handle, eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageEvent);
                }
                catch (EventLogNotFoundException)
                {
                    str = null;
                }
            }
            return str;
        }

        public string GetFormatDescription(string ProviderName, EventLogHandle eventHandle, string[] values)
        {
            string str;
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                ProviderMetadata providerMetadata = this.GetProviderMetadata(key);
                try
                {
                    str = NativeWrapper.EvtFormatMessageFormatDescription(providerMetadata.Handle, eventHandle, values);
                }
                catch (EventLogNotFoundException)
                {
                    str = null;
                }
            }
            return str;
        }

        [SecuritySafeCritical]
        public IEnumerable<string> GetKeywordDisplayNames(string ProviderName, EventLogHandle eventHandle)
        {
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                return NativeWrapper.EvtFormatMessageRenderKeywords(this.GetProviderMetadata(key).Handle, eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageKeyword);
            }
        }

        [SecuritySafeCritical]
        public string GetLevelDisplayName(string ProviderName, EventLogHandle eventHandle)
        {
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                return NativeWrapper.EvtFormatMessageRenderName(this.GetProviderMetadata(key).Handle, eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageLevel);
            }
        }

        [SecuritySafeCritical]
        public string GetOpcodeDisplayName(string ProviderName, EventLogHandle eventHandle)
        {
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                return NativeWrapper.EvtFormatMessageRenderName(this.GetProviderMetadata(key).Handle, eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageOpcode);
            }
        }

        private ProviderMetadata GetProviderMetadata(ProviderMetadataId key)
        {
            if (!this.IsProviderinCache(key))
            {
                ProviderMetadata metadata;
                try
                {
                    metadata = new ProviderMetadata(key.ProviderName, this.session, key.TheCultureInfo, this.logfile);
                }
                catch (EventLogNotFoundException)
                {
                    metadata = new ProviderMetadata(key.ProviderName, this.session, key.TheCultureInfo);
                }
                this.AddCacheEntry(key, metadata);
                return metadata;
            }
            CacheItem cacheItem = this.cache[key];
            ProviderMetadata providerMetadata = cacheItem.ProviderMetadata;
            try
            {
                providerMetadata.CheckReleased();
                UpdateCacheValueInfoForHit(cacheItem);
            }
            catch (EventLogException)
            {
                this.DeleteCacheEntry(key);
                try
                {
                    providerMetadata = new ProviderMetadata(key.ProviderName, this.session, key.TheCultureInfo, this.logfile);
                }
                catch (EventLogNotFoundException)
                {
                    providerMetadata = new ProviderMetadata(key.ProviderName, this.session, key.TheCultureInfo);
                }
                this.AddCacheEntry(key, providerMetadata);
            }
            return providerMetadata;
        }

        [SecuritySafeCritical]
        public string GetTaskDisplayName(string ProviderName, EventLogHandle eventHandle)
        {
            lock (this)
            {
                ProviderMetadataId key = new ProviderMetadataId(ProviderName, CultureInfo.CurrentCulture);
                return NativeWrapper.EvtFormatMessageRenderName(this.GetProviderMetadata(key).Handle, eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageTask);
            }
        }

        private bool IsCacheFull()
        {
            return (this.cache.Count == this.maximumCacheSize);
        }

        private bool IsProviderinCache(ProviderMetadataId key)
        {
            return this.cache.ContainsKey(key);
        }

        private static void UpdateCacheValueInfoForHit(CacheItem cacheItem)
        {
            cacheItem.TheTime = DateTime.Now;
        }

        private class CacheItem
        {
            private System.Diagnostics.Eventing.Reader.ProviderMetadata pm;
            private DateTime theTime;

            public CacheItem(System.Diagnostics.Eventing.Reader.ProviderMetadata pm)
            {
                this.pm = pm;
                this.theTime = DateTime.Now;
            }

            public System.Diagnostics.Eventing.Reader.ProviderMetadata ProviderMetadata
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.pm;
                }
            }

            public DateTime TheTime
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.theTime;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.theTime = value;
                }
            }
        }

        private class ProviderMetadataId
        {
            private CultureInfo cultureInfo;
            private string providerName;

            public ProviderMetadataId(string providerName, CultureInfo cultureInfo)
            {
                this.providerName = providerName;
                this.cultureInfo = cultureInfo;
            }

            public override bool Equals(object obj)
            {
                ProviderMetadataCachedInformation.ProviderMetadataId id = obj as ProviderMetadataCachedInformation.ProviderMetadataId;
                if (id == null)
                {
                    return false;
                }
                return (this.providerName.Equals(id.providerName) && (this.cultureInfo == id.cultureInfo));
            }

            public override int GetHashCode()
            {
                return (this.providerName.GetHashCode() ^ this.cultureInfo.GetHashCode());
            }

            public string ProviderName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.providerName;
                }
            }

            public CultureInfo TheCultureInfo
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.cultureInfo;
                }
            }
        }
    }
}

