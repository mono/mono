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

using System;
using System.Collections;
using System.Globalization;

namespace System.Data {
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
	[Serializable]
	public class DataRow
	{
		#region Fields

		private DataTable _table;

		private object[] original;
		private object[] proposed;
		private object[] current;

		private string[] columnErrors;
		private string rowError;
		private DataRowState rowState;
		internal int xmlRowID = 0;

		#endregion

		#region Constructors

		/// <summary>
		/// This member supports the .NET Framework infrastructure and is not intended to be 
		/// used directly from your code.
		/// </summary>
		protected internal DataRow (DataRowBuilder builder)
		{
			_table = builder.Table;

			original = null; 
			proposed = null;
			current = new object[_table.Columns.Count];
			// initialize to DBNull.Value
			for(int c = 0; c < _table.Columns.Count; c++) {
				current[c] = DBNull.Value;
			}

			columnErrors = new string[_table.Columns.Count];
			rowError = String.Empty;

			//rowState = DataRowState.Unchanged;

			//on first creating a DataRow it is always detached.
			rowState = DataRowState.Detached;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether there are errors in a row.
		/// </summary>
		public bool HasErrors {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets or sets the data stored in the column specified by name.
		/// </summary>
		public object this[string columnName] {
			[MonoTODO] //FIXME: will return different values depending on DataRowState
			get { return this[columnName, DataRowVersion.Current]; }
			[MonoTODO]
			set {
				DataColumn column = _table.Columns[columnName];
				if (column == null)
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in specified DataColumn
		/// </summary>
		public object this[DataColumn column] {
			[MonoTODO] //FIXME: will return different values depending on DataRowState
			get { return this[column, DataRowVersion.Current]; } 
								
			[MonoTODO]
			set {
				object v = null;

				if (column == null)
					throw new ArgumentNullException (Locale.GetText ("'column' argument cannot be null."));
				int columnIndex = _table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();

				_table.ChangingDataColumn (this, column, value);

				//MS Implementation doesn't seem to create the proposed or original
				//set of values when a datarow has just been created or added to the
				//DataTable and AcceptChanges() has not been called yet.

				if(rowState == DataRowState.Detached || rowState == DataRowState.Added) {
					v = SetColumnValue (value, columnIndex);
					current[columnIndex] = v;
					_table.ChangedDataColumn (this, column, v);
				}
				else {
					BeginEdit ();  // implicitly called

					v = SetColumnValue (value, columnIndex);
					proposed[columnIndex] = v;
					_table.ChangedDataColumn (this, column, v);

					rowState = DataRowState.Modified;
					current [columnIndex] = proposed[columnIndex];
					proposed[columnIndex] = null;

					//EndEdit ();
				}

				//Don't know if this is the rigth thing to do,
				//but it fixes my test. I believe the MS docs only say this
				//method is implicitly called when calling AcceptChanges()

				//EndEdit (); // is this the right thing to do?

			}
		}

		/// <summary>
		/// Gets or sets the data stored in column specified by index.
		/// </summary>
		public object this[int columnIndex] {
			[MonoTODO] //FIXME: not always supposed to return current
			get { return this[columnIndex, DataRowVersion.Current]; }
			[MonoTODO]
			set {
				DataColumn column = _table.Columns[columnIndex]; //FIXME: will throw
				if (column == null)  
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the named column.
		/// </summary>
		public object this[string columnName, DataRowVersion version] {
			[MonoTODO]
			get {
				DataColumn column = _table.Columns[columnName]; //FIXME: will throw
				if (column == null) 
					throw new IndexOutOfRangeException ();
				return this[column, version];
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the specified DataColumn.
		/// </summary>
		public object this[DataColumn column, DataRowVersion version] {
			get {
				if (column == null)
					throw new ArgumentNullException ();	

				int columnIndex = _table.Columns.IndexOf (column);

				if (columnIndex == -1)
					throw new ArgumentException ();

				if (version == DataRowVersion.Default)
					return column.DefaultValue;

				if (!HasVersion (version))
					throw new VersionNotFoundException (Locale.GetText ("There is no " + version.ToString () + " data to access."));

				switch (version)
				{
					case DataRowVersion.Proposed:
						return proposed[columnIndex];
					case DataRowVersion.Current:
					       	return current[columnIndex];
					case DataRowVersion.Original:
						return original[columnIndex];
					default:
						throw new ArgumentException ();
				}
			}
		}

		/// <summary>
		/// Gets the data stored in the column, specified by index and version of the data to
		/// retrieve.
		/// </summary>
		public object this[int columnIndex, DataRowVersion version] {
			[MonoTODO]
			get {
				DataColumn column = _table.Columns[columnIndex]; //FIXME: throws
				if (column == null) 
					throw new IndexOutOfRangeException ();
				return this[column, version];
			}
		}

		/// <summary>
		/// Gets or sets all of the values for this row through an array.
		/// </summary>
		[MonoTODO]
		public object[] ItemArray {
			get { 
				return current; 
			}
			set {
				if (value.Length > _table.Columns.Count)
					throw new ArgumentException ();

				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				
				object[] newItems = new object[_table.Columns.Count];			
				object v = null;
				int i = 0;
				while(i < _table.Columns.Count) {

					if (i < value.Length)
						v = value[i];
					else
						v = null;

					newItems[i] = SetColumnValue (v, i);
					i ++;
				}

				//FIXME: BeginEdit() not correct 
				BeginEdit ();  // implicitly called
				rowState = DataRowState.Modified;
				proposed = newItems;
				EndEdit ();
			}
		}

		private object SetColumnValue (object v, int index) 
		{		
			object newval = null;
			DataColumn col = _table.Columns[index];
			
			if (col.ReadOnly && v != this[index])
				throw new ReadOnlyException ();

			if (v == null) {
				if(col.DefaultValue != DBNull.Value) {
					newval = col.DefaultValue;
				}
				else if(col.AutoIncrement == true) {
					switch(col.DataType.ToString()) {
					case "System.Int16":
						newval = (short) col.AutoIncrementValue();
						break;
					case "System.Int32":
						newval = (int) col.AutoIncrementValue();
						break;
					case "System.Int64":
						newval = col.AutoIncrementValue();
						break;
					default:
						newval = col.AutoIncrementValue();
						break;
					}
				}
				else {
					if (!col.AllowDBNull)
						throw new NoNullAllowedException ();
					newval = DBNull.Value;
				}
			}
			else if (v == DBNull.Value) {
				if (!col.AllowDBNull)
					throw new NoNullAllowedException ();
				if (col.AutoIncrement == true) {
					col.AutoIncrementValue();
				}
				newval = DBNull.Value;
			}
			else {	
				Type vType = v.GetType(); // data type of value
				Type cType = col.DataType; // column data type
				if (cType != vType) {
					TypeCode typeCode = Type.GetTypeCode(cType);
					switch(typeCode) {
					case TypeCode.Boolean :
						v = Convert.ToBoolean (v);
						break;
					case TypeCode.Byte  :
						v = Convert.ToByte (v);
						break;
					case TypeCode.Char  :
						v = Convert.ToChar (v);
						break;
					case TypeCode.DateTime  :
						v = Convert.ToDateTime (v);
						break;
					case TypeCode.Decimal  :
						v = Convert.ToDecimal (v);
						break;
					case TypeCode.Double  :
						v = Convert.ToDouble (v);
						break;
					case TypeCode.Int16  :
						v = Convert.ToInt16 (v);
						break;
					case TypeCode.Int32  :
						v = Convert.ToInt32 (v);
						break;
					case TypeCode.Int64  :
						v = Convert.ToInt64 (v);
						break;
					case TypeCode.SByte  :
						v = Convert.ToSByte (v);
						break;
					case TypeCode.Single  :
						v = Convert.ToSingle (v);
						break;
					case TypeCode.String  :
						v = Convert.ToString (v);
						break;
					case TypeCode.UInt16  :
						v = Convert.ToUInt16 (v);
						break;
					case TypeCode.UInt32  :
						v = Convert.ToUInt32 (v);
						break;
					case TypeCode.UInt64  :
						v = Convert.ToUInt64 (v);
						break;
					default :
						switch(cType.ToString()) {
						case "System.TimeSpan" :
							v = (System.TimeSpan) v;
							break;
						case "System.Type" :
							v = (System.Type) v;
							break;
						case "System.Object" :
							//v = (System.Object) v;
							break;
						default:
							// FIXME: is exception correct?
							throw new InvalidCastException("Type not supported.");
						}
						break;
					}
					vType = v.GetType();
				}
				newval = v;
				if(col.AutoIncrement == true) {
					long inc = Convert.ToInt64(v);
					col.UpdateAutoIncrementValue (inc);
				}
			}
			col.DataHasBeenSet = true;
			return newval;
		}

		/// <summary>
		/// Gets or sets the custom error description for a row.
		/// </summary>
		public string RowError {
			get { return rowError; }
			set { rowError = value; }
		}

		/// <summary>
		/// Gets the current state of the row in regards to its relationship to the
		/// DataRowCollection.
		/// </summary>
		public DataRowState RowState {
			get { return rowState; }
		}

		//FIXME?: Couldn't find a way to set the RowState when adding the DataRow
		//to a Datatable so I added this method. Delete if there is a better way.
		internal DataRowState RowStateInternal {
			set { rowState = value;}
		}

		/// <summary>
		/// Gets the DataTable for which this row has a schema.
		/// </summary>
		public DataTable Table {
			get { return _table; }
		}

		/// <summary>
		/// Gets and sets index of row. This is used from 
		/// XmlDataDocument.
		// </summary>
		internal int XmlRowID {
			get { return xmlRowID; }
			set { xmlRowID = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Commits all the changes made to this row since the last time AcceptChanges was
		/// called.
		/// </summary>
		[MonoTODO]
		public void AcceptChanges () 
		{
			
			if(rowState == DataRowState.Added)
			{
				//Instantiate original and proposed values so that we can call
				//EndEdit()
				this.BeginEdit();
			}

			this.EndEdit ();

			switch (rowState)
			{
				case DataRowState.Added:
				case DataRowState.Detached:
				case DataRowState.Modified:
					rowState = DataRowState.Unchanged;
					break;
				case DataRowState.Deleted:
					_table.Rows.Remove (this); //FIXME: this should occur in end edit
					break;
			}

			//MS implementation assigns the Proposed values
			//to both current and original and keeps original after calling AcceptChanges
			//Copy proposed to original in this.EndEdit()
			//original = null;
		}

		/// <summary>
		/// Begins an edit operation on a DataRow object.
		/// </summary>
		[MonoTODO]
		public void BeginEdit() 
		{
			if (rowState == DataRowState.Deleted)
				throw new DeletedRowInaccessibleException ();

			if (!HasVersion (DataRowVersion.Proposed))
			{
				proposed = new object[_table.Columns.Count];
				Array.Copy (current, proposed, _table.Columns.Count);
			}
			//TODO: Suspend validation

			//FIXME: this doesn't happen on begin edit
			if (!HasVersion (DataRowVersion.Original))
			{
				original = new object[_table.Columns.Count];
				Array.Copy (current, original, _table.Columns.Count);
			}
		}

		/// <summary>
		/// Cancels the current edit on the row.
		/// </summary>
		[MonoTODO]
		public void CancelEdit () 
		{
			//FIXME: original doesn't get erased on CancelEdit
			//TODO: Events
			if (HasVersion (DataRowVersion.Proposed))
			{
				original = null;
				proposed = null;
				rowState = DataRowState.Unchanged;
			}
		}

		/// <summary>
		/// Clears the errors for the row, including the RowError and errors set with
		/// SetColumnError.
		/// </summary>
		public void ClearErrors () 
		{
			rowError = String.Empty;
			columnErrors = new String[_table.Columns.Count];
		}

		/// <summary>
		/// Deletes the DataRow.
		/// </summary>
		[MonoTODO]
		public void Delete () 
		{
			switch (rowState) {
			case DataRowState.Added:
				Table.Rows.Remove (this);
				break;
			case DataRowState.Deleted:
				throw new DeletedRowInaccessibleException ();
			default:
				//TODO: Events, Constraints
				rowState = DataRowState.Deleted;
				break;
			}
		}

		/// <summary>
		/// Ends the edit occurring on the row.
		/// </summary>
		[MonoTODO]
		public void EndEdit () 
		{
			if (HasVersion (DataRowVersion.Proposed))
			{
				rowState = DataRowState.Modified;
				
				//Calling next method validates UniqueConstraints
				//and ForeignKeys.
				_table.Rows.ValidateDataRowInternal(this);
				
				Array.Copy (proposed, current, _table.Columns.Count);
				
				//FIXME: MS implementation assigns the proposed values to
				//the original values. Should this be done here or on the
				//AcceptChanges() method?
				Array.Copy (proposed, original, _table.Columns.Count);

				proposed = null;
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
		[MonoTODO]
		public DataRow[] GetChildRows (DataRelation relation, DataRowVersion version) 
		{
			// TODO: Caching for better preformance
			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			foreach (DataRow row in relation.ChildTable.Rows) {
				bool allColumnsMatch = true;
				for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) {
					if (!this[parentColumns[columnCnt], version].Equals(
					    row[childColumns[columnCnt], version])) {
						allColumnsMatch = false;
						break;
					}
				}
				if (allColumnsMatch) rows.Add(row);
			}
			return rows.ToArray(typeof(DataRow)) as DataRow[];
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (string relationName, DataRowVersion version) 
		{
			return GetChildRows (Table.DataSet.Relations[relationName], version);
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
			if (columnIndex < 0 || columnIndex >= columnErrors.Length)
				throw new IndexOutOfRangeException ();

			return columnErrors[columnIndex];
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

			for (int i = 0; i < columnErrors.Length; i += 1)
			{
				if (columnErrors[i] != String.Empty)
					dataColumns.Add (_table.Columns[i]);
			}

			return (DataColumn[])(dataColumns.ToArray ());
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
			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			foreach (DataRow row in relation.ParentTable.Rows) {
				bool allColumnsMatch = true;
				for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) {
					if (!this[parentColumns[columnCnt], version].Equals(
					    row[childColumns[columnCnt], version])) {
						allColumnsMatch = false;
						break;
					}
				}
				if (allColumnsMatch) rows.Add(row);
			}
			return rows.ToArray(typeof(DataRow)) as DataRow[];
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
			switch (version)
			{
				case DataRowVersion.Default:
					return true;
				case DataRowVersion.Proposed:
					return (proposed != null);
				case DataRowVersion.Current:
					return (current != null);
				case DataRowVersion.Original:
					return (original != null);
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column) 
		{
			return (this[column] == null);
		}

		/// <summary>
		/// Gets a value indicating whether the column at the specified index contains a null
		/// value.
		/// </summary>
		public bool IsNull (int columnIndex) 
		{
			return (this[columnIndex] == null);
		}

		/// <summary>
		/// Gets a value indicating whether the named column contains a null value.
		/// </summary>
		public bool IsNull (string columnName) 
		{
			return (this[columnName] == null);
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn and DataRowVersion
		/// contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column, DataRowVersion version) 
		{
			return (this[column, version] == null);
		}

		/// <summary>
		/// Rejects all changes made to the row since AcceptChanges was last called.
		/// </summary>
		public void RejectChanges () 
		{
			// If original is null, then nothing has happened since AcceptChanges
			// was last called.  We have no "original" to go back to.
			if (original != null)
			{
				Array.Copy (original, current, _table.Columns.Count);
			       
				_table.ChangedDataRow (this, DataRowAction.Rollback);
				CancelEdit ();
				switch (rowState)
				{
					case DataRowState.Added:
						_table.Rows.Remove (this);
						break;
					case DataRowState.Modified:
						rowState = DataRowState.Unchanged;
						break;
					case DataRowState.Deleted:
						rowState = DataRowState.Unchanged;
						break;
				} 
				
			} 			
			else {
				// If rows are just loaded via Xml the original values are null.
				// So in this case we have to remove all columns.
				// FIXME: I'm not realy sure, does this break something else, but
				// if so: FIXME ;)
				
				if ((rowState & DataRowState.Added) > 0)
					_table.Rows.Remove (this);
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
			if (columnIndex < 0 || columnIndex >= columnErrors.Length)
				throw new IndexOutOfRangeException ();
			columnErrors[columnIndex] = error;
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
		[MonoTODO]
		protected void SetNull (DataColumn column) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow.
		/// </summary>
		[MonoTODO]
		public void SetParentRow (DataRow parentRow) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow and
		/// DataRelation.
		/// </summary>
		[MonoTODO]
		public void SetParentRow (DataRow parentRow, DataRelation relation) 
		{
			throw new NotImplementedException ();
		}

		
		#endregion // Methods
	}
}
