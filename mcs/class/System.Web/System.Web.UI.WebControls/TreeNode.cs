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

#if NET_2_0

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
		
		IHierarchyData hierarchyData;
		bool gotBinding;
		TreeNodeBinding binding;
		PropertyDescriptorCollection boundProperties;
		
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
		}
		
		internal TreeView Tree {
			get { return tree; }
			set {
				if (SelectedFlag) {
					if (value != null)
						value.SetSelectedNode (this);
					else if (tree != null)
						tree.SetSelectedNode (null);
				}
				tree = value;
				if (nodes != null)
					nodes.SetTree (tree);
				ResetPathData ();
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue (false)]
		[Browsable (false)]
		public bool DataBound {
			get { return hierarchyData != null; }
		}
		
		[DefaultValue (null)]
		[Browsable (false)]
		public object DataItem {
			get {
				if (hierarchyData == null) throw new InvalidOperationException ("TreeNode is not data bound.");
				return hierarchyData.Item;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue ("")]
		[Browsable (false)]
		public string DataPath {
			get {
				if (hierarchyData == null) throw new InvalidOperationException ("TreeNode is not data bound.");
				return hierarchyData.Path;
			}
		}
		
		[DefaultValue (false)]
		public virtual bool Checked {
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
		public virtual TreeNodeCollection ChildNodes {
			get {
				if (nodes == null) {
					if (PopulateOnDemand && tree == null)
						return null;

					if (DataBound)
						FillBoundChildren ();
					else
						nodes = new TreeNodeCollection (this);
						
					if (IsTrackingViewState)
						((IStateManager)nodes).TrackViewState();
					
					if (PopulateOnDemand && !Populated) {
						Populated = true;
						Populate ();
					}
				}
				return nodes;
			}
		}
		
		[DefaultValue (false)]
		public virtual bool Expanded {
			get {
				object o = ViewState ["Expanded"];
				if (o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Expanded"] = value;
				if (tree != null)
					tree.NotifyExpandedChanged (this);
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public virtual string ImageToolTip {
			get {
				object o = ViewState ["ImageToolTip"];
				if (o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ImageToolTipField != "")
							return (string) GetBoundPropertyValue (bin.ImageToolTipField);
						return bin.ImageToolTip;
					}
				}
				return "";
			}
			set {
				ViewState ["ImageToolTip"] = value;
			}
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string ImageUrl {
			get {
				object o = ViewState ["ImageUrl"];
				if (o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ImageUrlField != "")
							return (string) GetBoundPropertyValue (bin.ImageUrlField);
						return bin.ImageUrl;
					}
				}
				return "";
			}
			set {
				ViewState ["ImageUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.NavigateUrlField != "")
							return (string) GetBoundPropertyValue (bin.NavigateUrlField);
						return bin.NavigateUrl;
					}
				}
				return "";
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}
		}

		[DefaultValue (false)]
		public bool PopulateOnDemand {
			get {
				object o = ViewState ["PopulateOnDemand"];
				if (o != null) return (bool)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null)
						return bin.PopulateOnDemand;
				}
				return false;
			}
			set {
				ViewState ["PopulateOnDemand"] = value;
			}
		}

		[DefaultValue (TreeNodeSelectAction.Select)]
		public TreeNodeSelectAction SelectAction {
			get {
				object o = ViewState ["SelectAction"];
				if (o != null) return (TreeNodeSelectAction)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null)
						return bin.SelectAction;
				}
				return TreeNodeSelectAction.Select;
			}
			set {
				ViewState ["SelectAction"] = value;
			}
		}

		[DefaultValue (false)]
		public bool ShowCheckBox {
			get {
				object o = ViewState ["ShowCheckBox"];
				if (o != null) return (bool)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null)
						return bin.ShowCheckBox;
				}
				return false;
			}
			set {
				ViewState ["ShowCheckBox"] = value;
			}
		}
		
		internal bool IsShowCheckBoxSet {
			get { return ViewState ["ShowCheckBox"] != null; }
		}

		[DefaultValue ("")]
		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.TargetField != "")
							return (string) GetBoundPropertyValue (bin.TargetField);
						return bin.Target;
					}
				}
				return "";
			}
			set {
				ViewState ["Target"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("The display text of the tree node.")]
		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				if (o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						string text;
						if (bin.TextField != "")
							text = (string) GetBoundPropertyValue (bin.TextField);
						else if (bin.Text != "")
							text = bin.Text;
						else
							text = hierarchyData.ToString ();
							
						if (bin.FormatString.Length != 0)
							text = string.Format (bin.FormatString, text);
						return text;
					}
					return hierarchyData.ToString ();
				}
				return "";
			}
			set {
				ViewState ["Text"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public virtual string ToolTip {
			get {
				object o = ViewState ["ToolTip"];
				if(o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ToolTipField != "")
							return (string) GetBoundPropertyValue (bin.ToolTipField);
						return bin.ToolTip;
					}
				}
				return "";
			}
			set {
				ViewState ["ToolTip"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public virtual string Value {
			get {
				object o = ViewState ["Value"];
				if(o != null) return (string)o;
				if (DataBound) {
					TreeNodeBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ValueField != "")
							return (string) GetBoundPropertyValue (bin.ValueField);
						if (bin.Value != "")
							return bin.Value;
					}
					return hierarchyData.ToString ();
				}
				return "";
			}
			set {
				ViewState ["Value"] = value;
			}
		}
		
		[DefaultValue (false)]
		public virtual bool Selected {
			get {
				return SelectedFlag;
			}
			set {
				if (tree != null) {
					if (!value && tree.SelectedNode == this)
						tree.SetSelectedNode (null);
					else if (value)
						tree.SetSelectedNode (this);
				}
				else
					SelectedFlag = value;
			}
		}
		
		internal virtual bool SelectedFlag {
			get {
				object o = ViewState ["Selected"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Selected"] = value;
			}
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
		
		internal void SetParent (TreeNode node) {
			parent = node;
			ResetPathData ();
		}
		
		internal string Path {
			get {
				if (path != null) return path;
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
				if (o != null) return (bool) o;
				return false;
			}
			set {
				ViewState ["Populated"] = value;
			}
		}

		internal bool HasChildData {
			get { return nodes != null; }
		}
		
		protected virtual void Populate ()
		{
			tree.NotifyPopulateRequired (this);
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
			if (depth == 0) return;
			
			foreach (TreeNode nod in ChildNodes)
				nod.SetExpandedRec (expanded, depth - 1);
		}
		
		public void Select ()
		{
			Selected = true;
		}
		
		public void ToggleExpandState ()
		{
			Expanded = !Expanded;
		}

		public void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object[] states = (object[]) savedState;
			ViewState.LoadViewState (states [0]);
			
			if (tree != null && SelectedFlag)
				tree.SetSelectedNode (this);
			
			if (!PopulateOnDemand || Populated)
				((IStateManager)ChildNodes).LoadViewState (states [1]);
		}
		
		public object SaveViewState ()
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
		
		public void TrackViewState ()
		{
			if (marked) return;
			marked = true;
			ViewState.TrackViewState();

			if (nodes != null)
				((IStateManager)nodes).TrackViewState ();
		}
		
		public bool IsTrackingViewState
		{
			get { return marked; }
		}
		
		internal void SetDirty ()
		{
			ViewState.SetDirty ();
		}
		
		public object Clone ()
		{
			TreeNode nod = new TreeNode ();
			foreach (DictionaryEntry e in ViewState)
				nod.ViewState [(string)e.Key] = e.Value;
				
			foreach (TreeNode c in ChildNodes)
				nod.ChildNodes.Add ((TreeNode)c.Clone ());
				
			return nod;
		}
		
		internal void Bind (IHierarchyData hierarchyData)
		{
			this.hierarchyData = hierarchyData;
		}
		
		internal bool IsParentNode {
			get { return ChildNodes.Count > 0 && Parent != null; }
		}
		
		internal bool IsLeafNode {
			get { return ChildNodes.Count == 0; }
		}
		
		internal bool IsRootNode {
			get { return ChildNodes.Count > 0 && Parent == null; }
		}
		
		TreeNodeBinding GetBinding ()
		{
			if (tree == null) return null;
			if (gotBinding) return binding;
			binding = tree.FindBindingForNode (hierarchyData.Type, Depth);
			gotBinding = true;
			return binding;
		}
		
		object GetBoundPropertyValue (string name)
		{
			if (boundProperties == null) {
				ICustomTypeDescriptor desc = hierarchyData as ICustomTypeDescriptor;
				if (desc == null)
					throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
				boundProperties = desc.GetProperties ();
			}
			
			PropertyDescriptor prop = boundProperties.Find (name, true);
			if (prop == null)
				throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
			return prop.GetValue (hierarchyData);
		}

		void FillBoundChildren ()
		{
			nodes = new TreeNodeCollection (this);
			if (!hierarchyData.HasChildren) return;
			if (tree.MaxDataBindDepth != -1 && Depth >= tree.MaxDataBindDepth) return;

			IHierarchicalEnumerable e = hierarchyData.GetChildren ();
			foreach (object obj in e) {
				IHierarchyData hdata = e.GetHierarchyData (obj);
				TreeNode node = new TreeNode ();
				node.Bind (hdata);
				nodes.Add (node);
			}
		}
	}
}

#endif
