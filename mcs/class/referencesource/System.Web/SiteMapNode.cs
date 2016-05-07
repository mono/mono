//------------------------------------------------------------------------------
// <copyright file="SiteMapNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SiteMapNode class definition
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web {

    using System;
    using System.Configuration.Provider;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Resources;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public class SiteMapNode : ICloneable, IHierarchyData, INavigateUIData {

        private static readonly string _siteMapNodeType = typeof(SiteMapNode).Name;

        private SiteMapProvider _provider;

        private bool _readonly;
        private bool _parentNodeSet;
        private bool _childNodesSet;

        private VirtualPath _virtualPath;
        private string _title;
        private string _description;
        private string _url;
        private string _key;
        private string _resourceKey;

        private IList _roles;
        private NameValueCollection _attributes;
        private NameValueCollection _resourceKeys;

        private SiteMapNode _parentNode;
        private SiteMapNodeCollection _childNodes;

        public SiteMapNode(SiteMapProvider provider, string key) :
            this(provider, key, null, null, null, null, null, null, null) {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url) :
            this(provider, key, url, null, null, null, null, null, null) {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title) :
            this(provider, key, url, title, null, null, null, null, null) {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title, string description) :
            this(provider, key, url, title, description, null, null, null, null) {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title, string description,
            IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys, string implicitResourceKey) {

            _provider = provider;
            _title = title;
            _description = description;
            _roles = roles;
            _attributes = attributes;
            _key = key;
            _resourceKeys = explicitResourceKeys;
            _resourceKey = implicitResourceKey;

            if (url != null) {
                _url = url.Trim();
            }

            _virtualPath = CreateVirtualPathFromUrl(_url);

            if (_key == null) {
                throw new ArgumentNullException("key");
            }

            if (_provider == null) {
                throw new ArgumentNullException("provider");
            }
        }

        protected NameValueCollection Attributes {
            get {
                return _attributes;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Attributes"));
                }

                _attributes = value;
            }
        }

        // Access custom attributes.
        public virtual string this[string key] {
            get {
                string text = null;
                if (_attributes != null) {
                    text = _attributes[key];
                }

                if (_provider.EnableLocalization) {
                    // Try the implicit resource first
                    string localizedText = GetImplicitResourceString(key);
                    if (localizedText != null) {
                        return localizedText;
                    }

                    // If not found, try the explicit resource.
                    localizedText = GetExplicitResourceString(key, text, true);
                    if (localizedText != null) {
                        return localizedText;
                    }
                }

                return text;
            }

            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Item"));
                }

                if (_attributes == null) {
                    _attributes = new NameValueCollection();
                }

                _attributes[key] = value;
            }
        }

        public virtual SiteMapNodeCollection ChildNodes {
            get {
                if (_childNodesSet)
                    return _childNodes;

                return _provider.GetChildNodes(this);
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "ChildNodes"));
                }

                _childNodes = value;
                _childNodesSet = true;
            }
        }

        [
        Localizable(true)
        ]
        public virtual string Description {
            get {
                if (_provider.EnableLocalization) {
                    string localizedText = GetImplicitResourceString("description");
                    if (localizedText != null) {
                        return localizedText;
                    }

                    localizedText = GetExplicitResourceString("description", _description, true);
                    if (localizedText != null) {
                        return localizedText;
                    }
                }

                return _description == null? String.Empty : _description;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Description"));
                }

                _description = value;
            }
        }

        public string Key {
            get {
                return _key;
            }
        }

        public virtual bool HasChildNodes {
            get {
                IList children = ChildNodes;
                return children != null && children.Count > 0;
            }
        }

        public virtual SiteMapNode NextSibling {
            get {
                IList siblings = SiblingNodes;
                if (siblings == null) {
                    return null;
                }

                int index = siblings.IndexOf(this);
                if (index >= 0 && index < siblings.Count - 1) {
                    return (SiteMapNode)siblings[index + 1];
                }

                return null;
            }
        }

        // Get parent node. If not found in current provider, search recursively in parent providers.
        public virtual SiteMapNode ParentNode {
            get {
                if (_parentNodeSet)
                    return _parentNode;

                return _provider.GetParentNode(this);
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "ParentNode"));
                }

                _parentNode = value;
                _parentNodeSet = true;
            }
        }

        public virtual SiteMapNode PreviousSibling {
            get {
                IList siblings = SiblingNodes;
                if (siblings == null) {
                    return null;
                }

                int index = siblings.IndexOf(this);
                if (index > 0 && index <= siblings.Count - 1) {
                    return (SiteMapNode)siblings[index - 1];
                }

                return null;
            }
        }

        public SiteMapProvider Provider {
            get {
                return _provider;
            }
        }

        public bool ReadOnly {
            get {
                return _readonly;
            }
            set {
                _readonly = value;
            }
        }

        public String ResourceKey {
            get {
                return _resourceKey;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "ResourceKey"));
                }

                _resourceKey = value;
            }
        }

        public IList Roles {
            get {
                return _roles;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Roles"));
                }

                _roles = value;
            }
        }

        public virtual SiteMapNode RootNode {
            get {
                SiteMapNode root = _provider.RootProvider.RootNode;
                if (root == null) {
                    String name = ((ProviderBase)_provider.RootProvider).Name;
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapProvider_Invalid_RootNode, name));
                }

                return root;
            }
        }

        private SiteMapNodeCollection SiblingNodes {
            get {
                SiteMapNode parent = ParentNode;
                return parent == null? null : parent.ChildNodes;
            }
        }

        [
        Localizable(true)
        ]
        public virtual string Title {
            get {
                if (_provider.EnableLocalization) {
                    string localizedText = GetImplicitResourceString("title");
                    if (localizedText != null) {
                        return localizedText;
                    }

                    localizedText = GetExplicitResourceString("title", _title, true);
                    if (localizedText != null) {
                        return localizedText;
                    }
                }

                return _title == null? String.Empty : _title;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Title"));
                }

                _title = value;
            }
        }

        public virtual string Url {
            get {
                return _url == null? String.Empty : _url;
            }
            set {
                if (_readonly) {
                    throw new InvalidOperationException(SR.GetString(SR.SiteMapNode_readonly, "Url"));
                }

                if (value != null) {
                    _url = value.Trim();
                }

                _virtualPath = CreateVirtualPathFromUrl(_url);
            }
        }

        internal VirtualPath VirtualPath {
            get {
                return _virtualPath;
            }
        }

        private VirtualPath CreateVirtualPathFromUrl(string url) {
            if (String.IsNullOrEmpty(url)) {
                return null;
            }

            if (!UrlPath.IsValidVirtualPathWithoutProtocol(url)) {
                return null;
            }

            if (UrlPath.IsAbsolutePhysicalPath(url)) {
                return null;
            }

            // Do not generate the virtualPath class at designtime.
            if (HttpRuntime.AppDomainAppVirtualPath == null) {
                return null;
            }

            if (UrlPath.IsRelativeUrl(url) && !UrlPath.IsAppRelativePath(url)) {
                url = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url);
            }

            // Remove the query string from url so the path can be validated by Authorization module.
            int queryStringIndex = url.IndexOf('?');
            if (queryStringIndex != -1) {
                url = url.Substring(0, queryStringIndex);
            }

            return VirtualPath.Create(url, 
                VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.AllowAppRelativePath);
        }

        public virtual SiteMapNode Clone() {
            ArrayList newRoles = null;
            NameValueCollection newAttributes = null;
            NameValueCollection newResourceKeys = null;

            if (_roles != null) {
                newRoles = new ArrayList(_roles);
            }
            if (_attributes != null) {
                newAttributes = new NameValueCollection(_attributes);
            }
            if (_resourceKeys != null) {
                newResourceKeys = new NameValueCollection(_resourceKeys);
            }

            SiteMapNode newNode = new SiteMapNode(_provider, Key, Url, Title, Description, newRoles, newAttributes, newResourceKeys, _resourceKey);
            return newNode;
        }

        public virtual SiteMapNode Clone(bool cloneParentNodes) {
            SiteMapNode current = Clone();

            if (cloneParentNodes) {
                SiteMapNode node = current;
                SiteMapNode parent = ParentNode;
                while (parent != null) {
                    SiteMapNode cloneParent = parent.Clone();
                    node.ParentNode = cloneParent;
                    cloneParent.ChildNodes = new SiteMapNodeCollection(node);

                    parent = parent.ParentNode;
                    node = cloneParent;
                }
            }

            return current;
        }

        public override bool Equals(object obj) {
            SiteMapNode node = obj as SiteMapNode;
            return node != null && (_key == node.Key) && 
                (String.Equals(_url, node._url, StringComparison.OrdinalIgnoreCase));
        }

        public SiteMapNodeCollection GetAllNodes() {
            SiteMapNodeCollection collection = new SiteMapNodeCollection();
            GetAllNodesRecursive(collection);
            return SiteMapNodeCollection.ReadOnly(collection);
        }

        private void GetAllNodesRecursive(SiteMapNodeCollection collection) {
            SiteMapNodeCollection childNodes = this.ChildNodes;

            if (childNodes != null && childNodes.Count > 0) {
                collection.AddRange(childNodes);
                foreach(SiteMapNode node in childNodes)
                    node.GetAllNodesRecursive(collection);
            }
        }

        public SiteMapDataSourceView GetDataSourceView(SiteMapDataSource owner, string viewName) {
            return new SiteMapDataSourceView(owner, viewName, this);
        }


        public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView() {
            return new SiteMapHierarchicalDataSourceView(this);
        }

        // Helpe method to retrieve localized string based on attribute name
        protected string GetExplicitResourceString(string attributeName, string defaultValue, bool throwIfNotFound) {
            if (attributeName == null) {
                throw new ArgumentNullException("attributeName");
            }

            string text = null;
            if (_resourceKeys != null) {
                string[] keys = _resourceKeys.GetValues(attributeName);
                if (keys != null && keys.Length > 1) {
                    try {
                        text = ResourceExpressionBuilder.GetGlobalResourceObject(keys[0], keys[1]) as string;
                    }
                    catch (MissingManifestResourceException) {
                        if (defaultValue != null) {
                            return defaultValue;
                        }
                    }

                    if (text == null && throwIfNotFound) {
                        // throw if default value is not specified.
                        throw new InvalidOperationException(
                            SR.GetString(SR.Res_not_found_with_class_and_key, keys[0], keys[1])); ;
                    }
                }
            }

            return text;
        }

        // Only use the key to get the hashcode since url can be changed and makes the objects mutable.
        public override int GetHashCode() {
            return _key.GetHashCode();
        }

        // Helper method to retrieve localized string based on attribute name
        protected string GetImplicitResourceString(string attributeName) {
            if (attributeName == null) {
                throw new ArgumentNullException("attributeName");
            }

            string text = null;
            if (!String.IsNullOrEmpty(_resourceKey)) {
                try {
                    text = ResourceExpressionBuilder.GetGlobalResourceObject(Provider.ResourceKey, ResourceKey + "." + attributeName) as String;
                }
                catch { }
            }

            return text;
        }

        public virtual bool IsAccessibleToUser(HttpContext context) {
            return _provider.IsAccessibleToUser(context, this);
        }

        public virtual bool IsDescendantOf(SiteMapNode node) {
            SiteMapNode parent = ParentNode;
            while (parent != null) {
                if (parent.Equals(node)) {
                    return true;
                }

                parent = parent.ParentNode;
            }

            return false;
        }

        public override string ToString() {
            return Title;
        }

        #region ICloneable implementation

        /// <internalonly/>
        object ICloneable.Clone() {
            return Clone();
        }
        #endregion

        #region IHierarchyData implementation

        /// <internalonly/>
        bool IHierarchyData.HasChildren {
            get {
                return HasChildNodes;
            }
        }


        /// <internalonly/>
        object IHierarchyData.Item {
            get {
                return this;
            }
        }


        /// <internalonly/>
        string IHierarchyData.Path {
            get {
                return Key;
            }
        }


        /// <internalonly/>
        string IHierarchyData.Type {
            get {
                return _siteMapNodeType;
            }
        }


        /// <internalonly/>
        IHierarchicalEnumerable IHierarchyData.GetChildren() {
            return ChildNodes;
        }


        /// <internalonly/>
        IHierarchyData IHierarchyData.GetParent() {
            SiteMapNode parentNode = ParentNode;
            if (parentNode == null)
                return null;

            return parentNode;
        }
        #endregion

        #region INavigateUIData implementations
        string INavigateUIData.Description {
            get {
                return Description;
            }
        }
        

        /// <internalonly/>
        string INavigateUIData.Name {
             get {
                return Title;
             }
        }


        /// <internalonly/>
        string INavigateUIData.NavigateUrl {
            get {
                return Url;
            }
        }


        /// <internalonly/>
        string INavigateUIData.Value {
            get {
                return Title;
            }
        }
        #endregion
    }
}
