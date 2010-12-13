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
	public class DataGridViewCellCollection : BaseCollection, IList, ICollection, IEnumerable
	{
		private DataGridViewRow dataGridViewRow;

		public DataGridViewCellCollection (DataGridViewRow dataGridViewRow) : base()
		{
			this.dataGridViewRow = dataGridViewRow;
		}

		bool IList.IsFixedSize {
			get { return base.List.IsFixedSize; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = value as DataGridViewCell; }
		}

		public DataGridViewCell this [int index] {
			get { return (DataGridViewCell) base.List [index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				Insert(index, value);
			}
		}

		internal DataGridViewCell GetCellInternal (int colIndex)
		{
			return (DataGridViewCell) base.List [colIndex];
		}
		
		public DataGridViewCell this [string columnName] {
			get {
				if (columnName == null)
					throw new ArgumentNullException ("columnName");

				foreach (DataGridViewCell cell in base.List) {
					if (string.Compare (cell.OwningColumn.Name, columnName, true) == 0)
						return cell;
				}

				throw new ArgumentException (string.Format (
					"Column name {0} cannot be found.",
					columnName), "columnName");
			}
			set {
				if (columnName == null)
					throw new ArgumentNullException ("columnName");
				if (value == null)
					throw new ArgumentNullException ("value");

				for (int i = 0; i < base.List.Count; i++) {
					DataGridViewCell cell = (DataGridViewCell) base.List [i];
					if (string.Compare (cell.OwningColumn.Name, columnName, true) == 0) {
						Insert (i, value);
						return;
					}
				}
				Add (value);
			}
		}

		internal DataGridViewCell GetBoundCell (string dataPropertyName)
		{
			foreach (DataGridViewCell cell in base.List) {
				if (string.Compare (cell.OwningColumn.DataPropertyName, dataPropertyName, true) == 0)
					return cell;
			}
			
			return null;
		}
		
		public event CollectionChangeEventHandler CollectionChanged;

		int IList.Add (object value)
		{
			return Add (value as DataGridViewCell);
		}

		public virtual int Add (DataGridViewCell dataGridViewCell)
		{
			int result = base.List.Add (dataGridViewCell);
			dataGridViewCell.SetOwningRow (dataGridViewRow);
			dataGridViewCell.SetColumnIndex (result);
			dataGridViewCell.SetDataGridView (dataGridViewRow.DataGridView);
			OnCollectionChanged (new CollectionChangeEventArgs (
				CollectionChangeAction.Add, dataGridViewCell));
			return result;
		}

		internal void Replace (int columnIndex, DataGridViewCell dataGridViewCell)
		{
			RemoveAt (columnIndex);
			Insert (columnIndex, dataGridViewCell);
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual void AddRange (params DataGridViewCell[] dataGridViewCells)
		{
			foreach (DataGridViewCell cell in dataGridViewCells)
				Add (cell);
		}

		public virtual void Clear ()
		{
			base.List.Clear();
		}

		bool IList.Contains (object value)
		{
			return Contains (value as DataGridViewCell);
		}

		public virtual bool Contains (DataGridViewCell dataGridViewCell)
		{
			return base.List.Contains (dataGridViewCell);
		}

		public void CopyTo (DataGridViewCell[] array, int index)
		{
			base.List.CopyTo (array, index);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf (value as DataGridViewCell);
		}

		public int IndexOf (DataGridViewCell dataGridViewCell)
		{
			return base.List.IndexOf (dataGridViewCell);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, value as DataGridViewCell);
		}

		public virtual void Insert (int index, DataGridViewCell dataGridViewCell)
		{
			base.List.Insert (index, dataGridViewCell);
			dataGridViewCell.SetOwningRow (dataGridViewRow);
			dataGridViewCell.SetColumnIndex (index);
			dataGridViewCell.SetDataGridView (dataGridViewRow.DataGridView);
			ReIndex ();
			OnCollectionChanged (new CollectionChangeEventArgs (
				CollectionChangeAction.Add, dataGridViewCell));
		}

		void IList.Remove (object value)
		{
			Remove (value as DataGridViewCell);
		}

		public virtual void Remove (DataGridViewCell cell)
		{
			base.List.Remove (cell);
			ReIndex ();
			OnCollectionChanged (new CollectionChangeEventArgs (
				CollectionChangeAction.Remove, cell));
		}

		public virtual void RemoveAt (int index)
		{
			DataGridViewCell cell = this [index];
			base.List.RemoveAt (index);
			ReIndex ();
			OnCollectionChanged (new CollectionChangeEventArgs (
				CollectionChangeAction.Remove, cell));
		}

		private void ReIndex ()
		{
			for (int i = 0; i < base.List.Count; i++)
				this[i].SetColumnIndex (i);
		}

		protected override ArrayList List {
			get { return base.List; }
		}

		protected void OnCollectionChanged (CollectionChangeEventArgs e)
		{
			if (CollectionChanged != null) {
				CollectionChanged(this, e);
			}
		}
	}
}
