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

	public class DataGridViewRowCollection : IList, ICollection, IEnumerable {

		private ArrayList list;
		private DataGridView dataGridView;

		private bool raiseEvent = true;

		public DataGridViewRowCollection (DataGridView dataGridView) {
			if (dataGridView == null) {
				throw new ArgumentException("DataGridView is null.");
			}
			this.dataGridView = dataGridView;
			list = new ArrayList();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object IList.this [int index] {
			get {
				Console.WriteLine("acceso");
				return this[index];
			}
			set { list[index] = value as DataGridViewRow; }
		}

		public DataGridViewRow this [int index] {
			get {
				// Accessing a System.Windows.Forms.DataGridViewRow with this indexer causes the row to become unshared. To keep the row shared, use the System.Windows.Forms.DataGridViewRowCollection.SharedRow method. For more information, see Best Practices for Scaling the Windows Forms DataGridView Control.
				return (DataGridViewRow) list[index];
			}
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public event CollectionChangeEventHandler CollectionChange;

		public virtual int Add () {
			return Add(dataGridView.RowTemplate.Clone() as DataGridViewRow);
		}

		int IList.Add(object o) {
			return Add(o as DataGridViewRow);
		}

		public virtual int Add (DataGridViewRow dataGridViewRow) {
			if (dataGridView.DataSource != null) {
				throw new InvalidOperationException("DataSource of DataGridView is not null.");
			}
			if (dataGridView.Columns.Count == 0) {
				throw new InvalidOperationException("DataGridView has no columns.");
			}
			dataGridViewRow.SetIndex(list.Count);
			dataGridViewRow.SetDataGridView(dataGridView);
			int result = list.Add(dataGridViewRow);
			OnCollectionChange(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewRow));
			if (raiseEvent) {
				DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(result, 1));
			}
			return result;
		}

		public virtual int Add (int count) {
			if (count <= 0) {
				throw new ArgumentOutOfRangeException("Count is less than or equeal to 0.");
			}
			if (dataGridView.DataSource != null) {
				throw new InvalidOperationException("DataSource of DataGridView is not null.");
			}
			if (dataGridView.Columns.Count == 0) {
				throw new InvalidOperationException("DataGridView has no columns.");
			}
			raiseEvent = false;
			int result = 0;
			for (int i = 0; i < count; i++) {
				result = Add(dataGridView.RowTemplate.Clone() as DataGridViewRow);
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(result - count + 1, count));
			raiseEvent = true;
			return result;
		}

		public virtual int Add (params object[] values) {
			if (values == null) {
				throw new ArgumentNullException("values is null.");
			}
			if (dataGridView.VirtualMode) {
				throw new InvalidOperationException("DataGridView is in virtual mode.");
			}
			DataGridViewRow row = new DataGridViewRow();
			int result = Add(row);
			row.SetValues(values);
			return result;
		}

		public virtual int AddCopies (int indexSource, int count) {
			raiseEvent = false;
			int lastIndex = 0;
			for (int i = 0; i < count; i++) {
				lastIndex = AddCopy(indexSource);
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(lastIndex - count + 1, count));
			raiseEvent = true;
			return lastIndex;
		}

		public virtual int AddCopy (int indexSource) {
			return Add((list[indexSource] as DataGridViewRow).Clone() as DataGridViewRow);
		}

		public virtual void AddRange (params DataGridViewRow[] dataGridViewRows) {
			raiseEvent = false;
			int count = 0;
			int lastIndex = -1;
			foreach (DataGridViewRow row in dataGridViewRows) {
				lastIndex = Add(row);
				count++;
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(lastIndex - count + 1, count));
			raiseEvent = true;
		}

		public virtual void Clear () {
			list.Clear();
		}

		bool IList.Contains (object o) {
			return Contains(o as DataGridViewRow);
		}

		public virtual bool Contains (DataGridViewRow dataGridViewRow) {
			return list.Contains(dataGridViewRow);
		}

		public void CopyTo (Array array, int index) {
			list.CopyTo(array, index);
		}

		public void CopyTo (DataGridViewRow[] array, int index) {
			list.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator () {
			return list.GetEnumerator();
		}

		public int GetFirstRow (DataGridViewElementStates includeFilter) {
			for (int i = 0; i < list.Count; i++) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if ((row.State & includeFilter) != 0) {
					return i;
				}
			}
			return -1;
		}

		public int GetFirstRow (DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			for (int i = 0; i < list.Count; i++) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if (((row.State & includeFilter) != 0) && ((row.State & excludeFilter) == 0)) {
					return i;
				}
			}
			return -1;
		}

		public int GetLastRow (DataGridViewElementStates includeFilter) {
			for (int i = list.Count - 1; i >= 0; i--) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if ((row.State & includeFilter) != 0) {
					return i;
				}
			}
			return -1;
		}

		public int GetNextRow (int indexStart, DataGridViewElementStates includeFilter) {
			for (int i = indexStart + 1; i < list.Count; i++) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if ((row.State & includeFilter) != 0) {
					return i;
				}
			}
			return -1;
		}

		public int GetNextRow (int indexStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			for (int i = indexStart + 1; i < list.Count; i++) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if (((row.State & includeFilter) != 0) && ((row.State & excludeFilter) == 0)) {
					return i;
				}
			}
			return -1;
		}

		public int GetPreviousRow (int indexStart, DataGridViewElementStates includeFilter) {
			for (int i = indexStart - 1; i >= 0; i--) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if ((row.State & includeFilter) != 0) {
					return i;
				}
			}
			return -1;
		}

		public int GetPreviousRow (int indexStart, DataGridViewElementStates includeFilter, DataGridViewElementStates excludeFilter) {
			for (int i = indexStart - 1; i >= 0; i--) {
				DataGridViewRow row = (DataGridViewRow) list[i];
				if (((row.State & includeFilter) != 0) && ((row.State & excludeFilter) == 0)) {
					return i;
				}
			}
			return -1;
		}

		public int GetRowCount (DataGridViewElementStates includeFilter) {
			int result = 0;
			foreach (DataGridViewRow row in list) {
				if ((row.State & includeFilter) != 0) {
					result ++;
				}
			}
			return result;
		}

		public int GetRowsHeight (DataGridViewElementStates includeFilter) {
			int result = 0;
			foreach (DataGridViewRow row in list) {
				if ((row.State & includeFilter) != 0) {
					result += row.Height;
				}
			}
			return result;
		}

		public virtual DataGridViewElementStates GetRowState (int rowIndex) {
			return (list[rowIndex] as DataGridViewRow).State;
		}

		int IList.IndexOf (object o) {
			return IndexOf(o as DataGridViewRow);
		}

		public int IndexOf (DataGridViewRow dataGridViewRow) {
			return list.IndexOf(dataGridViewRow);
		}

		void IList.Insert (int rowIndex, object o) {
			Insert(rowIndex, o as DataGridViewRow);
		}

		public virtual void Insert (int rowIndex, DataGridViewRow dataGridViewRow) {
			dataGridViewRow.SetIndex(rowIndex);
			dataGridViewRow.SetDataGridView(dataGridView);
			list[rowIndex] = dataGridViewRow;
			OnCollectionChange(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewRow));
			if (raiseEvent) {
				DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(rowIndex, 1));
			}
		}

		public virtual void Insert (int rowIndex, int count) {
			int index = rowIndex;
			raiseEvent = false;
			for (int i = 0; i < count; i++) {
				Insert(index++, dataGridView.RowTemplate.Clone());
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(rowIndex, count));
			raiseEvent = true;
		}

		public virtual void Insert (int rowIndex, params object[] values) {
			if (values == null) {
				throw new ArgumentNullException("Values is null.");
			}
			if (dataGridView.VirtualMode || dataGridView.DataSource != null) {
				throw new InvalidOperationException();
			}
			DataGridViewRow row = new DataGridViewRow();
			row.SetValues(values);
			Insert(rowIndex, row);
		}

		public virtual void InsertCopies (int indexSource, int indexDestination, int count) {
			raiseEvent = false;
			int index = indexDestination;
			for (int i = 0; i < count; i++) {
				InsertCopy(indexSource, index++);
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(indexDestination, count));
			raiseEvent = true;
		}

		public virtual void InsertCopy (int indexSource, int indexDestination) {
			Insert(indexDestination, (list[indexSource] as DataGridViewRow).Clone());
		}

		public virtual void InsertRange (int rowIndex, params DataGridViewRow[] dataGridViewRows) {
			raiseEvent = false;
			int index = rowIndex;
			int count = 0;
			foreach (DataGridViewRow row in dataGridViewRows) {
				Insert(index++, row);
				count++;
			}
			DataGridView.OnRowsAdded(new DataGridViewRowsAddedEventArgs(rowIndex, count));
			raiseEvent = true;
		}

		void IList.Remove (object o) {
			Remove(o as DataGridViewRow);
		}

		public virtual void Remove (DataGridViewRow dataGridViewRow) {
			list.Remove(dataGridViewRow);
			OnCollectionChange(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataGridViewRow));
			DataGridView.OnRowsRemoved(new DataGridViewRowsRemovedEventArgs(dataGridViewRow.Index, 1));
		}

		public virtual void RemoveAt (int index) {
			DataGridViewRow row = this[index];
			list.RemoveAt(index);
			OnCollectionChange(new CollectionChangeEventArgs(CollectionChangeAction.Remove, row));
			DataGridView.OnRowsRemoved(new DataGridViewRowsRemovedEventArgs(index, 1));
		}

		public DataGridViewRow SharedRow (int rowIndex) {
			return (DataGridViewRow) list[rowIndex];
		}

		protected DataGridView DataGridView {
			get { return dataGridView; }
		}

		protected ArrayList List {
			get { return list; }
		}

		protected virtual void OnCollectionChange (CollectionChangeEventArgs e) {
			if (CollectionChange != null) {
				CollectionChange(this, e);
			}
		}

		/************************/

		internal void InternalAdd (DataGridViewRow dataGridViewRow) {
			dataGridViewRow.SetIndex(list.Count);
			dataGridViewRow.SetDataGridView(dataGridView);
			list.Add(dataGridViewRow);
		}

		internal ArrayList RowIndexSortedArrayList {
			get {
				ArrayList result = (ArrayList) list.Clone();
				result.Sort(new RowIndexComparator());
				return result;
			}
		}

		private class RowIndexComparator : IComparer {

			public int Compare (object o1, object o2) {
				DataGridViewRow row1 = (DataGridViewRow) o1;
				DataGridViewRow row2 = (DataGridViewRow) o2;
				if (row1.Index < row2.Index) {
					return -1;
				}
				else if (row1.Index > row2.Index) {
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
