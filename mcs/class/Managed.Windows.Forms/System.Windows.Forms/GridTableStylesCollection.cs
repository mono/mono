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
//	Jordi Mas i Hernandez <jordi@ximian.com>
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class GridTableStylesCollection : BaseCollection, IList
	{
		private ArrayList items;
		private DataGrid owner;

		internal GridTableStylesCollection (DataGrid grid)
		{
			items = new ArrayList ();
			owner = grid;
		}

		#region Public Instance Properties
		public DataGridTableStyle this[string columnName] {
			get {
				int idx = FromColumnNameToIndex (columnName);
				return idx == -1 ? null : this [idx];
			}
		}

		public DataGridTableStyle this[int index] {
			get {
				return (DataGridTableStyle) items[index];
			}
		}

		protected override ArrayList List {
			get { return items; }
		}

		int ICollection.Count {
			get { return items.Count;}
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this;}
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false;}
		}

		object IList.this [int index] {
			get {
				return items[index];
			}
			set {
				throw new NotSupportedException ();
			}
		}

		#endregion Public Instance Properties

		#region Public Instance Methods
		public virtual int Add (DataGridTableStyle column)
		{			
			int cnt = AddInternal (column);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, column));
			return cnt;
		}

		public void AddRange (DataGridTableStyle[] columns)
		{
			foreach (DataGridTableStyle mi in columns)
				AddInternal (mi);

			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
		}

		public void Clear ()
		{
			items.Clear ();
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh , null));
		}

		public bool Contains (DataGridTableStyle column)
		{
			return items.Contains (column);
		}

		public bool Contains (string name)
		{
			return (this[name] != null);
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			items.CopyTo (dest, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		int IList.Add (object value)
		{
			int cnt = AddInternal ((DataGridTableStyle)value);

			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, null));
			return cnt;
		}

		void IList.Clear ()
		{
			items.Clear ();
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh , null));
		}

		bool IList.Contains (object value)
		{
			return items.Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return items.IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Remove (object value)
		{
			items.Remove (value);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, value));
		}

		void IList.RemoveAt (int index)
		{
			object item = items[index];
			items.RemoveAt (index);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, item));
		}

		protected void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{
			if (CollectionChanged != null) {
				CollectionChanged (this, ccevent);
			}
		}

		public void Remove (DataGridTableStyle table)
		{
			items.Remove (table);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, table));
		}

		public void RemoveAt (int index)
		{
			object item = items[index];

			items.RemoveAt (index);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, item));
		}

		#endregion Public Instance Methods

		#region Events
		public event CollectionChangeEventHandler CollectionChanged;
		#endregion Events		
		
		
		#region Private Instance Methods
		private int AddInternal (DataGridTableStyle column)
		{				
			if (FromColumnNameToIndex (column.MappingName) != -1) {
				throw new ArgumentException ("The TableStyles collection already has a TableStyle with this mapping name");
			}
			
			column.DataGrid = owner;
			int cnt = items.Add (column);
			return cnt;			
		}
		
		private int FromColumnNameToIndex (string columnName)
		{		
			for (int i = 0; i < items.Count; i++) {
				DataGridTableStyle column = (DataGridTableStyle) items[i];
				if (column.MappingName == columnName) {
					return i;
				}
			}
			
			return -1;
		}
				
		#endregion Private Instance Methods
	}
}

