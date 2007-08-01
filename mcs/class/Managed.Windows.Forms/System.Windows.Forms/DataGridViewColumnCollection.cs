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

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {

	[ListBindable (false)]
	public class DataGridViewColumnCollection : BaseCollection, IList, ICollection, IEnumerable {

		private DataGridView dataGridView;

		public DataGridViewColumnCollection (DataGridView dataGridView) {
			this.dataGridView = dataGridView;
		}

		bool IList.IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		public DataGridViewColumn this [int index] {
			get { return (DataGridViewColumn) base.List[index]; }
		}

		public DataGridViewColumn this [string columnName] {
			get {
				foreach (DataGridViewColumn col in base.List) {
					if (col.Name == columnName) {
						return col;
					}
				}
				return null;
			}
		}

		public event CollectionChangeEventHandler CollectionChanged;

		int IList.Add (object o) {
			return Add(o as DataGridViewColumn);
		}

		public virtual int Add (DataGridViewColumn dataGridViewColumn) {
			dataGridViewColumn.SetIndex(base.List.Count);
			dataGridViewColumn.SetDataGridView(dataGridView);
			int result = base.List.Add(dataGridViewColumn);
			DataGridView.OnColumnAddedInternal (new DataGridViewColumnEventArgs (dataGridViewColumn));
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewColumn));
			return result;
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual int Add (string columnName, string headerText) {
			DataGridViewColumn col = new DataGridViewTextBoxColumn();
			col.Name = columnName;
			col.HeaderText = headerText;
			return Add(col);
		}

		public virtual void AddRange (params DataGridViewColumn[] dataGridViewColumns) {
			foreach (DataGridViewColumn col in dataGridViewColumns) {
				Add(col);
			}
		}

		public virtual void Clear () {
			base.List.Clear();
		}

		bool IList.Contains (object o) {
			return Contains(o as DataGridViewColumn);
		}

		public virtual bool Contains (DataGridViewColumn dataGridViewColumn) {
			return base.List.Contains(dataGridViewColumn);
		}

		public virtual bool Contains (string columnName) {
			foreach (DataGridViewColumn col in base.List) {
				if (col.Name == columnName) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo (DataGridViewColumn[] array, int index) {
			base.List.CopyTo(array, index);
		}

		public int GetColumnCount (DataGridViewElementStates includeFilter) {
			return 0;
		}

		public int GetColumnsWidth (DataGridViewElementStates includeFilter) {
			return 0;
		}

		public DataGridViewColumn GetFirstColumn (DataGridViewElementStates includeFilter) {
			return null;
		}

		public DataGridViewColumn GetFirstColumn (DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			return null;
		}

		public DataGridViewColumn GetLastColumn (DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			return null;
		}

		public DataGridViewColumn GetNextColumn (DataGridViewColumn dataGridViewColumnStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			return null;
		}

		public DataGridViewColumn GetPreviousColumn (DataGridViewColumn dataGridViewColumnStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			return null;
		}

		int IList.IndexOf (object o) {
			return IndexOf(o as DataGridViewColumn);
		}

		public int IndexOf (DataGridViewColumn dataGridViewColumn) {
			return base.List.IndexOf(dataGridViewColumn);
		}

		void IList.Insert (int columnIndex, object o) {
			Insert(columnIndex, o as DataGridViewColumn);
		}

		public virtual void Insert (int columnIndex, DataGridViewColumn dataGridViewColumn) {
			dataGridViewColumn.SetIndex(columnIndex);
			base.List.Insert(columnIndex, dataGridViewColumn);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewColumn));
		}

		void IList.Remove (object o) {
			Remove(o as DataGridViewColumn);
		}

		public virtual void Remove (DataGridViewColumn dataGridViewColumn) {
			base.List.Remove(dataGridViewColumn);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataGridViewColumn));
		}

		public virtual void Remove (string columnName) {
			foreach (DataGridViewColumn col in base.List) {
				if (col.Name == columnName) {
					Remove(col);
					return;
				}
			}
		}

		public virtual void RemoveAt (int index) {
			DataGridViewColumn col = this[index];
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, col));
		}

		protected DataGridView DataGridView {
			get { return dataGridView; }
		}

		protected virtual void OnCollectionChanged (CollectionChangeEventArgs e) {
			if (CollectionChanged != null) {
				CollectionChanged(this, e);
			}
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		/************************/

		internal ArrayList ColumnDisplayIndexSortedArrayList {
			get {
				ArrayList result = (ArrayList) base.List.Clone();
				result.Sort(new ColumnDisplayIndexComparator());
				return result;
			}
		}

		private class ColumnDisplayIndexComparator : IComparer {

			public int Compare (object o1, object o2) {
				DataGridViewColumn col1 = (DataGridViewColumn) o1;
				DataGridViewColumn col2 = (DataGridViewColumn) o2;
				if (col1.DisplayIndex < col2.DisplayIndex) {
					return -1;
				}
				else if (col1.DisplayIndex > col2.DisplayIndex) {
					return 1;
				}
				else {
					return 0;
				}
			}

		}

		/************************/


	}

}

#endif
