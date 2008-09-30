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
using System.IO;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Xml;

namespace System.Data.SqlClient
{
#if NET_2_0
	public class SqlDataReader : DbDataReader, IDataReader, IDisposable, IDataRecord
#else
	public sealed class SqlDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
#endif // NET_2_0
	{
		#region Fields

		SqlCommand command;
		ArrayList dataTypeNames;
		bool disposed;
		bool isClosed;
		bool moreResults;
		int resultsRead;
		int rowsRead;
		DataTable schemaTable;
		bool haveRead;
		bool readResult;
		bool readResultUsed;
#if NET_2_0
		int visibleFieldCount;
#endif

		#endregion // Fields

		const int COLUMN_NAME_IDX = 0;
		const int COLUMN_ORDINAL_IDX = 1;
		const int COLUMN_SIZE_IDX = 2;
		const int NUMERIC_PRECISION_IDX = 3;
		const int NUMERIC_SCALE_IDX = 4;
		const int IS_UNIQUE_IDX = 5;
		const int IS_KEY_IDX = 6;
		const int BASE_SERVER_NAME_IDX = 7;
		const int BASE_CATALOG_NAME_IDX = 8;
		const int BASE_COLUMN_NAME_IDX = 9;
		const int BASE_SCHEMA_NAME_IDX = 10;
		const int BASE_TABLE_NAME_IDX = 11;
		const int DATA_TYPE_IDX = 12;
		const int ALLOW_DBNULL_IDX = 13;
		const int PROVIDER_TYPE_IDX = 14;
		const int IS_ALIASED_IDX = 15;
		const int IS_EXPRESSION_IDX = 16;
		const int IS_IDENTITY_IDX = 17;
		const int IS_AUTO_INCREMENT_IDX = 18;
		const int IS_ROW_VERSION_IDX = 19;
		const int IS_HIDDEN_IDX = 20;
		const int IS_LONG_IDX = 21;
		const int IS_READ_ONLY_IDX = 22;

		#region Constructors

		internal SqlDataReader (SqlCommand command)
		{
			readResult = false;
			haveRead = false;
			readResultUsed = false;
			this.command = command;
			resultsRead = 0;
			isClosed = false;
			command.Tds.RecordsAffected = -1;
#if NET_2_0
			visibleFieldCount = 0;
#endif
			NextResult ();
		}

		#endregion // Constructors

		#region Properties

		public
#if NET_2_0
		override
#endif // NET_2_0
		int Depth {
			get { return 0; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int FieldCount {
			get { return command.Tds.Columns.Count; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool IsClosed {
			get { return isClosed; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object this [int i] {
			get { return GetValue (i); }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}
	
		public
#if NET_2_0
		override
#endif // NET_2_0
		int RecordsAffected {
			get {
				return command.Tds.RecordsAffected; 
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool HasRows {
			get {
				if (haveRead) 
					return readResult;
			
				haveRead = true;
				readResult = ReadRecord ();
				return readResult;
			}
		}
#if NET_2_0
		public override int VisibleFieldCount {
			get { return visibleFieldCount; }
		}

		protected SqlConnection Connection {
			get { return command.Connection; }
		}

		protected bool IsCommandBehavior (CommandBehavior condition) {
			return condition == command.CommandBehavior;
		}
#endif

		#endregion // Properties

		#region Methods

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Close ()
		{
			if (IsClosed)
				return;
			// skip to end & read output parameters.
			while (NextResult ())
				;
			isClosed = true;
			command.CloseDataReader ();
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
		
		private void GetSchemaRowTypeName (TdsColumnType ctype, int csize, out string typeName) 
		{
			int dbType;
			bool isLong;
			Type fieldType;
			
			GetSchemaRowType (ctype, csize, out dbType, out fieldType, out isLong, out typeName);
		}

		private void GetSchemaRowFieldType (TdsColumnType ctype, int csize, out Type fieldType) 
		{
			int dbType;
			bool isLong;
			string typeName;
			
			GetSchemaRowType (ctype, csize, out dbType, out fieldType, out isLong, out typeName);
		}

		private void GetSchemaRowDbType (TdsColumnType ctype, int csize, out int dbType) 
		{
			Type fieldType;
			bool isLong;
			string typeName;
			
			GetSchemaRowType (ctype, csize, out dbType, out fieldType, out isLong, out typeName);
		}
		
		private void GetSchemaRowType (TdsColumnType ctype, int csize, 
		                               out int dbType, out Type fieldType, 
		                               out bool isLong, out string typeName)
		{
			dbType = -1;
			typeName = string.Empty;
			isLong = false;
			fieldType = typeof (Type);
			
			switch (ctype) {
				case TdsColumnType.Int1:
				case TdsColumnType.Int2:
				case TdsColumnType.Int4:
				case TdsColumnType.IntN:
					switch (csize) {
					case 1:
						typeName = "tinyint";
						dbType = (int) SqlDbType.TinyInt;
						fieldType = typeof (byte);
						isLong = false;
						break;
					case 2:
						typeName = "smallint";
						dbType = (int) SqlDbType.SmallInt;
						fieldType = typeof (short);
						isLong = false;
						break;
					case 4:
						typeName = "int";
						dbType = (int) SqlDbType.Int;
						fieldType = typeof (int);
						isLong = false;
						break;
					case 8:
						typeName = "bigint";
						dbType = (int) SqlDbType.BigInt;
						fieldType = typeof (long);
						isLong = false;
						break;
					}
					break;
				case TdsColumnType.Real:
				case TdsColumnType.Float8:
				case TdsColumnType.FloatN:
					switch (csize) {
					case 4:
						typeName = "real";
						dbType = (int) SqlDbType.Real;
						fieldType = typeof (float);
						isLong = false;
						break;
					case 8:
						typeName = "float";
						dbType = (int) SqlDbType.Float;
						fieldType = typeof (double);
						isLong = false;
						break;
					}
					break;
				case TdsColumnType.Image :
					typeName = "image";
					dbType = (int) SqlDbType.Image;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.Text :
					typeName = "text";
					dbType = (int) SqlDbType.Text;
					fieldType = typeof (string);
					isLong = true;
					break;
				case TdsColumnType.UniqueIdentifier :
					typeName = "uniqueidentifier";
					dbType = (int) SqlDbType.UniqueIdentifier;
					fieldType = typeof (Guid);
					isLong = false;
					break;
				case TdsColumnType.VarBinary :
				case TdsColumnType.BigVarBinary :
					typeName = "varbinary";
					dbType = (int) SqlDbType.VarBinary;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.VarChar :
				case TdsColumnType.BigVarChar :
					typeName = "varchar";
					dbType = (int) SqlDbType.VarChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Binary :
				case TdsColumnType.BigBinary :
					typeName = "binary";
					dbType = (int) SqlDbType.Binary;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.Char :
				case TdsColumnType.BigChar :
					typeName = "char";
					dbType = (int) SqlDbType.Char;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Bit :
				case TdsColumnType.BitN :
					typeName = "bit";
					dbType = (int) SqlDbType.Bit;
					fieldType = typeof (bool);
					isLong = false;
					break;
				case TdsColumnType.DateTime4 :
				case TdsColumnType.DateTime :
				case TdsColumnType.DateTimeN :
					typeName = "datetime";
					dbType = (int) SqlDbType.DateTime;
					fieldType = typeof (DateTime);
					isLong = false;
					break;
				case TdsColumnType.Money :
				case TdsColumnType.MoneyN :
				case TdsColumnType.Money4 :
					typeName = "money";
					dbType = (int) SqlDbType.Money;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				case TdsColumnType.NText :
					typeName = "ntext";
					dbType = (int) SqlDbType.NText;
					fieldType = typeof (string);
					isLong = true;
					break;
				case TdsColumnType.NVarChar :
					typeName = "nvarchar";
					dbType = (int) SqlDbType.NVarChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Decimal :
				case TdsColumnType.Numeric :
					typeName = "decimal";
					dbType = (int) SqlDbType.Decimal;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				case TdsColumnType.NChar :
					typeName = "nchar";
					dbType = (int) SqlDbType.NChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.SmallMoney :
					typeName = "smallmoney";
					dbType = (int) SqlDbType.SmallMoney;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				default :
					typeName = "variant";
					dbType = (int) SqlDbType.Variant;
					fieldType = typeof (object);
					isLong = false;
					break;
			}
		}

#if NET_2_0
		new
#endif
		void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (schemaTable != null)
						schemaTable.Dispose ();
					Close ();
					command = null;
				}
				disposed = true;
			}
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		bool GetBoolean (int i)
		{
			object value = GetValue (i);
			if (!(value is bool)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (bool) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		byte GetByte (int i)
		{
			object value = GetValue (i);
			if (!(value is byte)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (byte) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			if ((command.CommandBehavior & CommandBehavior.SequentialAccess) != 0) {
				try {
					long len = ((Tds)command.Tds).GetSequentialColumnValue (i, dataIndex, buffer, bufferIndex, length);
					if (len == -1)
						throw new InvalidCastException ("Invalid attempt to GetBytes on column "
										+ "'" + command.Tds.Columns[i]["ColumnName"] +
										"'." + "The GetBytes function"
										+ " can only be used on columns of type Text, NText, or Image");
					return len;
				} catch (TdsInternalException ex) {
					command.Connection.Close ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				}
			}

			object value = GetValue (i);
			if (!(value is byte [])) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			
			if ( buffer == null )
				return ((byte []) value).Length; // Return length of data

			// Copy data into buffer
			int availLen = (int) ( ( (byte []) value).Length - dataIndex);
			if (availLen < length)
				length = availLen;
			Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
			return length; // return actual read count
		}

		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public
#if NET_2_0
		override
#endif // NET_2_0
		char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (char) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			if ((command.CommandBehavior & CommandBehavior.SequentialAccess) != 0) {
				Encoding encoding = null;
				byte mul = 1;
				TdsColumnType colType = (TdsColumnType) command.Tds.Columns[i]["ColumnType"];
				switch (colType) {
					case TdsColumnType.Text :
					case TdsColumnType.VarChar:
					case TdsColumnType.Char:
					case TdsColumnType.BigVarChar:
						encoding = Encoding.ASCII;
						break;
					case TdsColumnType.NText :
					case TdsColumnType.NVarChar:
					case TdsColumnType.NChar:
						encoding = Encoding.Unicode;
						mul = 2;
						break;
					default :
						return -1;
				}

				long count = 0;
				if (buffer == null) {
					count = GetBytes (i,0,(byte[]) null,0,0);
					return (count/mul);
				}

				length *= mul;
				byte[] arr = new byte [length];
				count = GetBytes (i, dataIndex, arr, 0, length);
				if (count == -1)
					throw new InvalidCastException ("Specified cast is not valid");

				Char[] val = encoding.GetChars (arr, 0, (int)count);
				val.CopyTo (buffer, bufferIndex);
				return val.Length;
			}

			char [] valueBuffer;
			object value = GetValue (i);
			
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
		
#if !NET_2_0
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public new IDataReader GetData (int i)
		{
			return ((IDataReader) this [i]);
		}
#endif

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetDataTypeName (int i)
		{
			TdsColumnType ctype;
			string datatypeName;
			int csize;
			
			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();
#if NET_2_0
			ctype = (TdsColumnType) command.Tds.Columns[i].ColumnType;
			csize = (int) command.Tds.Columns[i].ColumnSize;
#else
			ctype = (TdsColumnType) command.Tds.Columns[i]["ColumnType"];
			csize = (int) command.Tds.Columns[i]["ColumnSize"];
#endif
			GetSchemaRowTypeName (ctype, csize, out datatypeName);
			return datatypeName;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		DateTime GetDateTime (int i)
		{
			object value = GetValue (i);
			if (!(value is DateTime)) {
				if (value is DBNull) throw new SqlNullValueException ();
				else throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (DateTime) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (decimal) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (double) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		Type GetFieldType (int i)
		{
			TdsColumnType ctype;
			Type fieldType;
			int csize;
			
			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();
#if NET_2_0
			ctype = (TdsColumnType) command.Tds.Columns[i].ColumnType;
			csize = (int) command.Tds.Columns[i].ColumnSize;
#else
			ctype = (TdsColumnType) command.Tds.Columns[i]["ColumnType"];
			csize = (int) command.Tds.Columns[i]["ColumnSize"];
#endif
			GetSchemaRowFieldType (ctype, csize, out fieldType);			
			return fieldType;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		float GetFloat (int i)
		{
			object value = GetValue (i);
			if (!(value is float)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (float) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (Guid) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (short) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (int) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (long) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetName (int i)
		{
			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();
#if NET_2_0
			return (string) command.Tds.Columns[i].ColumnName;
#else
			return (string) command.Tds.Columns[i]["ColumnName"];
#endif
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int GetOrdinal (string name)
		{
			string colName;
			foreach (TdsDataColumn schema in command.Tds.Columns) {
#if NET_2_0
				colName = schema.ColumnName;
				if (colName.Equals (name) || String.Compare (colName, name, true) == 0)
					return (int) schema.ColumnOrdinal;
#else
				colName = (string) schema["ColumnName"];
				if (colName.Equals (name) || String.Compare (colName, name, true) == 0)
					return (int) schema["ColumnOrdinal"];
#endif						
			}			
			throw new IndexOutOfRangeException ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		DataTable GetSchemaTable ()
		{
			ValidateState ();

			if (schemaTable == null)
				schemaTable = ConstructSchemaTable ();

			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;

			if (!moreResults)
				return null;

			dataTypeNames = new ArrayList (command.Tds.Columns.Count);

			foreach (TdsDataColumn schema in command.Tds.Columns) {
				DataRow row = schemaTable.NewRow ();

#if NET_2_0
				row [COLUMN_NAME_IDX]		= GetSchemaValue (schema.ColumnName);
				row [COLUMN_ORDINAL_IDX]		= GetSchemaValue (schema.ColumnOrdinal);
				row [IS_UNIQUE_IDX]		= GetSchemaValue (schema.IsUnique);
				row [IS_AUTO_INCREMENT_IDX]		= GetSchemaValue (schema.IsAutoIncrement);
				row [IS_ROW_VERSION_IDX]		= GetSchemaValue (schema.IsRowVersion);
				row [IS_HIDDEN_IDX]		= GetSchemaValue (schema.IsHidden);
				row [IS_IDENTITY_IDX]		= GetSchemaValue (schema.IsIdentity);
				row [COLUMN_SIZE_IDX]		= GetSchemaValue (schema.ColumnSize);
				row [NUMERIC_PRECISION_IDX]	= GetSchemaValue (schema.NumericPrecision);
				row [NUMERIC_SCALE_IDX]		= GetSchemaValue (schema.NumericScale);
				row [IS_KEY_IDX]			= GetSchemaValue (schema.IsKey);
				row [IS_ALIASED_IDX]		= GetSchemaValue (schema.IsAliased);
				row [IS_EXPRESSION_IDX]		= GetSchemaValue (schema.IsExpression);
				row [IS_READ_ONLY_IDX]		= GetSchemaValue (schema.IsReadOnly);
				row [BASE_SERVER_NAME_IDX]		= GetSchemaValue (schema.BaseServerName);
				row [BASE_CATALOG_NAME_IDX]		= GetSchemaValue (schema.BaseCatalogName);
				row [BASE_COLUMN_NAME_IDX]		= GetSchemaValue (schema.BaseColumnName);
				row [BASE_SCHEMA_NAME_IDX]		= GetSchemaValue (schema.BaseSchemaName);
				row [BASE_TABLE_NAME_IDX]		= GetSchemaValue (schema.BaseTableName);
				row [ALLOW_DBNULL_IDX]		= GetSchemaValue (schema.AllowDBNull);
#else
				row ["ColumnName"]		= GetSchemaValue (schema, "ColumnName");
				row ["ColumnOrdinal"]		= GetSchemaValue (schema, "ColumnOrdinal");
				row ["IsUnique"]		= GetSchemaValue (schema, "IsUnique");
				row ["IsAutoIncrement"]		= GetSchemaValue (schema, "IsAutoIncrement");
				row ["IsRowVersion"]		= GetSchemaValue (schema, "IsRowVersion");
				row ["IsHidden"]		= GetSchemaValue (schema, "IsHidden");
				row ["IsIdentity"]		= GetSchemaValue (schema, "IsIdentity");
				row ["ColumnSize"]		= GetSchemaValue (schema, "ColumnSize");
				row ["NumericPrecision"]	= GetSchemaValue (schema, "NumericPrecision");
				row ["NumericScale"]		= GetSchemaValue (schema, "NumericScale");
				row ["IsKey"]			= GetSchemaValue (schema, "IsKey");
				row ["IsAliased"]		= GetSchemaValue (schema, "IsAliased");
				row ["IsExpression"]		= GetSchemaValue (schema, "IsExpression");
				row ["IsReadOnly"]		= GetSchemaValue (schema, "IsReadOnly");
				row ["BaseServerName"]		= GetSchemaValue (schema, "BaseServerName");
				row ["BaseCatalogName"]		= GetSchemaValue (schema, "BaseCatalogName");
				row ["BaseColumnName"]		= GetSchemaValue (schema, "BaseColumnName");
				row ["BaseSchemaName"]		= GetSchemaValue (schema, "BaseSchemaName");
				row ["BaseTableName"]		= GetSchemaValue (schema, "BaseTableName");
				row ["AllowDBNull"]		= GetSchemaValue (schema, "AllowDBNull");
#endif
				// We don't always get the base column name.
				if (row [BASE_COLUMN_NAME_IDX] == DBNull.Value)
					row [BASE_COLUMN_NAME_IDX] = row [COLUMN_NAME_IDX];

				TdsColumnType ctype;
				int csize, dbType;				
				Type fieldType;
				bool isLong;
				string typeName;
#if NET_2_0
				ctype = (TdsColumnType) schema.ColumnType;
				csize = (int) schema.ColumnSize;
#else
				ctype = (TdsColumnType) schema ["ColumnType"];
				csize = (int) schema ["ColumnSize"];
#endif

				GetSchemaRowType (ctype, csize, out dbType, 
									out fieldType, out isLong, out typeName);
				
				dataTypeNames.Add (typeName);
				row [PROVIDER_TYPE_IDX] = dbType;
				row [DATA_TYPE_IDX] = fieldType;
				row [IS_LONG_IDX] = isLong;			
#if NET_2_0
				if ((bool)row [IS_HIDDEN_IDX] == false)
					visibleFieldCount += 1;
#endif

				schemaTable.Rows.Add (row);
			}
			return schemaTable;
		}

		private static object GetSchemaValue (TdsDataColumn schema, string key)
		{
			object val = schema [key];
			if (val != null)
				return val;
			else
				return DBNull.Value;
		}

#if NET_2_0
		static object GetSchemaValue (object value)
		{
			if (value == null)
				return DBNull.Value;

			return value;
		}
#endif		
		
		public
#if NET_2_0
		virtual
#endif
		SqlBinary GetSqlBinary (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlBinary))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlBinary) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlBoolean GetSqlBoolean (int i) 
		{
			object value = GetSqlValue (i);
			if (!(value is SqlBoolean))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlBoolean) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlByte GetSqlByte (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlByte))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlByte) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlDateTime GetSqlDateTime (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDateTime))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDateTime) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlDecimal GetSqlDecimal (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDecimal))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDecimal) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlDouble GetSqlDouble (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlDouble))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlDouble) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlGuid GetSqlGuid (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlGuid))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlGuid) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlInt16 GetSqlInt16 (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlInt16))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt16) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlInt32 GetSqlInt32 (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlInt32))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt32) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlInt64 GetSqlInt64 (int i)
		{
			object value = GetSqlValue (i);
			// TDS 7.0 returns bigint as decimal(19,0)
			if (value is SqlDecimal) {
				TdsDataColumn schema = command.Tds.Columns[i];
				byte precision, scale;

#if NET_2_0
				precision = (byte)schema.NumericPrecision;
				scale = (byte)schema.NumericScale;
#else
				precision = (byte)schema["NumericPrecision"];
				scale = (byte)schema["NumericScale"];
#endif
				if (precision == 19 && scale == 0)
					value = (SqlInt64) (SqlDecimal) value;
			}
			if (!(value is SqlInt64))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlInt64) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlMoney GetSqlMoney (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlMoney))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlMoney) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlSingle GetSqlSingle (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlSingle))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlSingle) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SqlString GetSqlString (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlString))
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			return (SqlString) value;
		}

