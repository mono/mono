//
// Mono.Data.TdsClient.Internal.TdsPacketMessageResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketMessageResult : TdsPacketResult
	{
		TdsMessage message;

		public TdsPacketMessageResult (TdsPacketSubType type, TdsMessage message)
			: base (type)
		{
			this.message = message;
		}

		public TdsMessage Message {
			get { return message; } 
		}
	}
}
