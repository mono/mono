using System;

namespace IBM.Data.DB2
{
	public enum DB2Type
	{
		Invalid = 0,
		SmallInt = 1,
		Integer = 2,
		BigInt = 3,
		Real = 4,
		Double = 5,
		Float = 6,
		Decimal = 7,
		Numeric = 8,
		Date = 9,
		Time = 10,
		Timestamp = 11,
		Char = 12,
		VarChar = 13,
		LongVarChar = 14,
		Binary = 15,
		VarBinary = 16,
		LongVarBinary = 17,
		Graphic = 18,
		VarGraphic = 19,
		LongVarGraphic = 20,
		Clob = 21,
		Blob = 22,
		DbClob = 23,
		Datalink = 24,
		RowId = 25,
		XmlReader = 26,
	}
	public class DB2TypeConverter
	{
		public static Type GetManagedType(int sql_type)
		{
			switch(sql_type)
			{
				case DB2Constants.SQL_INTEGER:
					return typeof(int);
				case DB2Constants.SQL_SMALLINT:
					return typeof(short);
				case DB2Constants.SQL_BIGINT:
					return typeof(long);
				case DB2Constants.SQL_DOUBLE:
					return typeof(double);
				case DB2Constants.SQL_DECIMAL:
					return typeof(decimal);
				case DB2Constants.SQL_DATETIME:
				case DB2Constants.SQL_TYPE_DATE:
				case DB2Constants.SQL_TYPE_TIMESTAMP:
					return typeof(DateTime);
				case DB2Constants.SQL_TYPE_TIME:
					return typeof(TimeSpan);
				case DB2Constants.SQL_CHAR:
				case DB2Constants.SQL_VARCHAR:
				case DB2Constants.SQL_TYPE_CLOB:
					return typeof(string);
				case DB2Constants.SQL_TYPE_BLOB:
				case DB2Constants.SQL_TYPE_BINARY:
				case DB2Constants.SQL_LONGVARBINARY:
				case DB2Constants.SQL_VARBINARY:
					return typeof(byte[]);
			}
			throw new NotImplementedException("Unknown SQL type " + sql_type);
		}
	}
}
