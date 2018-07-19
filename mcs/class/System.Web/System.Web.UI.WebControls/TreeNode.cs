//
// System.Web.UI.WebControls.TreeNode.cs
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


using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ParseChildrenAttribute (true, "ChildNodes")]
	public class TreeNode: IStateManager, ICloneable
	{
		StateBag ViewState = new StateBag ();
		TreeNodeCollection nodes;
		bool marked;
		TreeView tree;
		TreeNode parent;
		int index;
		string path;
		int depth = -1;
		
		object dataItem;
		IHierarchyData hierarchyData;

		bool gotBinding;
		TreeNodeBinding binding;
		PropertyDescriptorCollection boundProperties;
		bool populating;
		bool hadChildrenBeforePopulating;
		
		internal TreeNode (TreeView tree)
		{
			Tree = tree;
		}
		
		public TreeNode ()
		{
		}
		
		public TreeNode (string text)
		{
			Text = text;
		}
		
		public TreeNode (string text, string value)
		{
			Text = text;
			Value = value;
		}
		
		public TreeNode (string text, string value, string imageUrl)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
		}
		
		public TreeNode (string text, string value, string imageUrl, string navigateUrl, string target)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
			NavigateUrl = navigateUrl;
			Target = target;
		}
		
		[MonoTODO ("Not implemented")]
		protected TreeNode (TreeView owner, bool isRoot)
		{
			throw new NotImplementedException ();
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public int Depth {
			get {
				if (depth != -1) return depth;
				depth = 0;
				TreeNode nod = parent;
				while (nod != null) {
					depth++;
					nod = nod.parent;
				}
				return depth;
			}
		}
		
		void ResetPathData ()
		{
			path = null;
			depth = -1;
			gotBinding = false;
			if (nodes != null) {
				foreach (TreeNode node in nodes)
					node.ResetPathData ();
			}
		}
		
		internal TreeView Tree {
			get { return tree; }
			set {
				if (SelectedFlag) {
					if (value != null)
						value.SetSelectedNode (this, false);
				}
				tree = value;
				if (nodes != null)
					nodes.SetTree (tree);
				ResetPathData ();
				if (PopulateOnDemand && !Populated && Expanded.HasValue && Expanded.Value)
					Populate ();
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue (false)]
		[Browsable (false)]
		public bool DataBound {
			get { return ViewState ["DataBound"] == null ? false : (bool) ViewState ["DataBound"]; }
			private set { ViewState ["DataBound"] = value; }
		}
		
		[DefaultValue (null)]
		[Browsable (false)]
		public object DataItem {
			get { return dataItem; }
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue ("")]
		[Browsable (false)]
		public string DataPath {
			get { return ViewState ["DataPath"] == null ? String.Empty : (String) ViewState ["DataPath"]; }
			private set { ViewState ["DataPath"] = value; }
		}
		
		[DefaultValue (false)]
		public bool Checked {
			get {
				object o = ViewState ["Checked"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Checked"] = value;
				if (tree != null)
					tree.NotifyCheckChanged (this);
			}
		}

		[DefaultValue (null)]
		[MergableProperty (false)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		public TreeNodeCollection ChildNodes {
			get {
				if (nodes == null) {
					nodes = new TreeNodeCollection (this);
						
					if (IsTrackingViewState)
						((IStateManager)nodes).TrackViewState();
				}
				return nodes;
			}
		}
		
		[DefaultValue (null)]
		public bool? Expanded {
			get {
				object o = ViewState ["Expanded"];
				return (bool?)o;
			}
			set {
				bool? current = (bool?) ViewState ["Expanded"];
				if (current == value)
					return;
				ViewState ["Expanded"] = value;
				if (tree != null)
					tree.NotifyExpandedChanged (this);
				if (PopulateOnDemand && !Populated && value.HasValue && value.Value)
					Populate ();
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string ImageToolTip {
			get {
				object o = ViewState ["ImageToolTip"];
				if (o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["ImageToolTip"] = value; }
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get {
				object o = ViewState ["ImageUrl"];
				if (o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["ImageUrl"] = value; }
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["NavigateUrl"] = value; }
		}

		internal bool HadChildrenBeforePopulating {
			get { return hadChildrenBeforePopulating; }
			set {
				if (populating)
					return;

				hadChildrenBeforePopulating = value;
			}
		}
		
		[DefaultValue (false)]
		public bool PopulateOnDemand {
			get {
				object o = ViewState ["PopulateOnDemand"];
				if (o != null)
					return (bool)o;
				return false;
			}
			set {
				ViewState ["PopulateOnDemand"] = value;
				if (value && nodes != null && nodes.Count > 0)
					HadChildrenBeforePopulating = true;
				else
					HadChildrenBeforePopulating = false;
			}
		}

		[DefaultValue (TreeNodeSelectAction.Select)]
		public TreeNodeSelectAction SelectAction {
			get {
				object o = ViewState ["SelectAction"];
				if (o != null)
					return (TreeNodeSelectAction)o;
				return TreeNodeSelectAction.Select;
			}
			set { ViewState ["SelectAction"] = value; }
		}

		[DefaultValue (null)]
		public bool? ShowCheckBox {
			get {
				object o = ViewState ["ShowCheckBox"];
				return (bool?)o;
			}
			set { ViewState ["ShowCheckBox"] = value; }
		}

		internal bool ShowCheckBoxInternal {
			get {
				if (ShowCheckBox.HasValue)
					return ShowCheckBox.Value;
				else
					return (Tree.ShowCheckBoxes == TreeNodeTypes.All) ||
						 ((Tree.ShowCheckBoxes & TreeNodeTypes.Leaf) > 0 && IsLeafNode) ||
						 ((Tree.ShowCheckBoxes & TreeNodeTypes.Parent) > 0 && IsParentNode && Parent != null) ||
						 ((Tree.ShowCheckBoxes & TreeNodeTypes.Root) > 0 && Parent == null && ChildNodes.Count > 0);
			}
		}
		
		[DefaultValue ("")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["Target"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("The display text of the tree node.")]
		public string Text {
			get {
				object o = ViewState ["Text"];
				if (o == null)
					o = ViewState ["Value"];
				if (o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["Text"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string ToolTip {
			get {
				object o = ViewState ["ToolTip"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["ToolTip"] = value; }
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string Value {
			get {
				object o = ViewState ["Value"];
				if (o == null)
					o = ViewState ["Text"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set { ViewState ["Value"] = value; }
		}
		
		[DefaultValue (false)]
		public bool Selected {
			get { return SelectedFlag; }
			set {
				SelectedFlag = value;
				
				if (tree != null) {
					if (!value && tree.SelectedNode == this)
						tree.SetSelectedNode (null, false);
					else if (value)
						tree.SetSelectedNode (this, false);
				}
			}
		}
		
		internal virtual bool SelectedFlag {
			get {
				object o = ViewState ["Selected"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set { ViewState ["Selected"] = value; }
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public TreeNode Parent {
			get { return parent; }
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string ValuePath {
			get {
				if (tree == null) return Value;
				
				StringBuilder sb = new StringBuilder (Value);
				TreeNode node = parent;
				while (node != null) {
					sb.Insert (0, tree.PathSeparator);
					sb.Insert (0, node.Value);
					node = node.Parent;
				}
				return sb.ToString ();
			}
		}
		
		internal int Index {
			get { return index; }
			set { index = value; ResetPathData (); }
		}
		
		internal void SetParent (TreeNode node)
		{
			parent = node;
			ResetPathData ();
		}
		
		internal string Path {
			get {
				if (path != null)
					return path;
				StringBuilder sb = new StringBuilder (index.ToString());
				TreeNode node = parent;
				while (node != null) {
					sb.Insert (0, '_');
					sb.Insert (0, node.Index.ToString ());
					node = node.Parent;
				}
				path = sb.ToString ();
				return path;
			}
		}
		
		internal bool Populated {
			get {
				object o = ViewState ["Populated"];
				if (o != null)
					return (bool) o;
				return false;
			}
			set { ViewState ["Populated"] = value; }
		}

		internal bool HasChildData {
			get { return nodes != null; }
		}
		
		internal void Populate ()
		{
			if (tree == null)
				return;

			populating = true;
			tree.NotifyPopulateRequired (this);
			populating = false;
			Populated = true;
		}
		
		public void Collapse ()
		{
			Expanded = false;
		}

		public void CollapseAll ()
		{
			SetExpandedRec (false, -1);
		}

		public void Expand ()
		{
			Expanded = true;
		}

		internal void Expand (int depth)
		{
			SetExpandedRec (true, depth);
		}

		public void ExpandAll ()
		{
			SetExpandedRec (true, -1);
		}
		
		void SetExpandedRec (bool expanded, int depth)
		{
			Expanded = expanded;
			if (depth == 0)
				return;
			
			foreach (TreeNode nod in ChildNodes)
				nod.SetExpandedRec (expanded, depth - 1);
		}
		
		public void Select ()
		{
			Selected = true;
		}
		
		public void ToggleExpandState ()
		{
			Expanded = !Expanded.GetValueOrDefault(false);
		}

		void IStateManager.LoadViewState (object state)
		{
			LoadViewState (state);
		}

		protected virtual void LoadViewState (object state)
		{
			if (state == null)
				return;

			object[] states = (object[]) state;
			ViewState.LoadViewState (states [0]);
			
			if (tree != null && SelectedFlag)
				tree.SetSelectedNode (this, true);
			
			if (!PopulateOnDemand || Populated)
				((IStateManager)ChildNodes).LoadViewState (states [1]);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}

		protected virtual object SaveViewState ()
		{
			object[] states = new object[2];
			states[0] = ViewState.SaveViewState();
			states[1] = (nodes == null ? null : ((IStateManager)nodes).SaveViewState());
			
			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;
		}

		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}

		protected void TrackViewState ()
		{
			if (marked) return;
			marked = true;
			ViewState.TrackViewState();

			if (nodes != null)
				((IStateManager)nodes).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		protected bool IsTrackingViewState {
			get { return marked; }
		}
		
		internal void SetDirty ()
		{
			ViewState.SetDirty (true);
			if (nodes != null)
				nodes.SetDirty ();
		}
		
		public virtual object Clone ()
		{
			TreeNode nod = tree != null ? tree.CreateNode () : new TreeNode ();
			foreach (DictionaryEntry e in ViewState)
				nod.ViewState [(string)e.Key] = ((StateItem)e.Value).Value;
				
			foreach (TreeNode c in ChildNodes)
				nod.ChildNodes.Add ((TreeNode)c.Clone ());
				
			return nod;
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}
		
		internal void Bind (IHierarchyData hierarchyData)
		{
			this.hierarchyData = hierarchyData;
			DataBound = true;
			DataPath = hierarchyData.Path;
			dataItem = hierarchyData.Item;
			
			TreeNodeBinding bin = GetBinding ();
			if (bin != null) {
			
				// Bind ImageToolTip property

				if (bin.ImageToolTipField.Length > 0) {
					ImageToolTip = Convert.ToString (GetBoundPropertyValue (bin.ImageToolTipField));
					if (ImageToolTip.Length == 0)
						ImageToolTip = bin.ImageToolTip;
				} else if (bin.ImageToolTip.Length > 0)
					ImageToolTip = bin.ImageToolTip;
					
				// Bind ImageUrl property

				if (bin.ImageUrlField.Length > 0) {
					ImageUrl = Convert.ToString (GetBoundPropertyValue (bin.ImageUrlField));
					if (ImageUrl.Length == 0)
						ImageUrl = bin.ImageUrl;
				} else if (bin.ImageUrl.Length > 0)
					ImageUrl = bin.ImageUrl;
					
				// Bind NavigateUrl property

				if (bin.NavigateUrlField.Length > 0) {
					NavigateUrl = Convert.ToString (GetBoundPropertyValue (bin.NavigateUrlField));
					if (NavigateUrl.Length == 0)
						NavigateUrl = bin.NavigateUrl;
				} else if (bin.NavigateUrl.Length > 0)
					NavigateUrl = bin.NavigateUrl;
					
				// Bind PopulateOnDemand property
				
				if (bin.HasPropertyValue ("PopulateOnDemand"))
					PopulateOnDemand = bin.PopulateOnDemand;
				
				// Bind SelectAction property
					
				if (bin.HasPropertyValue ("SelectAction"))
					SelectAction = bin.SelectAction;
				
				// Bind ShowCheckBox property
					
				if (bin.HasPropertyValue ("ShowCheckBox"))
					ShowCheckBox = bin.ShowCheckBox;
					
				// Bind Target property

				if (bin.TargetField.Length > 0) {
					Target = Convert.ToString (GetBoundPropertyValue (bin.TargetField));
					if (Target.Length == 0)
						Target = bin.Target;
				} else if (bin.Target.Length > 0)
					Target = bin.Target;
					
				// Bind Text property
				string text = null;
				if (bin.TextField.Length > 0) {
					text = Convert.ToString (GetBoundPropertyValue (bin.TextField));
					if (bin.FormatString.Length > 0)
						text = string.Format (bin.FormatString, text);
				}
				if (String.IsNullOrEmpty (text)) {
					if (bin.Text.Length > 0)
						text = bin.Text;
					else if (bin.Value.Length > 0)
						text = bin.Value;
				}
				if (!String.IsNullOrEmpty (text))
					Text = text;
					
				// Bind ToolTip property

				if (bin.ToolTipField.Length > 0) {
					ToolTip = Convert.ToString (GetBoundPropertyValue (bin.ToolTipField));
					if (ToolTip.Length == 0)
						ToolTip = bin.ToolTip;
				} else if (bin.ToolTip.Length > 0)
					ToolTip = bin.ToolTip;
					
				// Bind Value property
				string value = null;
				if (bin.ValueField.Length > 0) {
					value = Convert.ToString (GetBoundPropertyValue (bin.ValueField));
				}
				if (String.IsNullOrEmpty (value)) {
					if (bin.Value.Length > 0)
						value = bin.Value;
					else if (bin.Text.Length > 0)
						value = bin.Text;
				}
				if (!String.IsNullOrEmpty (value))
					Value = value;
			} else {
				Text = Value = GetDefaultBoundText ();
			}

			INavigateUIData navigateUIData = hierarchyData as INavigateUIData;
			if (navigateUIData != null) {
				SelectAction = TreeNodeSelectAction.None;
				Text = navigateUIData.ToString ();
				NavigateUrl = navigateUIData.NavigateUrl;
				ToolTip = navigateUIData.Description;
			}
		}
		
		internal void SetDataItem (object item)
		{
			dataItem = item;
		}
		
		internal void SetDataPath (string path)
		{
			DataPath = path;
		}
		
		internal void SetDataBound (bool bound)
		{
			DataBound = bound;
		}
		
		string GetDefaultBoundText ()
		{
			if (hierarchyData != null)
				return hierarchyData.ToString ();
			else if (dataItem != null)
				return dataItem.ToString ();
			else
				return string.Empty;
		}
		
		string GetDataItemType ()
		{
			if (hierarchyData != null)
				return hierarchyData.Type;
			else if (dataItem != null)
				return dataItem.GetType().ToString ();
			else
				return string.Empty;
		}
				
		internal bool IsParentNode {
			get { return ChildNodes.Count > 0 || (PopulateOnDemand && !Populated); }
		}
		
		internal bool IsLeafNode {
			get { return !IsParentNode; }
		}
		
		internal bool IsRootNode {
			get { return Depth == 0; }
		}
		
		TreeNodeBinding GetBinding ()
		{
			if (tree == null)
				return null;
			if (gotBinding)
				return binding;
			binding = tree.FindBindingForNode (GetDataItemType (), Depth);
			gotBinding = true;
			return binding;
		}
		
		object GetBoundPropertyValue (string name)
		{
			if (boundProperties == null) {
				if (hierarchyData != null)
					boundProperties = TypeDescriptor.GetProperties (hierarchyData);
				else
					boundProperties = TypeDescriptor.GetProperties (dataItem);
			}
			
			PropertyDescriptor prop = boundProperties.Find (name, true);
			if (prop == null)
				throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
				
			if (hierarchyData != null)
				return prop.GetValue (hierarchyData);
			else
				return prop.GetValue (dataItem);
		}

		internal void BeginRenderText (HtmlTextWriter writer)
		{
			RenderPreText (writer);
		}
		
		internal void EndRenderText (HtmlTextWriter writer)
		{
			RenderPostText (writer);
		}
		
		protected virtual void RenderPreText (HtmlTextWriter writer)
		{
		}
		
		protected virtual void RenderPostText (HtmlTextWriter writer)
		{
		}
	}
}
