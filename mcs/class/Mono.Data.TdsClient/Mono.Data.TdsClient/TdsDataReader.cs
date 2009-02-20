//
// Mono.Data.TdsClient.TdsDataReader.cs
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

using Mono.Data.TdsTypes;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient {
	public sealed class TdsDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		TdsCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		int fieldCount;
		bool isClosed;
		bool isSelect;
		bool moreResults;
		int resultsRead;
		int rowsRead;
		DataTable schemaTable;

		#endregion // Fields

		#region Constructors

		internal TdsDataReader (TdsCommand command)
		{
			this.command = command;
			schemaTable = ConstructSchemaTable ();
			resultsRead = 0;
			fieldCount = 0;
			isClosed = false;
			isSelect = (command.CommandText.Trim ().ToUpper ().StartsWith ("SELECT"));
			command.Tds.RecordsAffected = 0;
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
			get { 
				if (isSelect) 
					return -1;
				else
					return command.Tds.RecordsAffected; 
			}
		}

		#endregion // Properties

		#region Methods

		public void Close ()
		{
			isClosed = true;
			command.CloseDataReader (moreResults);
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = typeof (bool);
			Type stringType = typeof (string);
			Type intType = typeof (int);
			Type typeType = typeof (Type);
			Type shortType = typeof (short);

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

		private void Dispose (bool disposing) 
		{
			if (!disposed) {
				if (disposing) {
					schemaTable.Dispose ();
					Close ();
				}
				disposed = true;
			}
		}

		public bool GetBoolean (int i)
		{
			object value = GetValue (i);
			if (!(value is bool)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (bool) value;
		}

		public byte GetByte (int i)
		{
			object value = GetValue (i);
			if (!(value is byte)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (byte) value;
		}

		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is byte [])) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((byte []) value).Length - dataIndex;
		}

		public char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (char) value;
		}

		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is char[])) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((char []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((char []) value).Length - dataIndex;
		}

		[MonoTODO ("Implement GetData")]
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
			if (!(value is DateTime)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (DateTime) value;
		}

		public decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (decimal) value;
		}

		public double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (double) value;
		}

		public Type GetFieldType (int i)
		{
			return (Type) schemaTable.Rows[i]["DataType"];
		}

		public float GetFloat (int i)
		{
			object value = GetValue (i);
			if (!(value is float)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (float) value;
		}

		public Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (Guid) value;
		}

		public short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (short) value;
		}

		public int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (int) value;
		}

		public long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (long) value;
		}

		public string GetName (int i)
		{
			return (string) schemaTable.Rows[i]["ColumnName"];
		}

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

			foreach (TdsDataColumn schema in command.Tds.Columns) {
				DataRow row = schemaTable.NewRow ();

#if NET_2_0
				row ["ColumnName"]		= GetSchemaValue (schema.ColumnName);
				row ["ColumnSize"]		= GetSchemaValue (schema.ColumnSize); 
				row ["ColumnOrdinal"]		= GetSchemaValue (schema.ColumnOrdinal);
				row ["NumericPrecision"]	= GetSchemaValue (schema.NumericPrecision);
				row ["NumericScale"]		= GetSchemaValue (schema.NumericScale);
				row ["IsUnique"]		= GetSchemaValue (schema.IsUnique);
				row ["IsKey"]			= GetSchemaValue (schema.IsKey);
				row ["IsAliased"]		= GetSchemaValue (schema.IsAliased);
				row ["IsExpression"]		= GetSchemaValue (schema.IsExpression);
				row ["IsIdentity"]		= GetSchemaValue (schema.IsIdentity);
				row ["IsAutoIncrement"]		= GetSchemaValue (schema.IsAutoIncrement);
				row ["IsRowVersion"]		= GetSchemaValue (schema.IsRowVersion);
				row ["IsHidden"]		= GetSchemaValue (schema.IsHidden);
				row ["IsReadOnly"]		= GetSchemaValue (schema.IsReadOnly);
				row ["BaseServerName"]		= GetSchemaValue (schema.BaseServerName);
				row ["BaseCatalogName"]		= GetSchemaValue (schema.BaseCatalogName);
				row ["BaseColumnName"]		= GetSchemaValue (schema.BaseColumnName);
				row ["BaseSchemaName"]		= GetSchemaValue (schema.BaseSchemaName);
				row ["BaseTableName"]		= GetSchemaValue (schema.BaseTableName);
				row ["AllowDBNull"]		= GetSchemaValue (schema.AllowDBNull);
#else
				row ["ColumnName"]		= GetSchemaValue (schema, "ColumnName");
				row ["ColumnSize"]		= GetSchemaValue (schema, "ColumnSize"); 
				row ["ColumnOrdinal"]		= GetSchemaValue (schema, "ColumnOrdinal");
				row ["NumericPrecision"]	= GetSchemaValue (schema, "NumericPrecision");
				row ["NumericScale"]		= GetSchemaValue (schema, "NumericScale");
				row ["IsUnique"]		= GetSchemaValue (schema, "IsUnique");
				row ["IsKey"]			= GetSchemaValue (schema, "IsKey");
				row ["IsAliased"]		= GetSchemaValue (schema, "IsAliased");
				row ["IsExpression"]		= GetSchemaValue (schema, "IsExpression");
				row ["IsIdentity"]		= GetSchemaValue (schema, "IsIdentity");
				row ["IsAutoIncrement"]		= GetSchemaValue (schema, "IsAutoIncrement");
				row ["IsRowVersion"]		= GetSchemaValue (schema, "IsRowVersion");
				row ["IsHidden"]		= GetSchemaValue (schema, "IsHidden");
				row ["IsReadOnly"]		= GetSchemaValue (schema, "IsReadOnly");
				row ["BaseServerName"]		= GetSchemaValue (schema, "BaseServerName");
				row ["BaseCatalogName"]		= GetSchemaValue (schema, "BaseCatalogName");
				row ["BaseColumnName"]		= GetSchemaValue (schema, "BaseColumnName");
				row ["BaseSchemaName"]		= GetSchemaValue (schema, "BaseSchemaName");
				row ["BaseTableName"]		= GetSchemaValue (schema, "BaseTableName");
				row ["AllowDBNull"]		= GetSchemaValue (schema, "AllowDBNull");
#endif
				
				// We don't always get the base column name.
				if (row ["BaseColumnName"] == DBNull.Value)
					row ["BaseColumnName"] = row ["ColumnName"];

#if NET_2_0
				switch ((TdsColumnType) schema.ColumnType) {
#else
				switch ((TdsColumnType) schema ["ColumnType"]) {
#endif
					case TdsColumnType.Image :
						dataTypeNames.Add ("image");
						row ["ProviderType"] = (int) TdsType.Image;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.Text :
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
						row ["ProviderType"] = (int) TdsType.NChar;
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

		private static object GetSchemaValue (TdsDataColumn schema, string key)
		{
			object ret = schema [key];

			if (ret == null)
				return DBNull.Value;

			return ret;
		}

		static object GetSchemaValue (object value)
		{
			if (value == null)
				return DBNull.Value;

			return value;
		}
		
		public TdsBinary GetTdsBinary (int i)
		{
			throw new NotImplementedException ();
		}

		public TdsBoolean GetTdsBoolean (int i) 
		{
			object value = GetTdsValue (i);
			if (!(value is TdsBoolean))
				throw new InvalidCastException ();
			return (TdsBoolean) value;
		}

		public TdsByte GetTdsByte (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsByte))
				throw new InvalidCastException ();
			return (TdsByte) value;
		}

		public TdsDateTime GetTdsDateTime (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsDateTime))
				throw new InvalidCastException ();
			return (TdsDateTime) value;
		}

		public TdsDecimal GetTdsDecimal (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsDecimal))
				throw new InvalidCastException ();
			return (TdsDecimal) value;
		}

		public TdsDouble GetTdsDouble (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsDouble))
				throw new InvalidCastException ();
			return (TdsDouble) value;
		}

		public TdsGuid GetTdsGuid (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsGuid))
				throw new InvalidCastException ();
			return (TdsGuid) value;
		}

		public TdsInt16 GetTdsInt16 (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsInt16))
				throw new InvalidCastException ();
			return (TdsInt16) value;
		}

		public TdsInt32 GetTdsInt32 (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsInt32))
				throw new InvalidCastException ();
			return (TdsInt32) value;
		}

		public TdsInt64 GetTdsInt64 (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsInt64))
				throw new InvalidCastException ();
			return (TdsInt64) value;
		}

		public TdsMoney GetTdsMoney (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsMoney))
				throw new InvalidCastException ();
			return (TdsMoney) value;
		}

		public TdsSingle GetTdsSingle (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsSingle))
				throw new InvalidCastException ();
			return (TdsSingle) value;
		}

		public TdsString GetTdsString (int i)
		{
			object value = GetTdsValue (i);
			if (!(value is TdsString))
				throw new InvalidCastException ();
			return (TdsString) value;
		}

		[MonoTODO ("Implement TdsBigDecimal conversion.  TdsType.Real fails tests?")]
		public object GetTdsValue (int i)
		{
			TdsType type = (TdsType) (schemaTable.Rows [i]["ProviderType"]);
			object value = GetValue (i);

			switch (type) {
			case TdsType.BigInt:
				if (value == DBNull.Value)
					return TdsInt64.Null;
				return (TdsInt64) ((long) value);
			case TdsType.Binary:
			case TdsType.Image:
			case TdsType.VarBinary:
			case TdsType.Timestamp:
				if (value == DBNull.Value)
					return TdsBinary.Null;
				return (TdsBinary) ((byte[]) value);
			case TdsType.Bit:
				if (value == DBNull.Value)
					return TdsBoolean.Null;
				return (TdsBoolean) ((bool) value);
			case TdsType.Char:
			case TdsType.NChar:
			case TdsType.NText:
			case TdsType.NVarChar:
			case TdsType.Text:
			case TdsType.VarChar:
				if (value == DBNull.Value)
					return TdsString.Null;
				return (TdsString) ((string) value);
			case TdsType.DateTime:
			case TdsType.SmallDateTime:
				if (value == DBNull.Value)
					return TdsDateTime.Null;
				return (TdsDateTime) ((DateTime) value);
			case TdsType.Decimal:
				if (value == DBNull.Value)
					return TdsDecimal.Null;
				if (value is TdsBigDecimal)
					return TdsDecimal.FromTdsBigDecimal ((TdsBigDecimal) value);
				return (TdsDecimal) ((decimal) value);
			case TdsType.Float:
				if (value == DBNull.Value)
					return TdsDouble.Null;
				return (TdsDouble) ((double) value);
			case TdsType.Int:
				if (value == DBNull.Value)
					return TdsInt32.Null;
				return (TdsInt32) ((int) value);
			case TdsType.Money:
			case TdsType.SmallMoney:
				if (value == DBNull.Value)
					return TdsMoney.Null;
				return (TdsMoney) ((decimal) value);
			case TdsType.Real:
				if (value == DBNull.Value)
					return TdsSingle.Null;
				return (TdsSingle) ((float) value);
			case TdsType.UniqueIdentifier:
				if (value == DBNull.Value)
					return TdsGuid.Null;
				return (TdsGuid) ((Guid) value);
			case TdsType.SmallInt:
				if (value == DBNull.Value)
					return TdsInt16.Null;
				return (TdsInt16) ((short) value);
			case TdsType.TinyInt:
				if (value == DBNull.Value)
					return TdsByte.Null;
				return (TdsByte) ((byte) value);
			}

			throw new InvalidOperationException ("The type of this column is unknown.");
		}

		public int GetTdsValues (object[] values)
		{
			int count = 0;
			int columnCount = schemaTable.Rows.Count;
			int arrayCount = values.Length;

			if (arrayCount > columnCount)
				count = columnCount;
			else
				count = arrayCount;

			for (int i = 0; i < count; i += 1) 
				values [i] = GetTdsValue (i);

			return count;
		}

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string)) {
				if (value is DBNull) throw new TdsNullValueException ();
				throw new InvalidCastException ();
			}
			return (string) value;
		}

		public object GetValue (int i)
		{
			return command.Tds.ColumnValues [i];
		}

		public int GetValues (object[] values)
		{
			int len = values.Length;
			int bigDecimalIndex = command.Tds.ColumnValues.BigDecimalIndex;

			// If a four-byte decimal is stored, then we can't convert to
			// a native type.  Throw an OverflowException.
			if (bigDecimalIndex >= 0 && bigDecimalIndex < len)
				throw new OverflowException ();

			command.Tds.ColumnValues.CopyTo (0, values, 0, len);
			return (len > FieldCount ? len : FieldCount);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int i)
		{
			return GetValue (i) == DBNull.Value;
		}

		public bool NextResult ()
		{
			if ((command.CommandBehavior & CommandBehavior.SingleResult) != 0 && resultsRead > 0)
				return false;
			if (command.Tds.DoneProc)
				return false;

			schemaTable.Rows.Clear ();

			moreResults = command.Tds.NextResult ();
			GetSchemaTable ();

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

			rowsRead += 1;

			return result;
		}

		#endregion // Methods
	}
}
