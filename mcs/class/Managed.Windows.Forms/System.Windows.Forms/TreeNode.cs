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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

using System;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	[Serializable]
	public class TreeNode : MarshalByRefObject /*, ICloneable, ISerializable */ {

		private TreeView tree_view;
		internal TreeNode parent;
		private int index;

		private string text;
		private int image_index = -1;
		private int selected_image_index;
		private string full_path;
		internal TreeNodeCollection nodes;
		
		private bool is_expanded = true;
		private Rectangle bounds = Rectangle.Empty;
		private bool check;
		internal OwnerDrawPropertyBag prop_bag;

		private object tag;
		private IntPtr handle;
		
		internal TreeNode (TreeView tree_view) : this ()
		{
			this.tree_view = tree_view;
		}

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
			this.selected_image_index = image_index;
		}

		public TreeNode (string text, int image_index, int selected_image_index,
				TreeNode [] children) : this (text, image_index, selected_image_index)
		{
			Nodes.AddRange (children);
		}

		internal TreeView TreeView {
			get {
				if (tree_view != null)
					return tree_view;
				TreeNode walk = parent;
				while (walk != null) {
					if (walk.TreeView != null)
						tree_view = walk.TreeView;
					walk = walk.parent;
				}
				return tree_view;
			}
		}

		public TreeNode Parent {
			get {
				if (tree_view != null && tree_view.root_node == parent)
					return null;
				return parent;
			}
		}

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
				bounds.Width = 0;
			}
		}

		public Rectangle Bounds {
			get { return bounds; }
		}

		public bool Checked {
			get { return check; }
			set { check = value; }
		}

		public Color BackColor {
			get { return prop_bag.BackColor; }
			set { prop_bag.BackColor = value; }
		}

		public Color ForeColor {
			get { return prop_bag.ForeColor; }
			set { prop_bag.ForeColor = value; }
		}

		public Font NodeFont {
			get { return prop_bag.Font; }
			set { prop_bag.Font = value; }
		}

		public TreeNodeCollection Nodes {
			get {
				if (nodes == null)
					nodes = new TreeNodeCollection (this);
				return nodes;
			}
		}

		public TreeNode FirstNode {
			get {
				if (nodes.Count > 0)
					return nodes [0];
				return null;
			}
		}

		public string FullPath {
			get {
				if (full_path != null)
					return full_path;

				StringBuilder builder = new StringBuilder ();
				string ps = (TreeView == null ? "/" : TreeView.PathSeparator);
				for (int i = 0; i < nodes.Count; i++) {
					builder.Append (nodes [i].Text);
					if (i - 1 != nodes.Count)
						builder.Append (ps);
				}
				full_path = builder.ToString ();
				return full_path;
			}
		}

		public bool IsExpanded {
			get { return is_expanded; }
		}

		public TreeNode NextNode {
			get {
				if (parent == null)
					return null;
				if (parent.Nodes.Count > index + 1)
					return parent.Nodes [index + 1];
				return null;
			}
		}
		
		public TreeNode PrevNode {
			get {
				if (parent == null)
					return null;
				if (index == 0 || index > parent.Nodes.Count)
					return null;
				return parent.Nodes [index - 1];
			}
		}

		public TreeNode NextVisibleNode {
			get {
				OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (this);
				if (!o.MoveNext ())
					return null;
				TreeNode c = (TreeNode) o.Current;
				if (!c.IsInClippingRect)
					return null;
				return c;
			}
		}

		public TreeNode PrevVisibleNode {
			get {
				OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (this);
				if (!o.MovePrevious ())
					return null;
				TreeNode c = (TreeNode) o.Current;
				if (!c.IsInClippingRect)
					return null;
				return c;
			}
		}

		public TreeNode LastNode {
			get {
				return Nodes [Nodes.Count - 1];
			}
		}

		public int Index {
			get { return index; }
		}

		public int ImageIndex {
			get { return image_index; }
			set { image_index = value; }
		}

		public int SelectedImageIndex {
			get { return selected_image_index; }
			set { selected_image_index = value; }
		}

		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		public void Expand ()
		{
			if (is_expanded)
				return;

			bool cancel = false;
			if (TreeView != null) {
				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (this, false, TreeViewAction.Expand);
				TreeView.OnBeforeCollapse (e);
				cancel = e.Cancel;
			}

			if (!cancel) {
				is_expanded = true;
				if (TreeView != null)
					TreeView.OnAfterCollapse (new TreeViewEventArgs (this));
				if (IsNodeVisible () && TreeView != null)
					TreeView.UpdateBelow (this);
			}
		}

		public void Collapse ()
		{
			if (!is_expanded)
				return;

			bool cancel = false;
			if (TreeView != null) {
				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (this, false, TreeViewAction.Collapse);
				TreeView.OnBeforeCollapse (e);
				cancel = e.Cancel;
			}

			if (!cancel) {
				is_expanded = false;
				if (TreeView != null)
					TreeView.OnAfterCollapse (new TreeViewEventArgs (this));
				if (IsNodeVisible () && TreeView != null)
					TreeView.UpdateBelow (this);
			}
		}

		public void Remove ()
		{
			if (parent == null)
				return;
			parent.Nodes.RemoveAt (Index);
		}

		public void ExpandAll ()
		{
			ExpandRecursive (this);
		}

		private void ExpandRecursive (TreeNode node)
		{
			node.Expand ();
			foreach (TreeNode child in node.Nodes) {
				ExpandRecursive (child);
			}
		}

		internal void CollapseAll ()
		{
			CollapseRecursive (this);
		}

		private void CollapseRecursive (TreeNode node)
		{
			node.Collapse ();
			foreach (TreeNode child in node.Nodes) {
				CollapseRecursive (child);
			}
		}

		public int GetNodeCount (bool include_subtrees)
		{
			if (!include_subtrees)
				return Nodes.Count;

			int count = 0;
			GetNodeCountRecursive (this, ref count);

			return count;
		}

		public void Toggle ()
		{
			if (is_expanded)
				Collapse ();
			else
				Expand ();

			if (TreeView != null)
				TreeView.Refresh ();
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

		public override String ToString ()
		{
			return String.Concat ("TreeNode: ", Text);
		}

		internal void UpdateBounds (int x, int y, int width, int height)
		{
			bounds.X = x;
			bounds.Y = y;
			bounds.Width = width;
			bounds.Height = height;
		}

		internal void SetAddedData (TreeView tree_view, TreeNode parent, int index)
		{
			this.tree_view = tree_view;
			this.parent = parent;
			this.index = index;
		}

		private bool IsInClippingRect
		{
			get {
				if (TreeView == null)
					return false;
				if (bounds.Y < 0 && bounds.Y > tree_view.ClientRectangle.Height)
					return false;
				return true;
			}
		}

		private bool IsNodeVisible ()
		{
			if (TreeView == null)
				return false;

			if (bounds.Y < 0 && bounds.Y > TreeView.ClientRectangle.Height)
				return false;

			TreeNode parent = Parent;
			while (parent != null) {
				if (!parent.IsExpanded)
					return false;
				parent = parent.Parent;
			}
			return true;
		}
	}
}

