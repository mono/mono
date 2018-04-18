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


using System;
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public sealed class TreeNodeCollection: ICollection, IEnumerable, IStateManager
	{
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
		
		public TreeNode this [int index] {
			get { return (TreeNode) items [index]; }
		}
		
		public void Add (TreeNode child)
		{
			Add (child, true);
		}
		
		internal void Add (TreeNode child, bool updateParent)
		{
			int index = items.Add (child);

			if (parent != null)
				parent.HadChildrenBeforePopulating = true;
			
			if (!updateParent)
				return;

			child.Index = index;
			child.SetParent (parent);
			child.Tree = tree;
			if (marked) {
				((IStateManager)child).TrackViewState ();
				SetDirty ();
			}
		}
		
		public void AddAt (int index, TreeNode child)
		{
			items.Insert (index, child);
			child.Index = index;
			child.SetParent (parent);
			child.Tree = tree;
			for (int n=index+1; n<items.Count; n++)
				((TreeNode)items[n]).Index = n;
			if (marked) {
				((IStateManager)child).TrackViewState ();
				SetDirty ();
			}
		}

		internal void SetDirty () {
			for (int n = 0; n < Count; n++)
				this [n].SetDirty ();
			dirty = true;
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
			if (marked) {
				dirty = true;
			}
		}
		
		public bool Contains (TreeNode c)
		{
			return items.Contains (c);
		}
		
		public void CopyTo (TreeNode[] nodeArray, int index)
		{
			items.CopyTo (nodeArray, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		public int IndexOf (TreeNode value)
		{
			return items.IndexOf (value);
		}
		
		public void Remove (TreeNode value)
		{
			int i = IndexOf (value);
			if (i == -1) return;
			items.RemoveAt (i);
			if (tree != null)
				value.Tree = null;
			if (marked) {
				SetDirty ();
			}
		}
		
		public void RemoveAt (int index)
		{
			TreeNode node = (TreeNode) items [index];
			items.RemoveAt (index);
			if (tree != null)
				node.Tree = null;
			if (marked) {
				SetDirty ();
			}
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
			
			if (dirty) {
				items.Clear ();

				for (int n = 1; n < its.Length; n++) {
					var pair = its [n] as Pair;
					if (pair == null)
						throw new InvalidOperationException ("Broken view state (item " + n + ")");
					
					TreeNode item;
					Type type = pair.First as Type;

					if (type == null)
						item = new TreeNode ();
					else
						item = Activator.CreateInstance (pair.First as Type) as TreeNode;
					Add (item);
					object ns = pair.Second;
					if (ns != null)
						((IStateManager) item).LoadViewState (ns);
				}
			}
			else {
				for (int n = 1; n < its.Length; n++) {
					var pair = its [n] as Pair;
					if (pair  == null)
						throw new InvalidOperationException ("Broken view state " + n + ")");
					
					int oi = (int) pair.First;
					TreeNode node = (TreeNode) items [oi];
					((IStateManager) node).LoadViewState (pair.Second);
				}
			}

		}
		
		object IStateManager.SaveViewState ()
		{
			object[] state = null;
			bool hasData = false;
			
			if (dirty) {
				if (items.Count > 0) {
					hasData = true;
					state = new object [items.Count + 1];
					state [0] = true;
					for (int n = 0; n < items.Count; n++) {
						TreeNode node = items [n] as TreeNode;
						object ns = ((IStateManager) node).SaveViewState ();
						Type type = node.GetType ();
						state [n + 1] = new Pair (type == typeof (TreeNode) ? null : type, ns);
					}
				}
			} else {
				ArrayList list = new ArrayList ();
				for (int n=0; n<items.Count; n++) {
					TreeNode node = items[n] as TreeNode;
					object ns = ((IStateManager)node).SaveViewState ();
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
			for (int n=0; n<items.Count; n++) {
				((IStateManager) items [n]).TrackViewState ();
			}
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return marked; }
		}
	}
}

