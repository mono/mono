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
		private int index;

		private string text;
		private int image_index = -1;
		private int selected_image_index = -1;
		internal TreeNodeCollection nodes;
		
		private bool is_expanded = false;
		private Rectangle bounds = Rectangle.Empty;
		private Rectangle plus_minus_bounds = Rectangle.Empty;
		private Rectangle checkbox_bounds = Rectangle.Empty;
		private bool check;
		private bool is_editing;
		internal OwnerDrawPropertyBag prop_bag;

		private object tag;
		private IntPtr handle;
		#endregion	// Fields

		#region Internal Constructors		
		internal TreeNode (TreeView tree_view) : this ()
		{
			this.tree_view = tree_view;
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
		public object Clone()
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
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException();
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
			}
		}

		public Rectangle Bounds {
			get { return bounds; }
		}

		public bool Checked {
			get { return check; }
			set {
				if (check == value)
					return;
				check = value;

				if (TreeView != null)
					tree_view.UpdateNode (this);
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
			}
		}

		public string FullPath {
			get {
				if (tree_view == null)
					throw new Exception ("No TreeView associated");

				StringBuilder builder = new StringBuilder ();
				BuildFullPath (builder);
				return builder.ToString ();
			}
		}

		[Localizable(true)]
		public int ImageIndex {
			get { return image_index; }
			set { image_index = value; }
		}

		public bool IsEditing {
			get { return is_editing; }
		}

		public bool IsExpanded {
			get { return is_expanded; }
		}

		public bool IsSelected {
			get {
				if (TreeView == null)
					return false;
				return TreeView.SelectedNode == this;
			}
		}

		public bool IsVisible {
			get {
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

		public TreeNode LastNode {
			get {
				return (nodes == null || nodes.Count == 0) ? null : nodes [nodes.Count - 1];
			}
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
				if (tree_view != null && tree_view.root_node == parent)
					return null;
				return parent;
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

		[Localizable(true)]
		public int SelectedImageIndex {
			get { return selected_image_index; }
			set { selected_image_index = value; }
		}

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
				bounds.Width = 0;
			}
		}

		public TreeView TreeView {
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

		#endregion	// Public Instance Properties

		#region Public Static Methods
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public void BeginEdit () {
			is_editing = true;
		}

		public void Collapse () {
			Collapse(false);
		}

		public void EndEdit (bool cancel) {
			is_editing = false;
			if (!cancel && TreeView != null)
				text = TreeView.LabelEditText;
		}

		public void Expand () {
			Expand(false);
		}

		public void ExpandAll () {
			ExpandRecursive (this);
			if(TreeView != null)
				TreeView.Refresh();
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
			parent.Nodes.RemoveAt (Index);
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
		internal Rectangle PlusMinusBounds {
			get { return plus_minus_bounds; }
		}

		internal Rectangle CheckBoxBounds {
			get { return checkbox_bounds; }
		}

		bool BuildFullPath (StringBuilder path)
		{
			if (parent == null)
				return false;

			if (parent.BuildFullPath (path))
				path.Append (tree_view.PathSeparator);

			path.Append (text);
			return true;
		}

		public int Index {
			get { return index; }
		}

		private void Expand (bool byInternal)
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
				if (IsVisible && TreeView != null)
					TreeView.UpdateBelow (this);
			}
		}

		private void Collapse (bool byInternal)
		{
			if (!is_expanded)
				return;

			if (tree_view != null && tree_view.root_node == this)
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
				if (IsVisible && TreeView != null)
					TreeView.UpdateBelow (this);
				if(!byInternal && TreeView != null && HasFocusInChildren ())
					TreeView.SelectedNode = this;
			}
		}

		private bool HasFocusInChildren()
		{
			if(TreeView == null) return false;
			foreach(TreeNode node in nodes) {
				if(node == TreeView.SelectedNode) return true;
				if(node.HasFocusInChildren())
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

		internal void UpdateBounds (int x, int y, int width, int height)
		{
			bounds.X = x;
			bounds.Y = y;
			bounds.Width = width;
			bounds.Height = height;
		}

		internal void UpdatePlusMinusBounds (int x, int y, int width, int height)
		{
			plus_minus_bounds.X = x;
			plus_minus_bounds.Y = y;
			plus_minus_bounds.Width = width;
			plus_minus_bounds.Height = height;
		}

		internal void UpdateCheckBoxBounds (int x, int y, int width, int height)
		{
			checkbox_bounds.X = x;
			checkbox_bounds.Y = y;
			checkbox_bounds.Width = width;
			checkbox_bounds.Height = height;
		}

		internal void SetAddedData (TreeView tree_view, TreeNode parent, int index)
		{
			this.tree_view = tree_view;
			this.parent = parent;
			this.index = index;
		}

		internal void SetIndex (int index)
		{
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
		#endregion	// Internal & Private Methods and Properties

	}
}

