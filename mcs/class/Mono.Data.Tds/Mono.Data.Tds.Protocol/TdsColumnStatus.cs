//
// Mono.Data.Tds.Protocol.TdsColumnStatus.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public enum TdsColumnStatus {
		IsExpression = 0x04,
		IsKey = 0x08,
		Hidden = 0x10,
		Rename = 0x20
	}
}
