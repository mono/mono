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
}