#if NET_2_0
		public virtual SqlXml GetSqlXml (int i)
		{
			object value = GetSqlValue (i);
			if (!(value is SqlXml)) {
				if (value is DBNull) {
					throw new SqlNullValueException ();
				} else if (command.Tds.TdsVersion == TdsVersion.tds70 && value is SqlString) {
					// Workaround for TDS 7 clients
					// Xml column types are supported only from Sql Server 2005 / TDS 8, however
					// when a TDS 7 client requests for Xml column data, Sql Server 2005 returns
					// it as NTEXT
					MemoryStream stream = null;
					if (!((SqlString) value).IsNull)
						stream = new MemoryStream (Encoding.Unicode.GetBytes (value.ToString()));
					value = new SqlXml (stream);
				} else {
					throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
				}
			}
			return (SqlXml) value;
		}
#endif // NET_2_0

		public
#if NET_2_0
		virtual
#endif
		object GetSqlValue (int i)
		{
			int dbType, csize;
			TdsColumnType ctype;
			
			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();
#if NET_2_0
			ctype = (TdsColumnType) command.Tds.Columns[i].ColumnType;
			csize = (int) command.Tds.Columns[i].ColumnSize;
#else
			ctype = (TdsColumnType) command.Tds.Columns[i]["ColumnType"];
			csize = (int) command.Tds.Columns[i]["ColumnSize"];
#endif
			GetSchemaRowDbType (ctype, csize, out dbType);
			SqlDbType type = (SqlDbType) dbType;
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
#if NET_2_0
			case SqlDbType.Xml:
				if (value == DBNull.Value)
					return SqlByte.Null;
				return (SqlXml) value;
#endif
			}

			throw new InvalidOperationException ("The type of this column is unknown.");
		}

		public
