//
// Mono.Data.Tds.Protocol.TdsPacketControlResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketControlResult : TdsPacketResult
	{
		#region Constructors

		public TdsPacketControlResult ()
			: base (TdsPacketSubType.Control)
		{
		}

		#endregion // Constructors
	}
}
