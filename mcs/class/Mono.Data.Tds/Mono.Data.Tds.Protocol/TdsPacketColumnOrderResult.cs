//
// Mono.Data.TdsClient.Internal.TdsPacketColumnOrderResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnOrderResult : TdsPacketResult
	{
		#region Constructors

		public TdsPacketColumnOrderResult ()
			: base (TdsPacketSubType.Order)
		{
		}

		#endregion // Constructors
	}
}
