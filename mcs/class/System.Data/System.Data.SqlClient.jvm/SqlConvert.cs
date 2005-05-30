//
// System.Data.SqlClient.SqlConvert
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;
using System.Data.Common;

using java.sql;

namespace System.Data.SqlClient
{
	internal sealed class SqlConvert : DbConvert
	{
		#region Methods

		internal static String JdbcTypeNameToDbTypeName(string jdbcTypeName)
		{
			return jdbcTypeName.Trim();
		}

		internal static SqlDbType JdbcTypeToSqlDbType(int jdbcType)
		{
			// FIXME : other java.sql.Type
			// Types.ARRAY
			if(Types.BIGINT == jdbcType) return SqlDbType.BigInt;
			if(Types.BINARY == jdbcType) return SqlDbType.Binary;
			if(Types.BIT == jdbcType) return SqlDbType.Bit;
			if(Types.BLOB == jdbcType) return SqlDbType.Binary;
			// Types.BOOLEAN
			if(Types.CHAR == jdbcType) return SqlDbType.Char;
			if(Types.CLOB == jdbcType) return SqlDbType.Binary;
			if(Types.DATE == jdbcType) return SqlDbType.DateTime;
			if(Types.DECIMAL == jdbcType) return SqlDbType.Decimal;
			// Types.DISTINCT
			if(Types.DOUBLE == jdbcType) return SqlDbType.Float;
			if(Types.FLOAT == jdbcType) return SqlDbType.Float;
			if(Types.INTEGER == jdbcType) return SqlDbType.Int;
			// Types.JAVA_OBJECT
			if(Types.LONGVARBINARY == jdbcType) return SqlDbType.Image;
			if(Types.LONGVARCHAR == jdbcType) return SqlDbType.Text;
			// Types.NULL
			if(Types.NUMERIC == jdbcType) return SqlDbType.Decimal;
			if(Types.REAL == jdbcType) return SqlDbType.Real;
			// Types.REF
			if(Types.SMALLINT == jdbcType) return SqlDbType.SmallInt;
			// Types.STRUCT
			if(Types.TIME == jdbcType) return SqlDbType.Timestamp;
			if(Types.TIMESTAMP == jdbcType) return SqlDbType.Timestamp;
			if(Types.TINYINT == jdbcType) return SqlDbType.TinyInt;
			if(Types.VARBINARY == jdbcType) return SqlDbType.VarBinary;
			if(Types.VARCHAR == jdbcType) return SqlDbType.VarChar;
			return SqlDbType.Variant;
		}

		internal static SqlDbType ValueTypeToSqlDbType(Type type)
		{
			if (type.Equals(typeof(byte[]))) return  SqlDbType.VarBinary;
			if (type.Equals(typeof(bool))) return SqlDbType.Bit;
//				if (type.Equals(typeof(string))) return OleDbType.BSTR;
//				if (type.Equals(typeof(string))) return OleDbType.Char;
//				if (type.Equals(typeof(decimal))) return OleDbType.Currency;
			if (type.Equals(typeof(DateTime))) return SqlDbType.DateTime;
//				if (type.Equals(typeof(DateTime))) return OleDbType.DBDate;
//				if (type.Equals(typeof(TimeSpan))) return OleDbType.DBTime;
//				if (type.Equals(typeof(DateTime))) return OleDbType.DBTimeStamp;
			if (type.Equals(typeof(decimal))) return SqlDbType.Decimal;
//				if (type.Equals(typeof(int))) return OleDbType.Error;
//				if (type.Equals(typeof(DateTime))) return OleDbType.Filetime;
			if (type.Equals(typeof(Guid))) return SqlDbType.UniqueIdentifier;
//				if (type.Equals(typeof(short))) return OleDbType.TinyInt;
			if (type.Equals(typeof(short))) return SqlDbType.SmallInt;
			if (type.Equals(typeof(int))) return SqlDbType.Int;
			if (type.Equals(typeof(long))) return SqlDbType.BigInt;
//				if (type.Equals(typeof(object))) return OleDbType.IDispatch;
//				if (type.Equals(typeof(object))) return OleDbType.IUnknown;
//				if (type.Equals(typeof(byte[]))) return OleDbType.LongVarBinary;
//				if (type.Equals(typeof(string))) return OleDbType.LongVarChar;
//				if (type.Equals(typeof(decimal))) return OleDbType.Numeric;
//				if (type.Equals(typeof(object))) return OleDbType.PropVariant;
			if (type.Equals(typeof(float))) return SqlDbType.Float;
			if (type.Equals(typeof(double))) return SqlDbType.Float;
			if (type.Equals(typeof(byte))) return SqlDbType.TinyInt;
//				if (type.Equals(typeof(int))) return OleDbType.UnsignedSmallInt;
//				if (type.Equals(typeof(long))) return OleDbType.UnsignedInt;
//				if (type.Equals(typeof(decimal))) return OleDbType.UnsignedBigInt;
//				if (type.Equals(typeof(byte[]))) return OleDbType.VarBinary;
			if (type.Equals(typeof(string))) return SqlDbType.VarChar;
			if (type.Equals(typeof(object))) return SqlDbType.Variant;
//				if (type.Equals(typeof(decimal))) return OleDbType.VarNumeric;
//				if (type.Equals(typeof(string))) return OleDbType.WChar;
//				if (type.Equals(typeof(string))) return OleDbType.VarWChar;
//				if (type.Equals(typeof(string))) return OleDbType.LongVarWChar;
			return SqlDbType.Variant;
		}

