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
				if (index >= Count)
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
			

			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);
			
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
		public virtual DataRow Add (object[] values) 
		{
			if (values.Length > table.Columns.Count)
				throw new ArgumentException ("The array is larger than the number of columns in the table.");
			DataRow row = table.NewRow ();
			row.ItemArray = values;
			Add (row);
			return row;
		}

		/// <summary>
		/// Clears the collection of all rows.
		/// </summary>
		public void Clear () 
		{
			if (this.table.DataSet != null && this.table.DataSet.EnforceConstraints)
			{
				foreach (DataTable table in this.table.DataSet.Tables)
				{
					foreach (Constraint c in table.Constraints)
					{
						if (c is ForeignKeyConstraint)
						{
							if (((ForeignKeyConstraint) c).RelatedTable.Equals(this.table))
#if NET_1_1
								throw new InvalidConstraintException (String.Format ("Cannot clear table Parent because ForeignKeyConstraint {0} enforces Child.", c.ConstraintName));
#else
								throw new ArgumentException (String.Format ("Cannot clear table Parent because ForeignKeyConstraint {0} enforces Child.", c.ConstraintName));
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
			if (table.PrimaryKey.Length != keys.Length)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length + " value(s) for the key " + 
							     "being indexed, but received " + keys.Length + " value(s).");

			return Find (keys) != null;
		}

		/// <summary>
		/// Gets the row specified by the primary key value.
		/// </summary>
		public DataRow Find (object key) 
		{
			if (table.PrimaryKey.Length == 0)
				throw new MissingPrimaryKeyException ("Table doesn't have a primary key.");
			if (table.PrimaryKey.Length > 1)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length +" value(s) for the key being indexed, but received 1 value(s).");

			if (key == null)
#if NET_1_1
				return null;
#else
				throw new ArgumentException("Expecting 1 value(s) for the key being indexed, but received 0 value(s).");
#endif

			DataColumn primaryKey = table.PrimaryKey[0];
			Index primaryKeyIndex = table.GetIndexByColumns(table.PrimaryKey);
			int tmpRecord = table.RecordCache.NewRecord();
			try {
				primaryKey.DataContainer[tmpRecord] = key;

				// if we can search through index
				if (primaryKeyIndex != null) {
					// get the child rows from the index
					Node node = primaryKeyIndex.FindSimple(tmpRecord,1,true);
					if (node != null) {
						return node.Row;
					}
				}
			
				//loop through all collection rows			
				foreach (DataRow row in this) {
					if (row.RowState != DataRowState.Deleted) {
						int index = row.IndexFromVersion(DataRowVersion.Default); 
						if (primaryKey.DataContainer.CompareValues(index, tmpRecord) == 0) {
							return row;
						}
					}
				}
				return null;
			}
			finally {
				table.RecordCache.DisposeRecord(tmpRecord);
			}
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
			if (table.PrimaryKey.Length != keys.Length)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length +" value(s) for the key being indexed, but received " + keys.Length +  " value(s).");
                                                                                                    
			DataColumn[] primaryKey = table.PrimaryKey;
			int tmpRecord = table.RecordCache.NewRecord();
			try {
				int numColumn = keys.Length;
				for (int i = 0; i < numColumn; i++) {
					// according to MSDN: the DataType value for both columns must be identical.
					primaryKey[i].DataContainer[tmpRecord] = keys[i];
				}
				return Find(tmpRecord,numColumn);
			}
			finally {
				table.RecordCache.DisposeRecord(tmpRecord);
			}
		}

		internal DataRow Find(int index, int length)
		{
			DataColumn[] primaryKey = table.PrimaryKey;
			Index primaryKeyIndex = table.GetIndexByColumns(primaryKey);
			// if we can search through index
			if (primaryKeyIndex != null) {
				// get the child rows from the index
				Node node = primaryKeyIndex.FindSimple(index,length,true);
				if ( node != null ) {
					return node.Row;
				}
			}
		
			//loop through all collection rows			
			foreach (DataRow row in this) {
				if (row.RowState != DataRowState.Deleted) {
					int rowIndex = row.IndexFromVersion(DataRowVersion.Default);
					bool match = true;
					for (int columnCnt = 0; columnCnt < length; ++columnCnt) { 
						if (primaryKey[columnCnt].DataContainer.CompareValues(rowIndex, index) != 0) {
							match = false;
						}
					}
					if ( match ) {
						return row;
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
			if (row == null)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			int index = List.IndexOf(row);
			if (index < 0)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			row.Delete();
			// if the row was in added state it will be in Detached state after the
			// delete operation, so we have to check it.
			if (row.RowState != DataRowState.Detached)
				row.AcceptChanges();
		}

		/// <summary>
		/// Removes the row at the specified index from the collection.
		/// </summary>
		public void RemoveAt (int index) 
		{			
			if (index < 0 || index >= List.Count)
				throw new IndexOutOfRangeException ("There is no row at position " + index + ".");
			DataRow row = (DataRow)List [index];
			row.Delete();
			// if the row was in added state it will be in Detached state after the
			// delete operation, so we have to check it.
			if (row.RowState != DataRowState.Detached)
				row.AcceptChanges();
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
			// This validates constraints in the specific order : 
			// first unique/primary keys first, then Foreignkeys, etc
			ArrayList uniqueConstraintsDone = new ArrayList();
			ArrayList foreignKeyConstraintsDone = new ArrayList();
			try {
				foreach(Constraint constraint in table.Constraints.UniqueConstraints) {
					constraint.AssertConstraint(row);
					uniqueConstraintsDone.Add(constraint);
				}
			
				foreach(Constraint constraint in table.Constraints.ForeignKeyConstraints) {
					constraint.AssertConstraint(row);
					foreignKeyConstraintsDone.Add(constraint);
				}
			}
			// if one of the AssertConstraint failed - we need to "rollback" all the changes
			// caused by AssertCoinstraint calls already succeeded
			catch(ConstraintException e) {
				RollbackAsserts(row,foreignKeyConstraintsDone,uniqueConstraintsDone);
				throw e;
			}
			catch(InvalidConstraintException e) {	
				RollbackAsserts(row,foreignKeyConstraintsDone,uniqueConstraintsDone);
				throw e;
			}
		}

		private void RollbackAsserts(DataRow row,ICollection foreignKeyConstraintsDone,
			ICollection uniqueConstraintsDone)
		{
			// if any of constraints assert failed - 
			// we have to rollback all the asserts scceeded
			// on order reverse to thier original execution
			foreach(Constraint constraint in foreignKeyConstraintsDone) {
				constraint.RollbackAssert(row);
			}

			foreach(Constraint constraint in uniqueConstraintsDone) {
				constraint.RollbackAssert(row);
			}
		}
	}
}
