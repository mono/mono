//------------------------------------------------------------------------------
// <copyright file="SiteMapPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [
    Designer("System.Web.UI.Design.WebControls.SiteMapPathDesigner, " + AssemblyRef.SystemDesign)
    ]

    public class SiteMapPath : CompositeControl {

        private const string _defaultSeparator = " > ";

        private static readonly object _eventItemCreated = new object();
        private static readonly object _eventItemDataBound = new object();
        private SiteMapProvider _provider = null;

        private Style _currentNodeStyle;
        private Style _rootNodeStyle;
        private Style _nodeStyle;
        private Style _pathSeparatorStyle;

        private Style _mergedCurrentNodeStyle;
        private Style _mergedRootNodeStyle;

        private ITemplate _currentNodeTemplate;
        private ITemplate _rootNodeTemplate;
        private ITemplate _nodeTemplate;
        private ITemplate _pathSeparatorTemplate;


        public SiteMapPath() {
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.SiteMapPath_CurrentNodeStyle)
        ]
        public Style CurrentNodeStyle {
            get {
                if (_currentNodeStyle == null) {
                    _currentNodeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_currentNodeStyle).TrackViewState();
                    }
                }

                return _currentNodeStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the current node is rendered. </para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(SiteMapNodeItem)),
        WebSysDescription(SR.SiteMapPath_CurrentNodeTemplate)
        ]
        public virtual ITemplate CurrentNodeTemplate {
            get {
                return _currentNodeTemplate;
            }
            set {
                _currentNodeTemplate = value;
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.SiteMapPath_NodeStyle)
        ]
        public Style NodeStyle {
            get {
                if (_nodeStyle == null) {
                    _nodeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_nodeStyle).TrackViewState();
                    }
                }

                return _nodeStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the parent node is rendered. </para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(SiteMapNodeItem)),
        WebSysDescription(SR.SiteMapPath_NodeTemplate)
        ]
        public virtual ITemplate NodeTemplate {
            get {
                return _nodeTemplate;
            }
            set {
                _nodeTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the number of parent nodes to display.</para>
        /// </devdoc>
        [
        DefaultValue(-1),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapPath_ParentLevelsDisplayed)
        ]
        public virtual int ParentLevelsDisplayed {
            get {
                object o = ViewState["ParentLevelsDisplayed"];
                if (o == null) {
                    return -1;
                }
                return (int)o;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ParentLevelsDisplayed"] = value;
            }
        }


        [
        DefaultValue(PathDirection.RootToCurrent),
        WebCategory("Appearance"),
        WebSysDescription(SR.SiteMapPath_PathDirection)
        ]
        public virtual PathDirection PathDirection {
            get {
                object o = ViewState["PathDirection"];
                if (o == null) {
                    return PathDirection.RootToCurrent;
                }
                return (PathDirection)o;
            }
            set {
                if ((value < PathDirection.RootToCurrent) || (value > PathDirection.CurrentToRoot)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["PathDirection"] = value;
            }
        }


        [
        DefaultValue(_defaultSeparator),
        Localizable(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.SiteMapPath_PathSeparator)
        ]
        public virtual string PathSeparator {
            get {
                string s = (string)ViewState["PathSeparator"];
                if (s == null) {
                    return _defaultSeparator;
                }
                return s;
            }
            set {
                ViewState["PathSeparator"] = value;
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.SiteMapPath_PathSeparatorStyle)
        ]
        public Style PathSeparatorStyle {
            get {
                if (_pathSeparatorStyle == null) {
                    _pathSeparatorStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_pathSeparatorStyle).TrackViewState();
                    }
                }

                return _pathSeparatorStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the path Separator is rendered. </para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(SiteMapNodeItem)),
        WebSysDescription(SR.SiteMapPath_PathSeparatorTemplate)
        ]
        public virtual ITemplate PathSeparatorTemplate {
            get {
                return _pathSeparatorTemplate;
            }
            set {
                _pathSeparatorTemplate = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.SiteMapPath_Provider)
        ]
        public SiteMapProvider Provider {
            get {
                // Designer must specify a provider, as code below access runtime config
                if (_provider != null || DesignMode)
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
                _provider = value;
            }
        }


        [
        DefaultValue(false),
        WebCategory("Appearance"),
        WebSysDescription(SR.SiteMapPath_RenderCurrentNodeAsLink)
        ]
        public virtual bool RenderCurrentNodeAsLink {
            get {
                object o = ViewState["RenderCurrentNodeAsLink"];
                if (o == null) {
                    return false;
                }

                return (bool)o;
            }
            set {
                ViewState["RenderCurrentNodeAsLink"] = value;
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.SiteMapPath_RootNodeStyle)
        ]
        public Style RootNodeStyle {
            get {
                if (_rootNodeStyle == null) {
                    _rootNodeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_rootNodeStyle).TrackViewState();
                    }
                }

                return _rootNodeStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the root node is rendered. </para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(SiteMapNodeItem)),
        WebSysDescription(SR.SiteMapPath_RootNodeTemplate)
        ]
        public virtual ITemplate RootNodeTemplate {
            get {
                return _rootNodeTemplate;
            }
            set {
                _rootNodeTemplate = value;
            }
        }


        [
        Localizable(true),
        WebCategory("Accessibility"),
        WebSysDefaultValue(SR.SiteMapPath_Default_SkipToContentText),
        WebSysDescription(SR.SiteMapPath_SkipToContentText)
        ]
        public virtual String SkipLinkText {
            get {
                string s = ViewState["SkipLinkText"] as String;
                return s == null ? SR.GetString(SR.SiteMapPath_Default_SkipToContentText) : s;
            }
            set {
                ViewState["SkipLinkText"] = value;
            }
        }


        [
        DefaultValue(true),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapPath_ShowToolTips)
        ]
        public virtual bool ShowToolTips {
            get {
                object o = ViewState["ShowToolTips"];
                if (o == null) {
                    return true;
                }

                return (bool)o;
            }
            set {
                ViewState["ShowToolTips"] = value;
            }
        }


        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.SiteMapPath_SiteMapProvider)
        ]
        public virtual string SiteMapProvider {
            get {
                string provider = ViewState["SiteMapProvider"] as string;
                return (provider == null) ? String.Empty : provider;
            }
            set {
                ViewState["SiteMapProvider"] = value;
                _provider = null;
            }
        }


        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemCreated)
        ]
        public event SiteMapNodeItemEventHandler ItemCreated {
            add {
                Events.AddHandler(_eventItemCreated, value);
            }
            remove {
                Events.RemoveHandler(_eventItemCreated, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when an item is databound within a <see cref='System.Web.UI.WebControls.SiteMapPath'/> control tree.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.SiteMapPath_OnItemDataBound)
        ]
        public event SiteMapNodeItemEventHandler ItemDataBound {
            add {
                Events.AddHandler(_eventItemDataBound, value);
            }
            remove {
                Events.RemoveHandler(_eventItemDataBound, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();
            CreateControlHierarchy();
            ClearChildState();
        }


        /// <devdoc>
        ///    A protected method. Creates a control hierarchy based on current sitemap OM.
        /// </devdoc>
        protected virtual void CreateControlHierarchy() {
            if (Provider == null)
                return;

            int index = 0;

            CreateMergedStyles();

            SiteMapNode currentNode = Provider.GetCurrentNodeAndHintAncestorNodes(-1);
            if (currentNode != null) {
                SiteMapNode parentNode = currentNode.ParentNode;
                if (parentNode != null) {
                    CreateControlHierarchyRecursive(ref index, parentNode, ParentLevelsDisplayed);
                }

                CreateItem(index++, SiteMapNodeItemType.Current, currentNode);
            }
        }

        private void CreateControlHierarchyRecursive(ref int index, SiteMapNode node, int parentLevels) {
            if (parentLevels == 0)
                return;

            SiteMapNode parentNode = node.ParentNode;
            if (parentNode != null) {
                CreateControlHierarchyRecursive(ref index, parentNode, parentLevels - 1);
                CreateItem(index++, SiteMapNodeItemType.Parent, node);
            }
            else {
                CreateItem(index++, SiteMapNodeItemType.Root, node);
            }

            CreateItem(index, SiteMapNodeItemType.PathSeparator, null);
        }

        private SiteMapNodeItem CreateItem(int itemIndex, SiteMapNodeItemType itemType, SiteMapNode node) {
            SiteMapNodeItem item = new SiteMapNodeItem(itemIndex, itemType);

            int index = (PathDirection == PathDirection.CurrentToRoot ? 0 : -1);

            SiteMapNodeItemEventArgs e = new SiteMapNodeItemEventArgs(item);

            //Add sitemap nodes so that they are accessible during events.
            item.SiteMapNode = node;
            InitializeItem(item);

            // Notify items
            OnItemCreated(e);

            // Add items based on PathDirection.
            Controls.AddAt(index, item);

            // Databind.
            item.DataBind();

            // Notify items.
            OnItemDataBound(e);

            item.SiteMapNode = null;

            // SiteMapNodeItem is dynamically created each time, don't track viewstate.
            item.EnableViewState = false;

            return item;
        }

        private void CopyStyle(Style toStyle, Style fromStyle) {
            Debug.Assert(toStyle != null);

            // Review: How to change the default value of Font.Underline?
            if (fromStyle != null && fromStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FONT_UNDERLINE))
                toStyle.Font.Underline = fromStyle.Font.Underline;

            toStyle.CopyFrom(fromStyle);
        }

        private void CreateMergedStyles() {
            _mergedCurrentNodeStyle = new Style();
            CopyStyle(_mergedCurrentNodeStyle, _nodeStyle);
            CopyStyle(_mergedCurrentNodeStyle, _currentNodeStyle);

            _mergedRootNodeStyle = new Style();
            CopyStyle(_mergedRootNodeStyle, _nodeStyle);
            CopyStyle(_mergedRootNodeStyle, _rootNodeStyle);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Overriden to handle our own databinding.</para>
        /// </devdoc>
        public override void DataBind() {
            // do our own databinding
            OnDataBinding(EventArgs.Empty);

            // contained items will be databound after they have been created,
            // so we don't want to walk the hierarchy here.
        }


        /// <devdoc>
        /// <para>A protected method. Populates iteratively the specified <see cref='System.Web.UI.WebControls.SiteMapNodeItem'/> with a
        ///    sub-hierarchy of child controls.</para>
        /// </devdoc>
        protected virtual void InitializeItem(SiteMapNodeItem item) {
            Debug.Assert(_mergedCurrentNodeStyle != null && _mergedRootNodeStyle != null);

            ITemplate template = null;
            Style style = null;
            SiteMapNodeItemType itemType = item.ItemType;
            SiteMapNode node = item.SiteMapNode;

            switch (itemType) {
                case SiteMapNodeItemType.Root:
                    template = RootNodeTemplate != null ? RootNodeTemplate : NodeTemplate;
                    style = _mergedRootNodeStyle;
                    break;

                case SiteMapNodeItemType.Parent:
                    template = NodeTemplate;
                    style = _nodeStyle;
                    break;

                case SiteMapNodeItemType.Current:
                    template = CurrentNodeTemplate != null ? CurrentNodeTemplate : NodeTemplate;
                    style = _mergedCurrentNodeStyle;
                    break;

                case SiteMapNodeItemType.PathSeparator:
                    template = PathSeparatorTemplate;
                    style = _pathSeparatorStyle;
                    break;
            }

            if (template == null) {
                if (itemType == SiteMapNodeItemType.PathSeparator) {
                    Literal separatorLiteral = new Literal();
                    separatorLiteral.Mode = LiteralMode.Encode;
                    separatorLiteral.Text = PathSeparator;
                    item.Controls.Add(separatorLiteral);
                    item.ApplyStyle(style);
                }
                else if (itemType == SiteMapNodeItemType.Current && !RenderCurrentNodeAsLink) {
                    Literal currentNodeLiteral = new Literal();
                    currentNodeLiteral.Mode = LiteralMode.Encode;
                    currentNodeLiteral.Text = node.Title;
                    item.Controls.Add(currentNodeLiteral);
                    item.ApplyStyle(style);
                }
                else {
                    HyperLink link = new HyperLink();

                    if (style != null && style.IsSet(System.Web.UI.WebControls.Style.PROP_FONT_UNDERLINE))
                        link.Font.Underline = style.Font.Underline;

                    link.EnableTheming = false;
                    link.Enabled = this.Enabled;
                    // VSWhidbey 281869 Don't modify input when url pointing to unc share
                    if (node.Url.StartsWith("\\\\", StringComparison.Ordinal)) {
                        link.NavigateUrl = ResolveClientUrl(HttpUtility.UrlPathEncode(node.Url));
                    }
                    else {
                        link.NavigateUrl = Context != null ?
                            Context.Response.ApplyAppPathModifier(ResolveClientUrl(HttpUtility.UrlPathEncode(node.Url))) : node.Url;
                    }
                    link.Text = HttpUtility.HtmlEncode(node.Title);
                    if (ShowToolTips)
                        link.ToolTip = node.Description;
                    item.Controls.Add(link);
                    link.ApplyStyle(style);
                }
            }
            else {
                template.InstantiateIn(item);
                item.ApplyStyle(style);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Loads a saved state of the <see cref='System.Web.UI.WebControls.SiteMapPath'/>. </para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                Debug.Assert(myState.Length == 5);

                base.LoadViewState(myState[0]);

                if (myState[1] != null)
                    ((IStateManager)CurrentNodeStyle).LoadViewState(myState[1]);

                if (myState[2] != null)
                    ((IStateManager)NodeStyle).LoadViewState(myState[2]);

                if (myState[3] != null)
                    ((IStateManager)RootNodeStyle).LoadViewState(myState[3]);

                if (myState[4] != null)
                    ((IStateManager)PathSeparatorStyle).LoadViewState(myState[4]);
            }
            else {
                base.LoadViewState(null);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='DataBinding'/> event.</para>
        /// </devdoc>
        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);

            // reset the control state
            Controls.Clear();
            ClearChildState();

            CreateControlHierarchy();
            ChildControlsCreated = true;
        }


        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='ItemCreated'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemCreated(SiteMapNodeItemEventArgs e) {
            SiteMapNodeItemEventHandler onItemCreatedHandler =
                (SiteMapNodeItemEventHandler)Events[_eventItemCreated];
            if (onItemCreatedHandler != null) {
                onItemCreatedHandler(this, e);
            }
        }


        /// <devdoc>
        /// <para>A protected method. Raises the <see langword='ItemDataBound'/>
        /// event.</para>
        /// </devdoc>
        protected virtual void OnItemDataBound(SiteMapNodeItemEventArgs e) {
            SiteMapNodeItemEventHandler onItemDataBoundHandler =
                (SiteMapNodeItemEventHandler)Events[_eventItemDataBound];
            if (onItemDataBoundHandler != null) {
                onItemDataBoundHandler(this, e);
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            // Copied from CompositeControl.cs
            if (DesignMode) {
                ChildControlsCreated = false;
                EnsureChildControls();
            }

            base.Render(writer);
        }


        /// <devdoc>
        ///     Adds the SkipToContextText.
        /// </devdoc>

        protected internal override void RenderContents(HtmlTextWriter writer) {
            ControlRenderingHelper.WriteSkipLinkStart(writer, RenderingCompatibility, DesignMode, SkipLinkText, SpacerImageUrl, ClientID);

            base.RenderContents(writer);

            ControlRenderingHelper.WriteSkipLinkEnd(writer, DesignMode, SkipLinkText, ClientID);
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Stores the state of the System.Web.UI.WebControls.SiteMapPath.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            object[] myState = new object[5];

            myState[0] = base.SaveViewState();
            myState[1] = (_currentNodeStyle != null) ? ((IStateManager)_currentNodeStyle).SaveViewState() : null;
            myState[2] = (_nodeStyle != null) ? ((IStateManager)_nodeStyle).SaveViewState() : null;
            myState[3] = (_rootNodeStyle != null) ? ((IStateManager)_rootNodeStyle).SaveViewState() : null;
            myState[4] = (_pathSeparatorStyle != null) ? ((IStateManager)_pathSeparatorStyle).SaveViewState() : null;

            for (int i = 0; i < myState.Length; i++) {
                if (myState[i] != null)
                    return myState;
            }

            return null;
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Marks the starting point to begin tracking and saving changes to the
        ///       control as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_currentNodeStyle != null)
                ((IStateManager)_currentNodeStyle).TrackViewState();

            if (_nodeStyle != null)
                ((IStateManager)_nodeStyle).TrackViewState();

            if (_rootNodeStyle != null)
                ((IStateManager)_rootNodeStyle).TrackViewState();

            if (_pathSeparatorStyle != null)
                ((IStateManager)_pathSeparatorStyle).TrackViewState();
        }
    }
}