		internal static Type SqlDbTypeToValueType(SqlDbType sqlDbType)
		{
			switch (sqlDbType) {
				case SqlDbType.BigInt : return typeof(long);
				case SqlDbType.Binary : return typeof(byte[]);
				case SqlDbType.Bit : return typeof(bool);
				case SqlDbType.Char : return typeof(string);
				case SqlDbType.DateTime : return typeof(DateTime);
				case SqlDbType.Decimal : return typeof(decimal);
				case SqlDbType.Float : return typeof(double);
				case SqlDbType.Image : return typeof(byte[]);
				case SqlDbType.Int : return typeof(int);
				case SqlDbType.Money : return typeof(decimal);
				case SqlDbType.NChar : return typeof(string);
				case SqlDbType.NText : return typeof(string);
				case SqlDbType.NVarChar : return typeof(string);
				case SqlDbType.Real : return typeof(Single);
				case SqlDbType.UniqueIdentifier : return typeof(Guid);
				case SqlDbType.SmallDateTime : return typeof(DateTime);
				case SqlDbType.SmallInt : return typeof(Int16);
				case SqlDbType.SmallMoney : return typeof(decimal);
				case SqlDbType.Text : return typeof(string);
				case SqlDbType.Timestamp : return typeof(byte[]);
				case SqlDbType.TinyInt : return typeof(byte);
				case SqlDbType.VarBinary : return typeof(byte[]);
				case SqlDbType.VarChar : return typeof(string);
				case SqlDbType.Variant : return typeof(object);
				default : throw ExceptionHelper.InvalidSqlDbType((int)sqlDbType);
			}
		}

		internal static SqlDbType DbTypeToSqlDbType(DbType dbType)
		{
			switch (dbType) {
				case DbType.AnsiString : return SqlDbType.VarChar;
				case DbType.Binary : return SqlDbType.VarBinary;
				case DbType.Byte : return SqlDbType.TinyInt;
				case DbType.Boolean : return SqlDbType.Bit;
				case DbType.Currency : return SqlDbType.Money;
				case DbType.DateTime : return SqlDbType.DateTime;
				case DbType.Decimal : return SqlDbType.Decimal;
				case DbType.Double : return SqlDbType.Float;
				case DbType.Guid : return SqlDbType.UniqueIdentifier;
				case DbType.Int16 : return SqlDbType.SmallInt;
				case DbType.Int32 : return SqlDbType.Int;
				case DbType.Int64 : return SqlDbType.BigInt;
				case DbType.Object : return SqlDbType.Variant;
				case DbType.SByte : throw ExceptionHelper.UnknownDataType(dbType.ToString(),"SqlDbType");
				case DbType.Single : return SqlDbType.Real;
				case DbType.String : return SqlDbType.NVarChar;
				case DbType.UInt16 : throw ExceptionHelper.UnknownDataType(dbType.ToString(),"SqlDbType");
				case DbType.UInt32 : throw ExceptionHelper.UnknownDataType(dbType.ToString(),"SqlDbType");
				case DbType.UInt64 : throw ExceptionHelper.UnknownDataType(dbType.ToString(),"SqlDbType");
				case DbType.VarNumeric : throw ExceptionHelper.UnknownDataType(dbType.ToString(),"SqlDbType");
				case DbType.AnsiStringFixedLength : return SqlDbType.Char;
				case DbType.StringFixedLength : return SqlDbType.NChar;
				default : throw ExceptionHelper.InvalidDbType((int)dbType);
			}
		}

