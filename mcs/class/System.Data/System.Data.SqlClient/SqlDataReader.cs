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

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;

namespace System.Data.SqlClient {
	public sealed class SqlDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		SqlCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		int fieldCount;
		bool isClosed;
		bool isSelect;
		bool moreResults;
		int resultsRead;
		int rowsRead;
		DataTable schemaTable;
		bool hasRows;
		bool haveRead;
		bool readResult;
		bool readResultUsed;

		#endregion // Fields

		#region Constructors

		internal SqlDataReader (SqlCommand command)
		{
			readResult = false;
			haveRead = false;
			readResultUsed = false;
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

		public bool HasRows {
			get {
				if (haveRead) 
					return readResult;
			
				haveRead = true;
				readResult = ReadRecord ();
				return readResult;						
			}
		}
	
		#endregion // Properties

		#region Methods

		public void Close ()
		{
			// skip to end & read output parameters.
			while (NextResult ())
				;
			isClosed = true;
			command.Connection.DataReader = null;
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

		private void Dispose (bool disposing) 
		{
			if (!disposed) {
				if (disposing) {
					schemaTable.Dispose ();
					Close ();
					command = null;
				}
				disposed = true;
			}
		}

		public bool GetBoolean (int i)
		{
			object value = GetValue (i);
			if (!(value is bool)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (bool) value;
		}

		public byte GetByte (int i)
		{
			object value = GetValue (i);
			if (!(value is byte)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (byte) value;
		}

		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is byte [])) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			
			if ( buffer == null ) {
				// Return length of data
				return ((byte []) value).Length;
			}
			else {
				// Copy data into buffer
				Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
				return ((byte []) value).Length - dataIndex;
			}
		}
                                                                                                    
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (char) value;
		}

		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			char [] valueBuffer;
			
			if (value is char[])
				valueBuffer = (char[])value;
			else if (value is string)
				valueBuffer = ((string)value).ToCharArray();
			else {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			
			if ( buffer == null ) {
				// Return length of data
				return valueBuffer.Length;
			}
			else {
				// Copy data into buffer
				Array.Copy (valueBuffer, (int) dataIndex, buffer, bufferIndex, length);
				return valueBuffer.Length - dataIndex;
			}
		}
		
		[EditorBrowsableAttribute (EditorBrowsableState.Never)] 
		public IDataReader GetData (int i)
		{
			return ( (IDataReader) this [i]);
		}

		public string GetDataTypeName (int i)
		{
			return (string) dataTypeNames [i];
		}

		public DateTime GetDateTime (int i)
		{
			object value = GetValue (i);
			if (!(value is DateTime)) {
				if (value is DBNull) throw new SqlNullValueException ();
				else throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (DateTime) value;
		}

		public decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (decimal) value;
		}

