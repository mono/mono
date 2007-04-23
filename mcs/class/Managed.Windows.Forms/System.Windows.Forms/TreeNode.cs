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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Kazuki Oikawa (kazuki@panicode.com)

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;

namespace System.Windows.Forms {
	[TypeConverter(typeof(TreeNodeConverter))]
	[Serializable]
	public class TreeNode : MarshalByRefObject, ICloneable, ISerializable {
		#region Fields
		private TreeView tree_view;
		internal TreeNode parent;

		private string text;
		private int image_index = -1;
		private int selected_image_index = -1;
        private string image_key = String.Empty;
        private string selected_image_key = String.Empty;
        internal TreeNodeCollection nodes;
		internal TreeViewAction check_reason = TreeViewAction.Unknown;

		internal int visible_order = 0;
		internal int width = -1;
		
		internal bool is_expanded = false;
		private bool check;
		internal OwnerDrawPropertyBag prop_bag;

		private object tag;

		internal IntPtr handle;
		
#if NET_2_0
		private string name = string.Empty;
#endif
		#endregion	// Fields

		#region Internal Constructors		
		internal TreeNode (TreeView tree_view) : this ()
		{
			this.tree_view = tree_view;
			is_expanded = true;
		}

		private TreeNode (SerializationInfo info, StreamingContext context) : this ()
		{
			SerializationInfoEnumerator	en;
			SerializationEntry		e;
			int				children;

			en = info.GetEnumerator();
			children = 0;
			while (en.MoveNext()) { 
				e = en.Current;
				switch(e.Name) {
					case "Text": Text = (string)e.Value; break;
					case "PropBag": prop_bag = (OwnerDrawPropertyBag)e.Value; break;
					case "ImageIndex": image_index = (int)e.Value; break;
					case "SelectedImageIndex": selected_image_index = (int)e.Value; break;
					case "Tag": tag = e.Value; break;
					case "IsChecked": check = (bool)e.Value; break;
					case "ChildCount": children = (int)e.Value; break;
				}
			}
			if (children > 0) {
				for (int i = 0; i < children; i++) {
					TreeNode node = (TreeNode) info.GetValue ("children" + i, typeof (TreeNode));
					Nodes.Add (node);
				}
			}
		}
		#endregion	// Internal Constructors

		#region Public Constructors
		public TreeNode ()
		{
			nodes = new TreeNodeCollection (this);
		}

		public TreeNode (string text) : this ()
		{
			Text = text;
		}

		public TreeNode (string text, TreeNode [] children) : this (text)
		{
			Nodes.AddRange (children);
		}

		public TreeNode (string text, int image_index, int selected_image_index) : this (text)
		{
			this.image_index = image_index;
			this.selected_image_index = selected_image_index;
		}

		public TreeNode (string text, int image_index, int selected_image_index,
				TreeNode [] children) : this (text, image_index, selected_image_index)
		{
			Nodes.AddRange (children);
		}

		#endregion	// Public Constructors

		#region ICloneable Members
		public virtual object Clone()
		{
			TreeNode tn = new TreeNode (text, image_index, selected_image_index);
			if (nodes != null) {
				foreach (TreeNode child in nodes)
					tn.Nodes.Add ((TreeNode)child.Clone ());
			}
			tn.Tag = tag;
			tn.Checked = Checked;
			if (prop_bag != null)
				tn.prop_bag = OwnerDrawPropertyBag.Copy (prop_bag);
			return tn;	
		}

		#endregion	// ICloneable Members

		#region ISerializable Members
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("Text", Text);
			info.AddValue ("prop_bag", prop_bag, typeof (OwnerDrawPropertyBag));
			info.AddValue ("ImageIndex", ImageIndex);
			info.AddValue ("SelectedImageIndex", SelectedImageIndex);
			info.AddValue ("Tag", Tag);
			info.AddValue ("Checked", Checked);
			
