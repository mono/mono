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

		protected internal DataRow (DataRowBuilder builder)
		{
			table = builder.Table;

			original = new object[table.Columns.Count];
			proposed = null;
			current = new object[table.Columns.Count];
	
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

		public bool HasErrors {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public object this[string columnName] {
			get { return this[columnName, DataRowVersion.Current]; }
			set {
				DataColumn column = table.Columns[columnName];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

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

		public object this[int columnIndex] {
			get { return this[columnIndex, DataRowVersion.Current]; }
			set {
				DataColumn column = table.Columns[columnIndex];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				this[column] = value;
			}
		}

		public object this[string columnName, DataRowVersion version] {
			get {
				DataColumn column = table.Columns[columnName];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				return this[column, version];
			}
		}

		public object this[DataColumn column, DataRowVersion version] {
			get {
				if (column == null)
					throw new ArgumentNullException ();	
				int columnIndex = table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ();
				if (version == DataRowVersion.Default)
					return column.DefaultValue;
				if (versions[version] == null)
					throw new VersionNotFoundException ();
				return ((object[])(versions[version]))[columnIndex];
			}
		}

		public object this[int columnIndex, DataRowVersion version] {
			get {
				DataColumn column = table.Columns[columnIndex];
				if (column == null) 
					throw new IndexOutOfRangeException ();
				return this[column, version];
			}
		}

		public object[] ItemArray {
			get { return current; }
			set {
				if (value.Length > table.Columns.Count)
					throw new ArgumentException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				BeginEdit ();
				proposed = value;

				for (int i = 0; i < value.Length; i += 1)
				{
					if (table.Columns[i].DataType != value[i].GetType())
						throw new InvalidCastException ();
					if (table.Columns[i].ReadOnly && value[i] != this[i])
						throw new ReadOnlyException ();
					if (!table.Columns[i].AllowDBNull && value[i] == null)
						throw new NoNullAllowedException ();
				}
				EndEdit ();
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
			get { return table; }
		}

		#endregion

		#region Methods

		public void AcceptChanges () 
		{
			this.EndEdit ();

			switch (rowState)
			{
				case DataRowState.Added:
					rowState = DataRowState.Unchanged;
					break;
				case DataRowState.Modified:
					rowState = DataRowState.Unchanged;
					break;
				case DataRowState.Deleted:
					rowState = DataRowState.Deleted;
					break;
			}
		}

		public void BeginEdit() 
		{
			proposed = new object[table.Columns.Count];
			Array.Copy (current, proposed, table.Columns.Count);
		}

		public void CancelEdit() 
		{
			proposed = null;
			rowState = DataRowState.Unchanged;
		}

		[MonoTODO]
		public void ClearErrors() 
		{
			throw new NotImplementedException ();
		}

		public void Delete() 
		{
			rowState = DataRowState.Deleted;
		}

		public void EndEdit() 
		{
			Array.Copy (proposed, current, table.Columns.Count);
			proposed = null;
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

		public bool IsNull (DataColumn column) 
		{
			return (this[column] == null);
		}

		public bool IsNull (int columnIndex) 
		{
			return (this[columnIndex] == null);
		}

		public bool IsNull (string columnName) 
		{
			return (this[columnName] == null);
		}

		public bool IsNull (DataColumn column, DataRowVersion version) 
		{
			return (this[column, version] == null);
		}

		[MonoTODO]
		public void RejectChanges () 
		{
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
