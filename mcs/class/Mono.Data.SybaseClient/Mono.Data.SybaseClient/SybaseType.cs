//
// Mono.Data.SybaseClient.SybaseType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.SybaseClient {
	[Serializable]
	public enum SybaseType
	{
		BigInt = 0,
		Binary = 1,
		Bit = 2,
		Char = 3,
		DateTime = 4,
		Decimal = 5,
		Float = 6,
		Image = 7,
		Int = 8,
		Money = 9,
		NChar = 10,
		NText = 11,
		NVarChar = 12,
		Real = 13,
		UniqueIdentifier = 14,
		SmallDateTime = 15,
		SmallInt = 16,
		SmallMoney = 17,
		Text = 18,
		Timestamp = 19,
		TinyInt = 20,
		VarBinary = 21,
		VarChar = 22,
		Variant = 23
	}
}