			info.AddValue ("NumberOfChildren", Nodes.Count);
			for (int i = 0; i < Nodes.Count; i++)
				info.AddValue ("Child-" + i, Nodes [i], typeof (TreeNode));
		}
		#endregion	// ISerializable Members

		#region Public Instance Properties
		public Color BackColor {
			get { 
				if (prop_bag != null)
					return prop_bag.BackColor;
				if (TreeView != null)
					return TreeView.BackColor;
				return Color.Empty;
			}
			set { 
				if (prop_bag == null)
					prop_bag = new OwnerDrawPropertyBag ();
				prop_bag.BackColor = value;

				TreeView tree_view = TreeView;
				if (tree_view != null)
					tree_view.UpdateNode (this);
			}
		}

		public Rectangle Bounds {
			get {
				if (TreeView == null)
					return Rectangle.Empty;

				int x = GetX ();
				int y = GetY ();
				
				if (width == -1)
					width = TreeView.GetNodeWidth (this);

				Rectangle res = new Rectangle (x, y, width, TreeView.ActualItemHeight);
				return res;
			}
		}

		internal int GetY ()
		{
			if (TreeView == null)
				return 0;
			return (visible_order - 1) * TreeView.ActualItemHeight - (TreeView.skipped_nodes * TreeView.ActualItemHeight);
		}

		internal int GetX ()
		{
			if (TreeView == null)
				return 0;
			int indent_level = IndentLevel;
			int roots = (TreeView.ShowRootLines ? 1 : 0);
			int cb = (TreeView.CheckBoxes ? 19 : 0);
			int imgs = (TreeView.ImageList != null ?  TreeView.ImageList.ImageSize.Width + 3 : 0);
			return ((indent_level + roots) * TreeView.Indent) + cb + imgs - TreeView.hbar_offset;
		}

		internal int GetLinesX ()
		{
			int roots = (TreeView.ShowRootLines ? 1 : 0);
			return (IndentLevel + roots) * TreeView.Indent - TreeView.hbar_offset;
		}

		internal int GetImageX ()
		{
			return GetLinesX () + (TreeView.CheckBoxes ? 19 : 0);
		}

		// In theory we should be able to track this instead of computing
		// every single time we need it, however for now I am going to
		// do it this way to reduce bugs in my new bounds computing code
		internal int IndentLevel {
			get {
				TreeNode walk = this;
				int res = 0;
				while (walk.Parent != null) {
					walk = walk.Parent;
					res++;
				}

				return res;
			}
		}

		public bool Checked {
			get { return check; }
			set {
				if (check == value)
					return;
			        TreeViewCancelEventArgs args = new TreeViewCancelEventArgs (this, false, check_reason);
				if (TreeView != null)
					TreeView.OnBeforeCheck (args);
				if (!args.Cancel) {
					check = value;
					if (TreeView != null) {
						TreeView.OnAfterCheck (new TreeViewEventArgs (this, check_reason));
						TreeView.UpdateNode (this);
					}
				}
				check_reason = TreeViewAction.Unknown;
			}
		}

		public TreeNode FirstNode {
			get {
				if (nodes.Count > 0)
					return nodes [0];
				return null;
			}
		}

		public Color ForeColor {
			get {
				if (prop_bag != null)
					return prop_bag.ForeColor;
				if (TreeView != null)
					return TreeView.ForeColor;
				return Color.Empty;
			}
			set {
				if (prop_bag == null)
					prop_bag = new OwnerDrawPropertyBag ();
				prop_bag.ForeColor = value;

				TreeView tree_view = TreeView;
				if (tree_view != null)
					tree_view.UpdateNode (this);
			}
		}

		public string FullPath {
			get {
				if (TreeView == null)
#if NET_2_0
					throw new InvalidOperationException ("No TreeView associated");
#else
					throw new Exception ("No TreeView associated");
#endif

				StringBuilder builder = new StringBuilder ();
				BuildFullPath (builder);
				return builder.ToString ();
			}
		}

