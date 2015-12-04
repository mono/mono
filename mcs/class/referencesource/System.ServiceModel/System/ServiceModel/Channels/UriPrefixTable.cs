//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.ServiceModel;

    sealed class UriPrefixTable<TItem>
        where TItem : class
    {
        int count;
        const int HopperSize = 128;
        volatile HopperCache lookupCache; // cache matches, for lookup speed

        SegmentHierarchyNode<TItem> root;
        bool useWeakReferences;
        bool includePortInComparison;

        public UriPrefixTable()
            : this(false)
        {
        }

        public UriPrefixTable(bool includePortInComparison)
            : this(includePortInComparison, false)
        {
        }

        public UriPrefixTable(bool includePortInComparison, bool useWeakReferences)
        {
            this.includePortInComparison = includePortInComparison;
            this.useWeakReferences = useWeakReferences;
            this.root = new SegmentHierarchyNode<TItem>(null, useWeakReferences);
            this.lookupCache = new HopperCache(HopperSize, useWeakReferences);
        }

        internal UriPrefixTable(UriPrefixTable<TItem> objectToClone)
            : this(objectToClone.includePortInComparison, objectToClone.useWeakReferences)
        {
            if (objectToClone.Count > 0)
            {
                foreach (KeyValuePair<BaseUriWithWildcard, TItem> current in objectToClone.GetAll())
                {
                    this.RegisterUri(current.Key.BaseAddress, current.Key.HostNameComparisonMode, current.Value);
                }
            }
        }

        object ThisLock
        {
            get
            {
                // The UriPrefixTable instance itself is used as a 
                // synchronization primitive in the TransportManagers and the 
                // TransportManagerContainers so we return 'this' to keep them in [....].                 
                return this;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public bool IsRegistered(BaseUriWithWildcard key)
        {
            Uri uri = key.BaseAddress;

            // don't need to normalize path since SegmentHierarchyNode is 
            // already OrdinalIgnoreCase
            string[] paths = UriSegmenter.ToPath(uri, key.HostNameComparisonMode, this.includePortInComparison);
            bool exactMatch;
            SegmentHierarchyNode<TItem> node;
            lock (ThisLock)
            {
                node = FindDataNode(paths, out exactMatch);
            }
            return exactMatch && node != null && node.Data != null;
        }

        public IEnumerable<KeyValuePair<BaseUriWithWildcard, TItem>> GetAll()
        {
            lock (ThisLock)
            {
                List<KeyValuePair<BaseUriWithWildcard, TItem>> result = new List<KeyValuePair<BaseUriWithWildcard, TItem>>();
                this.root.Collect(result);
                return result;
            }
        }

        bool TryCacheLookup(BaseUriWithWildcard key, out TItem item)
        {
            object value = this.lookupCache.GetValue(ThisLock, key);

            // We might return null and true in the case of DBNull (cached negative result).
            // When TItem is object, the cast isn't sufficient to ---- out DBNulls, so we need an explicit check.
            item = value == DBNull.Value ? null : (TItem)value;
            return value != null;
        }

        void AddToCache(BaseUriWithWildcard key, TItem item)
        {
            // Don't allow explicitly adding DBNulls.
            Fx.Assert(item != DBNull.Value, "Can't add DBNull to UriPrefixTable.");

            // HopperCache uses null as 'doesn't exist', so use DBNull as a stand-in for null.
            this.lookupCache.Add(key, item ?? (object)DBNull.Value);
        }

        void ClearCache()
        {
            this.lookupCache = new HopperCache(HopperSize, this.useWeakReferences);
        }

        public bool TryLookupUri(Uri uri, HostNameComparisonMode hostNameComparisonMode, out TItem item)
        {
            BaseUriWithWildcard key = new BaseUriWithWildcard(uri, hostNameComparisonMode);
            if (TryCacheLookup(key, out item))
            {
                return item != null;
            }

            lock (ThisLock)
            {
                // exact match failed, perform the full lookup (which will also
                // catch case-insensitive variations that aren't yet in our cache)
                bool dummy;
                SegmentHierarchyNode<TItem> node = FindDataNode(
                    UriSegmenter.ToPath(key.BaseAddress, hostNameComparisonMode, this.includePortInComparison), out dummy);
                if (node != null)
                {
                    item = node.Data;
                }
                // We want to cache both positive AND negative results
                AddToCache(key, item);
                return (item != null);
            }
        }

        public void RegisterUri(Uri uri, HostNameComparisonMode hostNameComparisonMode, TItem item)
        {
            Fx.Assert(HostNameComparisonModeHelper.IsDefined(hostNameComparisonMode), "RegisterUri: Invalid HostNameComparisonMode value passed in.");

            lock (ThisLock)
            {
                // Since every newly registered Uri could alter what Prefixes should have matched, we
                // should clear the cache of any existing results and start over
                ClearCache();
                BaseUriWithWildcard key = new BaseUriWithWildcard(uri, hostNameComparisonMode);
                SegmentHierarchyNode<TItem> node = FindOrCreateNode(key);
                if (node.Data != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                        SR.DuplicateRegistration, uri)));
                }
                node.SetData(item, key);
                count++;
            }
        }

        public void UnregisterUri(Uri uri, HostNameComparisonMode hostNameComparisonMode)
        {
            lock (ThisLock)
            {
                // Since every removed Uri could alter what Prefixes should have matched, we
                // should clear the cache of any existing results and start over
                ClearCache();
                string[] path = UriSegmenter.ToPath(uri, hostNameComparisonMode, this.includePortInComparison);
                // Never remove the root
                if (path.Length == 0)
                {
                    this.root.RemoveData();
                }
                else
                {
                    this.root.RemovePath(path, 0);
                }
                count--;
            }
        }

        SegmentHierarchyNode<TItem> FindDataNode(string[] path, out bool exactMatch)
        {
            Fx.Assert(path != null, "FindDataNode: path is null");

            exactMatch = false;
            SegmentHierarchyNode<TItem> current = this.root;
            SegmentHierarchyNode<TItem> result = null;
            for (int i = 0; i < path.Length; ++i)
            {
                SegmentHierarchyNode<TItem> next;
                if (!current.TryGetChild(path[i], out next))
                {
                    break;
                }
                else if (next.Data != null)
                {
                    result = next;
                    exactMatch = (i == path.Length - 1);
                }
                current = next;
            }
            return result;
        }

        SegmentHierarchyNode<TItem> FindOrCreateNode(BaseUriWithWildcard baseUri)
        {
            Fx.Assert(baseUri != null, "FindOrCreateNode: baseUri is null");

            string[] path = UriSegmenter.ToPath(baseUri.BaseAddress, baseUri.HostNameComparisonMode, this.includePortInComparison);
            SegmentHierarchyNode<TItem> current = this.root;
            for (int i = 0; i < path.Length; ++i)
            {
                SegmentHierarchyNode<TItem> next;
                if (!current.TryGetChild(path[i], out next))
                {
                    next = new SegmentHierarchyNode<TItem>(path[i], useWeakReferences);
                    current.SetChildNode(path[i], next);
                }
                current = next;
            }
            return current;
        }

        static class UriSegmenter
        {
            internal static string[] ToPath(Uri uriPath, HostNameComparisonMode hostNameComparisonMode,
                bool includePortInComparison)
            {
                if (null == uriPath)
                {
                    return new string[0];
                }
                UriSegmentEnum segmentEnum = new UriSegmentEnum(uriPath); // struct
                return segmentEnum.GetSegments(hostNameComparisonMode, includePortInComparison);
            }

            struct UriSegmentEnum
            {
                string segment;
                int segmentStartAt;
                int segmentLength;
                UriSegmentType type;
                Uri uri;

                internal UriSegmentEnum(Uri uri)
                {
                    Fx.Assert(null != uri, "UreSegmentEnum: null uri");
                    this.uri = uri;
                    this.type = UriSegmentType.Unknown;
                    this.segment = null;
                    this.segmentStartAt = 0;
                    this.segmentLength = 0;
                }

                void ClearSegment()
                {
                    this.type = UriSegmentType.None;
                    this.segment = string.Empty;
                    this.segmentStartAt = 0;
                    this.segmentLength = 0;
                }

                public string[] GetSegments(HostNameComparisonMode hostNameComparisonMode,
                    bool includePortInComparison)
                {
                    List<string> segments = new List<string>();
                    while (this.Next())
                    {
                        switch (this.type)
                        {
                            case UriSegmentType.Path:
                                segments.Add(this.segment.Substring(this.segmentStartAt, this.segmentLength));
                                break;

                            case UriSegmentType.Host:
                                if (hostNameComparisonMode == HostNameComparisonMode.StrongWildcard)
                                {
                                    segments.Add("+");
                                }
                                else if (hostNameComparisonMode == HostNameComparisonMode.Exact)
                                {
                                    segments.Add(this.segment);
                                }
                                else
                                {
                                    segments.Add("*");
                                }
                                break;

                            case UriSegmentType.Port:
                                if (includePortInComparison || hostNameComparisonMode == HostNameComparisonMode.Exact)
                                {
                                    segments.Add(this.segment);
                                }
                                break;

                            default:
                                segments.Add(this.segment);
                                break;
                        }
                    }
                    return segments.ToArray();
                }

                public bool Next()
                {
                    while (true)
                    {
                        switch (this.type)
                        {
                            case UriSegmentType.Unknown:
                                this.type = UriSegmentType.Scheme;
                                this.SetSegment(this.uri.Scheme);
                                return true;

                            case UriSegmentType.Scheme:
                                this.type = UriSegmentType.Host;
                                string host = this.uri.Host;
                                // The userName+password also accompany...
                                string userInfo = this.uri.UserInfo;
                                if (null != userInfo && userInfo.Length > 0)
                                {
                                    host = userInfo + '@' + host;
                                }
                                this.SetSegment(host);
                                return true;

                            case UriSegmentType.Host:
                                this.type = UriSegmentType.Port;
                                int port = this.uri.Port;
                                this.SetSegment(port.ToString(CultureInfo.InvariantCulture));
                                return true;

                            case UriSegmentType.Port:
                                this.type = UriSegmentType.Path;
                                string absPath = this.uri.AbsolutePath;
                                Fx.Assert(null != absPath, "Next: nill absPath");
                                if (0 == absPath.Length)
                                {
                                    this.ClearSegment();
                                    return false;
                                }
                                this.segment = absPath;
                                this.segmentStartAt = 0;
                                this.segmentLength = 0;
                                return this.NextPathSegment();

                            case UriSegmentType.Path:
                                return this.NextPathSegment();

                            case UriSegmentType.None:
                                return false;

                            default:
                                Fx.Assert("Next: unknown enum value");
                                return false;
                        }
                    }
                }

                public bool NextPathSegment()
                {
                    this.segmentStartAt += this.segmentLength;
                    while (this.segmentStartAt < this.segment.Length && this.segment[this.segmentStartAt] == '/')
                    {
                        this.segmentStartAt++;
                    }

                    if (this.segmentStartAt < this.segment.Length)
                    {
                        int next = this.segment.IndexOf('/', this.segmentStartAt);
                        if (-1 == next)
                        {
                            this.segmentLength = this.segment.Length - this.segmentStartAt;
                        }
                        else
                        {
                            this.segmentLength = next - this.segmentStartAt;
                        }
                        return true;
                    }
                    this.ClearSegment();
                    return false;
                }

                void SetSegment(string segment)
                {
                    this.segment = segment;
                    this.segmentStartAt = 0;
                    this.segmentLength = segment.Length;
                }

                enum UriSegmentType
                {
                    Unknown,
                    Scheme,
                    Host,
                    Port,
                    Path,
                    None
                }
            }
        }
    }

    class SegmentHierarchyNode<TData>
        where TData : class
    {
        BaseUriWithWildcard path;
        TData data;
        string name;
        Dictionary<string, SegmentHierarchyNode<TData>> children;
        WeakReference weakData;
        bool useWeakReferences;

        public SegmentHierarchyNode(string name, bool useWeakReferences)
        {
            this.name = name;
            this.useWeakReferences = useWeakReferences;
            this.children = new Dictionary<string, SegmentHierarchyNode<TData>>(StringComparer.OrdinalIgnoreCase);
        }

        public TData Data
        {
            get
            {
                if (useWeakReferences)
                {
                    if (this.weakData == null)
                    {
                        return null;
                    }
                    else
                    {
                        return this.weakData.Target as TData;
                    }
                }
                else
                {
                    return this.data;
                }
            }
        }

        public void SetData(TData data, BaseUriWithWildcard path)
        {
            this.path = path;
            if (useWeakReferences)
            {
                if (data == null)
                {
                    this.weakData = null;
                }
                else
                {
                    this.weakData = new WeakReference(data);
                }
            }
            else
            {
                this.data = data;
            }
        }

        public void SetChildNode(string name, SegmentHierarchyNode<TData> node)
        {
            this.children[name] = node;
        }

        public void Collect(List<KeyValuePair<BaseUriWithWildcard, TData>> result)
        {
            TData localData = this.Data;
            if (localData != null)
            {
                result.Add(new KeyValuePair<BaseUriWithWildcard, TData>(path, localData));
            }

            foreach (SegmentHierarchyNode<TData> child in this.children.Values)
            {
                child.Collect(result);
            }
        }

        public bool TryGetChild(string segment, out SegmentHierarchyNode<TData> value)
        {
            return children.TryGetValue(segment, out value);
        }

        public void RemoveData()
        {
            SetData(null, null);
        }

        // bool is whether to remove this node
        public bool RemovePath(string[] path, int seg)
        {
            if (seg == path.Length)
            {
                RemoveData();
                return this.children.Count == 0;
            }

            SegmentHierarchyNode<TData> node;
            if (!TryGetChild(path[seg], out node))
            {
                return (this.children.Count == 0 && Data == null);
            }

            if (node.RemovePath(path, seg + 1))
            {
                this.children.Remove(path[seg]);
                return (this.children.Count == 0 && Data == null);
            }
            else
            {
                return false;
            }
        }
    }
}
