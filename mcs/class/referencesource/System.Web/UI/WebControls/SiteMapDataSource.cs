//------------------------------------------------------------------------------
// <copyright file="SiteMapDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [
    Designer("System.Web.UI.Design.WebControls.SiteMapDataSourceDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(SiteMapDataSource)),
    WebSysDescription(SR.SiteMapDataSource_Description),
    WebSysDisplayName(SR.SiteMapDataSource_DisplayName)
    ]

    public class SiteMapDataSource : HierarchicalDataSourceControl, IDataSource, IListSource {
        private const string DefaultViewName = "DefaultView";
        private ICollection _viewNames;
        private SiteMapDataSourceView _dataSourceView;
        private SiteMapProvider _provider;


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.SiteMapDataSource_ContainsListCollection)
        ]
        public virtual bool ContainsListCollection {
            get {
                return ListSourceHelper.ContainsListCollection(this);
            }
        }


        /// <devdoc>
        ///    <para></para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.SiteMapDataSource_Provider)
        ]
        public SiteMapProvider Provider {
            get {
                if (_provider != null)
                    return _provider;

                // If not specified, use the default provider.
                if (String.IsNullOrEmpty(SiteMapProvider)) {
                    _provider = SiteMap.Provider;

                    if (_provider == null) {
                        throw new HttpException(SR.GetString(SR.SiteMapDataSource_DefaultProviderNotFound));
                    }
                }
                else {
                    _provider = SiteMap.Providers[SiteMapProvider];

                    if (_provider == null) {
                        throw new HttpException(SR.GetString(SR.SiteMapDataSource_ProviderNotFound, SiteMapProvider));
                    }
                }

                return _provider;
            }
            set {
                if (_provider != value) {
                    _provider = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the starting node should be displayed.</para>
        /// </devdoc>
        [
        DefaultValue(true),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapDataSource_ShowStartingNode)
        ]
        public virtual bool ShowStartingNode {
            get {
                object o = ViewState["ShowStartingNode"];
                return (o == null) ? true : (bool)o;
            }
            set {
                if (value != ShowStartingNode) {
                    ViewState["ShowStartingNode"] = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the name of the SiteMapProvider used to populate the datasource control.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapDataSource_SiteMapProvider)
        ]
        public virtual string SiteMapProvider {
            get {
                string provider = ViewState["SiteMapProvider"] as string;
                return (provider == null) ? String.Empty : provider;
            }
            set {
                if (value != SiteMapProvider) {
                    _provider = null;
                    ViewState["SiteMapProvider"] = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the starting node offset used to populate the datasource control.</para>
        /// </devdoc>
        [
        DefaultValue(0),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapDataSource_StartingNodeOffset)
        ]
        public virtual int StartingNodeOffset {
            get{
                object o = ViewState["StartingNodeOffset"];
                if (o == null) {
                    return 0;
                }
                return (int)o;
            }
            set {
                if (value != StartingNodeOffset) {
                    ViewState["StartingNodeOffset"] = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        ///    <para></para>
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapDataSource_StartFromCurrentNode)
        ]
        public virtual bool StartFromCurrentNode {
            get {
                object o = ViewState["StartFromCurrentNode "];
                return (o == null) ? false : (bool)o;
            }
            set {
                if (value != StartFromCurrentNode) {
                    ViewState["StartFromCurrentNode "] = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the starting node url used to populate the datasource control.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapDataSource_StartingNodeUrl)
        ]
        public virtual string StartingNodeUrl {
            get {
                string startingNodeUrl = ViewState["StartingNodeUrl"] as string;
                return (startingNodeUrl == null) ? String.Empty : startingNodeUrl;
            }
            set {
                if (value != StartingNodeUrl) {
                    ViewState["StartingNodeUrl"] = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        // Helper method to return the nodes.
        private SiteMapNodeCollection GetNodes() {
            SiteMapNode node = null;
            int startingNodeOffset = StartingNodeOffset;

            if (!String.IsNullOrEmpty(StartingNodeUrl) && StartFromCurrentNode) {
                throw new InvalidOperationException(SR.GetString(SR.SiteMapDataSource_StartingNodeUrlAndStartFromcurrentNode_Defined));
            }

            if (StartFromCurrentNode) {
                node = Provider.CurrentNode;
            }
            else if (!String.IsNullOrEmpty(StartingNodeUrl)) {
                node = Provider.FindSiteMapNode(MakeUrlAbsolute(StartingNodeUrl));
                if (node == null) {
                    throw new ArgumentException(SR.GetString(SR.SiteMapPath_CannotFindUrl, StartingNodeUrl));
                }
            }
            else {
                node = Provider.RootNode;
            }

            if (node == null) {
                return null;
            }

            if (startingNodeOffset <= 0) {
                if (startingNodeOffset != 0) {
                    // Hind neighborhood nodes based on startingNodeOffset
                    Provider.HintNeighborhoodNodes(node, Math.Abs(startingNodeOffset), 0);

                    SiteMapNode parentNode = node.ParentNode;
                    while (startingNodeOffset < 0 && parentNode != null) {
                        node = node.ParentNode;
                        parentNode = node.ParentNode;
                        startingNodeOffset++;
                    }
                }

                return GetNodes(node);
            }

            // else if (startingNodeOffset > 0)
            SiteMapNode currentNode = Provider.GetCurrentNodeAndHintAncestorNodes(-1);

            // If the current node is not in StartingNode's subtree, return null.
            if (currentNode == null || !currentNode.IsDescendantOf(node) || currentNode.Equals(node)) {
                return null;
            }

            SiteMapNode leadingNode = currentNode;

            // Create a gap of n levels between the following and the leading nodes.
            for (int i = 0; i < startingNodeOffset; i++) {
                leadingNode = leadingNode.ParentNode;

                // If the current node is within n levels below the starting node, 
                // use the current node.
                if (leadingNode == null || leadingNode.Equals(node)) {
                    return GetNodes(currentNode);
                }
            }

            SiteMapNode followingNode = currentNode;
            while (leadingNode != null && !leadingNode.Equals(node)) {
                followingNode = followingNode.ParentNode;
                leadingNode = leadingNode.ParentNode;
            }

            return GetNodes(followingNode);
        }

        private SiteMapNodeCollection GetNodes(SiteMapNode node) {
            if (ShowStartingNode) {
                return new SiteMapNodeCollection(node);
            }

            return node.ChildNodes;
        }


        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath) {

            if (Provider == null)
                throw new HttpException(SR.GetString(SR.SiteMapDataSource_ProviderNotFound, SiteMapProvider));

            return GetTreeView(viewPath);
        }


        public virtual IList GetList() {
            return ListSourceHelper.GetList(this);
        }

        // Helper method to get the non-hierarhical path view. ie. from BaseNode to currentNode.
        // if currentNode is not in the subtree rooted at basenode, returns empty collection.
        internal SiteMapNodeCollection GetPathNodeCollection(string viewPath) {

            SiteMapNodeCollection collection = null;

            if (String.IsNullOrEmpty(viewPath)) {
                collection = GetNodes();
            }
            else {
                // Otherwise, return the child nodes specified by the key (viewPath)
                SiteMapNode node = Provider.FindSiteMapNodeFromKey(viewPath);
                if (node != null) {
                    collection = node.ChildNodes;
                }
            }

            if (collection == null) {
                collection = SiteMapNodeCollection.Empty;
            }

            return collection;
        }

        private HierarchicalDataSourceView GetTreeView(string viewPath) {

            SiteMapNode node = null;

            // When querying for the whole view, returns the view starting from the designated node.
            if (String.IsNullOrEmpty(viewPath)) {
                SiteMapNodeCollection nodes = GetNodes();
                if (nodes != null) {
                    return nodes.GetHierarchicalDataSourceView();
                }
            }
            // Otherwise, return the child nodes specified by the key (viewPath)
            else {
                node = Provider.FindSiteMapNodeFromKey(viewPath);
                if (node != null)
                    return node.ChildNodes.GetHierarchicalDataSourceView();
            }

            // return the view of an empty readonly collection
            return SiteMapNodeCollection.Empty.GetHierarchicalDataSourceView();
        }


        public virtual DataSourceView GetView(string viewName) {
            if (Provider == null)
                throw new HttpException(SR.GetString(SR.SiteMapDataSource_ProviderNotFound, SiteMapProvider));

            if (_dataSourceView == null) {
                _dataSourceView = SiteMapNodeCollection.ReadOnly(GetPathNodeCollection(viewName)).GetDataSourceView(this, String.Empty);

            }

            return _dataSourceView;
        }


        public virtual ICollection GetViewNames() {
            if (_viewNames == null) {
                _viewNames = new string[1] { DefaultViewName };
            }
            return _viewNames;
        }

        private string MakeUrlAbsolute(string url) {
            // check if its empty or already absolute
            if (url.Length == 0 || !UrlPath.IsRelativeUrl(url)) {
                return url;
            }

            string baseUrl = AppRelativeTemplateSourceDirectory;
            if (baseUrl.Length == 0) {
                return url;
            }

            // Make it absolute
            return UrlPath.Combine(baseUrl, url);
        }

        #region IDataSource implementations
        /// <summary>
        ///   Raised when the underlying data source has changed. The
        ///   change may be due to a change in the control's properties,
        ///   or a change in the data due to an edit action performed by
        ///   the DataSourceControl.
        /// </summary>
        event EventHandler IDataSource.DataSourceChanged {
            add {
                ((IHierarchicalDataSource)this).DataSourceChanged += value;
            }
            remove {
                ((IHierarchicalDataSource)this).DataSourceChanged -= value;
            }
        }


        /// <internalonly/>
        DataSourceView IDataSource.GetView(string viewName) {
            return GetView(viewName);
        }


        /// <internalonly/>
        ICollection IDataSource.GetViewNames() {
            return GetViewNames();
        }
        #endregion

        #region Implementation of IListSource

        /// <internalonly/>
        bool IListSource.ContainsListCollection {
            get {
                if (DesignMode) {
                    return false;
                }
                return ContainsListCollection;
            }
        }


        /// <internalonly/>
        IList IListSource.GetList() {
            if (DesignMode) {
                return null;
            }
            return GetList();
        }
        #endregion
    }
}
