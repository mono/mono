//
// Mono.Data.SybaseClient.SybaseDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002, 2008
// Copyright (C) Tim Coleman, 2002
//
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

using Mono.Data.SybaseTypes;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
#if NET_2_0
	public class SybaseDataReader : DbDataReader, IDataReader, IDisposable, IDataRecord
#else
	public sealed class SybaseDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
#endif // NET_2_0
	{
		#region Fields

		SybaseCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		int fieldCount;
		bool isClosed;
		bool isSelect;
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

		internal SybaseDataReader (SybaseCommand command)
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

		protected SybaseConnection Connection {
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
						dbType = (int) SybaseType.TinyInt;
						fieldType = typeof (byte);
						isLong = false;
						break;
					case 2:
						typeName = "smallint";
						dbType = (int) SybaseType.SmallInt;
						fieldType = typeof (short);
						isLong = false;
						break;
					case 4:
						typeName = "int";
						dbType = (int) SybaseType.Int;
						fieldType = typeof (int);
						isLong = false;
						break;
					case 8:
						typeName = "bigint";
						dbType = (int) SybaseType.BigInt;
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
						dbType = (int) SybaseType.Real;
						fieldType = typeof (float);
						isLong = false;
						break;
					case 8:
						typeName = "float";
						dbType = (int) SybaseType.Float;
						fieldType = typeof (double);
						isLong = false;
						break;
					}
					break;
				case TdsColumnType.Image :
					typeName = "image";
					dbType = (int) SybaseType.Image;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.Text :
					typeName = "text";
					dbType = (int) SybaseType.Text;
					fieldType = typeof (string);
					isLong = true;
					break;
				case TdsColumnType.UniqueIdentifier :
					typeName = "uniqueidentifier";
					dbType = (int) SybaseType.UniqueIdentifier;
					fieldType = typeof (Guid);
					isLong = false;
					break;
				case TdsColumnType.VarBinary :
				case TdsColumnType.BigVarBinary :
					typeName = "varbinary";
					dbType = (int) SybaseType.VarBinary;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.VarChar :
				case TdsColumnType.BigVarChar :
					typeName = "varchar";
					dbType = (int) SybaseType.VarChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Binary :
				case TdsColumnType.BigBinary :
					typeName = "binary";
					dbType = (int) SybaseType.Binary;
					fieldType = typeof (byte[]);
					isLong = true;
					break;
				case TdsColumnType.Char :
				case TdsColumnType.BigChar :
					typeName = "char";
					dbType = (int) SybaseType.Char;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Bit :
				case TdsColumnType.BitN :
					typeName = "bit";
					dbType = (int) SybaseType.Bit;
					fieldType = typeof (bool);
					isLong = false;
					break;
				case TdsColumnType.DateTime4 :
				case TdsColumnType.DateTime :
				case TdsColumnType.DateTimeN :
					typeName = "datetime";
					dbType = (int) SybaseType.DateTime;
					fieldType = typeof (DateTime);
					isLong = false;
					break;
				case TdsColumnType.Money :
				case TdsColumnType.MoneyN :
				case TdsColumnType.Money4 :
					typeName = "money";
					dbType = (int) SybaseType.Money;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				case TdsColumnType.NText :
					typeName = "ntext";
					dbType = (int) SybaseType.NText;
					fieldType = typeof (string);
					isLong = true;
					break;
				case TdsColumnType.NVarChar :
					typeName = "nvarchar";
					dbType = (int) SybaseType.NVarChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.Decimal :
				case TdsColumnType.Numeric :
					typeName = "decimal";
					dbType = (int) SybaseType.Decimal;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				case TdsColumnType.NChar :
					typeName = "nchar";
					dbType = (int) SybaseType.NChar;
					fieldType = typeof (string);
					isLong = false;
					break;
				case TdsColumnType.SmallMoney :
					typeName = "smallmoney";
					dbType = (int) SybaseType.SmallMoney;
					fieldType = typeof (decimal);
					isLong = false;
					break;
				default :
					typeName = "variant";
					dbType = (int) SybaseType.Variant;
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
					schemaTable.Dispose ();
					Close ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (byte) value;
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is byte [])) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((byte []) value).Length - dataIndex;
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (char) value;
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is char[])) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((char []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((char []) value).Length - dataIndex;
		}

		[MonoTODO ("Implement GetData")]
#if !NET_2_0
		public IDataReader GetData (int i)
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
			string datatypeName = null;
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
			Type fieldType = null;
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
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
		SybaseBinary GetSybaseBinary (int i)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseBoolean GetSybaseBoolean (int i) 
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseBoolean))
				throw new InvalidCastException ();
			return (SybaseBoolean) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseByte GetSybaseByte (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseByte))
				throw new InvalidCastException ();
			return (SybaseByte) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseDateTime GetSybaseDateTime (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDateTime))
				throw new InvalidCastException ();
			return (SybaseDateTime) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseDecimal GetSybaseDecimal (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDecimal))
				throw new InvalidCastException ();
			return (SybaseDecimal) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseDouble GetSybaseDouble (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDouble))
				throw new InvalidCastException ();
			return (SybaseDouble) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseGuid GetSybaseGuid (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseGuid))
				throw new InvalidCastException ();
			return (SybaseGuid) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseInt16 GetSybaseInt16 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt16))
				throw new InvalidCastException ();
			return (SybaseInt16) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseInt32 GetSybaseInt32 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt32))
				throw new InvalidCastException ();
			return (SybaseInt32) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseInt64 GetSybaseInt64 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt64))
				throw new InvalidCastException ();
			return (SybaseInt64) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseMoney GetSybaseMoney (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseMoney))
				throw new InvalidCastException ();
			return (SybaseMoney) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseSingle GetSybaseSingle (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseSingle))
				throw new InvalidCastException ();
			return (SybaseSingle) value;
		}

		public
