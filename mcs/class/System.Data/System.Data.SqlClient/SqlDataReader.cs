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
using System.Data.Common;

namespace System.Data.SqlClient {
	public sealed class SqlDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		int fieldCount;
		bool hasRows;
		bool isClosed;
		int recordsAffected;
		bool moreResults;

		int resultsRead;
		int rowsRead;

		SqlCommand command;
		DataTable schemaTable;
		FieldNameLookup lookup;

		ArrayList dataTypeNames;
		ArrayList dataTypes;

		#endregion // Fields

		#region Constructors

		internal SqlDataReader (SqlCommand command)
		{
			schemaTable = ConstructSchemaTable ();
			this.resultsRead = 0;
			this.command = command;
			this.fieldCount = 0;
			this.isClosed = false;

			NextResult ();
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
		
		internal FieldNameLookup Lookup {
			get { return lookup; }
		}

		public int RecordsAffected {
			get { return recordsAffected; }
		}

		#endregion // Properties

		#region Methods

		public void Close()
		{
			isClosed = true;
			command.CloseDataReader (moreResults);
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
			schemaTable.Columns.Add ("ProviderType", intType);
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

		public string GetDataTypeName (int i)
		{
			return (string) dataTypeNames [i];
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
			return (Type) schemaTable.Rows[i]["DataType"];
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

			if (!moreResults)
				return null;

			fieldCount = 0;

			dataTypeNames = new ArrayList ();
			dataTypes = new ArrayList ();

			foreach (TdsSchemaInfo schema in command.Tds.Schema) {
				DataRow row = schemaTable.NewRow ();


				switch (schema.ColumnType) {
					case TdsColumnType.Image :
						dataTypeNames.Add ("image");
						row ["ProviderType"] = (int) SqlDbType.Image;
						row ["DataType"] = typeof (byte[]);
						break;
					case TdsColumnType.Text :
						dataTypes.Add (typeof (string));
						dataTypeNames.Add ("text");
						row ["ProviderType"] = (int) SqlDbType.Text;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.UniqueIdentifier :
						dataTypeNames.Add ("uniqueidentifier");
						row ["ProviderType"] = (int) SqlDbType.UniqueIdentifier;
						row ["DataType"] = typeof (Guid);
						break;
					case TdsColumnType.VarBinary :
					case TdsColumnType.BigVarBinary :
						dataTypeNames.Add ("varbinary");
						row ["ProviderType"] = (int) SqlDbType.VarBinary;
						row ["DataType"] = typeof (byte[]);
						break;
					case TdsColumnType.IntN :
					case TdsColumnType.Int4 :
						dataTypeNames.Add ("int");
						row ["ProviderType"] = (int) SqlDbType.Int;
						row ["DataType"] = typeof (int);
						break;
					case TdsColumnType.VarChar :
					case TdsColumnType.BigVarChar :
						dataTypeNames.Add ("varchar");
						row ["ProviderType"] = (int) SqlDbType.VarChar;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.Binary :
					case TdsColumnType.BigBinary :
						dataTypeNames.Add ("binary");
						row ["ProviderType"] = (int) SqlDbType.Binary;
						row ["DataType"] = typeof (byte[]);
						break;
					case TdsColumnType.Char :
					case TdsColumnType.BigChar :
						dataTypeNames.Add ("char");
						row ["ProviderType"] = (int) SqlDbType.Char;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.Int1 :
						dataTypeNames.Add ("tinyint");
						row ["ProviderType"] = (int) SqlDbType.TinyInt;
						row ["DataType"] = typeof (byte);
						break;
					case TdsColumnType.Bit :
					case TdsColumnType.BitN :
						dataTypeNames.Add ("bit");
						row ["ProviderType"] = (int) SqlDbType.Bit;
						row ["DataType"] = typeof (bool);
						break;
					case TdsColumnType.Int2 :
						dataTypeNames.Add ("smallint");
						row ["ProviderType"] = (int) SqlDbType.SmallInt;
						row ["DataType"] = typeof (short);
						break;
					case TdsColumnType.DateTime4 :
					case TdsColumnType.DateTime :
					case TdsColumnType.DateTimeN :
						dataTypeNames.Add ("datetime");
						row ["ProviderType"] = (int) SqlDbType.DateTime;
						row ["DataType"] = typeof (DateTime);
						break;
					case TdsColumnType.Real :
						dataTypeNames.Add ("real");
						row ["ProviderType"] = (int) SqlDbType.Real;
						row ["DataType"] = typeof (float);
						break;
					case TdsColumnType.Money :
					case TdsColumnType.MoneyN :
					case TdsColumnType.Money4 :
						dataTypeNames.Add ("money");
						row ["ProviderType"] = (int) SqlDbType.Money;
						row ["DataType"] = typeof (decimal);
						break;
					case TdsColumnType.Float8 :
					case TdsColumnType.FloatN :
						dataTypeNames.Add ("float");
						row ["ProviderType"] = (int) SqlDbType.Float;
						row ["DataType"] = typeof (double);
						break;
					case TdsColumnType.NText :
						dataTypeNames.Add ("ntext");
						row ["ProviderType"] = (int) SqlDbType.NText;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.NVarChar :
						dataTypeNames.Add ("nvarchar");
						row ["ProviderType"] = (int) SqlDbType.NVarChar;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.Decimal :
					case TdsColumnType.Numeric :
						dataTypeNames.Add ("decimal");
						row ["ProviderType"] = (int) SqlDbType.Decimal;
						row ["DataType"] = typeof (decimal);
						break;
					case TdsColumnType.NChar :
						dataTypeNames.Add ("nchar");
						row ["ProviderType"] = (int) SqlDbType.Char;
						row ["DataType"] = typeof (string);
						break;
					case TdsColumnType.SmallMoney :
						dataTypeNames.Add ("smallmoney");
						row ["ProviderType"] = (int) SqlDbType.SmallMoney;
						row ["DataType"] = typeof (decimal);
						break;
					default :
						dataTypeNames.Add ("variant");
						row ["ProviderType"] = (int) SqlDbType.Variant;
						row ["DataType"] = typeof (object);
						break;
				}

				row ["ColumnOrdinal"] = schema.ColumnOrdinal;
				row ["AllowDBNull"] = schema.AllowDBNull;
				row ["IsReadOnly"] = schema.IsReadOnly;
				row ["IsIdentity"] = schema.IsIdentity;
				row ["IsKey"] = schema.IsKey;

				// FIXME: Base Column Name and Column Name are not necessarily the same
				if (schema.ColumnName == null)
					row ["BaseColumnName"] = DBNull.Value;
				else
					row ["BaseColumnName"] = schema.ColumnName;

				if (schema.ColumnName == null)
					row ["ColumnName"] = DBNull.Value;
				else
					row ["ColumnName"] = schema.ColumnName;

				if (schema.TableName == null)
					row ["BaseTableName"] = DBNull.Value;
				else
					row ["BaseTableName"] = schema.TableName;

				if (schema.NumericScale == 0)
					row ["NumericPrecision"] = DBNull.Value;
				else
					row ["NumericPrecision"] = schema.NumericPrecision;

				if (schema.NumericScale == 0)
					row ["NumericScale"] = DBNull.Value;
				else
					row ["NumericScale"] = schema.NumericScale;

				schemaTable.Rows.Add (row);

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

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int i)
		{
			return GetValue (i) == null;
		}

		public bool NextResult ()
		{
			if ((command.CommandBehavior & CommandBehavior.SingleResult) != 0 && resultsRead > 0)
				return false;

			schemaTable.Rows.Clear ();

			if (lookup != null)
				lookup.Clear ();

			moreResults = command.Tds.NextResult ();
			command.Connection.CheckForErrors ();
			if (moreResults)
				lookup = new FieldNameLookup (GetSchemaTable ());
			rowsRead = 0;
			resultsRead += 1;
			return moreResults;
		}

		public bool Read ()
		{
			if ((command.CommandBehavior & CommandBehavior.SingleRow) != 0 && rowsRead > 0)
				return false;
			if ((command.CommandBehavior & CommandBehavior.SchemaOnly) != 0)
				return false;
			if (!moreResults)
				return false;

			bool result = command.Tds.NextRow ();
			command.Connection.CheckForErrors ();

			rowsRead += 1;

			return result;
		}

		#endregion // Methods
	}
}
