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
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.Handlers;
using System.Collections.Specialized;

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
		TreeNode selectedNode;
		
		static string defaultExpandImage;
		static string defaultCollapseImage;
		static string defaultNoExpandImage;
		
		private static readonly object CheckChangedEvent = new object();
		private static readonly object SelectedNodeChangedEvent = new object();
		private static readonly object TreeNodeCollapsedEvent = new object();
		private static readonly object TreeNodeDataBoundEvent = new object();
		private static readonly object TreeNodeExpandedEvent = new object();
		private static readonly object TreeNodePopulateEvent = new object();
		
		static TreeView ()
		{
			defaultExpandImage = AssemblyResourceLoader.GetResourceUrl (typeof(TreeView), "TreeView_Default_Expand.gif");
			defaultCollapseImage = AssemblyResourceLoader.GetResourceUrl (typeof(TreeView), "TreeView_Default_Collapse.gif");
			defaultNoExpandImage = AssemblyResourceLoader.GetResourceUrl (typeof(TreeView), "TreeView_Default_NoExpand.gif");
		}
		
		public event TreeNodeEventHandler CheckChanged {
			add { Events.AddHandler (CheckChangedEvent, value); }
			remove { Events.RemoveHandler (CheckChangedEvent, value); }
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
		
		protected virtual void OnCheckChanged (TreeNodeEventArgs e)
		{
			if (Events != null) {
				TreeNodeEventHandler eh = (TreeNodeEventHandler) Events [CheckChangedEvent];
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
		
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Div; }
		}
		
		public TreeNode SelectedNode {
			get { return selectedNode; }
		}

		public string SelectedValue {
			get { return selectedNode != null ? selectedNode.Value : ""; }
		}

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
			OnCheckChanged (new TreeNodeEventArgs (node));
		}

		internal void NotifyExpandedChanged (TreeNode node)
		{
			if (node.Expanded)
				OnTreeNodeExpanded (new TreeNodeEventArgs (node));
			else
				OnTreeNodeCollapsed (new TreeNodeEventArgs (node));
		}

		protected override void TrackViewState()
		{
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
			if (nodes != null) {
				((IStateManager)nodes).TrackViewState();;
			}
		}

		protected override object SaveViewState()
		{
			object[] states = new object [9];
			states[0] = base.SaveViewState();
			states[1] = (hoverNodeStyle == null ? null : hoverNodeStyle.SaveViewState());
			states[2] = (leafNodeStyle == null ? null : leafNodeStyle.SaveViewState());
			states[3] = (levelStyles == null ? null : ((IStateManager)levelStyles).SaveViewState());
			states[4] = (nodeStyle == null ? null : nodeStyle.SaveViewState());
			states[5] = (parentNodeStyle == null ? null : parentNodeStyle.SaveViewState());
			states[6] = (rootNodeStyle == null ? null : rootNodeStyle.SaveViewState());
			states[7] = (selectedNodeStyle == null ? null : selectedNodeStyle.SaveViewState());
			states[8] = (nodes == null ? null : ((IStateManager)nodes).SaveViewState());

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
				((IStateManager)Nodes).LoadViewState(states[8]);
		}

		[MonoTODO]
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			string[] args = eventArgument.Split ('|');
			TreeNode node = FindNode (args[1]);
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
		
		[MonoTODO]
		string ICallbackEventHandler.RaiseCallbackEvent (string eventArgs)
		{
			Console.WriteLine ("RaiseCallbackEvent " + eventArgs);
			return "";
		}
		
		protected override void RenderContents (HtmlTextWriter writer)
		{
			foreach (TreeNode node in Nodes)
				RenderNode (writer, node, 0);
		}
		
		protected override void OnLoad (EventArgs e)
		{
			if (EnableClientScript && !Page.IsClientScriptBlockRegistered ("TreeView_ToggleExpand")) {
				string script = "<script language=javascript>\n<!--\nfunction TreeView_ToggleExpand (nodeId) {\n";
				script += "\tvar node = document.getElementById (nodeId);\n";
				script += "\tnode.style.display = (node.style.display == 'none') ? 'block' : 'none';\n";
				script += "}\n";
				script += "// -->\n</script>";
				Page.RegisterClientScriptBlock ("TreeView_ToggleExpand", script);
			}
			
			if (!Page.IsPostBack && nodes != null && ExpandDepth != 0) {
				foreach (TreeNode node in nodes)
					node.Expand (ExpandDepth - 1);
			}
		}
		
		void RenderNode (HtmlTextWriter writer, TreeNode node, int level)
		{
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			string nodeImage;
			
			if (node.ChildNodes.Count > 0) {
				if (node.Expanded)
					nodeImage = CollapseImageUrl != "" ? CollapseImageUrl : defaultCollapseImage;
				else
					nodeImage = ExpandImageUrl != "" ? ExpandImageUrl : defaultExpandImage;
			}
			else
				nodeImage = NoExpandImageUrl != "" ? NoExpandImageUrl : defaultNoExpandImage;
			
			bool clientExpand = EnableClientScript && Events [TreeNodeCollapsedEvent] == null && Events [TreeNodeExpandedEvent] == null;
			
			if (!clientExpand)
				writer.AddAttribute ("href", GetClientEvent (node, "ec"));
			else
				writer.AddAttribute ("href", "javascript:TreeView_ToggleExpand ('" + ClientID + "_" + GetNodePath (node) + "')");

			writer.RenderBeginTag (HtmlTextWriterTag.A);	// Anchor
			
			writer.AddAttribute ("src", nodeImage);
			writer.AddAttribute ("border", "0");
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();
			
			writer.RenderEndTag ();	// Anchor
			
			if (nodeStyle != null)
				nodeStyle.AddAttributesToRender (writer);
				
			if (levelStyles != null && level < levelStyles.Count)
				levelStyles [level].AddAttributesToRender (writer);
			else {
				if (rootNodeStyle != null && node.Parent == null && node.ChildNodes.Count > 0)
					rootNodeStyle.AddAttributesToRender (writer);
	
				if (leafNodeStyle != null && node.ChildNodes.Count == 0)
					leafNodeStyle.AddAttributesToRender (writer);
	
				if (parentNodeStyle != null && node.ChildNodes.Count > 0 && node.Parent != null)
					parentNodeStyle.AddAttributesToRender (writer);
			}
			
			if (node.Selected && selectedNodeStyle != null)
				selectedNodeStyle.AddAttributesToRender (writer);
			
			writer.AddAttribute ("href", GetClientEvent (node, "sel"));
			writer.AddStyleAttribute ("text-decoration", "none");
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			writer.Write (node.Text);
			writer.RenderEndTag ();
			
			if ((node.Expanded || clientExpand) && node.ChildNodes.Count > 0) {
			
				if (clientExpand) {
					if (!node.Expanded) writer.AddStyleAttribute ("display", "none");
					else writer.AddStyleAttribute ("display", "block");
					writer.AddAttribute ("id", ClientID + "_" + GetNodePath (node));
				}
				
				writer.AddStyleAttribute ("margin-left", NodeIndent.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				foreach (TreeNode child in node.ChildNodes)
					RenderNode (writer, child, level + 1);
				writer.RenderEndTag ();
			}
			writer.RenderEndTag ();
		}
		
		string GetClientEvent (TreeNode node, string ev)
		{
			return Page.GetPostBackClientHyperlink (this, ev + "|" + GetNodePath (node));
		}
		
		string GetNodePath (TreeNode node)
		{
			StringBuilder sb = new StringBuilder ();
			while (node != null) {
				if (sb.Length != 0) sb.Insert (0, '_');
				sb.Insert (0, node.Index.ToString ());
				node = node.Parent;
			}
			return sb.ToString ();
		}
		
		TreeNode FindNode (string path)
		{
			if (nodes == null) return null;
			
			string[] indexes = path.Split ('_');
			TreeNode node = null;
			
			foreach (string index in indexes) {
				int i = int.Parse (index);
				if (node == null) {
					if (i >= nodes.Count) return null;
					node = nodes [i];
				} else {
					if (i >= node.ChildNodes.Count) return null;
					node = node.ChildNodes [i];
				}
			}
			return node;
		}
	}
}

#endif