#if NET_2_0
		virtual
#endif
		SybaseString GetSybaseString (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseString))
				throw new InvalidCastException ();
			return (SybaseString) value;
		}

		[MonoTODO ("Implement TdsBigDecimal conversion.  SybaseType.Real fails tests?")]
		public
#if NET_2_0
		virtual
#endif
		object GetSybaseValue (int i)
		{
			SybaseType type = (SybaseType) (schemaTable.Rows [i]["ProviderType"]);
			object value = GetValue (i);

			switch (type) {
			case SybaseType.BigInt:
				if (value == DBNull.Value)
					return SybaseInt64.Null;
				return (SybaseInt64) ((long) value);
			case SybaseType.Binary:
			case SybaseType.Image:
			case SybaseType.VarBinary:
			case SybaseType.Timestamp:
				if (value == DBNull.Value)
					return SybaseBinary.Null;
				return (SybaseBinary) ((byte[]) value);
			case SybaseType.Bit:
				if (value == DBNull.Value)
					return SybaseBoolean.Null;
				return (SybaseBoolean) ((bool) value);
			case SybaseType.Char:
			case SybaseType.NChar:
			case SybaseType.NText:
			case SybaseType.NVarChar:
			case SybaseType.Text:
			case SybaseType.VarChar:
				if (value == DBNull.Value)
					return SybaseString.Null;
				return (SybaseString) ((string) value);
			case SybaseType.DateTime:
			case SybaseType.SmallDateTime:
				if (value == DBNull.Value)
					return SybaseDateTime.Null;
				return (SybaseDateTime) ((DateTime) value);
			case SybaseType.Decimal:
				if (value == DBNull.Value)
					return SybaseDecimal.Null;
				if (value is TdsBigDecimal)
					return SybaseDecimal.FromTdsBigDecimal ((TdsBigDecimal) value);
				return (SybaseDecimal) ((decimal) value);
			case SybaseType.Float:
				if (value == DBNull.Value)
					return SybaseDouble.Null;
				return (SybaseDouble) ((double) value);
			case SybaseType.Int:
				if (value == DBNull.Value)
					return SybaseInt32.Null;
				return (SybaseInt32) ((int) value);
			case SybaseType.Money:
			case SybaseType.SmallMoney:
				if (value == DBNull.Value)
					return SybaseMoney.Null;
				return (SybaseMoney) ((decimal) value);
			case SybaseType.Real:
				if (value == DBNull.Value)
					return SybaseSingle.Null;
				return (SybaseSingle) ((float) value);
			case SybaseType.UniqueIdentifier:
				if (value == DBNull.Value)
					return SybaseGuid.Null;
				return (SybaseGuid) ((Guid) value);
			case SybaseType.SmallInt:
				if (value == DBNull.Value)
					return SybaseInt16.Null;
				return (SybaseInt16) ((short) value);
			case SybaseType.TinyInt:
				if (value == DBNull.Value)
					return SybaseByte.Null;
				return (SybaseByte) ((byte) value);
			}

			throw new InvalidOperationException ("The type of this column is unknown.");
		}

		public
#if NET_2_0
		virtual
#endif
		int GetSybaseValues (object[] values)
		{
			int count = 0;
			int columnCount = schemaTable.Rows.Count;
			int arrayCount = values.Length;

			if (arrayCount > columnCount)
				count = columnCount;
			else
				count = arrayCount;

			for (int i = 0; i < count; i += 1) 
				values [i] = GetSybaseValue (i);

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
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (string) value;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object GetValue (int i)
		{
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

			command.Tds.ColumnValues.CopyTo (0, values, 0, len);
			return (len > FieldCount ? len : FieldCount);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

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
				throw SybaseException.FromTdsInternalException ((TdsInternalException) ex);
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
				throw SybaseException.FromTdsInternalException ((TdsInternalException) ex);
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
			return (GetSybaseValue (i).GetType());
		}

		public override object GetProviderSpecificValue (int i)
		{
			return (GetSybaseValue (i));
		}

		public override int GetProviderSpecificValues (object [] values)
		{
			return (GetSybaseValues (values));
		}

/* TODO: create SybaseBytes
		public virtual SybaseBytes GetSybaseBytes (int i)
		{
			Byte[] val = (byte[])GetValue(i);
			SybaseBytes sb = new SybaseBytes (val);
			return (sb);
		}
*/

#endif // NET_2_0

		#endregion // Methods
	}
}
