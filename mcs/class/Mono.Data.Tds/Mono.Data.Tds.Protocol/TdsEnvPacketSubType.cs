//
// Mono.Data.Tds.Protocol.TdsEnvPacketSubType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public enum TdsEnvPacketSubType {
		Database = 0x1,
		CharSet = 0x3,
		BlockSize = 0x4
	}
}
