//
// Mono.Data.TdsClient.Internal.TdsPacketSubType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal enum TdsPacketSubType {
		EnvChange = 0xe3,
		Error = 0xaa,
		Info = 0xab,
		EED = 0xe5,
		Param = 0xac,
		LoginAck = 0xad,
		ReturnStatus = 0x79,
		ProcId = 0x7c,
		Done = 0xfd,
		DoneProc = 0xfe,
		DoneInProc = 0xff,
		ColumnNameToken = 0xa0,
		ColumnInfoToken = 0xa1,
		ColumnDetail = 0xa5,
		Unknown0xA7 = 0xa7,
		Unknown0xA8 = 0xa8,
		TableName = 0xa4,
		Order = 0xa9,
		Control = 0xae,
		Row = 0xd1,
		ColumnMetadata = 0x81,
		RowFormat = 0xee
	}
}
