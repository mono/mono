//
// System.Data.DataRow.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//   Alan Tam Siu Lung <Tam@SiuLung.com>
//   Sureshkumar T <tsureshkumar@novell.com>
//   Veerapuram Varadhan <vvaradhan@novell.com>
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002, 2003
// Copyright (C) 2002 Tim Coleman
//

//
// Copyright (C) 2004-2009 Novell, Inc (http://www.novell.com)
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
using System.Data.Common;
using System.Collections;
using System.Globalization;
using System.Xml;
#if NET_2_0
using System.ComponentModel;
#endif

namespace System.Data {
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
#if !NET_2_0
	[Serializable]
#endif
	public class DataRow {
		#region Fields

		private DataTable _table;

		internal int _original = -1;
		internal int _current = -1;
		internal int _proposed = -1;

		private ArrayList _columnErrors;
		private string rowError;
		internal int xmlRowID = 0;
		internal bool _nullConstraintViolation;
		private string _nullConstraintMessage;
		private bool _inChangingEvent;
		private int _rowId;
		internal bool _rowChanged = false;

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

			rowError = String.Empty;
		}

		internal DataRow (DataTable table, int rowId)
		{
			_table = table;
			_rowId = rowId;
		}

		#endregion // Constructors

		#region Properties

		private ArrayList ColumnErrors {
			get {
				if (_columnErrors == null)
					_columnErrors = new ArrayList ();
				return _columnErrors;
			}

			set { _columnErrors = value; }
		}

