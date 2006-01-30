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
	internal sealed class OracleConvert : DbConvert {
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

		#region Methods

		internal static String JdbcTypeNameToDbTypeName(string jdbcTypeName) {
			jdbcTypeName = jdbcTypeName.Trim();
			string dbTypeName = (string)_typeNamesMap[jdbcTypeName];

			return (dbTypeName != null) ? dbTypeName : jdbcTypeName;
		}

		//TBD
		internal static OracleType JdbcTypeToOracleType(int jdbcType) {
			switch ((DbTypes.JavaSqlTypes)jdbcType) {
				case DbTypes.JavaSqlTypes.ARRAY: return OracleType.Blob;
				case DbTypes.JavaSqlTypes.BIGINT: return OracleType.Number;
				case DbTypes.JavaSqlTypes.BINARY: return OracleType.Blob;
				//case DbTypes.JavaSqlTypes.BIT: return OracleType.Boolean;
				case DbTypes.JavaSqlTypes.BLOB: return OracleType.Blob;
				//case DbTypes.JavaSqlTypes.BOOLEAN: return OracleType.Boolean;
				case DbTypes.JavaSqlTypes.CHAR: return OracleType.Char;
				//case DbTypes.JavaSqlTypes.CLOB: return OracleType.Clob;
//				case DbTypes.JavaSqlTypes.DATALINK: return OracleType.IUnknown;
//				case DbTypes.JavaSqlTypes.DATE: return OracleType.DBDate;
//				case DbTypes.JavaSqlTypes.DECIMAL: return OracleType.Decimal;
//				case DbTypes.JavaSqlTypes.DISTINCT: return OracleType.IUnknown; 
				case DbTypes.JavaSqlTypes.DOUBLE: return OracleType.Double;
				case DbTypes.JavaSqlTypes.FLOAT: return OracleType.Double;
//				case DbTypes.JavaSqlTypes.INTEGER: return OracleType.Integer;
//				case DbTypes.JavaSqlTypes.JAVA_OBJECT: return OracleType.IUnknown;
//				case DbTypes.JavaSqlTypes.LONGVARBINARY: return OracleType.LongVarBinary;
//				case DbTypes.JavaSqlTypes.LONGVARCHAR: return OracleType.LongVarWChar;
//				case DbTypes.JavaSqlTypes.NULL: return OracleType.Empty;
//				case DbTypes.JavaSqlTypes.NUMERIC: return OracleType.Numeric;
				default:
//				case DbTypes.JavaSqlTypes.OTHER: return OracleType.IUnknown;
//				case DbTypes.JavaSqlTypes.REAL: return OracleType.Single;
//				case DbTypes.JavaSqlTypes.REF: return OracleType.IUnknown;
//				case DbTypes.JavaSqlTypes.SMALLINT: return OracleType.SmallInt;
//				case DbTypes.JavaSqlTypes.STRUCT: return OracleType.IUnknown;
//				case DbTypes.JavaSqlTypes.TIME: return OracleType.DBTime;
//				case DbTypes.JavaSqlTypes.TIMESTAMP: return OracleType.DBTimeStamp;
//				case DbTypes.JavaSqlTypes.TINYINT: return OracleType.TinyInt;
//				case DbTypes.JavaSqlTypes.VARBINARY: return OracleType.VarBinary;
				case DbTypes.JavaSqlTypes.VARCHAR: return OracleType.VarChar;
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
				case OracleType.Char : return DbTypes.TypeOfString;
//				case OracleType.Currency : return DbTypes.TypeOfDecimal;
//				case OracleType.Date : return DbTypes.TypeOfDateTime;
//				case OracleType.DBDate : return DbTypes.TypeOfDateTime;
//				case OracleType.DBTime : return DbTypes.TypeOfTimespan;
//				case OracleType.DBTimeStamp : return DbTypes.TypeOfDateTime;
//				case OracleType.Decimal : return DbTypes.TypeOfDecimal;
				case OracleType.Double : return DbTypes.TypeOfDouble;
//				case OracleType.Empty : return null; //typeof(DBNull);
//				case OracleType.Error : return typeof(Exception);
//				case OracleType.Filetime : return DbTypes.TypeOfDateTime;
//				case OracleType.Guid : return DbTypes.TypeOfGuid;
//				case OracleType.IDispatch : return DbTypes.TypeOfObject;
//				case OracleType.Integer : return DbTypes.TypeOfInt32;
//				case OracleType.IUnknown : return DbTypes.TypeOfObject;
//				case OracleType.LongVarBinary : return DbTypes.TypeOfByteArray;
				case OracleType.LongVarChar : return DbTypes.TypeOfString;
//				case OracleType.LongVarWChar : return DbTypes.TypeOfString;
//				case OracleType.Numeric : return DbTypes.TypeOfDecimal;
//				case OracleType.PropVariant : return DbTypes.TypeOfObject;
//				case OracleType.Single : return DbTypes.TypeOfFloat;
//				case OracleType.SmallInt : return DbTypes.TypeOfInt16;
//				case OracleType.TinyInt : return DbTypes.TypeOfSByte;
//				case OracleType.UnsignedBigInt : return DbTypes.TypeOfUInt64;
//				case OracleType.UnsignedInt : return DbTypes.TypeOfUInt32;
//				case OracleType.UnsignedSmallInt : return DbTypes.TypeOfUInt16;
//				case OracleType.UnsignedTinyInt : return DbTypes.TypeOfByte;
//				case OracleType.VarBinary : return DbTypes.TypeOfByteArray;
//				case OracleType.VarChar : return DbTypes.TypeOfString;
//				case OracleType.Variant : return DbTypes.TypeOfObject;
//				case OracleType.VarNumeric : return DbTypes.TypeOfDecimal;
//				case OracleType.VarWChar : return DbTypes.TypeOfString;
//				case OracleType.WChar : return DbTypes.TypeOfString;
				default : return DbTypes.TypeOfObject;
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
