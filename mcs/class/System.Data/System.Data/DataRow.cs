//
// System.Data.DataRow.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//   Alan Tam Siu Lung <Tam@SiuLung.com>
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002, 2003
// Copyright (C) 2002 Tim Coleman
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
using System.Globalization;
using System.Xml;

namespace System.Data {
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
	[Serializable]
	public class DataRow
	{
		#region Fields

		private DataTable _table;

		internal int _original = -1;
		internal int _current = -1;
		internal int _proposed = -1;

		private ArrayList _columnErrors;
		private string rowError;
		private DataRowState rowState;
		internal int xmlRowID = 0;
		internal bool _nullConstraintViolation;
		private string _nullConstraintMessage;
		private bool editing = false;
		private bool _hasParentCollection;
		private bool _inChangingEvent;
		private int _rowId;

		private XmlDataDocument.XmlDataElement mappedElement;
		internal bool _inExpressionEvaluation = false;

		#endregion // Fields

		#region Constructors

		/// <summary>
		/// This member supports the .NET Framework infrastructure and is not intended to be 
		/// used directly from your code.
		/// </summary>
		protected internal DataRow (DataRowBuilder builder)
		{
			_table = builder.Table;
			// Get the row id from the builder.
			_rowId = builder._rowId;

			_proposed = _table.RecordCache.NewRecord();
			// Initialise the data columns of the row with the dafault values, if any 
			// TODO : should proposed version be available immediately after record creation ?
			foreach(DataColumn column in _table.Columns) {
				column.DataContainer.CopyValue(_table.DefaultValuesRowIndex,_proposed);
			}
			
			rowError = String.Empty;

			//on first creating a DataRow it is always detached.
			rowState = DataRowState.Detached;
			
			ArrayList aiColumns = _table.Columns.AutoIncrmentColumns;
			foreach (DataColumn dc in aiColumns) {
				this [dc] = dc.AutoIncrementValue();
			}

			// create mapped XmlDataElement
			DataSet ds = _table.DataSet;
			if (ds != null && ds._xmlDataDocument != null)
				mappedElement = new XmlDataDocument.XmlDataElement (this, _table.Prefix, _table.TableName, _table.Namespace, ds._xmlDataDocument);
		}

		internal DataRow(DataTable table,int rowId)
		{
			_table = table;
			_rowId = rowId;
		}

		#endregion // Constructors

		#region Properties

