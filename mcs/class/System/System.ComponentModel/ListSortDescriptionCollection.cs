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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//      Ivan N. Zlatev <contact@i-nz.net>
//


using System.Collections;

namespace System.ComponentModel {

	public class ListSortDescriptionCollection : IList, ICollection, IEnumerable {

		ArrayList list;

		public ListSortDescriptionCollection () {
			list = new ArrayList();
		}

		public ListSortDescriptionCollection (ListSortDescription[] sorts) {
			list = new ArrayList();
			foreach (ListSortDescription item in sorts)
				list.Add (item);
		}

		public int Count {
			get { return list.Count; }
		}

		public ListSortDescription this [int index] {
			get { return list[index] as ListSortDescription; }
			set { throw new InvalidOperationException("ListSortDescriptorCollection is read only."); }
		}

		public bool Contains (object value) {
			return list.Contains(value);
		}

		public void CopyTo (Array array, int index) {
			list.CopyTo(array, index);
		}

		public int IndexOf (object value) {
			return list.IndexOf(value);
		}

		object IList.this [int index] {
			get { return this[index]; }
			set { throw new InvalidOperationException("ListSortDescriptorCollection is read only."); }
		}

		bool IList.IsFixedSize {
			get { return list.IsFixedSize; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		bool IList.IsReadOnly {
			get { return list.IsReadOnly; }
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return list.GetEnumerator();
		}

		int IList.Add (object value) {
			return list.Add(value);
		}

		void IList.Clear () {
			list.Clear();
		}

		void IList.Insert (int index, object value) {
			list.Insert(index, value);
		}

		void IList.Remove (object value) {
			list.Remove(value);
		}

		void IList.RemoveAt (int index) {
			list.RemoveAt(index);
		}

	}

}
