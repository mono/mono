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
	public class GridColumnStylesCollection : BaseCollection, IList
	{
		private ArrayList items;
		private DataGridTableStyle owner;
		private bool fire_event;

		internal GridColumnStylesCollection (DataGridTableStyle tablestyle)
		{
			items = new ArrayList ();
			owner = tablestyle;
			fire_event = true;
		}

		#region Public Instance Properties
		public DataGridColumnStyle this [string columnName] {
			get {
				int idx = FromColumnNameToIndex (columnName);
				return idx == -1 ? null : this [idx];
			}
		}

		public DataGridColumnStyle this [int index] {
			get {
				return (DataGridColumnStyle) items[index];
			}
		}

		
		public DataGridColumnStyle this [PropertyDescriptor propDesc] {
			get {				
				for (int i = 0; i < items.Count; i++) {
					DataGridColumnStyle column = (DataGridColumnStyle) items[i];
					if (column.PropertyDescriptor.Equals (propDesc)) {
						return column;
					}
				}
				
				return null;
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
		
		#region Private Instance Properties
		internal bool FireEvents {
			get { return fire_event;}
			set { fire_event = value;}
		}
		#endregion Private Instance Properties

		#region Public Instance Methods
		public virtual int Add (DataGridColumnStyle column)
		{
			int cnt = AddInternal (column);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, column));
			return cnt;
		}

		public void AddRange (DataGridColumnStyle[] columns)
		{
			foreach (DataGridColumnStyle mi in columns)
				AddInternal (mi);

			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
		}

		public void Clear ()
		{
			items.Clear ();
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh , null));
		}

		public bool Contains (DataGridColumnStyle column)
		{
			return items.Contains (column);
		}

		
		public bool Contains (PropertyDescriptor propDesc)
		{
			return (this [propDesc] != null);
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
			int cnt = AddInternal ((DataGridColumnStyle)value);

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
		}

		void IList.RemoveAt (int index)
		{
			object item = items[index];
			
			items.RemoveAt (index);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, item));
		}
		
		public int IndexOf (DataGridColumnStyle element)
		{
			return items.IndexOf (element);
		}
		
		protected void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{						
			if (fire_event == true && CollectionChanged != null) {
				CollectionChanged (this, ccevent);
			}
		}
		
		public void Remove (DataGridColumnStyle column)
		{
			items.Remove (column);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, column));
		}
		
		public void RemoveAt (int index)
		{
			object item = items[index];
			items.RemoveAt (index);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, item));
		}		
		
		public void ResetPropertyDescriptors ()
		{
			for (int i = 0; i < items.Count; i++) {
				DataGridColumnStyle column = (DataGridColumnStyle) items[i];
				if (column.PropertyDescriptor != null) {
					column.PropertyDescriptor = null;
				}
			}
		}

		#endregion Public Instance Methods

		#region Events
		public event CollectionChangeEventHandler CollectionChanged;		
		#endregion Events		
		
		#region Private Instance Methods
		private int AddInternal (DataGridColumnStyle column)
		{				
			if (FromColumnNameToIndex (column.MappingName) != -1) {
				throw new ArgumentException ("The ColumnStyles collection already has a column with this mapping name");
			}
			
			column.TableStyle = owner;
			int cnt = items.Add (column);
			return cnt;			
		}
		
		private int FromColumnNameToIndex (string columnName)
		{		
			for (int i = 0; i < items.Count; i++) {
				DataGridColumnStyle column = (DataGridColumnStyle) items[i];
				if (column.MappingName == columnName) {
					return i;
				}
			}
			
			return -1;
		}
				
		#endregion Private Instance Methods
	}
}
