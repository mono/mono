//
// System.Data.DbType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Gets the data type of a field, a property, or a Parameter object of a .NET data provider.
	/// </summary>
	[Serializable]
	public enum DbType
	{
		AnsiString = 0,
		Binary = 1,
		Byte = 2,
		Boolean = 3,
		Currency = 4,
		Date = 5,
		DateTime = 6,
		Decimal = 7,
		Double = 8,
		Guid = 9,
		Int16 = 10,
		Int32 = 11,
		Int64 = 12,
		Object = 13,
		SByte = 14,
		Single = 15,
		String = 16,
		Time = 17,
		UInt16 = 18,
		UInt32 = 19,
		UInt64 = 20,
		VarNumeric = 21,
		AnsiStringFixedLength = 22,
		StringFixedLength = 23

	}
}