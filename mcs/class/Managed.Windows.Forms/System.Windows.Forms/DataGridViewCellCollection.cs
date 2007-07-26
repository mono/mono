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
	public class DataGridViewCellCollection : BaseCollection, IList, ICollection, IEnumerable {

		private DataGridViewRow dataGridViewRow;

		public DataGridViewCellCollection (DataGridViewRow dataGridViewRow) : base() {
			this.dataGridViewRow = dataGridViewRow;
		}

		bool IList.IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this[index]; }
			set { this[index] = value as DataGridViewCell; }
		}

		public DataGridViewCell this [int index] {
			get { return (DataGridViewCell) base.List[index]; }
			set { Insert(index, value); }
		}

		internal DataGridViewCell GetCellInternal (int colIndex)
		{
			return (DataGridViewCell) base.List [colIndex];
		}
		
		public DataGridViewCell this [string columnName] {
			get {
				foreach (DataGridViewCell cell in base.List) {
					if (cell.OwningColumn.Name == columnName) {
						return cell;
					}
				}
				return null;
			}
			set {
				for (int i = 0; i < base.List.Count; i++) {
					DataGridViewCell cell = (DataGridViewCell) base.List[i];
					if (cell.OwningColumn.Name == columnName) {
						Insert(i, value);
						return;
					}
				}
				Add(value);
			}
		}

		public event CollectionChangeEventHandler CollectionChanged;

		int IList.Add (object o) {
			return Add(o as DataGridViewCell);
		}

		public virtual int Add (DataGridViewCell dataGridViewCell) {
			dataGridViewCell.SetOwningRow(dataGridViewRow);
			dataGridViewCell.SetColumnIndex(base.List.Count);
			dataGridViewCell.SetDataGridView(dataGridViewRow.DataGridView);
			int result = base.List.Add(dataGridViewCell);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewCell));
			return result;
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual void AddRange (params DataGridViewCell[] dataGridViewCells) {
			foreach (DataGridViewCell cell in dataGridViewCells) {
				this.Add(cell);
			}
		}

		public virtual void Clear () {
			base.List.Clear();
		}

		bool IList.Contains (object o) {
			return Contains(o as DataGridViewCell);
		}

		public virtual bool Contains (DataGridViewCell dataGridViewCell) {
			return base.List.Contains(dataGridViewCell);
		}

		public void CopyTo (DataGridViewCell[] array, int index) {
			base.List.CopyTo(array, index);
		}

		int IList.IndexOf (object o) {
			return IndexOf(o as DataGridViewCell);
		}

		public int IndexOf (DataGridViewCell dataGridViewCell) {
			return base.List.IndexOf(dataGridViewCell);
		}

		void IList.Insert (int index, object o) {
			Insert(index, o as DataGridViewCell);
		}

		public virtual void Insert (int index, DataGridViewCell dataGridViewCell) {
			dataGridViewCell.SetOwningRow(dataGridViewRow);
			dataGridViewCell.SetColumnIndex(index);
			dataGridViewCell.SetDataGridView(dataGridViewRow.DataGridView);
			base.List.Insert(index, dataGridViewCell);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataGridViewCell));
		}

		void IList.Remove (object o) {
			Remove(o as DataGridViewCell);
		}

		public virtual void Remove (DataGridViewCell dataGridViewCell) {
			base.List.Remove(dataGridViewCell);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataGridViewCell));
		}

		public virtual void RemoveAt (int index) {
			DataGridViewCell cell = this[index];
			base.List.RemoveAt(index);
			OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, cell));
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		protected void OnCollectionChanged (CollectionChangeEventArgs e) {
			if (CollectionChanged != null) {
				CollectionChanged(this, e);
			}
		}

	}

}

#endif