		[Localizable(true)]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (image_index == value)
					return;
				image_index = value;
				TreeView tree = TreeView;
				if (tree != null)
					tree.UpdateNode (this);
			}
		}

#if NET_2_0
        [Localizable(true)]
        public string ImageKey
        {
            get { return image_key; }
            set
            {
                if (image_key == value)
                    return;
                image_key = value;
                TreeView tree = TreeView;
                if (tree != null)
                    tree.UpdateNode(this);
            }
        }
#endif

        public bool IsEditing {
			get {
				TreeView tv = TreeView;
				if (tv == null)
					return false;
				return tv.edit_node == this;
			}
		}

		public bool IsExpanded {
			get {
				TreeView tv = TreeView;

				if (tv != null && tv.IsHandleCreated) {
					// This is ridiculous
					bool found = false;
					foreach (TreeNode walk in TreeView.Nodes) {
						if (walk.Nodes.Count > 0)
							found = true;
					}

					if (!found)
						return false;
				}
					
				return is_expanded;
			}
		}

		public bool IsSelected {
			get {
				if (TreeView == null || !TreeView.IsHandleCreated)
					return false;
				return TreeView.SelectedNode == this;
			}
		}

		public bool IsVisible {
			get {
				if (TreeView == null || !TreeView.IsHandleCreated || !TreeView.Visible)
					return false;

				if (visible_order < TreeView.skipped_nodes || visible_order - TreeView.skipped_nodes > TreeView.VisibleCount)
					return false;

				TreeNode parent = Parent;
				while (parent != null) {
					if (!parent.is_expanded)
						return false;
					parent = parent.Parent;
				}
				return true;
			}
		}

		public TreeNode LastNode {
			get {
				return (nodes == null || nodes.Count == 0) ? null : nodes [nodes.Count - 1];
			}
		}

#if NET_2_0
		public string Name
		{
			get { return this.name; }
			set {
				// Value should never be null as per spec
				this.name = (value == null) ? string.Empty : value;
			}
		}
#endif

		public TreeNode NextNode {
			get {
				if (parent == null)
					return null;
				int index = Index;
				if (parent.Nodes.Count > index + 1)
					return parent.Nodes [index + 1];
				return null;
			}
		}
		
		public TreeNode NextVisibleNode {
			get {
				OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (this);
				if (!o.MoveNext ())
					return null;
				TreeNode c = o.CurrentNode;
				if (!c.IsInClippingRect)
					return null;
				return c;
			}
		}

		[Localizable(true)]
		public Font NodeFont {
			get {
				if (prop_bag != null)
					return prop_bag.Font;
				if (TreeView != null)
					return TreeView.Font;
				return null;
			}
			set {
				if (prop_bag == null)
					prop_bag = new OwnerDrawPropertyBag (); 
				prop_bag.Font = value;
				InvalidateWidth ();
			}
		}

		[ListBindable(false)]
		public TreeNodeCollection Nodes {
			get {
				if (nodes == null)
					nodes = new TreeNodeCollection (this);
				return nodes;
			}
		}

		public TreeNode Parent {
			get {
				TreeView tree_view = TreeView;
				if (tree_view != null && tree_view.root_node == parent)
					return null;
				return parent;
			}
		}

		public TreeNode PrevNode {
			get {
				if (parent == null)
					return null;
				int index = Index;
				if (index <= 0 || index > parent.Nodes.Count)
					return null;
				return parent.Nodes [index - 1];
			}
		}

		public TreeNode PrevVisibleNode {
			get {
				OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (this);
				if (!o.MovePrevious ())
					return null;
				TreeNode c = o.CurrentNode;
				if (!c.IsInClippingRect)
					return null;
				return c;
			}
		}

		[Localizable(true)]
		public int SelectedImageIndex {
			get { return selected_image_index; }
			set { selected_image_index = value; }
		}

#if NET_2_0
        [Localizable(true)]
        public string SelectedImageKey
        {
            get { return selected_image_key; }
            set { selected_image_key = value; }
        }
