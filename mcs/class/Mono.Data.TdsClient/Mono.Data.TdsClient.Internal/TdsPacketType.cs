//
// Mono.Data.TdsClient.Internal.TdsPacketTypeInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.TdsClient.Internal {
        internal enum TdsPacketTypeInternal 
	{
		Query = 0x1,
		Logon = 0x2,
		Proc = 0x3,
		Reply = 0x4,
		Cancel = 0x6,
		Logon70 = 0x10
	}
}
