//
// Mono.Data.Tds.Protocol.TdsPacketResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketResult 
	{
		TdsPacketSubType packetType;

		public TdsPacketResult (TdsPacketSubType packetType)
		{
			PacketType = packetType;
		}

		public TdsPacketSubType PacketType {
			get { return packetType; }
			set { packetType = value; }
		}
	}
}
