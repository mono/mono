//
// OracleType.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;

namespace System.Data.OracleClient {
#if !NET_2_0
	[Serializable]
#endif
	public enum OracleType
	{
		BFile = 0x01,
		Blob = 0x02,
		Byte = 0x17,
		Char = 0x03,
		Clob = 0x04,
		Cursor = 0x05, // REF CURSOR
		DateTime = 0x06,
		Double = 0x1e,
		Float = 0x1d,
		Int16 = 0x1b,
		Int32 = 0x1c,
		IntervalDayToSecond = 0x07,
		IntervalYearToMonth = 0x08,
		LongRaw = 0x09,
		LongVarChar = 0x0a,
		NChar = 0x0b,
		NClob = 0x0c,
		Number = 0x0d,
		NVarChar = 0x0e,
		Raw = 0x0f,
		RowId = 0x10,
		SByte = 0x1a,
		Timestamp = 0x12,
		TimestampLocal = 0x13,
		TimestampWithTZ = 0x14,
		UInt16 = 0x18,
		UInt32 = 0x19,
		VarChar = 0x16
	}
}
