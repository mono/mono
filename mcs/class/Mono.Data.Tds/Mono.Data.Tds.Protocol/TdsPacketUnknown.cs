//
// Mono.Data.Tds.Protocol.TdsPacketUnknown.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketUnknown : TdsPacketResult
	{
		#region Constructors

		public TdsPacketUnknown (TdsPacketSubType type)
			: base (type)
		{
		}

		#endregion // Constructors
	}
}
