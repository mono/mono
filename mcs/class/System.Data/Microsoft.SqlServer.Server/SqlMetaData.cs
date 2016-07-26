//
// Microsoft.SqlServer.Server.SqlMetaData
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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


using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Server {
	public sealed class SqlMetaData
	{
		#region Fields

		SqlCompareOptions compareOptions = SqlCompareOptions.None;
		string databaseName = null;
		long _localeId = 0L;
		long maxLength = 0L;
		string name;
		byte precision = 10;
		byte scale = 0;
		string owningSchema = null;
		string objectName = null;
		SqlDbType _sqlDbType = SqlDbType.NVarChar;
		DbType _dbType = DbType.String;
		Type type = typeof (string);

		#endregion // Fields

		#region Constructors

		public SqlMetaData (string name, SqlDbType dbType)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Bit:
				maxLength = 1;
				precision = 1;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Boolean;
				type = typeof (bool);
				break;
			case SqlDbType.BigInt:
				maxLength = 8;
				precision = 19;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int64;
				type = typeof (long);
				break;
			case SqlDbType.DateTime:
				maxLength = 8;
				precision = 23;
				scale = 3;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.DateTime;
				type = typeof (DateTime);
				break;
			case SqlDbType.Decimal:
				maxLength = 9;
				precision = 18;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Decimal;
				type = typeof (decimal);
				break;
			case SqlDbType.Float:
				maxLength = 8;
				precision = 53;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Double;
				type = typeof (float);
				break;
			case SqlDbType.Int:
				maxLength = 4;
				precision = 10;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int32;
				type = typeof (int);
				break;
			case SqlDbType.Money:
				maxLength = 8;
				precision = 19;
				scale = 4;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Currency;
				type = typeof (double);
				break;
			  /*
			case SqlDbType.Numeric:
				maxLength = ;
				precision = ;
				scale = ;
				localeId = 0;
				compareOptions = SqlCompareOptions.None;
				break;
			  */
			case SqlDbType.SmallDateTime:
				maxLength = 4;
				precision = 16;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.DateTime;
				type = typeof (DateTime);
				break;
			case SqlDbType.SmallInt:
				maxLength = 2;
				precision = 5;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int16;
				type = typeof (short);
				break;
			case SqlDbType.SmallMoney:
				maxLength = 4;
				precision = 10;
				scale = 4;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Currency;
				type = typeof (double);
				break;
			case SqlDbType.Timestamp:
				maxLength = 8;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.DateTime;
				type = typeof (DateTime);
				break;
			case SqlDbType.TinyInt:
				maxLength = 1;
				precision = 3;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int16;
				type = typeof (short);
				break;
			case SqlDbType.UniqueIdentifier:
				maxLength = 16;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Guid;
				type = typeof (Guid);
				break;
			case SqlDbType.Xml:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.Xml;
				type = typeof (string);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.name = name;
			this._sqlDbType = dbType;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, long maxLength, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, byte precision, byte scale, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, string database, string owningSchema, string objectName, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType, bool useServerDefault, bool isUniqueKey, SortOrder columnSortOrder, int sortOrdinal)
		{
			throw new NotImplementedException ();
		}

		public SqlMetaData (string name, SqlDbType dbType, long maxLength)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Binary:
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.Binary;
				type = typeof (byte []);
				break;
			case SqlDbType.Char:
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.AnsiStringFixedLength;
				type = typeof (string);
				break;
			case SqlDbType.Image:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Binary;
				type = typeof (byte []);
				break;
			case SqlDbType.NChar:
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (string);
				break;
			case SqlDbType.NText:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (string);
				break;
			case SqlDbType.NVarChar:
				maxLength = -1;
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (string);
				break;
			case SqlDbType.Text:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (char []);
				break;
			case SqlDbType.VarBinary:
				maxLength = -1;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.Binary;
				type = typeof (byte []);
				break;
			case SqlDbType.VarChar:
				maxLength = -1;
				_localeId = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (char []);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.maxLength = maxLength;
			this.name = name;
			this._sqlDbType = dbType;
		}

		[MonoTODO]
		public SqlMetaData (string name, SqlDbType dbType, Type userDefinedType)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Udt:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Guid;
				type = typeof (Guid);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.name = name;
			// FIXME:
			//this.sqlDbType = userDefinedType;
			throw new NotImplementedException ();
		}

		public SqlMetaData (string name, SqlDbType dbType, byte precision, byte scale)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Decimal:
				maxLength = 9;
				this.precision = precision;
				this.scale = scale;
				_localeId = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Decimal;
				type = typeof (decimal);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.name = name;
			this._sqlDbType = dbType;
		}

		public SqlMetaData (string name, SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Char:
				_dbType = DbType.AnsiStringFixedLength;
				type = typeof (char []);
				break;
			case SqlDbType.NChar:
				_dbType = DbType.StringFixedLength;
				type = typeof (char []);
				break;
			case SqlDbType.NText:
			case SqlDbType.NVarChar:
				_dbType = DbType.String;
				type = typeof (string);
				break;
			case SqlDbType.Text:
			case SqlDbType.VarChar:
				_dbType = DbType.AnsiString;
				type = typeof (char []);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.compareOptions = compareOptions;
			this._localeId = locale;
			this.maxLength = maxLength;
			this.name = name;
			this._sqlDbType = dbType;
		}

		public SqlMetaData (string name, SqlDbType dbType, string database, string owningSchema, string objectName)
		{
			if ((name == null || objectName == null) && database != null && owningSchema != null)
				throw new ArgumentNullException ("name can not be null");
			switch (dbType) {
			case SqlDbType.Xml:
				maxLength = -1;
				precision = 0;
				scale = 0;
				_localeId = 0;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (string);
				break;
			default:
				throw new ArgumentException ("SqlDbType not supported");
			}
			this.name = name;
			this._sqlDbType = dbType;
			databaseName = database;
			this.owningSchema = owningSchema;
			this.objectName = objectName;
		}

		public SqlMetaData (string name, SqlDbType dbType, long maxLength, byte precision,
				    byte scale, long locale, SqlCompareOptions compareOptions,
				    Type userDefinedType)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			this.compareOptions = compareOptions;
			this._localeId = locale;
			this.maxLength = maxLength;
			this.precision = precision;
			this.scale = scale;
			switch (dbType) {
			case SqlDbType.Bit:
				maxLength = 1;
				precision = 1;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Boolean;
				type = typeof (bool);
				break;
			case SqlDbType.BigInt:
				maxLength = 8;
				precision = 19;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int64;
				type = typeof (long);
				break;
			case SqlDbType.DateTime:
				maxLength = 8;
				precision = 23;
				scale = 3;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.DateTime;
				type = typeof (DateTime);
				break;
			case SqlDbType.Decimal:
				maxLength = 9;
				precision = 18;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Decimal;
				type = typeof (decimal);
				break;
			case SqlDbType.Float:
				maxLength = 8;
				precision = 53;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Decimal;
				type = typeof (float);
				break;
			case SqlDbType.Image:
				maxLength = -1;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Binary;
				type = typeof (byte []);
				break;
			case SqlDbType.Int:
				maxLength = 4;
				precision = 10;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int32;
				type = typeof (int);
				break;
			case SqlDbType.Money:
				maxLength = 8;
				precision = 19;
				scale = 4;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Currency;
				type = typeof (decimal);
				break;
			case SqlDbType.NText:
				maxLength = -1;
				precision = 0;
				scale = 0;
				locale = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.String;
				type = typeof (string);
				break;
			  /*
			case SqlDbType.Numeric:
				maxLength = ;
				precision = ;
				scale = ;
				localeId = 0;
				compareOptions = SqlCompareOptions.None;
				break;
			  */
			case SqlDbType.Real:
				maxLength = 4;
				precision = 24;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Single;
				type = typeof (Single);
				break;
			case SqlDbType.SmallDateTime:
				maxLength = 4;
				precision = 16;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.DateTime;
				type = typeof (DateTime);
				break;
			case SqlDbType.SmallInt:
				maxLength = 2;
				precision = 5;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int16;
				type = typeof (short);
				break;
			case SqlDbType.SmallMoney:
				maxLength = 4;
				precision = 10;
				scale = 4;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Currency;
				type = typeof (decimal);
				break;
			case SqlDbType.Text:
				maxLength = -1;
				precision = 0;
				scale = 0;
				locale = Thread.CurrentThread.CurrentCulture.LCID;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.AnsiString;
				type = typeof (char []);
				break;
			case SqlDbType.Timestamp:
				maxLength = 8;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Byte;
				type = typeof (byte []);
				break;
			case SqlDbType.TinyInt:
				maxLength = 1;
				precision = 3;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Int16;
				type = typeof (short);
				break;
			case SqlDbType.UniqueIdentifier:
				maxLength = 16;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Guid;
				type = typeof (Guid);
				break;
			case SqlDbType.Udt:
				maxLength = -1;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Object;
				type = typeof (object);
				break;
			case SqlDbType.Variant:
				maxLength = 8016;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.None;
				_dbType = DbType.Object;
				type = typeof (object);
				break;
			case SqlDbType.Xml:
				maxLength = -1;
				precision = 0;
				scale = 0;
				locale = 0;
				compareOptions = SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreWidth;
				_dbType = DbType.Xml;
				type = typeof (string);
				break;
			default:
			  /*
				if (typeof (DbType.Row) == typeof (userDefinedType)) {
					// FIXME:
					// maxLength = Number of columns;
					precision = 0;
					scale = 0;
					localeId = 0;
					compareOptions = SqlCompareOptions.None;
				} else
			  */
					throw new ArgumentException ("SqlDbType not supported");
			}
			this.name = name;
			this._sqlDbType = dbType;
		}

		#endregion // Constructors

		#region Properties

		public SqlCompareOptions CompareOptions {
			get { return compareOptions; }
		}

		public DbType DbType {
			get { return _dbType; }
		}

		public long LocaleId {
			get { return _localeId; }
		}

		public static long Max {
			get { return -1; }
		}

		public long MaxLength {
			get { return maxLength; }
		}

		public string Name {
			get { return name; }
		}

		public byte Precision { 
			get { return precision; }
		}

		public byte Scale { 
			get { return scale; }
		}

		public SqlDbType SqlDbType {
			get { return _sqlDbType; }
		}

		public string XmlSchemaCollectionDatabase {
			get { return databaseName; }
		}

		public string XmlSchemaCollectionName {
			get { return objectName; }
		}

		public string XmlSchemaCollectionOwningSchema {
			get { return owningSchema; }
		}

		[MonoTODO]
		public string TypeName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool IsUniqueKey {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SortOrder SortOrder {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int SortOrdinal {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool UseServerDefault {
			get { throw new NotImplementedException (); }
		}
		#endregion // Properties

		#region Methods

		public bool Adjust (bool value)
		{
			if (type != typeof (bool))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public byte Adjust (byte value)
		{
			if (type != typeof (byte))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public byte[] Adjust (byte[] value)
		{
			if (type != typeof (byte []))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public char Adjust (char value)
		{
			if (type != typeof (char))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public char[] Adjust (char[] value)
		{
			if (type != typeof (char []))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public DateTime Adjust (DateTime value)
		{
			if (type != typeof (DateTime))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public DateTimeOffset Adjust (DateTimeOffset value)
		{
			if (type != typeof (DateTimeOffset))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public TimeSpan Adjust (TimeSpan value)
		{
			if (type != typeof (TimeSpan))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public decimal Adjust (decimal value)
		{
			if (type != typeof (decimal))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public double Adjust (double value)
		{
			if (type != typeof (double))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public Guid Adjust (Guid value)
		{
			if (type != typeof (Guid))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public short Adjust (short value)
		{
			if (type != typeof (short))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public int Adjust (int value)
		{
			if (type != typeof (int))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public long Adjust (long value)
		{
			if (type != typeof (long))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public object Adjust (object value)
		{
			if (type != typeof (object))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public float Adjust (float value)
		{
			if (type != typeof (float))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlBinary Adjust (SqlBinary value)
		{
			if (type != typeof (byte []))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlBoolean Adjust (SqlBoolean value)
		{
			if (type != typeof (bool))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlByte Adjust (SqlByte value)
		{
			if (type != typeof (byte))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlBytes Adjust (SqlBytes value)
		{
			if (type != typeof (byte []))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlChars Adjust (SqlChars value)
		{
			if (type != typeof (char []))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlDateTime Adjust (SqlDateTime value)
		{
			if (type != typeof (DateTime))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlDecimal Adjust (SqlDecimal value)
		{
			if (type != typeof (decimal))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlDouble Adjust (SqlDouble value)
		{
			if (type != typeof (double))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlGuid Adjust (SqlGuid value)
		{
			if (type != typeof (Guid))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlInt16 Adjust (SqlInt16 value)
		{
			if (type != typeof (short))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlInt32 Adjust (SqlInt32 value)
		{
			if (type != typeof (int))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlInt64 Adjust (SqlInt64 value)
		{
			if (type != typeof (long))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlMoney Adjust (SqlMoney value)
		{
			if (type != typeof (decimal))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlSingle Adjust (SqlSingle value)
		{
			if (type != typeof (Single))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public SqlString Adjust (SqlString value)
		{
			if (type != typeof (string))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		[MonoTODO]
		public SqlXml Adjust (SqlXml value)
		{
			throw new NotImplementedException ();
		}

		public string Adjust (string value)
		{
			if (type != typeof (string))
				throw new ArgumentException ("Value does not match the SqlMetaData type");
			return value;
		}

		public static SqlMetaData InferFromValue (object value, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name can not be null");
			if (value == null)
				throw new ArgumentException ("value can not be null");
			SqlMetaData sqlMetaData = null;
			switch (value.GetType ().ToString ()) {
			case "System.Boolean":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Bit);
				break;
			case "System.Byte":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Binary);
				break;
			case "System.Byte[]":
				sqlMetaData = new SqlMetaData (name, SqlDbType.VarBinary);
				break;
			case "System.Char":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Char);
				break;
			case "System.Char[]":
				sqlMetaData = new SqlMetaData (name, SqlDbType.VarChar);
				break;
			case "System.DateTime":
				sqlMetaData = new SqlMetaData (name, SqlDbType.DateTime);
				break;
			case "System.Decimal":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Decimal);
				break;
			case "System.Double":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Float);
				break;
			case "System.Guid":
				sqlMetaData = new SqlMetaData (name, SqlDbType.UniqueIdentifier);
				break;
			case "System.Int16":
				sqlMetaData = new SqlMetaData (name, SqlDbType.SmallInt);
				break;
			case "System.Int32":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Int);
				break;
			case "System.Int64":
				sqlMetaData = new SqlMetaData (name, SqlDbType.BigInt);
				break;
			case "System.Single":
				sqlMetaData = new SqlMetaData (name, SqlDbType.Real);
				break;
			case "System.String":
				sqlMetaData = new SqlMetaData (name, SqlDbType.NVarChar);
				break;
			case "System.Object":
			default:
				sqlMetaData = new SqlMetaData (name, SqlDbType.Variant);
				break;
			}
			return sqlMetaData;
		}

		#endregion // Methods
	}
}

