//
// OciDataType.cs
//
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
//
// Authors:
//     Tim Coleman <tim@timcoleman.com>
//     Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2005
//
//

namespace System.Data.OracleClient.Oci {
	internal enum OciDataType : uint {
		VarChar2 = 0x01,
		Number = 0x02,
		Integer = 0x03,
		Float = 0x04,
		String = 0x05,
		VarNum = 0x06,
		Long = 0x08,
		VarChar = 0x09,
		RowId = 0x0b,
		Date = 0x0c,
		VarRaw = 0x0f,
		Raw = 0x17,
		LongRaw = 0x18,
		UnsignedInt = 0x44,
		LongVarChar = 0x5e,
		LongVarRaw = 0x5f,
		Char = 0x60,
		CharZ = 0x61,
		RowIdDescriptor = 0x68,
		NamedDataType = 0x6c,
		Ref = 0x6e,
		Clob = 0x70,
		Blob = 0x71,
		BFile = 0x72,
		RSet = 0x74, // REF CURSOR
		OciString = 0x9b,
		OciDate = 0x9c,
		TimeStamp = 0xbb,
		IntervalYearToMonth = 0xbd,
		IntervalDayToSecond = 0xbe
	}
}
