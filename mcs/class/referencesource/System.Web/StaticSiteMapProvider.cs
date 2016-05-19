//------------------------------------------------------------------------------
// <copyright file="SimpleSiteMapProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SimpleSiteMapProvider class definition
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.Util;

    public abstract class StaticSiteMapProvider : SiteMapProvider {

        // Table maps nodes to their child node collections
        private Hashtable _childNodeCollectionTable;
        internal IDictionary ChildNodeCollectionTable {
            get {
                if (_childNodeCollectionTable == null) {
                    lock (_lock) {
                        if (_childNodeCollectionTable == null) {
                            _childNodeCollectionTable = new Hashtable();
                        }
                    }
                }

                return _childNodeCollectionTable;
            }
        }

        // Case sensitive table that maps key to sitemap node.
        private Hashtable _keyTable;
        internal IDictionary KeyTable {
            get {
                if (_keyTable == null) {
                    lock (_lock) {
                        if (_keyTable == null) {
                            _keyTable = new Hashtable();
                        }
                    }
                }

                return _keyTable;
            }
        }

        // Table maps nodes to their parent nodes
        private Hashtable _parentNodeTable;
        internal IDictionary ParentNodeTable {
            get {
                if (_parentNodeTable == null) {
                    lock (_lock) {
                        if (_parentNodeTable == null) {
                            _parentNodeTable = new Hashtable();
                        }
                    }
                }

                return _parentNodeTable;
            }
        }

        // Case insensitive table that maps url to sitemap node.
        private Hashtable _urlTable;
        internal IDictionary UrlTable {
            get {
                if (_urlTable == null) {
                    lock (_lock) {
                        if (_urlTable == null) {
                            _urlTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                }

                return _urlTable;
            }
        }


        /// <devdoc>
        ///    <para>Add single node to provider tree and sets the parent-child relation.</para>
        /// </devdoc>
        protected internal override void AddNode(SiteMapNode node, SiteMapNode parentNode) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            lock (_lock) {
                bool validUrl = false;

                string url = node.Url;
                if (!String.IsNullOrEmpty(url)) {
                    if (HttpRuntime.AppDomainAppVirtualPath != null) {

                        if (!UrlPath.IsAbsolutePhysicalPath(url)) {
                            url = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url);

                            // Normalize url
                            url = UrlPath.MakeVirtualPathAppAbsolute(url);
                        }

                        if (UrlTable[url] != null)
                            throw new InvalidOperationException(
                                SR.GetString(SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url, url));
                    }

                    validUrl = true;
                }

                String key = node.Key;
                Debug.Assert(key != null);
                if (KeyTable.Contains(key)) {
                    throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key, key));
                }

                KeyTable[key] = node;

                if (validUrl) {
                    UrlTable[url] = node;
                }

                if (parentNode != null) {
                    ParentNodeTable[node] = parentNode;
                    if (ChildNodeCollectionTable[parentNode] == null)
                        ChildNodeCollectionTable[parentNode] = new SiteMapNodeCollection();

                    ((SiteMapNodeCollection)ChildNodeCollectionTable[parentNode]).Add(node);
                }
            }
        }


        public abstract SiteMapNode BuildSiteMap();

        // Clear tables

        protected virtual void Clear() {
            lock (_lock) {
                if (_childNodeCollectionTable != null)
                    _childNodeCollectionTable.Clear();

                if (_urlTable != null)
                    _urlTable.Clear();

                if (_parentNodeTable != null)
                    _parentNodeTable.Clear();

                if (_keyTable != null)
                    _keyTable.Clear();
            }
        }

        // Find sitemapnode in current provider

        public override SiteMapNode FindSiteMapNodeFromKey(string key) {
            SiteMapNode result = base.FindSiteMapNodeFromKey(key);

            if (result == null) {
                result = (SiteMapNode)KeyTable[key];
            }

            return ReturnNodeIfAccessible(result);
        }

        // Find sitemapnode in current provider

        public override SiteMapNode FindSiteMapNode(string rawUrl) {
            if (rawUrl == null)
                throw new ArgumentNullException("rawUrl");

            // VSWhidbey 411041. URL needs to be trimmed.
            rawUrl = rawUrl.Trim();

            if (rawUrl.Length == 0) {
                return null;
            }

            // Make sure it is an app absolute url
            if (UrlPath.IsAppRelativePath(rawUrl)) {
                rawUrl = UrlPath.MakeVirtualPathAppAbsolute(rawUrl);
            }

            BuildSiteMap();

            return ReturnNodeIfAccessible((SiteMapNode)UrlTable[rawUrl]);
        }

        // Return readonly child node collection

        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node) {
            if (node == null)
                throw new ArgumentNullException("node");

            BuildSiteMap();
            SiteMapNodeCollection collection = (SiteMapNodeCollection)ChildNodeCollectionTable[node];

            if (collection == null) {
                SiteMapNode childNodeFromKey = (SiteMapNode)KeyTable[node.Key];
                if (childNodeFromKey != null) {
                    collection = (SiteMapNodeCollection)ChildNodeCollectionTable[childNodeFromKey];
                }
            }

            if (collection != null) {
                if (!SecurityTrimmingEnabled) {
                    return SiteMapNodeCollection.ReadOnly(collection);
                }

                HttpContext context = HttpContext.Current;
                SiteMapNodeCollection trimmedCollection = new SiteMapNodeCollection(collection.Count);
                foreach (SiteMapNode subNode in collection) {
                    if (subNode.IsAccessibleToUser(context)) {
                        trimmedCollection.Add(subNode);
                    }
                }

                return SiteMapNodeCollection.ReadOnly(trimmedCollection);
            }

            return SiteMapNodeCollection.Empty;
        }


        public override SiteMapNode GetParentNode(SiteMapNode node) {
            if (node == null)
                throw new ArgumentNullException("node");

            BuildSiteMap();
            SiteMapNode parent = (SiteMapNode)ParentNodeTable[node];

            if (parent == null) {
                // Try to find the original node in the table using the key
                SiteMapNode fallbackNode = (SiteMapNode)KeyTable[node.Key];
                if (fallbackNode != null) {
                    parent = (SiteMapNode)ParentNodeTable[fallbackNode];
                }
            }

            // Try the parent providers.
            if (parent == null && ParentProvider != null) {
                parent = ParentProvider.GetParentNode(node);
            }

            return ReturnNodeIfAccessible(parent);
        }


        protected internal override void RemoveNode(SiteMapNode node) {
            if (node == null)
                throw new ArgumentNullException("node");

            lock (_lock) {
                SiteMapNode oldParent = (SiteMapNode)ParentNodeTable[node];
                if (ParentNodeTable.Contains(node))
                    ParentNodeTable.Remove(node);

                if (oldParent != null) {
                    SiteMapNodeCollection collection = (SiteMapNodeCollection)ChildNodeCollectionTable[oldParent];
                    if (collection != null && collection.Contains(node))
                        collection.Remove(node);
                }

                string url = node.Url;
                if (url != null && url.Length > 0 && UrlTable.Contains(url)) {
                    UrlTable.Remove(url);
                }

                string key = node.Key;
                if (KeyTable.Contains(key)) {
                    KeyTable.Remove(key);
                }
            }
        }
    }
}