		/// <summary>
		/// Gets a value indicating whether there are errors in a row.
		/// </summary>
		public bool HasErrors {
			get {
				if (RowError != string.Empty)
					return true;

				foreach (String columnError in ColumnErrors) {
					if (columnError != null && columnError != string.Empty)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in the column specified by name.
		/// </summary>
		public object this [string columnName] {
			get { return this [columnName, DataRowVersion.Default]; }
			set {
				DataColumn column = _table.Columns [columnName];
				if (column == null)
					throw new ArgumentException ("The column '" + columnName +
						"' does not belong to the table : " + _table.TableName);
				this [column.Ordinal] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in specified DataColumn
		/// </summary>
		public object this [DataColumn column] {
			get { return this [column, DataRowVersion.Default]; }
			set {
				if (column == null)
					throw new ArgumentNullException ("column");
				int columnIndex = _table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
						"The column '{0}' does not belong to the table : {1}.",
						column.ColumnName, _table.TableName));
				this [columnIndex] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in column specified by index.
		/// </summary>
		public object this [int columnIndex] {
			get { return this [columnIndex, DataRowVersion.Default]; }
			set {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();
				if (RowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();

				DataColumn column = _table.Columns [columnIndex];
				_table.ChangingDataColumn (this, column, value);

#if NET_2_0
				if (value == null && column.DataType.IsValueType)
#else
				if (value == null && column.DataType != typeof (string))
#endif
					throw new ArgumentException ("Canot set column '"
						+ column.ColumnName + "' to be null."
						+ " Please use DBNull instead.");

				_rowChanged = true;

				CheckValue (value, column);
				bool already_editing = Proposed >= 0;
				if (!already_editing)
					BeginEdit ();

				column [Proposed] = value;
				_table.ChangedDataColumn (this, column, value);

				if (!already_editing)
					EndEdit ();
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the named column.
		/// </summary>
		public object this [string columnName, DataRowVersion version] {
			get {
				DataColumn column = _table.Columns [columnName];
				if (column == null)
					throw new ArgumentException ("The column '" + columnName +
						"' does not belong to the table : " + _table.TableName);
				return this [column.Ordinal, version];
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the specified DataColumn.
		/// </summary>
		public object this [DataColumn column, DataRowVersion version] {
			get {
				if (column == null)
					throw new ArgumentNullException ("column");
				if (column.Table != Table)
					throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
						"The column '{0}' does not belong to the table : {1}.",
						column.ColumnName, _table.TableName));
				return this [column.Ordinal, version];
			}
		}

		/// <summary>
		/// Set a value for the column into the offset specified by the version.<br>
		/// If the value is auto increment or null, necessary auto increment value
		/// or the default value will be used.
		/// </summary>
		internal void SetValue (int column, object value, int version)
		{
			DataColumn dc = Table.Columns [column];

			if (value == null && !dc.AutoIncrement) // set default value / auto increment
				value = dc.DefaultValue;

			Table.ChangingDataColumn (this, dc, value);
			CheckValue (value, dc);
			if (!dc.AutoIncrement)
				dc [version] = value;
			else if (_proposed >= 0 && _proposed != version) // proposed holds the AI
				dc [version] = dc [_proposed];
		}

		/// <summary>
		/// Gets the data stored in the column, specified by index and version of the data to
		/// retrieve.
		/// </summary>
		public object this [int columnIndex, DataRowVersion version] {
			get {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();

				DataColumn column = _table.Columns [columnIndex];
				int recordIndex = IndexFromVersion (version);

				if (column.Expression != String.Empty && _table.Rows.IndexOf (this) != -1) {
					// FIXME: how does this handle 'version'?
					// TODO: Can we avoid the Eval each time by using the cached value?
					object o = column.CompiledExpression.Eval (this);
					if (o != null && o != DBNull.Value)
						o = Convert.ChangeType (o, column.DataType);
					column [recordIndex] = o;
					return column [recordIndex];
				}

				return column [recordIndex];
			}
		}

		/// <summary>
		/// Gets or sets all of the values for this row through an array.
		/// </summary>
		public object [] ItemArray {
			get {
				// Accessing deleted rows
				if (RowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");

				int index = Current;
				if (RowState == DataRowState.Detached) {
					// Check if datarow is removed from the table.
					if (Proposed < 0)
						throw new RowNotInTableException (
							"This row has been removed from a table and does not have any data."
							+ "  BeginEdit() will allow creation of new data in this row.");
					index = Proposed;
				}

				object[] items = new object [_table.Columns.Count];

				foreach(DataColumn column in _table.Columns)
					items [column.Ordinal] = column [index];
				return items;
			}
			set {
				if (value.Length > _table.Columns.Count)
					throw new ArgumentException ();

				if (RowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();

				BeginEdit ();

				DataColumnChangeEventArgs e = new DataColumnChangeEventArgs ();
				foreach (DataColumn column in _table.Columns) {
					int i = column.Ordinal;
					object newVal = i < value.Length ? value [i] : null;

					if (newVal == null)
						continue;

					e.Initialize (this, column, newVal);
					CheckValue (e.ProposedValue, column);
					_table.RaiseOnColumnChanging (e);
					column [Proposed] = e.ProposedValue;
					_table.RaiseOnColumnChanged (e);
				}

				EndEdit ();
			}
		}

		/// <summary>
		/// Gets the current state of the row in regards to its relationship to the
		/// DataRowCollection.
		/// </summary>
		public DataRowState RowState {
			get {
				//return rowState;
				if (Original == -1 && Current == -1)
					return DataRowState.Detached;
				if (Original == Current)
					return DataRowState.Unchanged;
				if (Original == -1)
					return DataRowState.Added;
				if (Current == -1)
					return DataRowState.Deleted;
				return DataRowState.Modified;
			}
#if NET_2_0
			internal set {
				if (DataRowState.Detached == value) {
					Original = -1;
					Current = -1;
				}
				if (DataRowState.Unchanged == value)
					Original = Current;
				if (DataRowState.Added == value)
					Original = -1;
				if (DataRowState.Deleted == value)
					Current = -1;
			}
#endif
		}

#if NET_2_0
		public void SetAdded ()
		{
			if (RowState != DataRowState.Unchanged)
				throw new InvalidOperationException ("SetAdded and SetModified can only be called on DataRows with Unchanged DataRowState.");
			Original = -1;
		}

		public void SetModified ()
		{
			if (RowState != DataRowState.Unchanged)
				throw new InvalidOperationException ("SetAdded and SetModified can only be called on DataRows with Unchanged DataRowState.");
			Current = _table.RecordCache.NewRecord ();
			_table.RecordCache.CopyRecord (_table, Original, Current);
		}
#endif

		/// <summary>
		/// Gets the DataTable for which this row has a schema.
		/// </summary>
		public DataTable Table {
			get { return _table; }
#if NET_2_0
			internal set { _table = value; }
#endif
		}

		/// <summary>
		/// Gets and sets index of row. This is used from
		/// XmlDataDocument.
		// </summary>
		internal int XmlRowID {
			get { return xmlRowID; }
			set { xmlRowID = value; }
		}

		/// <summary>
		/// Gets and sets index of row.
		// </summary>
		internal int RowID {
			get { return _rowId; }
			set { _rowId = value; }
		}

		internal int Original {
			get { return _original; }
			set {
				if (Table != null) {
					//Table.RecordCache[_original] = null;
					Table.RecordCache [value] = this;
				}
				_original = value;
			}
		}

		internal int Current {
			get { return _current; }
			set {
				if (Table != null) {
					//Table.RecordCache[_current] = null;
					Table.RecordCache [value] = this;
				}
				_current = value;
			}
		}

		internal int Proposed {
			get { return _proposed; }
			set {
				if (Table != null) {
					//Table.RecordCache[_proposed] = null;
					Table.RecordCache [value] = this;
				}
				_proposed = value;
			}
		}

		#endregion

		#region Methods
		// Called by DataRowCollection.Add/InsertAt
		internal void AttachAt (int row_id, DataRowAction action)
		{
			_rowId = row_id;
			if (Proposed != -1) {
				if (Current >= 0)
					Table.RecordCache.DisposeRecord (Current);
				Current = Proposed;
				Proposed = -1;
			}
#if NET_2_0
			if ((action & (DataRowAction.ChangeCurrentAndOriginal | DataRowAction.ChangeOriginal)) != 0)
				Original = Current;
#endif
		}

		void Detach ()
		{
			Table.DeleteRowFromIndexes (this);
			_table.Rows.RemoveInternal (this);

			if (Proposed >= 0 && Proposed != Current && Proposed != Original)
				_table.RecordCache.DisposeRecord (Proposed);
			Proposed = -1;

			if (Current >= 0 && Current != Original)
				_table.RecordCache.DisposeRecord (Current);
			Current = -1;

			if (Original >= 0)
				_table.RecordCache.DisposeRecord (Original);
			Original = -1;

			_rowId = -1;
		}

		internal void ImportRecord (int record)
		{
			if (HasVersion (DataRowVersion.Proposed))
				Table.RecordCache.DisposeRecord (Proposed);

			Proposed = record;

			foreach (DataColumn column in Table.Columns.AutoIncrmentColumns)
				column.UpdateAutoIncrementValue (column.DataContainer.GetInt64 (Proposed));

			foreach (DataColumn col in Table.Columns)
				CheckValue (this [col], col, false);
		}

		private void CheckValue (object v, DataColumn col)
		{
			CheckValue (v, col, true);
		}

		private void CheckValue (object v, DataColumn col, bool doROCheck)
		{
			if (doROCheck && _rowId != -1 && col.ReadOnly)
				throw new ReadOnlyException ();

			if (v == null || v == DBNull.Value) {
				if (col.AllowDBNull || col.AutoIncrement || col.DefaultValue != DBNull.Value)
					return;

				//Constraint violations during data load is raise in DataTable EndLoad
				this._nullConstraintViolation = true;
				if (this.Table._duringDataLoad || (Table.DataSet != null && !Table.DataSet.EnforceConstraints))
					this.Table._nullConstraintViolationDuringDataLoad = true;
				_nullConstraintMessage = "Column '" + col.ColumnName + "' does not allow nulls.";
			}
		}

		/// <summary>
		/// Gets or sets the custom error description for a row.
		/// </summary>
		public string RowError {
			get { return rowError; }
			set { rowError = value; }
		}

		internal int IndexFromVersion (DataRowVersion version)
		{
			switch (version) {
			case DataRowVersion.Default:
				if (Proposed >= 0)
					return Proposed;

				if (Current >= 0)
					return Current;

				if (Original < 0)
					throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

				throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");

			case DataRowVersion.Proposed:
				return AssertValidVersionIndex (version, Proposed);
			case DataRowVersion.Current:
				return AssertValidVersionIndex (version, Current);
			case DataRowVersion.Original:
				return AssertValidVersionIndex (version, Original);
			default:
				throw new DataException ("Version must be Original, Current, or Proposed.");
			}
		}

		private int AssertValidVersionIndex (DataRowVersion version, int index)
		{
			if (index >= 0)
				return index;

			throw new VersionNotFoundException (String.Format ("There is no {0} data to accces.", version));
		}

		internal DataRowVersion VersionFromIndex (int index)
		{
			if (index < 0)
				throw new ArgumentException ("Index must not be negative.");

			// the order of ifs matters
			if (index == Current)
				return DataRowVersion.Current;
			if (index == Original)
				return DataRowVersion.Original;
			if (index == Proposed)
				return DataRowVersion.Proposed;

			throw new ArgumentException (String.Format ("The index {0} does not belong to this row.", index));
		}

		internal XmlDataDocument.XmlDataElement DataElement {
			get {
				if (mappedElement != null || _table.DataSet == null || _table.DataSet._xmlDataDocument == null)
					return mappedElement;

				// create mapped XmlDataElement
				mappedElement = new XmlDataDocument.XmlDataElement (
					this, _table.Prefix, XmlHelper.Encode (_table.TableName),
					_table.Namespace, _table.DataSet._xmlDataDocument);
				return mappedElement;
			}
			set { mappedElement = value; }
		}

		internal void SetOriginalValue (string columnName, object val)
		{
			DataColumn column = _table.Columns [columnName];
			_table.ChangingDataColumn (this, column, val);

			if (Original < 0 || Original == Current) {
				Original = Table.RecordCache.NewRecord ();
				
				foreach (DataColumn col in _table.Columns)
					col.DataContainer.CopyValue (Table.DefaultValuesRowIndex, Original);
			}

			CheckValue (val, column);
			column [Original] = val;
		}

		/// <summary>
		/// Commits all the changes made to this row since the last time AcceptChanges was
		/// called.
		/// </summary>
		public void AcceptChanges ()
		{
			EndEdit (); // in case it hasn't been called

			_table.ChangingDataRow (this, DataRowAction.Commit);
			CheckChildRows (DataRowAction.Commit);
			switch (RowState) {
			case DataRowState.Added:
			case DataRowState.Modified:
				if (Original >= 0)
					Table.RecordCache.DisposeRecord (Original);
				Original = Current;
				break;
			case DataRowState.Deleted:
				Detach ();
				break;
			case DataRowState.Detached:
				throw new RowNotInTableException("Cannot perform this operation on a row not in the table.");
			}

			_table.ChangedDataRow (this, DataRowAction.Commit);
		}

		/// <summary>
		/// Begins an edit operation on a DataRow object.
		/// </summary>
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public void BeginEdit ()
		{
			if (_inChangingEvent)
				throw new InRowChangingEventException ("Cannot call BeginEdit inside an OnRowChanging event.");
			if (RowState == DataRowState.Deleted)
				throw new DeletedRowInaccessibleException ();

			if (!HasVersion (DataRowVersion.Proposed)) {
				Proposed = Table.RecordCache.NewRecord ();
				int from = HasVersion (DataRowVersion.Current) ? Current : Table.DefaultValuesRowIndex;
				for (int i = 0; i < Table.Columns.Count; i++){
					DataColumn column = Table.Columns [i];
					column.DataContainer.CopyValue (from, Proposed);
				}
			}
		}

		/// <summary>
		/// Cancels the current edit on the row.
		/// </summary>
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public void CancelEdit ()
		{
			 if (_inChangingEvent)
				throw new InRowChangingEventException ("Cannot call CancelEdit inside an OnRowChanging event.");

			if (HasVersion (DataRowVersion.Proposed)) {
				int oldRecord = Proposed;
				DataRowState oldState = RowState;
				Table.RecordCache.DisposeRecord(Proposed);
				Proposed = -1;

				foreach (Index index in Table.Indexes)
					index.Update (this, oldRecord, DataRowVersion.Proposed, oldState);
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
			_table.DeletingDataRow (this, DataRowAction.Delete);
			switch (RowState) {
			case DataRowState.Added:
				CheckChildRows (DataRowAction.Delete);
				Detach ();
				break;
			case DataRowState.Deleted:
			case DataRowState.Detached:
				break;
			default:
				// check what to do with child rows
				CheckChildRows (DataRowAction.Delete);
				break;
			}
			if (Current >= 0) {
				int current = Current;
				DataRowState oldState = RowState;
				if (Current != Original)
					_table.RecordCache.DisposeRecord (Current);
				Current = -1;
				foreach (Index index in Table.Indexes)
					index.Update (this, current, DataRowVersion.Current, oldState);
			}
			_table.DeletedDataRow (this, DataRowAction.Delete);
		}

		// check the child rows of this row before deleting the row.
		private void CheckChildRows (DataRowAction action)
		{
			DataSet ds = _table.DataSet;

			if (ds == null || !ds.EnforceConstraints)
				return;

			// if the table we're attached-to doesn't have an constraints, no foreign keys are pointing to us ...
			if (_table.Constraints.Count == 0)
				return;

			foreach (DataTable table in ds.Tables) {
				// loop on all ForeignKeyConstrain of the table.
				foreach (Constraint constraint in table.Constraints) {
					ForeignKeyConstraint fk = constraint as ForeignKeyConstraint;
					if (fk == null || fk.RelatedTable != _table)
						continue;

					switch (action) {
					case DataRowAction.Delete:
						CheckChildRows (fk, action, fk.DeleteRule);
						break;
					case DataRowAction.Commit:
					case DataRowAction.Rollback:
						if (fk.AcceptRejectRule != AcceptRejectRule.None)
							CheckChildRows (fk, action, Rule.Cascade);
						break;
					default:
						CheckChildRows (fk, action, fk.UpdateRule);
						break;
					}
				}
			}
		}

		private void CheckChildRows (ForeignKeyConstraint fkc, DataRowAction action, Rule rule)
		{
			DataRow [] childRows = GetChildRows (fkc, DataRowVersion.Current);
			if (childRows == null)
				return;

			switch (rule) {
			case Rule.Cascade:  // delete or change all relted rows.
				switch (action) {
				case DataRowAction.Delete:
					for (int j = 0; j < childRows.Length; j++) {
						if (childRows [j].RowState != DataRowState.Deleted)
							childRows [j].Delete ();
					}
					break;
				case DataRowAction.Change:
					for (int j = 0; j < childRows.Length; j++) {
						// if action is change we change the values in the child row
						// change only the values in the key columns
						// set the childcolumn value to the new parent row value
						for (int k = 0; k < fkc.Columns.Length; k++)
							if (!fkc.RelatedColumns [k].DataContainer [Current].Equals (fkc.RelatedColumns [k].DataContainer [Proposed]))
								childRows [j][fkc.Columns [k]] = this [fkc.RelatedColumns [k], DataRowVersion.Proposed];
					}
					break;
				case DataRowAction.Rollback:
					for (int j = 0; j < childRows.Length; j++) {
						if (childRows [j].RowState != DataRowState.Unchanged)
							childRows [j].RejectChanges ();
					}
					break;
				}
				break;
			case Rule.None: // throw an exception if there are any child rows.
				for (int j = 0; j < childRows.Length; j++) {
					if (childRows[j].RowState != DataRowState.Deleted) {
						string changeStr = "Cannot change this row because constraints are enforced on relation " + fkc.ConstraintName +", and changing this row will strand child rows.";
						string delStr = "Cannot delete this row because constraints are enforced on relation " + fkc.ConstraintName +", and deleting this row will strand child rows.";
						string message = action == DataRowAction.Delete ? delStr : changeStr;
						throw new InvalidConstraintException (message);
					}
				}
				break;
			case Rule.SetDefault: // set the values in the child rows to the default value of the columns.
				if (childRows.Length > 0) {
					int defaultValuesRowIndex = childRows [0].Table.DefaultValuesRowIndex;
					foreach (DataRow childRow in childRows) {
						if (childRow.RowState != DataRowState.Deleted) {
							int defaultIdx = childRow.IndexFromVersion (DataRowVersion.Default);
							foreach (DataColumn column in fkc.Columns)
								column.DataContainer.CopyValue (defaultValuesRowIndex, defaultIdx);
						}
					}
				}
				break;
			case Rule.SetNull: // set the values in the child row to null.
				for (int j = 0; j < childRows.Length; j++) {
					DataRow child = childRows [j];
					if (childRows[j].RowState != DataRowState.Deleted) {
						// set only the key columns to DBNull
						for (int k = 0; k < fkc.Columns.Length; k++)
							child.SetNull (fkc.Columns[k]);
					}
				}
				break;
			}
		}

		/// <summary>
		/// Ends the edit occurring on the row.
		/// </summary>
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public void EndEdit ()
		{
			if (_inChangingEvent)
				throw new InRowChangingEventException ("Cannot call EndEdit inside an OnRowChanging event.");

			if (RowState == DataRowState.Detached || !HasVersion (DataRowVersion.Proposed))
				return;

			CheckReadOnlyStatus ();

			_inChangingEvent = true;
			try {
				_table.ChangingDataRow (this, DataRowAction.Change);
			} finally {
				_inChangingEvent = false;
			}

			DataRowState oldState = RowState;

			int oldRecord = Current;
			Current = Proposed;
			Proposed = -1;


			//FIXME : ideally  indexes shouldnt be maintained during dataload.But this needs to
			//be implemented at multiple places.For now, just maintain the index.
			//if (!Table._duringDataLoad) {
			foreach (Index index in Table.Indexes)
				index.Update (this, oldRecord, DataRowVersion.Current, oldState);
			//}

			try {
				AssertConstraints ();
				
				// restore previous state to let the cascade update to find the rows 
				Proposed = Current;
				Current = oldRecord; 
				
				CheckChildRows (DataRowAction.Change);
				
				// apply new state
				Current = Proposed;
				Proposed = -1;
			} catch {
				int proposed = Proposed >= 0 ? Proposed : Current;
				Current = oldRecord;
				//if (!Table._duringDataLoad) {
				foreach (Index index in Table.Indexes)
					index.Update (this, proposed, DataRowVersion.Current, RowState);
				//}
				throw;
			}

			if (Original != oldRecord)
				Table.RecordCache.DisposeRecord (oldRecord);

			// Note : row state must not be changed before all the job on indexes finished,
			// since the indexes works with recods rather than with rows and the decision
			// which of row records to choose depends on row state.
			if (_rowChanged == true) {
				_table.ChangedDataRow (this, DataRowAction.Change);
				_rowChanged = false;
			}
		}

		/// <summary>
		/// Gets the child rows of this DataRow using the specified DataRelation.
		/// </summary>
		public DataRow [] GetChildRows (DataRelation relation)
		{
			return GetChildRows (relation, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow [] GetChildRows (string relationName)
		{
			return GetChildRows (Table.DataSet.Relations [relationName]);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (DataRelation relation, DataRowVersion version)
		{
			if (relation == null)
				return Table.NewRowArray (0);

			if (this.Table == null)
				throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException ();

			if (_table != relation.ParentTable)
				throw new InvalidConstraintException ("GetChildRow requires a row whose Table is " + relation.ParentTable + ", but the specified row's table is " + _table);

			if (relation.ChildKeyConstraint != null)
				return GetChildRows (relation.ChildKeyConstraint, version);

			ArrayList rows = new ArrayList ();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			DataRow[] result = null;

			int versionIndex = IndexFromVersion (version);
			int tmpRecord = relation.ChildTable.RecordCache.NewRecord ();

			try {
				for (int i = 0; i < numColumn; i++)
					// according to MSDN: the DataType value for both columns must be identical.
					childColumns [i].DataContainer.CopyValue (parentColumns [i].DataContainer, versionIndex, tmpRecord);

				Index index = relation.ChildTable.FindIndex (childColumns);

				if (index != null) {
					int [] records = index.FindAll (tmpRecord);
					result = relation.ChildTable.NewRowArray (records.Length);
					for(int i = 0; i < records.Length; i++)
						result [i] = relation.ChildTable.RecordCache [records [i]];
				} else {
					foreach (DataRow row in relation.ChildTable.Rows) {
						bool allColumnsMatch = false;
						if (row.HasVersion (DataRowVersion.Default)) {
							allColumnsMatch = true;
							int childIndex = row.IndexFromVersion (DataRowVersion.Default);
							for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) {
								if (childColumns[columnCnt].DataContainer.CompareValues (childIndex, tmpRecord) != 0) {
									allColumnsMatch = false;
									break;
								}
							}
						}
						if (allColumnsMatch)
							rows.Add (row);
					}
					result = relation.ChildTable.NewRowArray (rows.Count);
					rows.CopyTo (result, 0);
				}
			}
			finally {
				relation.ChildTable.RecordCache.DisposeRecord (tmpRecord);
			}

			return result;
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow [] GetChildRows (string relationName, DataRowVersion version)
		{
			return GetChildRows (Table.DataSet.Relations [relationName], version);
		}

		private DataRow [] GetChildRows (ForeignKeyConstraint fkc, DataRowVersion version)
		{
			ArrayList rows = new ArrayList ();
			DataColumn [] parentColumns = fkc.RelatedColumns;
			DataColumn [] childColumns = fkc.Columns;
			int numColumn = parentColumns.Length;

			Index index = fkc.Index;

			int curIndex = IndexFromVersion (version);
			int tmpRecord = fkc.Table.RecordCache.NewRecord ();
			for (int i = 0; i < numColumn; i++)
				// according to MSDN: the DataType value for both columns must be identical.
				childColumns [i].DataContainer.CopyValue (parentColumns [i].DataContainer, curIndex, tmpRecord);

			try {
				if (index != null) {
					// get the child rows from the index
					int[] childRecords = index.FindAll (tmpRecord);
					for (int i = 0; i < childRecords.Length; i++)
						rows.Add (childColumns [i].Table.RecordCache [childRecords [i]]);
				} else { // if there is no index we search manualy.
					foreach (DataRow row in fkc.Table.Rows) {
						bool allColumnsMatch = false;
						if (row.HasVersion (DataRowVersion.Default)) {
							allColumnsMatch = true;
							int childIndex = row.IndexFromVersion (DataRowVersion.Default);
							for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) {
								if (childColumns [columnCnt].DataContainer.CompareValues (childIndex, tmpRecord) != 0) {
									allColumnsMatch = false;
									break;
								}
							}
						}
						if (allColumnsMatch)
							rows.Add (row);
					}
				}
			} finally {
				fkc.Table.RecordCache.DisposeRecord(tmpRecord);
			}

			DataRow[] result = fkc.Table.NewRowArray (rows.Count);
			rows.CopyTo (result, 0);
			return result;
		}

		/// <summary>
		/// Gets the error description of the specified DataColumn.
		/// </summary>
		public string GetColumnError (DataColumn column)
		{
			if (column == null)
				throw new ArgumentNullException ("column");

			int index = _table.Columns.IndexOf (column);
			if (index < 0)
				throw new ArgumentException (String.Format ("Column '{0}' does not belong to table {1}.", column.ColumnName, Table.TableName));

			return GetColumnError (index);
		}

		/// <summary>
		/// Gets the error description for the column specified by index.
		/// </summary>
		public string GetColumnError (int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= Table.Columns.Count)
				throw new IndexOutOfRangeException ();

			string retVal = null;
			if (columnIndex < ColumnErrors.Count)
				retVal = (String) ColumnErrors [columnIndex];
			return (retVal != null) ? retVal : String.Empty;
		}

		/// <summary>
		/// Gets the error description for the column, specified by name.
		/// </summary>
		public string GetColumnError (string columnName)
		{
			return GetColumnError (_table.Columns.IndexOf (columnName));
		}

		/// <summary>
		/// Gets an array of columns that have errors.
		/// </summary>
		public DataColumn [] GetColumnsInError ()
		{
			ArrayList dataColumns = new ArrayList ();

			int columnOrdinal = 0;
			foreach (String columnError in ColumnErrors) {
				if (columnError != null && columnError != String.Empty)
					dataColumns.Add (_table.Columns [columnOrdinal]);
				columnOrdinal++;
			}

			return (DataColumn [])(dataColumns.ToArray (typeof (DataColumn)));
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation)
		{
			return GetParentRow (relation, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow GetParentRow (string relationName)
		{
			return GetParentRow (relationName, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation, DataRowVersion version)
		{
			DataRow[] rows = GetParentRows (relation, version);
			if (rows.Length == 0)
				return null;
			return rows [0];
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (string relationName, DataRowVersion version)
		{
			return GetParentRow (Table.DataSet.Relations [relationName], version);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow [] GetParentRows (DataRelation relation)
		{
			return GetParentRows (relation, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow [] GetParentRows (string relationName)
		{
			return GetParentRows (relationName, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow [] GetParentRows (DataRelation relation, DataRowVersion version)
		{
			// TODO: Caching for better preformance
			if (relation == null)
				return Table.NewRowArray (0);

			if (this.Table == null)
				throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException ();

			if (_table != relation.ChildTable)
				throw new InvalidConstraintException ("GetParentRows requires a row whose Table is " + relation.ChildTable + ", but the specified row's table is " + _table);

			ArrayList rows = new ArrayList ();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;

			int curIndex = IndexFromVersion (version);
			int tmpRecord = relation.ParentTable.RecordCache.NewRecord ();
			for (int i = 0; i < numColumn; i++)
				// according to MSDN: the DataType value for both columns must be identical.
				parentColumns [i].DataContainer.CopyValue(childColumns [i].DataContainer, curIndex, tmpRecord);

			try {
				Index index = relation.ParentTable.FindIndex(parentColumns);
				if (index != null) { // get the parent rows from the index
					int [] parentRecords = index.FindAll (tmpRecord);
					for (int i = 0; i < parentRecords.Length; i++)
						rows.Add (parentColumns [i].Table.RecordCache [parentRecords [i]]);
				} else { // no index so we have to search manualy.
					foreach (DataRow row in relation.ParentTable.Rows) {
						bool allColumnsMatch = false;
						if (row.HasVersion (DataRowVersion.Default)) {
							allColumnsMatch = true;
							int parentIndex = row.IndexFromVersion (DataRowVersion.Default);
							for (int columnCnt = 0; columnCnt < numColumn; columnCnt++) {
								if (parentColumns [columnCnt].DataContainer.CompareValues (parentIndex, tmpRecord) != 0) {
									allColumnsMatch = false;
									break;
								}
							}
						}
						if (allColumnsMatch)
							rows.Add(row);
					}
				}
			} finally {
				relation.ParentTable.RecordCache.DisposeRecord (tmpRecord);
			}

			DataRow [] result = relation.ParentTable.NewRowArray (rows.Count);
			rows.CopyTo (result, 0);
			return result;
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow [] GetParentRows (string relationName, DataRowVersion version)
		{
			return GetParentRows (Table.DataSet.Relations [relationName], version);
		}

		/// <summary>
		/// Gets a value indicating whether a specified version exists.
		/// </summary>
		public bool HasVersion (DataRowVersion version)
		{
			switch (version) {
			case DataRowVersion.Default:
				return (Proposed >= 0 || Current >= 0);
			case DataRowVersion.Proposed:
				return Proposed >= 0;
			case DataRowVersion.Current:
				return Current >= 0;
			case DataRowVersion.Original:
				return Original >= 0;
			default:
				return IndexFromVersion (version) >= 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column)
		{
			return IsNull (column, DataRowVersion.Default);
		}

		/// <summary>
		/// Gets a value indicating whether the column at the specified index contains a null
		/// value.
		/// </summary>
		public bool IsNull (int columnIndex)
		{
			return IsNull (Table.Columns [columnIndex]);
		}

		/// <summary>
		/// Gets a value indicating whether the named column contains a null value.
		/// </summary>
		public bool IsNull (string columnName)
		{
			return IsNull (Table.Columns [columnName]);
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn and DataRowVersion
		/// contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column, DataRowVersion version)
		{
			return column.DataContainer.IsNull (IndexFromVersion (version));
		}

		/// <summary>
		/// Returns a value indicating whether all of the row columns specified contain a null value.
		/// </summary>
		internal bool IsNullColumns (DataColumn [] columns)
		{
			int i;
			for (i = 0; i < columns.Length; i++) {
				if (!IsNull (columns [i]))
					break;
			}
			return i == columns.Length;
		}

		/// <summary>
		/// Rejects all changes made to the row since AcceptChanges was last called.
		/// </summary>
		public void RejectChanges ()
		{
			if (RowState == DataRowState.Detached)
				throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
			// If original is null, then nothing has happened since AcceptChanges
			// was last called.  We have no "original" to go back to.

			// Varadhan: Following if may be un-necessary
			/*if (_inChangingEvent) {
				_table.ChangedDataRow (this, DataRowAction.Rollback);
				CancelEdit ();
			}*/

			Table.ChangingDataRow (this, DataRowAction.Rollback);
			//TODO : Need to Verify the constraints..
			switch (RowState) {
			case DataRowState.Added:
				Detach ();
				break;
			case DataRowState.Modified:
				int current = Current;
				Table.RecordCache.DisposeRecord (Current);
				CheckChildRows (DataRowAction.Rollback);
				Current = Original;
				foreach (Index index in Table.Indexes)
					index.Update (this, current, DataRowVersion.Current, DataRowState.Modified);
				break;
			case DataRowState.Deleted:
				CheckChildRows (DataRowAction.Rollback);
				Current = Original;
				// Add row to index and validate if the constraints are satisfied
				this.Validate ();
				break;
			}
			Table.ChangedDataRow (this, DataRowAction.Rollback);
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

			while (columnIndex >= ColumnErrors.Count)
				ColumnErrors.Add (null);

			ColumnErrors [columnIndex] = error;
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
			this [column] = DBNull.Value;
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow.
		/// </summary>
		public void SetParentRow (DataRow parentRow)
		{
			SetParentRow (parentRow, null);
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow and
		/// DataRelation.
		/// </summary>
		public void SetParentRow (DataRow parentRow, DataRelation relation)
		{
			if (_table == null || parentRow.Table == null)
				throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (parentRow != null && _table.DataSet != parentRow.Table.DataSet)
				throw new ArgumentException ();

			if (RowState == DataRowState.Detached && !HasVersion (DataRowVersion.Default))
				// the row should have default data to access, i.e. we can do this for the newly created row, but not for the row once deleted from the table
				throw new RowNotInTableException ("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			BeginEdit();

			IEnumerable relations;
			if (relation == null)
				relations = _table.ParentRelations;
			else
				relations = new DataRelation [] { relation };

			foreach (DataRelation rel in relations) {
				DataColumn [] childCols = rel.ChildColumns;
				DataColumn [] parentCols = rel.ParentColumns;

				for (int i = 0; i < parentCols.Length; i++) {
					if (parentRow == null) {
						childCols [i].DataContainer [Proposed] = DBNull.Value;
					} else {
						int defaultIdx = parentRow.IndexFromVersion (DataRowVersion.Default);
						childCols [i].DataContainer.CopyValue(parentCols [i].DataContainer, defaultIdx, Proposed);
					}
				}
			}

			EndEdit();
		}

		//Copy all values of this DataRow to the row parameter.
		internal void CopyValuesToRow (DataRow row)
		{
			CopyValuesToRow(row, true);
		}
		
		internal void CopyValuesToRow (DataRow row, bool doROCheck)
		{
			if (row == null)
				throw new ArgumentNullException("row");
			if (row == this)
				throw new ArgumentException("'row' is the same as this object");

			// create target records if missing.
			if (HasVersion (DataRowVersion.Original)) {
				if (row.Original < 0)
					row.Original = row.Table.RecordCache.NewRecord ();
				else if (row.Original == row.Current) {
					row.Original = row.Table.RecordCache.NewRecord ();
					row.Table.RecordCache.CopyRecord (row.Table, row.Current, row.Original);
				}
			} else {
				if (row.Original > 0) {
					if (row.Original != row.Current)
						row.Table.RecordCache.DisposeRecord (row.Original);
					row.Original = -1;
				}
			}

			if (HasVersion (DataRowVersion.Current)) {
				if (Current == Original) {
					if (row.Current >= 0)
						row.Table.RecordCache.DisposeRecord (row.Current);
					row.Current = row.Original;
				} else {
					if (row.Current < 0)
						row.Current = row.Table.RecordCache.NewRecord ();
				}
			} else {
				if (row.Current > 0) {
					row.Table.RecordCache.DisposeRecord (row.Current);
					row.Current = -1;
				}
			}

			if (HasVersion (DataRowVersion.Proposed)) {
				if (row.Proposed < 0)
					row.Proposed = row.Table.RecordCache.NewRecord ();
			} else {
				if (row.Proposed > 0) {
					row.Table.RecordCache.DisposeRecord (row.Proposed);
					row.Proposed = -1;
				}
			}

			// copy source record values to target records
			foreach (DataColumn column in Table.Columns) {
				DataColumn targetColumn = row.Table.Columns [column.ColumnName];
				//if a column with the same name exists in both rows copy the values
				if (targetColumn != null) {
					if (HasVersion (DataRowVersion.Original)) {
						object val = column[Original];
						row.CheckValue (val, targetColumn, doROCheck);
						targetColumn [row.Original] = val;
					}

					if (HasVersion (DataRowVersion.Current) && Current != Original) {
						object val = column[Current];
						row.CheckValue (val, targetColumn, doROCheck);
						targetColumn [row.Current] = val;
					}

					if (HasVersion (DataRowVersion.Proposed)) {
						object val = column[row.Proposed];
						row.CheckValue (val, targetColumn, doROCheck);
						targetColumn [row.Proposed] = val;
					}
				}
			}
			if (HasErrors)
				CopyErrors (row);
		}

		//Merge all values of this DataRow to the row parameter according to merge rules.
		internal void MergeValuesToRow (DataRow row, bool preserveChanges)
		{
			if (row == null)
				throw new ArgumentNullException ("row");
			if (row == this)
				throw new ArgumentException ("'row' is the same as this object");

			// Original values are anyway copied
			if (HasVersion (DataRowVersion.Original)) {
				if (row.Original < 0)
					row.Original = row.Table.RecordCache.NewRecord ();
				else if (row.Original == row.Current && !(Original == Current && !preserveChanges)) {
					row.Original = row.Table.RecordCache.NewRecord ();
					row.Table.RecordCache.CopyRecord (row.Table, row.Current, row.Original);
				}
			} else {
				if (row.Original == row.Current) { // if target has same current, better create new original
					row.Original = row.Table.RecordCache.NewRecord ();
					row.Table.RecordCache.CopyRecord (row.Table, row.Current, row.Original);
				}
			}

			// if i have current, push all
			if (HasVersion (DataRowVersion.Current)) {
				if (! preserveChanges && row.Current < 0)
					row.Current = row.Table.RecordCache.NewRecord ();
			} else {
				if (row.Current > 0 && ! preserveChanges) {
					row.Table.RecordCache.DisposeRecord (row.Current);
					row.Current = -1;
				}
			}

			// copy source record values to target records
			foreach (DataColumn column in Table.Columns) {
				DataColumn targetColumn = row.Table.Columns[column.ColumnName];
				//if a column with the same name exists in both rows copy the values
				if (targetColumn != null) {
					if (HasVersion (DataRowVersion.Original)) {
						object val = column [Original];
						row.CheckValue (val, targetColumn);
						targetColumn [row.Original] = val;
					}

					if (HasVersion (DataRowVersion.Current) && !preserveChanges) {
						object val = column [Current];
						row.CheckValue (val, targetColumn);
						targetColumn [row.Current] = val;
					}
				}
			}

			if (HasErrors)
				CopyErrors(row);
		}

		internal void CopyErrors (DataRow row)
		{
			row.RowError = RowError;
			DataColumn[] errorColumns = GetColumnsInError();
			foreach (DataColumn col in errorColumns) {
				DataColumn targetColumn = row.Table.Columns [col.ColumnName];
				row.SetColumnError (targetColumn, GetColumnError (col));
			}
		}

		internal bool IsRowChanged (DataRowState rowState)
		{
			if((RowState & rowState) != 0)
				return true;

			//we need to find if child rows of this row changed.
			//if yes - we should return true

			// if the rowState is deleted we should get the original version of the row
			// else - we should get the current version of the row.
			DataRowVersion version = (rowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
			int count = Table.ChildRelations.Count;
			for (int i = 0; i < count; i++){
				DataRelation rel = Table.ChildRelations [i];
				DataRow [] childRows = GetChildRows (rel, version);
				for (int j = 0; j < childRows.Length; j++){
					if (childRows [j].IsRowChanged (rowState))
						return true;
				}
			}

			return false;
		}

		internal void Validate ()
		{
			Table.AddRowToIndexes (this);
			AssertConstraints ();
		}

		void AssertConstraints ()
		{
			if (Table == null || Table._duringDataLoad)
				return;

			if (Table.DataSet != null && !Table.DataSet.EnforceConstraints)
				return;
			for (int i = 0; i < Table.Columns.Count; ++i) {
				DataColumn column = Table.Columns [i];
				if (!column.AllowDBNull && IsNull (column))
					throw new NoNullAllowedException (_nullConstraintMessage);
			}

			foreach (Constraint constraint in Table.Constraints) {
				try {
					constraint.AssertConstraint (this);
				} catch (Exception e) {
					Table.DeleteRowFromIndexes (this);
					throw e;
				}
			}
		}

		internal void CheckNullConstraints ()
		{
			if (_nullConstraintViolation) {
				if (HasVersion (DataRowVersion.Proposed)) {
					foreach (DataColumn column in Table.Columns) {
						if (IsNull (column) && !column.AllowDBNull)
							throw new NoNullAllowedException (_nullConstraintMessage);
					}
				}
				_nullConstraintViolation = false;
			}
		}

		internal void CheckReadOnlyStatus()
		{
			int defaultIdx = IndexFromVersion (DataRowVersion.Default);
			foreach(DataColumn column in Table.Columns) {
				if ((column.DataContainer.CompareValues (defaultIdx,Proposed) != 0) && column.ReadOnly)
					throw new ReadOnlyException ();
			}
		}

		#endregion // Methods

#if NET_2_0
		/// <summary>
		///    This method loads a given value into the existing row affecting versions,
		///    state based on the LoadOption.  The matrix of changes for this method are as
		///    mentioned in the DataTable.Load (IDataReader, LoadOption) method.
		/// </summary>
		internal void Load (object [] values, LoadOption loadOption)
		{
			int temp = -1;

			if (loadOption == LoadOption.OverwriteChanges ||
			    (loadOption == LoadOption.PreserveChanges && RowState == DataRowState.Unchanged)) {
				Table.ChangingDataRow (this, DataRowAction.ChangeCurrentAndOriginal);
				temp = Table.CreateRecord (values);
				Table.DeleteRowFromIndexes(this);
				if (HasVersion (DataRowVersion.Original) && Current != Original)
					Table.RecordCache.DisposeRecord (Original);
				Original = temp;

				if (HasVersion (DataRowVersion.Current))
					Table.RecordCache.DisposeRecord (Current);
				Current = temp;
				Table.AddRowToIndexes(this);
				Table.ChangedDataRow (this, DataRowAction.ChangeCurrentAndOriginal);
				return;
			}

			if (loadOption == LoadOption.PreserveChanges) {
				Table.ChangingDataRow (this, DataRowAction.ChangeOriginal);
				temp = Table.CreateRecord (values);
				if (HasVersion (DataRowVersion.Original) && Current != Original)
					Table.RecordCache.DisposeRecord (Original);
				Original = temp;
				Table.ChangedDataRow (this, DataRowAction.ChangeOriginal);
				return;
			}

			// Upsert
			if (RowState != DataRowState.Deleted) {
				int rindex = HasVersion (DataRowVersion.Proposed) ? Proposed : Current;
				temp = Table.CreateRecord (values);
				if (RowState == DataRowState.Added || Table.CompareRecords (rindex, temp) != 0) {
					Table.ChangingDataRow (this, DataRowAction.Change);
					Table.DeleteRowFromIndexes(this);
					if (HasVersion (DataRowVersion.Proposed)) {
						Table.RecordCache.DisposeRecord (Proposed);
						Proposed = -1;
					}

					if (Original != Current)
						Table.RecordCache.DisposeRecord (Current);
					Current = temp;
					Table.AddRowToIndexes(this);
					Table.ChangedDataRow (this, DataRowAction.Change);
				} else {
					Table.ChangingDataRow (this, DataRowAction.Nothing);
					Table.RecordCache.DisposeRecord (temp);
					Table.ChangedDataRow (this, DataRowAction.Nothing);
				}
			}
		}
#endif // NET_2_0
	}
}
