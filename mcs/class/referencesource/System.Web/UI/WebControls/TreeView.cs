//------------------------------------------------------------------------------
// <copyright file="TreeView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    ///     Provides a tree view control
    /// </devdoc>
    [ControlValueProperty("SelectedValue")]
    [DefaultEvent("SelectedNodeChanged")]
    [Designer("System.Web.UI.Design.WebControls.TreeViewDesigner, " + AssemblyRef.SystemDesign)]
    [SupportsEventValidation]
    public class TreeView : HierarchicalDataBoundControl, IPostBackEventHandler, IPostBackDataHandler, ICallbackEventHandler {
        private static string populateNodeScript = @"
    function TreeView_PopulateNodeDoCallBack(context,param) {
        ";
        private static string populateNodeScriptEnd = @";
    }
";

        internal const int RootImageIndex = 0;
        internal const int ParentImageIndex = 1;
        internal const int LeafImageIndex = 2;

        internal const int NoExpandImageIndex = 3;
        internal const int PlusImageIndex = 4;
        internal const int MinusImageIndex = 5;

        internal const int IImageIndex = 6;

        internal const int RImageIndex = 7;
        internal const int RPlusImageIndex = 8;
        internal const int RMinusImageIndex = 9;

        internal const int TImageIndex = 10;
        internal const int TPlusImageIndex = 11;
        internal const int TMinusImageIndex = 12;

        internal const int LImageIndex = 13;
        internal const int LPlusImageIndex = 14;
        internal const int LMinusImageIndex = 15;

        internal const int DashImageIndex = 16;
        internal const int DashPlusImageIndex = 17;
        internal const int DashMinusImageIndex = 18;

        internal const int ImageUrlsCount = 19;

        // Also used by Menu
        internal const char InternalPathSeparator = '\\';
        private const char EscapeCharacter = '|';
        private const string EscapeSequenceForPathSeparator = "*|*";
        private const string EscapeSequenceForEscapeCharacter = "||";

        private string[] _imageUrls;
        private string[] _levelImageUrls;

        private static readonly object CheckChangedEvent = new object();
        private static readonly object SelectedNodeChangedEvent = new object();
        private static readonly object TreeNodeCollapsedEvent = new object();
        private static readonly object TreeNodeExpandedEvent = new object();
        private static readonly object TreeNodePopulateEvent = new object();
        private static readonly object TreeNodeDataBoundEvent = new object();

        private TreeNodeStyle _nodeStyle;
        private TreeNodeStyle _rootNodeStyle;
        private TreeNodeStyle _parentNodeStyle;
        private TreeNodeStyle _leafNodeStyle;
        private TreeNodeStyle _selectedNodeStyle;
        private Style _hoverNodeStyle;
        private HyperLinkStyle _hoverNodeHyperLinkStyle;

        private Style _baseNodeStyle;

        // Cached styles. In the current implementation, the styles are the same for all items
        // and submenus at a given depth.
        private List<TreeNodeStyle> _cachedParentNodeStyles;
        private List<string> _cachedParentNodeClassNames;
        private List<string> _cachedParentNodeHyperLinkClassNames;
        private List<TreeNodeStyle> _cachedLeafNodeStyles;
        private List<string> _cachedLeafNodeClassNames;
        private List<string> _cachedLeafNodeHyperLinkClassNames;
        private Collection<int> _cachedLevelsContainingCssClass;

        private TreeNode _rootNode;
        private TreeNode _selectedNode;

        private TreeNodeCollection _checkedNodes;

        private TreeNodeStyleCollection _levelStyles;

        private ArrayList _checkedChangedNodes;

        private TreeNodeBindingCollection _bindings;

        private int _cssStyleIndex;

        // 
        private bool _loadingNodeState;
        private bool _dataBound;
        private bool _accessKeyRendered;

        private bool _isNotIE;
        private bool _renderClientScript;

        private bool _fireSelectedNodeChanged;

        private string _cachedExpandImageUrl;
        private string _cachedCollapseImageUrl;
        private string _cachedNoExpandImageUrl;
        private string _cachedClientDataObjectID;
        private string _cachedExpandStateID;
        private string _cachedImageArrayID;
        private string _cachedPopulateLogID;
        private string _cachedSelectedNodeFieldID;

        private string _currentSiteMapNodeDataPath;

        private string _callbackEventArgument;

        internal bool AccessKeyRendered {
            get {
                return _accessKeyRendered;
            }
            set {
                _accessKeyRendered = value;
            }
        }

        private List<string> CachedLeafNodeClassNames {
            get {
                if (_cachedLeafNodeClassNames == null) {
                    _cachedLeafNodeClassNames = new List<string>();
                }
                return _cachedLeafNodeClassNames;
            }
        }

        private List<TreeNodeStyle> CachedLeafNodeStyles {
            get {
                if (_cachedLeafNodeStyles == null) {
                    _cachedLeafNodeStyles = new List<TreeNodeStyle>();
                }
                return _cachedLeafNodeStyles;
            }
        }

        private List<string> CachedLeafNodeHyperLinkClassNames {
            get {
                if (_cachedLeafNodeHyperLinkClassNames == null) {
                    _cachedLeafNodeHyperLinkClassNames = new List<string>();
                }
                return _cachedLeafNodeHyperLinkClassNames;
            }
        }

        private Collection<int> CachedLevelsContainingCssClass {
            get {
                if (_cachedLevelsContainingCssClass == null) {
                    _cachedLevelsContainingCssClass = new Collection<int>();
                }
                return _cachedLevelsContainingCssClass;
            }
        }

        private List<TreeNodeStyle> CachedParentNodeStyles {
            get {
                if (_cachedParentNodeStyles == null) {
                    _cachedParentNodeStyles = new List<TreeNodeStyle>();
                }
                return _cachedParentNodeStyles;
            }
        }

        private List<string> CachedParentNodeClassNames {
            get {
                if (_cachedParentNodeClassNames == null) {
                    _cachedParentNodeClassNames = new List<string>();
                }
                return _cachedParentNodeClassNames;
            }
        }

        private List<string> CachedParentNodeHyperLinkClassNames {
            get {
                if (_cachedParentNodeHyperLinkClassNames == null) {
                    _cachedParentNodeHyperLinkClassNames = new List<string>();
                }
                return _cachedParentNodeHyperLinkClassNames;
            }
        }


        /// <devdoc>
        /// Gets and sets whether the tree view will automatically bind to data
        /// </devdoc>
        [
        DefaultValue(true),
        WebCategory("Behavior"),
        WebSysDescription(SR.TreeView_AutoGenerateDataBindings)
        ]
        public bool AutoGenerateDataBindings {
            get {
                object o = ViewState["AutoGenerateDataBindings"];
                if (o == null) {
                    return true;
                }
                return (bool)o;
            }
            set {
                ViewState["AutoGenerateDataBindings"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the tree level data mappings
        /// </devdoc>
        [
        DefaultValue(null),
        MergableProperty(false),
        Editor("System.Web.UI.Design.WebControls.TreeViewBindingsEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.TreeView_DataBindings)
        ]
        public TreeNodeBindingCollection DataBindings {
            get {
                if (_bindings == null) {
                    _bindings = new TreeNodeBindingCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_bindings).TrackViewState();
                    }
                }
                return _bindings;
            }
        }


        /// <devdoc>
        ///     Gets the currently checked nodes in tree
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNodeCollection CheckedNodes {
            get {
                if (_checkedNodes == null) {
                    _checkedNodes = new TreeNodeCollection(null, false);
                }
                return _checkedNodes;
            }
        }

        private ArrayList CheckedChangedNodes {
            get {
                if (_checkedChangedNodes == null) {
                    _checkedChangedNodes = new ArrayList();
                }
                return _checkedChangedNodes;
            }
        }

        /// <devdoc>
        ///     Gets the hidden field ID for the expand state of this TreeView
        /// </devdoc>
        internal string ClientDataObjectID {
            get {
                if (_cachedClientDataObjectID == null) {
                    _cachedClientDataObjectID = ClientID + "_Data";
                }
                return _cachedClientDataObjectID;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image ToolTip for the collapse node icon (minus).
        /// </devdoc>
        [Localizable(true)]
        [WebSysDefaultValue(SR.TreeView_CollapseImageToolTipDefaultValue)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_CollapseImageToolTip)]
        public string CollapseImageToolTip {
            get {
                string s = (string)ViewState["CollapseImageToolTip"];

                if (s == null) {
                    return SR.GetString(SR.TreeView_CollapseImageToolTipDefaultValue);
                }

                return s;
            }
            set {
                ViewState["CollapseImageToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image url for the collapse node icon (minus).
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_CollapseImageUrl)]
        public string CollapseImageUrl {
            get {
                string s = (string)ViewState["CollapseImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["CollapseImageUrl"] = value;
            }
        }

        internal string CollapseImageUrlInternal {
            get {
                if (_cachedCollapseImageUrl == null) {
                    switch (ImageSet) {
                        case TreeViewImageSet.Arrows: {
                                _cachedCollapseImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_Collapse.gif");
                                break;
                            }
                        case TreeViewImageSet.Contacts: {
                                _cachedCollapseImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_Collapse.gif");
                                break;
                            }
                        case TreeViewImageSet.XPFileExplorer: {
                                _cachedCollapseImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_Collapse.gif");
                                break;
                            }
                        case TreeViewImageSet.Msdn: {
                                _cachedCollapseImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_Collapse.gif");
                                break;
                            }
                        case TreeViewImageSet.WindowsHelp: {
                                _cachedCollapseImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_Collapse.gif");
                                break;
                            }
                        case TreeViewImageSet.Custom: {
                                _cachedCollapseImageUrl = CollapseImageUrl;
                                break;
                            }
                        default: {
                                _cachedCollapseImageUrl = String.Empty;
                                break;
                            }
                    }
                }
                return _cachedCollapseImageUrl;
            }
        }

        internal bool CustomExpandCollapseHandlerExists {
            get {
                TreeNodeEventHandler collapseHandler = (TreeNodeEventHandler)Events[TreeNodeCollapsedEvent];
                TreeNodeEventHandler expandHandler = (TreeNodeEventHandler)Events[TreeNodeExpandedEvent];
                return ((collapseHandler != null) || (expandHandler != null));
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the control should try to use client script, if the browser is capable.
        /// </devdoc>
        [DefaultValue(true)]
        [WebCategory("Behavior")]
        [Themeable(false)]
        [WebSysDescription(SR.TreeView_EnableClientScript)]
        public bool EnableClientScript {
            get {
                object o = ViewState["EnableClientScript"];
                if (o == null) {
                    return true;
                }

                return (bool)o;
            }
            set {
                ViewState["EnableClientScript"] = value;
            }
        }

        /// <devdoc>
        ///     Gets whether hover styles have been enabled (set)
        /// </devdoc>
        internal bool EnableHover {
            get {
                return (Page != null &&
                    (Page.SupportsStyleSheets || Page.IsCallback ||
                    (Page.ScriptManager != null && Page.ScriptManager.IsInAsyncPostBack)) &&
                    RenderClientScript &&
                    (_hoverNodeStyle != null));
            }
        }

        [DefaultValue(-1)]
        [TypeConverter(typeof(TreeViewExpandDepthConverter))]
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_ExpandDepth)]
        public int ExpandDepth {
            get {
                object o = ViewState["ExpandDepth"];
                if (o == null) {
                    return -1;
                }
                return (int)o;
            }
            set {
                ViewState["ExpandDepth"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image ToolTip for the Expand node icon (minus).
        /// </devdoc>
        [Localizable(true)]
        [WebSysDefaultValue(SR.TreeView_ExpandImageToolTipDefaultValue)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_ExpandImageToolTip)]
        public string ExpandImageToolTip {
            get {
                string s = (string)ViewState["ExpandImageToolTip"];

                if (s == null) {
                    return SR.GetString(SR.TreeView_ExpandImageToolTipDefaultValue);
                }

                return s;
            }
            set {
                ViewState["ExpandImageToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image url for the expand node icon (plus).
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_ExpandImageUrl)]
        public string ExpandImageUrl {
            get {
                string s = (string)ViewState["ExpandImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["ExpandImageUrl"] = value;
            }
        }

        internal string ExpandImageUrlInternal {
            get {
                if (_cachedExpandImageUrl == null) {
                    switch (ImageSet) {
                        case TreeViewImageSet.Arrows: {
                                _cachedExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_Expand.gif");
                                break;
                            }
                        case TreeViewImageSet.Contacts: {
                                _cachedExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_Expand.gif");
                                break;
                            }
                        case TreeViewImageSet.XPFileExplorer: {
                                _cachedExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_Expand.gif");
                                break;
                            }
                        case TreeViewImageSet.Msdn: {
                                _cachedExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_Expand.gif");
                                break;
                            }
                        case TreeViewImageSet.WindowsHelp: {
                                _cachedExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_Expand.gif");
                                break;
                            }
                        case TreeViewImageSet.Custom: {
                                _cachedExpandImageUrl = ExpandImageUrl;
                                break;
                            }
                        default: {
                                _cachedExpandImageUrl = String.Empty;
                                break;
                            }
                    }
                }
                return _cachedExpandImageUrl;
            }
        }


        /// <devdoc>
        ///     Gets the hidden field ID for the expand state of this TreeView
        /// </devdoc>
        internal string ExpandStateID {
            get {
                if (_cachedExpandStateID == null) {
                    _cachedExpandStateID = ClientID + "_ExpandState";
                }
                return _cachedExpandStateID;
            }
        }


        /// <devdoc>
        ///     Gets the hover style properties for nodes.
        /// </devdoc>
        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.TreeView_HoverNodeStyle)
        ]
        public Style HoverNodeStyle {
            get {
                if (_hoverNodeStyle == null) {
                    _hoverNodeStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_hoverNodeStyle).TrackViewState();
                    }
                }
                return _hoverNodeStyle;
            }
        }

        /// <devdoc>
        /// ID of the client-side array of images (expand, collapse, lines, etc.)
        /// </devdoc>
        internal string ImageArrayID {
            get {
                if (_cachedImageArrayID == null) {
                    _cachedImageArrayID = ClientID + "_ImageArray";
                }
                return _cachedImageArrayID;
            }
        }


        [DefaultValue(TreeViewImageSet.Custom)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_ImageSet)]
        public TreeViewImageSet ImageSet {
            get {
                object o = ViewState["ImageSet"];
                if (o == null) {
                    return TreeViewImageSet.Custom;
                }
                return (TreeViewImageSet)o;
            }
            set {
                if (value < TreeViewImageSet.Custom || value > TreeViewImageSet.Faq) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ImageSet"] = value;
            }
        }


        /// <devdoc>
        ///     An cache of urls for the line and node type images.
        /// </devdoc>
        private string[] ImageUrls {
            get {
                if (_imageUrls == null) {
                    _imageUrls = new string[ImageUrlsCount];
                }
                return _imageUrls;
            }
        }


        /// <devdoc>
        ///     Gets whether the current browser is IE
        /// </devdoc>
        internal bool IsNotIE {
            get {
                return _isNotIE;
            }
        }


        /// <devdoc>
        ///     Gets the style properties of leaf nodes in the tree.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_LeafNodeStyle)
        ]
        public TreeNodeStyle LeafNodeStyle {
            get {
                if (_leafNodeStyle == null) {
                    _leafNodeStyle = new TreeNodeStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_leafNodeStyle).TrackViewState();
                    }
                }
                return _leafNodeStyle;
            }
        }

        private string[] LevelImageUrls {
            get {
                if (_levelImageUrls == null) {
                    _levelImageUrls = new string[LevelStyles.Count];
                }
                return _levelImageUrls;
            }
        }



        /// <devdoc>
        ///     Gets the collection of TreeNodeStyles corresponding to the each level
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.TreeNodeStyleCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.TreeView_LevelStyles),
        ]
        public TreeNodeStyleCollection LevelStyles {
            get {
                if (_levelStyles == null) {
                    _levelStyles = new TreeNodeStyleCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_levelStyles).TrackViewState();
                    }
                }

                return _levelStyles;
            }
        }


        /// <devdoc>
        ///     Gets and sets the url pointing to a folder containing TreeView line images.
        /// </devdoc>
        [DefaultValue("")]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_LineImagesFolderUrl)]
        public string LineImagesFolder {
            get {
                string s = (string)ViewState["LineImagesFolder"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["LineImagesFolder"] = value;
            }
        }


        /// <devdoc>
        ///     True is we are loading the state of a node that has changed on the client, specifically,
        ///     this is used by TreeNode.Expand to check if it needs to trigger a populate or not
        /// </devdoc>
        internal bool LoadingNodeState {
            get {
                return _loadingNodeState;
            }
        }


        /// <devdoc>
        ///     The maximum depth to which the TreeView will bind.
        /// </devdoc>
        [WebCategory("Behavior")]
        [DefaultValue(-1)]
        [WebSysDescription(SR.TreeView_MaxDataBindDepth)]
        public int MaxDataBindDepth {
            get {
                object o = ViewState["MaxDataBindDepth"];
                if (o == null) {
                    return -1;
                }
                return (int)o;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["MaxDataBindDepth"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image url for the non-expandable node indicator icon.
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_NoExpandImageUrl)]
        public string NoExpandImageUrl {
            get {
                string s = (string)ViewState["NoExpandImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["NoExpandImageUrl"] = value;
            }
        }

        internal string NoExpandImageUrlInternal {
            get {
                if (_cachedNoExpandImageUrl == null) {
                    switch (ImageSet) {
                        case TreeViewImageSet.Simple: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Simple_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.Simple2: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Simple2_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.Arrows: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Arrows_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.Contacts: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Contacts_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.XPFileExplorer: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.Msdn: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_MSDN_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.WindowsHelp: {
                                _cachedNoExpandImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Windows_Help_NoExpand.gif");
                                break;
                            }
                        case TreeViewImageSet.Custom: {
                                _cachedNoExpandImageUrl = NoExpandImageUrl;
                                break;
                            }
                        default: {
                                _cachedNoExpandImageUrl = String.Empty;
                                break;
                            }
                    }
                }
                return _cachedNoExpandImageUrl;
            }
        }


        /// <devdoc>
        ///     Gets and sets the indent width of each node
        /// </devdoc>
        [DefaultValue(20)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_NodeIndent)]
        public int NodeIndent {
            get {
                object o = ViewState["NodeIndent"];
                if (o == null) {
                    return 20;
                }
                return (int)o;
            }
            set {
                ViewState["NodeIndent"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the text of the nodes should be wrapped
        /// </devdoc>
        [DefaultValue(false)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_NodeWrap)]
        public bool NodeWrap {
            get {
                object o = ViewState["NodeWrap"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["NodeWrap"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the collection of top-level nodes.
        /// </devdoc>
        [
        DefaultValue(null),
        MergableProperty(false),
        Editor("System.Web.UI.Design.WebControls.TreeNodeCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_Nodes)
        ]
        public TreeNodeCollection Nodes {
            get {
                return RootNode.ChildNodes;
            }
        }



        /// <devdoc>
        ///     Gets the style properties of nodes in the tree.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_NodeStyle)
        ]
        public TreeNodeStyle NodeStyle {
            get {
                if (_nodeStyle == null) {
                    _nodeStyle = new TreeNodeStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_nodeStyle).TrackViewState();
                    }
                }
                return _nodeStyle;
            }
        }


        /// <devdoc>
        ///     Gets the style properties of parent nodes in the tree.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_ParentNodeStyle)
        ]
        public TreeNodeStyle ParentNodeStyle {
            get {
                if (_parentNodeStyle == null) {
                    _parentNodeStyle = new TreeNodeStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_parentNodeStyle).TrackViewState();
                    }
                }
                return _parentNodeStyle;
            }
        }


        /// <devdoc>
        ///     Gets and sets the character used to delimit paths.
        /// </devdoc>
        [DefaultValue('/')]
        [WebSysDescription(SR.TreeView_PathSeparator)]
        public char PathSeparator {
            get {
                object o = ViewState["PathSeparator"];
                if (o == null) {
                    return '/';
                }
                return (char)o;
            }
            set {
                if (value == '\0') {
                    ViewState["PathSeparator"] = null;
                }
                else {
                    ViewState["PathSeparator"] = value;
                }
                foreach (TreeNode node in Nodes) {
                    node.ResetValuePathRecursive();
                }
            }
        }


        /// <devdoc>
        ///     Gets the hidden field ID for the expand state of this TreeView
        /// </devdoc>
        internal string PopulateLogID {
            get {
                if (_cachedPopulateLogID == null) {
                    _cachedPopulateLogID = ClientID + "_PopulateLog";
                }
                return _cachedPopulateLogID;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the tree view should populate nodes from the client (if supported)
        /// </devdoc>
        [DefaultValue(true)]
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_PopulateNodesFromClient)]
        public bool PopulateNodesFromClient {
            get {
                if (!DesignMode &&
                    (Page != null && !Page.Request.Browser.SupportsCallback)) {
                    return false;
                }
                object o = ViewState["PopulateNodesFromClient"];
                if (o == null) {
                    return true;
                }
                return (bool)o;
            }
            set {
                ViewState["PopulateNodesFromClient"] = value;
            }
        }

        /// <devdoc>
        ///     Gets whether we should be rendering client script or not
        /// </devdoc>
        internal bool RenderClientScript {
            get {
                return _renderClientScript;
            }
        }


        /// <devdoc>
        ///     The 'virtual' root node of the tree
        /// </devdoc>
        internal TreeNode RootNode {
            get {
                if (_rootNode == null) {
                    // Using the constructor only here. Other places should use CreateNode.
                    _rootNode = new TreeNode(this, true);
                }
                return _rootNode;
            }
        }

        // BaseTreeNodeStyle is roughly equivalent to ControlStyle.HyperLinkStyle if it existed.
        internal Style BaseTreeNodeStyle {
            get {
                if (_baseNodeStyle == null) {
                    _baseNodeStyle = new Style();
                    _baseNodeStyle.Font.CopyFrom(Font);
                    if (!ForeColor.IsEmpty) {
                        _baseNodeStyle.ForeColor = ForeColor;
                    }
                    // Not defaulting to black anymore for not entirely satisfying but reasonable reasons (VSWhidbey 356729)
                    if (!ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FONT_UNDERLINE)) {
                        _baseNodeStyle.Font.Underline = false;
                    }
                }
                return _baseNodeStyle;
            }
        }


        /// <devdoc>
        ///     Gets the style properties of root nodes in the tree.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_RootNodeStyle)
        ]
        public TreeNodeStyle RootNodeStyle {
            get {
                if (_rootNodeStyle == null) {
                    _rootNodeStyle = new TreeNodeStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_rootNodeStyle).TrackViewState();
                    }
                }
                return _rootNodeStyle;
            }
        }


        /// <devdoc>
        ///     Gets and sets the TreeView's selected node.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public TreeNode SelectedNode {
            get {
                return _selectedNode;
            }
        }


        /// <devdoc>
        ///     Gets the tag ID for hidden field containing the id of the selected node of this TreeView
        /// </devdoc>
        internal string SelectedNodeFieldID {
            get {
                if (_cachedSelectedNodeFieldID == null) {
                    _cachedSelectedNodeFieldID = ClientID + "_SelectedNode";
                }
                return _cachedSelectedNodeFieldID;
            }
        }


        /// <devdoc>
        ///     Gets the style properties of the selected node in the tree.
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.TreeView_SelectedNodeStyle)
        ]
        public TreeNodeStyle SelectedNodeStyle {
            get {
                if (_selectedNodeStyle == null) {
                    _selectedNodeStyle = new TreeNodeStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_selectedNodeStyle).TrackViewState();
                    }
                }
                return _selectedNodeStyle;
            }
        }


        [Browsable(false)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedValue {
            get {
                if (SelectedNode != null) {
                    return SelectedNode.Value;
                }

                return String.Empty;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether to show check boxes next to specific types of nodes in the tree
        /// </devdoc>
        [DefaultValue(TreeNodeTypes.None)]
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_ShowCheckBoxes)]
        public TreeNodeTypes ShowCheckBoxes {
            get {
                object o = ViewState["ShowCheckBoxes"];
                if (o == null) {
                    return TreeNodeTypes.None;
                }
                return (TreeNodeTypes)o;
            }
            set {
                if ((value < TreeNodeTypes.None) || (value > TreeNodeTypes.All)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ShowCheckBoxes"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether to show the expander icon next to nodes in the tree
        /// </devdoc>
        [DefaultValue(true)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_ShowExpandCollapse)]
        public bool ShowExpandCollapse {
            get {
                object o = ViewState["ShowExpandCollapse"];
                if (o == null) {
                    return true;
                }
                return (bool)o;
            }
            set {
                ViewState["ShowExpandCollapse"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the TreeView should show lines.
        /// </devdoc>
        [DefaultValue(false)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeView_ShowLines)]
        public bool ShowLines {
            get {
                object o = ViewState["ShowLines"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["ShowLines"] = value;
            }
        }

        [
        Localizable(true),
        WebCategory("Accessibility"),
        WebSysDefaultValue(SR.TreeView_Default_SkipLinkText),
        WebSysDescription(SR.TreeView_SkipLinkText)
        ]
        public String SkipLinkText {
            get {
                string s = ViewState["SkipLinkText"] as String;
                return s == null ? SR.GetString(SR.TreeView_Default_SkipLinkText) : s;
            }
            set {
                ViewState["SkipLinkText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the target window that the TreeNodes will browse to if selected
        /// </devdoc>
        [DefaultValue("")]
        [WebSysDescription(SR.TreeNode_Target)]
        public string Target {
            get {
                string s = (string)ViewState["Target"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["Target"] = value;
            }
        }


        protected override HtmlTextWriterTag TagKey {
            get {
                return DesignMode ? HtmlTextWriterTag.Table : HtmlTextWriterTag.Div;
            }
        }

        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                // Remember that the tree was initially invisible and thus never expanded (VSWhidbey 349279)
                // See SaveViewState to see the code that sets this flag.
                if ((value == true) && (Page != null) && Page.IsPostBack &&
                    (ViewState["NeverExpanded"] != null) &&
                    ((bool)ViewState["NeverExpanded"] == true)) {

                    // This will reset the viewstate flag and expand the tree
                    ExpandToDepth(Nodes, ExpandDepth);
                }
                base.Visible = value;
            }
        }

        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_CheckChanged)]
        public event TreeNodeEventHandler TreeNodeCheckChanged {
            add {
                Events.AddHandler(CheckChangedEvent, value);
            }
            remove {
                Events.RemoveHandler(CheckChangedEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when the TreeView's selected node has changed.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_SelectedNodeChanged)]
        public event EventHandler SelectedNodeChanged {
            add {
                Events.AddHandler(SelectedNodeChangedEvent, value);
            }
            remove {
                Events.RemoveHandler(SelectedNodeChangedEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when a TreeNode has collapsed its children.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_TreeNodeCollapsed)]
        public event TreeNodeEventHandler TreeNodeCollapsed {
            add {
                Events.AddHandler(TreeNodeCollapsedEvent, value);
            }
            remove {
                Events.RemoveHandler(TreeNodeCollapsedEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when a TreeNode has been databound.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_TreeNodeDataBound)]
        public event TreeNodeEventHandler TreeNodeDataBound {
            add {
                Events.AddHandler(TreeNodeDataBoundEvent, value);
            }
            remove {
                Events.RemoveHandler(TreeNodeDataBoundEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when a TreeNode has expanded its children.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_TreeNodeExpanded)]
        public event TreeNodeEventHandler TreeNodeExpanded {
            add {
                Events.AddHandler(TreeNodeExpandedEvent, value);
            }
            remove {
                Events.RemoveHandler(TreeNodeExpandedEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when a TreeNode is populating its children.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.TreeView_TreeNodePopulate)]
        public event TreeNodeEventHandler TreeNodePopulate {
            add {
                Events.AddHandler(TreeNodePopulateEvent, value);
            }
            remove {
                Events.RemoveHandler(TreeNodePopulateEvent, value);
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer) {

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            string oldAccessKey = AccessKey;
            if (!String.IsNullOrEmpty(oldAccessKey)) {
                AccessKey = String.Empty;
                base.AddAttributesToRender(writer);
                AccessKey = oldAccessKey;
            }
            else {
                base.AddAttributesToRender(writer);
            }
        }

        // returns true if the style contains a class name
        private static bool AppendCssClassName(StringBuilder builder, TreeNodeStyle style, bool hyperlink) {
            bool containsClassName = false;
            if (style != null) {
                // We have to merge with any CssClass specified on the Style itself
                if (style.CssClass.Length != 0) {
                    builder.Append(style.CssClass);
                    builder.Append(' ');
                    containsClassName = true;
                }

                string className = (hyperlink ?
                    style.HyperLinkStyle.RegisteredCssClass :
                    style.RegisteredCssClass);
                if (className.Length > 0) {
                    builder.Append(className);
                    builder.Append(' ');
                }
            }
            return containsClassName;
        }

        private static T CacheGetItem<T>(List<T> cacheList, int index) where T : class {
            Debug.Assert(cacheList != null);
            if (index < cacheList.Count) return cacheList[index];
            return null;
        }

        private static void CacheSetItem<T>(List<T> cacheList, int index, T item) where T : class {
            if (cacheList.Count > index) {
                cacheList[index] = item;
            }
            else {
                for (int i = cacheList.Count; i < index; i++) {
                    cacheList.Add(null);
                }
                cacheList.Add(item);
            }
        }


        /// <devdoc>
        ///     Fully collapses all nodes in the tree
        /// </devdoc>
        public void CollapseAll() {
            foreach (TreeNode node in Nodes) {
                node.CollapseAll();
            }
        }


        /// <devdoc>
        ///     Overridden to disallow adding controls
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        protected virtual internal TreeNode CreateNode() {
            return new TreeNode(this, false);
        }


        /// <devdoc>
        ///     Creates a tree node ID based on an index
        /// </devdoc>
        internal string CreateNodeId(int index) {
            return ClientID + "n" + index;
        }


        /// <devdoc>
        ///     Creates a tree node text ID based on an index
        /// </devdoc>
        internal string CreateNodeTextId(int index) {
            return ClientID + "t" + index;
        }

        /// Data bound controls should override PerformDataBinding instead
        /// of DataBind.  If DataBind if overridden, the OnDataBinding and OnDataBound events will
        /// fire in the wrong order.  However, for backwards compat on ListControl and AdRotator, we 
        /// can't seal this method.  It is sealed on all new BaseDataBoundControl-derived controls.
        public override sealed void DataBind() {
            base.DataBind();
        }


        /// <devdoc>
        ///     Databinds the specified node to the datasource
        /// </devdoc>
        private void DataBindNode(TreeNode node) {
            if (node.PopulateOnDemand && !IsBoundUsingDataSourceID && !DesignMode) {
                throw new InvalidOperationException(SR.GetString(SR.TreeView_PopulateOnlyForDataSourceControls, ID));
            }

            HierarchicalDataSourceView view = GetData(node.DataPath);
            // Do nothing if no datasource was set
            if (!IsBoundUsingDataSourceID && (DataSource == null)) {
                return;
            }

            if (view == null) {
                throw new InvalidOperationException(SR.GetString(SR.TreeView_DataSourceReturnedNullView, ID));
            }
            IHierarchicalEnumerable enumerable = view.Select();
            node.ChildNodes.Clear();
            if (enumerable != null) {
                // If we're bound to a SiteMapDataSource, automatically select the node
                if (IsBoundUsingDataSourceID) {
                    SiteMapDataSource siteMapDataSource = GetDataSource() as SiteMapDataSource;
                    if (siteMapDataSource != null) {
                        if (_currentSiteMapNodeDataPath == null) {
                            IHierarchyData currentNodeData = (IHierarchyData)siteMapDataSource.Provider.CurrentNode;
                            if (currentNodeData != null) {
                                _currentSiteMapNodeDataPath = currentNodeData.Path;
                            }
                            else {
                                _currentSiteMapNodeDataPath = String.Empty;
                            }
                        }
                    }
                }

                DataBindRecursive(node, enumerable, true);
            }
        }


        /// <devdoc>
        ///     Databinds recursively, using the TreeView's Bindings collection, until it reaches a TreeNodeBinding
        ///     that is PopulateOnDemand or there is no more data.  Optionally ignores the first level's PopulateOnDemand
        ///     to facilitate populating that level
        /// </devdoc>
        private void DataBindRecursive(TreeNode node, IHierarchicalEnumerable enumerable, bool ignorePopulateOnDemand) {
            // Since we are binding children, get the level below the current node's depth
            int depth = checked(node.Depth + 1);

            // Don't databind beyond the maximum specified depth
            if ((MaxDataBindDepth != -1) && (depth > MaxDataBindDepth)) {
                return;
            }

            foreach (object item in enumerable) {
                IHierarchyData data = enumerable.GetHierarchyData(item);

                string text = null;
                string value = null;
                string navigateUrl = String.Empty;
                string imageUrl = String.Empty;
                string target = String.Empty;

                string toolTip = String.Empty;
                string imageToolTip = String.Empty;
                TreeNodeSelectAction selectAction = TreeNodeSelectAction.Select;
                bool? showCheckBox = null;

                string dataMember = String.Empty;
                bool populateOnDemand = false;

                dataMember = data.Type;

                TreeNodeBinding level = DataBindings.GetBinding(dataMember, depth);

                if (level != null) {
                    populateOnDemand = level.PopulateOnDemand;

                    PropertyDescriptorCollection props = TypeDescriptor.GetProperties(item);

                    // Bind Text, using the static value if necessary
                    string textField = level.TextField;
                    if (textField.Length > 0) {
                        PropertyDescriptor desc = props.Find(textField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                if (!String.IsNullOrEmpty(level.FormatString)) {
                                    text = string.Format(CultureInfo.CurrentCulture, level.FormatString, objData);
                                }
                                else {
                                    text = objData.ToString();
                                }
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, textField, "TextField"));
                        }
                    }

                    if (String.IsNullOrEmpty(text)) {
                        text = level.Text;
                    }

                    // Bind Value, using the static value if necessary
                    string valueField = level.ValueField;
                    if (valueField.Length > 0) {
                        PropertyDescriptor desc = props.Find(valueField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                value = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, valueField, "ValueField"));
                        }
                    }

                    if (String.IsNullOrEmpty(value)) {
                        value = level.Value;
                    }

                    // Bind ImageUrl, using the static value if necessary
                    string imageUrlField = level.ImageUrlField;
                    if (imageUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(imageUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                imageUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, imageUrlField, "ImageUrlField"));
                        }
                    }

                    if (imageUrl.Length == 0) {
                        imageUrl = level.ImageUrl;
                    }

                    // Bind NavigateUrl, using the static value if necessary
                    string navigateUrlField = level.NavigateUrlField;
                    if (navigateUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(navigateUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                navigateUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, navigateUrlField, "NavigateUrlField"));
                        }
                    }

                    if (navigateUrl.Length == 0) {
                        navigateUrl = level.NavigateUrl;
                    }

                    // Bind Target, using the static value if necessary
                    string targetField = level.TargetField;
                    if (targetField.Length > 0) {
                        PropertyDescriptor desc = props.Find(targetField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                target = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, targetField, "TargetField"));
                        }
                    }

                    if (String.IsNullOrEmpty(target)) {
                        target = level.Target;
                    }

                    // Bind ToolTip, using the static value if necessary
                    string toolTipField = level.ToolTipField;
                    if (toolTipField.Length > 0) {
                        PropertyDescriptor desc = props.Find(toolTipField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                toolTip = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, toolTipField, "ToolTipField"));
                        }
                    }

                    if (toolTip.Length == 0) {
                        toolTip = level.ToolTip;
                    }

                    // Bind ImageToolTip, using the static value if necessary
                    string imageToolTipField = level.ImageToolTipField;

                    if (imageToolTipField.Length > 0) {
                        PropertyDescriptor desc = props.Find(imageToolTipField, true);

                        if (desc != null) {
                            object objData = desc.GetValue(item);

                            if (objData != null) {
                                imageToolTip = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.TreeView_InvalidDataBinding, imageToolTipField, "imageToolTipField"));
                        }
                    }

                    if (imageToolTip.Length == 0) {
                        imageToolTip = level.ImageToolTip;
                    }

                    // Set the other static properties
                    selectAction = level.SelectAction;
                    showCheckBox = level.ShowCheckBox;
                }
                else {
                    if (item is INavigateUIData) {
                        INavigateUIData navigateUIData = (INavigateUIData)item;
                        text = navigateUIData.Name;
                        value = navigateUIData.Value;
                        navigateUrl = navigateUIData.NavigateUrl;
                        if (String.IsNullOrEmpty(navigateUrl)) {
                            selectAction = TreeNodeSelectAction.None;
                        }
                        toolTip = navigateUIData.Description;
                    }
                    if (IsBoundUsingDataSourceID) {
                        populateOnDemand = PopulateNodesFromClient;
                    }
                }

                if (AutoGenerateDataBindings && (text == null)) {
                    text = item.ToString();
                }

                TreeNode newNode = null;
                // Allow String.Empty for the text, but not null
                if ((text != null) || (value != null)) {
                    newNode = CreateNode();
                    if (!String.IsNullOrEmpty(text)) {
                        newNode.Text = text;
                    }
                    if (!String.IsNullOrEmpty(value)) {
                        newNode.Value = value;
                    }
                    if (!String.IsNullOrEmpty(imageUrl)) {
                        newNode.ImageUrl = imageUrl;
                    }
                    if (!String.IsNullOrEmpty(navigateUrl)) {
                        newNode.NavigateUrl = navigateUrl;
                    }
                    if (!String.IsNullOrEmpty(target)) {
                        newNode.Target = target;
                    }
                }

                if (newNode != null) {
                    if (!String.IsNullOrEmpty(toolTip)) {
                        newNode.ToolTip = toolTip;
                    }

                    if (!String.IsNullOrEmpty(imageToolTip)) {
                        newNode.ImageToolTip = imageToolTip;
                    }

                    if (selectAction != newNode.SelectAction) {
                        newNode.SelectAction = selectAction;
                    }

                    if (showCheckBox != null) {
                        newNode.ShowCheckBox = showCheckBox;
                    }

                    newNode.SetDataPath(data.Path);
                    newNode.SetDataBound(true);

                    node.ChildNodes.Add(newNode);

                    if (String.Equals(data.Path, _currentSiteMapNodeDataPath, StringComparison.OrdinalIgnoreCase)) {
                        newNode.Selected = true;

                        // Make sure the newly selected node's parents are expanded
                        if ((Page == null) || !Page.IsCallback) {
                            TreeNode newNodeParent = newNode.Parent;
                            while (newNodeParent != null) {
                                if (newNodeParent.Expanded != true) {
                                    newNodeParent.Expanded = true;
                                }

                                newNodeParent = newNodeParent.Parent;
                            }
                        }
                    }

                    // Make sure we call user code if they've hooked the populate event
                    newNode.SetDataItem(data.Item);
                    OnTreeNodeDataBound(new TreeNodeEventArgs(newNode));
                    newNode.SetDataItem(null);

                    if ((data.HasChildren) && ((MaxDataBindDepth == -1) || (depth < MaxDataBindDepth))) {
                        if (populateOnDemand && !DesignMode) {
                            newNode.PopulateOnDemand = true;
                        }
                        else {
                            IHierarchicalEnumerable newEnumerable = data.GetChildren();
                            if (newEnumerable != null) {
                                DataBindRecursive(newNode, newEnumerable, false);
                            }
                        }
                    }
                }
            }
        }

        /// <devdoc>
        ///     Make sure we are set up to render
        /// </devdoc>
        private void EnsureRenderSettings() {
            HttpBrowserCapabilities caps = Page.Request.Browser;
            _isNotIE = (Page.Request.Browser.MSDomVersion.Major < 4);
            _renderClientScript = GetRenderClientScript(caps);

            if (_hoverNodeStyle != null && Page != null && Page.Header == null) {
                throw new InvalidOperationException(SR.GetString(SR.NeedHeader, "TreeView.HoverStyle"));
            }

            if (Page != null && (Page.SupportsStyleSheets ||
                Page.IsCallback || (Page.ScriptManager != null && Page.ScriptManager.IsInAsyncPostBack))) {
                // Register the styles. NB the order here is important: later wins over earlier
                RegisterStyle(BaseTreeNodeStyle);

                // It's also vitally important to register hyperlinkstyles BEFORE
                // their associated styles as we need to copy the data from this style
                // and a registered style appears empty except for RegisteredClassName
                if (_nodeStyle != null) {
                    _nodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_nodeStyle.HyperLinkStyle);
                    RegisterStyle(_nodeStyle);
                }

                if (_rootNodeStyle != null) {
                    _rootNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_rootNodeStyle.HyperLinkStyle);
                    RegisterStyle(_rootNodeStyle);
                }

                if (_parentNodeStyle != null) {
                    _parentNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_parentNodeStyle.HyperLinkStyle);
                    RegisterStyle(_parentNodeStyle);
                }

                if (_leafNodeStyle != null) {
                    _leafNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_leafNodeStyle.HyperLinkStyle);
                    RegisterStyle(_leafNodeStyle);
                }

                foreach (TreeNodeStyle style in LevelStyles) {
                    style.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(style.HyperLinkStyle);
                    RegisterStyle(style);
                }

                if (_selectedNodeStyle != null) {
                    _selectedNodeStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_selectedNodeStyle.HyperLinkStyle);
                    RegisterStyle(_selectedNodeStyle);
                }

                if (_hoverNodeStyle != null) {
                    _hoverNodeHyperLinkStyle = new HyperLinkStyle(_hoverNodeStyle);
                    _hoverNodeHyperLinkStyle.DoNotRenderDefaults = true;
                    RegisterStyle(_hoverNodeHyperLinkStyle);
                    RegisterStyle(_hoverNodeStyle);
                }
            }
        }


        /// <devdoc>
        ///     Fully expands all nodes in the tree
        /// </devdoc>
        public void ExpandAll() {
            foreach (TreeNode node in Nodes) {
                node.ExpandAll();
            }
        }

        private void ExpandToDepth(TreeNodeCollection nodes, int depth) {
            // Reset the memory that the tree was never expanded (VSWhidbey 349279)
            ViewState["NeverExpanded"] = null;

            foreach (TreeNode node in nodes) {
                if ((depth == -1) || (node.Depth < depth)) {
                    // Only expanding nodes that have not been set, not those that have explicit Expanded=False.
                    if (node.Expanded == null) {
                        node.Expanded = true;
                        // No need to populate as setting Expanded to true already does the job.
                    }
                    ExpandToDepth(node.ChildNodes, depth);
                }
            }
        }


        public TreeNode FindNode(string valuePath) {
            if (valuePath == null) {
                return null;
            }
            return Nodes.FindNode(valuePath.Split(PathSeparator), 0);
        }

        internal string GetCssClassName(TreeNode node, bool hyperLink) {
            bool discarded;
            return GetCssClassName(node, hyperLink, out discarded);
        }

        internal string GetCssClassName(TreeNode node, bool hyperLink, out bool containsClassName) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            containsClassName = false;
            int depth = node.Depth;
            bool parent = node.ChildNodes.Count != 0 || node.PopulateOnDemand;
            List<string> cache = parent ?
                (hyperLink ? CachedParentNodeHyperLinkClassNames : CachedParentNodeClassNames) :
                (hyperLink ? CachedLeafNodeHyperLinkClassNames : CachedLeafNodeClassNames);
            string baseClassName = CacheGetItem<string>(cache, depth);
            if (CachedLevelsContainingCssClass.Contains(depth)) {
                containsClassName = true;
            }

            bool needsSelectedStyle = node.Selected && _selectedNodeStyle != null;

            if (!needsSelectedStyle && (baseClassName != null)) {
                return baseClassName;
            }

            StringBuilder builder = new StringBuilder();
            if (baseClassName != null) {
                builder.Append(baseClassName);
                builder.Append(' ');
            }
            else {
                // No cached style, so build it
                if (hyperLink) {
                    builder.Append(BaseTreeNodeStyle.RegisteredCssClass);
                    builder.Append(' ');
                }

                containsClassName |= AppendCssClassName(builder, _nodeStyle, hyperLink);

                if (depth < LevelStyles.Count && LevelStyles[depth] != null) {
                    containsClassName |= AppendCssClassName(builder, (TreeNodeStyle)LevelStyles[depth], hyperLink);
                }
                if (depth == 0 && parent) {
                    containsClassName |= AppendCssClassName(builder, _rootNodeStyle, hyperLink);
                }
                else if (parent) {
                    containsClassName |= AppendCssClassName(builder, _parentNodeStyle, hyperLink);
                }
                else {
                    containsClassName |= AppendCssClassName(builder, _leafNodeStyle, hyperLink);
                }

                baseClassName = builder.ToString().Trim();
                CacheSetItem<string>(cache, depth, baseClassName);
                if (containsClassName && !CachedLevelsContainingCssClass.Contains(depth)) {
                    CachedLevelsContainingCssClass.Add(depth);
                }
            }

            if (needsSelectedStyle) {
                containsClassName |= AppendCssClassName(builder, _selectedNodeStyle, hyperLink);
                return builder.ToString().Trim(); ;
            }
            return baseClassName;
        }


        /// <devdoc>
        ///     Gets the URL for the specified image, properly pathing the image filename depending on which image it is
        /// </devdoc>
        internal string GetImageUrl(int index) {
            if (ImageUrls[index] == null) {
                switch (index) {
                    case RootImageIndex:
                        string rootNodeImageUrl = RootNodeStyle.ImageUrl;
                        if (rootNodeImageUrl.Length == 0) {
                            rootNodeImageUrl = NodeStyle.ImageUrl;
                        }
                        if (rootNodeImageUrl.Length == 0) {
                            switch (ImageSet) {
                                case TreeViewImageSet.BulletedList: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList2: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList3: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList4: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.News: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Inbox: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Events: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Faq: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_RootNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.XPFileExplorer: {
                                        rootNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_RootNode.gif");
                                        break;
                                    }
                            }
                        }

                        if (rootNodeImageUrl.Length != 0) {
                            rootNodeImageUrl = ResolveClientUrl(rootNodeImageUrl);
                        }
                        ImageUrls[index] = rootNodeImageUrl;
                        break;
                    case ParentImageIndex:
                        string parentNodeImageUrl = ParentNodeStyle.ImageUrl;
                        if (parentNodeImageUrl.Length == 0) {
                            parentNodeImageUrl = NodeStyle.ImageUrl;
                        }
                        if (parentNodeImageUrl.Length == 0) {
                            switch (ImageSet) {
                                case TreeViewImageSet.BulletedList: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList2: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList3: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList4: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.News: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Inbox: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Events: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Faq: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_ParentNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.XPFileExplorer: {
                                        parentNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_ParentNode.gif");
                                        break;
                                    }
                            }
                        }


                        if (parentNodeImageUrl.Length != 0) {
                            parentNodeImageUrl = ResolveClientUrl(parentNodeImageUrl);
                        }
                        ImageUrls[index] = parentNodeImageUrl;
                        break;
                    case LeafImageIndex:
                        string leafNodeImageUrl = LeafNodeStyle.ImageUrl;
                        if (leafNodeImageUrl.Length == 0) {
                            leafNodeImageUrl = NodeStyle.ImageUrl;
                        }
                        if (leafNodeImageUrl.Length == 0) {
                            switch (ImageSet) {
                                case TreeViewImageSet.BulletedList: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList2: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList2_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList3: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList3_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.BulletedList4: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_BulletedList4_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.News: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_News_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Inbox: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Inbox_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Events: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Events_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.Faq: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_FAQ_LeafNode.gif");
                                        break;
                                    }
                                case TreeViewImageSet.XPFileExplorer: {
                                        leafNodeImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_XP_Explorer_LeafNode.gif");
                                        break;
                                    }
                            }
                        }

                        if (leafNodeImageUrl.Length != 0) {
                            leafNodeImageUrl = ResolveClientUrl(leafNodeImageUrl);
                        }
                        ImageUrls[index] = leafNodeImageUrl;
                        break;
                    case NoExpandImageIndex:
                        if (ShowLines) {
                            if (LineImagesFolder.Length == 0) {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_NoExpand.gif");
                            }
                            else {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "noexpand.gif"));
                            }
                        }
                        else {
                            if (NoExpandImageUrlInternal.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(NoExpandImageUrlInternal);
                            }
                            else if (LineImagesFolder.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "noexpand.gif"));
                            }
                            else {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_NoExpand.gif");
                            }
                        }
                        break;

                    case PlusImageIndex:
                        if (ShowLines) {
                            if (LineImagesFolder.Length == 0) {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Expand.gif");
                            }
                            else {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "plus.gif"));
                            }
                        }
                        else {
                            if (ExpandImageUrlInternal.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(ExpandImageUrlInternal);
                            }
                            else if (LineImagesFolder.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "plus.gif"));
                            }
                            else {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Expand.gif");
                            }
                        }
                        break;
                    case MinusImageIndex:
                        if (ShowLines) {
                            if (LineImagesFolder.Length == 0) {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Collapse.gif");
                            }
                            else {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "minus.gif"));
                            }
                        }
                        else {
                            if (CollapseImageUrlInternal.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(CollapseImageUrlInternal);
                            }
                            else if (LineImagesFolder.Length > 0) {
                                ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "minus.gif"));
                            }
                            else {
                                ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Collapse.gif");
                            }
                        }
                        break;
                    case IImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_I.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "i.gif"));
                        }
                        break;
                    case RImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_R.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "r.gif"));
                        }
                        break;
                    case RPlusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_RExpand.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "rplus.gif"));
                        }
                        break;
                    case RMinusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_RCollapse.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "rminus.gif"));
                        }
                        break;
                    case TImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_T.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "t.gif"));
                        }
                        break;
                    case TPlusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_TExpand.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "tplus.gif"));
                        }
                        break;
                    case TMinusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_TCollapse.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "tminus.gif"));
                        }
                        break;
                    case LImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_L.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "l.gif"));
                        }
                        break;
                    case LPlusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_LExpand.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "lplus.gif"));
                        }
                        break;
                    case LMinusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_LCollapse.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "lminus.gif"));
                        }
                        break;
                    case DashImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_Dash.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "dash.gif"));
                        }
                        break;
                    case DashPlusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_DashExpand.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "dashplus.gif"));
                        }
                        break;
                    case DashMinusImageIndex:
                        if (LineImagesFolder.Length == 0) {
                            ImageUrls[index] = Page.ClientScript.GetWebResourceUrl(typeof(TreeView), "TreeView_Default_DashCollapse.gif");
                        }
                        else {
                            ImageUrls[index] = ResolveClientUrl(UrlPath.SimpleCombine(LineImagesFolder, "dashminus.gif"));
                        }
                        break;
                }
            }

            return ImageUrls[index];
        }

        internal string GetLevelImageUrl(int index) {
            if (LevelImageUrls[index] == null) {
                string imageUrl = ((TreeNodeStyle)LevelStyles[index]).ImageUrl;
                if (imageUrl.Length > 0) {
                    LevelImageUrls[index] = ResolveClientUrl(imageUrl);
                }
                else {
                    LevelImageUrls[index] = String.Empty;
                }
            }

            return LevelImageUrls[index];
        }

        // After calling this, style1 has a merged class name,
        // and all properties explicitly set on style2 replace those on style1.
        // Also used by Menu
        internal static void GetMergedStyle(Style style1, Style style2) {
            string oldClass = style1.CssClass;
            style1.CopyFrom(style2);
            if (oldClass.Length != 0 && style2.CssClass.Length != 0) {
                style1.CssClass += ' ' + oldClass;
            }
        }

        private bool GetRenderClientScript(HttpBrowserCapabilities caps) {
            return (EnableClientScript &&
                    Enabled &&
                    (caps.EcmaScriptVersion.Major > 0) &&
                    (caps.W3CDomVersion.Major > 0) &&
                     !StringUtil.EqualsIgnoreCase(caps["tagwriter"], typeof(Html32TextWriter).FullName));
        }

        internal TreeNodeStyle GetStyle(TreeNode node) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            bool parent = node.ChildNodes.Count != 0 || node.PopulateOnDemand;
            List<TreeNodeStyle> cache = parent ? CachedParentNodeStyles : CachedLeafNodeStyles;
            bool needsSelectedStyle = node.Selected && _selectedNodeStyle != null;

            int depth = node.Depth;
            TreeNodeStyle typedStyle = CacheGetItem<TreeNodeStyle>(cache, depth);

            if (!needsSelectedStyle && typedStyle != null) return typedStyle;

            if (typedStyle == null) {
                typedStyle = new TreeNodeStyle();
                typedStyle.CopyFrom(BaseTreeNodeStyle);

                if (_nodeStyle != null) {
                    GetMergedStyle(typedStyle, _nodeStyle);
                }

                if (depth == 0 && parent) {
                    if (_rootNodeStyle != null) {
                        GetMergedStyle(typedStyle, _rootNodeStyle);
                    }
                }
                else if (parent) {
                    if (_parentNodeStyle != null) {
                        GetMergedStyle(typedStyle, _parentNodeStyle);
                    }
                }
                else if (_leafNodeStyle != null) {
                    GetMergedStyle(typedStyle, _leafNodeStyle);
                }

                if (depth < LevelStyles.Count && LevelStyles[depth] != null) {
                    GetMergedStyle(typedStyle, LevelStyles[depth]);
                }

                CacheSetItem<TreeNodeStyle>(cache, depth, typedStyle);
            }


            if (needsSelectedStyle) {
                TreeNodeStyle selectedStyle = new TreeNodeStyle();
                selectedStyle.CopyFrom(typedStyle);
                GetMergedStyle(selectedStyle, _selectedNodeStyle);
                return selectedStyle;
            }
            return typedStyle;
        }

        private int GetTrailingIndex(string s) {
            int i = s.Length - 1;
            while (i > 0) {
                if (!Char.IsDigit(s[i])) {
                    break;
                }
                i--;
            }

            if ((i > -1) && (i < (s.Length - 1)) && ((s.Length - i) < 11)) {
                return Int32.Parse(s.Substring(i + 1), CultureInfo.InvariantCulture);
            }

            return -1;
        }

        internal static string Escape(string value) {
            // This function escapes \ and | to avoid collisions with the internal path separator.
            // Also used by Menu
            StringBuilder b = null;

            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }

            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++) {
                switch (value[i]) {
                    case InternalPathSeparator:
                        if (b == null) {
                            b = new StringBuilder(value.Length + 5);
                        }

                        if (count > 0) {
                            b.Append(value, startIndex, count);
                        }

                        b.Append(EscapeSequenceForPathSeparator);

                        startIndex = i + 1;
                        count = 0;
                        break;
                    case EscapeCharacter:
                        if (b == null) {
                            b = new StringBuilder(value.Length + 5);
                        }

                        if (count > 0) {
                            b.Append(value, startIndex, count);
                        }

                        b.Append(EscapeSequenceForEscapeCharacter);

                        startIndex = i + 1;
                        count = 0;
                        break;
                    default:
                        count++;
                        break;
                }
            }

            if (b == null) {
                return value;
            }

            if (count > 0) {
                b.Append(value, startIndex, count);
            }

            return b.ToString();
        }

        internal static string UnEscape(string value) {
            // Also used by Menu
            return value.Replace(
                EscapeSequenceForPathSeparator, InternalPathSeparator.ToString()).Replace(
                EscapeSequenceForEscapeCharacter, EscapeCharacter.ToString());
        }

        /// <devdoc>
        ///     Loads a nodes state from the postback data.  Basically, there are expand state (which may have changed on the client) and
        ///     check state.  It also fills a dictionary of nodes that were populated on the client (and need to be populated on the server).
        /// </devdoc>
        private void LoadNodeState(TreeNode node, ref int index, string expandState, IDictionary populatedNodes, int selectedNodeIndex) {
            // Recursive method - prevent stack overflow.
            RuntimeHelpers.EnsureSufficientExecutionStack();

            // If our populatedNodes dictionary contains the index for the current node, that means
            // it was populated on the client-side and needs to have it's child node states also updated
            if (PopulateNodesFromClient && (populatedNodes != null)) {
                if (populatedNodes.Contains(index)) {
                    populatedNodes[index] = node;
                }
            }

            // If nothing was posted, selectedNodeIndex will be -1
            if (selectedNodeIndex != -1) {
                // When something was posted, update to the new selected node
                if (node.Selected && (index != selectedNodeIndex)) {
                    node.Selected = false;
                }

                if ((index == selectedNodeIndex) &&
                    ((node.SelectAction == TreeNodeSelectAction.Select) ||
                    (node.SelectAction == TreeNodeSelectAction.SelectExpand))) {

                    bool oldSelected = node.Selected;

                    node.Selected = true;

                    if (!oldSelected) {
                        _fireSelectedNodeChanged = true;
                    }
                }
            }
            else if (node.Selected) {
                // Otherwise, just reselect the old selected node
                SetSelectedNode(node);
            }

            // Check if the node's checked state has changed since the last postback
            // But only if the node has checkbox UI (VSWhidbey 421233)
            if (node.GetEffectiveShowCheckBox()) {
                bool originalChecked = node.Checked;
                string checkBoxFieldID = CreateNodeId(index) + "CheckBox";
                if ((Context.Request.Form[checkBoxFieldID] != null) ||
                    (Context.Request.QueryString[checkBoxFieldID] != null)) {
                    if (!node.Checked) {
                        node.Checked = true;
                    }
                    if (originalChecked != node.Checked) {
                        CheckedChangedNodes.Add(node);
                    }
                }
                else {
                    if (originalChecked && !node.PreserveChecked) {
                        if (node.Checked) {
                            node.Checked = false;
                        }
                    }

                    if (originalChecked != node.Checked) {
                        CheckedChangedNodes.Add(node);
                    }
                }
            }

            // Get the client-side expand state of the current node
            if ((Page != null) && (Page.RequestInternal != null) &&
                (expandState != null) &&
                (expandState.Length > index) &&
                (ShowExpandCollapse ||
                (node.SelectAction == TreeNodeSelectAction.Expand) ||
                (node.SelectAction == TreeNodeSelectAction.SelectExpand))) {

                char c = expandState[index];
                switch (c) {
                    case 'e':
                        node.Expanded = true;
                        break;
                    case 'c':
                        node.Expanded = false;
                        break;
                    //case 'n': case 'u':
                    //    break;
                }
            }

            index++;

            // If there were children for this node, load their states too
            TreeNodeCollection nodes = node.ChildNodes;
            if (nodes.Count > 0) {
                for (int i = 0; i < nodes.Count; i++) {
                    LoadNodeState(nodes[i], ref index, expandState, populatedNodes, selectedNodeIndex);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Loads a saved state of the <see cref='System.Web.UI.WebControls.TreeView'/>.
        /// </devdoc>
        protected override void LoadViewState(object state) {
            if (state != null) {
                object[] savedState = (object[])state;

                if (savedState[0] != null) {
                    base.LoadViewState(savedState[0]);
                }

                if (savedState[1] != null) {
                    ((IStateManager)NodeStyle).LoadViewState(savedState[1]);
                }

                if (savedState[2] != null) {
                    ((IStateManager)RootNodeStyle).LoadViewState(savedState[2]);
                }

                if (savedState[3] != null) {
                    ((IStateManager)ParentNodeStyle).LoadViewState(savedState[3]);
                }

                if (savedState[4] != null) {
                    ((IStateManager)LeafNodeStyle).LoadViewState(savedState[4]);
                }

                if (savedState[5] != null) {
                    ((IStateManager)SelectedNodeStyle).LoadViewState(savedState[5]);
                }

                if (savedState[6] != null) {
                    ((IStateManager)HoverNodeStyle).LoadViewState(savedState[6]);
                }

                if (savedState[7] != null) {
                    ((IStateManager)LevelStyles).LoadViewState(savedState[7]);
                }

                if (savedState[8] != null) {
                    ((IStateManager)Nodes).LoadViewState(savedState[8]);
                }
            }
        }

        protected internal override void OnInit(EventArgs e) {
            ChildControlsCreated = true;
            base.OnInit(e);
        }

        protected virtual void OnTreeNodeCheckChanged(TreeNodeEventArgs e) {
            TreeNodeEventHandler handler = (TreeNodeEventHandler)Events[CheckChangedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Overridden to register for postback, and if client script is enabled, renders out
        ///     the necessary script and hidden field to function.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            EnsureRenderSettings();

            if (Page != null) {
                if (!Page.IsPostBack && !_dataBound) {
                    ExpandToDepth(Nodes, ExpandDepth);
                }

                Page.RegisterRequiresPostBack(this);

                // Build up a hidden field of the expand state of all nodes
                StringBuilder expandState = new StringBuilder();

                // We need to number all of the nodes, so call save node state.
                int index = 0;
                for (int i = 0; i < Nodes.Count; i++) {
                    SaveNodeState(Nodes[i], ref index, expandState, true);
                }

                if (RenderClientScript) {
                    ClientScriptManager scriptOM = Page.ClientScript;
                    scriptOM.RegisterHiddenField(this, ExpandStateID, expandState.ToString());

                    // Register all the images (including lines if necessary)
                    int imageCount = 6;
                    if (ShowLines) {
                        imageCount = 19;
                    }
                    for (int i = 0; i < imageCount; i++) {
                        string imageUrl = GetImageUrl(i);
                        if (imageUrl.Length > 0) {
                            imageUrl = Util.QuoteJScriptString(imageUrl);
                        }
                        scriptOM.RegisterArrayDeclaration(this, ImageArrayID, "'" + imageUrl + "'");
                    }

                    // Register a hidden field for tracking the selected node and save it in viewstate so we can fire changed events on postback
                    string selectedNodeID = String.Empty;
                    if (SelectedNode != null) {
                        // Validate that the selected node has not been removed
                        TreeNode node = SelectedNode;
                        while ((node != null) && (node != RootNode)) {
                            node = node.GetParentInternal();
                        }

                        if (node == RootNode) {
                            selectedNodeID = SelectedNode.SelectID;
                            ViewState["SelectedNode"] = SelectedNode.SelectID;
                        }
                        else {
                            ViewState["SelectedNode"] = null;
                        }
                    }
                    else {
                        ViewState["SelectedNode"] = null;
                    }

                    scriptOM.RegisterHiddenField(this, SelectedNodeFieldID, selectedNodeID);

                    // TreeView.js depends on WebForms.js so register that too.
                    Page.RegisterWebFormsScript();
                    // Register the external TreeView javascript file.
                    scriptOM.RegisterClientScriptResource(this, typeof(TreeView), "TreeView.js");

                    string clientDataObjectID = ClientDataObjectID;

                    string populateStartupScript = String.Empty;
                    if (PopulateNodesFromClient) {
                        // Remember the max index of the nodes, so we can properly restore client-populated nodes on postback
                        ViewState["LastIndex"] = index;

                        // Register a log for client-populated nodes
                        scriptOM.RegisterHiddenField(this, PopulateLogID, String.Empty);

                        populateStartupScript = clientDataObjectID + ".lastIndex = " + index + ";\r\n" +
                                                clientDataObjectID + ".populateLog = theForm.elements['" + PopulateLogID + "'];\r\n" +
                                                clientDataObjectID + ".treeViewID = '" + UniqueID + "';\r\n" +
                                                clientDataObjectID + ".name = '" + clientDataObjectID + "';\r\n";
                        // Using GetType() here instead of typeof because derived TreeViews might conflict
                        if (!scriptOM.IsClientScriptBlockRegistered(GetType(), "PopulateNode")) {
                            // 
                            scriptOM.RegisterClientScriptBlock(this, GetType(), "PopulateNode",
                            populateNodeScript +
                            scriptOM.GetCallbackEventReference("context.data.treeViewID", "param", "TreeView_ProcessNodeData",
                                                               "context", "TreeView_ProcessNodeData", false) +
                            populateNodeScriptEnd,
                            true /* add script tags */);
                        }
                    }
                    string selectedInfo = String.Empty;
                    if (_selectedNodeStyle != null) {
                        string className = _selectedNodeStyle.RegisteredCssClass;
                        if (className.Length > 0) {
                            className += " ";
                        }
                        string hyperLinkClassName = _selectedNodeStyle.HyperLinkStyle.RegisteredCssClass;
                        if (hyperLinkClassName.Length > 0) {
                            hyperLinkClassName += " ";
                        }
                        if (!String.IsNullOrEmpty(_selectedNodeStyle.CssClass)) {
                            string cssClass = _selectedNodeStyle.CssClass + " ";
                            className += cssClass;
                            hyperLinkClassName += cssClass;
                        }
                        selectedInfo = clientDataObjectID + ".selectedClass = '" + className + "';\r\n" +
                                       clientDataObjectID + ".selectedHyperLinkClass = '" + hyperLinkClassName + "';\r\n";
                    }

                    string hoverInfo = String.Empty;
                    if (EnableHover) {
                        string className = _hoverNodeStyle.RegisteredCssClass;
                        string hyperLinkClassName = _hoverNodeHyperLinkStyle.RegisteredCssClass;
                        if (!String.IsNullOrEmpty(_hoverNodeStyle.CssClass)) {
                            string cssClass = _hoverNodeStyle.CssClass;
                            if (!String.IsNullOrEmpty(className)) {
                                className += " ";
                            }
                            if (!String.IsNullOrEmpty(hyperLinkClassName)) {
                                hyperLinkClassName += " ";
                            }
                            className += cssClass;
                            hyperLinkClassName += cssClass;
                        }

                        selectedInfo = clientDataObjectID + ".hoverClass = '" + className + "';\r\n" +
                                       clientDataObjectID + ".hoverHyperLinkClass = '" + hyperLinkClassName + "';\r\n";
                    }

                    string createDataObjectScript = "var " + clientDataObjectID + " = new Object();\r\n" +
                                                    clientDataObjectID + ".images = " + ImageArrayID + ";\r\n" +
                                                    clientDataObjectID + ".collapseToolTip = \""
                                                        + Util.QuoteJScriptString(CollapseImageToolTip) + "\";\r\n" +
                                                    clientDataObjectID + ".expandToolTip = \""
                                                        + Util.QuoteJScriptString(ExpandImageToolTip) + "\";\r\n" +
                                                    clientDataObjectID + ".expandState = theForm.elements['" + ExpandStateID + "'];\r\n" +
                                                    clientDataObjectID + ".selectedNodeID = theForm.elements['" + SelectedNodeFieldID + "'];\r\n" +
                                                    selectedInfo +
                                                    hoverInfo +
                                                    "(function() {\r\n  for (var i=0;i<" + imageCount + ";i++) {\r\n" +
                                                    "  var preLoad = new Image();\r\n" +
                                                    "  if (" + ImageArrayID + "[i].length > 0)\r\n" +
                                                    "    preLoad.src = " + ImageArrayID + "[i];\r\n" +
                                                    "  }\r\n})();\r\n" + populateStartupScript;

                    // Register a startup script that creates a tree data object
                    // Note: the first line is to prevent Firefox warnings on undeclared identifiers, needed if a user event occurs
                    // before all startup scripts have run.
                    scriptOM.RegisterClientScriptBlock(this, GetType(), ClientID + "_CreateDataObject1", "var " + clientDataObjectID + " = null;", true);
                    scriptOM.RegisterStartupScript(this, GetType(), ClientID + "_CreateDataObject2", createDataObjectScript, true);

                    // DevDiv 95670: Delete circular reference to prevent IE memory leaks during partial update
                    IScriptManager scriptManager = Page.ScriptManager;
                    if ((scriptManager != null) && scriptManager.SupportsPartialRendering) {
                        scriptManager.RegisterDispose(this, ImageArrayID + ".length = 0;\r\n" + clientDataObjectID + " = null;");
                    }
                }
            }
        }

        protected virtual void OnSelectedNodeChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[SelectedNodeChangedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeCollapsed(TreeNodeEventArgs e) {
            TreeNodeEventHandler handler = (TreeNodeEventHandler)Events[TreeNodeCollapsedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeExpanded(TreeNodeEventArgs e) {
            TreeNodeEventHandler handler = (TreeNodeEventHandler)Events[TreeNodeExpandedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodeDataBound(TreeNodeEventArgs e) {
            TreeNodeEventHandler handler = (TreeNodeEventHandler)Events[TreeNodeDataBoundEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTreeNodePopulate(TreeNodeEventArgs e) {
            TreeNodeEventHandler handler = (TreeNodeEventHandler)Events[TreeNodePopulateEvent];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Overridden to create all the tree nodes based on the datasource provided
        /// </devdoc>
        protected internal override void PerformDataBinding() {
            base.PerformDataBinding();

            // This is to treat the case where the tree has already been bound
            // but the data source was removed and we're rebinding (we want to get an emty tree)
            if (!DesignMode && _dataBound &&
                String.IsNullOrEmpty(DataSourceID) && DataSource == null) {

                Nodes.Clear();
                return;
            }

            DataBindNode(RootNode);

            if (!String.IsNullOrEmpty(DataSourceID) || DataSource != null) {
                _dataBound = true;
            }

            // Always expand depth if data is changed
            ExpandToDepth(Nodes, ExpandDepth);
        }


        /// <devdoc>
        ///     Triggers a populate event for the specified node
        /// </devdoc>
        internal void PopulateNode(TreeNode node) {
            if (node.DataBound) {
                DataBindNode(node);
            }
            else {
                OnTreeNodePopulate(new TreeNodeEventArgs(node));
            }
            node.Populated = true;
            node.PopulateOnDemand = false;
        }

        internal void RaiseSelectedNodeChanged() {
            OnSelectedNodeChanged(EventArgs.Empty);
        }


        internal void RaiseTreeNodeCollapsed(TreeNode node) {
            OnTreeNodeCollapsed(new TreeNodeEventArgs(node));
        }


        internal void RaiseTreeNodeExpanded(TreeNode node) {
            OnTreeNodeExpanded(new TreeNodeEventArgs(node));
        }

        private void RegisterStyle(Style style) {
            if (style.IsEmpty) {
                return;
            }

            if (Page != null && Page.SupportsStyleSheets) {
                string name = ClientID + "_" + _cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                Page.Header.StyleSheet.CreateStyleRule(style, this, "." + name);
                style.SetRegisteredCssClass(name);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer) {
            ControlRenderingHelper.WriteSkipLinkStart(writer, RenderingCompatibility, DesignMode, SkipLinkText, SpacerImageUrl, ClientID);

            base.RenderBeginTag(writer);

            if (DesignMode) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
            }
        }


        /// <devdoc>
        ///     Overridden to render all the tree nodes
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            base.RenderContents(writer);

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            bool enabled = IsEnabled;

            // Render all the root nodes and have them render their children recursively
            for (int i = 0; i < Nodes.Count; i++) {
                TreeNode node = Nodes[i];
                bool[] isLast = new bool[10];
                isLast[0] = (i == (Nodes.Count - 1));
                node.Render(writer, i, isLast, enabled);
            }

            // Reset all these cached values so things can pick up changes in the designer
            if (DesignMode) {
                // Reset all these cached values so things can pick up changes in the designer
                if (_nodeStyle != null) {
                    _nodeStyle.ResetCachedStyles();
                }
                if (_leafNodeStyle != null) {
                    _leafNodeStyle.ResetCachedStyles();
                }
                if (_parentNodeStyle != null) {
                    _parentNodeStyle.ResetCachedStyles();
                }
                if (_rootNodeStyle != null) {
                    _rootNodeStyle.ResetCachedStyles();
                }
                if (_selectedNodeStyle != null) {
                    _selectedNodeStyle.ResetCachedStyles();
                }
                if (_hoverNodeStyle != null) {
                    _hoverNodeHyperLinkStyle = new HyperLinkStyle(_hoverNodeStyle);
                }

                foreach (TreeNodeStyle style in LevelStyles) {
                    style.ResetCachedStyles();
                }

                if (_imageUrls != null) {
                    for (int i = 0; i < _imageUrls.Length; i++) {
                        _imageUrls[i] = null;
                    }
                }

                _cachedExpandImageUrl = null;
                _cachedCollapseImageUrl = null;
                _cachedNoExpandImageUrl = null;
                _cachedLeafNodeClassNames = null;
                _cachedLeafNodeHyperLinkClassNames = null;
                _cachedLeafNodeStyles = null;
                _cachedLevelsContainingCssClass = null;
                _cachedParentNodeClassNames = null;
                _cachedParentNodeHyperLinkClassNames = null;
                _cachedParentNodeStyles = null;
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer) {
            if (DesignMode) {
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            base.RenderEndTag(writer);

            ControlRenderingHelper.WriteSkipLinkEnd(writer, DesignMode, SkipLinkText, ClientID);
        }


        /// <devdoc>
        ///     Saves the expand state of nodes.  The value is placed into a hidden field on the page
        ///     which gets updated on the client as nodes are expanded and collapsed.  This also
        ///     numbers the nodes, which provides IDs for the nodes.
        /// </devdoc>
        private void SaveNodeState(TreeNode node, ref int index, StringBuilder expandState, bool rendered) {
            // Set the index for the current node
            node.Index = index++;

            // If we aren't using client script, some checked nodes might not get rendered, and hence,
            // won't postback their checked state.  We need to store some viewstate for those.
            if (node.CheckedSet) {
                if (!Enabled || (!RenderClientScript && !rendered && node.Checked)) {
                    node.PreserveChecked = true;
                }
                else {
                    node.PreserveChecked = false;
                }
            }

            if (node.PopulateOnDemand) {
                if ((node.ChildNodes.Count == 0) || (node.Expanded != true)) {
                    // If the node is to be populated on the client and there are no children or it
                    // has children and is not expanded, it's a collpased node ('c')
                    expandState.Append('c');
                }
                else {
                    // Otherwise, it's an expanded node
                    expandState.Append('e');
                }
            }
            else if (node.ChildNodes.Count == 0) {
                // If there aren't any child nodes, then it's a normal node
                expandState.Append('n');
            }
            else {
                if (node.Expanded == null) {
                    expandState.Append('u');
                }
                else if (node.Expanded == true) {
                    // If it has children and it's expanded, it's expanded
                    expandState.Append('e');
                }
                else {
                    // If it has children and it isn't expanded, it's collapsed
                    expandState.Append('c');
                }
            }

            // If there are children, save their state too
            if (node.ChildNodes.Count > 0) {
                TreeNodeCollection nodes = node.ChildNodes;
                for (int i = 0; i < nodes.Count; i++) {
                    SaveNodeState(nodes[i], ref index, expandState, (node.Expanded == true) && rendered);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///  Saves the state of the <see cref='System.Web.UI.WebControls.TreeView'/>.
        /// </devdoc>
        protected override object SaveViewState() {
            // If the tree is invisible (or one of its parents is) and we're in the GET request, we have to remember (VSWhidbey 349279)
            // 


            if (!Visible && (Page != null) && !Page.IsPostBack) {
                ViewState["NeverExpanded"] = true;
            }

            object[] state = new object[9];

            state[0] = base.SaveViewState();

            bool hasViewState = (state[0] != null);

            if (_nodeStyle != null) {
                state[1] = ((IStateManager)_nodeStyle).SaveViewState();
                hasViewState |= (state[1] != null);
            }

            if (_rootNodeStyle != null) {
                state[2] = ((IStateManager)_rootNodeStyle).SaveViewState();
                hasViewState |= (state[2] != null);
            }

            if (_parentNodeStyle != null) {
                state[3] = ((IStateManager)_parentNodeStyle).SaveViewState();
                hasViewState |= (state[3] != null);
            }

            if (_leafNodeStyle != null) {
                state[4] = ((IStateManager)_leafNodeStyle).SaveViewState();
                hasViewState |= (state[4] != null);
            }

            if (_selectedNodeStyle != null) {
                state[5] = ((IStateManager)_selectedNodeStyle).SaveViewState();
                hasViewState |= (state[5] != null);
            }

            if (_hoverNodeStyle != null) {
                state[6] = ((IStateManager)_hoverNodeStyle).SaveViewState();
                hasViewState |= (state[6] != null);
            }

            if (_levelStyles != null) {
                state[7] = ((IStateManager)_levelStyles).SaveViewState();
                hasViewState |= (state[7] != null);
            }

            state[8] = ((IStateManager)Nodes).SaveViewState();
            hasViewState |= (state[8] != null);

            if (hasViewState) {
                return state;
            }
            else {
                return null;
            }
        }


        /// <devdoc>
        /// Allows a derived TreeView to set the DataBound proprety on a node
        /// </devdoc>
        protected void SetNodeDataBound(TreeNode node, bool dataBound) {
            node.SetDataBound(dataBound);
        }


        /// <devdoc>
        /// Allows a derived TreeView to set the DataItem on a node
        /// </devdoc>
        protected void SetNodeDataItem(TreeNode node, object dataItem) {
            node.SetDataItem(dataItem);
        }


        /// <devdoc>
        /// Allows a derived TreeView to set the DataPath on a node
        /// </devdoc>
        protected void SetNodeDataPath(TreeNode node, string dataPath) {
            node.SetDataPath(dataPath);
        }

        internal void SetSelectedNode(TreeNode node) {
            Debug.Assert(node == null || node.Owner == this);

            if (_selectedNode != node) {
                // Unselect the previously selected node
                if ((_selectedNode != null) && (_selectedNode.Selected)) {
                    _selectedNode.SetSelected(false);
                }
                _selectedNode = node;
                // Notify the new selected node that it's now selected
                if ((_selectedNode != null) && !_selectedNode.Selected) {
                    _selectedNode.SetSelected(true);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Marks the starting point to begin tracking and saving changes to the
        ///    control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();
            if (_nodeStyle != null) {
                ((IStateManager)_nodeStyle).TrackViewState();
            }

            if (_rootNodeStyle != null) {
                ((IStateManager)_rootNodeStyle).TrackViewState();
            }

            if (_parentNodeStyle != null) {
                ((IStateManager)_parentNodeStyle).TrackViewState();
            }

            if (_leafNodeStyle != null) {
                ((IStateManager)_leafNodeStyle).TrackViewState();
            }

            if (_selectedNodeStyle != null) {
                ((IStateManager)_selectedNodeStyle).TrackViewState();
            }

            if (_hoverNodeStyle != null) {
                ((IStateManager)_hoverNodeStyle).TrackViewState();
            }

            if (_levelStyles != null) {
                ((IStateManager)_levelStyles).TrackViewState();
            }

            if (_bindings != null) {
                ((IStateManager)_bindings).TrackViewState();
            }

            ((IStateManager)Nodes).TrackViewState();
        }

        #region IPostBackEventHandler implementation

        /// <internalonly/>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            // Do not take any postback into account if the tree is disabled.
            if (!IsEnabled) return;

            if (AdapterInternal != null) {
                IPostBackEventHandler pbeh = AdapterInternal as IPostBackEventHandler;
                if (pbeh != null) {
                    pbeh.RaisePostBackEvent(eventArgument);
                }
            }
            else {
                if (eventArgument.Length == 0) {
                    return;
                }

                // On postback, see what kind of event we received by checking the first character
                char eventType = eventArgument[0];
                // Get the path of the node specified in the eventArgument
                string nodePath = HttpUtility.HtmlDecode(eventArgument.Substring(1));
                // Find that node in the tree
                TreeNode node = Nodes.FindNode(nodePath.Split(InternalPathSeparator), 0);

                if (node != null) {
                    switch (eventType) {
                        case 't': {
                                // 't' means that we're toggling the expand state of the node
                                if (ShowExpandCollapse ||
                                    (node.SelectAction == TreeNodeSelectAction.Expand) ||
                                    (node.SelectAction == TreeNodeSelectAction.SelectExpand)) {

                                    node.ToggleExpandState();
                                }
                                break;
                            }
                        case 's': {
                                // 's' means that the node has been selected
                                if ((node.SelectAction == TreeNodeSelectAction.Expand) || (node.SelectAction == TreeNodeSelectAction.SelectExpand)) {
                                    if (node.Expanded != true) {
                                        node.Expanded = true;
                                    }
                                    else if (node.SelectAction == TreeNodeSelectAction.Expand) {
                                        // Expand is really just toggle expand state (while SelectExpand is just expand)
                                        node.Expanded = false;
                                    }
                                }

                                if ((node.SelectAction == TreeNodeSelectAction.Select) || (node.SelectAction == TreeNodeSelectAction.SelectExpand)) {
                                    bool selectedChanged = false;
                                    if (!node.Selected) {
                                        selectedChanged = true;
                                    }

                                    node.Selected = true;

                                    if (selectedChanged) {
                                        _fireSelectedNodeChanged = true;
                                    }
                                }
                                break;
                            }
                    }
                }

                if (_fireSelectedNodeChanged) {
                    try {
                        RaiseSelectedNodeChanged();
                    }
                    finally {
                        _fireSelectedNodeChanged = false;
                    }
                }
            }
        }
        #endregion

        #region ICallbackEventHandler implementation

        /// <internalonly/>
        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument) {
            RaiseCallbackEvent(eventArgument);
        }

        string ICallbackEventHandler.GetCallbackResult() {
            return GetCallbackResult();
        }

        protected virtual void RaiseCallbackEvent(string eventArgument) {
            _callbackEventArgument = eventArgument;
        }

        protected virtual string GetCallbackResult() {
            // Do not take any callback into account if the tree is disabled.
            if (!IsEnabled) return String.Empty;

            // Split the eventArgument into pieces
            // The format is (without the spaces):
            // nodeIndex | lastIndex | databound | parentIsLast | text.length | text datapath.Length | datapath path

            // The first piece is always the node index
            int startIndex = 0;
            int endIndex = _callbackEventArgument.IndexOf('|');
            string nodeIndexString = _callbackEventArgument.Substring(startIndex, endIndex);
            int nodeIndex = Int32.Parse(nodeIndexString, CultureInfo.InvariantCulture);

            // The second piece is always the last index
            startIndex = endIndex + 1;
            endIndex = _callbackEventArgument.IndexOf('|', startIndex);
            int lastIndex = Int32.Parse(_callbackEventArgument.Substring(startIndex, endIndex - startIndex), CultureInfo.InvariantCulture);

            // The third piece is always the last databound bool followed by the checked bool
            bool dataBound = (_callbackEventArgument[endIndex + 1] == 't');
            bool nodeChecked = (_callbackEventArgument[endIndex + 2] == 't');

            // Fourth is the parentIsLast array
            startIndex = endIndex + 3;
            endIndex = _callbackEventArgument.IndexOf('|', startIndex);
            string parentIsLast = _callbackEventArgument.Substring(startIndex, endIndex - startIndex);

            // Fifth is the node text
            startIndex = endIndex + 1;
            endIndex = _callbackEventArgument.IndexOf('|', startIndex);
            int nodeTextLength = Int32.Parse(_callbackEventArgument.Substring(startIndex, endIndex - startIndex), CultureInfo.InvariantCulture);
            startIndex = endIndex + 1;
            endIndex = startIndex + nodeTextLength;
            string nodeText = _callbackEventArgument.Substring(startIndex, endIndex - startIndex);

            // Sixth is the data path
            startIndex = endIndex;
            endIndex = _callbackEventArgument.IndexOf('|', startIndex);
            int dataPathLength = Int32.Parse(_callbackEventArgument.Substring(startIndex, endIndex - startIndex), CultureInfo.InvariantCulture);
            startIndex = endIndex + 1;
            endIndex = startIndex + dataPathLength;
            string dataPath = _callbackEventArgument.Substring(startIndex, endIndex - startIndex);

            // Last piece is the value path
            startIndex = endIndex;
            string valuePath = _callbackEventArgument.Substring(startIndex);

            // Last piece of the value path is the node value
            startIndex = valuePath.LastIndexOf(InternalPathSeparator);
            string nodeValue = TreeView.UnEscape(valuePath.Substring(startIndex + 1));

            // Validate that input for forged callbacks
            ValidateEvent(UniqueID,
                String.Concat(nodeIndexString, nodeText, valuePath, dataPath));

            TreeNode node = CreateNode();
            node.PopulateOnDemand = true;
            if (nodeText != null && nodeText.Length != 0) {
                node.Text = nodeText;
            }
            if (nodeValue != null && nodeValue.Length != 0) {
                node.Value = nodeValue;
            }
            node.SetDataBound(dataBound);
            node.Checked = nodeChecked;
            node.SetPath(valuePath);
            node.SetDataPath(dataPath);
            PopulateNode(node);

            string result = String.Empty;
            if (node.ChildNodes.Count > 0) {
                // Get the expand state for all the nodes (like we do in OnPreRender)
                StringBuilder expandState = new StringBuilder();
                for (int i = 0; i < node.ChildNodes.Count; i++) {
                    SaveNodeState(node.ChildNodes[i], ref lastIndex, expandState, true);
                }

                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                // 
                HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

                int depth = node.Depth;
                bool[] isLast = new bool[depth + 5];
                if (parentIsLast.Length > 0) {
                    // Restore the isLast bool array so we can properly draw the lines
                    for (int i = 0; i < parentIsLast.Length; i++) {
                        if (parentIsLast[i] == 't') {
                            isLast[i] = true;
                        }
                    }
                }

                EnsureRenderSettings();

                // Render out the child nodes
                if (node.Expanded != true) {
                    writer.AddStyleAttribute("display", "none");
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Id, CreateNodeId(nodeIndex) + "Nodes");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                node.RenderChildNodes(writer, depth, isLast, true);
                writer.RenderEndTag();
                writer.Flush();
                writer.Close();

                result = lastIndex.ToString(CultureInfo.InvariantCulture) + "|" + expandState.ToString() + "|" + stringWriter.ToString();
            }

            _callbackEventArgument = String.Empty;
            return result;
        }
        #endregion

        #region IPostBackDataHandler implementation

        /// <internalonly/>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            // Do not take any postback into account if the tree is disabled.
            if (!IsEnabled) return false;

            int selectedNodeIndex = -1;

            string postedSelectedNodeID = postCollection[SelectedNodeFieldID];
            if (!String.IsNullOrEmpty(postedSelectedNodeID)) {
                selectedNodeIndex = GetTrailingIndex(postedSelectedNodeID);
            }

            _loadingNodeState = true;
            try {
                Dictionary<int, TreeNode> populatedNodes = null;
                int[] logList = null;
                int logLength = -1;
                // If we're populating on the client, we need to repopulate the nodes that were
                // populated on the client, so add all the node indexes that were populated on the client
                if (PopulateNodesFromClient) {
                    string log = postCollection[PopulateLogID];
                    if (log != null) {
                        string[] logParts = log.Split(',');
                        logLength = logParts.Length;
                        populatedNodes = new Dictionary<int, TreeNode>(Math.Min(logLength, 16)); // don't eagerly allocate the maximum dictionary size
                        logList = new int[logLength];
                        for (int i = 0; i < logLength; i++) {
                            if (logParts[i].Length > 0) {
                                int populateIndex = Int32.Parse(logParts[i], NumberStyles.Integer, CultureInfo.InvariantCulture);
                                if (populateIndex >= 0 && !populatedNodes.ContainsKey(populateIndex)) {
                                    logList[i] = populateIndex;
                                    // Putting null, which will be replaced during LoadNodeState
                                    populatedNodes.Add(populateIndex, null);
                                }
                                else {
                                    logList[i] = -1;
                                }
                            }
                            else {
                                logList[i] = -1;
                            }
                        }
                    }
                }

                // Make sure all the nodes that were checked on the client get checked
                // and restore the expand state of all those nodes.  Also, fill in the populatedNodes dictionary
                // with the actual TreeNode instances
                string expandState = postCollection[ExpandStateID];
                int index = 0;
                for (int i = 0; i < Nodes.Count; i++) {
                    LoadNodeState(Nodes[i], ref index, expandState, populatedNodes, selectedNodeIndex);
                }

                // Now that the populatedNodes dictionary is filled in with TreeNode objects, we need
                // to call populate on those nodes.
                if (PopulateNodesFromClient && (logLength > 0)) {
                    object oLastIndex = ViewState["LastIndex"];
                    int lastIndex = (oLastIndex != null) ? (int)oLastIndex : -1;
                    for (int i = 0; i < logLength; i++) {
                        index = logList[i];
                        if ((index >= 0) && populatedNodes.ContainsKey(index)) {
                            TreeNode node = populatedNodes[index];
                            if (node != null) {
                                PopulateNode(node);

                                // Since the just-populated nodes could have been expanded and populated on the client as well,
                                // we need to load the node state of those nodes (filling in the populatedNodes dictionary with
                                // those TreeNode instances
                                if ((node.ChildNodes.Count > 0) && (lastIndex != -1)) {
                                    TreeNodeCollection nodes = node.ChildNodes;
                                    for (int j = 0; j < nodes.Count; j++) {
                                        LoadNodeState(nodes[j], ref lastIndex, expandState, populatedNodes, selectedNodeIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally {
                _loadingNodeState = false;
            }

            return (_checkedChangedNodes != null);
        }


        /// <internalonly/>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }

        protected virtual void RaisePostDataChangedEvent() {
            // If there were nodes whose check state has changed, fire events for each one
            if (_checkedChangedNodes != null) {
                foreach (TreeNode node in _checkedChangedNodes) {
                    OnTreeNodeCheckChanged(new TreeNodeEventArgs(node));
                }
            }
        }
        #endregion

        private class TreeViewExpandDepthConverter : Int32Converter {
            private const string fullyExpandedString = "FullyExpand";
            private static object[] expandDepthValues = { -1,
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                21, 22, 23, 24, 25, 26, 27, 28, 29, 30};

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                if (sourceType == typeof(string)) {
                    return true;
                }

                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                if (destinationType == typeof(int)) {
                    return true;
                }
                else if (destinationType == typeof(string)) {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }


            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                string strValue = value as string;
                if (strValue != null) {
                    if (String.Equals(strValue, fullyExpandedString, StringComparison.OrdinalIgnoreCase)) {
                        return -1;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string)) {
                    if ((value is int) && ((int)value == -1)) {
                        return fullyExpandedString;
                    }

                    string strValue = value as string;
                    if (strValue != null) {
                        if (String.Equals(strValue, fullyExpandedString, StringComparison.OrdinalIgnoreCase)) {
                            return value;
                        }
                    }
                }
                else if (destinationType == typeof(int)) {
                    string strValue = value as string;
                    if (strValue != null) {
                        if (String.Equals(strValue, fullyExpandedString, StringComparison.OrdinalIgnoreCase)) {
                            return -1;
                        }
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                return new StandardValuesCollection(expandDepthValues);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return true;
            }
        }
    }
}
