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
	[Serializable]
	public enum OracleType
	{
		BFile,
		Blob,
		Byte,
		Char,
		Clob,
		Cursor,
		DateTime,
		Double,
		Float,
		Int16,
		Int32,
		IntervalDayToSecond,
		IntervalYearToMonth,
		LongRaw,
		LongVarChar,
		NChar,
		NClob,
		Number,
		NVarChar,
		Raw,
		RowId,
		SByte,
		Timestamp,
		TimestampLocal,
		TimestampWithTZ,
		UInt16,
		UInt32,
		VarChar
	}
}
