//
// Mono.Data.TdsClient.Internal.TdsEnvPacketSubType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal enum TdsEnvPacketSubType {
		Database = 0x1,
		CharSet = 0x3,
		BlockSize = 0x4
	}
}
