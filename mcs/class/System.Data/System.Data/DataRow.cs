//
// System.Data.DataRow.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;

namespace System.Data
{
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
	public class DataRow
	{
		#region Fields

		private DataTable table;

		private object[] original;
		private object[] proposed;
		private object[] current;

		private Hashtable versions;
		private string[] columnErrors;
		private string rowError;
		private DataRowState rowState;

		#endregion

		#region Constructors

		/// <summary>
		/// This member supports the .NET Framework infrastructure and is not intended to be 
		/// used directly from your code.
		/// </summary>
		protected internal DataRow (DataRowBuilder builder)
		{
			table = builder.Table;

			original = new object[table.Columns.Count];
			proposed = null;
			current = new object[table.Columns.Count];

			Array.Copy (current, original, table.Columns.Count);
	
			versions = new Hashtable ();

			versions[DataRowVersion.Original] = original;
			versions[DataRowVersion.Proposed] = proposed;
			versions[DataRowVersion.Current] = current;

			columnErrors = new string[table.Columns.Count];
			rowError = String.Empty;

			rowState = DataRowState.Unchanged;
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
			get { return this[columnName, DataRowVersion.Current]; }
			set {
				DataColumn column = table.Columns[columnName];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in specified DataColumn
		/// </summary>
		public object this[DataColumn column] {
			get { return this[column, DataRowVersion.Current]; }
			set {
				if (column == null)
					throw new ArgumentNullException ();
				int columnIndex = table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ();
				if (column.DataType != value.GetType ())
					throw new InvalidCastException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				BeginEdit ();
				proposed[columnIndex] = value;
				EndEdit ();
			}
		}

		/// <summary>
		/// Gets or sets the data stored in column specified by index.
		/// </summary>
		public object this[int columnIndex] {
			get { return this[columnIndex, DataRowVersion.Current]; }
			set {
				DataColumn column = table.Columns[columnIndex];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the named column.
		/// </summary>
		public object this[string columnName, DataRowVersion version] {
			get {
				DataColumn column = table.Columns[columnName];
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
				int columnIndex = table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ();
				if (version == DataRowVersion.Default)
					return column.DefaultValue;
				if (!HasVersion (version))
					throw new VersionNotFoundException ();
				return ((object[])(versions[version]))[columnIndex];
			}
		}

		/// <summary>
		/// Gets the data stored in the column, specified by index and version of the data to
		/// retrieve.
		/// </summary>
		public object this[int columnIndex, DataRowVersion version] {
			get {
				DataColumn column = table.Columns[columnIndex];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				return this[column, version];
			}
		}

		/// <summary>
		/// Gets or sets all of the values for this row through an array.
		/// </summary>
		public object[] ItemArray {
			get { return current; }
			set {
				if (value.Length > table.Columns.Count)
					throw new ArgumentException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();

				for (int i = 0; i < value.Length; i += 1)
				{
					if (table.Columns[i].ReadOnly && value[i] != this[i])
						throw new ReadOnlyException ();
					if (value[i] == null)
					{
						if (!table.Columns[i].AllowDBNull)
							throw new NoNullAllowedException ();
						continue;
					}
					if (table.Columns[i].DataType != value[i].GetType())
						throw new InvalidCastException ();
				}

				BeginEdit ();
				proposed = value;
				EndEdit ();
			}
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

		/// <summary>
		/// Gets the DataTable for which this row has a schema.
		/// </summary>
		public DataTable Table {
			get { return table; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Commits all the changes made to this row since the last time AcceptChanges was
		/// called.
		/// </summary>
		public void AcceptChanges () 
		{
			this.EndEdit ();

			Array.Copy (proposed, current, table.Columns.Count);
			proposed = null;

			switch (rowState)
			{
				case DataRowState.Added:
					rowState = DataRowState.Unchanged;
					break;
				case DataRowState.Modified:
					rowState = DataRowState.Unchanged;
					break;
				case DataRowState.Deleted:
					table.Rows.Remove (this);
					break;
			}
		}

		/// <summary>
		/// Begins an edit operation on a DataRow object.
		/// </summary>
		public void BeginEdit() 
		{
			proposed = new object[table.Columns.Count];
			Array.Copy (current, proposed, table.Columns.Count);
		}

		/// <summary>
		/// Cancels the current edit on the row.
		/// </summary>
		public void CancelEdit() 
		{
			proposed = null;
			rowState = DataRowState.Unchanged;
		}

		/// <summary>
		/// Clears the errors for the row, including the RowError and errors set with
		/// SetColumnError.
		/// </summary>
		public void ClearErrors() 
		{
			rowError = String.Empty;
			columnErrors = new String[table.Columns.Count];
		}

		/// <summary>
		/// Deletes the DataRow.
		/// </summary>
		public void Delete() 
		{
			rowState = DataRowState.Deleted;
		}

		/// <summary>
		/// Ends the edit occurring on the row.
		/// </summary>
		public void EndEdit() 
		{
			rowState = DataRowState.Modified;
		}

		/// <summary>
		/// Gets the child rows of this DataRow using the specified DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetChildRows (DataRelation relation) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetChildRows (string relationName) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetChildRows (DataRelation relation, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetChildRows (string relationName, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the error description of the specified DataColumn.
		/// </summary>
		public string GetColumnError (DataColumn column) 
		{
			return GetColumnError (table.Columns.IndexOf(column));
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
			return GetColumnError (table.Columns.IndexOf(columnName));
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
					dataColumns.Add (table.Columns[i]);
			}

			return (DataColumn[])(dataColumns.ToArray ());
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow GetParentRow (DataRelation relation) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow GetParentRow (string relationName) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow GetParentRow (DataRelation relation, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow GetParentRow (string relationName, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetParentRows (DataRelation relation) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetParentRows (string relationName) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetParentRows (DataRelation relation, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		[MonoTODO]
		public DataRow[] GetParentRows (string relationName, DataRowVersion version) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets a value indicating whether a specified version exists.
		/// </summary>
		public bool HasVersion (DataRowVersion version) 
		{
			return (versions[version] != null);
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
		[MonoTODO]
		public void RejectChanges () 
		{
			CancelEdit ();
		}

		/// <summary>
		/// Sets the error description for a column specified as a DataColumn.
		/// </summary>
		public void SetColumnError (DataColumn column, string error) 
		{
			SetColumnError (table.Columns.IndexOf (column), error);
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
			SetColumnError (table.Columns.IndexOf (columnName), error);
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
