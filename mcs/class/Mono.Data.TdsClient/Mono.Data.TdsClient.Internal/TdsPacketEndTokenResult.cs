//
// Mono.Data.TdsClient.Internal.TdsPacketEndTokenResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsPacketEndTokenResult : TdsPacketResult
	{

		#region Fields

		byte status;
		int rowCount;
		bool isRetStatSet;
		int retStat;
		TdsPacketSubType type;

		#endregion // Fields

		#region Constructors

		public TdsPacketEndTokenResult (TdsPacketSubType type, byte status, int rowCount)
			: base (type)
		{
			this.type = type;
			this.status = status;
			this.rowCount = rowCount;
			this.isRetStatSet = false;
		}

		#endregion // Constructors

		#region Properties

		public int RowCount {
			get { return rowCount; }
		}

		public bool MoreResults {
			get { return (status & 0x01) != 0; }
		}
		
		public bool Cancelled {
			get { return (status & 0x20) != 0; }
		}

		public bool ReturnStatusExists {
			get { return isRetStatSet; }
		}

		public int ReturnStatus {
			get { return retStat; }
			set { 
				isRetStatSet = true;
				retStat = value;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			return String.Format ("token type: {0}, row count: {1}, more results: {2}, was cancelled? {3}, return status: {4}", type, RowCount, MoreResults, Cancelled, (ReturnStatusExists ? ReturnStatus.ToString () : "No return status"));
		}	

		#endregion // Methods
	}
}