		private ArrayList ColumnErrors
		{
			get {
				if (_columnErrors == null) {
					_columnErrors = new ArrayList();
				}
				return _columnErrors;
			}

			set {
				_columnErrors = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether there are errors in a row.
		/// </summary>
		public bool HasErrors {
			get {
				if (RowError != string.Empty)
					return true;

				foreach(String columnError in ColumnErrors) {
					if (columnError != null && columnError != string.Empty) {
						return true;
				}
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in the column specified by name.
		/// </summary>
		public object this[string columnName] {
			get { return this[columnName, DataRowVersion.Default]; }
			set {
				int columnIndex = _table.Columns.IndexOf (columnName);
				if (columnIndex == -1)
					throw new IndexOutOfRangeException ();
				this[columnIndex] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in specified DataColumn
		/// </summary>
		public object this[DataColumn column] {

			get {
				return this[column, DataRowVersion.Default];} 
			set {
				int columnIndex = _table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ("The column does not belong to this table.");
				this[columnIndex] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in column specified by index.
		/// </summary>
		public object this[int columnIndex] {
			get { return this[columnIndex, DataRowVersion.Default]; }
			set {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();

				DataColumn column = _table.Columns[columnIndex];
				_table.ChangingDataColumn (this, column, value);
				
				if (value == null && column.DataType != typeof(string)) {
					throw new ArgumentException("Cannot set column " + column.ColumnName + " to be null, Please use DBNull instead");
				}
				
				CheckValue (value, column);

				bool orginalEditing = editing;
				if (!orginalEditing) {
					BeginEdit ();
				}
				
				column[_proposed] = value;
				_table.ChangedDataColumn (this, column, value);
				if (!orginalEditing) {
					EndEdit ();
				}
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the named column.
		/// </summary>
		public object this[string columnName, DataRowVersion version] {
			get {
				int columnIndex = _table.Columns.IndexOf (columnName);
				if (columnIndex == -1)
					throw new IndexOutOfRangeException ();
				return this[columnIndex, version];
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the specified DataColumn.
		/// </summary>
		public object this[DataColumn column, DataRowVersion version] {
			get {
				if (column.Table != Table)
					throw new ArgumentException ("The column does not belong to this table.");
				int columnIndex = column.Ordinal;
				return this[columnIndex, version];
			}
		}

		/// <summary>
		/// Gets the data stored in the column, specified by index and version of the data to
		/// retrieve.
		/// </summary>
		public object this[int columnIndex, DataRowVersion version] {
			get {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();
				// Accessing deleted rows
				if (!_inExpressionEvaluation && rowState == DataRowState.Deleted && version != DataRowVersion.Original)
					throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");
				
				DataColumn column = _table.Columns[columnIndex];
				if (column.Expression != String.Empty) {
					object o = column.CompiledExpression.Eval (this);
					return Convert.ChangeType (o, column.DataType);
				}
				
				int recordIndex = IndexFromVersion(version);

				if (recordIndex >= 0) {
					return column[recordIndex];
				}

				if (rowState == DataRowState.Detached && version == DataRowVersion.Default && _proposed < 0)
					throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
				
				throw new VersionNotFoundException (Locale.GetText ("There is no " + version.ToString () + " data to access."));
			}
		}
		
		/// <summary>
		/// Gets or sets all of the values for this row through an array.
		/// </summary>
		public object[] ItemArray {
			get { 
				// row not in table
				if (rowState == DataRowState.Detached)
					throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
				// Accessing deleted rows
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");
				
				object[] items = new object[_table.Columns.Count];
				foreach(DataColumn column in _table.Columns) {
					items[column.Ordinal] = column[_current];
				}
				return items;
			}
			set {
				if (value.Length > _table.Columns.Count)
					throw new ArgumentException ();

				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				
				bool orginalEditing = editing;
				if (!orginalEditing) { 
					BeginEdit ();
				}
				object newVal = null;
				DataColumnChangeEventArgs e = new DataColumnChangeEventArgs();
				foreach(DataColumn column in _table.Columns) {
					int i = column.Ordinal;
					newVal = (i < value.Length) ? value[i] : null;
					
					e.Initialize(this, column, newVal);
					_table.RaiseOnColumnChanged(e);
					CheckValue (e.ProposedValue, column);
					column[_proposed] = e.ProposedValue;
				}
				if (!orginalEditing) {
					EndEdit ();
				}
			}
		}

		/// <summary>
		/// Gets the current state of the row in regards to its relationship to the
		/// DataRowCollection.
		/// </summary>
		public DataRowState RowState {
			get { 
				return rowState; 
			}
		}

		/// <summary>
		/// Gets the DataTable for which this row has a schema.
		/// </summary>
		public DataTable Table {
			get { 
				return _table; 
			}
		}

		/// <summary>
		/// Gets and sets index of row. This is used from 
		/// XmlDataDocument.
		// </summary>
		internal int XmlRowID {
			get { 
				return xmlRowID; 
			}
			set { 
				xmlRowID = value; 
			}
		}
		
		/// <summary>
		/// Gets and sets index of row.
		// </summary>
		internal int RowID {
			get { 
				return _rowId; 
			}
			set { 
				_rowId = value; 
			}
		}

		#endregion

		#region Methods

		//FIXME?: Couldn't find a way to set the RowState when adding the DataRow
		//to a Datatable so I added this method. Delete if there is a better way.
		internal void AttachRow() {
			if (_current >= 0) {
				Table.RecordCache.DisposeRecord(_current);
			}
			_current = _proposed;
			_proposed = -1;
			rowState = DataRowState.Added;
		}

		//FIXME?: Couldn't find a way to set the RowState when removing the DataRow
		//from a Datatable so I added this method. Delete if there is a better way.
		internal void DetachRow() {
			if (_proposed >= 0) {
				_table.RecordCache.DisposeRecord(_proposed);
				_proposed = -1;
			}
			_rowId = -1;
			_hasParentCollection = false;
			rowState = DataRowState.Detached;
		}

		private void CheckValue (object v, DataColumn col) 
		{		
			if (_hasParentCollection && col.ReadOnly) {
				throw new ReadOnlyException ();
			}

			if (v == null || v == DBNull.Value) {
				if (col.AllowDBNull || col.AutoIncrement || col.DefaultValue != DBNull.Value) {
					return;
				}

				//Constraint violations during data load is raise in DataTable EndLoad
				this._nullConstraintViolation = true;
				if (this.Table._duringDataLoad) {
					this.Table._nullConstraintViolationDuringDataLoad = true;
				}
				_nullConstraintMessage = "Column '" + col.ColumnName + "' does not allow nulls.";
			
			}
		}

		internal void SetValuesFromDataRecord(IDataRecord record, int[] mapping)
		{
			if ( mapping.Length > Table.Columns.Count)
				throw new ArgumentException ();

//			bool orginalEditing = editing;
//			if (!orginalEditing) { 
//				BeginEdit ();
//			}
			
			if (!HasVersion(DataRowVersion.Proposed)) {
				_proposed = Table.RecordCache.NewRecord();
			}

			try {
				for(int i=0; i < mapping.Length; i++) {
					DataColumn column = Table.Columns[i];
					column.DataContainer.SetItemFromDataRecord(_proposed, record,mapping[i]);
					if ( column.AutoIncrement ) { 
						column.UpdateAutoIncrementValue(column.DataContainer.GetInt64(_proposed));
					}
				}
			}
			catch (Exception e){
				Table.RecordCache.DisposeRecord(_proposed);
				_proposed = -1;
				throw e;
			}

//			if (!orginalEditing) {
//				EndEdit ();
//			}
		}

		/// <summary>
		/// Gets or sets the custom error description for a row.
		/// </summary>
		public string RowError {
			get { 
				return rowError; 
			}
			set { 
				rowError = value; 
			}
		}

		internal int IndexFromVersion(DataRowVersion version)
		{
			if (HasVersion(version))
			{
				int recordIndex;
				switch (version) {
					case DataRowVersion.Default:
						if (editing || rowState == DataRowState.Detached) {
							recordIndex = _proposed;
						}
						else {
							recordIndex = _current;
						}
						break;
					case DataRowVersion.Proposed:
						recordIndex = _proposed;
						break;
					case DataRowVersion.Current:
						recordIndex = _current;
						break;
					case DataRowVersion.Original:
						recordIndex = _original;
						break;
					default:
						throw new ArgumentException ();
				}
				return recordIndex;
			}
			return -1;
		}

		internal XmlDataDocument.XmlDataElement DataElement {
			get { return mappedElement; }
			set { mappedElement = value; }
		}

		internal void SetOriginalValue (string columnName, object val)
		{
			DataColumn column = _table.Columns[columnName];
			_table.ChangingDataColumn (this, column, val);
				
                        if (_original < 0 || _original == _current) { 
				// really add a row cache, if _original is not there & 
				// make row modified
				_original = Table.RecordCache.NewRecord();
			}
			CheckValue (val, column);
			column[_original] = val;
			rowState = DataRowState.Modified;
		}

		/// <summary>
		/// Commits all the changes made to this row since the last time AcceptChanges was
		/// called.
		/// </summary>
		public void AcceptChanges () 
		{
			EndEdit(); // in case it hasn't been called
			switch (rowState) {
				case DataRowState.Unchanged:
					return;
			case DataRowState.Added:
			case DataRowState.Modified:
				rowState = DataRowState.Unchanged;
				break;
			case DataRowState.Deleted:
				_table.Rows.RemoveInternal (this);
				DetachRow();
				break;
			case DataRowState.Detached:
				throw new RowNotInTableException("Cannot perform this operation on a row not in the table.");
			}
			// Accept from detached
			if (_original >= 0) {
				Table.RecordCache.DisposeRecord(_original);
			}
			_original = _current;
		}

		/// <summary>
		/// Begins an edit operation on a DataRow object.
		/// </summary>
		public void BeginEdit () 
		{
			if (_inChangingEvent)
                                throw new InRowChangingEventException("Cannot call BeginEdit inside an OnRowChanging event.");
			if (rowState == DataRowState.Deleted)
				throw new DeletedRowInaccessibleException ();
			if (!HasVersion (DataRowVersion.Proposed)) {
				_proposed = Table.RecordCache.NewRecord();
				foreach(DataColumn column in Table.Columns) {
					column.DataContainer.CopyValue(_current,_proposed);
				}
			}
			// setting editing to true stops validations on the row
			editing = true;
		}

		/// <summary>
		/// Cancels the current edit on the row.
		/// </summary>
		public void CancelEdit () 
		{
			 if (_inChangingEvent)
                                throw new InRowChangingEventException("Cannot call CancelEdit inside an OnRowChanging event.");
			editing = false;
			if (HasVersion (DataRowVersion.Proposed)) {
				Table.RecordCache.DisposeRecord(_proposed);
				_proposed = -1;
				if (rowState == DataRowState.Modified) {
				    rowState = DataRowState.Unchanged;
				}
			}
		}

		/// <summary>
		/// Clears the errors for the row, including the RowError and errors set with
		/// SetColumnError.
		/// </summary>
		public void ClearErrors () 
		{
			rowError = String.Empty;
			ColumnErrors.Clear();
		}

		/// <summary>
		/// Deletes the DataRow.
		/// </summary>
		public void Delete () 
		{
			_table.DeletingDataRow(this, DataRowAction.Delete);
			switch (rowState) {
			case DataRowState.Added:
				// check what to do with child rows
				CheckChildRows(DataRowAction.Delete);
				_table.DeleteRowFromIndexes (this);
				Table.Rows.RemoveInternal (this);

				// if row was in Added state we move it to Detached.
				DetachRow();
				break;
			case DataRowState.Deleted:
				break;		
			default:
				// check what to do with child rows
				CheckChildRows(DataRowAction.Delete);
				_table.DeleteRowFromIndexes (this);
				rowState = DataRowState.Deleted;
				break;
			}
			_table.DeletedDataRow(this, DataRowAction.Delete);
		}

		// check the child rows of this row before deleting the row.
		private void CheckChildRows(DataRowAction action)
		{
			
			// in this method we find the row that this row is in a relation with them.
			// in shortly we find all child rows of this row.
			// then we function according to the DeleteRule of the foriegnkey.


			// 1. find if this row is attached to dataset.
			// 2. find if EnforceConstraints is true.
			// 3. find if there are any constraint on the table that the row is in.
			if (_table.DataSet != null && _table.DataSet.EnforceConstraints && _table.Constraints.Count > 0)
			{
				foreach (DataTable table in _table.DataSet.Tables)
				{
					// loop on all ForeignKeyConstrain of the table.
					foreach (ForeignKeyConstraint fk in table.Constraints.ForeignKeyConstraints)
					{
						if (fk.RelatedTable == _table)
						{
							Rule rule;
							if (action == DataRowAction.Delete)
								rule = fk.DeleteRule;
							else
								rule = fk.UpdateRule;
							CheckChildRows(fk, action, rule);
						}			
					}
				}
			}
		}

		private void CheckChildRows(ForeignKeyConstraint fkc, DataRowAction action, Rule rule)
		{				
			DataRow[] childRows = GetChildRows(fkc, DataRowVersion.Default);
			switch (rule)
			{
				case Rule.Cascade:  // delete or change all relted rows.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							// if action is delete we delete all child rows
							if (action == DataRowAction.Delete)
							{
								if (childRows[j].RowState != DataRowState.Deleted)
									childRows[j].Delete();
							}
							// if action is change we change the values in the child row
							else if (action == DataRowAction.Change)
							{
								// change only the values in the key columns
								// set the childcolumn value to the new parent row value
								for (int k = 0; k < fkc.Columns.Length; k++)
									childRows[j][fkc.Columns[k]] = this[fkc.RelatedColumns[k], DataRowVersion.Proposed];
							}
						}
					}
					break;
				case Rule.None: // throw an exception if there are any child rows.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							if (childRows[j].RowState != DataRowState.Deleted)
							{
								string changeStr = "Cannot change this row because constraints are enforced on relation " + fkc.ConstraintName +", and changing this row will strand child rows.";
								string delStr = "Cannot delete this row because constraints are enforced on relation " + fkc.ConstraintName +", and deleting this row will strand child rows.";
								string message = action == DataRowAction.Delete ? delStr : changeStr;
								throw new InvalidConstraintException(message);
							}
						}
					}
					break;
				case Rule.SetDefault: // set the values in the child rows to the defult value of the columns.
					if (childRows != null && childRows.Length > 0) {
						int defaultValuesRowIndex = childRows[0].Table.DefaultValuesRowIndex;
						foreach(DataRow childRow in childRows) {
							if (childRow.RowState != DataRowState.Deleted) {
								int defaultIdx = childRow.IndexFromVersion(DataRowVersion.Default);
								foreach(DataColumn column in fkc.Columns) {
									column.DataContainer.CopyValue(defaultValuesRowIndex,defaultIdx);
								}
							}
						}
					}
					break;
				case Rule.SetNull: // set the values in the child row to null.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							DataRow child = childRows[j];
							if (childRows[j].RowState != DataRowState.Deleted)
							{
								// set only the key columns to DBNull
								for (int k = 0; k < fkc.Columns.Length; k++)
									child.SetNull(fkc.Columns[k]);
							}
						}
					}
					break;
			}

		}

		/// <summary>
		/// Ends the edit occurring on the row.
		/// </summary>
		public void EndEdit () 
		{
			if (_inChangingEvent)
				throw new InRowChangingEventException("Cannot call EndEdit inside an OnRowChanging event.");
			if (rowState == DataRowState.Detached)
			{
				editing = false;
				return;
			}
			
			CheckReadOnlyStatus();
			if (HasVersion (DataRowVersion.Proposed))
			{
				_inChangingEvent = true;
				try
				{
					_table.ChangingDataRow(this, DataRowAction.Change);
				}
				finally
				{
					_inChangingEvent = false;
				}
				if (rowState == DataRowState.Unchanged)
					rowState = DataRowState.Modified;
				
				//Calling next method validates UniqueConstraints
				//and ForeignKeys.
				try
				{
					if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
						_table.Rows.ValidateDataRowInternal(this);
				}
				catch (Exception e)
				{
					editing = false;
					Table.RecordCache.DisposeRecord(_proposed);
					_proposed = -1;
					throw e;
				}

				// Now we are going to check all child rows of current row.
				// In the case the cascade is true the child rows will look up for
				// parent row. since lookup in index is always on current,
				// we have to move proposed version of current row to current
				// in the case of check child row failure we are rolling 
				// current row state back.
				int backup = _current;
				_current = _proposed;
				bool editing_backup = editing;
				editing = false;
				try {
					// check all child rows.
					CheckChildRows(DataRowAction.Change);
					_proposed = -1;
					if (_original != backup) {
						Table.RecordCache.DisposeRecord(backup);
					}
				}
				catch (Exception ex) {
					// if check child rows failed - rollback to previous state
					// i.e. restore proposed and current versions
					_proposed = _current;
					_current = backup;
					editing = editing_backup;
					// since we failed - propagate an exception
					throw ex;
				}
				_table.ChangedDataRow(this, DataRowAction.Change);
			}
		}

		/// <summary>
		/// Gets the child rows of this DataRow using the specified DataRelation.
		/// </summary>
		public DataRow[] GetChildRows (DataRelation relation) 
		{
			return GetChildRows (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow[] GetChildRows (string relationName) 
		{
			return GetChildRows (Table.DataSet.Relations[relationName]);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (DataRelation relation, DataRowVersion version) 
		{
			if (relation == null)
				return new DataRow[0];

			if (this.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException();

			if (relation.ChildKeyConstraint != null)
				return GetChildRows (relation.ChildKeyConstraint, version);

			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version))
			{
				object[] vals = new object[parentColumns.Length];
				for (int i = 0; i < vals.Length; i++)
					vals[i] = this[parentColumns[i], version];
				
				foreach (DataRow row in relation.ChildTable.Rows) 
				{
					bool allColumnsMatch = false;
					if (row.HasVersion(DataRowVersion.Default))
					{
						allColumnsMatch = true;
						for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) 
						{
							if (!vals[columnCnt].Equals(
								row[childColumns[columnCnt], DataRowVersion.Default])) 
							{
								allColumnsMatch = false;
								break;
							}
						}
					}
					if (allColumnsMatch) rows.Add(row);
				}
			}
			DataRow[] result = relation.ChildTable.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (string relationName, DataRowVersion version) 
		{
			return GetChildRows (Table.DataSet.Relations[relationName], version);
		}

		private DataRow[] GetChildRows (ForeignKeyConstraint fkc, DataRowVersion version) 
		{
			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = fkc.RelatedColumns;
			DataColumn[] childColumns = fkc.Columns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version)) {
				Index index = fkc.Index;
				if (index != null) {
					// get the child rows from the index
					Node[] childNodes = index.FindAllSimple (parentColumns, IndexFromVersion(version));
					for (int i = 0; i < childNodes.Length; i++) {
						rows.Add (childNodes[i].Row);
					}
				}
				else { // if there is no index we search manualy.
					int curIndex = IndexFromVersion(DataRowVersion.Current);
					int tmpRecord = fkc.Table.RecordCache.NewRecord();

					try {
						for (int i = 0; i < numColumn; i++) {
							// according to MSDN: the DataType value for both columns must be identical.
							childColumns[i].DataContainer.CopyValue(parentColumns[i].DataContainer, curIndex, tmpRecord);
						}

						foreach (DataRow row in fkc.Table.Rows) {
							bool allColumnsMatch = false;
							if (row.HasVersion(DataRowVersion.Default)) {
								allColumnsMatch = true;
								int childIndex = row.IndexFromVersion(DataRowVersion.Default);
								for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) {
									if (childColumns[columnCnt].DataContainer.CompareValues(childIndex, tmpRecord) != 0) {
										allColumnsMatch = false;
										break;
									}
								}
							}
							if (allColumnsMatch) {
								rows.Add(row);
							}
						}
					}
					finally {
						fkc.Table.RecordCache.DisposeRecord(tmpRecord);
					}
				}
			}

			DataRow[] result = fkc.Table.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the error description of the specified DataColumn.
		/// </summary>
		public string GetColumnError (DataColumn column) 
		{
			return GetColumnError (_table.Columns.IndexOf(column));
		}

		/// <summary>
		/// Gets the error description for the column specified by index.
		/// </summary>
		public string GetColumnError (int columnIndex) 
		{
			if (columnIndex < 0 || columnIndex >= Table.Columns.Count)
				throw new IndexOutOfRangeException ();

			string retVal = null;
			if (columnIndex < ColumnErrors.Count) {
				retVal = (String) ColumnErrors[columnIndex];
			}
			return (retVal != null) ? retVal : String.Empty;
		}

		/// <summary>
		/// Gets the error description for the column, specified by name.
		/// </summary>
		public string GetColumnError (string columnName) 
		{
			return GetColumnError (_table.Columns.IndexOf(columnName));
		}

		/// <summary>
		/// Gets an array of columns that have errors.
		/// </summary>
		public DataColumn[] GetColumnsInError () 
		{
			ArrayList dataColumns = new ArrayList ();

			int columnOrdinal = 0;
			foreach(String columnError in ColumnErrors) {
				if (columnError != null && columnError != String.Empty) {
					dataColumns.Add (_table.Columns[columnOrdinal]);
				}
				columnOrdinal++;
			}

			return (DataColumn[])(dataColumns.ToArray (typeof(DataColumn)));
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation) 
		{
			return GetParentRow (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow GetParentRow (string relationName) 
		{
			return GetParentRow (relationName, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation, DataRowVersion version) 
		{
			DataRow[] rows = GetParentRows(relation, version);
			if (rows.Length == 0) return null;
			return rows[0];
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (string relationName, DataRowVersion version) 
		{
			return GetParentRow (Table.DataSet.Relations[relationName], version);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow[] GetParentRows (DataRelation relation) 
		{
			return GetParentRows (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation.
		/// </summary>
		public DataRow[] GetParentRows (string relationName) 
		{
			return GetParentRows (relationName, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow[] GetParentRows (DataRelation relation, DataRowVersion version) 
		{
			// TODO: Caching for better preformance
			if (relation == null)
				return new DataRow[0];

			if (this.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException();

			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version)) {
				Index indx = relation.ParentTable.GetIndexByColumns (parentColumns);
				if (indx != null &&
				    (Table == null || Table.DataSet == null ||   
				   Table.DataSet.EnforceConstraints)) { // get the child rows from the index
					Node[] childNodes = indx.FindAllSimple(childColumns, IndexFromVersion(version));
					for (int i = 0; i < childNodes.Length; i++) {
						rows.Add (childNodes[i].Row);
					}
				}
				else { // no index so we have to search manualy.
					int curIndex = IndexFromVersion(DataRowVersion.Current);
					int tmpRecord = relation.ParentTable.RecordCache.NewRecord();

					try {
						for (int i = 0; i < numColumn; i++) {
							// according to MSDN: the DataType value for both columns must be identical.
							parentColumns[i].DataContainer.CopyValue(childColumns[i].DataContainer, curIndex, tmpRecord);
						}

						foreach (DataRow row in relation.ParentTable.Rows) {
							bool allColumnsMatch = false;
							if (row.HasVersion(DataRowVersion.Default)) {
								allColumnsMatch = true;
								int parentIndex = row.IndexFromVersion(DataRowVersion.Default);
								for (int columnCnt = 0; columnCnt < numColumn; columnCnt++) {
									if (parentColumns[columnCnt].DataContainer.CompareValues(parentIndex, tmpRecord) != 0) {
										allColumnsMatch = false;
										break;
									}
								}
							}
							if (allColumnsMatch) {
								rows.Add(row);
							}
						}
					}
					finally {
						relation.ParentTable.RecordCache.DisposeRecord(tmpRecord);
					}
				}
			}

			DataRow[] result = relation.ParentTable.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow[] GetParentRows (string relationName, DataRowVersion version) 
		{
			return GetParentRows (Table.DataSet.Relations[relationName], version);
		}

		/// <summary>
		/// Gets a value indicating whether a specified version exists.
		/// </summary>
		public bool HasVersion (DataRowVersion version) 
		{
			switch (version) {
				case DataRowVersion.Default:
					if (rowState == DataRowState.Deleted && !_inExpressionEvaluation)
						return false;
					if (rowState == DataRowState.Detached)
						return _proposed >= 0;
					return true;
				case DataRowVersion.Proposed:
					if (rowState == DataRowState.Deleted && !_inExpressionEvaluation)
						return false;
					return _proposed >= 0;
				case DataRowVersion.Current:
					if ((rowState == DataRowState.Deleted && !_inExpressionEvaluation) || rowState == DataRowState.Detached)
						return false;
					return _current >= 0;
				case DataRowVersion.Original:
					if (rowState == DataRowState.Detached)
						return false;
					return _original >= 0;
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column) 
		{
			return IsNull(column, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets a value indicating whether the column at the specified index contains a null
		/// value.
		/// </summary>
		public bool IsNull (int columnIndex) 
		{
			return IsNull(Table.Columns[columnIndex]);
		}

		/// <summary>
		/// Gets a value indicating whether the named column contains a null value.
		/// </summary>
		public bool IsNull (string columnName) 
		{
			return IsNull(Table.Columns[columnName]);
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn and DataRowVersion
		/// contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column, DataRowVersion version) 
		{
			return column.DataContainer.IsNull(IndexFromVersion(version));
		}

		/// <summary>
		/// Returns a value indicating whether all of the row columns specified contain a null value.
		/// </summary>
		internal bool IsNullColumns(DataColumn[] columns)
		{
			bool allNull = true;
			for (int i = 0; i < columns.Length; i++) 
			{
				if (!IsNull(columns[i])) 
				{
					allNull = false;
					break;
				}
			}
			return allNull;
		}

		/// <summary>
		/// Rejects all changes made to the row since AcceptChanges was last called.
		/// </summary>
		public void RejectChanges () 
		{
			if (RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
			// If original is null, then nothing has happened since AcceptChanges
			// was last called.  We have no "original" to go back to.
			if (HasVersion(DataRowVersion.Original)) {
				if (_current >= 0 ) {
					Table.RecordCache.DisposeRecord(_current);
				}
				_current = _original;
			       
				_table.ChangedDataRow (this, DataRowAction.Rollback);
				CancelEdit ();
				switch (rowState)
				{
					case DataRowState.Added:
						_table.DeleteRowFromIndexes (this);
						_table.Rows.RemoveInternal (this);
						break;
					case DataRowState.Modified:
						if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
							_table.Rows.ValidateDataRowInternal(this);
						rowState = DataRowState.Unchanged;
						break;
					case DataRowState.Deleted:
						rowState = DataRowState.Unchanged;
						if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
							_table.Rows.ValidateDataRowInternal(this);
						break;
				} 
				
			} 			
			else {
				// If rows are just loaded via Xml the original values are null.
				// So in this case we have to remove all columns.
				// FIXME: I'm not realy sure, does this break something else, but
				// if so: FIXME ;)
				
				if ((rowState & DataRowState.Added) > 0)
				{
					_table.DeleteRowFromIndexes (this);
					_table.Rows.RemoveInternal (this);
					// if row was in Added state we move it to Detached.
					DetachRow();
				}
			}
		}

		/// <summary>
		/// Sets the error description for a column specified as a DataColumn.
		/// </summary>
		public void SetColumnError (DataColumn column, string error) 
		{
			SetColumnError (_table.Columns.IndexOf (column), error);
		}

		/// <summary>
		/// Sets the error description for a column specified by index.
		/// </summary>
		public void SetColumnError (int columnIndex, string error) 
		{
			if (columnIndex < 0 || columnIndex >= Table.Columns.Count)
				throw new IndexOutOfRangeException ();

			while(ColumnErrors.Count < columnIndex) {
				ColumnErrors.Add(null);
			}
			ColumnErrors.Add(error);
		}

		/// <summary>
		/// Sets the error description for a column specified by name.
		/// </summary>
		public void SetColumnError (string columnName, string error) 
		{
			SetColumnError (_table.Columns.IndexOf (columnName), error);
		}

		/// <summary>
		/// Sets the value of the specified DataColumn to a null value.
		/// </summary>
		protected void SetNull (DataColumn column) 
		{
			this[column] = DBNull.Value;
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow.
		/// </summary>
		public void SetParentRow (DataRow parentRow) 
		{
			SetParentRow(parentRow, null);
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow and
		/// DataRelation.
		/// </summary>
		public void SetParentRow (DataRow parentRow, DataRelation relation) 
		{
			if (_table == null || parentRow.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (parentRow != null && _table.DataSet != parentRow.Table.DataSet)
				throw new ArgumentException();
			
			BeginEdit();
			if (relation == null)
			{
				foreach (DataRelation parentRel in _table.ParentRelations)
				{
					DataColumn[] childCols = parentRel.ChildKeyConstraint.Columns;
					DataColumn[] parentCols = parentRel.ChildKeyConstraint.RelatedColumns;
					
					for (int i = 0; i < parentCols.Length; i++)
					{
						if (parentRow == null)
							this[childCols[i].Ordinal] = DBNull.Value;
						else
							this[childCols[i].Ordinal] = parentRow[parentCols[i]];
					}
					
				}
			}
			else
			{
				DataColumn[] childCols = relation.ChildKeyConstraint.Columns;
				DataColumn[] parentCols = relation.ChildKeyConstraint.RelatedColumns;
					
				for (int i = 0; i < parentCols.Length; i++)
				{
					if (parentRow == null)
						this[childCols[i].Ordinal] = DBNull.Value;
					else
						this[childCols[i].Ordinal] = parentRow[parentCols[i]];
				}
			}
			EndEdit();
		}
		
		//Copy all values of this DataaRow to the row parameter.
		internal void CopyValuesToRow(DataRow row)
		{
			if (row == null)
				throw new ArgumentNullException("row");
			if (row == this)
				throw new ArgumentException("'row' is the same as this object");

			foreach(DataColumn column in Table.Columns) {
				DataColumn targetColumn = row.Table.Columns[column.ColumnName];
				//if a column with the same name exists in both rows copy the values
				if(targetColumn != null) {
					int index = targetColumn.Ordinal;
					if (HasVersion(DataRowVersion.Original)) {
						if (row._original < 0) {
							row._original = row.Table.RecordCache.NewRecord();
						}
						object val = column[_original];
						row.CheckValue(val, targetColumn);
						targetColumn[row._original] = val;
					}
					if (HasVersion(DataRowVersion.Current)) {
						if (row._current < 0) {
							row._current = row.Table.RecordCache.NewRecord();
						}
						object val = column[_current];
						row.CheckValue(val, targetColumn);
						targetColumn[row._current] = val;
					}
					if (HasVersion(DataRowVersion.Proposed)) {
						if (row._proposed < 0) {
							row._proposed = row.Table.RecordCache.NewRecord();
						}
						object val = column[row._proposed];
						row.CheckValue(val, targetColumn);
						targetColumn[row._proposed] = val;
					}
					
					//Saving the current value as the column value
					row[index] = targetColumn[row._current];
					
				}
			}
			CopyState(row);
		}

		// Copy row state - rowState and errors
		internal void CopyState(DataRow row)
		{
			row.rowState = RowState;
			row.RowError = RowError;
			row.ColumnErrors = (ArrayList)ColumnErrors.Clone();
		}

		internal bool IsRowChanged(DataRowState rowState) {
			if((RowState & rowState) != 0)
				return true;

			//we need to find if child rows of this row changed.
			//if yes - we should return true

			// if the rowState is deleted we should get the original version of the row
			// else - we should get the current version of the row.
			DataRowVersion version = (rowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
			int count = Table.ChildRelations.Count;
			for (int i = 0; i < count; i++){
				DataRelation rel = Table.ChildRelations[i];
				DataRow[] childRows = GetChildRows(rel, version);
				for (int j = 0; j < childRows.Length; j++){
					if (childRows[j].IsRowChanged(rowState))
						return true;
				}
			}

			return false;
		}

		internal bool HasParentCollection
		{
			get
			{
				return _hasParentCollection;
			}
			set
			{
				_hasParentCollection = value;
			}
		}

		internal void CheckNullConstraints()
		{
			if (_nullConstraintViolation) {
				if (HasVersion(DataRowVersion.Proposed)) {
					foreach(DataColumn column in Table.Columns) {
						if (IsNull(column) && !column.AllowDBNull) {
							throw new NoNullAllowedException(_nullConstraintMessage);
					}
				}
				}
				_nullConstraintViolation = false;
			}
		}
		
		internal void CheckReadOnlyStatus()
                {
			if (HasVersion(DataRowVersion.Proposed)) {
				int defaultIdx = IndexFromVersion(DataRowVersion.Default); 
				foreach(DataColumn column in Table.Columns) {
					if ((column.DataContainer.CompareValues(defaultIdx,_proposed) != 0) && column.ReadOnly) {
        	                        throw new ReadOnlyException();
                        }
                }
			}                       
                }
	
		#endregion // Methods
	}

	

}
