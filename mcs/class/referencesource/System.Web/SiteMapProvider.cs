//------------------------------------------------------------------------------
// <copyright file="SiteMapProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Copyright (c) 2002 Microsoft Corporation
 */
namespace System.Web {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.Util;
    using System.Security.Permissions;

    public abstract class SiteMapProvider : ProviderBase {

        private bool _securityTrimmingEnabled;
        private bool _enableLocalization;
        private String _resourceKey;

        internal const String _securityTrimmingEnabledAttrName = "securityTrimmingEnabled";
        private const string _allRoles = "*";

        private SiteMapProvider _rootProvider;
        private SiteMapProvider _parentProvider;
        private object _resolutionTicket = new object();

        internal readonly object _lock = new Object();

        public virtual SiteMapNode CurrentNode {
            get {
                HttpContext context = HttpContext.Current;
                SiteMapNode result = null;

                // First check the SiteMap resolve events.
                result = ResolveSiteMapNode(context);

                if (result == null) {
                    result = FindSiteMapNode(context);
                }

                return ReturnNodeIfAccessible(result);
            }
        }

        public bool EnableLocalization {
            get {
                return _enableLocalization;
            }
            set {
                _enableLocalization = value;
            }
        }

        // Parent provider
        public virtual SiteMapProvider ParentProvider {
            get {
                return _parentProvider;
            }
            set {
                _parentProvider = value;
            }
        }

        public string ResourceKey {
            get {
                return _resourceKey;
            }
            set {
                _resourceKey = value;
            }
        }

        public virtual SiteMapProvider RootProvider {
            get {
                if (_rootProvider == null) {
                    lock (_lock) {
                        if (_rootProvider == null) {
                            Hashtable providers = new Hashtable();
                            SiteMapProvider candidate = this;

                            providers.Add(candidate, null);
                            while (candidate.ParentProvider != null) {
                                if (providers.Contains(candidate.ParentProvider))
                                    throw new ProviderException(SR.GetString(SR.SiteMapProvider_Circular_Provider));

                                candidate = candidate.ParentProvider;
                                providers.Add(candidate, null);
                            }

                            _rootProvider = candidate;
                        }
                    }
                }

                return _rootProvider;
            }
        }

        public virtual SiteMapNode RootNode {
            get {
                SiteMapNode node = GetRootNodeCore();
                return ReturnNodeIfAccessible(node);
            }
        }

        public bool SecurityTrimmingEnabled {
            get {
                return _securityTrimmingEnabled;
            }
        }

        public event SiteMapResolveEventHandler SiteMapResolve;

        /// <devdoc>
        ///    <para>Add single node to provider.</para>
        /// </devdoc>
        protected virtual void AddNode(SiteMapNode node) {
            AddNode(node, null);
        }

        protected internal virtual void AddNode(SiteMapNode node, SiteMapNode parentNode) {
            throw new NotImplementedException();
        }

        public virtual SiteMapNode FindSiteMapNode(HttpContext context) {
            if (context == null) {
                return null;
            }

            string rawUrl = context.Request.RawUrl;

            SiteMapNode result = null;

            // First check the RawUrl
            result = FindSiteMapNode(rawUrl);

            if (result == null) {
                int queryStringIndex = rawUrl.IndexOf("?", StringComparison.Ordinal);
                if (queryStringIndex != -1) {
                    // check the RawUrl without querystring
                    result = FindSiteMapNode(rawUrl.Substring(0, queryStringIndex));
                }

                if (result == null) {
                    Page page = context.CurrentHandler as Page;
                    if (page != null) {
                        // Try without server side query strings
                        string qs = page.ClientQueryString;
                        if (qs.Length > 0) {
                            result = FindSiteMapNode(context.Request.Path + "?" + qs);
                        }
                    }

                    if (result == null) {
                        // Check the request path
                        result = FindSiteMapNode(context.Request.Path);
                    }
                }
            }

            return result;
        }

        public virtual SiteMapNode FindSiteMapNodeFromKey(string key) {
            return FindSiteMapNode(key);
        }

        public abstract SiteMapNode FindSiteMapNode(string rawUrl);

        public abstract SiteMapNodeCollection GetChildNodes(SiteMapNode node);

        public virtual SiteMapNode GetCurrentNodeAndHintAncestorNodes(int upLevel) {
            if (upLevel < -1) {
                throw new ArgumentOutOfRangeException("upLevel");
            }

            return CurrentNode;
        }

        public virtual SiteMapNode GetCurrentNodeAndHintNeighborhoodNodes(int upLevel, int downLevel) {
            if (upLevel < -1) {
                throw new ArgumentOutOfRangeException("upLevel");
            }

            if (downLevel < -1) {
                throw new ArgumentOutOfRangeException("downLevel");
            }

            return CurrentNode;
        }

        public abstract SiteMapNode GetParentNode(SiteMapNode node);

