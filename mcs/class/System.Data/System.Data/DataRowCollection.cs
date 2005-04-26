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
	[Serializable]
	public class DataRowCollection : InternalDataCollectionBase 
	{
		private DataTable table;

		/// <summary>
		/// Internal constructor used to build a DataRowCollection.
		/// </summary>
		internal DataRowCollection (DataTable table) : base ()
		{
			this.table = table;
		}

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		public DataRow this[int index] 
		{
			get { 
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ("There is no row at position " + index + ".");

				return (DataRow) List[index]; 
			}
		}

		/// <summary>
		/// This member overrides InternalDataCollectionBase.List
		/// </summary>
		protected override ArrayList List 
		{
			get { return base.List; }
		}		

		/// <summary>
		/// Adds the specified DataRow to the DataRowCollection object.
		/// </summary>
		public void Add (DataRow row) 
		{
			//TODO: validation
			if (row == null)
				throw new ArgumentNullException("row", "'row' argument cannot be null.");

			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");
			
			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");
			
			row.BeginEdit();

			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);

			AddInternal(row);
		}

		internal void AddInternal(DataRow row) {
			row.Table.ChangingDataRow (row, DataRowAction.Add);
			row.HasParentCollection = true;
			List.Add (row);
			// Set the row id.
			row.RowID = List.Count - 1;
			row.AttachRow ();
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Creates a row using specified values and adds it to the DataRowCollection.
		/// </summary>
#if NET_2_0
		public virtual DataRow Add (params object[] values) 
#else
		public virtual DataRow Add (object[] values) 
#endif
		{
			DataRow row = table.NewNotInitializedRow();
			int newRecord = table.CreateRecord(values);
			row.ImportRecord(newRecord);
			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);
			AddInternal (row);
			return row;
		}

		/// <summary>
		/// Clears the collection of all rows.
		/// </summary>
		public void Clear () 
		{
			if (this.table.DataSet != null && this.table.DataSet.EnforceConstraints) {
				foreach (DataTable table in this.table.DataSet.Tables) {
					foreach (Constraint c in table.Constraints) {
						if (c is ForeignKeyConstraint) {
                                                                                                                ForeignKeyConstraint fk = (ForeignKeyConstraint) c;
							if (fk.RelatedTable.Equals(this.table) 
                                                            && fk.Table.Rows.Count > 0) // check does not make sense if we don't have rows
#if NET_1_1
								throw new InvalidConstraintException (String.Format ("Cannot clear table Parent" + 
                                                                                                                     " because ForeignKeyConstraint "+
                                                                                                                     "{0} enforces Child.", 
                                                                                                                     c.ConstraintName));
#else
								throw new ArgumentException (String.Format ("Cannot clear table Parent because " +
                                                                                                            "ForeignKeyConstraint {0} enforces Child.", 
                                                                                                            c.ConstraintName));
#endif
						}
					}
				}
			}
			List.Clear ();
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
		public bool Contains (object[] keys) 
		{
			return Find (keys) != null;
		}

		/// <summary>
		/// Gets the row specified by the primary key value.
		/// </summary>
		public DataRow Find (object key) 
		{
			return Find(new object[]{key});
		}

		/// <summary>
		/// Gets the row containing the specified primary key values.
		/// </summary>
		public DataRow Find (object[] keys) 
		{
			if (table.PrimaryKey.Length == 0)
				throw new MissingPrimaryKeyException ("Table doesn't have a primary key.");

			 if (keys == null)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length +" value(s) for the key being indexed, but received 0 value(s).");
                                                                                                    
			Index index = table.GetIndex(table.PrimaryKey,null,DataViewRowState.None,null,false);

			int record = index.Find(keys);
			return (record != -1) ? table.RecordCache[record] : null;
		}

		internal DataRow Find(int index)
		{
			DataColumn[] primaryKey = table.PrimaryKey;
			Index primaryKeyIndex = table.FindIndex(primaryKey);
			// if we can search through index
			if (primaryKeyIndex != null) {
				// get the child rows from the index
				int record = primaryKeyIndex.Find(index);
				if ( record != -1 ) {
					return table.RecordCache[record];
				}
			}
			else {
				//loop through all collection rows			
				foreach (DataRow row in this) {
					if (row.RowState != DataRowState.Deleted) {
						int rowIndex = row.IndexFromVersion(DataRowVersion.Default);
						bool match = true;
						for (int columnCnt = 0; columnCnt < primaryKey.Length; ++columnCnt) { 
							if (primaryKey[columnCnt].DataContainer.CompareValues(rowIndex, index) != 0) {
								match = false;
							}
						}
						if ( match ) {
							return row;
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Inserts a new row into the collection at the specified location.
		/// </summary>
		public void InsertAt (DataRow row, int pos) 
		{
			if (pos < 0)
				throw new IndexOutOfRangeException ("The row insert position " + pos + " is invalid.");
			
			if (row == null)
				throw new ArgumentNullException("row", "'row' argument cannot be null.");
	
			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");

			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");
			
			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);
				
			row.Table.ChangingDataRow (row, DataRowAction.Add);

			if (pos >= List.Count) {
				row.RowID = List.Count;
				List.Add (row);
			}
			else {
				List.Insert (pos, row);
				row.RowID = pos;
				for (int i = pos+1; i < List.Count; i++) {
        	                        ((DataRow)List [i]).RowID = i;
	                        }
			}
				
			row.HasParentCollection = true;
			row.AttachRow ();
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Removes the specified DataRow from the internal list. Used by DataRow to commit the removing.
		/// </summary>
		internal void RemoveInternal (DataRow row) {
			if (row == null) {
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			}
			int index = List.IndexOf(row);
			if (index < 0) {
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			}
			List.RemoveAt(index);
		}

		/// <summary>
		/// Removes the specified DataRow from the collection.
		/// </summary>
		public void Remove (DataRow row) 
		{
			if (!List.Contains(row))
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");

			DataRowState state = row.RowState;
			if (state != DataRowState.Deleted &&
				state != DataRowState.Detached) {
				row.Delete();
				// if the row was in added state it will be in Detached state after the
				// delete operation, so we have to check it.
				if (row.RowState != DataRowState.Detached)
					row.AcceptChanges();
			}
		}

		/// <summary>
		/// Removes the row at the specified index from the collection.
		/// </summary>
		public void RemoveAt (int index) 
		{			
			Remove(this[index]);
		}

		///<summary>
		///Internal method used to validate a given DataRow with respect
		///to the DataRowCollection
		///</summary>
		[MonoTODO]
		internal void ValidateDataRowInternal(DataRow row)
		{
			//first check for null violations.
			row._nullConstraintViolation = true;
			row.CheckNullConstraints();

			int newRecord = (row.Proposed >= 0) ? row.Proposed : row.Current;
			foreach(Index index in table.Indexes) {
				index.Update(row,newRecord);
			}

			foreach(Constraint constraint in table.Constraints) {
				try {
					constraint.AssertConstraint(row);
				}
				catch(Exception e) {
					// remove row from indexes
					foreach(Index index in table.Indexes) {
						index.Delete(newRecord);
					}
					throw e;
				}
			}
		}
	}
}