		public double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
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
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (float) value;
		}

		public Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (Guid) value;
		}

		public short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (short) value;
		}

		public int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (int) value;
		}

		public long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
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

				// We don't always get the base column name.
				if (row ["BaseColumnName"] == DBNull.Value)
					row ["BaseColumnName"] = row ["ColumnName"];

				switch ((TdsColumnType) schema ["ColumnType"]) {
					case TdsColumnType.Int1:
					case TdsColumnType.Int2:
					case TdsColumnType.Int4:
					case TdsColumnType.IntN:
						switch ((int) schema ["ColumnSize"]) {
						case 1:
							dataTypeNames.Add ("tinyint");
							row ["ProviderType"] = (int) SqlDbType.TinyInt;
							row ["DataType"] = typeof (byte);
							row ["IsLong"] = false;
							break;
						case 2:
							dataTypeNames.Add ("smallint");
							row ["ProviderType"] = (int) SqlDbType.SmallInt;
							row ["DataType"] = typeof (short);
							row ["IsLong"] = false;
							break;
						case 4:
							dataTypeNames.Add ("int");
							row ["ProviderType"] = (int) SqlDbType.Int;
							row ["DataType"] = typeof (int);
							row ["IsLong"] = false;
							break;
						case 8:
							dataTypeNames.Add ("bigint");
							row ["ProviderType"] = (int) SqlDbType.BigInt;
							row ["DataType"] = typeof (long);
							row ["IsLong"] = false;
							break;
						}
						break;
					case TdsColumnType.Real:
					case TdsColumnType.Float8:
					case TdsColumnType.FloatN:
						switch ((int) schema ["ColumnSize"]) {
						case 4:
							dataTypeNames.Add ("real");
							row ["ProviderType"] = (int) SqlDbType.Real;
							row ["DataType"] = typeof (float);
							row ["IsLong"] = false;
							break;
						case 8:
							dataTypeNames.Add ("float");
							row ["ProviderType"] = (int) SqlDbType.Float;
							row ["DataType"] = typeof (double);
							row ["IsLong"] = false;
							break;
						}
						break;
					case TdsColumnType.Image :
						dataTypeNames.Add ("image");
						row ["ProviderType"] = (int) SqlDbType.Image;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.Text :
						dataTypeNames.Add ("text");
						row ["ProviderType"] = (int) SqlDbType.Text;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.UniqueIdentifier :
						dataTypeNames.Add ("uniqueidentifier");
						row ["ProviderType"] = (int) SqlDbType.UniqueIdentifier;
						row ["DataType"] = typeof (Guid);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.VarBinary :
					case TdsColumnType.BigVarBinary :
						dataTypeNames.Add ("varbinary");
						row ["ProviderType"] = (int) SqlDbType.VarBinary;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.VarChar :
					case TdsColumnType.BigVarChar :
						dataTypeNames.Add ("varchar");
						row ["ProviderType"] = (int) SqlDbType.VarChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Binary :
					case TdsColumnType.BigBinary :
						dataTypeNames.Add ("binary");
						row ["ProviderType"] = (int) SqlDbType.Binary;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.Char :
					case TdsColumnType.BigChar :
						dataTypeNames.Add ("char");
						row ["ProviderType"] = (int) SqlDbType.Char;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Bit :
					case TdsColumnType.BitN :
						dataTypeNames.Add ("bit");
						row ["ProviderType"] = (int) SqlDbType.Bit;
						row ["DataType"] = typeof (bool);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.DateTime4 :
					case TdsColumnType.DateTime :
					case TdsColumnType.DateTimeN :
						dataTypeNames.Add ("datetime");
						row ["ProviderType"] = (int) SqlDbType.DateTime;
						row ["DataType"] = typeof (DateTime);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Money :
					case TdsColumnType.MoneyN :
					case TdsColumnType.Money4 :
						dataTypeNames.Add ("money");
						row ["ProviderType"] = (int) SqlDbType.Money;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.NText :
						dataTypeNames.Add ("ntext");
						row ["ProviderType"] = (int) SqlDbType.NText;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.NVarChar :
						dataTypeNames.Add ("nvarchar");
						row ["ProviderType"] = (int) SqlDbType.NVarChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Decimal :
					case TdsColumnType.Numeric :
						dataTypeNames.Add ("decimal");
						row ["ProviderType"] = (int) SqlDbType.Decimal;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.NChar :
						dataTypeNames.Add ("nchar");
						row ["ProviderType"] = (int) SqlDbType.NChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.SmallMoney :
						dataTypeNames.Add ("smallmoney");
						row ["ProviderType"] = (int) SqlDbType.SmallMoney;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					default :
						dataTypeNames.Add ("variant");
						row ["ProviderType"] = (int) SqlDbType.Variant;
						row ["DataType"] = typeof (object);
						row ["IsLong"] = false;
						break;
				}

				schemaTable.Rows.Add (row);

				fieldCount += 1;
			}
			return schemaTable;
		}		

		private static object GetSchemaValue (TdsDataColumn schema, object key)
		{
			if (schema.ContainsKey (key) && schema [key] != null)
				return schema [key];
			return DBNull.Value;
		}

		public SqlBinary GetSqlBinary (int i)
		{
			throw new NotImplementedException ();
		}

		public SqlBoolean GetSqlBoolean (int i) 
		{
			object value = GetSqlValue (i);
			if (!(value is SqlBoolean))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlBoolean) value;
		}

		public SqlByte GetSqlByte (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlByte))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlByte) value;
		}

		public SqlDateTime GetSqlDateTime (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDateTime))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDateTime) value;
		}

		public SqlDecimal GetSqlDecimal (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDecimal))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDecimal) value;
		}

		public SqlDouble GetSqlDouble (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDouble))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDouble) value;
		}

		public SqlGuid GetSqlGuid (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlGuid))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlGuid) value;
		}

		public SqlInt16 GetSqlInt16 (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlInt16))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt16) value;
		}

		public SqlInt32 GetSqlInt32 (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlInt32))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt32) value;
		}

		public SqlInt64 GetSqlInt64 (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlInt64))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt64) value;
		}

		public SqlMoney GetSqlMoney (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlMoney))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlMoney) value;
		}

		public SqlSingle GetSqlSingle (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlSingle))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlSingle) value;
		}

		public SqlString GetSqlString (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlString))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlString) value;
		}

		public object GetSqlValue (int i)
		{
			SqlDbType type = (SqlDbType) (schemaTable.Rows [i]["ProviderType"]);
			object value = GetValue (i);

			switch (type) {
			case SqlDbType.BigInt:
				if (value == DBNull.Value)
					return SqlInt64.Null;
				return (SqlInt64) ((long) value);
			case SqlDbType.Binary:
			case SqlDbType.Image:
			case SqlDbType.VarBinary:
			case SqlDbType.Timestamp:
				if (value == DBNull.Value)
					return SqlBinary.Null;
				return (SqlBinary) ((byte[]) value);
			case SqlDbType.Bit:
				if (value == DBNull.Value)
					return SqlBoolean.Null;
				return (SqlBoolean) ((bool) value);
			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.NText:
			case SqlDbType.NVarChar:
			case SqlDbType.Text:
			case SqlDbType.VarChar:
				if (value == DBNull.Value)
					return SqlString.Null;
				return (SqlString) ((string) value);
			case SqlDbType.DateTime:
			case SqlDbType.SmallDateTime:
				if (value == DBNull.Value)
					return SqlDateTime.Null;
				return (SqlDateTime) ((DateTime) value);
			case SqlDbType.Decimal:
				if (value == DBNull.Value)
					return SqlDecimal.Null;
				if (value is TdsBigDecimal)
					return SqlDecimal.FromTdsBigDecimal ((TdsBigDecimal) value);
				return (SqlDecimal) ((decimal) value);
			case SqlDbType.Float:
				if (value == DBNull.Value)
					return SqlDouble.Null;
				return (SqlDouble) ((double) value);
			case SqlDbType.Int:
				if (value == DBNull.Value)
					return SqlInt32.Null;
				return (SqlInt32) ((int) value);
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				if (value == DBNull.Value)
					return SqlMoney.Null;
				return (SqlMoney) ((decimal) value);
			case SqlDbType.Real:
				if (value == DBNull.Value)
					return SqlSingle.Null;
				return (SqlSingle) ((float) value);
			case SqlDbType.UniqueIdentifier:
				if (value == DBNull.Value)
					return SqlGuid.Null;
				return (SqlGuid) ((Guid) value);
			case SqlDbType.SmallInt:
				if (value == DBNull.Value)
					return SqlInt16.Null;
				return (SqlInt16) ((short) value);
			case SqlDbType.TinyInt:
				if (value == DBNull.Value)
					return SqlByte.Null;
				return (SqlByte) ((byte) value);
			}

			throw new InvalidOperationException ("The type of this column is unknown.");
		}

		public int GetSqlValues (object[] values)
		{
			int count = 0;
			int columnCount = schemaTable.Rows.Count;
			int arrayCount = values.Length;

			if (arrayCount > columnCount)
				count = columnCount;
			else
				count = arrayCount;

			for (int i = 0; i < count; i += 1) 
				values [i] = GetSqlValue (i);

			return count;
		}

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
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

			schemaTable.Rows.Clear ();

			moreResults = command.Tds.NextResult ();
			if (!moreResults)
				command.GetOutputParameters ();

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
	
			if ((haveRead) && (!readResultUsed))
			{
				readResultUsed = true;
				return true;
			}
			
			
			return (ReadRecord ());

		}

		internal bool ReadRecord ()
		{
			
			bool result = command.Tds.NextRow ();
			
			rowsRead += 1;
			
			return result;
		}
				
		

		#endregion // Methods
	}
}
