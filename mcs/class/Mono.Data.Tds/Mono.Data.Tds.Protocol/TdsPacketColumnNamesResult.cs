//
// Mono.Data.TdsClient.Internal.TdsPacketColumnNamesResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketColumnNamesResult : TdsPacketResult
	{
		DataColumnCollection columns;

		public TdsPacketColumnNamesResult (DataColumnCollection columns)
			: base (TdsPacketSubType.ColumnNameToken)
		{
			this.columns = columns;
		}

		public DataColumnCollection Columns {
			get { return columns; }
		}
	}
}
