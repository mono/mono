//
// System.Windows.Forms.TreeNode.cs
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
// Autors:
//		Marek Safar		marek.safar@seznam.cz
//
//
//
//

// NOT COMPLETE

using System;
using System.Collections;

namespace System.Windows.Forms
{
	public class TreeNodeCollection: ICollection, IEnumerable, IList
	{
		TreeNode owner;

		ArrayList collection = new ArrayList ();

		internal TreeNodeCollection (TreeNode owner)
		{
			this.owner = owner;
		}

        [MonoTODO ("Check implementation")]
		public virtual int Add (TreeNode node)
		{
			if (node == null)
				throw new ArgumentNullException("value");

			if (node.Parent != null)
				throw new ArgumentException("Object already has a parent", "node");

			node.SetParent (owner);
			int index = collection.Add (node);
			node.SetIndex (index);
			return index;		
		}

		public virtual void AddRange (TreeNode[] nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			foreach (TreeNode node in nodes) {
				Add (node);
			}
		}

		public int Count {
			get {
				return collection.Count;
			}
		}

		[MonoTODO ("set")]
		public virtual TreeNode this [int index] {
			get {
				return (TreeNode)collection [index];
			}
			set {
				throw new NotImplementedException ();
			}
		}
	
		#region IEnumerable Members

		public IEnumerator GetEnumerator ()
		{
			return collection.GetEnumerator ();
		}

		#endregion
	
		#region ICollection Members

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		public void CopyTo(Array array, int index)
		{
			collection.CopyTo (array, index);
		}

		object ICollection.SyncRoot {
			get {
				return this;
			}
		}

		#endregion

		#region IList Members

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		[MonoTODO]
		object IList.this[int index] {
			get {
				return null;
			}
			set {
			}
		}

		public void RemoveAt (int index)
		{
			collection.RemoveAt (index);
		}

		[MonoTODO]
		void IList.Insert(int index, object value)
        {
		}

		[MonoTODO]
		void IList.Remove(object value)
		{
		}
                
		[MonoTODO]
		bool IList.Contains(object value)
		{
			return false;
		}

		public void Clear()
		{
			collection.Clear ();
		}

		[MonoTODO]
		int IList.IndexOf(object value)
		{
			return 0;
		}

		[MonoTODO]
		int IList.Add(object value)
		{
			return 0;
		}

		public bool IsFixedSize {
			get {
				return false;
			}
		}

		#endregion
	}
}
