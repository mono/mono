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
using System.Globalization;
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
		const int PROVIDER_SPECIFIC_TYPE_IDX = 23;
		const int DATA_TYPE_NAME_IDX = 24;
		const int XML_SCHEMA_COLLCTN_DB_IDX = 25;
		const int XML_SCHEMA_COLLCTN_OWN_SCHEMA_IDX = 26;
		const int XML_SCHEMA_COLLCTN_NAME_IDX = 27;
		const int UDT_ASMBLY_QUALIFIED_NAME_IDX = 28;
		const int NON_VER_PROVIDER_TYPE_IDX = 29;
		const int IS_COLUMN_SET = 30;
		
		#region Constructors

		internal SqlDataReader (SqlCommand command)
		{
			this.command = command;
			command.Tds.RecordsAffected = -1;
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
			get {
				ValidateState ();
				return command.Tds.Columns.Count;
			}
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
				ValidateState ();

				if (rowsRead > 0)
					return true;
				if (!haveRead)
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
#if NET_2_0
			Type typeType = typeof (Type);
#endif
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
#if NET_2_0
			schemaTable.Columns.Add ("DataType", typeType);
#else
			schemaTable.Columns.Add ("DataType", typeof (object));
#endif
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
#if NET_2_0
			schemaTable.Columns.Add ("ProviderSpecificDataType", typeType);
			schemaTable.Columns.Add ("DataTypeName", stringType);
			schemaTable.Columns.Add ("XmlSchemaCollectionDatabase", stringType);
			schemaTable.Columns.Add ("XmlSchemaCollectionOwningSchema", stringType);
			schemaTable.Columns.Add ("XmlSchemaCollectionName", stringType);
			schemaTable.Columns.Add ("UdtAssemblyQualifiedName", stringType);
			schemaTable.Columns.Add ("NonVersionedProviderType", intType);
			schemaTable.Columns.Add ("IsColumnSet", booleanType);
#endif
			
			return schemaTable;
		}
		
		private string GetSchemaRowTypeName (TdsColumnType ctype, int csize, short precision, short scale)
		{
			int dbType;
			bool isLong;
			Type fieldType;

			string typeName;
			GetSchemaRowType (ctype, csize, precision, scale,
				out dbType, out fieldType, out isLong,
				out typeName);
			return typeName;
		}

		private Type GetSchemaRowFieldType (TdsColumnType ctype, int csize, short precision, short scale)
		{
			int dbType;
			bool isLong;
			Type fieldType;
			string typeName;

			GetSchemaRowType (ctype, csize, precision, scale,
				out dbType, out fieldType, out isLong,
				out typeName);
			return fieldType;
		}

		SqlDbType GetSchemaRowDbType (int ordinal)
		{
			int csize;
			short precision, scale;
			TdsColumnType ctype;
			TdsDataColumn column;

			if (ordinal < 0 || ordinal >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();

			column = command.Tds.Columns [ordinal];
#if NET_2_0
			ctype = (TdsColumnType) column.ColumnType;
			csize = (int) column.ColumnSize;
			precision = (short) (column.NumericPrecision ?? 0);
			scale = (short) (column.NumericScale ?? 0);
#else
			ctype = (TdsColumnType) column ["ColumnType"];
			csize = (int) column ["ColumnSize"];
			precision = (short) ((byte) column ["NumericPrecision"]);
			scale = (short) ((byte) column ["NumericScale"]);
#endif
			return GetSchemaRowDbType (ctype, csize, precision, scale);
		}

		private SqlDbType GetSchemaRowDbType (TdsColumnType ctype, int csize, short precision, short scale)
		{
			Type fieldType;
			bool isLong;
			string typeName;
			int dbType;

			GetSchemaRowType (ctype, csize, precision, scale,
				out dbType, out fieldType, out isLong,
				out typeName);
			return (SqlDbType) dbType;
		}
		
		private void GetSchemaRowType (TdsColumnType ctype, int csize,
                                               short precision, short scale,
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
				case TdsColumnType.BigInt:
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
					isLong = false;
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
					isLong = false;
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
					switch (csize) {
					case 4:
						typeName = "smalldatetime";
						dbType = (int) SqlDbType.SmallDateTime;
						fieldType = typeof (DateTime);
						isLong = false;
						break;
					case 8:
						typeName = "datetime";
						dbType = (int) SqlDbType.DateTime;
						fieldType = typeof (DateTime);
						isLong = false;
						break;
					}
					break;
				case TdsColumnType.Money :
				case TdsColumnType.MoneyN :
				case TdsColumnType.Money4 :
					switch (csize) {
					case 4:
						typeName = "smallmoney";
						dbType = (int) SqlDbType.SmallMoney;
						fieldType = typeof (decimal);
						isLong = false;
						break;
					case 8:
						typeName = "money";
						dbType = (int) SqlDbType.Money;
						fieldType = typeof (decimal);
						isLong = false;
						break;
					}
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
					// TDS 7.0 returns bigint as decimal(19,0)
					if (precision == 19 && scale == 0) {
						typeName = "bigint";
						dbType = (int) SqlDbType.BigInt;
						fieldType = typeof (long);
					} else {
						typeName = "decimal";
						dbType = (int) SqlDbType.Decimal;
						fieldType = typeof (decimal);
					}
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
				ValidateState ();
				EnsureDataAvailable ();

				try {
					long len = ((Tds)command.Tds).GetSequentialColumnValue (i, dataIndex, buffer, bufferIndex, length);
					if (len == -1)
						throw CreateGetBytesOnInvalidColumnTypeException (i);
					if (len == -2)
#if NET_2_0
						throw new SqlNullValueException ();
#else
						return 0;
#endif
					return len;
				} catch (TdsInternalException ex) {
					command.Connection.Close ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				}
			}

			object value = GetValue (i);
			if (!(value is byte [])) {
				SqlDbType type = GetSchemaRowDbType (i);
				switch (type) {
				case SqlDbType.Image:
					if (value is DBNull)
						throw new SqlNullValueException ();
					break;
				case SqlDbType.Text:
#if NET_2_0
					string text = value as string;
					if (text != null)
						value = Encoding.Default.GetBytes (text);
					else
						value = null;
					break;
#else
					throw new InvalidCastException ();
#endif
				case SqlDbType.NText:
#if NET_2_0
					string ntext = value as string;
					if (ntext != null)
						value = Encoding.Unicode.GetBytes (ntext);
					else
						value = null;
					break;
#else
					throw new InvalidCastException ();
#endif
				default:
					throw CreateGetBytesOnInvalidColumnTypeException (i);
				}
			}

			if (buffer == null)
				return ((byte []) value).Length; // Return length of data

			// Copy data into buffer
			int availLen = (int) ( ( (byte []) value).Length - dataIndex);
			if (availLen < length)
				length = availLen;
			if (dataIndex < 0)
				return 0;
			
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
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			if ((command.CommandBehavior & CommandBehavior.SequentialAccess) != 0) {
				ValidateState ();
				EnsureDataAvailable ();

				if (i < 0 || i >= command.Tds.Columns.Count)
					throw new IndexOutOfRangeException ();

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
			TdsDataColumn column;
			TdsColumnType ctype;
			int csize;
			short precision;
			short scale;

			ValidateState ();

			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();

			column = command.Tds.Columns [i];
#if NET_2_0
			ctype = (TdsColumnType) column.ColumnType;
			csize = (int) column.ColumnSize;
			precision = (short) (column.NumericPrecision ?? 0);
			scale = (short) (column.NumericScale ?? 0);
#else
			ctype = (TdsColumnType) column ["ColumnType"];
			csize = (int) column ["ColumnSize"];
			precision = (short) ((byte) column ["NumericPrecision"]);
			scale = (short) ((byte) column ["NumericScale"]);
#endif
			return GetSchemaRowTypeName (ctype, csize, precision, scale);
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

		[MonoTODO]
		public virtual DateTimeOffset GetDateTimeOffset (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual TimeSpan GetTimeSpan (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual SqlChars GetSqlChars (int i)
		{
			throw new NotImplementedException ();
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
			TdsDataColumn column;
			TdsColumnType ctype;
			int csize;
			short precision;
			short scale;

			ValidateState ();

			if (i < 0 || i >= command.Tds.Columns.Count)
				throw new IndexOutOfRangeException ();

			column = command.Tds.Columns [i];
#if NET_2_0
			ctype = (TdsColumnType) column.ColumnType;
			csize = (int) column.ColumnSize;
			precision = (short) (column.NumericPrecision ?? 0);
			scale = (short) (column.NumericScale ?? 0);
#else
			ctype = (TdsColumnType) column ["ColumnType"];
			csize = (int) column ["ColumnSize"];
			precision = (short) ((byte) column ["NumericPrecision"]);
			scale = (short) ((byte) column ["NumericScale"]);
#endif
			return GetSchemaRowFieldType (ctype, csize, precision,
				scale);
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
			ValidateState ();

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
			ValidateState ();

			if (name == null)
				throw new ArgumentNullException ("fieldName");

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
				row [NUMERIC_PRECISION_IDX]	= GetSchemaValue (schema.NumericPrecision);
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
				row [PROVIDER_SPECIFIC_TYPE_IDX] = DBNull.Value;
				row [DATA_TYPE_NAME_IDX] = GetSchemaValue (schema.DataTypeName);
				row [XML_SCHEMA_COLLCTN_DB_IDX] = DBNull.Value;
				row [XML_SCHEMA_COLLCTN_OWN_SCHEMA_IDX] = DBNull.Value;
				row [XML_SCHEMA_COLLCTN_NAME_IDX] = DBNull.Value;
				row [UDT_ASMBLY_QUALIFIED_NAME_IDX] = DBNull.Value;
				row [NON_VER_PROVIDER_TYPE_IDX] = DBNull.Value;
				row [IS_COLUMN_SET] = DBNull.Value;
#else
				row [COLUMN_NAME_IDX]		= GetSchemaValue (schema, "ColumnName");
				row [COLUMN_ORDINAL_IDX]		= GetSchemaValue (schema, "ColumnOrdinal");
				row [IS_UNIQUE_IDX]		= GetSchemaValue (schema, "IsUnique");
				row [IS_AUTO_INCREMENT_IDX]		= GetSchemaValue (schema, "IsAutoIncrement");
				row [IS_ROW_VERSION_IDX]		= GetSchemaValue (schema, "IsRowVersion");
				row [IS_HIDDEN_IDX]		= GetSchemaValue (schema, "IsHidden");
				row [IS_IDENTITY_IDX]		= GetSchemaValue (schema, "IsIdentity");
				row [IS_KEY_IDX]			= GetSchemaValue (schema, "IsKey");
				row [IS_ALIASED_IDX]		= GetSchemaValue (schema, "IsAliased");
				row [IS_EXPRESSION_IDX]		= GetSchemaValue (schema, "IsExpression");
				row [IS_READ_ONLY_IDX]		= GetSchemaValue (schema, "IsReadOnly");
				row [BASE_SERVER_NAME_IDX]		= GetSchemaValue (schema, "BaseServerName");
				row [BASE_CATALOG_NAME_IDX]		= GetSchemaValue (schema, "BaseCatalogName");
				row [BASE_COLUMN_NAME_IDX]		= GetSchemaValue (schema, "BaseColumnName");
				row [BASE_SCHEMA_NAME_IDX]		= GetSchemaValue (schema, "BaseSchemaName");
				row [BASE_TABLE_NAME_IDX]		= GetSchemaValue (schema, "BaseTableName");
				row [ALLOW_DBNULL_IDX]		= GetSchemaValue (schema, "AllowDBNull");
#endif
				// We don't always get the base column name.
				if (row [BASE_COLUMN_NAME_IDX] == DBNull.Value)
					row [BASE_COLUMN_NAME_IDX] = row [COLUMN_NAME_IDX];

				TdsColumnType ctype;
				int csize, dbType;
				Type fieldType;
				bool isLong;
				string typeName;
				short precision;
				short scale;
#if NET_2_0
				ctype = (TdsColumnType) schema.ColumnType;
				csize = (int) schema.ColumnSize;
				precision = (short) GetSchemaValue (schema.NumericPrecision);
				scale = (short) GetSchemaValue (schema.NumericScale);
#else
				ctype = (TdsColumnType) schema ["ColumnType"];
				csize = (int) schema ["ColumnSize"];
				precision = (short) ((byte) GetSchemaValue (schema, "NumericPrecision"));
				scale = (short) ((byte) GetSchemaValue (schema, "NumericScale"));
#endif

				GetSchemaRowType (ctype, csize, precision, scale,
					out dbType, out fieldType, out isLong,
					out typeName);
				
				row [COLUMN_SIZE_IDX] = csize;
				row [NUMERIC_PRECISION_IDX] = precision;
				row [NUMERIC_SCALE_IDX] = scale;
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
				} else if (command.Tds.TdsVersion <= TdsVersion.tds80 && value is SqlString) {
					// Workaround for TDS 7/8/8.1 clients
					// Xml column types are supported only from Sql Server 2005 / TDS 9, however
					// when a TDS 7/8/8.1 client requests for Xml column data, Sql Server 2005 returns
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
			object value = GetValue (i);
			//Console.WriteLine ("Type of value: {0}", value.GetType ());
			
			SqlDbType type = GetSchemaRowDbType (i);
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
				if (value is Int64)
					return (SqlDecimal)((long) value);
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
			ValidateState ();
			EnsureDataAvailable ();

			if (values == null)
				throw new ArgumentNullException ("values");

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
			ValidateState ();
			EnsureDataAvailable ();

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
			ValidateState ();
			EnsureDataAvailable ();

			if (values == null)
				throw new ArgumentNullException ("values");

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

			if ((command.CommandBehavior & CommandBehavior.SingleResult) != 0 && resultsRead > 0) {
				moreResults = false;
				rowsRead = 0;
				haveRead = false;
				return false;
			}

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
			}

			rowsRead = 0;
			haveRead = false;
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

			if (!haveRead || readResultUsed)
				readResult = ReadRecord ();
			readResultUsed = true;
			return readResult;
		}

		internal bool ReadRecord ()
		{
			readResultUsed = false;

			if ((command.CommandBehavior & CommandBehavior.SingleRow) != 0 && haveRead)
				return false;
			if ((command.CommandBehavior & CommandBehavior.SchemaOnly) != 0)
				return false;
			if (!moreResults)
				return false;

			try {
				bool result = command.Tds.NextRow ();
				if (result)
					rowsRead++;
				haveRead = true;
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

		void EnsureDataAvailable ()
		{
			if (!readResult || !haveRead || !readResultUsed)
				throw new InvalidOperationException ("No data available.");
		}

		InvalidCastException CreateGetBytesOnInvalidColumnTypeException (int ordinal)
		{
			string message = string.Format (CultureInfo.InvariantCulture,
				"Invalid attempt to GetBytes on column '{0}'." +
				"The GetBytes function can only be used on " +
				"columns of type Text, NText, or Image.",
				GetName (ordinal));
			return new InvalidCastException (message);
		}

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

#if NET_4_5
		[MonoTODO]
		public override T GetFieldValue<T> (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlReader GetXmlReader (int i)
		{
			throw new NotImplementedException ();	
		}

#endif
		#endregion // Methods
	}
}
