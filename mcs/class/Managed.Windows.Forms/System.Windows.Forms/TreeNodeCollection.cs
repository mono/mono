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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Windows.Forms {
	[Editor("System.Windows.Forms.Design.TreeNodeCollectionEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
	public class TreeNodeCollection : IList, ICollection, IEnumerable {

		private static readonly int OrigSize = 50;

		private TreeNode owner;
		private int count;
		private TreeNode [] nodes;

		private TreeNodeCollection ()
		{
		}

		internal TreeNodeCollection (TreeNode owner)
		{
			this.owner = owner;
			nodes = new TreeNode [OrigSize];
		}

#if !NET_2_0
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		[Browsable(false)]
		public int Count {
			get { return count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		object IList.this [int index] {
			get {
				return this [index];
			}
			set {
				if (!(value is TreeNode))
					throw new ArgumentException ("Parameter must be of type TreeNode.", "value");
				this [index] = (TreeNode) value;
			}
		}

		public virtual TreeNode this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				return nodes [index];
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				SetupNode (value);
				nodes [index] = value;
			}
		}

#if NET_2_0
		public virtual TreeNode this [string key] {
			get {
				for (int i = 0; i < count; i++)
					if (string.Compare (key, nodes[i].Name, true) == 0)
						return nodes[i];
						
				return null;
			}
		}
#endif

		bool UsingSorting {
			get {
				TreeView tv = owner == null ? null : owner.TreeView;
				return tv != null && (tv.Sorted || tv.TreeViewNodeSorter != null);
			}
		}

		public virtual TreeNode Add (string text)
		{
			TreeNode res = new TreeNode (text);
			Add (res);
			return res;
		}

		public virtual int Add (TreeNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			int res;
			TreeView tree_view = null;

			if (owner != null)
				tree_view = owner.TreeView;
				
			if (tree_view != null && UsingSorting) {
				res = AddSorted (node);
			} else {
				if (count >= nodes.Length)
					Grow ();
				nodes[count] = node;
				res = count;
				count++;
			}

			SetupNode (node);
#if NET_2_0
			// UIA Framework Event: Collection Changed
			if (tree_view != null)
				tree_view.OnUIACollectionChanged (owner, new CollectionChangeEventArgs (CollectionChangeAction.Add, node));
#endif
			return res;
		}

#if NET_2_0
		public virtual TreeNode Add (string key, string text)
		{
			TreeNode node = new TreeNode (text);
			node.Name = key;
			Add (node);
			return node;
		}

		public virtual TreeNode Add (string key, string text, int imageIndex)
		{
			TreeNode node = Add (key, text);
			node.ImageIndex = imageIndex;
			return node;
		}

		public virtual TreeNode Add (string key, string text, string imageKey)
		{
			TreeNode node = Add (key, text);
			node.ImageKey = imageKey;
			return node;

		}

		public virtual TreeNode Add (string key, string text, int imageIndex, int selectedImageIndex)
		{
			TreeNode node = Add (key, text);
			node.ImageIndex = imageIndex;
			node.SelectedImageIndex = selectedImageIndex;
			return node;
		}

		public virtual TreeNode Add (string key, string text, string imageKey, string selectedImageKey)
		{
			TreeNode node = Add (key, text);
			node.ImageKey = imageKey;
			node.SelectedImageKey = selectedImageKey;
			return node;
		}


#endif

		public virtual void AddRange (TreeNode [] nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			// We can't just use Array.Copy because the nodes also
			// need to have some properties set when they are added.
			for (int i = 0; i < nodes.Length; i++)
				Add (nodes [i]);
		}

		public virtual void Clear ()
		{
			while (count > 0)
				RemoveAt (0, false);
			
			Array.Clear (nodes, 0, count);
			count = 0;

			TreeView tree_view = null;
			if (owner != null) {
				tree_view = owner.TreeView;
				if (tree_view != null) {
					tree_view.UpdateBelow (owner);
					tree_view.RecalculateVisibleOrder (owner);
					tree_view.UpdateScrollBars (false);
				}
			}
		}

		public bool Contains (TreeNode node)
		{
			return Array.IndexOf (nodes, node, 0, count) != -1;
		}
#if NET_2_0
		public virtual bool ContainsKey (string key)
		{
			for (int i = 0; i < count; i++) {
				if (string.Compare (nodes [i].Name, key, true, CultureInfo.InvariantCulture) == 0)
					return true;
			}
			return false;
		}
#endif

		public void CopyTo (Array dest, int index)
		{
			Array.Copy (nodes, index, dest, index, count);
		}

		public IEnumerator GetEnumerator ()
		{
			return new TreeNodeEnumerator (this);
		}

		public int IndexOf (TreeNode node)
		{
			return Array.IndexOf (nodes, node);
		}

#if NET_2_0
		public virtual int IndexOfKey (string key)
		{
			for (int i = 0; i < count; i++) {
				if (string.Compare (nodes [i].Name, key, true, CultureInfo.InvariantCulture) == 0)
					return i;
			}
			return -1;
		}
		
		public virtual TreeNode Insert (int index, string text)
		{
			TreeNode node = new TreeNode (text);
			Insert (index, node);
			return node;
		}
#endif

		public virtual void Insert (int index, TreeNode node)
		{
			if (count >= nodes.Length)
				Grow ();

			Array.Copy (nodes, index, nodes, index + 1, count - index);
			nodes [index] = node;
			count++;

			// If we can use sorting, it means we have an owner *and* a TreeView
			if (UsingSorting)
				Sort (owner.TreeView.TreeViewNodeSorter);

			SetupNode (node);
		}

#if NET_2_0
		public virtual TreeNode Insert (int index, string key, string text)
		{
			TreeNode node = new TreeNode (text);
			node.Name = key;
			Insert (index, node);
			return node;
		}

		public virtual TreeNode Insert (int index, string key, string text, int imageIndex)
		{
			TreeNode node = new TreeNode (text);
			node.Name = key;
			node.ImageIndex = imageIndex;
			Insert (index, node);
			return node;
		}

		public virtual TreeNode Insert (int index, string key, string text, string imageKey)
		{
			TreeNode node = new TreeNode (text);
			node.Name = key;
			node.ImageKey = imageKey;
			Insert (index, node);
			return node;
		}

		public virtual TreeNode Insert (int index, string key, string text, int imageIndex, int selectedImageIndex)
		{
			TreeNode node = new TreeNode (text, imageIndex, selectedImageIndex);
			node.Name = key;
			Insert (index, node);
			return node;
		}

		public virtual TreeNode Insert (int index, string key, string text, string imageKey, string selectedImageKey)
		{
			TreeNode node = new TreeNode (text);
			node.Name = key;
			node.ImageKey = imageKey;
			node.SelectedImageKey = selectedImageKey;
			Insert (index, node);
			return node;
		}
#endif

		public void Remove (TreeNode node)
		{
			if (node == null)
				throw new NullReferenceException ();

			int index = IndexOf (node);
			if (index != -1)
				RemoveAt (index);
#if ONLY_1_1
			else
				throw new NullReferenceException ();
#endif
		}

		public virtual void RemoveAt (int index)
		{
			RemoveAt (index, true);
		}

		private void RemoveAt (int index, bool update)
		{
			TreeNode removed = nodes [index];
			TreeNode prev = GetPrevNode (removed);
			TreeNode new_selected = null;
			bool re_set_selected = false;
			bool visible = removed.IsVisible;

			TreeView tree_view = null;
			if (owner != null)
				tree_view = owner.TreeView;

			if (tree_view != null) {
				tree_view.RecalculateVisibleOrder (prev);

				if (removed == tree_view.SelectedNode) {
					re_set_selected = true;
					OpenTreeNodeEnumerator oe = new OpenTreeNodeEnumerator (removed);
					if (oe.MoveNext () && oe.MoveNext ()) {
						new_selected = oe.CurrentNode;
					} else {
						oe = new OpenTreeNodeEnumerator (removed);
						oe.MovePrevious ();
						new_selected = oe.CurrentNode == removed ? null : oe.CurrentNode;
					}
				}
			}

			Array.Copy (nodes, index + 1, nodes, index, count - index - 1);
			count--;
			
			nodes[count] = null;
			
			if (nodes.Length > OrigSize && nodes.Length > (count * 2))
				Shrink ();

			if (tree_view != null && re_set_selected) {
				tree_view.SelectedNode = new_selected;
			}

			TreeNode parent = removed.parent;
			removed.parent = null;

			if (update && tree_view != null && visible) {
				tree_view.RecalculateVisibleOrder (prev);
				tree_view.UpdateScrollBars (false);
				tree_view.UpdateBelow (parent);
			}
#if NET_2_0
			// UIA Framework Event: Collection Changed
			if (tree_view != null)
				tree_view.OnUIACollectionChanged (owner, new CollectionChangeEventArgs (CollectionChangeAction.Remove, removed));
#endif
		}

#if NET_2_0
		public virtual void RemoveByKey (string key)
		{
			TreeNode node = this[key];
			
			if (node != null)
				Remove (node);
		}
#endif

		private TreeNode GetPrevNode (TreeNode node)
		{
			OpenTreeNodeEnumerator one = new OpenTreeNodeEnumerator (node);

			if (one.MovePrevious () && one.MovePrevious ())
				return one.CurrentNode;
			return null;
		}

		private void SetupNode (TreeNode node)
		{
			// We used to remove this from the previous parent, but .Net
			// skips this step (even if setting the owner field).
			//node.Remove ();

			node.parent = owner;

			TreeView tree_view = null;
			if (owner != null)
				tree_view = owner.TreeView;

			if (tree_view != null) {
				// We may need to invalidate this entire node collection if sorted.
				TreeNode prev = UsingSorting ? owner : GetPrevNode (node);

				if (tree_view.IsHandleCreated && node.ArePreviousNodesExpanded)
					tree_view.RecalculateVisibleOrder (prev);
				if (owner == tree_view.root_node || node.Parent.IsVisible && node.Parent.IsExpanded)
					tree_view.UpdateScrollBars (false);

				tree_view.UpdateBelow (owner);
			}
		}

		int IList.Add (object node)
		{
			return Add ((TreeNode) node);
		}

		bool IList.Contains (object node)
		{
			return Contains ((TreeNode) node);
		}
		
		int IList.IndexOf (object node)
		{
			return IndexOf ((TreeNode) node);
		}

		void IList.Insert (int index, object node)
		{
			Insert (index, (TreeNode) node);
		}

		void IList.Remove (object node)
		{
			Remove ((TreeNode) node);
		}

		private int AddSorted (TreeNode node)
		{
			if (count >= nodes.Length)
				Grow ();

			TreeView tree_view = owner.TreeView;
			if (tree_view.TreeViewNodeSorter != null) { // Custom sorting
				nodes [count++] = node;
				Sort (tree_view.TreeViewNodeSorter);
				return count - 1;
			}

			CompareInfo compare = Application.CurrentCulture.CompareInfo;
			int pos = 0;
			bool found = false;
			for (int i = 0; i < count; i++) {
				pos = i;
				int comp = compare.Compare (node.Text, nodes [i].Text);
				if (comp < 0) {
					found = true;
					break;
				}
			}

			// Stick it at the end
			if (!found)
				pos = count;

			// Move the nodes up and adjust their indices
			for (int i = count - 1; i >= pos; i--) {
				nodes [i + 1] = nodes [i];
			}
			count++;
			nodes [pos] = node;

			return pos;
		}

		// Would be nice to do this without running through the collection twice
		internal void Sort (IComparer sorter) {
			Array.Sort (nodes, 0, count, sorter == null ? new TreeNodeComparer (Application.CurrentCulture.CompareInfo) : sorter);

			for (int i = 0; i < count; i++) {
				nodes [i].Nodes.Sort (sorter);
			}

			// Sorted may have been set to false even if TreeViewNodeSorter is being used.
			TreeView tv = owner == null ? null : owner.TreeView;
			if (tv != null)
				tv.sorted = true;
		}

		private void Grow ()
		{
			TreeNode [] nn = new TreeNode [nodes.Length + 50];
			Array.Copy (nodes, nn, nodes.Length);
			nodes = nn;
		}

		private void Shrink ()
		{
			int len = (count + 1 > OrigSize ? count + 1 : OrigSize);
			TreeNode [] nn = new TreeNode [len];
			Array.Copy (nodes, nn, count);
			nodes = nn;
		}

#if NET_2_0
		public TreeNode[] Find (string key, bool searchAllChildren)
		{
			List<TreeNode> results = new List<TreeNode> (0);
			Find (key, searchAllChildren, this, results);

			return results.ToArray ();             
		}
		
		private static void Find (string key, bool searchAllChildren, TreeNodeCollection nodes, List<TreeNode> results)
		{
			for (int i = 0; i < nodes.Count; i++) {
				TreeNode thisNode = nodes [i];
				
				if (string.Compare (thisNode.Name, key, true, CultureInfo.InvariantCulture) == 0) 
					results.Add (thisNode);

			}
			// Need to match the Microsoft order.

			if (searchAllChildren){
				for (int i = 0; i < nodes.Count; i++){
					TreeNodeCollection childNodes = nodes [i].Nodes;
					if (childNodes.Count > 0) {
						Find (key, searchAllChildren, childNodes, results);
					}
				}
			}
		}
#endif
		internal class TreeNodeEnumerator : IEnumerator {

			private TreeNodeCollection collection;
			private int index = -1;

			public TreeNodeEnumerator (TreeNodeCollection collection)
			{
				this.collection = collection;
			}

			public object Current {
				get {
					if (index == -1)
						return null;
					return collection [index];
				}
			}

			public bool MoveNext ()
			{
				if (index + 1 >= collection.Count)
					return false;
				index++;
				return true;
			}

			public void Reset ()
			{
				index = -1;
			}
		}

		private class TreeNodeComparer : IComparer {

			private CompareInfo compare;
		
			public TreeNodeComparer (CompareInfo compare)
			{
				this.compare = compare;
			}
		
			public int Compare (object x, object y)
			{
				TreeNode l = (TreeNode) x;
				TreeNode r = (TreeNode) y;
				int res = compare.Compare (l.Text, r.Text);

				return (res == 0 ? l.Index - r.Index : res);
			}
		}
	}
}

