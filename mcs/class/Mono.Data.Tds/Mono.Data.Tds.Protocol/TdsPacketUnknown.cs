//
// Mono.Data.TdsClient.Internal.TdsPacketUnknown.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketUnknown : TdsPacketResult
	{
		#region Constructors

		public TdsPacketUnknown (TdsPacketSubType type)
			: base (type)
		{
		}

		#endregion // Constructors
	}
}
