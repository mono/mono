//
// System.Data.DataRowCollection.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Ximian, Inc 2002
// (C) Copyright 2002 Tim Coleman
// (C) Copyright 2002 Daniel Morgan
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;

namespace System.Data
{
	/// <summary>
	/// Collection of DataRows in a DataTable
	/// </summary>
	public partial class DataRowCollection : InternalDataCollectionBase {
		private DataTable table;

		internal event ListChangedEventHandler ListChanged;

		/// <summary>
		/// Internal constructor used to build a DataRowCollection.
		/// </summary>
		internal DataRowCollection (DataTable table)
		{
			this.table = table;
		}

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		public DataRow this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ("There is no row at position " + index + ".");

				return (DataRow) List [index];
			}
		}

		/// <summary>
		/// Adds the specified DataRow to the DataRowCollection object.
		/// </summary>
		public void Add (DataRow row)
		{
			//TODO: validation
			if (row == null)
				throw new ArgumentNullException ("row", "'row' argument cannot be null.");

			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");

			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");

			row.BeginEdit ();

			row.Validate ();

			AddInternal (row);
		}

#if NET_2_0
		public
#else
		internal
#endif
		int IndexOf (DataRow row)
		{
			if (row == null || row.Table != table)
				return -1;

			int id = row.RowID;
			return (id >= 0 && id < List.Count && row == List [id]) ? id : -1;
		}

		internal void AddInternal (DataRow row)
		{
			AddInternal (row, DataRowAction.Add);
		}

		internal void AddInternal (DataRow row, DataRowAction action)
		{
			row.Table.ChangingDataRow (row, action);
			List.Add (row);
			row.AttachAt (List.Count - 1, action);
			row.Table.ChangedDataRow (row, action);
			if (row._rowChanged)
				row._rowChanged = false;
		}

		/// <summary>
		/// Creates a row using specified values and adds it to the DataRowCollection.
		/// </summary>
		public DataRow Add (params object[] values)
		{
			if (values == null)
				throw new NullReferenceException ();
			DataRow row = table.NewNotInitializedRow ();
			int newRecord = table.CreateRecord (values);
			row.ImportRecord (newRecord);

			row.Validate ();
			AddInternal (row);
			return row;
		}

		/// <summary>
		/// Clears the collection of all rows.
		/// </summary>
		public void Clear ()
		{
			if (this.table.DataSet != null && this.table.DataSet.EnforceConstraints) {
				foreach (Constraint c in table.Constraints) {
					UniqueConstraint uc = c as UniqueConstraint;
					if (uc == null)
						continue;
					if (uc.ChildConstraint == null || uc.ChildConstraint.Table.Rows.Count == 0)
						continue;

					string err = String.Format ("Cannot clear table Parent because " +
								"ForeignKeyConstraint {0} enforces Child.", uc.ConstraintName);
					throw new InvalidConstraintException (err);
				}
			}

			table.DataTableClearing ();
			List.Clear ();

			// Remove from indexes
			table.ResetIndexes ();
			table.DataTableCleared ();
			OnListChanged (this, new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
		}

		/// <summary>
		/// Gets a value indicating whether the primary key of any row in the collection contains
		/// the specified value.
		/// </summary>
		public bool Contains (object key)
		{
			return Find (key) != null;
		}

		/// <summary>
		/// Gets a value indicating whether the primary key column(s) of any row in the
		/// collection contains the values specified in the object array.
		/// </summary>
		public bool Contains (object [] keys)
		{
			return Find (keys) != null;
		}

		/// <summary>
		/// Gets the row specified by the primary key value.
		/// </summary>
		public DataRow Find (object key)
		{
			return Find (new object []{key}, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets the row containing the specified primary key values.
		/// </summary>
		public DataRow Find (object[] keys)
		{
			return Find (keys, DataViewRowState.CurrentRows);
		}

		/// <summary>
		/// Gets the row containing the specified primary key values by searching the rows
		/// filtered by the state.
		/// </summary>
		internal DataRow Find (object [] keys, DataViewRowState rowStateFilter)
		{
			if (table.PrimaryKey.Length == 0)
				throw new MissingPrimaryKeyException ("Table doesn't have a primary key.");

			if (keys == null)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length +" value(s) for the key being indexed, but received 0 value(s).");

			Index index = table.GetIndex (table.PrimaryKey, null, rowStateFilter, null, false);
			int record = index.Find (keys);

			if (record != -1 || !table._duringDataLoad)
				return (record != -1 ? table.RecordCache [record] : null);

			// If the key is not found using Index *and* if DataTable is under BeginLoadData
			// then, check all the DataRows for the key
			record = table.RecordCache.NewRecord ();
			try {
				for (int i=0; i < table.PrimaryKey.Length; ++i)
					table.PrimaryKey [i].DataContainer [record] = keys [i];

				bool found;
				foreach (DataRow row in this) {
					int rowIndex = Key.GetRecord (row, rowStateFilter);
					if (rowIndex == -1)
						continue;

					found = true;
					for (int columnCnt = 0; columnCnt < table.PrimaryKey.Length; ++columnCnt) {
						if (table.PrimaryKey [columnCnt].CompareValues (rowIndex, record) == 0)
							continue;
						found = false;
						break;
					}
					if (found)
						return row;
				}
				return null;
			} finally {
				table.RecordCache.DisposeRecord (record);
			}
		}

		/// <summary>
		/// Inserts a new row into the collection at the specified location.
		/// </summary>
		public void InsertAt (DataRow row, int pos)
		{
			if (pos < 0)
				throw new IndexOutOfRangeException ("The row insert position " + pos + " is invalid.");

			if (row == null)
				throw new ArgumentNullException ("row", "'row' argument cannot be null.");

			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");

			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");

			row.Validate ();

			row.Table.ChangingDataRow (row, DataRowAction.Add);

			if (pos >= List.Count) {
				pos = List.Count;
				List.Add (row);
			} else {
				List.Insert (pos, row);
				for (int i = pos+1; i < List.Count; i++)
					((DataRow) List [i]).RowID = i;
			}

			row.AttachAt (pos, DataRowAction.Add);
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Removes the specified DataRow from the internal list. Used by DataRow to commit the removing.
		/// </summary>
		internal void RemoveInternal (DataRow row)
		{
			if (row == null)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			int index = this.IndexOf (row);
			if (index < 0)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			List.RemoveAt (index);
			for (; index < List.Count; ++index)
				((DataRow) List [index]).RowID = index;
		}

		/// <summary>
		/// Removes the specified DataRow from the collection.
		/// </summary>
		public void Remove (DataRow row)
		{
			if (IndexOf (row) < 0)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");

			DataRowState state = row.RowState;
			if (state != DataRowState.Deleted && state != DataRowState.Detached) {
				row.Delete ();
				// if the row was in added state it will be in Detached state after the
				// delete operation, so we have to check it.
				if (row.RowState != DataRowState.Detached)
					row.AcceptChanges ();
			}
		}

		/// <summary>
		/// Removes the row at the specified index from the collection.
		/// </summary>
		public void RemoveAt (int index)
		{
			Remove (this [index]);
		}

		internal void OnListChanged (object sender, ListChangedEventArgs args)
		{
			if (ListChanged != null)
				ListChanged (sender, args);
		}
	}

	sealed partial class DataRowCollection {
		public override int Count {
			get { return List.Count; }
		}

		public void CopyTo (DataRow [] array, int index)
		{
			CopyTo ((Array) array, index);
		}

		public override void CopyTo (Array ar, int index)
		{
			base.CopyTo (ar, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return base.GetEnumerator ();
		}
	}
}
