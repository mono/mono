//
// Mono.Data.TdsClient.Internal.TdsPacketColumnInfoResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnInfoResult : TdsPacketResult
	{
		DataColumnCollection columns;

		public TdsPacketColumnInfoResult (DataColumnCollection columns)
			: base (TdsPacketSubType.ColumnNameToken)
		{
			this.columns = columns;
		}

		public DataColumnCollection Columns {
			get { return columns; }
		}
	}
}
