//
// Mono.Data.Tds.Protocol.TdsPacketSubType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public enum TdsPacketSubType {
		Capability = 0xe2,
		Dynamic = 0xe7,
		Dynamic2 = 0xa3,
		EnvironmentChange = 0xe3,
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
		ColumnName = 0xa0,
		ColumnInfo = 0xa1,
		ColumnDetail = 0xa5,
		AltName = 0xa7,
		AltFormat = 0xa8,
		TableName = 0xa4,
		ColumnOrder = 0xa9,
		Control = 0xae,
		Row = 0xd1,
		ColumnMetadata = 0x81,
		RowFormat = 0xee,
		ParamFormat = 0xec,
		Parameters = 0xd7
	}
}
