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

using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.Handlers;
using System.Collections.Specialized;
using System.IO;

namespace System.Web.UI.WebControls
{
	[ControlValueProperty ("SelectedValue")]
	[DefaultEvent ("SelectedNodeChanged")]
	public class TreeView: HierarchicalDataBoundControl, IPostBackEventHandler, IPostBackDataHandler, ICallbackEventHandler
	{
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
		
		private static readonly object TreeNodeCheckChangedEvent = new object();
		private static readonly object SelectedNodeChangedEvent = new object();
		private static readonly object TreeNodeCollapsedEvent = new object();
		private static readonly object TreeNodeDataBoundEvent = new object();
		private static readonly object TreeNodeExpandedEvent = new object();
		private static readonly object TreeNodePopulateEvent = new object();
		
		static Hashtable imageStyles = new Hashtable ();
		
		class ImageStyle
		{
			public ImageStyle (string expand, string collapse, string noExpand, string icon, string iconLeaf, string iconRoot) {
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
			imageStyles [TreeViewImageSet.Events] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, "warning", null);
			imageStyles [TreeViewImageSet.Inbox] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", "inbox", "inbox", "inbox");
			imageStyles [TreeViewImageSet.Msdn] = new ImageStyle ("box_plus", "box_minus", "box_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.Simple] = new ImageStyle ("TreeView_plus", "TreeView_minus", "box_full", null, null, null);
			imageStyles [TreeViewImageSet.Simple2] = new ImageStyle ("TreeView_plus", "TreeView_minus", "box_empty", null, null, null);

			// TODO
			imageStyles [TreeViewImageSet.News] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.Faq] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.WindowsHelp] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
			imageStyles [TreeViewImageSet.XPFileExplorer] = new ImageStyle ("TreeView_plus", "TreeView_minus", "TreeView_noexpand", null, null, null);
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
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnSelectedNodeChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [SelectedNodeChangedEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnTreeNodeCollapsed (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeCollapsedEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnTreeNodeDataBound (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeDataBoundEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnTreeNodeExpanded (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodeExpandedEvent];
				if (eh != null) eh (this, e);
			}
		}

		protected virtual void OnTreeNodePopulate (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [TreeNodePopulateEvent];
				if (eh != null) eh (this, e);
			}
		}


