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
		private Hashtable dataRowVersions;
		private ArrayList columnErrors;
		private string rowError;
		private bool deleted;
		private DataRowState rowState;

		#endregion

		#region Constructors

		protected internal DataRow (DataRowBuilder builder)
		{
			table = builder.Table;

			dataRowVersions = new Hashtable ();
			dataRowVersions[DataRowVersion.Original] = new ArrayList ();
			dataRowVersions[DataRowVersion.Default] = new ArrayList ();
			dataRowVersions[DataRowVersion.Current] = new ArrayList ();
			dataRowVersions[DataRowVersion.Proposed] = new ArrayList ();

			columnErrors = new ArrayList ();
			rowError = String.Empty;

			deleted = false;

			rowState = DataRowState.Unchanged;
		}

		#endregion

		#region Properties

		public bool HasErrors {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public object this[string columnName] {
			get { return this[columnName, DataRowVersion.Default]; }
			set { ((ArrayList)(dataRowVersions[DataRowVersion.Default]))[table.Columns.IndexOf(columnName)] = value; }
		}

		public object this[DataColumn column] {
			get { return this[column, DataRowVersion.Default]; }
			set { ((ArrayList)(dataRowVersions[DataRowVersion.Default]))[table.Columns.IndexOf(column)] = value; }
		}

		public object this[int columnIndex] {
			get { return this[columnIndex, DataRowVersion.Default]; }
			set { ((ArrayList)(dataRowVersions[DataRowVersion.Default]))[columnIndex] = value; }
		}

		public object this[string columnName, DataRowVersion version] {
			get { return ((ArrayList)(dataRowVersions[version]))[table.Columns.IndexOf(columnName)]; }
		}

		public object this[DataColumn column, DataRowVersion version] {
			get { return ((ArrayList)(dataRowVersions[version]))[table.Columns.IndexOf(column)]; }
		}

		public object this[int columnIndex, DataRowVersion version] {
			get { return ((ArrayList)(dataRowVersions[version]))[columnIndex]; }
		}

		public object[] ItemArray {
			get { return ((ArrayList)(dataRowVersions[DataRowVersion.Default])).ToArray (); }
			set {
				if (deleted)
					throw new DeletedRowInaccessibleException ();

				if (value.Length > table.Columns.Count)
					throw new ArgumentException ("The array is larger than the number of columns in the table.");
				for (int i = 0; i < value.Length; i += 1)
				{
					if (table.Columns[i].DataType != value[i].GetType())
						throw new InvalidCastException ();
					if (table.Columns[i].ReadOnly && value[i] != ((ArrayList)(dataRowVersions[DataRowVersion.Default]))[i])
						throw new ReadOnlyException ();
					if (!table.Columns[i].AllowDBNull && value[i] == null)
						throw new NoNullAllowedException ();
					((ArrayList)(dataRowVersions[DataRowVersion.Default]))[i] = value[i];
				}
			}
		}

		public string RowError {
			get {
				if (this.HasErrors)
					return rowError;

				return String.Empty;
			}
			set { rowError = value; }
		}

		public DataRowState RowState {
			get { return rowState; }
		}

		public DataTable Table {
			[MonoTODO]
			get {
				return table;
			}
		}

		#endregion

		#region Methods

		public void AcceptChanges () 
		{
			this.EndEdit ();
			// if the RowState was Added or Modified, RowState becomes Unchanged
			// if the RowState was Deleted, the row is removed.
		}

		public void BeginEdit() 
		{
			dataRowVersions[DataRowVersion.Proposed] = dataRowVersions[DataRowVersion.Current];
			dataRowVersions[DataRowVersion.Default] = dataRowVersions[DataRowVersion.Proposed];
		}

		public void CancelEdit() 
		{
			dataRowVersions[DataRowVersion.Proposed] = null;
			dataRowVersions[DataRowVersion.Default] = dataRowVersions[DataRowVersion.Current];
			rowState = DataRowState.Unchanged;
		}

		[MonoTODO]
		public void ClearErrors() {
			throw new NotImplementedException ();
		}

		public void Delete() 
		{
			rowState = DataRowState.Deleted;
		}

		public void EndEdit() 
		{
			dataRowVersions[DataRowVersion.Current] = dataRowVersions[DataRowVersion.Proposed];
			dataRowVersions[DataRowVersion.Default] = dataRowVersions[DataRowVersion.Current];
			dataRowVersions[DataRowVersion.Proposed] = null;
			rowState = DataRowState.Modified;
		}

		[MonoTODO]
		public DataRow[] GetChildRows (DataRelation dr) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows (DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows (string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		public string GetColumnError (DataColumn column) 
		{
			if (this.HasErrors)
				return (string)(columnErrors[table.Columns.IndexOf(column)]);
			
			return String.Empty;
		}

		public string GetColumnError (int columnIndex) 
		{
			if (columnIndex > table.Columns.Count)
				throw new IndexOutOfRangeException ();

			if (this.HasErrors)
				return (string)(columnErrors[columnIndex]);

			return String.Empty;
		}

		public string GetColumnError (string columnName) 
		{
			if (this.HasErrors)
				return (string)(columnErrors[table.Columns.IndexOf(columnName)]);

			return String.Empty;
		}

		[MonoTODO]
		public DataColumn[] GetColumnsInError () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow (DataRelation dr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow (DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow (string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows (DataRelation dr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows (DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows (string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasVersion (DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull (DataColumn dc) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull (int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull (DataColumn dc, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RejectChanges () {
			CancelEdit ();
		}

		[MonoTODO]
		public void SetColumnError (DataColumn dc, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetColumnError (int i, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetColumnError (string a, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetParentRow (DataRow row) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetParentRow (DataRow row, DataRelation rel) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetNull (DataColumn column) {
			throw new NotImplementedException ();
		}
		
		#endregion // Methods
	}
}
