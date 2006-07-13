//
// System.Data.Common.FieldNameLookup.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Data;

namespace System.Data.Common {
	internal sealed class FieldNameLookup : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;

		#endregion
		
		#region Constructors

		public FieldNameLookup ()
		{
			list = new ArrayList ();
		}

		public FieldNameLookup (DataTable schemaTable)
			: this ()
		{
			foreach (DataRow row in schemaTable.Rows)
				list.Add ((string) row["ColumnName"]);
		}

		#endregion

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public string this [int index] {
			get { return (string) list[index]; }
			set { list[index] = value; }
		}

		public object SyncRoot {	
			get { return list.SyncRoot; }
		}

		#endregion

		#region Methods

		public int Add (object value) 
		{
			return list.Add (value); 
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator (); 
		}

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Remove (object value)
		{ 
			list.Remove (value);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}
