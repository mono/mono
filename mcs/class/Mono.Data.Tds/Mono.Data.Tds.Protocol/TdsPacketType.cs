//
// Mono.Data.Tds.Protocol.TdsPacketType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.Tds.Protocol {
        public enum TdsPacketType
	{
		None = 0x0,
		Query = 0x1,
		Logon = 0x2,
		Proc = 0x3,
		Reply = 0x4,
		Cancel = 0x6,
		Logon70 = 0x10,
		Logoff = 0x71,
		Normal = 0x0f
	}
}
