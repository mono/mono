//
// Mono.Data.TdsClient.Internal.TdsPacketTableNameResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketTableNameResult : TdsPacketResult
	{
		public TdsPacketTableNameResult ()
			: base (TdsPacketSubType.TableName)
		{
		}
	}
}
