//------------------------------------------------------------------------------
// <copyright file="XmlSiteMapProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * XmlSiteMapProvider class definition
 *
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    // XmlMapProvider that generates sitemap tree from xml files

    public class XmlSiteMapProvider : StaticSiteMapProvider, IDisposable {

        private string _filename;
        private VirtualPath _virtualPath;
        private VirtualPath _normalizedVirtualPath;
        private SiteMapNode _siteMapNode;
        private XmlDocument _document;
        private bool _initialized;
        private FileChangeEventHandler _handler;
        private StringCollection _parentSiteMapFileCollection;

        private const string _providerAttribute = "provider";
        private const string _siteMapFileAttribute = "siteMapFile";
        private const string _siteMapNodeName = "siteMapNode";
        private const string _xmlSiteMapFileExtension = ".sitemap";
        private const string _resourcePrefix = "$resources:";
        private const int _resourcePrefixLength = 10;
        private const char _resourceKeySeparator = ',';
        private static readonly char[] _seperators = new char[] { ';', ',' };

        private ArrayList _childProviderList;

        // table containing mappings from child providers to their root nodes.
        private Hashtable _childProviderTable;


        public XmlSiteMapProvider() {
        }

        private ArrayList ChildProviderList {
            get {
                ArrayList returnList = _childProviderList;
                if (returnList == null) {
                    lock (_lock) {
                        if (_childProviderList == null) {
                            returnList = ArrayList.ReadOnly(new ArrayList(ChildProviderTable.Keys));
                            _childProviderList = returnList;
                        }
                        else {
                            returnList = _childProviderList;
                        }
                    }
                }

                return returnList;
            }
        }

        private Hashtable ChildProviderTable {
            get {
                if (_childProviderTable == null) {
                    lock (_lock) {
                        if (_childProviderTable == null) {
                            _childProviderTable = new Hashtable();
                        }
                    }
                }

                return _childProviderTable;
            }
        }


        public override SiteMapNode RootNode {
            get {
                BuildSiteMap();
                SiteMapNode node = ReturnNodeIfAccessible(_siteMapNode);
                return ApplyModifierIfExists(node);
            }
        }

        public override SiteMapNode CurrentNode {
            get {
                return ApplyModifierIfExists(base.CurrentNode);
            }
        }

        public override SiteMapNode GetParentNode(SiteMapNode node) {
            SiteMapNode parentNode = base.GetParentNode(node);
            return ApplyModifierIfExists(parentNode);
        }

        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node) {
            SiteMapNodeCollection subNodes = base.GetChildNodes(node);
            HttpContext context = HttpContext.Current;

            // Do nothing if the modifier doesn't apply
            if (context == null || !context.Response.UsePathModifier || subNodes.Count == 0) {
                return subNodes;
            }

            // Apply the modifier to the children nodes
            SiteMapNodeCollection resultNodes = new SiteMapNodeCollection(subNodes.Count);
        
            foreach (SiteMapNode n in subNodes) {
                resultNodes.Add(ApplyModifierIfExists(n));
            }

            return resultNodes;
        }

        protected internal override void AddNode(SiteMapNode node, SiteMapNode parentNode) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            if (parentNode == null) {
                throw new ArgumentNullException("parentNode");
            }

            SiteMapProvider ownerProvider = node.Provider;
            SiteMapProvider parentOwnerProvider = parentNode.Provider;

            if (ownerProvider != this) {
                throw new ArgumentException(SR.GetString(SR.XmlSiteMapProvider_cannot_add_node, node.ToString()), "node");
            }

            if (parentOwnerProvider != this) {
                throw new ArgumentException(SR.GetString(SR.XmlSiteMapProvider_cannot_add_node, parentNode.ToString()), "parentNode");
            }

            lock (_lock) {
                // First remove it from its current location.
                RemoveNode(node);
                AddNodeInternal(node, parentNode, null);
            }
        }

        private void AddNodeInternal(SiteMapNode node, SiteMapNode parentNode, XmlNode xmlNode) {
            lock (_lock) {
                String url = node.Url;
                String key = node.Key;

                bool isValidUrl = false;

                // Only add the node to the url table if it's a static node.
                if (!String.IsNullOrEmpty(url)) {
                    if (UrlTable[url] != null) {
                        if (xmlNode != null) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url, url),
                                xmlNode);
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(
                                SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url, url));
                        }
                    }

                    isValidUrl = true;
                }

                if (KeyTable.Contains(key)) {
                    if (xmlNode != null) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key, key),
                            xmlNode);
                    }
                    else {
                        throw new InvalidOperationException(
                           SR.GetString(SR.XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key, key));
                    }
                }

                if (isValidUrl) {
                    UrlTable[url] = node;
                }

                KeyTable[key] = node;

                // Add the new node into parentNode collection
                if (parentNode != null) {
                    ParentNodeTable[node] = parentNode;

                    if (ChildNodeCollectionTable[parentNode] == null) {
                        ChildNodeCollectionTable[parentNode] = new SiteMapNodeCollection();
                    }

                    ((SiteMapNodeCollection)ChildNodeCollectionTable[parentNode]).Add(node);
                }
            }
        }

        protected virtual void AddProvider(string providerName, SiteMapNode parentNode) {
            if (parentNode == null) {
                throw new ArgumentNullException("parentNode");
            }

            if (parentNode.Provider != this) {
                throw new ArgumentException(SR.GetString(SR.XmlSiteMapProvider_cannot_add_node, parentNode.ToString()), "parentNode");
            }

            SiteMapNode node = GetNodeFromProvider(providerName);
            AddNodeInternal(node, parentNode, null);
        }


        [SuppressMessage("Microsoft.Security", "MSEC1205:DoNotAllowDtdOnXmlTextReader", Justification = "Legacy code that trusts our developer-controlled input.")]
        public override SiteMapNode BuildSiteMap() {

            SiteMapNode tempNode = _siteMapNode;

            // If siteMap is already constructed, simply returns it.
            // Child providers will only be updated when the parent providers need to access them.
            if (tempNode != null) {
                return tempNode;
            }

            XmlDocument document = GetConfigDocument();

            lock (_lock) {
                if (_siteMapNode != null) {
                    return _siteMapNode;
                }

                Clear();

                // Need to check if the sitemap file exists before opening it.
                CheckSiteMapFileExists();

                try {
                    using (Stream stream = _normalizedVirtualPath.OpenFile()) {
                        XmlReader reader = new XmlTextReader(stream);
                        document.Load(reader);
                    }
                }
                catch (XmlException e) {
                    string sourceFile = _virtualPath.VirtualPathString;
                    string physicalDir = _normalizedVirtualPath.MapPathInternal();
                    if (physicalDir != null && HttpRuntime.HasPathDiscoveryPermission(physicalDir)) {
                        sourceFile = physicalDir;
                    }

                    throw new ConfigurationErrorsException(
                                            SR.GetString(SR.XmlSiteMapProvider_Error_loading_Config_file, _virtualPath, e.Message),
                                            e, sourceFile, e.LineNumber);
                }
                catch (Exception e) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.XmlSiteMapProvider_Error_loading_Config_file, _virtualPath, e.Message), e);
                }

                XmlNode node = null;
                foreach (XmlNode siteMapMode in document.ChildNodes) {
                    if (String.Equals(siteMapMode.Name, "siteMap", StringComparison.Ordinal)) {
                        node = siteMapMode;
                        break;
                    }
                }

                if (node == null)
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.XmlSiteMapProvider_Top_Element_Must_Be_SiteMap),
                        document);

                bool enableLocalization = false;
                HandlerBase.GetAndRemoveBooleanAttribute(node, "enableLocalization", ref enableLocalization);
                EnableLocalization = enableLocalization;

                XmlNode topElement = null;
                foreach (XmlNode subNode in node.ChildNodes) {
                    if (subNode.NodeType == XmlNodeType.Element) {
                        if (!_siteMapNodeName.Equals(subNode.Name)) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.XmlSiteMapProvider_Only_SiteMapNode_Allowed),
                                subNode);
                        }

                        if (topElement != null) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.XmlSiteMapProvider_Only_One_SiteMapNode_Required_At_Top),
                                subNode);
                        }

                        topElement = subNode;
                    }
                }

                if (topElement == null) {
                    throw new ConfigurationErrorsException(
                         SR.GetString(SR.XmlSiteMapProvider_Only_One_SiteMapNode_Required_At_Top),
                         node);
                }

                Queue queue = new Queue(50);

                // The parentnode of the top node does not exist,
                // simply add a null to satisfy the ConvertFromXmlNode condition.
                queue.Enqueue(null);
                queue.Enqueue(topElement);
                _siteMapNode = ConvertFromXmlNode(queue);

                return _siteMapNode;
            }
        }

        private void CheckSiteMapFileExists() {
            if (!System.Web.UI.Util.VirtualFileExistsWithAssert(_normalizedVirtualPath)) {
                throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_FileName_does_not_exist, _virtualPath));
            }
        }


        protected override void Clear() {
            lock (_lock) {
                ChildProviderTable.Clear();
                _siteMapNode = null;
                _childProviderList = null;

                base.Clear();
            }
        }

        // helper method to convert an XmlNode to a SiteMapNode
        private SiteMapNode ConvertFromXmlNode(Queue queue) {

            SiteMapNode rootNode = null;
            while (true) {
                if (queue.Count == 0) {
                    return rootNode;
                }

                SiteMapNode parentNode = (SiteMapNode)queue.Dequeue();
                XmlNode xmlNode = (XmlNode)queue.Dequeue();

                SiteMapNode node = null;

                if (!_siteMapNodeName.Equals(xmlNode.Name)) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.XmlSiteMapProvider_Only_SiteMapNode_Allowed),
                        xmlNode);
                }

                string providerName = null;
                HandlerBase.GetAndRemoveNonEmptyStringAttribute(xmlNode, _providerAttribute, ref providerName);

                // If the siteMapNode references another provider
                if (providerName != null) {
                    node = GetNodeFromProvider(providerName);

                    // No other attributes or child nodes are allowed on a provider node.
                    HandlerBase.CheckForUnrecognizedAttributes(xmlNode);
                    HandlerBase.CheckForNonCommentChildNodes(xmlNode);
                }
                else {
                    string siteMapFile = null;
                    HandlerBase.GetAndRemoveNonEmptyStringAttribute(xmlNode, _siteMapFileAttribute, ref siteMapFile);

                    if (siteMapFile != null) {
                        node = GetNodeFromSiteMapFile(xmlNode, VirtualPath.Create(siteMapFile));
                    }
                    else {
                        node = GetNodeFromXmlNode(xmlNode, queue);
                    }
                }

                AddNodeInternal(node, parentNode, xmlNode);

                if (rootNode == null) {
                    rootNode = node;
                }
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (_handler != null) {
                Debug.Assert(_filename != null);
                HttpRuntime.FileChangesMonitor.StopMonitoringFile(_filename, _handler);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void EnsureChildSiteMapProviderUpToDate(SiteMapProvider childProvider) {
            SiteMapNode oldNode = (SiteMapNode)ChildProviderTable[childProvider];

            SiteMapNode newNode = childProvider.GetRootNodeCore();
            if (newNode == null) {
                throw new ProviderException(SR.GetString(SR.XmlSiteMapProvider_invalid_sitemapnode_returned, childProvider.Name));
            }

            // child providers have been updated.
            if (!oldNode.Equals(newNode)) {

                // If the child provider table has been updated, simply return null.
                // This will happen when the current provider's sitemap file is changed or Clear() is called;
                if (oldNode == null) {
                    return;
                }

                lock (_lock) {
                    oldNode = (SiteMapNode)ChildProviderTable[childProvider];
                    // If the child provider table has been updated, simply return null. See above.
                    if (oldNode == null) {
                        return;
                    }

                    newNode = childProvider.GetRootNodeCore();
                    if (newNode == null) {
                        throw new ProviderException(SR.GetString(SR.XmlSiteMapProvider_invalid_sitemapnode_returned, childProvider.Name));
                    }

                    if (!oldNode.Equals(newNode)) {

                        // If the current provider does not contain any nodes but one child provider
                        // ie. _siteMapNode == oldNode
                        // the oldNode needs to be removed from Url table and the new node will be added.
                        if (_siteMapNode.Equals(oldNode)) {
                            UrlTable.Remove(oldNode.Url);
                            KeyTable.Remove(oldNode.Key);

                            UrlTable.Add(newNode.Url, newNode);
                            KeyTable.Add(newNode.Key, newNode);

                            _siteMapNode = newNode;
                        }

                        // First find the parent node
                        SiteMapNode parent = (SiteMapNode)ParentNodeTable[oldNode];

                        // parent is null when the provider does not contain any static nodes, ie.
                        // it only contains definition to include one child provider.
                        if (parent != null) {
                            // Update the child nodes table
                            SiteMapNodeCollection list = (SiteMapNodeCollection)ChildNodeCollectionTable[parent];

                            // Add the newNode to where the oldNode is within parent node's collection.
                            int index = list.IndexOf(oldNode);
                            if (index != -1) {
                                list.Remove(oldNode);
                                list.Insert(index, newNode);
                            }
                            else {
                                list.Add(newNode);
                            }

                            // Update the parent table
                            ParentNodeTable[newNode] = parent;
                            ParentNodeTable.Remove(oldNode);

                            // Update the Url table
                            UrlTable.Remove(oldNode.Url);
                            KeyTable.Remove(oldNode.Key);

                            UrlTable.Add(newNode.Url, newNode);
                            KeyTable.Add(newNode.Key, newNode);
                        }
                        else {
                            // Notify the parent provider to update its child provider collection.
                            XmlSiteMapProvider provider = ParentProvider as XmlSiteMapProvider;
                            if (provider != null) {
                                provider.EnsureChildSiteMapProviderUpToDate(this);
                            }
                        }

                        // Update provider nodes;
                        ChildProviderTable[childProvider] = newNode;
                        _childProviderList = null;
                    }
                }
            }
        }

        // Returns sitemap node; Search recursively in child providers if not found.

        public override SiteMapNode FindSiteMapNode(string rawUrl) {
            SiteMapNode node = base.FindSiteMapNode(rawUrl);

            if (node == null) {
                foreach(SiteMapProvider provider in ChildProviderList) {
                    // First make sure the child provider is up-to-date.
                    EnsureChildSiteMapProviderUpToDate(provider);

                    node = provider.FindSiteMapNode(rawUrl);
                    if (node != null) {
                        return node;
                    }
                }
            }

            return node;
        }

        // Returns sitemap node; Search recursively in child providers if not found.
        public override SiteMapNode FindSiteMapNodeFromKey(string key) {
            SiteMapNode node = base.FindSiteMapNodeFromKey(key);

            if (node == null) {
                foreach (SiteMapProvider provider in ChildProviderList) {
                    // First make sure the child provider is up-to-date.
                    EnsureChildSiteMapProviderUpToDate(provider);

                    node = provider.FindSiteMapNodeFromKey(key);
                    if (node != null) {
                        return node;
                    }
                }
            }

            return node;
        }

        private XmlDocument GetConfigDocument() {
            if (_document != null)
                return _document;

            if (!_initialized) {
                throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_Not_Initialized));
            }

            // Do the error checking here
            if (_virtualPath == null) {
                throw new ArgumentException(
                    SR.GetString(SR.XmlSiteMapProvider_missing_siteMapFile, _siteMapFileAttribute));
            }

            if (!_virtualPath.Extension.Equals(_xmlSiteMapFileExtension, StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_Invalid_Extension, _virtualPath));
            }

            _normalizedVirtualPath = _virtualPath.CombineWithAppRoot();
            _normalizedVirtualPath.FailIfNotWithinAppRoot();

            // Make sure the file exists
            CheckSiteMapFileExists();

            _parentSiteMapFileCollection = new StringCollection();
            XmlSiteMapProvider xmlParentProvider = ParentProvider as XmlSiteMapProvider;
            if (xmlParentProvider != null && xmlParentProvider._parentSiteMapFileCollection != null) {
                if (xmlParentProvider._parentSiteMapFileCollection.Contains(_normalizedVirtualPath.VirtualPathString)) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.XmlSiteMapProvider_FileName_already_in_use, _virtualPath));
                }

                // Copy the sitemapfiles in used from parent provider to current provider.
                foreach (string filename in xmlParentProvider._parentSiteMapFileCollection) {
                    _parentSiteMapFileCollection.Add(filename);
                }
            }

            // Add current sitemap file to the collection
            _parentSiteMapFileCollection.Add(_normalizedVirtualPath.VirtualPathString);

            _filename = HostingEnvironment.MapPathInternal(_normalizedVirtualPath);

            if (!String.IsNullOrEmpty(_filename)) {
                _handler = new FileChangeEventHandler(this.OnConfigFileChange);
                HttpRuntime.FileChangesMonitor.StartMonitoringFile(_filename, _handler);
                ResourceKey = (new FileInfo(_filename)).Name;
            }

            _document = new ConfigXmlDocument();

            return _document;
        }

        private SiteMapNode GetNodeFromProvider(string providerName) {
            SiteMapProvider provider = GetProviderFromName(providerName);
            SiteMapNode node = null;

            // Check infinite recursive sitemap files
            if (provider is XmlSiteMapProvider) {
                XmlSiteMapProvider xmlProvider = (XmlSiteMapProvider)provider;

                StringCollection parentSiteMapFileCollection = new StringCollection();
                if (_parentSiteMapFileCollection != null) {
                    foreach (string filename in _parentSiteMapFileCollection) {
                        parentSiteMapFileCollection.Add(filename);
                    }
                }

                // Make sure the provider is initialized before adding to the collection.
                xmlProvider.BuildSiteMap();

                parentSiteMapFileCollection.Add(_normalizedVirtualPath.VirtualPathString);
                if (parentSiteMapFileCollection.Contains(VirtualPath.GetVirtualPathString(xmlProvider._normalizedVirtualPath))) {
                    throw new InvalidOperationException(SR.GetString(SR.XmlSiteMapProvider_FileName_already_in_use, xmlProvider._virtualPath));
                }

                xmlProvider._parentSiteMapFileCollection = parentSiteMapFileCollection;
            }

            node = provider.GetRootNodeCore();
            if (node == null) {
                throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_invalid_GetRootNodeCore, ((ProviderBase)provider).Name));
            }

            ChildProviderTable.Add(provider, node);
            _childProviderList = null;

            provider.ParentProvider = this;

            return node;
        }

        private SiteMapNode GetNodeFromSiteMapFile(XmlNode xmlNode, VirtualPath siteMapFile) {

            SiteMapNode node = null;

            // For external sitemap files, its secuity setting is inherited from parent provider
            bool secuityTrimmingEnabled = SecurityTrimmingEnabled;
            HandlerBase.GetAndRemoveBooleanAttribute(xmlNode, _securityTrimmingEnabledAttrName, ref secuityTrimmingEnabled);

            // No other attributes or non-comment nodes are allowed on a siteMapFile node
            HandlerBase.CheckForUnrecognizedAttributes(xmlNode);
            HandlerBase.CheckForNonCommentChildNodes(xmlNode);

            XmlSiteMapProvider childProvider = new XmlSiteMapProvider();

            // siteMapFile was relative to the sitemap file where this xmlnode is defined, make it an application path.
            siteMapFile = _normalizedVirtualPath.Parent.Combine(siteMapFile);

            childProvider.ParentProvider = this;
            childProvider.Initialize(siteMapFile, secuityTrimmingEnabled);
            childProvider.BuildSiteMap();

            node = childProvider._siteMapNode;

            ChildProviderTable.Add(childProvider, node);
            _childProviderList = null;

            return node;
        }

        private void HandleResourceAttribute(XmlNode xmlNode, ref NameValueCollection collection, 
            string attrName, ref string text, bool allowImplicitResource) {
            if (String.IsNullOrEmpty(text)) {
                return;
            }

            string resourceKey = null;
            string temp = text.TrimStart(new char[] { ' ' });

            if (temp != null && temp.Length > _resourcePrefixLength) {
                if (temp.ToLower(CultureInfo.InvariantCulture).StartsWith(_resourcePrefix, StringComparison.Ordinal)) {
                    if (!allowImplicitResource) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.XmlSiteMapProvider_multiple_resource_definition, attrName), xmlNode);
                    }

                    resourceKey = temp.Substring(_resourcePrefixLength + 1);

                    if (resourceKey.Length == 0) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.XmlSiteMapProvider_resourceKey_cannot_be_empty), xmlNode);
                    }

                    // Retrieve className from attribute
                    string className = null;
                    string key = null;
                    int index = resourceKey.IndexOf(_resourceKeySeparator);
                    if (index == -1) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.XmlSiteMapProvider_invalid_resource_key, resourceKey), xmlNode);
                    }

                    className = resourceKey.Substring(0, index);
                    key = resourceKey.Substring(index + 1);

                    // Retrieve resource key and default value from attribute
                    int defaultIndex = key.IndexOf(_resourceKeySeparator);
                    if (defaultIndex != -1) {
                        text = key.Substring(defaultIndex + 1);
                        key = key.Substring(0, defaultIndex);
                    }
                    else {
                        text = null;
                    }

                    if (collection == null) {
                        collection = new NameValueCollection();
                    }

                    collection.Add(attrName, className.Trim());
                    collection.Add(attrName, key.Trim());
                }
            }
        }

        private SiteMapNode GetNodeFromXmlNode(XmlNode xmlNode, Queue queue) {
            SiteMapNode node = null;
            // static nodes
            string title = null, url = null, description = null, roles = null, resourceKey = null;

            // Url attribute is NOT required for a xml node.
            HandlerBase.GetAndRemoveStringAttribute(xmlNode, "url", ref url);
            HandlerBase.GetAndRemoveStringAttribute(xmlNode, "title", ref title);
            HandlerBase.GetAndRemoveStringAttribute(xmlNode, "description", ref description);
            HandlerBase.GetAndRemoveStringAttribute(xmlNode, "roles", ref roles);
            HandlerBase.GetAndRemoveStringAttribute(xmlNode, "resourceKey", ref resourceKey);

            // Do not add the resourceKey if the resource is not valid.
            if (!String.IsNullOrEmpty(resourceKey) && 
                !ValidateResource(ResourceKey, resourceKey + ".title")) {
                resourceKey = null;
            }

            HandlerBase.CheckForbiddenAttribute(xmlNode, _securityTrimmingEnabledAttrName);

            NameValueCollection resourceKeyCollection = null;
            bool allowImplicitResourceAttribute = String.IsNullOrEmpty(resourceKey);
            HandleResourceAttribute(xmlNode, ref resourceKeyCollection, 
                "title", ref title, allowImplicitResourceAttribute);
            HandleResourceAttribute(xmlNode, ref resourceKeyCollection, 
                "description", ref description, allowImplicitResourceAttribute);

            ArrayList roleList = new ArrayList();
            if (roles != null) {
                int foundIndex = roles.IndexOf('?');
                if (foundIndex != -1) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Auth_rule_names_cant_contain_char,
                        roles[foundIndex].ToString(CultureInfo.InvariantCulture)), xmlNode);
                }

                foreach (string role in roles.Split(_seperators)) {
                    string trimmedRole = role.Trim();
                    if (trimmedRole.Length > 0) {
                        roleList.Add(trimmedRole);
                    }
                }
            }
            roleList = ArrayList.ReadOnly(roleList);

            String key = null;

            // Make urls absolute.
            if (!String.IsNullOrEmpty(url)) {
                // URL needs to be trimmed. VSWhidbey 411041
                url = url.Trim();

                if (!UrlPath.IsAbsolutePhysicalPath(url)) {
                    if (UrlPath.IsRelativeUrl(url)) {
                        url = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url);
                    }
                }

                // VSWhidbey 418056, Reject any suspicious or mal-formed Urls.
                string decodedUrl = HttpUtility.UrlDecode(url);
                if (!String.Equals(url, decodedUrl, StringComparison.Ordinal)) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Property_Had_Malformed_Url, "url", url), xmlNode);
                }

                key = url.ToLowerInvariant();
            }
            else {
                key = Guid.NewGuid().ToString();
            }

            // attribute collection does not contain pre-defined properties like title, url, etc.
            ReadOnlyNameValueCollection attributeCollection = new ReadOnlyNameValueCollection();
            attributeCollection.SetReadOnly(false);
            foreach (XmlAttribute attribute in xmlNode.Attributes) {
                string value = attribute.Value;
                HandleResourceAttribute(xmlNode, ref resourceKeyCollection, attribute.Name, ref value, allowImplicitResourceAttribute);
                attributeCollection[attribute.Name] = value;
            }
            attributeCollection.SetReadOnly(true);

            node = new SiteMapNode(this, key, url, title, description, roleList, attributeCollection, resourceKeyCollection, resourceKey);
            node.ReadOnly = true;

            foreach (XmlNode subNode in xmlNode.ChildNodes) {
                if (subNode.NodeType != XmlNodeType.Element)
                    continue;

                queue.Enqueue(node);
                queue.Enqueue(subNode);
            }

            return node;
        }

        private SiteMapProvider GetProviderFromName(string providerName) {
            Debug.Assert(providerName != null);

            SiteMapProvider provider = SiteMap.Providers[providerName];
            if (provider == null) {
                throw new ProviderException(SR.GetString(SR.Provider_Not_Found, providerName));
            }

            return provider;
        }

        protected internal override SiteMapNode GetRootNodeCore() {
            BuildSiteMap();
            return _siteMapNode;
        }


        public override void Initialize(string name, NameValueCollection attributes) {
            if (_initialized) {
                throw new InvalidOperationException(
                    SR.GetString(SR.XmlSiteMapProvider_Cannot_Be_Inited_Twice));
            }

            if (attributes != null) {
                if (string.IsNullOrEmpty(attributes["description"])) {
                    attributes.Remove("description");
                    attributes.Add("description", SR.GetString(SR.XmlSiteMapProvider_Description));
                }

                string siteMapFile = null;
                ProviderUtil.GetAndRemoveStringAttribute(attributes, _siteMapFileAttribute, name, ref siteMapFile);
                _virtualPath = VirtualPath.CreateAllowNull(siteMapFile);
            }

            base.Initialize(name, attributes);

            if (attributes != null) {
                ProviderUtil.CheckUnrecognizedAttributes(attributes, name);
            }

            _initialized = true;
        }

        private void Initialize(VirtualPath virtualPath, bool secuityTrimmingEnabled) {
            NameValueCollection coll = new NameValueCollection();
            coll.Add(_siteMapFileAttribute, virtualPath.VirtualPathString);
            coll.Add(_securityTrimmingEnabledAttrName, System.Web.UI.Util.GetStringFromBool(secuityTrimmingEnabled));

            // Use the siteMapFile virtual path as the provider name
            Initialize(virtualPath.VirtualPathString, coll);
        }

        private void OnConfigFileChange(Object sender, FileChangeEvent e) {
            // Notifiy the parent for the change.
            XmlSiteMapProvider parentProvider = ParentProvider as XmlSiteMapProvider;
            if (parentProvider != null) {
                parentProvider.OnConfigFileChange(sender, e);
            }

            Clear();
        }

        protected internal override void RemoveNode(SiteMapNode node) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            SiteMapProvider ownerProvider = node.Provider;

            if (ownerProvider != this) {

                // Only nodes defined in this provider tree can be removed.
                SiteMapProvider parentProvider = ownerProvider.ParentProvider;
                while (parentProvider != this) {
                    if (parentProvider == null) {
                        // Cannot remove nodes defined in other providers
                        throw new InvalidOperationException(
                            SR.GetString(SR.XmlSiteMapProvider_cannot_remove_node, node.ToString(), 
                            this.Name, ownerProvider.Name));
                    }

                    parentProvider = parentProvider.ParentProvider;
                }
            }

            if (node.Equals(ownerProvider.GetRootNodeCore())) {
                throw new InvalidOperationException(SR.GetString(SR.SiteMapProvider_cannot_remove_root_node));
            }

            if (ownerProvider != this) {
                // Remove node from the owner provider.
                ownerProvider.RemoveNode(node);
            }

            base.RemoveNode(node);
        }

        protected virtual void RemoveProvider(string providerName) {
            if (providerName == null) {
                throw new ArgumentNullException("providerName");
            }

            lock (_lock) {
                SiteMapProvider provider = GetProviderFromName(providerName);
                SiteMapNode rootNode = (SiteMapNode)ChildProviderTable[provider];

                if (rootNode == null) {
                    throw new InvalidOperationException(SR.GetString(SR.XmlSiteMapProvider_cannot_find_provider, provider.Name, this.Name));
                }

                provider.ParentProvider = null;
                ChildProviderTable.Remove(provider);
                _childProviderList = null;

                base.RemoveNode(rootNode);
            }
        }

        // VSWhidbey: 493981 Helper method to check if the valid resource type exists. 
        // Note that this only returns false if the classKey cannot be found, regardless of resourceKey.
        private bool ValidateResource(string classKey, string resourceKey) {
            try {
                HttpContext.GetGlobalResourceObject(classKey, resourceKey);
            }
            catch (MissingManifestResourceException) {
                return false;
            }

            return true;
        }

        // Dev10# 923217 - SiteMapProvider URL Table Invalid Using Cookieless
        // Don't keep the modifier inside the links table. Apply the modifier as approriate on demand
        private static SiteMapNode ApplyModifierIfExists(SiteMapNode node) {
            HttpContext context = HttpContext.Current;

            // Do nothing if the modifier doesn't apply
            if (node == null || context == null || !context.Response.UsePathModifier) {
                return node;
            }

            // Set Url with the modifier applied
            SiteMapNode resultNode = node.Clone();
            resultNode.Url = context.Response.ApplyAppPathModifier(node.Url);
 
            return resultNode;
        }

        private class ReadOnlyNameValueCollection : NameValueCollection {

            public ReadOnlyNameValueCollection() {
                IsReadOnly = true;
            }

            internal void SetReadOnly(bool isReadonly) {
                IsReadOnly = isReadonly;
            }
        }
    }
}