#if NET_2_0
		virtual
#endif
		int GetSqlValues (object[] values)
		{
			int count = 0;
			int columnCount = command.Tds.Columns.Count;
			int arrayCount = values.Length;

			if (arrayCount > columnCount)
				count = columnCount;
			else
				count = arrayCount;

			for (int i = 0; i < count; i += 1) 
				values [i] = GetSqlValue (i);

			return count;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string)) {
				if (value is DBNull) throw new SqlNullValueException ();
				throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			}
			return (string) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object GetValue (int i)
		{
			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();

			try {
				if ((command.CommandBehavior & CommandBehavior.SequentialAccess) != 0) {
					return ((Tds)command.Tds).GetSequentialColumnValue (i);
				}
			} catch (TdsInternalException ex) {
				command.Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			}

			return command.Tds.ColumnValues [i];
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int GetValues (object[] values)
		{
			int len = values.Length;
			int bigDecimalIndex = command.Tds.ColumnValues.BigDecimalIndex;

			// If a four-byte decimal is stored, then we can't convert to
			// a native type.  Throw an OverflowException.
			if (bigDecimalIndex >= 0 && bigDecimalIndex < len)
				throw new OverflowException ();
			try {
				command.Tds.ColumnValues.CopyTo (0, values, 0,
								 len > command.Tds.ColumnValues.Count ? command.Tds.ColumnValues.Count : len);
			} catch (TdsInternalException ex) {
				command.Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			}
			return (len < FieldCount ? len : FieldCount);
		}

#if !NET_2_0
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
#endif

#if NET_2_0
		public override IEnumerator GetEnumerator ()
#else
		IEnumerator IEnumerable.GetEnumerator ()
#endif
		{
			return new DbEnumerator (this);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool IsDBNull (int i)
		{
			return GetValue (i) == DBNull.Value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool NextResult ()
		{
			ValidateState ();

			if ((command.CommandBehavior & CommandBehavior.SingleResult) != 0 && resultsRead > 0)
				return false;

			try {
				moreResults = command.Tds.NextResult ();
			} catch (TdsInternalException ex) {
				command.Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			}
			if (!moreResults)
				command.GetOutputParameters ();
			else {
				// new schema - don't do anything except reset schemaTable as command.Tds.Columns is already updated
				schemaTable = null;
				dataTypeNames = null;
			}

			rowsRead = 0;
			resultsRead += 1;
			return moreResults;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool Read ()
		{
			ValidateState ();

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
			try {
				bool result = command.Tds.NextRow ();
			
				rowsRead += 1;
				return result;
			} catch (TdsInternalException ex) {
				command.Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			}
		}
		
		void ValidateState ()
		{
			if (IsClosed)
				throw new InvalidOperationException ("Invalid attempt to read data when reader is closed");
		}
		
#if NET_2_0
		public override Type GetProviderSpecificFieldType (int i)
		{
			return (GetSqlValue (i).GetType());
		}

		public override object GetProviderSpecificValue (int i)
		{
			return (GetSqlValue (i));
		}

		public override int GetProviderSpecificValues (object [] values)
		{
			return (GetSqlValues (values));
		}

		public virtual SqlBytes GetSqlBytes (int i)
		{
			//object value = GetSqlValue (i);
			//if (!(value is SqlBinary))
			//	throw new InvalidCastException ("Type is " + value.GetType ().ToString ());
			Byte[] val = (byte[])GetValue(i);
			SqlBytes sb = new SqlBytes (val);
			return (sb);
		}
#endif // NET_2_0

		#endregion // Methods
	}
}
