//
// Mono.Data.TdsClient.Internal.ITds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Data.Common;
using System.Collections;

namespace Mono.Data.TdsClient.Internal {
        internal interface ITds 
	{
		#region Properties

		string Database {
			get;
		}

		bool IsConnected {
			get;
		}

		string ServerVersion {
			get;
		}

		TdsPacketColumnInfoResult Schema {
			get;
		}

		TdsPacketRowResult ColumnValues {
			get;
		}

		TdsPacketErrorResultCollection Errors {
			get;
		}


		ArrayList OutputParameters {
			get;	
			set;
		}

		#endregion // Properties

		#region Methods

		void Cancel ();
		bool Connect (TdsConnectionParameters connectionParameters);
		void Disconnect ();
		int ExecuteNonQuery (string sql);
		void ExecuteQuery (string sql);
		bool NextResult ();
		bool NextRow ();

		#endregion
	}
}