#endif

        [Bindable(true)]
		[Localizable(false)]
		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		[DefaultValue(null)]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[Localizable(true)]
		public string Text {
			get {
				if (text == null)
					return String.Empty;
				return text;
			}
			set {
				if (text == value)
					return;
				text = value;
				InvalidateWidth ();
			}
		}

		public TreeView TreeView {
			get {
				if (tree_view != null)
					return tree_view;
				TreeNode walk = parent;
				while (walk != null) {
					if (walk.TreeView != null)
						break;
					walk = walk.parent;
				}
				if (walk == null)
					return null;
				return walk.TreeView;
			}
		}

                public IntPtr Handle {
			get {
				// MS throws a NullReferenceException if the TreeView isn't set...
				if (handle == IntPtr.Zero && TreeView != null)
					handle = TreeView.CreateNodeHandle ();
				return handle;
			}
		}

		#endregion	// Public Instance Properties

		
		public static TreeNode FromHandle (TreeView tree, IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;
			// No arg checking on MS it just throws a NullRef if treeview is null
			return tree.NodeFromHandle (handle);
		}

		#region Public Instance Methods
		public void BeginEdit () {
			TreeView tv = TreeView;
			if (tv != null)
				tv.BeginEdit (this);
		}

		public void Collapse () {
			CollapseInternal (false);
		}

#if NET_2_0
		public void Collapse (bool ignore_children)
		{
			if (ignore_children)
				Collapse ();
			else
				CollapseRecursive (this);
		}
