//
// System.Data.SqlClient.SqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Data.SqlClient {
	public sealed class SqlDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		int fieldCount;
		bool hasRows;
		bool isClosed;
		int recordsAffected;

		SqlCommand command;
		DataTable schemaTable;

		#endregion // Fields

		#region Constructors

		internal SqlDataReader (SqlCommand command)
		{
			schemaTable = ConstructSchemaTable ();
			this.command = command;
			this.fieldCount = 0;
			this.isClosed = false;
		}

		#endregion

		#region Properties

		public int Depth {
			get { return 0; }
		}

		public int FieldCount {
			get { return fieldCount; }
		}

		public bool HasRows {
			get { return hasRows; }
		}

		public bool IsClosed {
			get { return isClosed; }
		}

		public object this [int i] {
			get { return GetValue (i); }
		}

		public object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}

		public int RecordsAffected {
			get { return recordsAffected; }
		}

		#endregion // Properties

		#region Methods

		public void Close()
		{
			isClosed = true;
			command.Connection.DataReaderOpen = false;
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = Type.GetType ("System.Boolean");
			Type stringType = Type.GetType ("System.String");
			Type intType = Type.GetType ("System.Int32");
			Type typeType = Type.GetType ("System.Type");
			Type shortType = Type.GetType ("System.Int16");

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("BaseServerName", stringType);
			schemaTable.Columns.Add ("BaseCatalogName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("ProviderType", booleanType);
			schemaTable.Columns.Add ("IsAliased", booleanType);
			schemaTable.Columns.Add ("IsExpression", booleanType);
			schemaTable.Columns.Add ("IsIdentity", booleanType);
			schemaTable.Columns.Add ("IsAutoIncrement", booleanType);
			schemaTable.Columns.Add ("IsRowVersion", booleanType);
			schemaTable.Columns.Add ("IsHidden", booleanType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("IsReadOnly", booleanType);

			return schemaTable;
		}

		[MonoTODO]
		public bool GetBoolean (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte GetByte (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDataTypeName (int i)
		{
			throw new NotImplementedException ();
		}

		public DateTime GetDateTime (int i)
		{
			object value = GetValue (i);
			if (!(value is DateTime))
				throw new InvalidCastException ();
			return (DateTime) value;
		}

		public decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal))
				throw new InvalidCastException ();
			return (decimal) value;
		}
		public double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double))
				throw new InvalidCastException ();
			return (double) value;
		}

		public Type GetFieldType (int i)
		{
			return GetValue (i).GetType ();
		}

		public float GetFloat (int i)
		{
			object value = GetValue (i);
			if (!(value is float))
				throw new InvalidCastException ();
			return (float) value;
		}

		[MonoTODO]
		public Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}

		public short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short))
				throw new InvalidCastException ();
			return (short) value;
		}

		public int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int))
				throw new InvalidCastException ();
			return (int) value;
		}

		public long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long))
				throw new InvalidCastException ();
			return (long) value;
		}

		public string GetName (int i)
		{
			return (string) schemaTable.Rows[i]["ColumnName"];
		}

		[MonoTODO]
		public int GetOrdinal (string name)
		{
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (((string) schemaRow ["ColumnName"]).Equals (name))
					return (int) schemaRow ["ColumnOrdinal"];
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (String.Compare (((string) schemaRow ["ColumnName"]), name, true) == 0)
					return (int) schemaRow ["ColumnOrdinal"];
			throw new IndexOutOfRangeException ();
		}

		public DataTable GetSchemaTable ()
		{
			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;

			fieldCount = 0;

			foreach (TdsColumnSchema schemaObject in command.Tds.ColumnInfo) {
				DataRow schemaRow = schemaTable.NewRow ();

				schemaRow ["ColumnName"] = schemaObject.ColumnName;
				schemaRow ["ColumnOrdinal"] = schemaObject.ColumnOrdinal;
				schemaRow ["BaseTableName"] = schemaObject.TableName;
				schemaRow ["AllowDBNull"] = schemaObject.Nullable;
				schemaRow ["IsReadOnly"] = !schemaObject.Writable;

				if (schemaObject.NumericPrecision >= 0)
					schemaRow ["NumericPrecision"] = schemaObject.NumericPrecision;
				else
					schemaRow ["NumericPrecision"] = DBNull.Value;

				if (schemaObject.NumericScale >= 0)
					schemaRow ["NumericScale"] = schemaObject.NumericScale;
				else
					schemaRow ["NumericScale"] = DBNull.Value;

				schemaTable.Rows.Add (schemaRow);

				fieldCount += 1;
			}
			return schemaTable;
		}		

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string))
				throw new InvalidCastException ();
			return (string) value;
		}

		public object GetValue (int i)
		{
			return command.Tds.ColumnValues[i];
		}

		public int GetValues (object[] values)
		{
			int len = values.Length;
			command.Tds.ColumnValues.CopyTo (0, values, 0, len);
			return (len > FieldCount ? len : FieldCount);
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsDBNull (int i)
		{
			throw new NotImplementedException ();
		}

		public bool NextResult ()
		{
			schemaTable.Rows.Clear ();
			bool result = command.Tds.NextResult ();
			if (command.Tds.Errors.Count > 0)
				throw SqlException.FromTdsError (command.Tds.Errors);
			return result;
		}

		public bool Read ()
		{
			bool result = command.Tds.NextRow ();
			if (command.Tds.Errors.Count > 0)
				throw SqlException.FromTdsError (command.Tds.Errors);
			return result;
		}

		#endregion // Methods
	}
}
