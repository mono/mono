//
// Mono.Data.Tds.Protocol.TdsColumnType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public enum TdsColumnType {
		Void = 0x1f,
		Image = 0x22,
		Text = 0x23,
		UniqueIdentifier = 0x24,
		VarBinary = 0x25,
		IntN = 0x26,
		VarChar = 0x27,
		Binary = 0x2d,
		Char = 0x2f,
		Int1 = 0x30,
		Bit = 0x32,
		Int2 = 0x34,
		Int4 = 0x38,
		DateTime4 = 0x3a,
		Real = 0x3b,
		Money = 0x3c,
		DateTime = 0x3d,
		Float8 = 0x3e,
		NText = 0x63,
		NVarChar = 0x67,
		BitN = 0x68,
		Decimal = 0x6a,
		Numeric = 0x6c,
		FloatN = 0x6d,
		MoneyN = 0x6e,
		DateTimeN = 0x6f,
		Money4 = 0x70,
		NChar = 0xef,
		BigVarBinary = 0xa5,
		BigVarChar = 0xa7,
		BigBinary = 0xad,
		BigChar = 0xaf,
		SmallMoney = 0x7a
	}
}
