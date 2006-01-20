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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


#if NET_2_0

using System.Collections;

namespace System.Windows.Forms {

	public class DataGridViewSelectedRowCollection : BaseCollection, IList, ICollection, IEnumerable {

		public bool IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this[index]; }
			set { throw new NotSupportedException("Can't insert or modify this collection."); }
		}

		public DataGridViewRow this [int index] {
			get { return (DataGridViewRow) base.List[index]; }
		}

		public int Add (object o) {
			throw new NotSupportedException("Can't add elements to this collection.");
		}
		
		public void Clear () {
			throw new NotSupportedException("This collection cannot be cleared.");
		}

		public bool Contains (object o) {
			return Contains(o as DataGridViewRow);
		}

		public bool Contains (DataGridViewRow dataGridViewRow) {
			return base.List.Contains(dataGridViewRow);
		}

		public void CopyTo (DataGridViewRow[] array, int index) {
			base.List.CopyTo(array, index);
		}

		public int IndexOf (object o) {
			return base.List.IndexOf(o);
		}

		public void Insert (int index, object o) {
			Insert(index, o as DataGridViewRow);
		}

		public void Insert (int index, DataGridViewRow dataGridViewRow) {
			throw new NotSupportedException("Insert is not allowed.");
		}

		public void Remove (object o) {
			throw new NotSupportedException("Can't remove elements of this collection.");
		}

		public void RemoveAt (int index) {
			throw new NotSupportedException("Can't remove elements of this collection.");
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		internal void InternalAdd (DataGridViewRow dataGridViewRow) {
			base.List.Add(dataGridViewRow);
		}

		internal void InternalRemove (DataGridViewRow dataGridViewRow) {
			base.List.Remove(dataGridViewRow);
		}

	}

}

#endif
