//
// System.Data.DataTableReader.cs
//
// Author:
//   Sureshkumar T      <tsureshkumar@novell.com>
//   Tim Coleman        (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.Collections;
using System.Data.Common;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace System.Data {
	public sealed class DataTableReader : DbDataReader {
		bool            _closed;
		DataTable []    _tables;
		int             _current = -1;
		int             _index;
		DataTable       _schemaTable;
		bool            _tableCleared = false;
		bool            _subscribed = false;
		DataRow         _rowRef;
		bool 		_schemaChanged = false;

		#region Constructors

		public DataTableReader (DataTable dataTable)
			: this (new DataTable[] {dataTable})
		{
		}

		public DataTableReader (DataTable[] dataTables)
		{
			if (dataTables == null || dataTables.Length <= 0)
				throw new ArgumentException ("Cannot Create DataTable. Argument Empty!");

			this._tables = new DataTable [dataTables.Length];

			for (int i = 0; i < dataTables.Length; i++)
				this._tables [i] = dataTables [i];

			_closed = false;
			_index = 0;
			_current = -1;
			_rowRef = null;
			_tableCleared = false;

			SubscribeEvents ();
		}

		#endregion // Constructors

		#region Properties

		public override int Depth {
			get { return 0; }
		}

		public override int FieldCount {
			get { return CurrentTable.Columns.Count; }
		}

		public override bool HasRows {
			get { return CurrentTable.Rows.Count > 0; }
		}

		public override bool IsClosed {
			get { return _closed; }
		}

		public override object this [int ordinal] {
			get {
				Validate ();
				if (ordinal < 0 || ordinal >= FieldCount)
					throw new ArgumentOutOfRangeException ("index " + ordinal + " is not in the range");
				DataRow row = CurrentRow;
				if (row.RowState == DataRowState.Deleted)
					throw new InvalidOperationException ("Deleted Row's information cannot be accessed!");
				return row [ordinal];
			}
		}

		private DataTable CurrentTable {
			get { return _tables [_index]; }
		}

		private DataRow CurrentRow {
			get { return (DataRow) CurrentTable.Rows [_current]; }
		}


		public override object this [string name] {
			get {
				Validate ();
				DataRow row = CurrentRow;
				if (row.RowState == DataRowState.Deleted)
					throw new InvalidOperationException ("Deleted Row's information cannot be accessed!");
				return row [name];
			}
		}

		public override int RecordsAffected {
			get { return 0; }
		}

		#endregion // Properties

		#region Methods

		private void SubscribeEvents ()
		{
			if (_subscribed)        // avoid subscribing multiple times
				return;
			CurrentTable.TableCleared += new DataTableClearEventHandler (OnTableCleared);
			CurrentTable.RowChanged += new DataRowChangeEventHandler (OnRowChanged);
			CurrentTable.Columns.CollectionChanged += new CollectionChangeEventHandler (OnColumnCollectionChanged);
			for (int i=0; i < CurrentTable.Columns.Count; ++i)
				CurrentTable.Columns [i].PropertyChanged += new PropertyChangedEventHandler (OnColumnChanged);
			_subscribed = true;
			_schemaChanged = false;
		}

		private void UnsubscribeEvents ()
		{
			if (!_subscribed)       // avoid un-subscribing multiple times
				return;
			CurrentTable.TableCleared -= new DataTableClearEventHandler (OnTableCleared);
			CurrentTable.RowChanged -= new DataRowChangeEventHandler (OnRowChanged);
			CurrentTable.Columns.CollectionChanged -= new CollectionChangeEventHandler (OnColumnCollectionChanged);
			for (int i=0; i < CurrentTable.Columns.Count; ++i)
				CurrentTable.Columns [i].PropertyChanged -= new PropertyChangedEventHandler (OnColumnChanged);
			_subscribed = false;
			_schemaChanged = false;
		}

		public override void Close ()
		{
			if (IsClosed)
				return;

			UnsubscribeEvents ();
			_closed = true;
		}

		public override bool GetBoolean (int ordinal)
		{
			return (bool) GetValue (ordinal);
		}

		public override byte GetByte (int ordinal)
		{
			return (byte) GetValue (ordinal);
		}

		public override long GetBytes (int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			byte[] value = this [ordinal] as byte[];
			if (value == null)
				ThrowInvalidCastException (this [ordinal].GetType (), typeof (byte[]));
			if (buffer == null)
				return value.Length;
			int copylen = length > value.Length ? value.Length : length;
			Array.Copy (value, dataIndex, buffer, bufferIndex, copylen);
			return copylen;
		}

		public override char GetChar (int ordinal)
		{
			return (char) GetValue (ordinal);
		}

		public override long GetChars (int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			char[] value = this [ordinal] as char[];
			if (value == null)
				ThrowInvalidCastException (this [ordinal].GetType (), typeof (char[]));
			if (buffer == null)
				return value.Length;
			int copylen = length > value.Length ? value.Length : length;
			Array.Copy (value, dataIndex, buffer, bufferIndex, copylen);
			return copylen;
		}

		public override string GetDataTypeName (int ordinal)
		{
			return GetFieldType (ordinal).ToString ();
		}

		public override DateTime GetDateTime (int ordinal)
		{
			return (DateTime) GetValue (ordinal);
		}

		public override decimal GetDecimal (int ordinal)
		{
			return (decimal) GetValue (ordinal);
		}

		public override double GetDouble (int ordinal)
		{
			return (double) GetValue (ordinal);
		}

		public override IEnumerator GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public override Type GetProviderSpecificFieldType (int ordinal)
		{
			return GetFieldType (ordinal);
		}

		public override Type GetFieldType (int ordinal)
		{
			ValidateClosed ();
			return CurrentTable.Columns [ordinal].DataType;
		}

		public override float GetFloat (int ordinal)
		{
			return (float) GetValue (ordinal);
		}

		public override Guid GetGuid (int ordinal)
		{
			return (Guid) GetValue (ordinal);
		}

		public override short GetInt16 (int ordinal)
		{
			return (short) GetValue (ordinal);
		}

		public override int GetInt32 (int ordinal)
		{
			return (int) GetValue (ordinal);
		}

		public override long GetInt64 (int ordinal)
		{
			return (long) GetValue (ordinal);
		}

		public override string GetName (int ordinal)
		{
			ValidateClosed ();
			return CurrentTable.Columns [ordinal].ColumnName;
		}

		public override int GetOrdinal (string name)
		{
			ValidateClosed ();
			int index = CurrentTable.Columns.IndexOf (name);
			if (index == -1)
				throw new ArgumentException (String.Format ("Column {0} is not found in the schema", name));
			return index;
		}

		public override object GetProviderSpecificValue (int ordinal)
		{
			return GetValue (ordinal);
		}

		public override int GetProviderSpecificValues (object[] values)
		{
			return GetValues (values);
		}

		public override string GetString (int ordinal)
		{
			return (string) GetValue (ordinal);
		}

		public override object GetValue (int ordinal)
		{
			return this [ordinal];
		}

		public override int GetValues (object[] values)
		{
			Validate ();
			if (CurrentRow.RowState == DataRowState.Deleted)
				throw new DeletedRowInaccessibleException ("");

			int count = (FieldCount < values.Length ? FieldCount : values.Length);
			for (int i=0; i < count; ++i)
				values [i] = CurrentRow [i];
			return count;
		}

		public override bool IsDBNull (int ordinal)
		{
			return GetValue (ordinal) is DBNull;
		}

		public override DataTable GetSchemaTable ()
		{
			ValidateClosed ();
			ValidateSchemaIntact ();

			if (_schemaTable != null)
				return _schemaTable;

			DataTable dt = new DataTable ();
			dt.Columns.Add ("ColumnName", typeof (string));
			dt.Columns.Add ("ColumnOrdinal", typeof (int));
			dt.Columns.Add ("ColumnSize", typeof (int));
			dt.Columns.Add ("NumericPrecision", typeof (short));
			dt.Columns.Add ("NumericScale", typeof (short));
			dt.Columns.Add ("DataType", typeof (Type));
			dt.Columns.Add ("ProviderType", typeof (int));
			dt.Columns.Add ("IsLong", typeof (bool));
			dt.Columns.Add ("AllowDBNull", typeof (bool));
			dt.Columns.Add ("IsReadOnly", typeof (bool));
			dt.Columns.Add ("IsRowVersion", typeof (bool));
			dt.Columns.Add ("IsUnique", typeof (bool));
			dt.Columns.Add ("IsKey", typeof (bool));
			dt.Columns.Add ("IsAutoIncrement", typeof (bool));
			dt.Columns.Add ("BaseCatalogName", typeof (string));
			dt.Columns.Add ("BaseSchemaName", typeof (string));
			dt.Columns.Add ("BaseTableName", typeof (string));
			dt.Columns.Add ("BaseColumnName", typeof (string));
			dt.Columns.Add ("AutoIncrementSeed", typeof (Int64));
			dt.Columns.Add ("AutoIncrementStep", typeof (Int64));
			dt.Columns.Add ("DefaultValue", typeof (object));
			dt.Columns.Add ("Expression", typeof (string));
			dt.Columns.Add ("ColumnMapping", typeof (MappingType));
			dt.Columns.Add ("BaseTableNamespace", typeof (string));
			dt.Columns.Add ("BaseColumnNamespace", typeof (string));

			DataRow row;
			DataColumn col;
			for (int i=0; i < CurrentTable.Columns.Count; ++i) {
				row = dt.NewRow ();
				col = CurrentTable.Columns [i];
				row ["ColumnName"] = col.ColumnName;
				row ["BaseColumnName"] = col.ColumnName;
				row ["ColumnOrdinal"] = col.Ordinal;
				row ["ColumnSize"] = col.MaxLength;
				// ms.net doesent set precision and scale even for Decimal values
				// when are these set ?
				row ["NumericPrecision"] = DBNull.Value;
				row ["NumericScale"] = DBNull.Value;
				row ["DataType"] = col.DataType;
				row ["ProviderType"] = DBNull.Value; //col.ProviderType;
				// ms.net doesent set this when datatype is string and maxlength = -1
				// when is this set ?
				row ["IsLong"] = false;
				row ["AllowDBNull"] = col.AllowDBNull;
				row ["IsReadOnly"] = col.ReadOnly;
				row ["IsRowVersion"] = false; //this is always false
				row ["IsUnique"] = col.Unique;
				row ["IsKey"] = (Array.IndexOf (CurrentTable.PrimaryKey, col) != -1) ;
				row ["IsAutoIncrement"]= col.AutoIncrement;
				row ["AutoIncrementSeed"] = col.AutoIncrementSeed;
				row ["AutoIncrementStep"] = col.AutoIncrementStep;
				row ["BaseCatalogName"] = (CurrentTable.DataSet != null ? CurrentTable.DataSet.DataSetName : null);
				row ["BaseSchemaName"] = DBNull.Value; // this is always null
				row ["BaseTableName"] = CurrentTable.TableName;
				row ["DefaultValue"] = col.DefaultValue;
				// If col expression depends on any external table , then the
				// Expression value is set to empty string in the schematable.
				if (col.Expression == "")
					row ["Expression"] = col.Expression;
				else {
					Regex reg = new Regex ("((Parent|Child)( )*[.(])", RegexOptions.IgnoreCase);
					if (reg.IsMatch (col.Expression, 0))
						row ["Expression"] = DBNull.Value;
					else
						row ["Expression"] = col.Expression;
				}

				row ["ColumnMapping"] = col.ColumnMapping;
				row ["BaseTableNamespace"] = CurrentTable.Namespace;
				row ["BaseColumnNamespace"] = col.Namespace;
				dt.Rows.Add (row);
			}

			return _schemaTable = dt;
		}

		private void Validate ()
		{
			ValidateClosed ();

			if (_index >= _tables.Length)
				throw new InvalidOperationException ("Invalid attempt to read when no data is present");
			if (_tableCleared)
				throw new RowNotInTableException ("The table is cleared, no rows are accessible");
			if (_current == -1)
				throw new InvalidOperationException ("DataReader is invalid for the DataTable");
			ValidateSchemaIntact ();
		}

		private void ValidateClosed ()
		{
			if (IsClosed)
				throw new InvalidOperationException ("Invalid attempt to read when the reader is closed");
		}

		private void ValidateSchemaIntact ()
		{
			if (_schemaChanged)
				throw new InvalidOperationException ("Schema of current DataTable '" + CurrentTable.TableName +
						"' in DataTableReader has changed, DataTableReader is invalid.");
		}

		void ThrowInvalidCastException (Type sourceType, Type destType)
		{
			throw new InvalidCastException (
				String.Format ("Unable to cast object of type '{0}' to type '{1}'.", sourceType, destType));
		}

		private bool MoveNext ()
		{
			if (_index >= _tables.Length || _tableCleared)
				return false;

			do {
				_current++;
			} while (_current < CurrentTable.Rows.Count && CurrentRow.RowState == DataRowState.Deleted);

			_rowRef = _current < CurrentTable.Rows.Count ? CurrentRow : null;

			return _current < CurrentTable.Rows.Count;

		}

		public override bool NextResult ()
		{
			if ((_index + 1) >= _tables.Length) {
				UnsubscribeEvents ();
				_index = _tables.Length;     // to make any attempt invalid
				return false; // end of tables.
			}

			UnsubscribeEvents ();
			_index++;
			_current = -1;
			_rowRef = null;
			_schemaTable = null;            // force to create fresh
			_tableCleared = false;
			SubscribeEvents ();
			return true;
		}

		public override bool Read ()
		{
			ValidateClosed ();
			return MoveNext ();
		}

		#endregion // Methods

		#region // Event Handlers

		private void OnColumnChanged (object sender, PropertyChangedEventArgs args)
		{
			_schemaChanged = true;
		}

		private void OnColumnCollectionChanged (object sender, CollectionChangeEventArgs args)
		{
			_schemaChanged = true;
		}

		private void OnRowChanged (object src, DataRowChangeEventArgs args)
		{
			DataRowAction action = args.Action;
			DataRow row = args.Row;
			if (action == DataRowAction.Add) {
				if (_tableCleared && _current != -1)
					return;

				if (_current == -1 || (_current >= 0 && row.RowID > CurrentRow.RowID)) {
					_tableCleared = false;
					return; // no. movement required, if row added after current.
				}

				_current++;
				_rowRef = CurrentRow;
			}

			if (action == DataRowAction.Commit && row.RowState == DataRowState.Detached) {
				// if i am the row deleted, move one down
				if (_rowRef == row) {
					_current --;
					_rowRef = _current >= 0 ? CurrentRow : null;
				}

				// if the row deleted is the last row, move down
				if (_current >= CurrentTable.Rows.Count) {
					_current--;
					_rowRef = _current >= 0 ? CurrentRow : null;
					return;
				}

				// deleting a row below _current moves the row one down
				if (_current > 0 && _rowRef == CurrentTable.Rows [_current-1]) {
					_current--;
					_rowRef = CurrentRow;
					return;
				}
			}
		}

		private void OnTableCleared (object src, DataTableClearEventArgs args)
		{
			_tableCleared = true;
		}

		#endregion // Event Handlers
	}
}

#endif // NET_2_0