		[Localizable (true)]
		public virtual string CollapseImageToolTip {
			get {
				object o = ViewState ["CollapseImageToolTip"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["CollapseImageToolTip"] = value;
			}
		}

		[MonoTODO ("Implement support for this")]
		[WebCategory ("Behavior")]
		[WebSysDescription ("Whether the tree will automatically generate bindings.")]
		[DefaultValue (true)]
		public virtual bool AutoGenerateDataBindings {
			get {
				object o = ViewState ["AutoGenerateDataBindings"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["AutoGenerateDataBindings"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription ("The url of the image to show when a node can be collapsed.")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string CollapseImageUrl {
			get {
				object o = ViewState ["CollapseImageUrl"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["CollapseImageUrl"] = value;
			}
		}

		[WebCategory ("Data")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("Bindings for tree nodes.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Web.UI.Design.TreeViewBindingsEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual TreeNodeBindingCollection DataBindings {
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
		public virtual bool EnableClientScript {
			get {
				object o = ViewState ["EnableClientScript"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["EnableClientScript"] = value;
			}
		}

		[DefaultValue (-1)]
		[WebCategory ("Behavior")]
		[WebSysDescription ("The initial expand depth.")]
		public virtual int ExpandDepth {
			get {
				object o = ViewState ["ExpandDepth"];
				if (o != null) return (int)o;
				return -1;
			}
			set {
				ViewState["ExpandDepth"] = value;
			}
		}

		[Localizable (true)]
		public virtual string ExpandImageToolTip {
			get {
				object o = ViewState ["ExpandImageToolTip"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState["ExpandImageToolTip"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebSysDescription ("The url of the image to show when a node can be expanded.")]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string ExpandImageUrl {
			get {
				object o = ViewState ["ExpandImageUrl"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState["ExpandImageUrl"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual Style HoverNodeStyle {
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
		public virtual TreeViewImageSet ImageSet {
			get {
				object o = ViewState ["ImageSet"];
				if(o != null) return (TreeViewImageSet)o;
				return TreeViewImageSet.Custom;
			}
			set {
				ViewState["ImageSet"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TreeNodeStyle LeafNodeStyle {
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
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Web.UI.Design.TreeNodeStyleCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual TreeNodeStyleCollection LevelStyles {
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
		public virtual string LineImagesFolder {
			get {
				object o = ViewState ["LineImagesFolder"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState["LineImagesFolder"] = value;
			}
		}

		[DefaultValue (-1)]
		public virtual int MaxDataBindDepth {
			get {
				object o = ViewState ["MaxDataBindDepth"];
				if(o != null) return (int)o;
				return -1;
			}
			set {
				ViewState["MaxDataBindDepth"] = value;
			}
		}

		[DefaultValue (20)]
		public virtual int NodeIndent {
			get {
				object o = ViewState ["NodeIndent"];
				if(o != null) return (int)o;
				return 20;
			}
			set {
				ViewState["NodeIndent"] = value;
			}
		}
		
		[WebSysDescription ("The collection of nodes of the tree.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.TreeNodeCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual TreeNodeCollection Nodes {
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
		public virtual TreeNodeStyle NodeStyle {
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
		public virtual bool NodeWrap {
			get {
				object o = ViewState ["NodeWrap"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["NodeWrap"] = value;
			}
		}

		[UrlProperty]
		[DefaultValue ("")]
		[WebSysDescription ("The url of the image to show for leaf nodes.")]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string NoExpandImageUrl {
			get {
				object o = ViewState ["NoExpandImageUrl"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["NoExpandImageUrl"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TreeNodeStyle ParentNodeStyle {
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
		public virtual char PathSeparator {
			get {
				object o = ViewState ["PathSeparator"];
				if(o != null) return (char)o;
				return '/';
			}
			set {
				ViewState ["PathSeparator"] = value;
			}
		}

		[DefaultValue (true)]
		public virtual bool PopulateNodesFromClient {
			get {
				object o = ViewState ["PopulateNodesFromClient"];
				if(o != null) return (bool)o;
				return true;
			}
			set {
				ViewState ["PopulateNodesFromClient"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[WebCategory ("Styles")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual TreeNodeStyle RootNodeStyle {
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
		public virtual TreeNodeStyle SelectedNodeStyle {
			get {
				if (selectedNodeStyle == null) {
					selectedNodeStyle = new TreeNodeStyle ();
					if (IsTrackingViewState)
						selectedNodeStyle.TrackViewState();
				}
				return selectedNodeStyle;
			}
		}
		
		[DefaultValue (TreeNodeTypes.None)]
		public virtual TreeNodeTypes ShowCheckBoxes {
			get {
				object o = ViewState ["ShowCheckBoxes"];
				if(o != null) return (TreeNodeTypes) o;
				return TreeNodeTypes.None;
			}
			set {
				ViewState ["ShowCheckBoxes"] = value;
			}
		}

		[DefaultValue (true)]
		public virtual bool ShowExpandCollapse {
			get {
				object o = ViewState ["ShowExpandCollapse"];
				if(o != null) return (bool)o;
				return true;
			}
			set {
				ViewState ["ShowExpandCollapse"] = value;
			}
		}

		[DefaultValue (false)]
		public virtual bool ShowLines {
			get {
				object o = ViewState ["ShowLines"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["ShowLines"] = value;
			}
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
			get { return selectedNode != null ? selectedNode.Value : ""; }
		}

		[DefaultValue ("")]
		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null) return (string)o;
				return "";
			}
			set {
				ViewState ["Target"] = value;
			}
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
				if (node.Checked) result.Add (node);
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
			if (valuePath == null) throw new ArgumentNullException ("valuePath");
			string[] path = valuePath.Split (PathSeparator);
			int n = 0;
			TreeNodeCollection col = Nodes;
			bool foundBranch = true;
			while (col.Count > 0 && foundBranch) {
				foundBranch = false;
				foreach (TreeNode node in col) {
					if (node.Value == path [n]) {
						if (++n == path.Length) return node;
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
		
		internal void SetSelectedNode (TreeNode node)
		{
			if (selectedNode == node) return;
			if (selectedNode != null)
				selectedNode.SelectedFlag = false;
			selectedNode = node;
			selectedNode.SelectedFlag = true;
			OnSelectedNodeChanged (new TreeNodeEventArgs (selectedNode));
		}
		
		internal void NotifyCheckChanged (TreeNode node)
		{
			OnTreeNodeCheckChanged (new TreeNodeEventArgs (node));
		}

		internal void NotifyExpandedChanged (TreeNode node)
		{
			if (node.Expanded)
				OnTreeNodeExpanded (new TreeNodeEventArgs (node));
			else
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
			if (hoverNodeStyle != null) {
				hoverNodeStyle.TrackViewState();
			}
			if (leafNodeStyle != null) {
				leafNodeStyle.TrackViewState();
			}
			if (levelStyles != null) {
				((IStateManager)levelStyles).TrackViewState();
			}
			if (nodeStyle != null) {
				nodeStyle.TrackViewState();
			}
			if (parentNodeStyle != null) {
				parentNodeStyle.TrackViewState();
			}
			if (rootNodeStyle != null) {
				rootNodeStyle.TrackViewState();
			}
			if (selectedNodeStyle != null) {
				selectedNodeStyle.TrackViewState();
			}
			if (dataBindings != null) {
				((IStateManager)dataBindings).TrackViewState ();
			}
			if (nodes != null) {
				((IStateManager)nodes).TrackViewState();;
			}
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
				((IStateManager)dataBindings).LoadViewState(states[8]);
			if (states[9] != null)
				((IStateManager)Nodes).LoadViewState(states[9]);
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			string[] args = eventArgument.Split ('|');
			TreeNode node = FindNodeByPos (args[1]);
			if (node == null) return;
			
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
		
		[MonoTODO]
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			Console.WriteLine ("LoadPostData " + postDataKey);
			return true;
		}
		
		[MonoTODO]
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			Console.WriteLine ("RaisePostDataChangedEvent");
		}
		
		string ICallbackEventHandler.RaiseCallbackEvent (string eventArgs)
		{
			TreeNode node = FindNodeByPos (eventArgs);
			ArrayList levelLines = new ArrayList ();
			TreeNode nd = node;
			while (nd != null) {
				int childCount = nd.Parent != null ? nd.Parent.ChildNodes.Count : Nodes.Count;
				levelLines.Insert (0, (nd.Index < childCount - 1) ? this : null);
				nd = nd.Parent;
			}
			
			StringWriter sw = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			
			int num = node.ChildNodes.Count;
			for (int n=0; n<num; n++)
				RenderNode (writer, node.ChildNodes [n], node.Depth + 1, levelLines, true, n<num-1);
			
			string res = sw.ToString ();
			return res != "" ? res : "*";
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}
		
		protected internal override void PerformDataBinding ()
		{
			base.PerformDataBinding ();
			HierarchicalDataSourceView data = GetData ("");
			IHierarchicalEnumerable e = data.Select ();
			foreach (object obj in e) {
				IHierarchyData hdata = e.GetHierarchyData (obj);
				TreeNode node = new TreeNode ();
				node.Bind (hdata);
				Nodes.Add (node);
			}
		}
		
		protected override void OnLoad (EventArgs e)
		{
			EnsureDataBound ();
			
			if (!Page.IsPostBack && ExpandDepth != 0) {
				foreach (TreeNode node in Nodes)
					node.Expand (ExpandDepth - 1);
			}
			
			if (Page.IsPostBack) {
				if (ShowCheckBoxes != TreeNodeTypes.None) {
					UnsetCheckStates (Nodes, Context.Request.Form);
					SetCheckStates (Context.Request.Form);
				}
				
				if (EnableClientScript) {
					string states = Context.Request [ClientID + "_ExpandStates"];
					if (states != null) {
						string[] ids = states.Split ('|');
						UnsetExpandStates (Nodes, ids);
						SetExpandStates (ids);
					}
					else
						UnsetExpandStates (Nodes, new string[0]);
				}
			}
			
			base.OnLoad (e);
		}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (EnableClientScript && !Page.ClientScript.IsClientScriptIncludeRegistered (typeof(TreeView), "TreeView.js")) {
				string url = Page.GetWebResourceUrl (typeof(TreeView), "TreeView.js");
				Page.ClientScript.RegisterClientScriptInclude (GetType(), "TreeView.js", url);
				
				string ctree = ClientID + "_data";
				string script = string.Format ("var {0} = new Object ();\n", ctree);
				script += string.Format ("{0}.showImage = {1};\n", ctree, GetScriptLiteral (ShowExpandCollapse));
				
				if (ShowExpandCollapse) {
					bool defaultImages = ShowLines || ImageSet != TreeViewImageSet.Custom || (ExpandImageUrl == "" && CollapseImageUrl == "");
					script += string.Format ("{0}.defaultImages = {1};\n", ctree, GetScriptLiteral (defaultImages));
					ImageStyle imageStyle = GetImageStyle ();
					if (!defaultImages) {
						script += string.Format ("{0}.expandImage = {1};\n", ctree, GetScriptLiteral (GetNodeImageUrl ("plus", imageStyle)));
						script += string.Format ("{0}.collapseImage = {1};\n", ctree, GetScriptLiteral (GetNodeImageUrl ("minus", imageStyle)));
					}
					if (PopulateNodesFromClient)
						script += string.Format ("{0}.noExpandImage = {1};\n", ctree, GetScriptLiteral (GetNodeImageUrl ("noexpand", imageStyle)));
				}
				script += string.Format ("{0}.populateFromClient = {1};\n", ctree, GetScriptLiteral (PopulateNodesFromClient));
				script += string.Format ("{0}.expandAlt = {1};\n", ctree, GetScriptLiteral (GetNodeImageToolTip (true, null)));
				script += string.Format ("{0}.collapseAlt = {1};\n", ctree, GetScriptLiteral (GetNodeImageToolTip (false, null)));
				Page.ClientScript.RegisterStartupScript (GetType(), "", script, true);
			}

			if (EnableClientScript) {
				Page.ClientScript.RegisterHiddenField (ClientID + "_ExpandStates", GetExpandStates ());
				
				// Make sure the basic script infrastructure is rendered
		        Page.GetCallbackEventReference (this, "null", "", "null");
				Page.GetPostBackClientHyperlink (this, "");
			}
			
			if (dataBindings != null && dataBindings.Count > 0) {
				bindings = new Hashtable ();
				foreach (TreeNodeBinding bin in dataBindings) {
					string key = GetBindingKey (bin.DataMember, bin.Depth);
					bindings [key] = bin;
				}
			}
			else
				bindings = null;
		}
		
		string GetScriptLiteral (object ob)
		{
			if (ob is string) {
				string s = (string)ob;
				s = s.Replace ("\"", "\\\"");
				return "\"" + s + "\"";
			} else if (ob is bool) {
				return ob.ToString().ToLower();
			} else {
				return ob.ToString ();
			}
		}
		
		string GetBindingKey (string dataMember, int depth)
		{
			return dataMember + " " + depth;
		}
		
		internal TreeNodeBinding FindBindingForNode (string type, int depth)
		{
			if (bindings == null) return null;

			TreeNodeBinding bin = (TreeNodeBinding) bindings [GetBindingKey (type, depth)];
			if (bin != null) return bin;
			
			bin = (TreeNodeBinding) bindings [GetBindingKey (type, -1)];
			if (bin != null) return bin;
			
			bin = (TreeNodeBinding) bindings [GetBindingKey ("", depth)];
			if (bin != null) return bin;
			
			bin = (TreeNodeBinding) bindings [GetBindingKey ("", -1)];
			return bin;
		}
		
		protected override void RenderContents (HtmlTextWriter writer)
		{
			ArrayList levelLines = new ArrayList ();
			int num = Nodes.Count;
			for (int n=0; n<num; n++)
				RenderNode (writer, Nodes [n], 0, levelLines, n>0, n<num-1);
		}
		
 		void RenderNode (HtmlTextWriter writer, TreeNode node, int level, ArrayList levelLines, bool hasPrevious, bool hasNext)
		{
			string nodeImage;
			bool clientExpand = EnableClientScript && Events [TreeNodeCollapsedEvent] == null && Events [TreeNodeExpandedEvent] == null;
			ImageStyle imageStyle = GetImageStyle ();
			bool renderChildNodes = node.Expanded;
			
			if (clientExpand && !renderChildNodes)
				renderChildNodes = (!PopulateNodesFromClient || HasChildInputData (node));
				
			bool hasChildNodes;
			
			if (renderChildNodes)
				hasChildNodes = node.ChildNodes.Count > 0;
			else
				hasChildNodes = (node.PopulateOnDemand && !node.Populated) || node.ChildNodes.Count > 0;
				
			writer.AddAttribute ("cellpadding", "0");
			writer.AddAttribute ("cellspacing", "0");
			writer.AddStyleAttribute ("border-width", "0");
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			// Vertical lines from previous levels
			
			for (int n=0; n<level; n++) {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				if (ShowLines) {
					if (levelLines [n] == null)
						nodeImage = GetNodeImageUrl ("noexpand", imageStyle);
					else
						nodeImage = GetNodeImageUrl ("i", imageStyle);

					writer.AddAttribute ("src", nodeImage);
					writer.AddAttribute ("border", "0");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
					
					writer.RenderEndTag ();	// TD
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
				}
				writer.AddStyleAttribute ("width", NodeIndent + "px");
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				writer.RenderEndTag ();
				writer.RenderEndTag ();	// TD
			}
			
			// Node image + line
			
			if (ShowExpandCollapse || ShowLines) {
				bool buttonImage = false;
				string tooltip = null;
				string shape;
				
				if (ShowLines) {
					if (hasPrevious && hasNext) shape = "t";
					else if (hasPrevious && !hasNext) shape = "l";
					else if (!hasPrevious && hasNext) shape = "r";
					else shape = "dash";
				} else
					shape = "";
				
				if (ShowExpandCollapse) {
					if (hasChildNodes) {
						buttonImage = true;
						if (node.Expanded) shape += "minus";
						else shape += "plus";
						tooltip = GetNodeImageToolTip (!node.Expanded, node.Text);
					} else if (!ShowLines)
						shape = "noexpand";
				}

				if (shape != "") {
					nodeImage = GetNodeImageUrl (shape, imageStyle);
					writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
					
					if (buttonImage) {
						if (!clientExpand)
							writer.AddAttribute ("href", GetClientEvent (node, "ec"));
						else
							writer.AddAttribute ("href", GetClientExpandEvent(node));
						writer.RenderBeginTag (HtmlTextWriterTag.A);	// Anchor
					}
					
					if (tooltip != null)
						writer.AddAttribute ("alt", tooltip);
					if (buttonImage && clientExpand)
						writer.AddAttribute ("id", GetNodeClientId (node, "img"));
					writer.AddAttribute ("src", nodeImage);
					writer.AddAttribute ("border", "0");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();
					
					if (buttonImage)
						writer.RenderEndTag ();		// Anchor

					writer.RenderEndTag ();		// TD
				}
			}
			
			// Node icon
			
			string imageUrl = node.ImageUrl;
			if (imageUrl == "" && imageStyle != null) {
				if (imageStyle.RootIcon != null && node.IsRootNode)
					imageUrl = GetNodeIconUrl (imageStyle.RootIcon);
				else if (imageStyle.ParentIcon != null && node.IsParentNode)
					imageUrl = GetNodeIconUrl (imageStyle.ParentIcon);
				else if (imageStyle.LeafIcon != null && node.IsLeafNode)
					imageUrl = GetNodeIconUrl (imageStyle.LeafIcon);
			}
			
			if (imageUrl != "") {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
				BeginNodeTag (writer, node, clientExpand);
				writer.AddAttribute ("src", imageUrl);
				writer.AddAttribute ("border", "0");
				if (node.ImageToolTip != "") writer.AddAttribute ("alt", node.ImageToolTip);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				writer.RenderEndTag ();	// style tag
				writer.RenderEndTag ();	// TD
			}
			
			// Checkbox
			
			bool showChecks;
			if (node.IsShowCheckBoxSet)
				showChecks = node.ShowCheckBox;
			else
				showChecks = (ShowCheckBoxes == TreeNodeTypes.All) ||
							 (ShowCheckBoxes == TreeNodeTypes.Leaf && node.ChildNodes.Count == 0) ||
							 (ShowCheckBoxes == TreeNodeTypes.Parent && node.ChildNodes.Count > 0 && node.Parent != null) ||
							 (ShowCheckBoxes == TreeNodeTypes.Root && node.Parent == null && node.ChildNodes.Count > 0);

			if (showChecks) {
				AddNodeStyle (writer, node, level);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
				writer.AddAttribute ("name", ClientID + "_cs_" + node.Path);
				writer.AddAttribute ("type", "checkbox");
				if (node.Checked) writer.AddAttribute ("checked", "checked");
				writer.RenderBeginTag (HtmlTextWriterTag.Input);	// INPUT
				writer.RenderEndTag ();	// INPUT
				writer.RenderEndTag ();	// TD
			}
			
			// Text
			
			if (!NodeWrap)
				writer.AddAttribute ("nowrap", "nowrap");
			writer.RenderBeginTag (HtmlTextWriterTag.Td);	// TD
			
			AddNodeStyle (writer, node, level);
			if (clientExpand)
				writer.AddAttribute ("id", GetNodeClientId (node, "txt"));
			BeginNodeTag (writer, node, clientExpand);
			writer.Write (node.Text);
			writer.RenderEndTag ();	// style tag
			
			writer.RenderEndTag ();	// TD
			
			writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
			// Children
			
			if (hasChildNodes)
			{
				if (level >= levelLines.Count) {
					if (hasNext) levelLines.Add (this);
					else levelLines.Add (null);
				} else {
					if (hasNext) levelLines [level] = this;
					else levelLines [level] = null;
				}
				
				if (clientExpand) {
					if (!node.Expanded) writer.AddStyleAttribute ("display", "none");
					else writer.AddStyleAttribute ("display", "block");
					writer.AddAttribute ("id", GetNodeClientId (node, null));
					writer.RenderBeginTag (HtmlTextWriterTag.Span);
					
					if (renderChildNodes) {
						int num = node.ChildNodes.Count;
						for (int n=0; n<num; n++)
							RenderNode (writer, node.ChildNodes [n], level + 1, levelLines, true, n<num-1);
					}
					writer.RenderEndTag ();	// SPAN
				}
				else if (renderChildNodes) {
					int num = node.ChildNodes.Count;
					for (int n=0; n<num; n++)
						RenderNode (writer, node.ChildNodes [n], level + 1, levelLines, true, n<num-1);
				}
			}
		}
		
		void AddNodeStyle (HtmlTextWriter writer, TreeNode node, int level)
		{
			if (nodeStyle != null)
				nodeStyle.AddAttributesToRender (writer);
				
			if (levelStyles != null && level < levelStyles.Count)
				levelStyles [level].AddAttributesToRender (writer);
			else {
				if (rootNodeStyle != null && node.IsRootNode)
					rootNodeStyle.AddAttributesToRender (writer);
	
				if (leafNodeStyle != null && node.IsLeafNode)
					leafNodeStyle.AddAttributesToRender (writer);
	
				if (parentNodeStyle != null && node.IsParentNode)
					parentNodeStyle.AddAttributesToRender (writer);
			}
			
			if (node.Selected && selectedNodeStyle != null)
				selectedNodeStyle.AddAttributesToRender (writer);
		}
		
		void BeginNodeTag (HtmlTextWriter writer, TreeNode node, bool clientExpand)
		{
			if (node.NavigateUrl != "") {
				writer.AddAttribute ("href", node.NavigateUrl);
				if (node.Target != null)
					writer.AddAttribute ("target", node.Target);
				writer.AddStyleAttribute ("text-decoration", "none");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
			}
			else if (node.SelectAction != TreeNodeSelectAction.None) {
				if (node.SelectAction == TreeNodeSelectAction.Expand && clientExpand)
					writer.AddAttribute ("href", GetClientExpandEvent (node));
				else
					writer.AddAttribute ("href", GetClientEvent (node, "sel"));
				writer.AddStyleAttribute ("text-decoration", "none");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
			}
			else
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
		}
		
		bool HasChildInputData (TreeNode node)
		{
			// Returns true if this node contain childs whose state is hold in
			// input elements that are rendered together with the node.
			
			if (node.Checked) return true;
			if (!node.HasChildData) return false;

			foreach (TreeNode n in node.ChildNodes)
				if (HasChildInputData (n)) return true;
			return false;
		}
		
		string GetNodeImageToolTip (bool expand, string txt) {
			if (expand)  {
				if (ExpandImageToolTip != "")
					return ExpandImageToolTip;
				else if (txt != null)
					return "Expand " + txt;
				else
					return "Expand {0}";
			} else {
				if (CollapseImageToolTip != "")
					return CollapseImageToolTip;
				else if (txt != null)
					return "Collapse " + txt;
				else
					return "Collapse {0}";
			}
		}
		
		string GetNodeClientId (TreeNode node, string sufix)
		{
			return ClientID + "_" + node.Path + (sufix != null ? "_" + sufix : "");
		}
							
		string GetNodeImageUrl (string shape, ImageStyle imageStyle)
		{
			if (ShowLines) {
				if (LineImagesFolder != "")
					return LineImagesFolder + "/" + shape + ".gif";
			} else {
				if (shape == "plus") {
					if (ExpandImageUrl != "")
						return ExpandImageUrl;
					if (imageStyle != null && imageStyle.Expand != null)
						return imageStyle.Expand;
				}
				else if (shape == "minus") {
					if (CollapseImageUrl != "")
						return CollapseImageUrl;
					if (imageStyle != null && imageStyle.Collapse != null)
						return imageStyle.Collapse;
				}
				else if (shape == "noexpand") {
					if (NoExpandImageUrl != "")
						return NoExpandImageUrl;
					if (imageStyle != null && imageStyle.NoExpand != null)
						return imageStyle.NoExpand;
				}
			}
			return AssemblyResourceLoader.GetResourceUrl (typeof(TreeView), "TreeView_" + shape + ".gif");
		}
		
		string GetNodeIconUrl (string icon)
		{
			return AssemblyResourceLoader.GetResourceUrl (typeof(TreeView), icon + ".gif");
		}
		
		string GetClientEvent (TreeNode node, string ev)
		{
			return Page.GetPostBackClientHyperlink (this, ev + "|" + node.Path);
		}
		
		string GetClientExpandEvent (TreeNode node)
		{
			return "javascript:TreeView_ToggleExpand ('" + ClientID + "', '" + node.Path + "')";
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
				if (node.Checked) {
					string val = states [ClientID + "_cs_" + node.Path];
					if (val != "on") node.Checked = false;
				}
				if (node.HasChildData)
					UnsetCheckStates (node.ChildNodes, states);
			}
		}
		
		void SetCheckStates (NameValueCollection states)
		{
			string keyPrefix = ClientID + "_cs_";
			foreach (string key in states) {
				if (key.StartsWith (keyPrefix)) {
					string id = key.Substring (keyPrefix.Length);
					TreeNode node = FindNodeByPos (id);
					if (node != null)
						node.Checked = (Context.Request.Form [key] == "on");
				}
			}
		}
		
		void UnsetExpandStates (TreeNodeCollection col, string[] states)
		{
			foreach (TreeNode node in col) {
				if (node.Expanded) {
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
				if (id == null || id == "") continue;
				TreeNode node = FindNodeByPos (id);
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
			if (node.Expanded) {
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
