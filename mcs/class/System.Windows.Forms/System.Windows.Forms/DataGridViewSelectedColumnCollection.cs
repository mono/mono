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

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ListBindable (false)]
	public class DataGridViewSelectedColumnCollection : BaseCollection, IList, ICollection, IEnumerable
	{
		internal DataGridViewSelectedColumnCollection ()
		{
		}

		bool IList.IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { throw new NotSupportedException("Can't insert or modify this collection."); }
		}

		public DataGridViewColumn this [int index] {
			get { return (DataGridViewColumn) base.List [index]; }
		}

		int IList.Add (object value)
		{
			throw new NotSupportedException ("Can't add elements to this collection.");
		}

		void IList.Clear ()
		{
			Clear ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Clear ()
		{
			throw new NotSupportedException ("This collection cannot be cleared.");
		}

		bool IList.Contains (object value)
		{
			return Contains (value as DataGridViewColumn);
		}

		public bool Contains (DataGridViewColumn dataGridViewColumn)
		{
			return base.List.Contains (dataGridViewColumn);
		}

		public void CopyTo (DataGridViewColumn [] array, int index)
		{
			base.List.CopyTo (array, index);
		}

		int IList.IndexOf (object value)
		{
			return base.List.IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, value as DataGridViewColumn);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Insert (int index, DataGridViewColumn dataGridViewColumn)
		{
			throw new NotSupportedException ("Insert is not allowed.");
		}

		void IList.Remove (object value)
		{
			throw new NotSupportedException ("Can't remove elements of this collection.");
		}

		void IList.RemoveAt (int index)
		{
			throw new NotSupportedException ("Can't remove elements of this collection.");
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		internal void InternalAdd (DataGridViewColumn dataGridViewColumn)
		{
			base.List.Add (dataGridViewColumn);
		}

		internal void InternalAddRange (DataGridViewSelectedColumnCollection columns)
		{
			if (columns == null)
				return;

			// Believe it or not, MS adds the columns in reverse order...
			for (int i = columns.Count - 1; i >= 0; i--)
				base.List.Add (columns [i]);
		}
		
		internal void InternalClear ()
		{
			List.Clear ();
		}
		
		internal void InternalRemove (DataGridViewColumn dataGridViewColumn)
		{
			base.List.Remove(dataGridViewColumn);
		}
	}
}
