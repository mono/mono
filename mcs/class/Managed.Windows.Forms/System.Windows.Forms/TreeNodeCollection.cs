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

// TODO: Sorting

using System;
using System.Collections;
using System.Globalization;

namespace System.Windows.Forms {

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

		public virtual int Count {
			get { return count; }
		}

		public virtual bool IsReadOnly {
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
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");
				return nodes [index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");
				TreeNode node = (TreeNode) value;
				SetData (node);
				nodes [index] = node;
			}
		}

		public virtual TreeNode this [int index] {
			get {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");
				return nodes [index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");
				SetData (value);
				nodes [index] = value;
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

			if (owner != null && owner.TreeView != null && owner.TreeView.Sorted)
				return AddSorted (node);
			SetData (node);
			if (count >= nodes.Length)
				Grow ();
			nodes [count++] = node;

			if (owner.TreeView != null)
				owner.TreeView.Refresh ();
			return count;
		}

		public virtual void AddRange (TreeNode [] nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("node");

			// We can't just use Array.Copy because the nodes also
			// need to have some properties set when they are added.
			for (int i = 0; i < nodes.Length; i++)
				Add (nodes [i]);
		}

		public virtual void Clear ()
		{
			Array.Clear (nodes, 0, count);
			count = 0;
		}

		public bool Contains (TreeNode node)
		{
			return (Array.BinarySearch (nodes, node) > 0);
		}

		public virtual void CopyTo (Array dest, int index)
		{
			nodes.CopyTo (dest, index);
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return new TreeNodeEnumerator (this);
		}

		public int IndexOf (TreeNode node)
		{
			return Array.IndexOf (nodes, node);
		}

		public virtual void Insert (int index, TreeNode node)
		{
			SetData (node);
			IList list = (IList) nodes;
			list.Insert (index, node);
		}

		public virtual void Remove (TreeNode node)
		{
			int index = IndexOf (node);
			if (index > 0)
				RemoveAt (index);
		}

		public virtual void RemoveAt (int index)
		{
			Array.Copy (nodes, index, nodes, index - 1, count - index);
			count--;
			if (nodes.Length > OrigSize && nodes.Length > (count * 2))
				Shrink ();
			if (owner.TreeView != null)
				owner.TreeView.TotalNodeCount--;
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
				nodes [i + 1].SetIndex (i + 1);
			}
			count++;
			nodes [pos] = node;

			SetData (node, pos);
			return count;
		}

		// Would be nice to do this without running through the collection twice
		internal void Sort () {

			Array.Sort (nodes, 0, count, new TreeNodeComparer (Application.CurrentCulture.CompareInfo));

			for (int i = 0; i < count; i++) {
				nodes [i].SetIndex (i);
				nodes [i].Nodes.Sort ();
			}
		}

		private void SetData (TreeNode node)
		{
			SetData (node, count);
		}

		private void SetData (TreeNode node, int pos)
		{
			node.SetAddedData ((owner != null ? owner.TreeView : null), owner, pos);
		}

		private void Grow ()
		{
			TreeNode [] nn = new TreeNode [nodes.Length + 50];
			Array.Copy (nodes, nn, nodes.Length);
			nodes = nn;
		}

		private void Shrink ()
		{
			int len = (count > OrigSize ? count : OrigSize);
			TreeNode [] nn = new TreeNode [len];
			Array.Copy (nodes, nn, count);
			nodes = nn;
		}

		
		internal class TreeNodeEnumerator : IEnumerator {

			private TreeNodeCollection collection;
			private int index = -1;

			public TreeNodeEnumerator (TreeNodeCollection collection)
			{
				this.collection = collection;
			}

			public object Current {
				get { return collection [index]; }
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
				index = 0;
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

