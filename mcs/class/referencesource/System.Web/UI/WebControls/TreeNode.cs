//------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    ///     Provides a tree node for use in the TreeView class
    /// </devdoc>
    [ParseChildren(true, "ChildNodes")]
    public class TreeNode : IStateManager, ICloneable {
        private bool _isTrackingViewState;
        private StateBag _viewState;

        private TreeNodeCollection _childNodes;
        private TreeView _owner;
        private TreeNode _parent;

        private bool _populateDesired;
        private int _selectDesired;
        private bool _modifyCheckedNodes;

        private string _parentIsLast;
        private string _toggleNodeAttributeValue;

        private object _dataItem;

        private int _index;

        private string _valuePath;
        private string _internalValuePath;
        private int _depth = -2;

        private bool _isRoot;


        /// <devdoc>
        ///     Constructs a new TreeNode without a text or value
        /// </devdoc>
        public TreeNode() {
            _selectDesired = 0;
        }


        /// <devdoc>
        ///     Constructs a new TreeNode with the specified owner TreeView
        /// </devdoc>
        protected internal TreeNode(TreeView owner, bool isRoot) : this() {
            _owner = owner;
            _isRoot = isRoot;
        }


        /// <devdoc>
        ///     Constructs a new TreeNode with the specified text
        /// </devdoc>
        public TreeNode(string text) : this(text, null, null, null, null) {
        }


        /// <devdoc>
        ///     Constructs a new TreeNode with the specified text, and value
        /// </devdoc>
        public TreeNode(string text, string value) : this(text, value, null, null, null) {
        }


        /// <devdoc>
        ///     Constructs a new TreeNode with the specified text, value, and image URL
        /// </devdoc>
        public TreeNode(string text, string value, string imageUrl) : this(text, value, imageUrl, null, null){
        }


        /// <devdoc>
        ///     Constructs a new TreeNode with the specified text, value, image URL, navigation URL, and target.
        /// </devdoc>
        public TreeNode(string text, string value, string imageUrl, string navigateUrl, string target) : this() {
            if (text != null) {
                Text = text;
            }

            if (value != null) {
                Value = value;
            }

            if (!String.IsNullOrEmpty(imageUrl)) {
                ImageUrl = imageUrl;
            }

            if (!String.IsNullOrEmpty(navigateUrl)) {
                NavigateUrl = navigateUrl;
            }

            if (!String.IsNullOrEmpty(target)) {
                Target = target;
            }
        }


        /// <devdoc>
        ///     Gets and sets the checked state
        /// </devdoc>
        [DefaultValue(false)]
        [WebSysDescription(SR.TreeNode_Checked)]
        public bool Checked {
            get {
                object o = ViewState["Checked"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["Checked"] = value;
                NotifyOwnerChecked();
            }
        }

        /// <devdov>
        /// </devdoc>
        internal bool CheckedSet {
            get {
                return (ViewState["Checked"] != null);
            }
        }


        /// <devdoc>
        ///     Gets whether this node was creating through databinding
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DataBound {
            get {
                object o = ViewState["DataBound"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
        }


        /// <devdoc>
        ///     Gets the collection of children nodes parented to this TreeNode
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(null)]
        [MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public TreeNodeCollection ChildNodes {
            get {
                if (_childNodes == null) {
                    _childNodes = new TreeNodeCollection(this);
                }
                return _childNodes;
            }
        }


        /// <devdoc>
        ///     Gets path to the data to which this node is bound.
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DataPath {
            get {
                string s = (string)ViewState["DataPath"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
        }


        /// <devdoc>
        ///     Gets the depth of the tree node.
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Depth {
            get {
                if (_depth == -2) {
                    if (_isRoot) {
                        return -1;
                    }

                    if (Parent != null) {
                        _depth = Parent.Depth + 1;
                    }
                    else if (_owner != null) {
                        _depth = InternalValuePath.Split(TreeView.InternalPathSeparator).Length - 1;
                    }
                    else {
                        return 0;
                    }
                }
                return _depth;
            }
        }

        /// <devdoc>
        ///     Gets and sets the expand state
        /// </devdoc>
        [DefaultValue(typeof(Nullable<bool>), "")]
        [WebSysDescription(SR.TreeNode_Expanded)]
        public bool? Expanded {
            get {
                object o = ViewState["Expanded"];
                if (o == null) {
                    return null;
                }
                return (bool?)o;
            }
            set {
                bool? oldValue = Expanded;
                // We need to set the viewstate so that it wins over ExpandDepth on the get request (VSWhidbey 331936)
                // N.B. We don't want this to happen when restoring ViewState.
                ViewState["Expanded"] = value;
                if (value != oldValue) {
                    if (_owner != null && _owner.DesignMode) return;

                    if (value == true) {
                        if (PopulateOnDemand) {
                            // If the owner isn't set, remember to populate the node when the
                            // owner is determined
                            if (_owner == null) {
                                _populateDesired = true;
                            }
                            // Don't populate when the TreeView is restoring client expand state
                            else if (!_owner.LoadingNodeState) {
                                Populate();
                            }
                        }

                        if (_owner != null) {
                            _owner.RaiseTreeNodeExpanded(this);
                        }
                    }
                    else if ((value == false) && (oldValue == true) && (ChildNodes.Count > 0)) {
                        if (_owner != null) {
                            _owner.RaiseTreeNodeCollapsed(this);
                        }
                    }
                }
            }
        }

        /// <devdoc>
        ///     Gets the data item for the tree node.
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(null)]
        public object DataItem {
            get {
                return _dataItem;
            }
        }


        /// <devdoc>
        ///     Gets and sets the TreeNode ImageToolTip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.TreeNode_ImageToolTip)]
        public string ImageToolTip {
            get {
                string s = (string)ViewState["ImageToolTip"];

                if (s == null) {
                    return String.Empty;
                }

                return s;
            }
            set {
                ViewState["ImageToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered for this node
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.TreeNode_ImageUrl)]
        public string ImageUrl {
            get {
                string s = (string)ViewState["ImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the unique index for the tree node
        /// </devdoc>
        internal int Index {
            get {
                return _index;
            }
            set {
                _index = value;
            }
        }
       
        // This is the value path that we use internally. It is different from ValuePath
        // because we always use the same separator character and escape it from values.
        internal string InternalValuePath {
            get {
                if (_internalValuePath != null ) {
                    return _internalValuePath;
                }
                if (_parent != null) {
                    // StringBuilder.Insert is expensive, but we need to build starting from the end.
                    // First build a list, then build the string starting from the end of the list.
                    List<string> pathParts = new List<string>();
                    pathParts.Add(TreeView.Escape(Value));
                    TreeNode parent = _parent;
                    while ((parent != null) && !parent._isRoot) {
                        if (parent._internalValuePath != null) {
                            pathParts.Add(parent._internalValuePath);
                            break;
                        }
                        else {
                            pathParts.Add(TreeView.Escape(parent.Value));
                        }
                        parent = parent._parent;
                    }
                    pathParts.Reverse();
                    _internalValuePath = String.Join(TreeView.InternalPathSeparator.ToString(), pathParts.ToArray());
                    return _internalValuePath;
                }
                else {
                    return String.Empty;
                }
            }
        }


        /// <devdoc>
        ///     Gets and sets the URL to navigate to when the node is clicked
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.TreeNode_NavigateUrl)]
        public string NavigateUrl {
            get {
                string s = (string)ViewState["NavigateUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["NavigateUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the owner TreeView for this TreeNode, if there is one
        /// </devdoc>
        internal TreeView Owner {
            get {
                return _owner;
            }
        }


        /// <devdoc>
        ///     Gets the parent TreeNode
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeNode Parent {
            get {
                if ((_parent == null) || _parent._isRoot) {
                    return null;
                }

                return _parent;
            }

        }


        /// <devdoc>
        ///     Gets and sets whether node has been populated
        /// </devdoc>
        internal bool Populated {
            get {
                object o = ViewState["Populated"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["Populated"] = value;
            }
        }


        /// <devdoc>
        ///     Specifies whether the node should have its children populate immediately or
        ///     only when the node is expanded.
        /// </devdoc>
        [DefaultValue(false)]
        [WebSysDescription(SR.TreeNode_PopulateOnDemand)]
        public bool PopulateOnDemand {
            get {
                object o = ViewState["PopulateOnDemand"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["PopulateOnDemand"] = value;
                if (value && (Expanded == true)) {
                    Expanded = null;
                }
            }
        }


        /// <devdoc>
        ///     Gets and sets the PreserveChecked state
        /// </devdoc>
        [DefaultValue(false)]
        internal bool PreserveChecked {
            get {
                object o = ViewState["PreserveChecked"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["PreserveChecked"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the action which the TreeNode will perform when selected
        /// </devdoc>
        [DefaultValue(TreeNodeSelectAction.Select)]
        [WebSysDescription(SR.TreeNode_SelectAction)]
        public TreeNodeSelectAction SelectAction {
            get {
                object o = ViewState["SelectAction"];
                if (o == null) {
                    return TreeNodeSelectAction.Select;
                }
                return (TreeNodeSelectAction)o;
            }
            set {
                ViewState["SelectAction"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the selected state
        /// </devdoc>
        [DefaultValue(false)]
        [WebSysDescription(SR.TreeNode_Selected)]
        public bool Selected {
            get {
                object o = ViewState["Selected"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                SetSelected(value);
                if (_owner == null) {
                    _selectDesired = (value ? +1 : -1);
                    return;
                }
                else if (value) {
                    // Set the TreeView's selected node to this one
                    _owner.SetSelectedNode(this);
                }
                else if (this == _owner.SelectedNode) {
                    _owner.SetSelectedNode(null);
                }
            }
        }
        
        internal string SelectID {
            get {
                if (_owner.ShowExpandCollapse) {
                    return _owner.CreateNodeTextId(Index);
                }
                else {
                    return _owner.CreateNodeId(Index);
                }
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the TreeNode has a CheckBox
        ///     See ShouldSerializeShowCheckBox remarks.
        /// </devdoc>
        [DefaultValue(typeof(Nullable<bool>), "")]
        [WebSysDescription(SR.TreeNode_ShowCheckBox)]
        public bool? ShowCheckBox {
            get {
                object o = ViewState["ShowCheckBox"];
                if (o == null) {
                    return null;
                }
                return (bool?)o;
            }
            set {
                ViewState["ShowCheckBox"] = value;
            }
        }

        /// <devdoc>
        ///     Gets and sets the target window that the TreeNode will browse to if selected
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


        /// <devdoc>
        ///     Gets and sets the display text
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.TreeNode_Text)]
        public string Text {
            get {
                string s = (string)ViewState["Text"];
                if (s == null) {
                    s = (string)ViewState["Value"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return s;
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the TreeNode tooltip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.TreeNode_ToolTip)]
        public string ToolTip {
            get {
                string s = (string)ViewState["ToolTip"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["ToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the value
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.TreeNode_Value)]
        public string Value {
            get {
                string s = (string)ViewState["Value"];
                if (s == null) {
                    s = (string)ViewState["Text"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return s;
            }
            set {
                ViewState["Value"] = value;
                ResetValuePathRecursive();
            }
        }


        /// <devdoc>
        ///     Gets the full path of the TreeNode
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValuePath {
            get {
                if (_valuePath != null) {
                    return _valuePath;
                }

                if (_parent != null) {
                    string parentPath = _parent.ValuePath;
                    _valuePath = ((parentPath.Length == 0) && (_parent.Depth == -1)) ?
                        Value : parentPath + _owner.PathSeparator + Value;
                    return _valuePath;
                }
                else if ((Owner != null) && !String.IsNullOrEmpty(InternalValuePath)) {
                    // Reconstruct the value path from the internal value path (callback case, VSWhidbey 340121)
                    string[] splitValuePath = InternalValuePath.Split(TreeView.InternalPathSeparator);
                    for (int i = 0; i < splitValuePath.Length; i++) {
                        splitValuePath[i] = TreeView.UnEscape(splitValuePath[i]);
                    }
                    _valuePath = String.Join(Owner.PathSeparator.ToString(), splitValuePath);
                    return _valuePath;
                }
                else {
                    return String.Empty;
                }
            }
        }


        /// <devdoc>
        ///     The state for this TreeNode
        /// </devdoc>
        private StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_isTrackingViewState) {
                        ((IStateManager)_viewState).TrackViewState();
                    }
                }
                return _viewState;
            }
        }

        private void ApplyAttributeList(HtmlTextWriter writer, ArrayList list) {
            for (int i = 0; i < list.Count; i += 2) {
                object o = list[i];
                if (o is string) {
                    writer.AddAttribute((string)o, (string)list[i + 1]);
                }
                else {
                    writer.AddAttribute((HtmlTextWriterAttribute)o, (string)list[i + 1]);
                }
            }
        }


        /// <devdoc>
        ///     Collapses the current node
        /// </devdoc>
        public void Collapse() {
            Expanded = false;
        }


        /// <devdoc>
        ///     Collapse the current node and all children recursively
        /// </devdoc>
        public void CollapseAll() {
            SetExpandedRecursive(false);
        }


        /// <devdoc>
        ///     Expands the current node
        /// </devdoc>
        public void Expand() {
            Expanded = true;
        }


        /// <devdoc>
        ///     Expands the current node and all children recursively
        /// </devdoc>
        public void ExpandAll() {
            SetExpandedRecursive(true);
        }

        internal TreeNode GetParentInternal() {
            return _parent;
        }


        /// <devdoc>
        ///     Adds the href javascript for doing client-side node population
        /// </devdoc>
        private string GetPopulateNodeAttribute(HtmlTextWriter writer, string myId, string selectId, string selectImageId, string lineType, int depth, bool[] isLast) {
            string populateNodeAttributeValue = String.Empty;

            // In a callback population scenario, we need to know which node is last in
            // order to render lines or childNodesPadding properly
            if (_parentIsLast == null) {
                char[] parentIsLast = new char[depth + 1];
                for (int i = 0; i < depth + 1; i++) {
                    if (isLast[i]) {
                        parentIsLast[i] = 't';
                    }
                    else {
                        parentIsLast[i] = 'f';
                    }
                }
                _parentIsLast = new string(parentIsLast);
            }

            // function populateNode(data,index,node,lineType,text,path,databound,datapath)
            string indexString = Index.ToString(CultureInfo.InvariantCulture);
            if (_owner.IsNotIE) {
                populateNodeAttributeValue = "javascript:TreeView_PopulateNode(" +
                                             _owner.ClientDataObjectID + "," +
                                             indexString + "," +
                                             "document.getElementById('" + myId + "')," +
                                             "document.getElementById('" + selectId + "')," +
                                             ((selectImageId.Length == 0) ? "null" : ("document.getElementById('" + selectImageId + "')")) + "," +
                                             "'" + lineType + "'," +
                                             "'" + Util.QuoteJScriptString(Text, true) + "'," +
                                             "'" + Util.QuoteJScriptString(InternalValuePath, true) + "'," +
                                             "'" + (DataBound ? 't' : 'f') + "'," +
                                             "'" + Util.QuoteJScriptString(DataPath, true) + "','" +
                                             _parentIsLast + "')";
            }
            else {
                populateNodeAttributeValue = "javascript:TreeView_PopulateNode(" +
                                             _owner.ClientDataObjectID + "," +
                                             indexString + "," +
                                             myId + "," +
                                             selectId + "," +
                                             ((selectImageId.Length == 0) ? "null" : selectImageId) + "," +
                                             "'" + lineType + "'," +
                                             "'" + Util.QuoteJScriptString(Text, true) + "'," +
                                             "'" + Util.QuoteJScriptString(InternalValuePath, true) + "'," +
                                             "'" + (DataBound ? 't' : 'f') + "'," +
                                             "'" + Util.QuoteJScriptString(DataPath, true) + "','" +
                                             _parentIsLast + "')";
            }
            if (_owner.Page != null) {
                _owner.Page.ClientScript.RegisterForEventValidation(_owner.UniqueID,
                    String.Concat(indexString, Text, InternalValuePath, DataPath));
            }

            return populateNodeAttributeValue;
        }

        internal bool GetEffectiveShowCheckBox() {
            return GetEffectiveShowCheckBox(GetTreeNodeType());
        }

        private bool GetEffectiveShowCheckBox(TreeNodeTypes type) {
            if (ShowCheckBox == true) {
                return true;
            }
            if (ShowCheckBox == false) {
                return false;
            }
            return ((_owner.ShowCheckBoxes & type) != 0);
        }

        /// <devdoc>
        ///     Adds the href javascript for doing client-side node expand state toggling
        /// </devdoc>
        private string GetToggleNodeAttributeValue(string myId, string lineType) {
            Debug.Assert(ChildNodes.Count > 0, "No nodes for expansion, why are we rendering an expander?");
            if (_toggleNodeAttributeValue == null) {
                if (_owner.IsNotIE) {
                    _toggleNodeAttributeValue = "javascript:TreeView_ToggleNode(" +
                                                _owner.ClientDataObjectID + "," +
                                                Index.ToString(CultureInfo.InvariantCulture) + "," +
                                                "document.getElementById('" + myId + "')," +
                                                "'" + lineType + "'," +
                                                "document.getElementById('" + myId + "Nodes'))";
                }
                else {
                    _toggleNodeAttributeValue = "javascript:TreeView_ToggleNode(" +
                                                _owner.ClientDataObjectID + "," +
                                                Index.ToString(CultureInfo.InvariantCulture) + "," +
                                                myId + "," +
                                                "'" + lineType + "'," +
                                                myId + "Nodes)";
                }
            }

            return _toggleNodeAttributeValue;
        }

        private TreeNodeTypes GetTreeNodeType() {
            TreeNodeTypes type = TreeNodeTypes.Leaf;
            if ((Depth == 0) && (ChildNodes.Count > 0)) {
                type = TreeNodeTypes.Root;
            }
            else if ((ChildNodes.Count > 0) || PopulateOnDemand) {
                type = TreeNodeTypes.Parent;
            }
            return type;
        }

        private void NotifyOwnerChecked() {
            if (_owner == null) {
                _modifyCheckedNodes = true;
            }
            else {
                object o = ViewState["Checked"];
                if ((o != null) && ((bool)o == true)) {
                    TreeNodeCollection checkedNodes = _owner.CheckedNodes;
                    if (!checkedNodes.Contains(this)) {
                        _owner.CheckedNodes.Add(this);
                    }
                }
                else {
                    _owner.CheckedNodes.Remove(this);
                }
            }
        }


        /// <devdoc>
        ///     Fills in the children for the tree node.
        /// </devdoc>
        internal void Populate() {
            if (!Populated && (ChildNodes.Count == 0)) {
                if (_owner != null) {
                    _owner.PopulateNode(this);
                }
                // If the owner hasn't been determined yet, remember that we need to populate this node
                // when the owner actually does get set
                else {
                    _populateDesired = true;
                }
            }
        }


        /// <devdoc>
        ///     Renders the contents of the node and its children.  It uses the position and isLast parameters
        ///     to determine which lines and which kind of lines to render.
        /// </devdoc>
        internal void Render(HtmlTextWriter writer, int position, bool[] isLast, bool enabled) {
            string myId = String.Empty;
            Debug.Assert(Index != -1, "Didn't assign an index to a node.");
            myId = _owner.CreateNodeId(Index);

            int depth = Depth;
            bool last = false;
            if (depth > -1) {
                last = isLast[depth];
            }

            bool expanded = (Expanded == true);

            TreeNodeStyle mergedStyle = _owner.GetStyle(this);

            // <table cellpadding="0" cellspacing="0" border="0" height="nodespacing">
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // Don't render the top spacing if this is the first root node
            if ((mergedStyle != null) && !mergedStyle.NodeSpacing.IsEmpty && ((depth != 0) || (position != 0))) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height,
                    mergedStyle.NodeSpacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // If this is not a root node, fill in the lines or indent space
            if (depth > 0) {
                for (int i = 0; i < depth; i++) {
                    if (writer is Html32TextWriter) {
                        // <td>
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _owner.NodeIndent.ToString(CultureInfo.InvariantCulture) + "px");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        if (_owner.ShowLines && !isLast[i]) {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.GetImageUrl(TreeView.IImageIndex));
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                        // </td>
                        writer.RenderEndTag();
                    }
                    else {
                        // <td>
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.Write("<div style=\"width:" + _owner.NodeIndent.ToString(CultureInfo.InvariantCulture) + "px;height:1px\">");
                        if (_owner.ShowLines && !isLast[i]) {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.GetImageUrl(TreeView.IImageIndex));
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                        writer.Write("</div>");
                        // </td>
                        writer.RenderEndTag();
                    }
                }
            }

            // A node can expand if its populate on demand or if it has children
            bool canExpand = (PopulateOnDemand || (ChildNodes.Count > 0)) && _owner.ShowExpandCollapse;

            string imageUrl = String.Empty;
            string lineType = " ";
            string imageToolTip = String.Empty;
            if (last) {
                if (canExpand) {
                    if (expanded) {
                        if (_owner.ShowLines) {
                            if (depth == 0) {
                                if (position == 0) {
                                    // The first and only root node
                                    lineType = "-";
                                    imageUrl = _owner.GetImageUrl(TreeView.DashMinusImageIndex);
                                    imageToolTip = _owner.CollapseImageToolTip;
                                }
                                else {
                                    // The last root node
                                    lineType = "l";
                                    imageUrl = _owner.GetImageUrl(TreeView.LMinusImageIndex);
                                    imageToolTip = _owner.CollapseImageToolTip;
                                }
                            }
                            else {
                                // The last node in a set of siblings
                                lineType = "l";
                                imageUrl = _owner.GetImageUrl(TreeView.LMinusImageIndex);
                                imageToolTip = _owner.CollapseImageToolTip;
                            }
                        }
                        else {
                            imageUrl = _owner.GetImageUrl(TreeView.MinusImageIndex);
                            imageToolTip = _owner.CollapseImageToolTip;
                        }
                    }
                    else {
                        if (_owner.ShowLines) {
                            if (depth == 0) {
                                if (position == 0) {
                                    // The first and only root node
                                    lineType = "-";
                                    imageUrl = _owner.GetImageUrl(TreeView.DashPlusImageIndex);
                                    imageToolTip = _owner.ExpandImageToolTip;
                                }
                                else {
                                    // The last root node
                                    lineType = "l";
                                    imageUrl = _owner.GetImageUrl(TreeView.LPlusImageIndex);
                                    imageToolTip = _owner.ExpandImageToolTip;
                                }
                            }
                            else {
                                // The last node in a set of sibling
                                lineType = "l";
                                imageUrl = _owner.GetImageUrl(TreeView.LPlusImageIndex);
                                imageToolTip = _owner.ExpandImageToolTip;
                            }
                        }
                        else {
                            imageUrl = _owner.GetImageUrl(TreeView.PlusImageIndex);
                            imageToolTip = _owner.ExpandImageToolTip;
                        }
                    }
                }
                else {
                    if (_owner.ShowLines) {
                        if (depth == 0) {
                            if (position == 0) {
                                // The first and only node, no children
                                lineType = "-";
                                imageUrl = _owner.GetImageUrl(TreeView.DashImageIndex);
                            }
                            else {
                                // The last root node, no children
                                lineType = "l";
                                imageUrl = _owner.GetImageUrl(TreeView.LImageIndex);
                            }
                        }
                        else {
                            // The last node in a set of siblings, no children
                            lineType = "l";
                            imageUrl = _owner.GetImageUrl(TreeView.LImageIndex);
                        }
                    }
                    else if (_owner.ShowExpandCollapse) {
                        imageUrl = _owner.GetImageUrl(TreeView.NoExpandImageIndex);
                    }
                }
            }
            else {
                if (canExpand) {
                    if (expanded) {
                        if (_owner.ShowLines) {
                            if (depth == 0) {
                                if (position == 0) {
                                    // The first root node
                                    lineType = "r";
                                    imageUrl = _owner.GetImageUrl(TreeView.RMinusImageIndex);
                                    imageToolTip = _owner.CollapseImageToolTip;
                                }
                                else {
                                    // A middle root node
                                    lineType = "t";
                                    imageUrl = _owner.GetImageUrl(TreeView.TMinusImageIndex);
                                    imageToolTip = _owner.CollapseImageToolTip;
                                }
                            }
                            else {
                                // A middle node
                                lineType = "t";
                                imageUrl = _owner.GetImageUrl(TreeView.TMinusImageIndex);
                                imageToolTip = _owner.CollapseImageToolTip;
                            }
                        }
                        else {
                            imageUrl = _owner.GetImageUrl(TreeView.MinusImageIndex);
                            imageToolTip = _owner.CollapseImageToolTip;
                        }
                    }
                    else {
                        if (_owner.ShowLines) {
                            if (depth == 0) {
                                if (position == 0) {
                                    // The first root node
                                    lineType = "r";
                                    imageUrl = _owner.GetImageUrl(TreeView.RPlusImageIndex);
                                    imageToolTip = _owner.ExpandImageToolTip;
                                }
                                else {
                                    // A middle root node
                                    lineType = "t";
                                    imageUrl = _owner.GetImageUrl(TreeView.TPlusImageIndex);
                                    imageToolTip = _owner.ExpandImageToolTip;
                                }
                            }
                            else {
                                // A middle node
                                lineType = "t";
                                imageUrl = _owner.GetImageUrl(TreeView.TPlusImageIndex);
                                imageToolTip = _owner.ExpandImageToolTip;
                            }
                        }
                        else {
                            imageUrl = _owner.GetImageUrl(TreeView.PlusImageIndex);
                            imageToolTip = _owner.ExpandImageToolTip;
                        }
                    }
                }
                else {
                    if (_owner.ShowLines) {
                        if (depth == 0) {
                            if (position == 0) {
                                // The first root node, no children
                                lineType = "r";
                                imageUrl = _owner.GetImageUrl(TreeView.RImageIndex);
                            }
                            else {
                                // A middle root node, no children
                                lineType = "t";
                                imageUrl = _owner.GetImageUrl(TreeView.TImageIndex);
                            }
                        }
                        else {
                            // A middle node, no children
                            lineType = "t";
                            imageUrl = _owner.GetImageUrl(TreeView.TImageIndex);
                        }
                    }
                    else if (_owner.ShowExpandCollapse) {
                        imageUrl = _owner.GetImageUrl(TreeView.NoExpandImageIndex);
                    }
                }
            }

            TreeNodeTypes type = GetTreeNodeType();

            // Figure out the proper node icon
            string nodeImageUrl = String.Empty;

            if (ImageUrl.Length > 0) {
                nodeImageUrl = _owner.ResolveClientUrl(ImageUrl);
            }
            else {
                if ((depth < _owner.LevelStyles.Count) && (_owner.LevelStyles[depth] != null) && mergedStyle.ImageUrl.Length > 0) {
                    nodeImageUrl = _owner.GetLevelImageUrl(depth);
                }
                else {
                    switch (type) {
                        case TreeNodeTypes.Root:
                            nodeImageUrl = _owner.GetImageUrl(TreeView.RootImageIndex);
                            break;
                        case TreeNodeTypes.Parent:
                            nodeImageUrl = _owner.GetImageUrl(TreeView.ParentImageIndex);
                            break;
                        case TreeNodeTypes.Leaf:
                            nodeImageUrl = _owner.GetImageUrl(TreeView.LeafImageIndex);
                            break;
                    }
                }
            }

            string selectImageId = String.Empty;
            if (nodeImageUrl.Length > 0) {
                selectImageId = SelectID + "i";
            }

            if (imageUrl.Length > 0) {
                // <td>
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                if (canExpand) {
                    // If we are using client script and there aren't any expand/collapse handlers attached, do all the toggling/populating
                    if (_owner.RenderClientScript && !_owner.CustomExpandCollapseHandlerExists) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, myId);

                        if (PopulateOnDemand) {
                            // If we are populating from the client, add all the needed attributes and call the client script needed to make the callback
                            if (_owner.PopulateNodesFromClient) {
                                if (ChildNodes.Count != 0) {
                                    throw new InvalidOperationException(SR.GetString(SR.TreeView_PopulateOnlyEmptyNodes, _owner.ID));
                                }
                                writer.AddAttribute(HtmlTextWriterAttribute.Href, GetPopulateNodeAttribute(writer, myId, SelectID, selectImageId, lineType, depth, isLast));
                            }
                            // If we aren't populating from the client, make a postback to do the population
                            else {
                                string href = "javascript:0";
                                if (_owner.Page != null) {
                                    href = _owner.Page.ClientScript.GetPostBackClientHyperlink(_owner,
                                        "t" + InternalValuePath, true, true);
                                }
                                writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
                            }
                        }
                        else {
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, GetToggleNodeAttributeValue(myId, lineType));
                        }
                    }
                    else {
                        string href = "javascript:0";
                        if (_owner.Page != null) {
                            href = _owner.Page.ClientScript.GetPostBackClientHyperlink(_owner, "t" + InternalValuePath, true);
                        }
                        // If we aren't using client script to perform expansions, get a postback reference
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
                    }

                    if (enabled == true) {
                        // <a href=href>
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                    if (imageToolTip.Length > 0) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt,
                            String.Format(CultureInfo.CurrentCulture, imageToolTip, Text));
			            //fix bug 1197460, quirk it so the fix will only be enabled on projects on 4.6.1 or later version of framework
			            if (BinaryCompatibility.Current.TargetsAtLeastFramework461) {
                            writer.AddAttribute(HtmlTextWriterAttribute.Title,
                                String.Format(CultureInfo.CurrentCulture, imageToolTip, Text));
			            }
                    }
                    else {
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
			            //fix bug 1197460, quirk it so the fix will only be enabled on projects on 4.6.1 or later version of framework
			            if (BinaryCompatibility.Current.TargetsAtLeastFramework461) {
                        	writer.AddAttribute(HtmlTextWriterAttribute.Title, String.Empty);
			            }
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();

                    if (enabled == true) {
                        // </a>
                        writer.RenderEndTag();
                    }
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }

                // </td>
                writer.RenderEndTag();
            }

            // Since we need the same set of anchor attributes on both the image and the text
            // accumulate a list here first
            ArrayList anchorAttributes = new ArrayList();
            // If there is a navigation url on this node, set up the navigation stuff
            if (NavigateUrl.Length > 0) {
                // writer.AddAttribute(HtmlTextWriterAttribute.Href, _owner.ResolveClientUrl(NavigateUrl));
                anchorAttributes.Add(HtmlTextWriterAttribute.Href);
                anchorAttributes.Add(_owner.ResolveClientUrl(NavigateUrl));

                // Use the TreeNode Target if it has one, the TreeView's if it doesn't
                string target = ViewState["Target"] as string;
                if (target == null) {
                    target = _owner.Target;
                }

                if (target.Length > 0) {
                    // writer.AddAttribute(HtmlTextWriterAttribute.Target, Target);
                    anchorAttributes.Add(HtmlTextWriterAttribute.Target);
                    anchorAttributes.Add(target);

                    if (_owner.RenderClientScript) {
                        // Use client-script to merge in the selected node style
                        string onClick = String.Empty;
                        if (_owner.Page != null && _owner.Page.SupportsStyleSheets &&
                            (SelectAction == TreeNodeSelectAction.Select) || (SelectAction == TreeNodeSelectAction.SelectExpand)) {

                            onClick = Util.MergeScript(onClick,
                                "TreeView_SelectNode(" + _owner.ClientDataObjectID + 
                                    ", this,'" + SelectID + "');");
                        }

                        if ((SelectAction == TreeNodeSelectAction.Expand) || (SelectAction == TreeNodeSelectAction.SelectExpand)) {
                            if (PopulateOnDemand) {
                                // Postback to populate
                                // 
                                onClick = Util.MergeScript(onClick,
                                    _owner.Page.ClientScript.GetPostBackClientHyperlink(_owner, "t" + InternalValuePath, true, true));
                            }
                            else if (!_owner.CustomExpandCollapseHandlerExists && canExpand) {
                                onClick = Util.MergeScript(onClick, GetToggleNodeAttributeValue(myId, lineType));
                            }
                        }

                        // writer.AddAttribute("onclick", onClick);
                        if (onClick.Length != 0) {
                            anchorAttributes.Add("onclick");
                            anchorAttributes.Add(onClick);
                        }
                    }
                }
            }
            // Otherwise, write out a postback that will select the node
            else {
                // If client script is on, call the proper javascript and the select action is expand
                if (_owner.RenderClientScript && (SelectAction == TreeNodeSelectAction.Expand) && !_owner.CustomExpandCollapseHandlerExists) {
                    // and if the node is being populated on demand, and we are populating nodes from the client, call the populateNode javascript
                    if (PopulateOnDemand) {
                        if (_owner.PopulateNodesFromClient) {
                            // writer.AddAttribute(HtmlTextWriterAttribute.Href, GetPopulateNodeAttribute(writer, myId, SelectID, lineType, depth, isLast));
                            anchorAttributes.Add(HtmlTextWriterAttribute.Href);
                            anchorAttributes.Add(GetPopulateNodeAttribute(writer, myId, SelectID, selectImageId, lineType, depth, isLast));
                        }
                        else {
                            // If we're not populating from the client, postback to populate
                            // writer.AddAttribute(HtmlTextWriterAttribute.Href, _owner.Page.GetPostBackClientHyperlink(_owner, "t" + HttpUtility.HtmlEncode(InternalValuePath), true));
                            anchorAttributes.Add(HtmlTextWriterAttribute.Href);

                            string href = "javascript:0";
                            if (_owner.Page != null) {
                                href = _owner.Page.ClientScript.GetPostBackClientHyperlink(
                                    _owner, "t" + InternalValuePath, true, true);
                            }
                            anchorAttributes.Add(href);
                        }
                    }
                    else if (canExpand) {
                        // writer.AddAttribute(HtmlTextWriterAttribute.Href, GetToggleNodeAttributeValue(myId, lineType));
                        anchorAttributes.Add(HtmlTextWriterAttribute.Href);
                        anchorAttributes.Add(GetToggleNodeAttributeValue(myId, lineType));
                    }
                }
                // If not, just render an href for a postback
                else if (SelectAction != TreeNodeSelectAction.None) {
                    // writer.AddAttribute(HtmlTextWriterAttribute.Href, _owner.Page.GetPostBackClientHyperlink(_owner, "s" + HttpUtility.HtmlEncode(InternalValuePath), true));
                    anchorAttributes.Add(HtmlTextWriterAttribute.Href);

                    if (_owner.Page != null) {
                        string href = _owner.Page.ClientScript.GetPostBackClientHyperlink(
                            _owner, "s" + InternalValuePath, true, true);
                        anchorAttributes.Add(href);

                        if (_owner.RenderClientScript) {
                            anchorAttributes.Add("onclick");
                            anchorAttributes.Add("TreeView_SelectNode(" + _owner.ClientDataObjectID + 
                                ", this,'" + SelectID + "');");
                        }
                    }
                    else {
                        anchorAttributes.Add("javascript:0");
                    }
                }
            }

            if (ToolTip.Length > 0) {
                anchorAttributes.Add(HtmlTextWriterAttribute.Title);
                anchorAttributes.Add(ToolTip);
            }

            // Render out the node icon, if it is set
            if (nodeImageUrl.Length > 0) {
                // <td>
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                // <a>
                ApplyAttributeList(writer, anchorAttributes);

                // Set the id of the text hyperlink
                writer.AddAttribute(HtmlTextWriterAttribute.Id, selectImageId);

                if (enabled == true && SelectAction != TreeNodeSelectAction.None) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, "-1");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Src, nodeImageUrl);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                if (ImageToolTip.Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, ImageToolTip);
		            //fix bug 1197460, quirk it so the fix will only be enabled on projects on 4.6.1 or later version of framework
	                if (BinaryCompatibility.Current.TargetsAtLeastFramework461) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Title, ImageToolTip);
		            }
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
		    	    //fix bug 1197460, quirk it so the fix will only be enabled on projects on 4.6.1 or later version of framework
			        if (BinaryCompatibility.Current.TargetsAtLeastFramework461) {
	                    writer.AddAttribute(HtmlTextWriterAttribute.Title, String.Empty);
			        }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                if (enabled == true && SelectAction != TreeNodeSelectAction.None) {
                    // </a>
                    writer.RenderEndTag();
                }
                // </td>
                writer.RenderEndTag();
            }

            // <td nowrap="nowrap">
            if (!_owner.NodeWrap) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }

            if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                string styleClass = _owner.GetCssClassName(this, false);
                if (styleClass.Trim().Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, styleClass);
                }
            }
            else {
                if (mergedStyle != null) {
                    mergedStyle.AddAttributesToRender(writer);
                }
            }

            if (_owner.EnableHover && (SelectAction != TreeNodeSelectAction.None)) {
                writer.AddAttribute("onmouseover", "TreeView_HoverNode(" + _owner.ClientDataObjectID + ", this)");
                writer.AddAttribute("onmouseout", "TreeView_UnhoverNode(this)");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            // Render a check box if we need to
            if (GetEffectiveShowCheckBox(type)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                string id = myId + "CheckBox";
                writer.AddAttribute(HtmlTextWriterAttribute.Name, id);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, id);  // VSWhidbey 497326: Render id for easier client-side scripting
                if (Checked) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                }
                if (enabled == false) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    if (!_owner.Enabled && (_owner.RenderingCompatibility >= VersionUtil.Framework40) 
                        && !String.IsNullOrEmpty(WebControl.DisabledCssClass)) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, WebControl.DisabledCssClass);
                    }
                }

                if (ToolTip.Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
            }

            // Rendering hook for extended treeviews
            RenderPreText(writer);
            if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                bool renderInlineBorder;
                string styleClass = _owner.GetCssClassName(this, true, out renderInlineBorder);
                if (styleClass.Trim().Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, styleClass);
                    if (renderInlineBorder) {
                        // Add inline style to force the border to none to override any CssClass (VSWhidbey 336610)
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                        // And an inline font-size of 1em to avoid squaring relative font sizes by applying them twice
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                    }
                }
            }
            else {
                if (mergedStyle != null) {
                    mergedStyle.HyperLinkStyle.AddAttributesToRender(writer);
                }
            }

            ApplyAttributeList(writer, anchorAttributes);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, SelectID);

            if ((SelectAction == TreeNodeSelectAction.None) || !enabled) {
                // Render a span so that the styles get applied, with the ID so that POD works
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                // Render plain text if the select action was none
                writer.Write(Text);
                writer.RenderEndTag();
            }
            else {
                // AccessKey
                if (!_owner.AccessKeyRendered && _owner.AccessKey.Length != 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, _owner.AccessKey, true);
                    _owner.AccessKeyRendered = true;
                }
                // <a href=href>
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(Text);
                writer.RenderEndTag(); // </a>
            }
            // Rendering hook for extended treeviews
            RenderPostText(writer);

            // </td>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            if ((mergedStyle != null) && !mergedStyle.NodeSpacing.IsEmpty) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, mergedStyle.NodeSpacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            // </table>
            writer.RenderEndTag();

            // If children exist, maybe render them
            if (ChildNodes.Count > 0) {
                if (isLast.Length < depth + 2) {
                    bool[] newIsLast = new bool[depth + 5];
                    Array.Copy(isLast, 0, newIsLast, 0, isLast.Length);
                    isLast = newIsLast;
                }

                // If client script is enabled, always render the child nodes, and also render a div around the child nodes, with a 'display' style
                if (_owner.RenderClientScript) {
                    if (!expanded) {
                        writer.AddStyleAttribute("display", "none");
                    }
                    else {
                        writer.AddStyleAttribute("display", "block");
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, myId + "Nodes");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    RenderChildNodes(writer, depth, isLast, enabled);
                    writer.RenderEndTag();
                }
                // If client script is not enabled, only render the children if this node is expanded
                else {
                    if (expanded) {
                        RenderChildNodes(writer, depth, isLast, enabled);
                    }
                }
            }
        }


        /// <devdoc>
        ///     Renders the children nodes of the TreeNode
        /// </devdoc>
        internal void RenderChildNodes(HtmlTextWriter writer, int depth, bool[] isLast, bool enabled) {
            TreeNodeStyle mergedStyle = _owner.GetStyle(this);
            // Render the child nodes padding
            if (!mergedStyle.ChildNodesPadding.IsEmpty) {
                writer.AddAttribute(HtmlTextWriterAttribute.Height, mergedStyle.ChildNodesPadding.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            for (int i = 0; i < ChildNodes.Count; i++) {
                TreeNode node = ChildNodes[i];
                isLast[depth + 1] = (i == ChildNodes.Count - 1);
                node.Render(writer, i, isLast, enabled);
            }

            // Render the child nodes padding only if there is another sibling node
            if (!isLast[depth] && !mergedStyle.ChildNodesPadding.IsEmpty) {
                writer.AddAttribute(HtmlTextWriterAttribute.Height, mergedStyle.ChildNodesPadding.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        protected virtual void RenderPostText(HtmlTextWriter writer) {
        }

        protected virtual void RenderPreText(HtmlTextWriter writer) {
        }

        internal void ResetValuePathRecursive() {
            if (_valuePath != null) {
                _valuePath = null;
                foreach (TreeNode child in ChildNodes) {
                    child.ResetValuePathRecursive();
                }
            }
        }


        /// <devdoc>
        ///     Selects the TreeNode
        /// </devdoc>
        public void Select() {
            Selected = true;
        }


        /// <devdoc>
        ///     Marks this node as a databound node
        /// </devdoc>
        internal void SetDataBound(bool dataBound) {
            ViewState["DataBound"] = dataBound;
        }


        /// <devdoc>
        ///     Used by ExpandAll and CollapseAll to recusively set all nodes' Expanded property to the specified value
        /// </devdoc>
        private void SetExpandedRecursive(bool value) {
            Expanded = value;
            if (ChildNodes.Count > 0) {
                for (int i = 0; i < ChildNodes.Count; i++) {
                    ChildNodes[i].SetExpandedRecursive(value);
                }
            }
        }


        /// <devdoc>
        ///     Sets the data item for use by the user in databinding
        /// </devdoc>
        internal void SetDataItem(object dataItem) {
            _dataItem = dataItem;
        }

        /// <devdoc>
        ///     Sets the data path for use by the TreeView in databinding
        /// </devdoc>
        internal void SetDataPath(string dataPath) {
            ViewState["DataPath"] = dataPath;
        }

        internal void SetDirty() {
            ViewState.SetDirty(true);

            if (ChildNodes.Count > 0) {
                ChildNodes.SetDirty();
            }
        }


        /// <devdoc>
        ///     Sets the owner TreeView of this node.
        /// </devdoc>
        internal void SetOwner(TreeView owner) {
            _owner = owner;

            if (_selectDesired == +1) {
                _selectDesired = 0;
                Selected = true;
            }
            else if (_selectDesired == -1) {
                _selectDesired = 0;
                Selected = false;
            }

            if (_populateDesired) {
                _populateDesired = false;
                Populate();
            }

            if (_modifyCheckedNodes) {
                if (_owner != null) {
                    _modifyCheckedNodes = false;

                    if (Checked) {
                        TreeNodeCollection checkedNodes = _owner.CheckedNodes;
                        if (!checkedNodes.Contains(this)) {
                            _owner.CheckedNodes.Add(this);
                        }
                    }
                    else {
                        _owner.CheckedNodes.Remove(this);
                    }
                }
            }

            foreach (TreeNode node in ChildNodes) {
                node.SetOwner(_owner);
            }
        }


        /// <devdoc>
        ///     Sets the parent TreeNode of the node
        /// </devdoc>
        internal void SetParent(TreeNode parent) {
            _parent = parent;
            SetPath(null);
        }


        /// <devdoc>
        ///     Sets the path of the node (without reparenting).  Used in the PopulateNodesFromClient scenario.
        /// </devdoc>
        internal void SetPath(string newPath) {
            _internalValuePath = newPath;
            _depth = -2;
        }

        internal void SetSelected(bool value) {
            ViewState["Selected"] = value;
            // If the owner hasn't been set, remember that we want to select this node
            // when the owner is determined
            if (_owner == null) {
                _selectDesired = (value ? +1 : -1);
            }
        }

        /// <devdoc>
        ///     Switches the expand state of the node
        /// </devdoc>
        public void ToggleExpandState() {
            Expanded = !(Expanded == true);
        }

        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        protected bool IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        protected virtual void LoadViewState(object state) {
            object[] nodeState = (object[])state;

            if (nodeState != null) {
                if (nodeState[0] != null) {
                    ((IStateManager)ViewState).LoadViewState(nodeState[0]);
                    NotifyOwnerChecked();
                }

                if (nodeState[1] != null) {
                    ((IStateManager)ChildNodes).LoadViewState(nodeState[1]);
                }
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        protected virtual object SaveViewState() {
            object[] state = new object[2];
            if (_viewState != null) {
                state[0] = ((IStateManager)_viewState).SaveViewState();
            }

            if (_childNodes != null) {
                state[1] = ((IStateManager)_childNodes).SaveViewState();
            }

            if ((state[0] == null) && (state[1] == null)) {
                return null;
            }

            return state;
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        protected void TrackViewState() {
            _isTrackingViewState = true;

            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }

            if (_childNodes != null) {
                ((IStateManager)_childNodes).TrackViewState();
            }
        }
        #endregion

        #region ICloneable implementation

        /// <internalonly/>
        object ICloneable.Clone() {
            return Clone();
        }

        protected virtual object Clone() {
            TreeNode newNode = new TreeNode();
            newNode.Checked = Checked;
            newNode.Expanded = Expanded;
            newNode.ImageUrl = ImageUrl;
            newNode.ImageToolTip = ImageToolTip;
            newNode.NavigateUrl = NavigateUrl;
            newNode.PopulateOnDemand = PopulateOnDemand;
            newNode.SelectAction = SelectAction;
            newNode.Selected = Selected;
            if (ViewState["ShowCheckBox"] != null) {
                newNode.ShowCheckBox = ShowCheckBox;
            }
            newNode.Target = Target;
            newNode.Text = Text;
            newNode.ToolTip = ToolTip;
            newNode.Value = Value;

            return newNode;
        }
        #endregion
    }
}
