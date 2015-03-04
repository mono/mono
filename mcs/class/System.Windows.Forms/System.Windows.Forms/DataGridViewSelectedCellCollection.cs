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

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ListBindable (false)]
	public class DataGridViewSelectedCellCollection : BaseCollection, IList, ICollection, IEnumerable
	{
		internal DataGridViewSelectedCellCollection ()
		{
		}

		bool IList.IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { throw new NotSupportedException(); }
		}

		public DataGridViewCell this [int index] {
			get { return (DataGridViewCell) base.List [index]; }
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		int IList.Add (object value)
		{
			throw new NotSupportedException ();
		}
		
		void IList.Clear ()
		{
			Clear ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Clear ()
		{
			throw new NotSupportedException ("Cannot clear this base.List");
		}

		bool IList.Contains (object value)
		{
			return Contains (value as DataGridViewCell);
		}

		public bool Contains (DataGridViewCell dataGridViewCell)
		{
			return base.List.Contains (dataGridViewCell);
		}

		public void CopyTo (DataGridViewCell [] array, int index)
		{
			base.List.CopyTo (array, index);
			/*
			if (array == null) {
				throw new ArgumentNullException("array is null");
			}
			if (index < 0) {
				throw new IndexOutOfRangeException("index is out of range");
			}
			if (index >= arrayl.Length) {
				throw new ArgumentException("index is equal or greater than the length of the array");
			}
			if ((array.Length - index) < base.List.Count) {
				throw new ArgumentException("not enought space for the elements from index to the end");
			}
			*/
		}

		int IList.IndexOf (object value)
		{
			return base.List.IndexOf (value as DataGridViewCell);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, value as DataGridViewCell);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Insert (int index, DataGridViewCell dataGridViewCell)
		{
			throw new NotSupportedException ("Can't insert to selected cell base.List");
		}

		void IList.Remove (object value)
		{
			throw new NotSupportedException ("Can't remove elements of selected cell base.List.");
		}

		void IList.RemoveAt (int index)
		{
			throw new NotSupportedException ("Can't remove elements of selected cell base.List.");
		}

		internal void InternalAdd (DataGridViewCell dataGridViewCell)
		{
			base.List.Add (dataGridViewCell);
		}

		internal void InternalRemove (DataGridViewCell dataGridViewCell)
		{
			base.List.Remove (dataGridViewCell);
		}
	}
}
