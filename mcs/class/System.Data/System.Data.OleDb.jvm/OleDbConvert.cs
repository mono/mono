//
// System.Data.OleDb.OleDbConvert
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
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

namespace System.Data.OleDb
{
	internal sealed class OleDbConvert : DbConvert
	{
		#region Fields

		private static Hashtable _typeNamesMap;

		#endregion // Fields

		#region Constructors

		static OleDbConvert()
		{
			_typeNamesMap = new Hashtable(30,CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			
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

		internal static String JdbcTypeNameToDbTypeName(string jdbcTypeName)
		{
			jdbcTypeName = jdbcTypeName.Trim();
			string dbTypeName = (string)_typeNamesMap[jdbcTypeName];

			return (dbTypeName != null) ? dbTypeName : jdbcTypeName;
		}

		internal static OleDbType JdbcTypeToOleDbType(int jdbcType)
		{
			switch ((JavaSqlTypes)jdbcType) {
				case JavaSqlTypes.ARRAY: return OleDbType.Binary;
				case JavaSqlTypes.BIGINT: return OleDbType.BigInt;
				case JavaSqlTypes.BINARY: return OleDbType.Binary;
				case JavaSqlTypes.BIT: return OleDbType.Boolean;
				case JavaSqlTypes.BLOB: return OleDbType.Binary;
				case JavaSqlTypes.BOOLEAN: return OleDbType.Boolean;
				case JavaSqlTypes.CHAR: return OleDbType.Char;
				case JavaSqlTypes.CLOB: return OleDbType.LongVarWChar;
				case JavaSqlTypes.DATALINK: return OleDbType.IUnknown;
				case JavaSqlTypes.DATE: return OleDbType.DBDate;
				case JavaSqlTypes.DECIMAL: return OleDbType.Decimal;
				case JavaSqlTypes.DISTINCT: return OleDbType.IUnknown; 
				case JavaSqlTypes.DOUBLE: return OleDbType.Double;
				case JavaSqlTypes.FLOAT: return OleDbType.Double;
				case JavaSqlTypes.INTEGER: return OleDbType.Integer;
				case JavaSqlTypes.JAVA_OBJECT: return OleDbType.IUnknown;
				case JavaSqlTypes.LONGVARBINARY: return OleDbType.LongVarBinary;
				case JavaSqlTypes.LONGVARCHAR: return OleDbType.LongVarWChar;
				case JavaSqlTypes.NULL: return OleDbType.Empty;
				case JavaSqlTypes.NUMERIC: return OleDbType.Numeric;
				default:
				case JavaSqlTypes.OTHER: return OleDbType.IUnknown;
				case JavaSqlTypes.REAL: return OleDbType.Single;
				case JavaSqlTypes.REF: return OleDbType.IUnknown;
				case JavaSqlTypes.SMALLINT: return OleDbType.SmallInt;
				case JavaSqlTypes.STRUCT: return OleDbType.IUnknown;
				case JavaSqlTypes.TIME: return OleDbType.DBTime;
				case JavaSqlTypes.TIMESTAMP: return OleDbType.DBTimeStamp;
				case JavaSqlTypes.TINYINT: return OleDbType.TinyInt;
				case JavaSqlTypes.VARBINARY: return OleDbType.VarBinary;
				case JavaSqlTypes.VARCHAR: return OleDbType.VarChar;
			}
		}

		internal static OleDbType ValueTypeToOleDbType(Type type)
		{
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Boolean: return OleDbType.Boolean;
				case TypeCode.Byte: return OleDbType.UnsignedTinyInt;
				case TypeCode.Char: return OleDbType.Char;
				case TypeCode.DateTime: return OleDbType.Date;
				case TypeCode.DBNull: return OleDbType.Empty;
				case TypeCode.Decimal: return OleDbType.Decimal;
				case TypeCode.Double: return OleDbType.Double;
				case TypeCode.Empty: return OleDbType.Empty;
				case TypeCode.Int16: return OleDbType.SmallInt;
				case TypeCode.Int32: return OleDbType.Integer;
				case TypeCode.Int64: return OleDbType.BigInt;
				default:
				case TypeCode.Object: {
					if (type.Equals(DbTypes.TypeOfByteArray)) return  OleDbType.Binary;
					if (type.Equals(DbTypes.TypeOfTimespan)) return OleDbType.DBTime;
					if (type.Equals(DbTypes.TypeOfGuid)) return OleDbType.Guid;

					if (type.IsEnum)
						return ValueTypeToOleDbType (Enum.GetUnderlyingType (type));

					return OleDbType.IUnknown;
				}
				case TypeCode.SByte: return OleDbType.TinyInt;
				case TypeCode.Single: return OleDbType.Single;
				case TypeCode.String: return OleDbType.VarWChar;
				case TypeCode.UInt16: return OleDbType.UnsignedSmallInt;
				case TypeCode.UInt32: return OleDbType.UnsignedInt;
				case TypeCode.UInt64: return OleDbType.UnsignedBigInt;
			}
		}

		internal static Type OleDbTypeToValueType(OleDbType oleDbType)
		{
			switch (oleDbType) {
				case OleDbType.BigInt : return DbTypes.TypeOfInt64;// typeof(long);
				case OleDbType.Binary : return DbTypes.TypeOfByteArray;
				case OleDbType.Boolean : return DbTypes.TypeOfBoolean;
				case OleDbType.BSTR : return DbTypes.TypeOfString;
				case OleDbType.Char : return DbTypes.TypeOfString;
				case OleDbType.Currency : return DbTypes.TypeOfDecimal;
				case OleDbType.Date : return DbTypes.TypeOfDateTime;
				case OleDbType.DBDate : return DbTypes.TypeOfDateTime;
				case OleDbType.DBTime : return DbTypes.TypeOfTimespan;
				case OleDbType.DBTimeStamp : return DbTypes.TypeOfDateTime;
				case OleDbType.Decimal : return DbTypes.TypeOfDecimal;
				case OleDbType.Double : return DbTypes.TypeOfDouble;
				case OleDbType.Empty : return null; //typeof(DBNull);
				case OleDbType.Error : return typeof(Exception);
				case OleDbType.Filetime : return DbTypes.TypeOfDateTime;
				case OleDbType.Guid : return DbTypes.TypeOfGuid;
				case OleDbType.IDispatch : return DbTypes.TypeOfObject;
				case OleDbType.Integer : return DbTypes.TypeOfInt32;
				case OleDbType.IUnknown : return DbTypes.TypeOfObject;
				case OleDbType.LongVarBinary : return DbTypes.TypeOfByteArray;
				case OleDbType.LongVarChar : return DbTypes.TypeOfString;
				case OleDbType.LongVarWChar : return DbTypes.TypeOfString;
				case OleDbType.Numeric : return DbTypes.TypeOfDecimal;
				case OleDbType.PropVariant : return DbTypes.TypeOfObject;
				case OleDbType.Single : return DbTypes.TypeOfFloat;
				case OleDbType.SmallInt : return DbTypes.TypeOfInt16;
				case OleDbType.TinyInt : return DbTypes.TypeOfSByte;
				case OleDbType.UnsignedBigInt : return DbTypes.TypeOfUInt64;
				case OleDbType.UnsignedInt : return DbTypes.TypeOfUInt32;
				case OleDbType.UnsignedSmallInt : return DbTypes.TypeOfUInt16;
				case OleDbType.UnsignedTinyInt : return DbTypes.TypeOfByte;
				case OleDbType.VarBinary : return DbTypes.TypeOfByteArray;
				case OleDbType.VarChar : return DbTypes.TypeOfString;
				case OleDbType.Variant : return DbTypes.TypeOfObject;
				case OleDbType.VarNumeric : return DbTypes.TypeOfDecimal;
				case OleDbType.VarWChar : return DbTypes.TypeOfString;
				case OleDbType.WChar : return DbTypes.TypeOfString;
				default : return DbTypes.TypeOfObject;
			}
		}

		internal static OleDbType DbTypeToOleDbType(DbType dbType)
		{
			switch (dbType) {
				case DbType.AnsiString : return OleDbType.VarChar;
				case DbType.Binary : return OleDbType.VarBinary;
				case DbType.Byte : return OleDbType.UnsignedTinyInt;
				case DbType.Boolean : return OleDbType.Boolean;
				case DbType.Currency : return OleDbType.Currency;
				case DbType.Date : return OleDbType.DBDate;
				case DbType.DateTime : return OleDbType.DBTimeStamp;
				case DbType.Decimal : return OleDbType.Decimal;
				case DbType.Double : return OleDbType.Double;
				case DbType.Guid : return OleDbType.Guid;
				case DbType.Int16 : return OleDbType.SmallInt;
				case DbType.Int32 : return OleDbType.Integer;
				case DbType.Int64 : return OleDbType.BigInt;
				case DbType.Object : return OleDbType.Variant;
				case DbType.SByte : return OleDbType.TinyInt;
				case DbType.Single : return OleDbType.Single;
				case DbType.String : return OleDbType.VarWChar;
				case DbType.Time : return OleDbType.DBTime;
				case DbType.UInt16 : return OleDbType.UnsignedSmallInt;
				case DbType.UInt32 : return OleDbType.UnsignedInt;
				case DbType.UInt64 : return OleDbType.UnsignedBigInt;
				case DbType.VarNumeric : return OleDbType.VarNumeric;
				case DbType.AnsiStringFixedLength : return OleDbType.Char;
				case DbType.StringFixedLength : return OleDbType.WChar;
				default : throw ExceptionHelper.InvalidDbType((int)dbType);
			}
		}

		internal static DbType OleDbTypeToDbType(OleDbType oleDbType)
		{
			switch (oleDbType) {
				case OleDbType.Empty : return DbType.Object;
				case OleDbType.SmallInt : return DbType.Int16;
				case OleDbType.Integer : return DbType.Int32;
				case OleDbType.Single : return DbType.Single;
				case OleDbType.Double : return DbType.Double;
				case OleDbType.Currency : return DbType.Currency;
				case OleDbType.Date : return DbType.DateTime;
				case OleDbType.BSTR : return DbType.String;
				case OleDbType.IDispatch : return DbType.Object;
				case OleDbType.Error : return DbType.Object;
				case OleDbType.Boolean : return DbType.Boolean;
				case OleDbType.Variant : return DbType.Object;
				case OleDbType.IUnknown : return DbType.Object;
				case OleDbType.Decimal : return DbType.Decimal;
				case OleDbType.TinyInt : return DbType.SByte;
				case OleDbType.UnsignedTinyInt : return DbType.Byte;
				case OleDbType.UnsignedSmallInt : return DbType.UInt16;
				case OleDbType.UnsignedInt : return DbType.UInt32;
				case OleDbType.BigInt : return DbType.Int64;
				case OleDbType.UnsignedBigInt : return DbType.UInt64;
				case OleDbType.Filetime : return DbType.DateTime;
				case OleDbType.Guid : return DbType.Guid;
				case OleDbType.Binary : return DbType.Binary;
				case OleDbType.Char : return DbType.AnsiStringFixedLength;
				case OleDbType.WChar : return DbType.StringFixedLength;
				case OleDbType.Numeric : return DbType.Decimal;
				case OleDbType.DBDate : return DbType.Date;
				case OleDbType.DBTime : return DbType.Time;
				case OleDbType.DBTimeStamp : return DbType.DateTime;
				case OleDbType.PropVariant : return DbType.Object;
				case OleDbType.VarNumeric : return DbType.VarNumeric;
				case OleDbType.VarChar : return DbType.AnsiString;
				case OleDbType.LongVarChar : return DbType.AnsiString;
				case OleDbType.VarWChar : return DbType.String;
				case OleDbType.LongVarWChar : return DbType.String;
				case OleDbType.VarBinary : return DbType.Binary;
				case OleDbType.LongVarBinary : return DbType.Binary;
				default : throw ExceptionHelper.InvalidOleDbType((int)oleDbType);
			}
		}

		internal static int	OleDbTypeToJdbcType(OleDbType oleDbType)
		{
			switch(oleDbType) {
				case OleDbType.BigInt : return Types.BIGINT;
				case OleDbType.Binary : return Types.BINARY;
				case OleDbType.Boolean : return Types.BIT;
				case OleDbType.BSTR : return Types.VARCHAR;
				case OleDbType.Char : return Types.CHAR;
				case OleDbType.Currency : return Types.DECIMAL;
				case OleDbType.Date : return Types.TIMESTAMP;
				case OleDbType.DBDate : return Types.DATE;
				case OleDbType.DBTime : return Types.TIME;
				case OleDbType.DBTimeStamp : return Types.TIMESTAMP;
				case OleDbType.Decimal : return Types.DECIMAL;
				case OleDbType.Double : return Types.DOUBLE;
				case OleDbType.Empty : return Types.NULL;
				case OleDbType.Error : return Types.OTHER;
				case OleDbType.Filetime : return Types.TIMESTAMP;
				case OleDbType.Guid : return Types.CHAR;
				case OleDbType.IDispatch : return Types.OTHER; //throw new ArgumentException("The " + oleDbType + " OleDbType value is not supported.");
				case OleDbType.Integer : return Types.INTEGER;
				case OleDbType.IUnknown :  return Types.OTHER; //throw new ArgumentException("The " + oleDbType + " OleDbType value is not supported.");
				case OleDbType.LongVarBinary : return Types.LONGVARBINARY;
				case OleDbType.LongVarChar : return Types.LONGVARCHAR;
				case OleDbType.LongVarWChar : return Types.LONGVARCHAR;
				case OleDbType.Numeric : return Types.NUMERIC;
				case OleDbType.PropVariant : return Types.OTHER;
				case OleDbType.Single : return Types.FLOAT;
				case OleDbType.SmallInt : return Types.SMALLINT;
				case OleDbType.TinyInt : return Types.TINYINT;
				case OleDbType.UnsignedBigInt : return Types.BIGINT;
				case OleDbType.UnsignedInt : return Types.INTEGER;
				case OleDbType.UnsignedSmallInt : return Types.SMALLINT;
				case OleDbType.UnsignedTinyInt : return Types.TINYINT;
				case OleDbType.VarBinary : return Types.VARBINARY;
				case OleDbType.VarChar : return Types.VARCHAR;
				case OleDbType.Variant : return Types.VARCHAR;
				case OleDbType.VarNumeric : return Types.DECIMAL;
				case OleDbType.VarWChar : return Types.VARCHAR;
				case OleDbType.WChar : return Types.VARCHAR;
				default : throw ExceptionHelper.InvalidOleDbType((int)oleDbType);
			}

			#endregion // Methods
		}
	}
}
