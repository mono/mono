//
// Mono.Data.Tds.Protocol.TdsPacketColumnOrderResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketColumnOrderResult : TdsPacketResult
	{
		#region Constructors

		public TdsPacketColumnOrderResult ()
			: base (TdsPacketSubType.ColumnOrder)
		{
		}

		#endregion // Constructors
	}
}
