//
// Mono.Data.Tds.Protocol.TdsPacketRetStatResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketRetStatResult : TdsPacketResult
	{
		int status;

		public TdsPacketRetStatResult (int status)
			: base (TdsPacketSubType.ReturnStatus)
		{
			this.status = status;
		}

		public int Status {
			get { return status; }
		}
	}
}