		internal static DbType SqlDbTypeToDbType(SqlDbType sqlDbType)
		{
			switch (sqlDbType) {
				case SqlDbType.BigInt : return DbType.Int64;
				case SqlDbType.Binary : return DbType.Binary;
				case SqlDbType.Bit : return DbType.Boolean;
				case SqlDbType.Char : return DbType.AnsiStringFixedLength;
				case SqlDbType.DateTime : return DbType.DateTime;
				case SqlDbType.Decimal : return DbType.Decimal;
				case SqlDbType.Float : return DbType.Double;
				case SqlDbType.Image : return DbType.Binary;
				case SqlDbType.Int : return DbType.Int32;
				case SqlDbType.Money : return DbType.Currency;
				case SqlDbType.NChar : return DbType.StringFixedLength;
				case SqlDbType.NText : return DbType.String;
				case SqlDbType.NVarChar : return DbType.String;
				case SqlDbType.Real : return DbType.Single;
				case SqlDbType.UniqueIdentifier : return DbType.Guid;
				case SqlDbType.SmallDateTime : return DbType.DateTime;
				case SqlDbType.SmallInt : return DbType.Int16;
				case SqlDbType.SmallMoney : return DbType.Currency;
				case SqlDbType.Text : return DbType.AnsiString;
				case SqlDbType.Timestamp : return DbType.Binary;
				case SqlDbType.TinyInt : return DbType.Byte;
				case SqlDbType.VarBinary : return DbType.Binary;
				case SqlDbType.VarChar : return DbType.AnsiString;
				case SqlDbType.Variant : return DbType.Object;
				default : throw ExceptionHelper.InvalidSqlDbType((int)sqlDbType);
			}
		}

		internal static int	SqlDbTypeToJdbcType(SqlDbType sqlDbType)
		{
			switch(sqlDbType) {
				case SqlDbType.BigInt : return Types.BIGINT;
				case SqlDbType.Binary : return Types.BINARY;
				case SqlDbType.Bit : return Types.BIT;
				case SqlDbType.Char : return Types.CHAR;
				case SqlDbType.DateTime : return Types.DATE;
				case SqlDbType.Decimal : return Types.DECIMAL;
				case SqlDbType.Float : return Types.FLOAT;
				case SqlDbType.Image : return Types.LONGVARBINARY;
				case SqlDbType.Int : return Types.INTEGER;
				case SqlDbType.Money : return Types.DECIMAL;
				case SqlDbType.NChar : return Types.CHAR;
				case SqlDbType.NText : return Types.LONGVARCHAR;
				case SqlDbType.NVarChar : return Types.VARCHAR;
				case SqlDbType.Real : return Types.REAL;
				case SqlDbType.UniqueIdentifier : return Types.CHAR;
				case SqlDbType.SmallDateTime : return Types.DATE;
				case SqlDbType.SmallInt : return Types.SMALLINT;
				case SqlDbType.SmallMoney : return Types.DECIMAL;
				case SqlDbType.Text : return Types.LONGVARCHAR;
				case SqlDbType.Timestamp : return Types.TIMESTAMP;
				case SqlDbType.TinyInt : return Types.TINYINT;
				case SqlDbType.VarBinary : return Types.VARBINARY;
				case SqlDbType.VarChar : return Types.VARCHAR;
				case SqlDbType.Variant : return Types.VARCHAR; // note : ms jdbc driver recognize this sqlserver as varchar
				default : throw ExceptionHelper.InvalidSqlDbType((int)sqlDbType);
			}
		}

		#endregion // Methods
	}
}