#endif
		public void EndEdit (bool cancel) {
			TreeView tv = TreeView;
			if (!cancel && tv != null)
				tv.EndEdit (this);
		}

		public void Expand () {
			Expand(false);
		}

		public void ExpandAll () {
			ExpandRecursive (this);
			if(TreeView != null)
				TreeView.UpdateNode (TreeView.root_node);
		}

		public void EnsureVisible ()
		{
			if (TreeView == null)
				return;

			if (this.Parent != null)
				ExpandParentRecursive (this.Parent);

			Rectangle bounds = Bounds;
			if (bounds.Y < 0) {
				TreeView.SetTop (this);
			} else if (bounds.Bottom > TreeView.ViewportRectangle.Bottom) {
				TreeView.SetBottom (this);
			}
		}

		public int GetNodeCount (bool include_subtrees) {
			if (!include_subtrees)
				return Nodes.Count;

			int count = 0;
			GetNodeCountRecursive (this, ref count);

			return count;
		}

		public void Remove () {
			if (parent == null)
				return;
			int index = Index;
			parent.Nodes.RemoveAt (index);
		}

		public void Toggle () {
			if (is_expanded)
				Collapse ();
			else
				Expand ();
		}

		public override String ToString () {
			return String.Concat ("TreeNode: ", Text);
		}

		#endregion	// Public Instance Methods

		#region Internal & Private Methods and Properties

		internal bool IsRoot {
			get {
				TreeView tree_view = TreeView;
				if (tree_view == null)
					return false;
				if (tree_view.root_node == this)
					return true;
				return false;
			}
		}

		bool BuildFullPath (StringBuilder path)
		{
			if (parent == null)
				return false;

			if (parent.BuildFullPath (path))
				path.Append (TreeView.PathSeparator);

			path.Append (text);
			return true;
		}

		public int Index {
			get {
				if (parent == null)
					return 0;
				return parent.Nodes.IndexOf (this);
			}
		}

		private void Expand (bool byInternal)
		{
			if (is_expanded || nodes.Count < 1) {
				is_expanded = true;
				return;
			}

			bool cancel = false;
			TreeView tree_view = TreeView;
			if (tree_view != null) {
				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (this, false, TreeViewAction.Expand);
				tree_view.OnBeforeExpand (e);
				cancel = e.Cancel;
			}

			if (!cancel) {
				is_expanded = true;
				int count_to_next = CountToNext ();

				if (tree_view != null) {
					tree_view.OnAfterExpand (new TreeViewEventArgs (this));

					tree_view.RecalculateVisibleOrder (this);
					tree_view.UpdateScrollBars (false);

					if (IsVisible)
						tree_view.ExpandBelow (this, count_to_next);
				}
			}

			
		}

		private void CollapseInternal (bool byInternal)
		{
			if (!is_expanded || nodes.Count < 1)
				return;

			if (IsRoot)
				return;

			bool cancel = false;
			TreeView tree_view = TreeView;
			if (tree_view != null) {
				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (this, false, TreeViewAction.Collapse);
				tree_view.OnBeforeCollapse (e);
				cancel = e.Cancel;
			}

			if (!cancel) {
				int count_to_next = CountToNext ();

				is_expanded = false;

				if (tree_view != null) {
					tree_view.OnAfterCollapse (new TreeViewEventArgs (this));

					tree_view.RecalculateVisibleOrder (this);
					tree_view.UpdateScrollBars (false);

					if (IsVisible)
						tree_view.CollapseBelow (this, count_to_next);
					if(!byInternal && HasFocusInChildren ())
						tree_view.SelectedNode = this;
				}
			}
		}

		private int CountToNext ()
		{
			bool expanded = is_expanded;
			is_expanded = false;
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (this);

			TreeNode next= null;
			if (walk.MoveNext () && walk.MoveNext ())
				next = walk.CurrentNode;

			is_expanded = expanded;
			walk.Reset ();
			walk.MoveNext ();

			int count = 0;
			while (walk.MoveNext () && walk.CurrentNode != next)
				count++;

			return count;
		}

		private bool HasFocusInChildren()
		{
			if(TreeView == null) return false;
			foreach (TreeNode node in nodes) {
				if(node == TreeView.SelectedNode)
					return true;
				if(node.HasFocusInChildren ())
					return true;
			}
			return false;
		}

		private void ExpandRecursive (TreeNode node)
		{
			node.Expand (true);
			foreach (TreeNode child in node.Nodes) {
				ExpandRecursive (child);
			}
		}

		private void ExpandParentRecursive (TreeNode node)
		{
			node.Expand (true);
			if (node.Parent != null)
				ExpandParentRecursive (node.Parent);
		}

		internal void CollapseAll ()
		{
			CollapseRecursive (this);
		}

		internal void CollapseAllUncheck ()
		{
			CollapseUncheckRecursive (this);
		}

		private void CollapseRecursive (TreeNode node)
		{
			node.Collapse ();
			foreach (TreeNode child in node.Nodes) {
				CollapseRecursive (child);
			}
		}

		private void CollapseUncheckRecursive (TreeNode node)
		{
			node.Collapse ();
			node.Checked = false;
			foreach (TreeNode child in node.Nodes) {
				CollapseUncheckRecursive (child);
			}
		}

		internal void SetNodes (TreeNodeCollection nodes)
		{
			this.nodes = nodes;
		}

		private void GetNodeCountRecursive (TreeNode node, ref int count)
		{
			count += node.Nodes.Count;
			foreach (TreeNode child in node.Nodes) {
				GetNodeCountRecursive (child, ref count);
			}
		}

		internal bool NeedsWidth {
			get { return width == -1; }
		}

		internal void InvalidateWidth ()
		{
			// bounds.Width = 0;
			width = -1;
		}

		internal void SetWidth (int width)
		{
			this.width = width;
		}

		internal void SetParent (TreeNode parent)
		{
			this.parent = parent;
		}

		private bool IsInClippingRect
		{
			get {
				if (TreeView == null)
					return false;
				Rectangle bounds = Bounds;
				if (bounds.Y < 0 && bounds.Y > TreeView.ClientRectangle.Height)
					return false;
				return true;
			}
		}
		#endregion	// Internal & Private Methods and Properties

	}
}


