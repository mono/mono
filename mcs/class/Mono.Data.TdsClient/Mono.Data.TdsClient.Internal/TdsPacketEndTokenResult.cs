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

		#endregion // Fields

		#region Constructors

		public TdsPacketEndTokenResult (TdsPacketSubType type, byte status, int rowCount)
			: base (type)
		{
			if (type == TdsPacketSubType.DoneInProc)
				throw new TdsException ("");
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
			throw new NotImplementedException ();
		}	

		#endregion // Methods
	}
}
