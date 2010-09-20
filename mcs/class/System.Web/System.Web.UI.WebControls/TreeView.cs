//
// System.Web.UI.WebControls.TreeView.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Web.Handlers;
using System.Collections.Specialized;
using System.IO;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[SupportsEventValidation]
	[ControlValueProperty ("SelectedValue")]
	[DefaultEvent ("SelectedNodeChanged")]
	[Designer ("System.Web.UI.Design.WebControls.TreeViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class TreeView: HierarchicalDataBoundControl, IPostBackEventHandler, IPostBackDataHandler, ICallbackEventHandler
	{
		static readonly char[] postDataSplitChars = {'|'};
		
		string activeSiteMapPath;
		bool stylesPrepared;
		Style hoverNodeStyle;
		TreeNodeStyle leafNodeStyle;
		TreeNodeStyle nodeStyle;
		TreeNodeStyle parentNodeStyle;
		TreeNodeStyle rootNodeStyle;
		TreeNodeStyle selectedNodeStyle;
		
		TreeNodeStyleCollection levelStyles;
		TreeNodeCollection nodes;
		TreeNodeBindingCollection dataBindings;
		
		TreeNode selectedNode;
		Hashtable bindings;

		int registeredStylesCounter = -1;
		List<Style> levelLinkStyles;
		Style controlLinkStyle;
		Style nodeLinkStyle;
		Style rootNodeLinkStyle;
		Style parentNodeLinkStyle;
		Style leafNodeLinkStyle;
		Style selectedNodeLinkStyle;
		Style hoverNodeLinkStyle;
		
		static readonly object TreeNodeCheckChangedEvent = new object();
		static readonly object SelectedNodeChangedEvent = new object();
		static readonly object TreeNodeCollapsedEvent = new object();
		static readonly object TreeNodeDataBoundEvent = new object();
		static readonly object TreeNodeExpandedEvent = new object();
		static readonly object TreeNodePopulateEvent = new object();
		
		static Hashtable imageStyles = new Hashtable ();

		class TreeViewExpandDepthConverter : TypeConverter
		{
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				if (sourceType == typeof (string) || sourceType == typeof (int))
					return true;

				return base.CanConvertFrom (context, sourceType);
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof (string) || destinationType == typeof (int))
					return true;

				return base.CanConvertTo (context, destinationType);
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType != typeof (int) && destinationType != typeof (string))
					return base.ConvertTo (context, culture, value, destinationType);
				
				if (value is string) {
					if (destinationType == typeof (int)) {
						if (String.Compare ("FullyExpand", (string)value, StringComparison.OrdinalIgnoreCase) == 0)
							return -1;
						
						try {
							return Int32.Parse ((string)value);
						} catch (Exception) {
							return -1;
						}
					} else
						return value;	
				}

				int val = (int)value;
				if (destinationType == typeof (string)) {
					if (val == -1)
						return "FullyExpand";
					return val.ToString ();
				}
					
				return value;
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (!(value is string) && !(value is int))
					return base.ConvertFrom (context, culture, value);

				if (value is string) {
					if (String.Compare ("FullyExpand", (string)value, StringComparison.OrdinalIgnoreCase) == 0)
						return -1;

					try {
						return Int32.Parse ((string)value);
					} catch (Exception) {
						return null;
					}
				}

				return value;
			}
		}
		
		class ImageStyle
		{
			public ImageStyle (string expand, string collapse, string noExpand, string icon, string iconLeaf, string iconRoot)
			{
				Expand = expand;
				Collapse = collapse;
				NoExpand = noExpand;
				RootIcon = iconRoot;
				ParentIcon = icon;
				LeafIcon = iconLeaf;
			}
			
			public string Expand;
			public string Collapse;
			public string NoExpand;
			public string RootIcon;
			public string ParentIcon;
			public string LeafIcon;
		}
		
		static TreeView ()
		{
			imageStyles [TreeViewImageSet.Arrows] = new ImageStyle ("arrow_plus", "arrow_minus", "arrow_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.BulletedList] = new ImageStyle (null, null, null, "dot_full", "dot_empty", "dot_full");
			imageStyles [TreeViewImageSet.BulletedList2] = new ImageStyle (null, null, null, "box_full", "box_empty", "box_full");
			imageStyles [TreeViewImageSet.BulletedList3] = new ImageStyle (null, null, null, "star_full", "star_empty", "star_full");
			imageStyles [TreeViewImageSet.BulletedList4] = new ImageStyle (null, null, null, "star_full", "star_empty", "dots");
			imageStyles [TreeViewImageSet.Contacts] = new ImageStyle ("TreeView_plus", "TreeView_minus", "contact", null, null, null);
			imageStyles [TreeViewImageSet.Events] = new ImageStyle (null, null, null, "warning", "warning", "warning");
			imageStyles [TreeViewImageSet.Inbox] = new ImageStyle (null, null, null, "inbox", "inbox", "inbox");
			imageStyles [TreeViewImageSet.Msdn] = new ImageStyle ("box_plus", "box_minus", "box_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.Simple] = new ImageStyle (null, null, "box_full", null, null, null);
			imageStyles [TreeViewImageSet.Simple2] = new ImageStyle (null, null, "box_empty", null, null, null);

			// TODO
			imageStyles [TreeViewImageSet.News] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.Faq] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.WindowsHelp] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.XPFileExplorer] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", "folder", "file", "computer");
		}
		
		public event TreeNodeEventHandler TreeNodeCheckChanged {
			add { Events.AddHandler (TreeNodeCheckChangedEvent, value); }
			remove { Events.RemoveHandler (TreeNodeCheckChangedEvent, value); }
		}
		
		public event EventHandler SelectedNodeChanged {
			add { Events.AddHandler (SelectedNodeChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedNodeChangedEvent, value); }
		}
		
		public event TreeNodeEventHandler TreeNodeCollapsed {
			add { Events.AddHandler (TreeNodeCollapsedEvent, value); }
			remove { Events.RemoveHandler (TreeNodeCollapsedEvent, value); }
		}
		
		public event TreeNodeEventHandler TreeNodeDataBound {
			add { Events.AddHandler (TreeNodeDataBoundEvent, value); }
			remove { Events.RemoveHandler (TreeNodeDataBoundEvent, value); }
		}
		
		public event TreeNodeEventHandler TreeNodeExpanded {
			add { Events.AddHandler (TreeNodeExpandedEvent, value); }
			remove { Events.RemoveHandler (TreeNodeExpandedEvent, value); }
		}
		
		public event TreeNodeEventHandler TreeNodePopulate {
			add { Events.AddHandler (TreeNodePopulateEvent, value); }
			remove { Events.RemoveHandler (TreeNodePopulateEvent, value); }
		}
		
		protected virtual void OnTreeNodeCheckChanged (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeCheckChangedEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnSelectedNodeChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [SelectedNodeChangedEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnTreeNodeCollapsed (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeCollapsedEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnTreeNodeDataBound (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeDataBoundEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnTreeNodeExpanded (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeExpandedEvent];
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnTreeNodePopulate (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodePopulateEvent];
				if (eh != null)
					eh (this, e);
			}
		}


		[Localizable (true)]
		public string CollapseImageToolTip {
			get { return ViewState.GetString ("CollapseImageToolTip", "Collapse {0}"); }
			set { ViewState["CollapseImageToolTip"] = value; }
		}

		[MonoTODO ("Implement support for this")]
		[WebCategory ("Behavior")]
		[WebSysDescription ("Whether the tree will automatically generate bindings.")]
		[DefaultValue (true)]
		public bool AutoGenerateDataBindings {
			get { return ViewState.GetBool ("AutoGenerateDataBindings", true); }
			set { ViewState["AutoGenerateDataBindings"] = value; }
		}

		[DefaultValue ("")]
		[WebSysDescription ("The url of the image to show when a node can be collapsed.")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string CollapseImageUrl {
			get { return ViewState.GetString ("CollapseImageUrl", String.Empty); }
			set { ViewState["CollapseImageUrl"] = value; }
		}

		[WebCategory ("Data")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("Bindings for tree nodes.")]
		[Editor ("System.Web.UI.Design.WebControls.TreeViewBindingsEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValue (null)]
		[MergablePropertyAttribute (false)]
		public TreeNodeBindingCollection DataBindings {
			get {
				if (dataBindings == null) {
					dataBindings = new TreeNodeBindingCollection ();
					if (IsTrackingViewState)
						((IStateManager)dataBindings).TrackViewState();
				}
				return dataBindings;
			}
		}

		[WebCategory ("Behavior")]
		[WebSysDescription ("Whether the tree view can use client-side script to expand and collapse nodes.")]
		[Themeable (false)]
		[DefaultValue (true)]
		public bool EnableClientScript {
			get { return ViewState.GetBool ("EnableClientScript", true); }
			set { ViewState["EnableClientScript"] = value; }
		}

		[DefaultValue (-1)]
		[WebCategory ("Behavior")]
		[WebSysDescription ("The initial expand depth.")]
		[TypeConverter ("System.Web.UI.WebControls.TreeView+TreeViewExpandDepthConverter, " + Consts.AssemblySystem_Web)]
		public int ExpandDepth {
			get { return ViewState.GetInt ("ExpandDepth", -1); }
			set { ViewState["ExpandDepth"] = value; }
		}

		[Localizable (true)]
		public string ExpandImageToolTip {
			get { return ViewState.GetString ("ExpandImageToolTip", "Expand {0}"); }
			set { ViewState["ExpandImageToolTip"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebSysDescription ("The url of the image to show when a node can be expanded.")]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string ExpandImageUrl {
			get { return ViewState.GetString ("ExpandImageUrl", String.Empty); }
			set { ViewState["ExpandImageUrl"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style HoverNodeStyle {
			get {
				if (hoverNodeStyle == null) {
					hoverNodeStyle = new Style();
					if (IsTrackingViewState)
						hoverNodeStyle.TrackViewState();
				}
				return hoverNodeStyle;
			}
		}

		[DefaultValue (TreeViewImageSet.Custom)]
		public TreeViewImageSet ImageSet {
			get { return (TreeViewImageSet)ViewState.GetInt ("ImageSet", (int)TreeViewImageSet.Custom); }
			set {
				if (!Enum.IsDefined (typeof (TreeViewImageSet), value))
					throw new ArgumentOutOfRangeException ();
				ViewState["ImageSet"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TreeNodeStyle LeafNodeStyle {
			get {
				if (leafNodeStyle == null) {
					leafNodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						leafNodeStyle.TrackViewState();
				}
				return leafNodeStyle;
			}
		}
		
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.WebControls.TreeNodeStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public TreeNodeStyleCollection LevelStyles {
			get {
				if (levelStyles == null) {
					levelStyles = new TreeNodeStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelStyles).TrackViewState();
				}
				return levelStyles;
			}
		}

		[DefaultValue ("")]
		public string LineImagesFolder {
			get { return ViewState.GetString ("LineImagesFolder", String.Empty); }
			set { ViewState["LineImagesFolder"] = value; }
		}

		[DefaultValue (-1)]
		public int MaxDataBindDepth {
			get { return ViewState.GetInt ("MaxDataBindDepth", -1); }
			set { ViewState["MaxDataBindDepth"] = value; }
		}

		[DefaultValue (20)]
		public int NodeIndent {
			get { return ViewState.GetInt ("NodeIndent", 20); }
			set { ViewState["NodeIndent"] = value; }
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.WebControls.TreeNodeCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[MergablePropertyAttribute (false)]
		public TreeNodeCollection Nodes {
			get {
				if (nodes == null) {
					nodes = new TreeNodeCollection (this);
					if (IsTrackingViewState)
						((IStateManager)nodes).TrackViewState();
				}
				return nodes;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TreeNodeStyle NodeStyle {
			get {
				if (nodeStyle == null) {
					nodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						nodeStyle.TrackViewState();
				}
				return nodeStyle;
			}
		}
		
		[DefaultValue (false)]
		public bool NodeWrap {
			get { return ViewState.GetBool ("NodeWrap", false); }
			set { ViewState ["NodeWrap"] = value; }
		}

		[UrlProperty]
		[DefaultValue ("")]
		[WebSysDescription ("The url of the image to show for leaf nodes.")]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string NoExpandImageUrl {
			get { return ViewState.GetString ("NoExpandImageUrl", String.Empty); }
			set { ViewState ["NoExpandImageUrl"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TreeNodeStyle ParentNodeStyle {
			get {
				if (parentNodeStyle == null) {
					parentNodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						parentNodeStyle.TrackViewState();
				}
				return parentNodeStyle;
			}
		}
		
		[DefaultValue ('/')]
		public char PathSeparator {
			get { return ViewState.GetChar ("PathSeparator", '/'); }
			set { ViewState ["PathSeparator"] = value; }
		}

		[DefaultValue (true)]
		public bool PopulateNodesFromClient {
			get { return ViewState.GetBool ("PopulateNodesFromClient", true); }
			set { ViewState ["PopulateNodesFromClient"] = value; }
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TreeNodeStyle RootNodeStyle {
			get {
				if (rootNodeStyle == null) {
					rootNodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						rootNodeStyle.TrackViewState();
				}
				return rootNodeStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TreeNodeStyle SelectedNodeStyle {
			get {
				if (selectedNodeStyle == null) {
					selectedNodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						selectedNodeStyle.TrackViewState();
				}
				return selectedNodeStyle;
			}
		}

		Style ControlLinkStyle {
			get {
				if (controlLinkStyle == null) {
					controlLinkStyle = new Style ();
					controlLinkStyle.AlwaysRenderTextDecoration = true;
				}
				return controlLinkStyle;
			}
		}

		Style NodeLinkStyle {
			get {
				if (nodeLinkStyle == null) {
					nodeLinkStyle = new Style ();
				}
				return nodeLinkStyle;
			}
		}

		Style RootNodeLinkStyle {
			get {
				if (rootNodeLinkStyle == null)
					rootNodeLinkStyle = new Style ();
				return rootNodeLinkStyle;
			}
		}

		Style ParentNodeLinkStyle {
			get {
				if (parentNodeLinkStyle == null)
					parentNodeLinkStyle = new Style ();
				return parentNodeLinkStyle;
			}
		}

		Style SelectedNodeLinkStyle {
			get {
				if (selectedNodeLinkStyle == null)
					selectedNodeLinkStyle = new Style ();
				return selectedNodeLinkStyle;
			}
		}

		Style LeafNodeLinkStyle {
			get {
				if (leafNodeLinkStyle == null)
					leafNodeLinkStyle = new Style ();
				return leafNodeLinkStyle;
			}
		}

		Style HoverNodeLinkStyle {
			get {
				if (hoverNodeLinkStyle == null)
					hoverNodeLinkStyle = new Style ();
				return hoverNodeLinkStyle;
			}
		}
		
		[DefaultValue (TreeNodeTypes.None)]
		public TreeNodeTypes ShowCheckBoxes {
			get { return (TreeNodeTypes)ViewState.GetInt ("ShowCheckBoxes", (int)TreeNodeTypes.None); }
			set {
				if ((int) value > 7)
					throw new ArgumentOutOfRangeException ();
				ViewState ["ShowCheckBoxes"] = value;
			}
		}

		[DefaultValue (true)]
		public bool ShowExpandCollapse {
			get { return ViewState.GetBool ("ShowExpandCollapse", true); }
			set { ViewState ["ShowExpandCollapse"] = value; }
		}

		[DefaultValue (false)]
		public bool ShowLines {
			get { return ViewState.GetBool ("ShowLines", false); }
			set { ViewState ["ShowLines"] = value; }
		}

		[Localizable (true)]
		public string SkipLinkText
		{
			get { return ViewState.GetString ("SkipLinkText", "Skip Navigation Links."); }
			set { ViewState ["SkipLinkText"] = value; }
		}
		
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TreeNode SelectedNode {
			get { return selectedNode; }
		}

		[Browsable (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedValue {
			get { return selectedNode != null ? selectedNode.Value : String.Empty; }
		}

		[DefaultValue ("")]
		public string Target {
			get { return ViewState.GetString ("Target", String.Empty); }
			set { ViewState ["Target"] = value; }
		}

		[MonoTODO ("why override?")]
		public override bool Visible 
		{
			get { return base.Visible; }
			set { base.Visible = value; }
		}
				
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TreeNodeCollection CheckedNodes {
			get {
				TreeNodeCollection col = new TreeNodeCollection ();
				FindCheckedNodes (Nodes, col);
				return col;
			}
		}
		
		void FindCheckedNodes (TreeNodeCollection nodeList, TreeNodeCollection result)
		{
			foreach (TreeNode node in nodeList) {
				if (node.Checked)
					result.Add (node, false);
				FindCheckedNodes (node.ChildNodes, result);
			}
		}
		
		public void ExpandAll ()
		{
			foreach (TreeNode node in Nodes)
				node.ExpandAll ();
		}
		
		public void CollapseAll ()
		{
			foreach (TreeNode node in Nodes)
				node.CollapseAll ();
		}
		
		public TreeNode FindNode (string valuePath)
		{
			if (valuePath == null)
				throw new ArgumentNullException ("valuePath");
			string[] path = valuePath.Split (PathSeparator);
			int n = 0;
			TreeNodeCollection col = Nodes;
			bool foundBranch = true;
			while (col.Count > 0 && foundBranch) {
				foundBranch = false;
				foreach (TreeNode node in col) {
					if (node.Value == path [n]) {
						if (++n == path.Length)
							return node;
						col = node.ChildNodes;
						foundBranch = true;
						break;
					}
				}
			}
			return null;
		}
		
		ImageStyle GetImageStyle ()
		{
			if (ImageSet != TreeViewImageSet.Custom)
				return (ImageStyle) imageStyles [ImageSet];
			else
				return null;
		}
		
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Div; }
		}
		
		protected internal virtual TreeNode CreateNode ()
		{
			return new TreeNode (this);
		}
		
		public sealed override void DataBind ()
		{
			base.DataBind ();
		}
		
		protected void SetNodeDataBound (TreeNode node, bool dataBound)
		{
			node.SetDataBound (dataBound);
		}
		
		protected void SetNodeDataPath (TreeNode node, string dataPath)
		{
			node.SetDataPath (dataPath);
		}
		
		protected void SetNodeDataItem (TreeNode node, object dataItem)
		{
			node.SetDataItem (dataItem);
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}
		
		internal void SetSelectedNode (TreeNode node, bool loading)
		{
			if (selectedNode == node)
				return;
			if (selectedNode != null)
				selectedNode.SelectedFlag = false;
			selectedNode = node;
			if (!loading)
				OnSelectedNodeChanged (new TreeNodeEventArgs (selectedNode));
		}
		
		internal void NotifyCheckChanged (TreeNode node)
		{
			OnTreeNodeCheckChanged (new TreeNodeEventArgs (node));
		}

		internal void NotifyExpandedChanged (TreeNode node)
		{
			if (node.Expanded.HasValue && node.Expanded.Value)
				OnTreeNodeExpanded (new TreeNodeEventArgs (node));
			else if (node.Expanded.HasValue && node.IsParentNode)
				OnTreeNodeCollapsed (new TreeNodeEventArgs (node));
		}

		internal void NotifyPopulateRequired (TreeNode node)
		{
			OnTreeNodePopulate (new TreeNodeEventArgs (node));
		}

		protected override void TrackViewState()
		{
			EnsureDataBound ();
			
			base.TrackViewState();
			if (hoverNodeStyle != null)
				hoverNodeStyle.TrackViewState();
			if (leafNodeStyle != null)
				leafNodeStyle.TrackViewState();
			if (levelStyles != null && levelStyles.Count > 0)
				((IStateManager)levelStyles).TrackViewState();
			if (nodeStyle != null)
				nodeStyle.TrackViewState();
			if (parentNodeStyle != null)
				parentNodeStyle.TrackViewState();
			if (rootNodeStyle != null)
				rootNodeStyle.TrackViewState();
			if (selectedNodeStyle != null)
				selectedNodeStyle.TrackViewState();
			if (dataBindings != null)
				((IStateManager)dataBindings).TrackViewState ();
			if (nodes != null)
				((IStateManager)nodes).TrackViewState();;
		}

		protected override object SaveViewState()
		{
			object[] states = new object [10];
			states[0] = base.SaveViewState();
			states[1] = (hoverNodeStyle == null ? null : hoverNodeStyle.SaveViewState());
			states[2] = (leafNodeStyle == null ? null : leafNodeStyle.SaveViewState());
			states[3] = (levelStyles == null ? null : ((IStateManager)levelStyles).SaveViewState());
			states[4] = (nodeStyle == null ? null : nodeStyle.SaveViewState());
			states[5] = (parentNodeStyle == null ? null : parentNodeStyle.SaveViewState());
			states[6] = (rootNodeStyle == null ? null : rootNodeStyle.SaveViewState());
			states[7] = (selectedNodeStyle == null ? null : selectedNodeStyle.SaveViewState());
			states[8] = (dataBindings == null ? null : ((IStateManager)dataBindings).SaveViewState());
			states[9] = (nodes == null ? null : ((IStateManager)nodes).SaveViewState());

			for (int i = states.Length - 1; i >= 0; i--) {
				if (states [i] != null)
					return states;
			}
			
			return null;
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;
				
			object [] states = (object []) savedState;
			base.LoadViewState (states[0]);
			
			if (states[1] != null)
				HoverNodeStyle.LoadViewState (states[1]);
			if (states[2] != null)
				LeafNodeStyle.LoadViewState(states[2]);
			if (states[3] != null)
				((IStateManager)LevelStyles).LoadViewState(states[3]);
			if (states[4] != null)
				NodeStyle.LoadViewState(states[4]);
			if (states[5] != null)
				ParentNodeStyle.LoadViewState(states[5]);
			if (states[6] != null)
				RootNodeStyle.LoadViewState(states[6]);
			if (states[7] != null)
				SelectedNodeStyle.LoadViewState(states[7]);
			if (states[8] != null)
				((IStateManager)DataBindings).LoadViewState(states[8]);
			if (states[9] != null)
				((IStateManager)Nodes).LoadViewState(states[9]);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			string[] args = eventArgument.Split ('|');
			TreeNode node = FindNodeByPos (args[1]);
			if (node == null)
				return;
			
			if (args [0] == "sel")
				HandleSelectEvent (node);
			else if (args [0] == "ec")
				HandleExpandCollapseEvent (node);
		}
		
		void HandleSelectEvent (TreeNode node)
		{
			switch (node.SelectAction) {
				case TreeNodeSelectAction.Select:
					node.Select ();
					break;
				case TreeNodeSelectAction.Expand:
					node.Expand ();
					break;
				case TreeNodeSelectAction.SelectExpand:
					node.Select ();
					node.Expand ();
					break;
			}
		}
		
		void HandleExpandCollapseEvent (TreeNode node)
		{
			node.ToggleExpandState ();
		}
		
		protected virtual void RaisePostDataChangedEvent ()
		{
		}

		TreeNode MakeNodeTree (string[] args)
		{
			string[] segments = args [0].Split ('_');
			TreeNode ret = null, node;

			foreach (string seg in segments) {
				int idx = Int32.Parse (seg);
				node = new TreeNode (seg);
				if (ret != null) {
					ret.ChildNodes.Add (node);
					node.Index = idx;
				}
				ret = node;
			}

			ret.Value = args [1].Replace ("U+007C", "|");
			ret.ImageUrl = args [2].Replace ("U+007C", "|");
			ret.NavigateUrl = args [3].Replace ("U+007C", "|");
			ret.Target = args [4].Replace ("U+007C", "|");
			ret.Tree = this;
			
			NotifyPopulateRequired (ret);
			
			return ret;
		}
		
		string callbackResult;
		protected virtual void RaiseCallbackEvent (string eventArgs)
		{
			string[] args = eventArgs.Split ('|');
			RequiresDataBinding = true;
			EnsureDataBound ();
			
			TreeNode node = MakeNodeTree (args);
			ArrayList levelLines = new ArrayList ();
			TreeNode nd = node;
			while (nd != null) {
				int childCount = nd.Parent != null ? nd.Parent.ChildNodes.Count : Nodes.Count;
				levelLines.Insert (0, (nd.Index < childCount - 1) ? this : null);
				nd = nd.Parent;
			}
			
			StringWriter sw = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			EnsureStylesPrepared ();

			node.Expanded = true;
			int num = node.ChildNodes.Count;
			for (int n=0; n<num; n++)
				RenderNode (writer, node.ChildNodes [n], node.Depth + 1, levelLines, true, n<num-1);
			
			string res = sw.ToString ();
			callbackResult = res.Length > 0 ? res : "*";
		}
		
		protected virtual string GetCallbackResult ()
		{
			return callbackResult;
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		
		void ICallbackEventHandler.RaiseCallbackEvent (string eventArgs)
		{
			RaiseCallbackEvent (eventArgs);
		}
		
		string ICallbackEventHandler.GetCallbackResult ()
		{
			return GetCallbackResult ();
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
		
		protected internal override void PerformDataBinding ()
		{
			base.PerformDataBinding ();
			InitializeDataBindings ();
			HierarchicalDataSourceView data = GetData (String.Empty);
			if (data == null)
				return;
			Nodes.Clear ();
			IHierarchicalEnumerable e = data.Select ();
			FillBoundChildrenRecursive (e, Nodes);
		}
		
		void FillBoundChildrenRecursive (IHierarchicalEnumerable hEnumerable, TreeNodeCollection nodeCollection)
		{
			if (hEnumerable == null)
				return;			
			
			foreach (object obj in hEnumerable) {
				IHierarchyData hdata = hEnumerable.GetHierarchyData (obj);
				TreeNode child = new TreeNode ();
				nodeCollection.Add (child);
				child.Bind (hdata);
				OnTreeNodeDataBound (new TreeNodeEventArgs (child));

				if (MaxDataBindDepth >= 0 && child.Depth == MaxDataBindDepth)
					continue;

				if (hdata == null || !hdata.HasChildren)
					continue;

				IHierarchicalEnumerable e = hdata.GetChildren ();
				FillBoundChildrenRecursive (e, child.ChildNodes);
			}
		}
		
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			bool res = false;

			if (EnableClientScript && PopulateNodesFromClient) {
				string states = postCollection [ClientID + "_PopulatedStates"];
				if (states != null) {
					foreach (string id in states.Split (postDataSplitChars, StringSplitOptions.RemoveEmptyEntries)) {
						TreeNode node = FindNodeByPos (id);
						if (node != null && node.PopulateOnDemand && !node.Populated) {
							Page page = Page;
							if (page != null && page.IsCallback)
								node.Populated = true; // Bug #492307
							else
								node.Populate (); // Bug #626829
						}
					}
				}
				res = true;
			}

			UnsetCheckStates (Nodes, postCollection);
			SetCheckStates (postCollection);
			
			if (EnableClientScript) {
				string states = postCollection [ClientID + "_ExpandStates"];
				if (states != null) {
					string[] ids = states.Split (postDataSplitChars, StringSplitOptions.RemoveEmptyEntries);
					UnsetExpandStates (Nodes, ids);
					SetExpandStates (ids);
				} else
					UnsetExpandStates (Nodes, new string[0]);
				res = true;
			}
			return res;
		}

		const string _OnPreRender_Script_Preamble =
			"var {0} = new Object ();\n" +
			"{0}.treeId = {1};\n" +
			"{0}.uid = {2};\n" +
			"{0}.showImage = {3};\n";

		const string _OnPreRender_Script_ShowExpandCollapse =
			"{0}.expandImage = {1};\n" +
			"{0}.collapseImage = {2};\n";

		const string _OnPreRender_Script_ShowExpandCollapse_Populate =
			"{0}.noExpandImage = {1};\n";

		const string _OnPreRender_Script_PopulateCallback =
			"{0}.form = {1};\n" +
			"{0}.PopulateNode = function (nodeId, nodeValue, nodeImageUrl, nodeNavigateUrl, nodeTarget) {{\n" +
			"\t{2}.__theFormPostData = \"\";\n" +
			"\t{2}.__theFormPostCollection = new Array ();\n" +
			"\t{2}.WebForm_InitCallback ();\n" +
			"\tTreeView_PopulateNode (this.uid, this.treeId, nodeId, nodeValue, nodeImageUrl, nodeNavigateUrl, nodeTarget)\n}};\n";

		const string _OnPreRender_Script_CallbackOptions =
			"{0}.populateFromClient = {1};\n" +
			"{0}.expandAlt = {2};\n" +
			"{0}.collapseAlt = {3};\n";

		const string _OnPreRender_Script_HoverStyle =
			"{0}.hoverClass = {1};\n" +
			"{0}.hoverLinkClass = {2};\n";
		
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			Page page = Page;
			if (page != null) {
				if (IsEnabled)
					page.RegisterRequiresPostBack (this);
			
				if (EnableClientScript && !page.ClientScript.IsClientScriptIncludeRegistered (typeof(TreeView), "TreeView.js")) {
					string url = page.ClientScript.GetWebResourceUrl (typeof(TreeView), "TreeView.js");
					page.ClientScript.RegisterClientScriptInclude (typeof(TreeView), "TreeView.js", url);
				}
			}
			
			string ctree = ClientID + "_data";
			StringBuilder script = new StringBuilder ();
			script.AppendFormat (_OnPreRender_Script_Preamble,
					     ctree,
					     ClientScriptManager.GetScriptLiteral (ClientID),
					     ClientScriptManager.GetScriptLiteral (UniqueID),
					     ClientScriptManager.GetScriptLiteral (ShowExpandCollapse));			
			
			if (ShowExpandCollapse) {
				ImageStyle imageStyle = GetImageStyle ();
				script.AppendFormat (_OnPreRender_Script_ShowExpandCollapse,
						     ctree,
						     ClientScriptManager.GetScriptLiteral (GetNodeImageUrl ("plus", imageStyle)),
						     ClientScriptManager.GetScriptLiteral (GetNodeImageUrl ("minus", imageStyle)));
						     
				if (PopulateNodesFromClient)
					script.AppendFormat (_OnPreRender_Script_ShowExpandCollapse_Populate,
							     ctree,
							     ClientScriptManager.GetScriptLiteral (GetNodeImageUrl ("noexpand", imageStyle)));
			}

			if (page != null) {
				script.AppendFormat (_OnPreRender_Script_PopulateCallback,
						     ctree,
						     page.theForm,
						     page.WebFormScriptReference);
				
						     // Page.ClientScript.GetCallbackEventReference (
						     // 	     "this.uid", "nodeId",
						     // 	     "TreeView_PopulateCallback",
						     // 	     "this.treeId + \" \" + nodeId", "TreeView_PopulateCallback", false));
				
				script.AppendFormat (_OnPreRender_Script_CallbackOptions,
						     ctree,
						     ClientScriptManager.GetScriptLiteral (PopulateNodesFromClient),
						     ClientScriptManager.GetScriptLiteral (GetNodeImageToolTip (true, null)),
						     ClientScriptManager.GetScriptLiteral (GetNodeImageToolTip (false, null)));
				
				if (!page.IsPostBack)
					SetNodesExpandedToDepthRecursive (Nodes);

				bool enableClientScript = EnableClientScript;
				if (enableClientScript) {
					page.ClientScript.RegisterHiddenField (ClientID + "_ExpandStates", GetExpandStates ());

					// Make sure the basic script infrastructure is rendered
					page.ClientScript.RegisterWebFormClientScript ();
				}

				if (enableClientScript && PopulateNodesFromClient)
					page.ClientScript.RegisterHiddenField (ClientID + "_PopulatedStates", "|");

				EnsureStylesPrepared ();

				if (hoverNodeStyle != null) {
					if (page.Header == null)
						throw new InvalidOperationException ("Using TreeView.HoverNodeStyle requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");
					RegisterStyle (HoverNodeStyle, HoverNodeLinkStyle);
					script.AppendFormat (_OnPreRender_Script_HoverStyle,
							     ctree,
							     ClientScriptManager.GetScriptLiteral (HoverNodeStyle.RegisteredCssClass),
							     ClientScriptManager.GetScriptLiteral (HoverNodeLinkStyle.RegisteredCssClass));					
				}
				
				page.ClientScript.RegisterStartupScript (typeof(TreeView), this.UniqueID, script.ToString (), true);
				script = null;
			}
		}

		void EnsureStylesPrepared ()
		{
			if (stylesPrepared)
				return;
			stylesPrepared = true;
			PrepareStyles ();
		}

		void PrepareStyles ()
		{
			// The order in which styles are defined matters when more than one class
			// is assigned to an element
			ControlLinkStyle.CopyTextStylesFrom (ControlStyle);
			RegisterStyle (ControlLinkStyle);

			if (nodeStyle != null)
				RegisterStyle (NodeStyle, NodeLinkStyle);

			if (rootNodeStyle != null)
				RegisterStyle (RootNodeStyle, RootNodeLinkStyle);

			if (parentNodeStyle != null)
				RegisterStyle (ParentNodeStyle, ParentNodeLinkStyle);

			if (leafNodeStyle != null)
				RegisterStyle (LeafNodeStyle, LeafNodeLinkStyle);


			if (levelStyles != null && levelStyles.Count > 0) {
				levelLinkStyles = new List<Style> (levelStyles.Count);
				foreach (Style style in levelStyles) {
					Style linkStyle = new Style ();
					levelLinkStyles.Add (linkStyle);
					RegisterStyle (style, linkStyle);
				}
			}

			if (selectedNodeStyle != null)
				RegisterStyle (SelectedNodeStyle, SelectedNodeLinkStyle);
		}

		void SetNodesExpandedToDepthRecursive (TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes) {
				if (!node.Expanded.HasValue) {
					if (ExpandDepth < 0 || node.Depth < ExpandDepth)
						node.Expanded = true;
				}
				SetNodesExpandedToDepthRecursive (node.ChildNodes);
			}
		}

		string IncrementStyleClassName ()
		{
			registeredStylesCounter++;
			return ClientID + "_" + registeredStylesCounter;
		}

		void RegisterStyle (Style baseStyle, Style linkStyle)
		{
			linkStyle.CopyTextStylesFrom (baseStyle);
			linkStyle.BorderStyle = BorderStyle.None;
			linkStyle.AddCssClass (baseStyle.CssClass);
			baseStyle.Font.Reset ();
			RegisterStyle (linkStyle);
			RegisterStyle (baseStyle);
		}

		void RegisterStyle (Style baseStyle)
		{
			if (Page.Header == null)
				return;
			string className = IncrementStyleClassName ().Trim ('_');
			baseStyle.SetRegisteredCssClass (className);
			Page.Header.StyleSheet.CreateStyleRule (baseStyle, this, "." + className);
		}
		
		string GetBindingKey (string dataMember, int depth)
		{
			return dataMember + " " + depth;
		}
		
		void InitializeDataBindings ()
		{
			if (dataBindings != null && dataBindings.Count > 0) {
				bindings = new Hashtable ();
				foreach (TreeNodeBinding bin in dataBindings) {
					string key = GetBindingKey (bin.DataMember, bin.Depth);
					if (!bindings.ContainsKey(key))
						bindings [key] = bin;
				}
			} else
				bindings = null;
		}
		
		internal TreeNodeBinding FindBindingForNode (string type, int depth)
		{
			if (bindings == null)
				return null;
				
			TreeNodeBinding bin = (TreeNodeBinding) bindings [GetBindingKey (type, depth)];
			if (bin != null)
				return bin;
			
			bin = (TreeNodeBinding) bindings [GetBindingKey (type, -1)];
			if (bin != null)
				return bin;
			
			bin = (TreeNodeBinding) bindings [GetBindingKey (String.Empty, depth)];
			if (bin != null)
				return bin;
			
			return (TreeNodeBinding) bindings [GetBindingKey (String.Empty, -1)];
		}
		
		internal void DecorateNode(TreeNode node)
		{
			if (node == null)
				return;
			
			if (node.ImageUrl != null && node.ImageUrl.Length > 0)
				return;

			if (node.IsRootNode && rootNodeStyle != null) {
				node.ImageUrl = rootNodeStyle.ImageUrl;
				return;
			}
			if (node.IsParentNode && parentNodeStyle != null) {
				node.ImageUrl = parentNodeStyle.ImageUrl;
				return;
			}
			if (node.IsLeafNode && leafNodeStyle != null)
				node.ImageUrl = leafNodeStyle.ImageUrl;
		}
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			SiteMapDataSource siteMap = GetDataSource () as SiteMapDataSource;
			bool checkSitePath = IsBoundUsingDataSourceID && siteMap != null;

			if (checkSitePath) {
				IHierarchyData data = siteMap.Provider.CurrentNode;
				if (data != null)
					activeSiteMapPath = data.Path;
			}
			
			ArrayList levelLines = new ArrayList ();
			int num = Nodes.Count;
			for (int n=0; n<num; n++)
				RenderNode (writer, Nodes [n], 0, levelLines, n>0, n<num-1);
		}
		
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
		}
		
		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			string skipLinkText = SkipLinkText;
			
			if (!String.IsNullOrEmpty (skipLinkText)) {
				writer.AddAttribute (HtmlTextWriterAttribute.Href, "#" + ClientID + "_SkipLink");
				writer.RenderBeginTag (HtmlTextWriterTag.A);

				ClientScriptManager csm = new ClientScriptManager (null);
				
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, skipLinkText);
				writer.AddAttribute (HtmlTextWriterAttribute.Src, csm.GetWebResourceUrl (typeof (SiteMapPath), "transparent.gif"));
				writer.AddAttribute (HtmlTextWriterAttribute.Height, "0");
				writer.AddAttribute (HtmlTextWriterAttribute.Width, "0");
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				
				writer.RenderEndTag (); // img
				writer.RenderEndTag (); // a
			}
			base.RenderBeginTag (writer);
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
			base.RenderEndTag (writer);

			if (!String.IsNullOrEmpty (SkipLinkText)) {
				writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID + "_SkipLink");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				writer.RenderEndTag ();
			}
		}
		
 		void RenderNode (HtmlTextWriter writer, TreeNode node, int level, ArrayList levelLines, bool hasPrevious, bool hasNext)
		{
			// FIXME: disabled for now - needs closer investigation
			//if (node.PopulateOnDemand && node.HadChildrenBeforePopulating)
			// 	throw new InvalidOperationException ("PopulateOnDemand cannot be set to true on a node that already has children.");
			
			DecorateNode(node);
			
			string nodeImage;
			bool clientExpand = EnableClientScript && Events [TreeNodeCollapsedEvent] == null && Events [TreeNodeExpandedEvent] == null;
			ImageStyle imageStyle = GetImageStyle ();
			bool renderChildNodes = node.Expanded.HasValue && node.Expanded.Value;
			
			if (clientExpand && !renderChildNodes)
				renderChildNodes = (!node.PopulateOnDemand || node.Populated);
				
			bool hasChildNodes;
			
			if (renderChildNodes)
				hasChildNodes = node.ChildNodes.Count > 0;
			else
				hasChildNodes = (node.PopulateOnDemand && !node.Populated) || node.ChildNodes.Count > 0;
				
			writer.AddAttribute (HtmlTextWriterAttribute.Cellpadding, "0", false);
			writer.AddAttribute (HtmlTextWriterAttribute.Cellspacing, "0", false);
			writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0");
			writer.RenderBeginTag (HtmlTextWriterTag.Table);

			Unit nodeSpacing = GetNodeSpacing (node);
#if !NET_4_0
			if (nodeSpacing != Unit.Empty && (node.Depth > 0 || node.Index > 0))
				RenderMenuItemSpacing (writer, nodeSpacing);
#endif
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			// Vertical lines from previous levels

			nodeImage = GetNodeImageUrl ("i", imageStyle);
			for (int n=0; n<level; n++) {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddStyleAttribute (HtmlTextWriterStyle.Width, NodeIndent + "px");
				writer.AddStyleAttribute (HtmlTextWriterStyle.Height, "1px");
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				if (ShowLines && levelLines [n] != null) {
					writer.AddAttribute (HtmlTextWriterAttribute.Src, nodeImage);
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, String.Empty, false);
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
				}
				writer.RenderEndTag ();
				writer.RenderEndTag ();	// TD
			}
			
			// Node image + line
			bool showExpandCollapse = ShowExpandCollapse;
			bool showLines = ShowLines;
			if (showExpandCollapse || showLines) {
				bool buttonImage = false;
				string tooltip = String.Empty;
				string shape = String.Empty;
				
				if (showLines) {
					if (hasPrevious && hasNext)
						shape = "t";
					else if (hasPrevious && !hasNext)
						shape = "l";
					else if (!hasPrevious && hasNext)
						shape = "r";
					else
						shape = "dash";
				}
				
				if (showExpandCollapse) {
					if (hasChildNodes) {
						buttonImage = true;
						if (node.Expanded.HasValue && node.Expanded.Value)
							shape += "minus";
						else
							shape += "plus";
						tooltip = GetNodeImageToolTip (!(node.Expanded.HasValue && node.Expanded.Value), node.Text);
					} else if (!showLines)
						shape = "noexpand";
				}

				if (!String.IsNullOrEmpty (shape)) {
					nodeImage = GetNodeImageUrl (shape, imageStyle);
					writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
					
					if (buttonImage) {
						if (!clientExpand || (!PopulateNodesFromClient && node.PopulateOnDemand && !node.Populated))
							writer.AddAttribute (HtmlTextWriterAttribute.Href, GetClientEvent (node, "ec"));
						else
							writer.AddAttribute (HtmlTextWriterAttribute.Href, GetClientExpandEvent(node));
						writer.RenderBeginTag (HtmlTextWriterTag.A);	// Anchor
					}

					// tooltip is 'HtmlAttributeEncoded'
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, tooltip);

					if (buttonImage && clientExpand)
						writer.AddAttribute (HtmlTextWriterAttribute.Id, GetNodeClientId (node, "img"));
					writer.AddAttribute (HtmlTextWriterAttribute.Src, nodeImage);
					if (buttonImage)
						writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
					
					if (buttonImage)
						writer.RenderEndTag ();		// Anchor

					writer.RenderEndTag ();		// TD
				}
			}
			
			// Node icon
			
			string imageUrl = node.ImageUrl.Length > 0 ? ResolveClientUrl (node.ImageUrl) : null;
			if (String.IsNullOrEmpty (imageUrl) && imageStyle != null) {
				if (imageStyle.RootIcon != null && node.IsRootNode)
					imageUrl = GetNodeIconUrl (imageStyle.RootIcon);
				else if (imageStyle.ParentIcon != null && node.IsParentNode)
					imageUrl = GetNodeIconUrl (imageStyle.ParentIcon);
				else if (imageStyle.LeafIcon != null && node.IsLeafNode)
					imageUrl = GetNodeIconUrl (imageStyle.LeafIcon);
			}
			
			if (level < LevelStyles.Count && LevelStyles [level].ImageUrl != null)
				imageUrl = ResolveClientUrl (LevelStyles [level].ImageUrl);
			
			if (!String.IsNullOrEmpty (imageUrl)) {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
#if NET_4_0
				writer.AddAttribute (HtmlTextWriterAttribute.Tabindex, "-1");
#endif
				BeginNodeTag (writer, node, clientExpand);
				writer.AddAttribute (HtmlTextWriterAttribute.Src, imageUrl);
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0");
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, node.ImageToolTip);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				writer.RenderEndTag ();	// style tag
				writer.RenderEndTag ();	// TD
			}

			if (!NodeWrap)
				writer.AddStyleAttribute (HtmlTextWriterStyle.WhiteSpace, "nowrap");

			bool nodeIsSelected = node == SelectedNode && selectedNodeStyle != null;
			if (!nodeIsSelected && selectedNodeStyle != null) {
				if (!String.IsNullOrEmpty (activeSiteMapPath))
					nodeIsSelected = String.Compare (activeSiteMapPath, node.NavigateUrl, RuntimeHelpers.StringComparison) == 0;
			}
			
			AddNodeStyle (writer, node, level, nodeIsSelected);
			if (EnableClientScript) {
				writer.AddAttribute ("onmouseout", "TreeView_UnhoverNode(this)", false);
				writer.AddAttribute ("onmouseover", "TreeView_HoverNode('" + ClientID + "', this)");
			}
			writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
			
			// Checkbox
			
			if (node.ShowCheckBoxInternal) {
				writer.AddAttribute (HtmlTextWriterAttribute.Name, ClientID + "_cs_" + node.Path);
				writer.AddAttribute (HtmlTextWriterAttribute.Type, "checkbox", false);
#if NET_4_0
				string str = node.ToolTip;
				if (!String.IsNullOrEmpty (str))
					writer.AddAttribute (HtmlTextWriterAttribute.Title, str);
#else
				writer.AddAttribute (HtmlTextWriterAttribute.Title, node.Text);
#endif
				if (node.Checked)
					writer.AddAttribute (HtmlTextWriterAttribute.Checked, "checked", false);
				writer.RenderBeginTag (HtmlTextWriterTag.Input);	// INPUT
				writer.RenderEndTag ();	// INPUT
			}
			
			// Text
			
			node.BeginRenderText (writer);
			
			if (clientExpand)
				writer.AddAttribute (HtmlTextWriterAttribute.Id, GetNodeClientId (node, "txt"));
			AddNodeLinkStyle (writer, node, level, nodeIsSelected);
			BeginNodeTag (writer, node, clientExpand);
			writer.Write (node.Text);
			writer.RenderEndTag ();	// style tag
			
			node.EndRenderText (writer);
			
			writer.RenderEndTag ();	// TD
			
			writer.RenderEndTag ();	// TR
#if !NET_4_0
			if (nodeSpacing != Unit.Empty)
				RenderMenuItemSpacing (writer, nodeSpacing);
#endif
			
			writer.RenderEndTag ();	// TABLE
			
			// Children
			
			if (hasChildNodes) {
				if (level >= levelLines.Count) {
					if (hasNext)
						levelLines.Add (this);
					else
						levelLines.Add (null);
				} else {
					if (hasNext)
						levelLines [level] = this;
					else
						levelLines [level] = null;
				}
				
				if (clientExpand) {
					if (!(node.Expanded.HasValue && node.Expanded.Value))
						writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "none");
					else
						writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "block");
					writer.AddAttribute (HtmlTextWriterAttribute.Id, GetNodeClientId (node, null));
					writer.RenderBeginTag (HtmlTextWriterTag.Span);
					
					if (renderChildNodes) {
						AddChildrenPadding (writer, node);
						int num = node.ChildNodes.Count;
						for (int n=0; n<num; n++)
							RenderNode (writer, node.ChildNodes [n], level + 1, levelLines, true, n<num-1);
						if (hasNext)
							AddChildrenPadding (writer, node);
					}
					writer.RenderEndTag ();	// SPAN
				} else if (renderChildNodes) {
					AddChildrenPadding (writer, node);
					int num = node.ChildNodes.Count;
					for (int n=0; n<num; n++)
						RenderNode (writer, node.ChildNodes [n], level + 1, levelLines, true, n<num-1);
					if (hasNext)
						AddChildrenPadding (writer, node);
				}
			}
		}

		void AddChildrenPadding (HtmlTextWriter writer, TreeNode node)
		{
			int level = node.Depth;
			Unit cnp = Unit.Empty;
			
			if (levelStyles != null && level < levelStyles.Count)
				cnp = levelStyles [level].ChildNodesPadding;
			if (cnp.IsEmpty && nodeStyle != null)
				cnp = nodeStyle.ChildNodesPadding;
			
			double value;
			if (cnp.IsEmpty || (value = cnp.Value) == 0 || cnp.Type != UnitType.Pixel)
				return;

			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.AddAttribute (HtmlTextWriterAttribute.Height, ((int) value).ToString (), false);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			writer.RenderEndTag (); // td
			writer.RenderEndTag (); // tr
			writer.RenderEndTag (); // table
		}
		
		void RenderMenuItemSpacing (HtmlTextWriter writer, Unit itemSpacing)
		{
#if !NET_4_0
			writer.AddStyleAttribute (HtmlTextWriterStyle.Height, itemSpacing.ToString ());
#endif
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			writer.RenderEndTag ();
			writer.RenderEndTag ();
		}

		Unit GetNodeSpacing (TreeNode node)
		{
			if (node.Selected && selectedNodeStyle != null && selectedNodeStyle.NodeSpacing != Unit.Empty)
				return selectedNodeStyle.NodeSpacing;

			if (levelStyles != null && node.Depth < levelStyles.Count && levelStyles [node.Depth].NodeSpacing != Unit.Empty)
				return levelStyles [node.Depth].NodeSpacing;

			if (node.IsLeafNode) {
				if (leafNodeStyle != null && leafNodeStyle.NodeSpacing != Unit.Empty)
					return leafNodeStyle.NodeSpacing;
			} else if (node.IsRootNode) {
				if (rootNodeStyle != null && rootNodeStyle.NodeSpacing != Unit.Empty)
					return rootNodeStyle.NodeSpacing;
			} else if (node.IsParentNode) {
				if (parentNodeStyle != null && parentNodeStyle.NodeSpacing != Unit.Empty)
					return parentNodeStyle.NodeSpacing;
			}

			if (nodeStyle != null)
				return nodeStyle.NodeSpacing;
			else
				return Unit.Empty;
		}

		void AddNodeStyle (HtmlTextWriter writer, TreeNode node, int level, bool nodeIsSelected)
		{
			TreeNodeStyle style = new TreeNodeStyle ();
			if (Page.Header != null) {
				// styles are registered
				if (nodeStyle != null) {
#if NET_4_0
 					style.PrependCssClass (nodeStyle.RegisteredCssClass);
 					style.PrependCssClass (nodeStyle.CssClass);
#else
					style.AddCssClass (nodeStyle.CssClass);
					style.AddCssClass (nodeStyle.RegisteredCssClass);
#endif
				}
				if (node.IsLeafNode) {
					if (leafNodeStyle != null) {
#if NET_4_0
						style.PrependCssClass (leafNodeStyle.RegisteredCssClass);
						style.PrependCssClass (leafNodeStyle.CssClass);
#else
						style.AddCssClass (leafNodeStyle.CssClass);
						style.AddCssClass (leafNodeStyle.RegisteredCssClass);
#endif
					}
				} else if (node.IsRootNode) {
					if (rootNodeStyle != null) {
#if NET_4_0
						style.PrependCssClass (rootNodeStyle.RegisteredCssClass);
						style.PrependCssClass (rootNodeStyle.CssClass);
#else
						style.AddCssClass (rootNodeStyle.CssClass);
						style.AddCssClass (rootNodeStyle.RegisteredCssClass);
#endif
					}
				} else if (node.IsParentNode) {
					if (parentNodeStyle != null) {
#if NET_4_0
						style.AddCssClass (parentNodeStyle.RegisteredCssClass);
						style.AddCssClass (parentNodeStyle.CssClass);
#else
						style.AddCssClass (parentNodeStyle.CssClass);
						style.AddCssClass (parentNodeStyle.RegisteredCssClass);
#endif
					}
				}
				
				if (levelStyles != null && levelStyles.Count > level) {
#if NET_4_0
 					style.PrependCssClass (levelStyles [level].RegisteredCssClass);
 					style.PrependCssClass (levelStyles [level].CssClass);
#else
					style.AddCssClass (levelStyles [level].CssClass);
					style.AddCssClass (levelStyles [level].RegisteredCssClass);
#endif
				}
				
				if (nodeIsSelected) {
#if NET_4_0
					style.AddCssClass (selectedNodeStyle.RegisteredCssClass);
					style.AddCssClass (selectedNodeStyle.CssClass);
#else
					style.AddCssClass (selectedNodeStyle.CssClass);
					style.AddCssClass (selectedNodeStyle.RegisteredCssClass);
#endif
				}
			} else {
				// styles are not registered
				if (nodeStyle != null) {
					style.CopyFrom (nodeStyle);
				}
				if (node.IsLeafNode) {
					if (leafNodeStyle != null) {
						style.CopyFrom (leafNodeStyle);
					}
				} else if (node.IsRootNode) {
					if (rootNodeStyle != null) {
						style.CopyFrom (rootNodeStyle);
					}
				} else if (node.IsParentNode) {
					if (parentNodeStyle != null) {
						style.CopyFrom (parentNodeStyle);
					}
				}
				if (levelStyles != null && levelStyles.Count > level)
					style.CopyFrom (levelStyles [level]);
				
				if (nodeIsSelected)
					style.CopyFrom (selectedNodeStyle);
			}
			style.AddAttributesToRender (writer);
		}

		void AddNodeLinkStyle (HtmlTextWriter writer, TreeNode node, int level, bool nodeIsSelected)
		{
			Style style = new Style ();
#if NET_4_0
			bool addBorderStyle = false;
#endif
			if (Page.Header != null) {
				// styles are registered
				style.AddCssClass (ControlLinkStyle.RegisteredCssClass);

				if (nodeStyle != null) {
					style.AddCssClass (nodeLinkStyle.CssClass);
					style.AddCssClass (nodeLinkStyle.RegisteredCssClass);
				}

				if (levelLinkStyles != null && levelLinkStyles.Count > level) {
					style.AddCssClass (levelLinkStyles [level].CssClass);
					style.AddCssClass (levelLinkStyles [level].RegisteredCssClass);
#if NET_4_0
					addBorderStyle = true;
#endif
				}
				
				if (node.IsLeafNode) {
					if (leafNodeStyle != null) {
						style.AddCssClass (leafNodeLinkStyle.CssClass);
						style.AddCssClass (leafNodeLinkStyle.RegisteredCssClass);
					}
				} else if (node.IsRootNode) {
					if (rootNodeStyle != null) {
						style.AddCssClass (rootNodeLinkStyle.CssClass);
						style.AddCssClass (rootNodeLinkStyle.RegisteredCssClass);
					}
				} else if (node.IsParentNode) {
					if (parentNodeStyle != null) {
						style.AddCssClass (parentNodeLinkStyle.CssClass);
						style.AddCssClass (parentNodeLinkStyle.RegisteredCssClass);
					}
				}
				
				if (nodeIsSelected) {
					style.AddCssClass (selectedNodeLinkStyle.CssClass);
					style.AddCssClass (selectedNodeLinkStyle.RegisteredCssClass);
				}
			} else {
				// styles are not registered
				style.CopyFrom (ControlLinkStyle);
				if (nodeStyle != null)
					style.CopyFrom (nodeLinkStyle);
				
				if (levelLinkStyles != null && levelLinkStyles.Count > level) {
					style.CopyFrom (levelLinkStyles [level]);
#if NET_4_0
					addBorderStyle = true;
#endif
				}
				
				if (node.IsLeafNode) {
					if (node.IsLeafNode && leafNodeStyle != null)
						style.CopyFrom (leafNodeLinkStyle);
				} else if (node.IsRootNode) {
					if (node.IsRootNode && rootNodeStyle != null)
						style.CopyFrom (rootNodeLinkStyle);
				} else if (node.IsParentNode) {
					if (node.IsParentNode && parentNodeStyle != null)
						style.CopyFrom (parentNodeLinkStyle);
				}
				
				if (nodeIsSelected)
					style.CopyFrom (selectedNodeLinkStyle);
				
				style.AlwaysRenderTextDecoration = true;
			}
#if NET_4_0
			if (addBorderStyle) {
				// This appears not to come from any style. Instead, it's added
				// whenever a level style is present.
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderStyle, "none");
				writer.AddStyleAttribute (HtmlTextWriterStyle.FontSize, "1em");
			}		
#endif
			style.AddAttributesToRender (writer);
		}

		void BeginNodeTag (HtmlTextWriter writer, TreeNode node, bool clientExpand)
		{
			if(node.ToolTip.Length>0)
				writer.AddAttribute (HtmlTextWriterAttribute.Title, node.ToolTip);

			string navigateUrl = node.NavigateUrl;
			if (!String.IsNullOrEmpty (navigateUrl)) {
				string target = node.Target.Length > 0 ? node.Target : Target;
#if TARGET_J2EE
				string navUrl = ResolveClientUrl (navigateUrl, String.Compare (target, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0);
#else
				string navUrl = ResolveClientUrl (navigateUrl);
#endif
				writer.AddAttribute (HtmlTextWriterAttribute.Href, navUrl);
				if (target.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Target, target);
				writer.RenderBeginTag (HtmlTextWriterTag.A);
			} else if (node.SelectAction != TreeNodeSelectAction.None) {
				if (node.SelectAction == TreeNodeSelectAction.Expand && clientExpand)
					writer.AddAttribute (HtmlTextWriterAttribute.Href, GetClientExpandEvent (node));
				else
					writer.AddAttribute (HtmlTextWriterAttribute.Href, GetClientEvent (node, "sel"));

				writer.RenderBeginTag (HtmlTextWriterTag.A);
			} else
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
		}
		
		string GetNodeImageToolTip (bool expand, string txt)
		{
			if (expand) {
				string expandImageToolTip = ExpandImageToolTip;
				if (!String.IsNullOrEmpty (expandImageToolTip))
					return String.Format (expandImageToolTip, HttpUtility.HtmlAttributeEncode (txt));
				else if (txt != null)
					return "Expand " + HttpUtility.HtmlAttributeEncode (txt);
				else
					return "Expand {0}";
			} else {
				string collapseImageToolTip = CollapseImageToolTip;
				if (!String.IsNullOrEmpty (collapseImageToolTip))
					return String.Format (collapseImageToolTip, HttpUtility.HtmlAttributeEncode (txt));
				else if (txt != null)
					return "Collapse " + HttpUtility.HtmlAttributeEncode (txt);
				else
					return "Collapse {0}";
			}
		}
		
		string GetNodeClientId (TreeNode node, string sufix)
		{
			return ClientID + "_" + node.Path + (sufix != null ? "_" + sufix : String.Empty);
		}
							
		string GetNodeImageUrl (string shape, ImageStyle imageStyle)
		{
			if (ShowLines) {
				if (!String.IsNullOrEmpty (LineImagesFolder))
					return ResolveClientUrl (LineImagesFolder + "/" + shape + ".gif");
			} else {
				if (imageStyle != null) {
					if (shape == "plus") {
						if (!String.IsNullOrEmpty (imageStyle.Expand))
							return GetNodeIconUrl (imageStyle.Expand);
					} else if (shape == "minus") {
						if (!String.IsNullOrEmpty (imageStyle.Collapse))
							return GetNodeIconUrl (imageStyle.Collapse);
					} else if (shape == "noexpand") {
						if (!String.IsNullOrEmpty (imageStyle.NoExpand))
							return GetNodeIconUrl (imageStyle.NoExpand);
					}
				} else {
					if (shape == "plus") {
						if (!String.IsNullOrEmpty (ExpandImageUrl))
							return ResolveClientUrl (ExpandImageUrl);
					} else if (shape == "minus") {
						if (!String.IsNullOrEmpty (CollapseImageUrl))
							return ResolveClientUrl (CollapseImageUrl);
					} else if (shape == "noexpand") {
						if (!String.IsNullOrEmpty (NoExpandImageUrl))
							return ResolveClientUrl (NoExpandImageUrl);
					}
				}
				if (!String.IsNullOrEmpty (LineImagesFolder))
					return ResolveClientUrl (LineImagesFolder + "/" + shape + ".gif");
			}
			return Page.ClientScript.GetWebResourceUrl (typeof (TreeView), "TreeView_" + shape + ".gif");
		}
		
		string GetNodeIconUrl (string icon)
		{
			return Page.ClientScript.GetWebResourceUrl (typeof (TreeView), icon + ".gif");
		}
		
		string GetClientEvent (TreeNode node, string ev)
		{
			return Page.ClientScript.GetPostBackClientHyperlink (this, ev + "|" + node.Path, true);
		}
		
		string GetClientExpandEvent (TreeNode node)
		{
			return String.Format ("javascript:TreeView_ToggleExpand ('{0}','{1}','{2}','{3}','{4}','{5}')",
					      ClientID,
					      node.Path,
					      HttpUtility.HtmlAttributeEncode (node.Value).Replace ("'", "\\'").Replace ("|","U+007C"),
					      HttpUtility.HtmlAttributeEncode (node.ImageUrl).Replace ("'", "\\'").Replace ("|","U+007c"),
					      HttpUtility.HtmlAttributeEncode (node.NavigateUrl).Replace ("'", "\\'").Replace ("|","U+007C"),
					      HttpUtility.HtmlAttributeEncode (node.Target).Replace ("'", "\\'").Replace ("|","U+007C"));
		}

		TreeNode FindNodeByPos (string path)
		{
			string[] indexes = path.Split ('_');
			TreeNode node = null;
			
			foreach (string index in indexes) {
				int i = int.Parse (index);
				if (node == null) {
					if (i >= Nodes.Count) return null;
					node = Nodes [i];
				} else {
					if (i >= node.ChildNodes.Count) return null;
					node = node.ChildNodes [i];
				}
			}
			return node;
		}
		
		void UnsetCheckStates (TreeNodeCollection col, NameValueCollection states)
		{
			foreach (TreeNode node in col) {
				if (node.ShowCheckBoxInternal && node.Checked) {
					if (states == null || states [ClientID + "_cs_" + node.Path] == null)
						node.Checked = false;
				}
				if (node.HasChildData)
					UnsetCheckStates (node.ChildNodes, states);
			}
		}
		
		void SetCheckStates (NameValueCollection states)
		{
			if (states == null)
				return;

			string keyPrefix = ClientID + "_cs_";
			foreach (string key in states) {
				if (key.StartsWith (keyPrefix, StringComparison.Ordinal)) {
					string id = key.Substring (keyPrefix.Length);
					TreeNode node = FindNodeByPos (id);
					if (node != null && !node.Checked)
						node.Checked = true;
				}
			}
		}
		
		void UnsetExpandStates (TreeNodeCollection col, string[] states)
		{
			foreach (TreeNode node in col) {
				if (node.Expanded.HasValue && node.Expanded.Value) {
					bool expand = (Array.IndexOf (states, node.Path) != -1);
					if (!expand) node.Expanded = false;
				}
				if (node.HasChildData)
					UnsetExpandStates (node.ChildNodes, states);
			}
		}
		
		void SetExpandStates (string[] states)
		{
			foreach (string id in states) {
				if (String.IsNullOrEmpty (id))
					continue;
				TreeNode node = FindNodeByPos (id);
				if (node != null)
					node.Expanded = true;
			}
		}
		
		string GetExpandStates ()
		{
			StringBuilder sb = new StringBuilder ("|");
			
			foreach (TreeNode node in Nodes)
				GetExpandStates (sb, node);

			return sb.ToString ();
		}
		
		void GetExpandStates (StringBuilder sb, TreeNode node)
		{
			if (node.Expanded.HasValue && node.Expanded.Value) {
				sb.Append (node.Path);
				sb.Append ('|');
			}
			if (node.HasChildData) {
				foreach (TreeNode child in node.ChildNodes)
					GetExpandStates (sb, child);
			}
		}
	}
}
#endif
