//
// Mono.Data.TdsClient.Internal.TdsPacketErrorResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketErrorResult : TdsPacketMessageResult
	{
		#region Constructors

		public TdsPacketErrorResult (TdsPacketSubType type, TdsMessage message)
			: base (type, message)
		{
		}

		#endregion // Constructors
	}
}