        public virtual SiteMapNode GetParentNodeRelativeToCurrentNodeAndHintDownFromParent (
            int walkupLevels, int relativeDepthFromWalkup) {

            if (walkupLevels < 0) {
                throw new ArgumentOutOfRangeException("walkupLevels");
            }

            if (relativeDepthFromWalkup < 0) {
                throw new ArgumentOutOfRangeException("relativeDepthFromWalkup");
            }

            // First get current nodes and hints about its ancestors.
            SiteMapNode currentNode = GetCurrentNodeAndHintAncestorNodes(walkupLevels);

            // Simply return if the currentNode is null.
            if (currentNode == null) {
                return null;
            }

            // Find the target node by walk up the parent tree.
            SiteMapNode targetNode = GetParentNodesInternal(currentNode, walkupLevels);

            if (targetNode == null) {
                return null;
            }

            // Get hints about its lower neighborhood nodes.
            HintNeighborhoodNodes(targetNode, 0, relativeDepthFromWalkup);

            return targetNode;
        }

        public virtual SiteMapNode GetParentNodeRelativeToNodeAndHintDownFromParent(
            SiteMapNode node, int walkupLevels, int relativeDepthFromWalkup) {

            if (walkupLevels < 0) {
                throw new ArgumentOutOfRangeException("walkupLevels");
            }

            if (relativeDepthFromWalkup < 0) {
                throw new ArgumentOutOfRangeException("relativeDepthFromWalkup");
            }

            if (node == null) {
                throw new ArgumentNullException("node");
            }

            // First get hints about ancestor nodes;
            HintAncestorNodes(node, walkupLevels);

            // walk up the parent node until the target node is found.
            SiteMapNode ancestorNode = GetParentNodesInternal(node, walkupLevels);

            if (ancestorNode == null) {
                return null;
            }

            // Get hints about its neighthood nodes
            HintNeighborhoodNodes(ancestorNode, 0, relativeDepthFromWalkup);

            return ancestorNode;
        }

        private SiteMapNode GetParentNodesInternal(SiteMapNode node, int walkupLevels) {
            Debug.Assert(node != null);
            if (walkupLevels <= 0) {
                return node;
            }

            do {
                node = node.ParentNode;
                walkupLevels--;
            } while (node != null && walkupLevels != 0);

            return node;
        }

        /*
         * A reference node that must be returned by all sitemap providers, this is
         * required for the parent provider to keep track of the relations between
         * two providers.
         * For example, the parent provider uses this method to keep track of the parent
         * node of child provider's root node.
         */
        protected internal abstract SiteMapNode GetRootNodeCore();

        protected static SiteMapNode GetRootNodeCoreFromProvider(SiteMapProvider provider) {
            return provider.GetRootNodeCore();
        }

        public virtual void HintAncestorNodes(SiteMapNode node, int upLevel) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (upLevel < -1) {
                throw new ArgumentOutOfRangeException("upLevel");
            }
        }

        public virtual void HintNeighborhoodNodes(SiteMapNode node, int upLevel, int downLevel) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (upLevel < -1) {
                throw new ArgumentOutOfRangeException("upLevel");
            }

            if (downLevel < -1) {
                throw new ArgumentOutOfRangeException("downLevel");
            }
        }

        public override void Initialize(string name, NameValueCollection attributes) {
            if (attributes != null) {
                if (string.IsNullOrEmpty(attributes["description"])) {
                    attributes.Remove("description");
                    attributes.Add("description", this.GetType().Name);
                }

                ProviderUtil.GetAndRemoveBooleanAttribute(attributes, _securityTrimmingEnabledAttrName, Name, ref _securityTrimmingEnabled);
            }

            base.Initialize(name, attributes);
        }

        public virtual bool IsAccessibleToUser(HttpContext context, SiteMapNode node) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (!SecurityTrimmingEnabled) {
                return true;
            }

            if (node.Roles != null) {
                foreach (string role in node.Roles) {
                    // Grant access if one of the roles is a "*".
                    if (role == _allRoles || 
                        context.User != null && context.User.IsInRole(role)) {
                        return true;
                    }
                }
            }

            VirtualPath virtualPath = node.VirtualPath;
            if (virtualPath == null ||
                !virtualPath.IsWithinAppRoot) {

                return false;
            }

            return System.Web.UI.Util.IsUserAllowedToPath(context, virtualPath);
        }

        protected internal virtual void RemoveNode(SiteMapNode node) {
            throw new NotImplementedException();
        }

        protected SiteMapNode ResolveSiteMapNode(HttpContext context) {
            SiteMapResolveEventHandler eventHandler = SiteMapResolve;
            if (eventHandler == null)
                return null;

            if (!context.Items.Contains(_resolutionTicket)) {
                context.Items.Add(_resolutionTicket, true);

                try {
                    Delegate[] ds = eventHandler.GetInvocationList();
                    int len = ds.Length;
                    for (int i = 0; i < len; i++) {
                        SiteMapNode ret = ((SiteMapResolveEventHandler)ds[i])(this, new SiteMapResolveEventArgs(context, this));
                        if (ret != null) {
                            return ret;
                        }
                    }
                } finally {
                    context.Items.Remove(_resolutionTicket);
                }
            }


            return null;
        }

        internal SiteMapNode ReturnNodeIfAccessible(SiteMapNode node) {
            if (node != null && node.IsAccessibleToUser(HttpContext.Current)) {
                return node;
            }

            return null;
        }
    }
}
