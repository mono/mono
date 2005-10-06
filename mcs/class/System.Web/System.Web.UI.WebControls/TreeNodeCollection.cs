//
// System.Web.UI.WebControls.TreeNodeCollection.cs
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
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public sealed class TreeNodeCollection: ICollection, IEnumerable, IStateManager
	{
		TreeNode[] originalItems;
		ArrayList items = new ArrayList ();
		TreeView tree;
		TreeNode parent;
		bool marked;
		bool dirty;
		
		public TreeNodeCollection ()
		{
		}
		
		public TreeNodeCollection (TreeNode owner)
		{
			this.parent = owner;
			this.tree = owner.Tree;
		}
		
		internal TreeNodeCollection (TreeView tree)
		{
			this.tree = tree;
		}
		
		internal void SetTree (TreeView tree)
		{
			this.tree = tree;
			foreach (TreeNode node in items)
				node.Tree = tree;
		}
		
		public TreeNode this [int i] {
			get { return (TreeNode) items [i]; }
		}
		
		public void Add (TreeNode child)
		{
			child.Index = items.Add (child);
			child.Tree = tree;
			child.SetParent (parent);
			if (marked) {
				child.TrackViewState ();
				child.SetDirty ();
				dirty = true;
			}
		}
		
		public void AddAt (int index, TreeNode child)
		{
			items.Insert (index, child);
			child.Index = index;
			child.Tree = tree;
			child.SetParent (parent);
			for (int n=index+1; n<items.Count; n++)
				((TreeNode)items[n]).Index = n;
			if (marked) {
				child.TrackViewState ();
				child.SetDirty ();
				dirty = true;
			}
		}
		
		public void Clear ()
		{
			if (tree != null || parent != null) {
				foreach (TreeNode nod in items) {
					nod.Tree = null;
					nod.SetParent (null);
				}
			}
			items.Clear ();
			dirty = true;
		}
		
		public bool Contains (TreeNode child)
		{
			return items.Contains (child);
		}
		
		public void CopyTo (TreeNode[] nodeArray, int index)
		{
			items.CopyTo (nodeArray, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		public int IndexOf (TreeNode node)
		{
			return items.IndexOf (node);
		}
		
		public void Remove (TreeNode node)
		{
			int i = IndexOf (node);
			if (i == -1) return;
			items.RemoveAt (i);
			if (tree != null)
				node.Tree = null;
			dirty = true;
		}
		
		public void RemoveAt (int index)
		{
			TreeNode node = (TreeNode) items [index];
			items.RemoveAt (index);
			if (tree != null)
				node.Tree = null;
			dirty = true;
		}
		
		public int Count {
			get { return items.Count; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return items; }
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			items.CopyTo (array, index);
		}

		void IStateManager.LoadViewState (object state)
		{
			if (state == null) return;
			object[] its = (object[]) state;
			
			dirty = (bool)its [0];
			
			if (dirty)
				items.Clear ();

			for (int n=1; n<its.Length; n++) {
				Pair pair = (Pair) its [n];
				int oi = (int) pair.First;
				TreeNode node;
				if (oi != -1) node = originalItems [oi];
				else node = new TreeNode ();
				if (dirty) Add (node);
				node.LoadViewState (pair.Second);
			}
		}
		
		object IStateManager.SaveViewState ()
		{
			object[] state = null;
			bool hasData = false;
			
			if (dirty) {
				state = new object [items.Count + 1];
				state [0] = true;
				for (int n=0; n<items.Count; n++) {
					TreeNode node = items[n] as TreeNode;
					int oi = Array.IndexOf (originalItems, node);
					object ns = node.SaveViewState ();
					if (ns != null) hasData = true;
					state [n + 1] = new Pair (oi, ns);
				}
			} else {
				ArrayList list = new ArrayList ();
				for (int n=0; n<items.Count; n++) {
					TreeNode node = items[n] as TreeNode;
					object ns = node.SaveViewState ();
					if (ns != null) {
						hasData = true;
						list.Add (new Pair (n, ns));
					}
				}
				if (hasData) {
					list.Insert (0, false);
					state = list.ToArray ();
				}
			}
			
			if (hasData)
				return state;
			else
				return null;
		}
		
		void IStateManager.TrackViewState ()
		{
			marked = true;
			originalItems = new TreeNode [items.Count];
			for (int n=0; n<items.Count; n++) {
				originalItems [n] = (TreeNode) items [n];
				originalItems [n].TrackViewState ();
			}
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return marked; }
		}
	}
}

#endif
