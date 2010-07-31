//
// System.Data.Oracle.OracleConvert
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System;
using System.Collections;
using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.OracleClient {
	#region oracle.sql.Types constants

	internal enum JavaSqlTypes {
		ARRAY = 2003 ,
		BIGINT = -5, 
		BINARY = -2 ,
		BIT = -7 ,
		BLOB = 2004, 
		BOOLEAN = 16, 
		CHAR = 1, 
		CLOB = 2005, 
		DATALINK = 70, 
		DATE = 91, 
		DECIMAL = 3, 
		DISTINCT = 2001, 
		DOUBLE = 8, 
		FLOAT = 6, 
		INTEGER = 4, 
		JAVA_OBJECT = 2000, 
		LONGVARBINARY = -4,
		LONGVARCHAR = -1, 
		NULL = 0, 
		NUMERIC = 2 ,
		OTHER = 1111 ,
		REAL = 7 ,
		REF = 2006 ,
		SMALLINT = 5,
		STRUCT = 2002, 
		TIME = 92, 
		TIMESTAMP = 93, 
		TINYINT = -6, 
		VARBINARY = -3, 
		VARCHAR = 12,

		//ORACLE types, see oracle.jdbc.OracleTypes
		BINARY_FLOAT  = 100,
		BINARY_DOUBLE =	101,
		ROWID =	-8,
		CURSOR = -10,
		TIMESTAMPNS = -100,
		TIMESTAMPTZ = -101,
		TIMESTAMPLTZ = -102,
		INTERVALYM 	= -103,
		INTERVALDS 	= -104,
	}

	#endregion
	sealed class OracleConvert : DbConvert {

		#region .Net types constants

		internal static readonly Type TypeOfBoolean = typeof(Boolean);
		internal static readonly Type TypeOfSByte = typeof(SByte);
		internal static readonly Type TypeOfChar = typeof(Char);
		internal static readonly Type TypeOfInt16 = typeof(Int16);
		internal static readonly Type TypeOfInt32 = typeof(Int32);
		internal static readonly Type TypeOfInt64 = typeof(Int64);
		internal static readonly Type TypeOfByte = typeof(Byte);
		internal static readonly Type TypeOfUInt16 = typeof(UInt16);
		internal static readonly Type TypeOfUInt32 = typeof(UInt32);
		internal static readonly Type TypeOfUInt64 = typeof(UInt64);
		internal static readonly Type TypeOfDouble = typeof(Double);
		internal static readonly Type TypeOfSingle = typeof(Single);
		internal static readonly Type TypeOfDecimal = typeof(Decimal);
		internal static readonly Type TypeOfString = typeof(String);
		internal static readonly Type TypeOfDateTime = typeof(DateTime);		
		internal static readonly Type TypeOfObject = typeof(object);
		internal static readonly Type TypeOfGuid = typeof(Guid);
		internal static readonly Type TypeOfType = typeof(Type);

		// additional types
		internal static readonly Type TypeOfByteArray = typeof(Byte[]);
		internal static readonly Type TypeOfCharArray = typeof(Char[]);
		internal static readonly Type TypeOfFloat = typeof (float);
		internal static readonly Type TypeOfTimespan = typeof (TimeSpan);
		static readonly Type TypeOfIDataReader = typeof(IDataReader);

		#endregion

		#region Methods

		internal static String JdbcTypeNameToDbTypeName(string jdbcTypeName) {
			return jdbcTypeName.Trim();;
		}

		internal static OracleType JdbcTypeToOracleType(int jdbcType) {
			switch ((JavaSqlTypes)jdbcType) {
				case JavaSqlTypes.ARRAY: return OracleType.Blob;
				case JavaSqlTypes.BIGINT: return OracleType.Number;
				case JavaSqlTypes.BINARY: return OracleType.Blob;
				case JavaSqlTypes.BIT: return OracleType.Byte;
				case JavaSqlTypes.BLOB: return OracleType.Blob;
				case JavaSqlTypes.BOOLEAN: return OracleType.Byte;
				case JavaSqlTypes.CHAR: return OracleType.Char;
				case JavaSqlTypes.CLOB: return OracleType.Clob;
//				case JavaSqlTypes.DATALINK: return OracleType.IUnknown;
				case JavaSqlTypes.DATE: return OracleType.DateTime;
				case JavaSqlTypes.DECIMAL: return OracleType.Number;
//				case JavaSqlTypes.DISTINCT: return OracleType.IUnknown; 
				case JavaSqlTypes.DOUBLE: return OracleType.Double;
				case JavaSqlTypes.FLOAT: return OracleType.Float;
				case JavaSqlTypes.INTEGER: return OracleType.Int32;
//				case JavaSqlTypes.JAVA_OBJECT: return OracleType.IUnknown;
				case JavaSqlTypes.LONGVARBINARY: return OracleType.LongRaw;
				case JavaSqlTypes.LONGVARCHAR: return OracleType.LongVarChar;
//				case JavaSqlTypes.NULL: return OracleType.Empty;
				case JavaSqlTypes.NUMERIC: return OracleType.Number;
//				case JavaSqlTypes.OTHER: return OracleType.IUnknown;
//				case JavaSqlTypes.REAL: return OracleType.Single;
//				case JavaSqlTypes.REF: return OracleType.IUnknown;
				case JavaSqlTypes.SMALLINT: return OracleType.Int16;
//				case JavaSqlTypes.STRUCT: return OracleType.IUnknown;
				case JavaSqlTypes.TIME: return OracleType.TimestampLocal;
				case JavaSqlTypes.TIMESTAMP: return OracleType.Timestamp;
				case JavaSqlTypes.TINYINT: return OracleType.Byte;
				case JavaSqlTypes.VARBINARY: return OracleType.LongVarChar;
				default:
				case JavaSqlTypes.VARCHAR: return OracleType.VarChar;

				case JavaSqlTypes.BINARY_FLOAT: return OracleType.Float;
				case JavaSqlTypes.BINARY_DOUBLE: return OracleType.Double;
				case JavaSqlTypes.ROWID: return OracleType.RowId;
				case JavaSqlTypes.CURSOR: return OracleType.Cursor;
				case JavaSqlTypes.TIMESTAMPNS: return OracleType.Timestamp;
				case JavaSqlTypes.TIMESTAMPTZ: return OracleType.TimestampWithTZ;
				case JavaSqlTypes.TIMESTAMPLTZ: return OracleType.TimestampLocal; 
				case JavaSqlTypes.INTERVALYM: return OracleType.IntervalYearToMonth;
				case JavaSqlTypes.INTERVALDS: return OracleType.IntervalDayToSecond;
			}
		}

		internal static OracleType ValueTypeToOracleType(Type type) {
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Boolean: return OracleType.Byte;
				case TypeCode.Byte: return OracleType.Byte;
				case TypeCode.Char: return OracleType.Char;
				case TypeCode.DateTime: return OracleType.DateTime;
//				case TypeCode.DBNull: return OracleType.Empty;
				case TypeCode.Decimal: return OracleType.Number;
				case TypeCode.Double: return OracleType.Double;
//				case TypeCode.Empty: return OracleType.Empty;
				case TypeCode.Int16: return OracleType.Int16;
				case TypeCode.Int32: return OracleType.Int32;
				case TypeCode.Int64: return OracleType.Number;
				default:
				case TypeCode.Object: {
					if (type.Equals(TypeOfByteArray)) return  OracleType.Blob;
					if (type.Equals(TypeOfTimespan)) return OracleType.Timestamp;
					if (type.IsSubclassOf(TypeOfIDataReader)) return OracleType.Cursor;
//					if (type.Equals(DbTypes.TypeOfGuid)) return OracleType.Guid;
//
					if (type.IsEnum)
						return ValueTypeToOracleType (Enum.GetUnderlyingType (type));
//
					return OracleType.VarChar;
				}
				case TypeCode.SByte: return OracleType.SByte;
				case TypeCode.Single: return OracleType.Float;
				case TypeCode.String: return OracleType.VarChar;
				case TypeCode.UInt16: return OracleType.UInt16;
				case TypeCode.UInt32: return OracleType.UInt32;
				case TypeCode.UInt64: return OracleType.Number;
			}
		}

		internal static Type OracleTypeToValueType(OracleType oleDbType) {
			switch (oleDbType) {
//				case OracleType.BigInt : return DbTypes.TypeOfInt64;// typeof(long);
//				case OracleType.Binary : return DbTypes.TypeOfByteArray;
//				case OracleType.Boolean : return DbTypes.TypeOfBoolean;
//				case OracleType.BSTR : return DbTypes.TypeOfString;
				case OracleType.BFile : return TypeOfByteArray;
				case OracleType.Blob : return TypeOfByteArray;
				case OracleType.Byte : return TypeOfByte;
				case OracleType.Char : return TypeOfString;
				case OracleType.Clob : return TypeOfCharArray;
				case OracleType.Cursor : return TypeOfIDataReader;
				case OracleType.DateTime : return TypeOfDateTime;
//				case OracleType.Currency : return TypeOfDecimal;
//				case OracleType.Date : return TypeOfDateTime;
//				case OracleType.DBDate : return TypeOfDateTime;
//				case OracleType.DBTime : return TypeOfTimespan;
//				case OracleType.DBTimeStamp : return TypeOfDateTime;
//				case OracleType.Decimal : return TypeOfDecimal;
				case OracleType.Double : return TypeOfDouble;
				case OracleType.Float : return TypeOfFloat;
				case OracleType.Int16 : return TypeOfInt16;
				case OracleType.Int32 : return TypeOfInt32;
				case OracleType.IntervalDayToSecond : return TypeOfTimespan;
				case OracleType.IntervalYearToMonth : return TypeOfInt32;
				case OracleType.LongRaw : return TypeOfByteArray;
//				case OracleType.Empty : return null; //typeof(DBNull);
//				case OracleType.Error : return typeof(Exception);
//				case OracleType.Filetime : return TypeOfDateTime;
//				case OracleType.Guid : return TypeOfGuid;
//				case OracleType.IDispatch : return TypeOfObject;
//				case OracleType.Integer : return TypeOfInt32;
//				case OracleType.IUnknown : return TypeOfObject;
//				case OracleType.LongVarBinary : return TypeOfByteArray;
				case OracleType.LongVarChar : return TypeOfString;
				case OracleType.NChar : return TypeOfString;
				case OracleType.NClob : return TypeOfString;
				case OracleType.Number : return TypeOfDecimal;
				case OracleType.NVarChar : return TypeOfString;
				case OracleType.Raw : return TypeOfByteArray;

				case OracleType.RowId : return TypeOfString;
				case OracleType.SByte : return TypeOfSByte;
				case OracleType.Timestamp : return TypeOfTimespan;
				case OracleType.TimestampLocal : return TypeOfTimespan;
				case OracleType.TimestampWithTZ : return TypeOfTimespan;
				case OracleType.UInt16 : return TypeOfUInt16;

				case OracleType.UInt32 : return TypeOfUInt32;
				case OracleType.VarChar : return TypeOfString;
//				case OracleType.LongVarWChar : return TypeOfString;
//				case OracleType.Numeric : return TypeOfDecimal;
//				case OracleType.PropVariant : return TypeOfObject;
//				case OracleType.Single : return TypeOfFloat;
//				case OracleType.SmallInt : return TypeOfInt16;
//				case OracleType.TinyInt : return TypeOfSByte;
//				case OracleType.UnsignedBigInt : return TypeOfUInt64;
//				case OracleType.UnsignedInt : return TypeOfUInt32;
//				case OracleType.UnsignedSmallInt : return TypeOfUInt16;
//				case OracleType.UnsignedTinyInt : return TypeOfByte;
//				case OracleType.VarBinary : return TypeOfByteArray;
//				case OracleType.VarChar : return TypeOfString;
//				case OracleType.Variant : return TypeOfObject;
//				case OracleType.VarNumeric : return TypeOfDecimal;
//				case OracleType.VarWChar : return TypeOfString;
//				case OracleType.WChar : return TypeOfString;
				default : return TypeOfObject;
			}
		}

		internal static OracleType DbTypeToOracleType(DbType dbType) {
			switch (dbType) {
				case DbType.AnsiString : return OracleType.VarChar;
				case DbType.Binary : return OracleType.Blob;
				case DbType.Byte : return OracleType.Byte;
				case DbType.Boolean : return OracleType.Byte;
				case DbType.Currency : return OracleType.Number;
				case DbType.Date : return OracleType.DateTime;
				case DbType.DateTime : return OracleType.DateTime;
				case DbType.Decimal : return OracleType.Number;
				case DbType.Double : return OracleType.Double;
				case DbType.Guid : return OracleType.Char;
				case DbType.Int16 : return OracleType.Int16;
				case DbType.Int32 : return OracleType.Int32;
				case DbType.Int64 : return OracleType.Number;
				case DbType.Object : return OracleType.Cursor;
				case DbType.SByte : return OracleType.SByte;
				case DbType.Single : return OracleType.Float;
				case DbType.String : return OracleType.VarChar;
				case DbType.Time : return OracleType.Timestamp;
				case DbType.UInt16 : return OracleType.UInt16;
				case DbType.UInt32 : return OracleType.UInt32;
				case DbType.UInt64 : return OracleType.Number;
				case DbType.VarNumeric : return OracleType.Number;
				case DbType.AnsiStringFixedLength : return OracleType.NChar;
				case DbType.StringFixedLength : return OracleType.Char;
				default : throw ExceptionHelper.InvalidDbType((int)dbType);
			}
		}

		internal static DbType OracleTypeToDbType(OracleType oleDbType) {
			switch (oleDbType) {
				case OracleType.BFile : return DbType.Binary;
				case OracleType.Blob : return DbType.Binary;
				case OracleType.Byte : return DbType.Byte;
				case OracleType.Char : return DbType.StringFixedLength;
				case OracleType.Clob : return DbType.String;
				case OracleType.Cursor : return DbType.Object;
				case OracleType.DateTime : return DbType.DateTime;
				case OracleType.Double : return DbType.Double;
				case OracleType.Float : return DbType.Single;
				case OracleType.Int16 : return DbType.Int16;
				case OracleType.Int32 : return DbType.Int32;
				case OracleType.IntervalDayToSecond : return DbType.Time;
				case OracleType.IntervalYearToMonth : return DbType.Int32;
				case OracleType.LongRaw : return DbType.Binary;
				case OracleType.LongVarChar : return DbType.String;
				case OracleType.NChar : return DbType.AnsiStringFixedLength;
				case OracleType.NClob : return DbType.AnsiString;
				case OracleType.Number : return DbType.VarNumeric;
				case OracleType.NVarChar : return DbType.AnsiString;
				case OracleType.Raw : return DbType.Binary;

				case OracleType.RowId : return DbType.AnsiStringFixedLength;
				case OracleType.SByte : return DbType.SByte;
				case OracleType.Timestamp : return DbType.Time;
				case OracleType.TimestampLocal : return DbType.Time;
				case OracleType.TimestampWithTZ : return DbType.Time;
				case OracleType.UInt16 : return DbType.UInt16;

				case OracleType.UInt32 : return DbType.UInt32;
				case OracleType.VarChar : return DbType.String;
//				case OracleType.Empty : return DbType.Object;
//				case OracleType.SmallInt : return DbType.Int16;
//				case OracleType.Integer : return DbType.Int32;
//				case OracleType.Single : return DbType.Single;
//				case OracleType.Double : return DbType.Double;
//				case OracleType.Currency : return DbType.Currency;
//				case OracleType.Date : return DbType.DateTime;
//				case OracleType.BSTR : return DbType.String;
//				case OracleType.IDispatch : return DbType.Object;
//				case OracleType.Error : return DbType.Object;
//				case OracleType.Boolean : return DbType.Boolean;
//				case OracleType.Variant : return DbType.Object;
//				case OracleType.IUnknown : return DbType.Object;
//				case OracleType.Decimal : return DbType.Decimal;
//				case OracleType.TinyInt : return DbType.SByte;
//				case OracleType.UnsignedTinyInt : return DbType.Byte;
//				case OracleType.UnsignedSmallInt : return DbType.UInt16;
//				case OracleType.UnsignedInt : return DbType.UInt32;
//				case OracleType.BigInt : return DbType.Int64;
//				case OracleType.UnsignedBigInt : return DbType.UInt64;
//				case OracleType.Filetime : return DbType.DateTime;
//				case OracleType.Guid : return DbType.Guid;
//				case OracleType.Binary : return DbType.Binary;
//				case OracleType.Char : return DbType.AnsiStringFixedLength;
//				case OracleType.WChar : return DbType.StringFixedLength;
//				case OracleType.Numeric : return DbType.Decimal;
//				case OracleType.DBDate : return DbType.Date;
//				case OracleType.DBTime : return DbType.Time;
//				case OracleType.DBTimeStamp : return DbType.DateTime;
//				case OracleType.PropVariant : return DbType.Object;
//				case OracleType.VarNumeric : return DbType.VarNumeric;
//				case OracleType.VarChar : return DbType.AnsiString;
//				case OracleType.LongVarChar : return DbType.AnsiString;
//				case OracleType.VarWChar : return DbType.String;
//				case OracleType.LongVarWChar : return DbType.String;
//				case OracleType.VarBinary : return DbType.Binary;
//				case OracleType.LongVarBinary : return DbType.Binary;
				default : throw ExceptionHelper.InvalidOleDbType((int)oleDbType);
			}
		}

		internal static int	OracleTypeToJdbcType(OracleType oleDbType) {
			switch(oleDbType) {
				case OracleType.BFile : return (int)JavaSqlTypes.BINARY;
				case OracleType.Blob : return (int)JavaSqlTypes.BINARY;
				case OracleType.Byte : return (int)JavaSqlTypes.TINYINT;
				case OracleType.Char : return (int)JavaSqlTypes.CHAR;
				case OracleType.Clob : return (int)JavaSqlTypes.CLOB;
				case OracleType.Cursor : return (int)JavaSqlTypes.CURSOR;
				case OracleType.DateTime : return (int)JavaSqlTypes.TIMESTAMP;
				case OracleType.Double : return (int)JavaSqlTypes.DOUBLE;
				case OracleType.Float : return (int)JavaSqlTypes.FLOAT;
				case OracleType.Int16 : return (int)JavaSqlTypes.SMALLINT;
				case OracleType.Int32 : return (int)JavaSqlTypes.INTEGER;
				case OracleType.IntervalDayToSecond : return (int)JavaSqlTypes.INTERVALDS;
				case OracleType.IntervalYearToMonth : return (int)JavaSqlTypes.INTERVALYM;
				case OracleType.LongRaw : return (int)JavaSqlTypes.LONGVARBINARY;
				case OracleType.LongVarChar : return (int)JavaSqlTypes.LONGVARCHAR;
				case OracleType.NChar : return (int)JavaSqlTypes.CHAR;
				case OracleType.NClob : return (int)JavaSqlTypes.CLOB;
				case OracleType.Number : return (int)JavaSqlTypes.NUMERIC;
				case OracleType.NVarChar : return (int)JavaSqlTypes.VARCHAR;
				case OracleType.Raw : return (int)JavaSqlTypes.BINARY;

				case OracleType.RowId : return (int)JavaSqlTypes.VARCHAR;
				case OracleType.SByte : return (int)JavaSqlTypes.TINYINT;
				case OracleType.Timestamp : return (int)JavaSqlTypes.TIMESTAMP;
				case OracleType.TimestampLocal : return (int)JavaSqlTypes.TIMESTAMP;
				case OracleType.TimestampWithTZ : return (int)JavaSqlTypes.TIMESTAMP;
				case OracleType.UInt16 : return (int)JavaSqlTypes.SMALLINT;

				case OracleType.UInt32 : return (int)JavaSqlTypes.INTEGER;
				case OracleType.VarChar : return (int)JavaSqlTypes.VARCHAR;
//				case OracleType.BigInt : return Types.BIGINT;
//				case OracleType.Binary : return Types.BINARY;
//				case OracleType.Boolean : return Types.BIT;
//				case OracleType.BSTR : return Types.VARCHAR;
//				case OracleType.Char : return Types.CHAR;
//				case OracleType.Currency : return Types.DECIMAL;
//				case OracleType.Date : return Types.TIMESTAMP;
//				case OracleType.DBDate : return Types.DATE;
//				case OracleType.DBTime : return Types.TIME;
//				case OracleType.DBTimeStamp : return Types.TIMESTAMP;
//				case OracleType.Decimal : return Types.DECIMAL;
//				case OracleType.Double : return Types.DOUBLE;
//				case OracleType.Empty : return Types.NULL;
//				case OracleType.Error : return Types.OTHER;
//				case OracleType.Filetime : return Types.TIMESTAMP;
//				case OracleType.Guid : return Types.CHAR;
//				case OracleType.IDispatch : return Types.OTHER; //throw new ArgumentException("The " + oleDbType + " OracleType value is not supported.");
//				case OracleType.Integer : return Types.INTEGER;
//				case OracleType.IUnknown :  return Types.OTHER; //throw new ArgumentException("The " + oleDbType + " OracleType value is not supported.");
//				case OracleType.LongVarBinary : return Types.LONGVARBINARY;
//				case OracleType.LongVarChar : return Types.LONGVARCHAR;
//				case OracleType.LongVarWChar : return Types.LONGVARCHAR;
//				case OracleType.Numeric : return Types.NUMERIC;
//				case OracleType.PropVariant : return Types.OTHER;
//				case OracleType.Single : return Types.FLOAT;
//				case OracleType.SmallInt : return Types.SMALLINT;
//				case OracleType.TinyInt : return Types.TINYINT;
//				case OracleType.UnsignedBigInt : return Types.BIGINT;
//				case OracleType.UnsignedInt : return Types.INTEGER;
//				case OracleType.UnsignedSmallInt : return Types.SMALLINT;
//				case OracleType.UnsignedTinyInt : return Types.TINYINT;
//				case OracleType.VarBinary : return Types.VARBINARY;
//				case OracleType.VarChar : return Types.VARCHAR;
//				case OracleType.Variant : return Types.VARCHAR;
//				case OracleType.VarNumeric : return Types.DECIMAL;
//				case OracleType.VarWChar : return Types.VARCHAR;
//				case OracleType.WChar : return Types.VARCHAR;
				default : throw ExceptionHelper.InvalidOleDbType((int)oleDbType);
			}

			#endregion // Methods
		}
	}
}
