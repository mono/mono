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
	sealed class OracleConvert : DbConvert {
		#region Fields

		private static Hashtable _typeNamesMap;

		#endregion // Fields

		#region Constructors

		static OracleConvert() {
			_typeNamesMap = new Hashtable(30,new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
			
			// SqlServer types mapping
			//			_typeNamesMap.Add("numeric(3, 0)1","DBTYPE_I1");
			_typeNamesMap.Add("smallint","DBTYPE_I2");
			_typeNamesMap.Add("Int","DBTYPE_I4");
			_typeNamesMap.Add("bigint","DBTYPE_I8");
			_typeNamesMap.Add("tinyint","DBTYPE_UI1");
			//			_typeNamesMap.Add("numeric(5,0)","DBTYPE_UI2");
			//			_typeNamesMap.Add("numeric(10,0)","DBTYPE_UI4");
			//			_typeNamesMap.Add("numeric(20,0)","DBTYPE_UI8");
			_typeNamesMap.Add("Float","DBTYPE_R8");
			_typeNamesMap.Add("Real","DBTYPE_R4");
			_typeNamesMap.Add("numeric","DBTYPE_NUMERIC");
			_typeNamesMap.Add("decimal","DBTYPE_NUMERIC");
			_typeNamesMap.Add("money","DBTYPE_CY");
			_typeNamesMap.Add("smallmoney","DBTYPE_CY");
			_typeNamesMap.Add("ntext","DBTYPE_WLONGVARCHAR");
			_typeNamesMap.Add("nchar","DBTYPE_WCHAR");
			_typeNamesMap.Add("nvarchar","DBTYPE_WVARCHAR");
			_typeNamesMap.Add("Bit","DBTYPE_BOOL");
			//			_typeNamesMap.Add("nvarchar(4000)","DBTYPE_VARIANT");
			_typeNamesMap.Add("sql_variant","DBTYPE_VARIANT");
			_typeNamesMap.Add("uniqueidentifier","DBTYPE_GUID");
			_typeNamesMap.Add("image","DBTYPE_LONGVARBINARY");
			_typeNamesMap.Add("timestamp","DBTYPE_BINARY");
			_typeNamesMap.Add("binary","DBTYPE_BINARY");
			_typeNamesMap.Add("varbinary","DBTYPE_VARBINARY");
			_typeNamesMap.Add("char","DBTYPE_CHAR");
			_typeNamesMap.Add("varchar","DBTYPE_VARCHAR");
			_typeNamesMap.Add("text","DBTYPE_LONGVARCHAR");
			//			_typeNamesMap.Add("nchar","DBTYPE_WSTR");
			//			_typeNamesMap.Add("nvarchar","DBTYPE_WSTR");
			//			_typeNamesMap.Add("ntext","DBTYPE_WSTR");
			//			_typeNamesMap.Add("datetime","DBTYPE_DATE");
			_typeNamesMap.Add("datetime","DBTYPE_DBTIMESTAMP");
			_typeNamesMap.Add("smalldatetime","DBTYPE_DBTIMESTAMP");
			_typeNamesMap.Add("Ignored","DBTYPE_BYREF");
		}

		#endregion //Constructors

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
			//			NOTSET = int.MinValue
		}

		#endregion

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
		internal static readonly Type TypeOfFloat = typeof (float);
		internal static readonly Type TypeOfTimespan = typeof (TimeSpan);

		#endregion

		#region Methods

		internal static String JdbcTypeNameToDbTypeName(string jdbcTypeName) {
			jdbcTypeName = jdbcTypeName.Trim();
			string dbTypeName = (string)_typeNamesMap[jdbcTypeName];

			return (dbTypeName != null) ? dbTypeName : jdbcTypeName;
		}

		//TBD
		internal static OracleType JdbcTypeToOracleType(int jdbcType) {
			switch ((JavaSqlTypes)jdbcType) {
				case JavaSqlTypes.ARRAY: return OracleType.Blob;
				case JavaSqlTypes.BIGINT: return OracleType.Number;
				case JavaSqlTypes.BINARY: return OracleType.Blob;
				//case JavaSqlTypes.BIT: return OracleType.Boolean;
				case JavaSqlTypes.BLOB: return OracleType.Blob;
				//case JavaSqlTypes.BOOLEAN: return OracleType.Boolean;
				case JavaSqlTypes.CHAR: return OracleType.Char;
				//case JavaSqlTypes.CLOB: return OracleType.Clob;
//				case JavaSqlTypes.DATALINK: return OracleType.IUnknown;
//				case JavaSqlTypes.DATE: return OracleType.DBDate;
//				case JavaSqlTypes.DECIMAL: return OracleType.Decimal;
//				case JavaSqlTypes.DISTINCT: return OracleType.IUnknown; 
				case JavaSqlTypes.DOUBLE: return OracleType.Double;
				case JavaSqlTypes.FLOAT: return OracleType.Double;
//				case JavaSqlTypes.INTEGER: return OracleType.Integer;
//				case JavaSqlTypes.JAVA_OBJECT: return OracleType.IUnknown;
//				case JavaSqlTypes.LONGVARBINARY: return OracleType.LongVarBinary;
//				case JavaSqlTypes.LONGVARCHAR: return OracleType.LongVarWChar;
//				case JavaSqlTypes.NULL: return OracleType.Empty;
//				case JavaSqlTypes.NUMERIC: return OracleType.Numeric;
				default:
//				case JavaSqlTypes.OTHER: return OracleType.IUnknown;
//				case JavaSqlTypes.REAL: return OracleType.Single;
//				case JavaSqlTypes.REF: return OracleType.IUnknown;
//				case JavaSqlTypes.SMALLINT: return OracleType.SmallInt;
//				case JavaSqlTypes.STRUCT: return OracleType.IUnknown;
//				case JavaSqlTypes.TIME: return OracleType.DBTime;
//				case JavaSqlTypes.TIMESTAMP: return OracleType.DBTimeStamp;
//				case JavaSqlTypes.TINYINT: return OracleType.TinyInt;
//				case JavaSqlTypes.VARBINARY: return OracleType.VarBinary;
				case JavaSqlTypes.VARCHAR: return OracleType.VarChar;
			}
		}

		internal static OracleType ValueTypeToOracleType(Type type) {
			switch (Type.GetTypeCode(type)) {
//				case TypeCode.Boolean: return OracleType.Boolean;
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
					return  OracleType.VarChar;
//					if (type.Equals(DbTypes.TypeOfByteArray)) return  OracleType.Binary;
//					if (type.Equals(DbTypes.TypeOfTimespan)) return OracleType.DBTime;
//					if (type.Equals(DbTypes.TypeOfGuid)) return OracleType.Guid;
//
//					if (type.IsEnum)
//						return ValueTypeToOracleType (Enum.GetUnderlyingType (type));
//
//					return OracleType.IUnknown;
				}
				case TypeCode.SByte: return OracleType.SByte;
				case TypeCode.Single: return OracleType.Float;
				case TypeCode.String: return OracleType.VarChar;
				case TypeCode.UInt16: return OracleType.UInt16;
				case TypeCode.UInt32: return OracleType.UInt32;
//				case TypeCode.UInt64: return OracleType.UnsignedBigInt;
			}
		}

		internal static Type OracleTypeToValueType(OracleType oleDbType) {
			switch (oleDbType) {
//				case OracleType.BigInt : return DbTypes.TypeOfInt64;// typeof(long);
//				case OracleType.Binary : return DbTypes.TypeOfByteArray;
//				case OracleType.Boolean : return DbTypes.TypeOfBoolean;
//				case OracleType.BSTR : return DbTypes.TypeOfString;
				case OracleType.Char : return TypeOfString;
//				case OracleType.Currency : return TypeOfDecimal;
//				case OracleType.Date : return TypeOfDateTime;
//				case OracleType.DBDate : return TypeOfDateTime;
//				case OracleType.DBTime : return TypeOfTimespan;
//				case OracleType.DBTimeStamp : return TypeOfDateTime;
//				case OracleType.Decimal : return TypeOfDecimal;
				case OracleType.Double : return TypeOfDouble;
//				case OracleType.Empty : return null; //typeof(DBNull);
//				case OracleType.Error : return typeof(Exception);
//				case OracleType.Filetime : return TypeOfDateTime;
//				case OracleType.Guid : return TypeOfGuid;
//				case OracleType.IDispatch : return TypeOfObject;
//				case OracleType.Integer : return TypeOfInt32;
//				case OracleType.IUnknown : return TypeOfObject;
//				case OracleType.LongVarBinary : return TypeOfByteArray;
				case OracleType.LongVarChar : return TypeOfString;
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
//				case DbType.Binary : return OracleType.VarBinary;
//				case DbType.Byte : return OracleType.UnsignedTinyInt;
//				case DbType.Boolean : return OracleType.Boolean;
//				case DbType.Currency : return OracleType.Currency;
//				case DbType.Date : return OracleType.DBDate;
//				case DbType.DateTime : return OracleType.DBTimeStamp;
//				case DbType.Decimal : return OracleType.Decimal;
				case DbType.Double : return OracleType.Double;
//				case DbType.Guid : return OracleType.Guid;
//				case DbType.Int16 : return OracleType.SmallInt;
//				case DbType.Int32 : return OracleType.Integer;
//				case DbType.Int64 : return OracleType.BigInt;
//				case DbType.Object : return OracleType.Variant;
//				case DbType.SByte : return OracleType.TinyInt;
//				case DbType.Single : return OracleType.Single;
//				case DbType.String : return OracleType.VarWChar;
//				case DbType.Time : return OracleType.DBTime;
//				case DbType.UInt16 : return OracleType.UnsignedSmallInt;
//				case DbType.UInt32 : return OracleType.UnsignedInt;
//				case DbType.UInt64 : return OracleType.UnsignedBigInt;
//				case DbType.VarNumeric : return OracleType.VarNumeric;
				case DbType.AnsiStringFixedLength : return OracleType.Char;
//				case DbType.StringFixedLength : return OracleType.WChar;
				default : throw ExceptionHelper.InvalidDbType((int)dbType);
			}
		}

		internal static DbType OracleTypeToDbType(OracleType oleDbType) {
			switch (oleDbType) {
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
//				case OracleType.BigInt : return Types.BIGINT;
//				case OracleType.Binary : return Types.BINARY;
//				case OracleType.Boolean : return Types.BIT;
//				case OracleType.BSTR : return Types.VARCHAR;
				case OracleType.Char : return Types.CHAR;
//				case OracleType.Currency : return Types.DECIMAL;
//				case OracleType.Date : return Types.TIMESTAMP;
//				case OracleType.DBDate : return Types.DATE;
//				case OracleType.DBTime : return Types.TIME;
//				case OracleType.DBTimeStamp : return Types.TIMESTAMP;
//				case OracleType.Decimal : return Types.DECIMAL;
				case OracleType.Double : return Types.DOUBLE;
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
