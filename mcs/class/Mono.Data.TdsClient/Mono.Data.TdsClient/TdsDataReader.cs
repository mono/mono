//
// Mono.Data.TdsClient.TdsDataReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient {
        public class TdsDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		int fieldCount;
		bool hasRows;
		bool isClosed;
		int recordsAffected;
		bool moreResults;

		ArrayList dataTypeNames;
		ArrayList dataTypes;

		TdsCommand command;

		DataTable schemaTable = ConstructSchemaTable ();

		#endregion // Fields

		#region Constructors

		internal TdsDataReader (TdsCommand command)
		{
			this.command = command;
			this.fieldCount = 0;
			this.isClosed = false;
			NextResult ();
		}

		#endregion // Constructors

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

		[MonoTODO]
		public void Close ()
		{
			isClosed = true;

			throw new NotImplementedException (); 
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = Type.GetType ("System.Boolean");
			Type stringType = Type.GetType ("System.String");
			Type intType = Type.GetType ("System.Int32");
			Type typeType = Type.GetType ("System.Type");

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", intType);
			schemaTable.Columns.Add ("NumericScale", intType);
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

			if (!moreResults)
				return null;

			fieldCount = 0;

			dataTypeNames = new ArrayList ();
			dataTypes = new ArrayList ();

			foreach (TdsSchemaInfo schema in command.Tds.Schema) {
				DataRow row = schemaTable.NewRow ();

				row ["ColumnName"]		= GetSchemaValue (schema, "ColumnName");
				row ["ColumnSize"]		= GetSchemaValue (schema, "ColumnSize");
				row ["ColumnOrdinal"]		= GetSchemaValue (schema, "ColumnOrdinal");
				row ["NumericPrecision"]	= GetSchemaValue (schema, "NumericPrecision");
				row ["NumericScale"]		= GetSchemaValue (schema, "NumericScale");
				row ["IsUnique"]		= GetSchemaValue (schema, "IsUnique");
				row ["IsKey"]			= GetSchemaValue (schema, "IsKey");
				row ["BaseServerName"]		= GetSchemaValue (schema, "BaseServerName");
				row ["BaseCatalogName"]		= GetSchemaValue (schema, "BaseCatalogName");
				row ["BaseColumnName"]		= GetSchemaValue (schema, "BaseColumnName");
				row ["BaseSchemaName"]		= GetSchemaValue (schema, "BaseSchemaName");
				row ["BaseTableName"]		= GetSchemaValue (schema, "BaseTableName");
				row ["AllowDBNull"]		= GetSchemaValue (schema, "AllowDBNull");
				row ["IsAliased"]		= GetSchemaValue (schema, "IsAliased");
				row ["IsExpression"]		= GetSchemaValue (schema, "IsExpression");
				row ["IsIdentity"]		= GetSchemaValue (schema, "IsIdentity");
				row ["IsAutoIncrement"]		= GetSchemaValue (schema, "IsAutoIncrement");
				row ["IsRowVersion"]		= GetSchemaValue (schema, "IsRowVersion");
				row ["IsHidden"]		= GetSchemaValue (schema, "IsHidden");
				row ["IsReadOnly"]		= GetSchemaValue (schema, "IsReadOnly");

				switch ((TdsColumnType) schema ["ColumnType"]) {
				case TdsColumnType.Image :
					dataTypeNames.Add ("image");
					row ["ProviderType"] = (int) TdsType.Image;
					row ["DataType"] = typeof (byte[]);
					row ["IsLong"] = true;
					break;
				case TdsColumnType.Text :
					dataTypes.Add (typeof (string));
					dataTypeNames.Add ("text");
					row ["ProviderType"] = (int) TdsType.Text;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = true;
					break;
				case TdsColumnType.UniqueIdentifier :
					dataTypeNames.Add ("uniqueidentifier");
					row ["ProviderType"] = (int) TdsType.UniqueIdentifier;
					row ["DataType"] = typeof (Guid);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.VarBinary :
				case TdsColumnType.BigVarBinary :
					dataTypeNames.Add ("varbinary");
					row ["ProviderType"] = (int) TdsType.VarBinary;
					row ["DataType"] = typeof (byte[]);
					row ["IsLong"] = true;
					break;
				case TdsColumnType.IntN :
				case TdsColumnType.Int4 :
					dataTypeNames.Add ("int");
					row ["ProviderType"] = (int) TdsType.Int;
					row ["DataType"] = typeof (int);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.VarChar :
				case TdsColumnType.BigVarChar :
					dataTypeNames.Add ("varchar");
					row ["ProviderType"] = (int) TdsType.VarChar;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Binary :
				case TdsColumnType.BigBinary :
					dataTypeNames.Add ("binary");
					row ["ProviderType"] = (int) TdsType.Binary;
					row ["DataType"] = typeof (byte[]);
					row ["IsLong"] = true;
					break;
				case TdsColumnType.Char :
				case TdsColumnType.BigChar :
					dataTypeNames.Add ("char");
					row ["ProviderType"] = (int) TdsType.Char;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Int1 :
					dataTypeNames.Add ("tinyint");
					row ["ProviderType"] = (int) TdsType.TinyInt;
					row ["DataType"] = typeof (byte);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Bit :
				case TdsColumnType.BitN :
					dataTypeNames.Add ("bit");
					row ["ProviderType"] = (int) TdsType.Bit;
					row ["DataType"] = typeof (bool);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Int2 :
					dataTypeNames.Add ("smallint");
					row ["ProviderType"] = (int) TdsType.SmallInt;
					row ["DataType"] = typeof (short);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.DateTime4 :
				case TdsColumnType.DateTime :
				case TdsColumnType.DateTimeN :
					dataTypeNames.Add ("datetime");
					row ["ProviderType"] = (int) TdsType.DateTime;
					row ["DataType"] = typeof (DateTime);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Real :
					dataTypeNames.Add ("real");
					row ["ProviderType"] = (int) TdsType.Real;
					row ["DataType"] = typeof (float);
					break;
				case TdsColumnType.Money :
				case TdsColumnType.MoneyN :
				case TdsColumnType.Money4 :
					dataTypeNames.Add ("money");
					row ["ProviderType"] = (int) TdsType.Money;
					row ["DataType"] = typeof (decimal);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Float8 :
				case TdsColumnType.FloatN :
					dataTypeNames.Add ("float");
					row ["ProviderType"] = (int) TdsType.Float;
					row ["DataType"] = typeof (double);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.NText :
					dataTypeNames.Add ("ntext");
					row ["ProviderType"] = (int) TdsType.NText;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = true;
					break;
				case TdsColumnType.NVarChar :
					dataTypeNames.Add ("nvarchar");
					row ["ProviderType"] = (int) TdsType.NVarChar;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.Decimal :
				case TdsColumnType.Numeric :
					dataTypeNames.Add ("decimal");
					row ["ProviderType"] = (int) TdsType.Decimal;
					row ["DataType"] = typeof (decimal);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.NChar :
					dataTypeNames.Add ("nchar");
					row ["ProviderType"] = (int) TdsType.Char;
					row ["DataType"] = typeof (string);
					row ["IsLong"] = false;
					break;
				case TdsColumnType.SmallMoney :
					dataTypeNames.Add ("smallmoney");
					row ["ProviderType"] = (int) TdsType.SmallMoney;
					row ["DataType"] = typeof (decimal);
					row ["IsLong"] = false;
					break;
				default :
					dataTypeNames.Add ("variant");
					row ["ProviderType"] = (int) TdsType.Variant;
					row ["DataType"] = typeof (object);
					row ["IsLong"] = false;
					break;
				}

				schemaTable.Rows.Add (row);

				fieldCount += 1;
			}
			return schemaTable;
		}

		private static object GetSchemaValue (TdsSchemaInfo schema, object key)
		{
			if (schema.ContainsKey (key) && schema [key] != null)
				return schema [key];
			return DBNull.Value;
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
			return command.Tds.NextResult ();
		}

		public bool Read ()
		{
			return command.Tds.NextRow ();
		}

                #endregion // Methods
	}
}
